using APUW.API.Core;
using APUW.Domain.Core.Results;
using APUW.Domain.Interfaces;
using APUW.Model.DTOs.Auth;
using APUW.Model.DTOs.Auth.Requests;
using APUW.Model.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APUW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, "User successfully authenticated, returns JWT token.", typeof(Result<LoginResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication failed due to invalid credentials.", typeof(Result))]
        [Produces("application/json")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            return ApiResult.GetIActionResult(await authService.Login(request));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status201Created, "User successfully registered.", typeof(Result<UserDto>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Registration failed due to duplicate username.", typeof(Result))]
        [Produces("application/json")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            return ApiResult.GetIActionResult(await authService.Register(request));
        }
    }
}
