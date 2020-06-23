using Microsoft.AspNetCore.Identity;

namespace Ecs.Entities
{
    public class Employee : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
    }
}