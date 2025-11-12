using APUW.Domain.Core.Results;
using APUW.Model.DTOs.Events;
using APUW.Model.DTOs.Tickets;
using APUW.Model.DTOs.Tickets.Requests;

namespace APUW.Domain.Interfaces
{
    public interface ITicketsService
    {
        Task<Result<TicketDto>> CreateTicket(int boardId, CreateTicketRequestDto request);
        Task<Result<TicketDto>> UpdateTicket(int boardId, int ticketId, UpdateTicketRequestDto request);
        Task<Result<TicketDto>> GetTicket(int boardId, int ticketId);
        Task<Result<List<TicketListItemDto>>> GetTicketList(int boardId);
        Task<Result> DeleteTicket(int boardId, int ticketId);
        Task<Result<List<EventDto>>> GetEventList(int boardId, int ticketId);
    }
}
