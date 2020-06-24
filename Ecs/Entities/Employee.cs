using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Ecs.Entities
{
    public class Employee : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<TimeStamp> TimeStamps { get; set; }
    }
}