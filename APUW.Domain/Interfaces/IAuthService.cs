using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;

namespace APUW.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result> Register(RegisterRequestDto request);
        Task<Result<LoginResultDto>> Login(LoginRequestDto request);
    }
}
