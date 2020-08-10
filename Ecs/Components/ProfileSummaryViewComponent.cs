using Ecs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Ecs.Components
{
    public class ProfileSummaryViewComponent : ViewComponent
    {
        private UserManager<ApplicationUser> _userManager;

        public ProfileSummaryViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var user = _userManager.Users.Where(user => user.UserName == User.Identity.Name).FirstOrDefault();
            return View(user);
        }
    }
}
