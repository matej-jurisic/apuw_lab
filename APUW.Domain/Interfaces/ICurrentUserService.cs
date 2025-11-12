using APUW.Model.DTOs.Users;

namespace APUW.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        UserDto GetCurrentUser();
        Task<bool> HasRole(params string[] requiredRoles);
    }
}
