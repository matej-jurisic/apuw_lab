using APUW.API.Core;
using APUW.API.Middleware.Authorization;
using APUW.Domain.Core.Results;
using APUW.Domain.Interfaces;
using APUW.Model.DTOs.Boards;
using APUW.Model.DTOs.Boards.Requests;
using APUW.Model.DTOs.Users;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APUW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class BoardsController(IBoardsService boardsService) : ControllerBase
    {
        /// <summary>
        /// Returns all boards visible to the user.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<List<BoardDto>>))]
        public async Task<IActionResult> GetBoardList()
        {
            return ApiResult.GetIActionResult(await boardsService.GetBoardList());
        }

        /// <summary>
        /// Returns a board by ID.
        /// </summary>
        /// <remarks>
        /// Only accessible by board members or users with role Admin
        /// </remarks>
        [HttpGet("{boardId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "User can access board.", typeof(Result<BoardDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to access board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        public async Task<IActionResult> GetBoard(int boardId)
        {
            return ApiResult.GetIActionResult(await boardsService.GetBoard(boardId));
        }

        /// <summary>
        /// Creates and returns a new board.
        /// </summary>
        /// <remarks>
        /// Only accessible by users with the "User" role.
        /// </remarks>
        [HttpPost]
        [CheckRoles("User")]
        [SwaggerResponse(StatusCodes.Status201Created, "Board created successfully.", typeof(Result<BoardDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not in role User.", typeof(Result))]
        public async Task<IActionResult> CreateBoard(CreateBoardRequestDto request)
        {
            return ApiResult.GetIActionResult(await boardsService.CreateBoard(request));
        }

        /// <summary>
        /// Updates and returns an existing board.
        /// </summary>
        /// <remarks>
        /// Only accessible by board owners.
        /// </remarks>
        [HttpPut("{boardId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Board updated successfully.", typeof(Result<BoardDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to update the board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        public async Task<IActionResult> UpdateBoard(int boardId, UpdateBoardRequestDto request)
        {
            return ApiResult.GetIActionResult(await boardsService.UpdateBoard(boardId, request));
        }

        /// <summary>
        /// Deletes an existing board.
        /// </summary>
        /// <remarks>
        /// Only accessible by board owners or users with role Admin
        /// </remarks>
        [HttpDelete("{boardId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Board deleted successfully.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to delete the board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        public async Task<IActionResult> DeleteBoard(int boardId)
        {
            return ApiResult.GetIActionResult(await boardsService.DeleteBoard(boardId));
        }

        /// <summary>
        /// Returns a list of users that are members of a specific board.
        /// </summary>
        /// <remarks>
        /// Only accessible by board members or users with role Admin
        /// </remarks>
        [HttpGet("{boardId}/members")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<List<UserDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to view board members.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        public async Task<IActionResult> GetBoardMembers(int boardId)
        {
            return ApiResult.GetIActionResult(await boardsService.GetBoardMembers(boardId));
        }

        /// <summary>
        /// Adds a new member to a board.
        /// </summary>
        /// <remarks>
        /// Only accessible by board owners or users with role Admin
        /// </remarks>
        [HttpPut("{boardId}/members/{userId}")]
        [SwaggerResponse(StatusCodes.Status201Created, type: typeof(Result<BoardMemberDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to add board members.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "User is already a member of the board.", typeof(Result))]
        public async Task<IActionResult> AddBoardMember(int boardId, int userId)
        {
            return ApiResult.GetIActionResult(await boardsService.AddBoardMember(boardId, userId));
        }

        /// <summary>
        /// Removes a member from a board.
        /// </summary>
        /// <remarks>
        /// Board owners and users with role Admin can remove other members.
        /// Members can remove themselves.
        /// </remarks>
        [HttpDelete("{boardId}/members/{userId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to remove board members.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found or user not in board.", typeof(Result))]
        public async Task<IActionResult> RemoveBoardMember(int boardId, int userId)
        {
            return ApiResult.GetIActionResult(await boardsService.RemoveBoardMember(boardId, userId));
        }
    }
}
