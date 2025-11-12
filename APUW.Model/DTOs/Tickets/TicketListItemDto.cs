using APUW.Model.DTOs.Users;

namespace APUW.Model.DTOs.Tickets
{
    public class TicketListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public UserDto? AssignedToUser { get; set; }
    }
}
