using System.ComponentModel.DataAnnotations;

namespace APUW.Model.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public virtual List<UserRole> UserRoles { get; set; } = [];
        public virtual List<Board> OwnedBoards { get; set; } = [];
        public virtual List<UserBoard> MemberBoards { get; set; } = [];
    }
}
