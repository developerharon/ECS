using Microsoft.AspNetCore.Identity;

namespace Ecs.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}
