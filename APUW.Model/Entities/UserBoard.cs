using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APUW.Model.Entities
{
    public class UserBoard
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public int BoardId { get; set; }
        [ForeignKey(nameof(BoardId))]
        public virtual Board Board { get; set; } = null!;
    }
}
