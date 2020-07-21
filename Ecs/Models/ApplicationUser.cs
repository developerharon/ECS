using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Ecs.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string ProfilePicture { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<Timestamp> Timestamps { get; set; }
    }
}
