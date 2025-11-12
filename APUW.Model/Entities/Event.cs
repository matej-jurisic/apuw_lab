using System.ComponentModel.DataAnnotations.Schema;

namespace APUW.Model.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; } = string.Empty;

        public int TicketId { get; set; }
        [ForeignKey(nameof(TicketId))]
        public virtual Ticket Ticket { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
