using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [StringLength(8, MinimumLength = 4)]
        [Required]
        public string Password { get; set; }
    }
}