using APUW.API.Core;
using APUW.API.Middleware.Authorization;
using APUW.Domain.Core.Results;
using APUW.Domain.Interfaces;
using APUW.Model.DTOs.Users;
using APUW.Model.DTOs.Users.Requests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APUW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController(IUsersService usersService) : ControllerBase
    {
        /// <summary>
        /// Changes the role of a specific user.
        /// </summary>
        /// <remarks>
        /// Only accessible by users with the "Admin" role.  
        /// Deletes all existing roles for the user and assigns the new role provided in the request.
        /// </remarks>
        [HttpPut("{id}/roles")]
        [CheckRoles("Admin")]
        [SwaggerResponse(StatusCodes.Status200OK, "Role changed successfully.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not in role Admin.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or role not found.", typeof(Result))]
        public async Task<IActionResult> ChangeUserRole(int id, ChangeUserRoleRequestDto request)
        {
            return ApiResult.GetIActionResult(await usersService.ChangeUserRole(id, request));
        }

        /// <summary>
        /// Retrieves the list of all users along with their assigned roles.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<List<UserDto>>))]
        public async Task<IActionResult> GetUserList()
        {
            return ApiResult.GetIActionResult(await usersService.GetUserList());
        }
    }
}
