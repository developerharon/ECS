using Ecs.Entities;
using Ecs.Settings;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Ecs.Contexts
{
    public class ApplicationDbContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Administrator.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.User.ToString()));
        }

        public static async Task SeedDefaultUserAsync(UserManager<Employee> userManager)
        {
            var defaultUser = new Employee { UserName = Authorization.default_admin_username, Email = Authorization.default_admin_email, EmailConfirmed = true, PhoneNumberConfirmed = true };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                await userManager.CreateAsync(defaultUser, Authorization.default_admin_password);
                await userManager.AddToRoleAsync(defaultUser, Authorization.default_admin_role.ToString());
            }
        }
    }
}
