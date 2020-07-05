using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Ecs.Models
{
    public class SeedDatabase
    {
        private class Authorization
        {
            public enum Roles
            {
                Administrators, Users
            }

            public const string default_username = "admin";
            public const string default_email = "admin@example.com";
            public const string default_password = "#Secret123";
            public const Roles default_role = Roles.Users;
        }

        public async static Task SeedEssentialsAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Default Roles
            await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Administrators.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Users.ToString()));

            // Seed the default user
            var defaultUser = new ApplicationUser { UserName = Authorization.default_username, Email = Authorization.default_email, EmailConfirmed = true, PhoneNumberConfirmed = true };

            if (userManager.Users.All(u => u.Email != Authorization.default_email))
            {
                await userManager.CreateAsync(defaultUser, Authorization.default_password);
                await userManager.AddToRoleAsync(defaultUser, Authorization.Roles.Administrators.ToString());
            }
        }
    }
}
