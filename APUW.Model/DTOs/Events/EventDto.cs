using APUW.Model.DTOs.Users;

namespace APUW.Model.DTOs.Events
{
    public class EventDto
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; } = string.Empty;

        public UserDto CreatedByUser { get; set; } = null!;
    }
}
