using APUW.API.Core;
using APUW.API.Middleware.Authorization;
using APUW.Domain.Core.Results;
using APUW.Domain.Interfaces;
using APUW.Model.DTOs.Events;
using APUW.Model.DTOs.Tickets;
using APUW.Model.DTOs.Tickets.Requests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APUW.API.Controllers
{
    [ApiController]
    [Route("api/boards/{boardId}/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class TicketsController(ITicketsService ticketsService) : ControllerBase
    {
        /// <summary>
        /// Returns all tickets associated with the board.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<List<TicketListItemDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to access tickets in board.", typeof(Result))]
        public async Task<IActionResult> GetTicketList(int boardId)
        {
            return ApiResult.GetIActionResult(await ticketsService.GetTicketList(boardId));
        }

        /// <summary>
        /// Returns a ticket by ID. Can only be accessed by board members or users with role "Admin".
        /// </summary>
        [HttpGet("{ticketId}")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<TicketDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to access tickets in this board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ticket not found.", typeof(Result))]
        public async Task<IActionResult> GetTicket(int boardId, int ticketId)
        {
            return ApiResult.GetIActionResult(await ticketsService.GetTicket(boardId, ticketId));
        }

        /// <summary>
        /// Creates and returns a new ticket.
        /// </summary>
        /// <remarks>
        /// Only accessible by members of the board
        /// </remarks>
        [HttpPost]
        [CheckRoles("User")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<TicketDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Assigned user is not a member of the board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to create tickets in this board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Board not found.", typeof(Result))]
        public async Task<IActionResult> CreateTicket(int boardId, CreateTicketRequestDto request)
        {
            return ApiResult.GetIActionResult(await ticketsService.CreateTicket(boardId, request));
        }

        /// <summary>
        /// Updates and returns an existing ticket.
        /// </summary>
        /// <remarks>
        /// Only accessible by the board owner and the ticket assignee.
        /// </remarks>
        [HttpPut("{ticketId}")]
        [CheckRoles("User")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<TicketDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Assigned user is not a member of the board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to update tickets in this board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ticket not found.", typeof(Result))]
        public async Task<IActionResult> UpdateTicket(int boardId, int ticketId, UpdateTicketRequestDto request)
        {
            return ApiResult.GetIActionResult(await ticketsService.UpdateTicket(boardId, ticketId, request));
        }

        /// <summary>
        /// Deletes an existing ticket.
        /// </summary>
        /// <remarks>
        /// Only accessible by the board owner.
        /// </remarks>
        [HttpDelete("{ticketId}")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to delete tickets in this board.", typeof(Result))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Ticket not found.", typeof(Result))]
        public async Task<IActionResult> DeleteTicket(int boardId, int ticketId)
        {
            return ApiResult.GetIActionResult(await ticketsService.DeleteTicket(boardId, ticketId));
        }

        /// <summary>
        /// Returns all events associated with the ticket.
        /// </summary>
        /// <remarks>
        /// Only accessible by board members.
        /// </remarks>
        [HttpGet("{ticketId}/events")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Result<List<EventDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to access events for ticket.", typeof(Result))]
        public async Task<IActionResult> GetEventList(int boardId, int ticketId)
        {
            return ApiResult.GetIActionResult(await ticketsService.GetEventList(boardId, ticketId));
        }
    }
}
