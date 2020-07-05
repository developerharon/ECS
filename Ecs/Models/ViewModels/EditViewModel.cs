using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ecs.Models.ViewModels
{
    public class EditViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Department { get; set; }
        public IFormFile ProfilePicture { get; set; }
    }
}
