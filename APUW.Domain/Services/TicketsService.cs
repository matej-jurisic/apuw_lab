using APUW.Domain.Core.Results;
using APUW.Domain.Interfaces;
using APUW.Model;
using APUW.Model.DTOs.Events;
using APUW.Model.DTOs.Tickets;
using APUW.Model.DTOs.Tickets.Requests;
using APUW.Model.DTOs.Users;
using APUW.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace APUW.Domain.Services
{
    public class TicketsService(AppDbContext context, ICurrentUserService currentUserService) : ITicketsService
    {
        private readonly AppDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly UserDto _currentUser = currentUserService.GetCurrentUser();

        public async Task<Result<TicketDto>> CreateTicket(int boardId, CreateTicketRequestDto request)
        {
            var board = await _context.Boards.FindAsync(boardId);

            if (board == null) return Result.Failure(ResultStatus.NotFound, "Board not found.");

            var isMember = await _context.UserBoards.AnyAsync(x => x.UserId == _currentUser.Id && x.BoardId == boardId);

            if (!isMember) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to create tickets in this board.");

            if (request.AssignedToUserId != null)
            {
                var isAssignedUserMember = await _context.UserBoards.AnyAsync(x => x.UserId == request.AssignedToUserId && x.BoardId == boardId);
                if (!isAssignedUserMember) return Result.Failure(ResultStatus.BadRequest, "Assigned user is not a member of the board.");
            }

            var ticket = new Ticket
            {
                AssignedToUserId = request.AssignedToUserId,
                BoardId = boardId,
                Content = request.Content,
                Name = request.Name,
            };

            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            await CreateEvent(ticket.Id, $"Ticket '{ticket.Name}' was created by {_currentUser.Username}.");

            return await GetTicket(boardId, ticket.Id);
        }

        public async Task<Result> DeleteTicket(int boardId, int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(x => x.Board)
                .FirstOrDefaultAsync(x => x.BoardId == boardId && x.Id == ticketId);

            if (ticket == null) return Result.Failure(ResultStatus.NotFound, "Ticket not found.");

            if (ticket.Board.OwnerId != _currentUser.Id) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to delete tickets in this board.");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<List<EventDto>>> GetEventList(int boardId, int ticketId)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");
            var isMember = await _context.UserBoards.AnyAsync(x => x.BoardId == boardId && x.UserId == _currentUser.Id);

            if (!isAdmin && !isMember) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to access events in this board.");

            var events = await _context.Events
                .Where(x => x.TicketId == ticketId && x.Ticket.BoardId == boardId)
                .Select(x => new EventDto
                {
                    Content = x.Content,
                    CreatedByUser = new UserDto
                    {
                        Id = x.CreatedByUserId,
                        Roles = x.CreatedByUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
                        Username = x.CreatedByUser.Username,
                    },
                    CreatedDate = x.CreatedDate,
                    Id = x.Id,
                }).ToListAsync();

            return Result.Success(events);
        }

        public async Task<Result<TicketDto>> GetTicket(int boardId, int ticketId)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");
            var isMember = await _context.UserBoards.AnyAsync(x => x.BoardId == boardId && x.UserId == _currentUser.Id);

            if (!isAdmin && !isMember) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to access tickets in this board.");

            var ticket = await _context.Tickets
                .Where(x => x.BoardId == boardId)
                .Select(x => new TicketDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Content = x.Content,
                    AssignedToUser = x.AssignedToUser != null ? new UserDto
                    {
                        Id = x.AssignedToUser.Id,
                        Username = x.AssignedToUser.Username,
                        Roles = x.AssignedToUser.UserRoles.Select(ur => ur.Role.Name).ToList()
                    } : null
                })
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null) return Result.Failure(ResultStatus.NotFound, "Ticket not found.");

            return Result.Success(ticket);
        }

        public async Task<Result<List<TicketListItemDto>>> GetTicketList(int boardId)
        {
            var isAdmin = await _currentUserService.HasRole("Admin");
            var isMember = await _context.UserBoards.AnyAsync(x => x.BoardId == boardId && x.UserId == _currentUser.Id);

            if (!isAdmin && !isMember) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to access tickets in this board.");

            var tickets = await _context.Tickets
                .Where(x => x.BoardId == boardId)
                .Select(x => new TicketListItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    AssignedToUser = x.AssignedToUser != null ? new UserDto
                    {
                        Id = x.AssignedToUser.Id,
                        Username = x.AssignedToUser.Username,
                        Roles = x.AssignedToUser.UserRoles.Select(ur => ur.Role.Name).ToList()
                    } : null
                })
                .ToListAsync();

            return Result.Success(tickets);
        }

        public async Task<Result<TicketDto>> UpdateTicket(int boardId, int ticketId, UpdateTicketRequestDto request)
        {
            var ticket = await _context.Tickets
                .Include(x => x.Board)
                .FirstOrDefaultAsync(x => x.Id == ticketId && x.BoardId == boardId);

            if (ticket == null) return Result.Failure(ResultStatus.NotFound, "Ticket not found.");

            if (ticket.AssignedToUserId != _currentUser.Id && ticket.Board.OwnerId != _currentUser.Id) return Result.Failure(ResultStatus.Forbidden, "You are not authorized to update tickets in this board.");

            if (request.AssignedToUserId != null)
            {
                var isAssignedUserMember = await _context.UserBoards.AnyAsync(x => x.UserId == request.AssignedToUserId && x.BoardId == boardId);
                if (!isAssignedUserMember) return Result.Failure(ResultStatus.BadRequest, "Assigned user is not a member of the board.");
            }

            if (ticket.AssignedToUserId != request.AssignedToUserId)
            {
                var assignedUser = await _context.Users.FindAsync(request.AssignedToUserId);
                var assignedUsername = assignedUser?.Username ?? "none";
                await CreateEvent(ticket.Id, $"Ticket assigned to '{assignedUsername}' by {_currentUser.Username}.");
            }

            if (ticket.Content != request.Content)
            {
                await CreateEvent(ticket.Id, $"Ticket content was updated by {_currentUser.Username}.");
            }

            if (ticket.Name != request.Name)
            {
                await CreateEvent(ticket.Id, $"Ticket name changed from '{ticket.Name}' to '{request.Name}' by {_currentUser.Username}.");
            }

            ticket.AssignedToUserId = request.AssignedToUserId;
            ticket.Content = request.Content;
            ticket.Name = request.Name;

            await _context.SaveChangesAsync();

            return await GetTicket(boardId, ticket.Id);
        }

        private async Task CreateEvent(int ticketId, string content)
        {
            var createdEvent = new Event
            {
                Content = content,
                CreatedByUserId = _currentUser.Id,
                CreatedDate = DateTime.UtcNow,
                TicketId = ticketId,
            };

            await _context.Events.AddAsync(createdEvent);
            await _context.SaveChangesAsync();
        }
    }
}
