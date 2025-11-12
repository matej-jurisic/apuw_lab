namespace APUW.Model.DTOs.Tickets.Requests
{
    public class UpdateTicketRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? AssignedToUserId { get; set; } = null;
    }
}
