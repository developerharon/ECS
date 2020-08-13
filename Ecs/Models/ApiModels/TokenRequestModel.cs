using System.ComponentModel.DataAnnotations;

namespace Ecs.Models.ApiModels
{
    public class TokenRequestModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}