using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APUW.Model.Entities
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int BoardId { get; set; }
        [ForeignKey(nameof(BoardId))]
        public virtual Board Board { get; set; } = null!;

        public int? AssignedToUserId { get; set; }
        [ForeignKey(nameof(AssignedToUserId))]
        public virtual User? AssignedToUser { get; set; }

        public virtual List<Event> Events { get; set; } = [];
    }
}
