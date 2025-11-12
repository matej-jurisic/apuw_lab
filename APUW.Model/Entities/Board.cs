using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APUW.Model.Entities
{
    public class Board
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public virtual User Owner { get; set; } = null!;

        public virtual List<UserBoard> BoardUsers { get; set; } = [];
    }
}
