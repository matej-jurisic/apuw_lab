using System.ComponentModel.DataAnnotations;

namespace APUW.Model.DTOs.Users.Requests
{
    public class ChangeUserRoleRequestDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
