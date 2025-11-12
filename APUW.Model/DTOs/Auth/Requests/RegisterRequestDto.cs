using System.ComponentModel.DataAnnotations;

namespace APUW.Model.DTOs.Auth.Requests
{
    public class RegisterRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
