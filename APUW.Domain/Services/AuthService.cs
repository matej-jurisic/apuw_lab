using APUW.Domain.Core.Results;
using APUW.Domain.Core.Transactions;
using APUW.Domain.Interfaces;
using APUW.Model;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;
using APUW.Model.DTOs.Users;
using APUW.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APUW.Domain.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration, IUsersService usersService) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly IUsersService _usersService = usersService;

        public async Task<Result<LoginResultDto>> Login(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
            if (user == default || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result.Failure(ResultStatus.Unauthorized);
            }

            var rolesResult = await _usersService.GetUserRoles(request.Username);
            var roles = rolesResult.Data;

            var jwtKey = _configuration.GetValue<string>("Authentication:JWT:Key") ?? throw new Exception("Missing JWT configuration");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, request.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Result.Success(new LoginResultDto()
            {
                Id = user.Id,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        public async Task<Result<UserDto>> Register(RegisterRequestDto request)
        {
            var existsUser = await _context.Users.AnyAsync(x => x.Username == request.Username);
            if (existsUser)
            {
                return Result.Failure(ResultStatus.Conflict, "Username is taken");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "User");

            var userResult = await _context.InTransaction(async () =>
            {
                var user = (await _context.Users.AddAsync(new User()
                {
                    Username = request.Username,
                    PasswordHash = hash
                })).Entity;

                await _context.SaveChangesAsync();

                if (role != null)
                {
                    var userRole = new UserRole
                    {
                        RoleId = role.Id,
                        UserId = user.Id,
                    };

                    await _context.UserRoles.AddAsync(userRole);
                }

                return Result.Success();
            });

            if (userResult.IsFailure) return userResult;

            var userDto = await _context.Users
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
                })
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (userDto == null) return Result.Failure(ResultStatus.Error);

            return Result.Success(userDto, code: ResultStatus.Created);
        }
    }
}
