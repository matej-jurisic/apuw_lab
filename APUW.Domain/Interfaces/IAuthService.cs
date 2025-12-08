using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;
using APUW.Model.DTOs.Users;

namespace APUW.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result<UserDto>> Register(RegisterRequestDto request);
        Task<Result<LoginResultDto>> Login(LoginRequestDto request);
    }
}
