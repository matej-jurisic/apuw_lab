using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Users;
using APUW.Model.DTOs.Users.Requests;

namespace APUW.Domain.Interfaces
{
    public interface IUsersService
    {
        Task<Result> ChangeUserRole(int userId, ChangeUserRoleRequestDto request);
        Task<Result<List<UserDto>>> GetUserList();
        Task<Result<List<string>>> GetUserRoles(string username);
    }
}
