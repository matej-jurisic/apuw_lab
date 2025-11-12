using System.ComponentModel.DataAnnotations;

namespace APUW.Model.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
