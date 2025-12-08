using APUW.Domain.Core.Results;
using APUW.Domain.Core.Transactions;
using APUW.Domain.Interfaces;
using APUW.Model;
using APUW.Model.DTOs.Users;
using APUW.Model.DTOs.Users.Requests;
using APUW.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace APUW.Domain.Services
{
    public class UsersService(AppDbContext context) : IUsersService
    {
        private readonly AppDbContext _context = context;

        public async Task<Result> ChangeUserRole(int userId, ChangeUserRoleRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return Result.Failure(ResultStatus.NotFound, "User not found");

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == request.RoleName);
            if (role == null) return Result.Failure(ResultStatus.NotFound, "Role not found");

            return await _context.InTransaction(async () =>
            {
                var userRolesToRemove = await _context.UserRoles
                    .Where(x => x.UserId == user.Id)
                    .ToListAsync();
                _context.UserRoles.RemoveRange(userRolesToRemove);

                await _context.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });

                return Result.Success(code: ResultStatus.NoContent);
            });
        }

        public async Task<Result<List<UserDto>>> GetUserList()
        {
            var userList = await _context.Users.Select(x => new UserDto
            {
                Id = x.Id,
                Username = x.Username,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).ToList()
            }).ToListAsync();

            return Result.Success(userList);
        }

        public async Task<Result<List<string>>> GetUserRoles(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
            {
                return Result.Failure(ResultStatus.NotFound);
            }

            var roles = await _context.UserRoles.Where(x => x.UserId == user.Id).Select(x => x.Role.Name).ToListAsync();
            return Result.Success(roles);
        }
    }
}
