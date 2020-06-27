using System.ComponentModel.DataAnnotations;

namespace Ecs.Models
{
    public class EditModel
    {
        [Required]
        public string employeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
