using APUW.Domain.Interfaces;
using APUW.Model;
using APUW.Model.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APUW.Domain.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext context) : ICurrentUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public UserDto GetCurrentUser()
        {
            var httpContext = _httpContextAccessor.HttpContext ?? throw new Exception("Missing HttpContext");

            var usernameClaim = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)
                ?? throw new Exception("Missing username claim");
            var idClaim = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
                ?? throw new Exception("Missing id claim");
            var roleClaims = httpContext.User.Claims.Where(claim => claim.Type == ClaimTypes.Role);

            return new UserDto()
            {
                Id = int.Parse(idClaim.Value),
                Username = usernameClaim.Value,
                Roles = [.. roleClaims.Select(x => x.Value)]
            };
        }

        public async Task<bool> HasRole(params string[] requiredRoles)
        {
            var user = GetCurrentUser();
            var roles = await _context.UserRoles.Where(x => x.UserId == user.Id).Select(x => x.Role.Name).ToListAsync();

            return roles.Any(x => requiredRoles.Contains(x));
        }
    }
}
