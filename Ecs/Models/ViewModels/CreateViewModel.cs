using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ecs.Models.ViewModels
{
    public class CreateViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Department { get; set; }
        public IFormFile ProfilePicture { get; set; }
        [Required]
        [UIHint("password")]
        public string NewPassword { get; set; }
        [Required]
        [UIHint("password")]
        public string ConfirmPassword { get; set; }
    }
}
