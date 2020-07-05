using Ecs.Models;
using Ecs.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;

namespace Ecs.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListEmployees() => View(_userManager.Users);

        public async Task<IActionResult> ListAdministrators()
        {
            List<ApplicationUser> admins = new List<ApplicationUser>();

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, SeedDatabase.Authorization.Roles.Administrators.ToString()))
                {
                    admins.Add(user);
                }
            }
            return View(admins);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                    return View(model);
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Name = model.Name,
                    Department = model.Department,
                };

                if (model.ProfilePicture != null)
                {
                    using (var dataStream = new MemoryStream())
                    {
                        await model.ProfilePicture.CopyToAsync(dataStream);
                        user.ProfilePicture = dataStream.ToArray();
                    }
                }

                IdentityResult result = await _userManager.CreateAsync(user, model.NewPassword);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(SeedDatabase.Authorization.Roles.Users.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SeedDatabase.Authorization.Roles.Users.ToString()));
                    }

                    result = await _userManager.AddToRoleAsync(user, SeedDatabase.Authorization.Roles.Users.ToString());

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListEmployees");
                    }
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListEmployees");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View("ListEmployees", _userManager.Users);
        }

        [HttpPost]
        public async Task<IActionResult> MakeAdministrator(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, SeedDatabase.Authorization.Roles.Administrators.ToString()))
                {
                    TempData["message"] = $"{user.UserName} is already an administrator.";
                    return RedirectToAction("ListEmployees");
                }

                IdentityResult result = await _userManager.AddToRoleAsync(user, SeedDatabase.Authorization.Roles.Administrators.ToString());

                if (result.Succeeded)
                {
                    TempData["message"] = $"{user.UserName} is now an administrator.";
                    return RedirectToAction("ListEmployees");
                }

                TempData["message"] = "Error occurred when processing your request";
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View("ListEmployees", _userManager.Users);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdministrator(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, SeedDatabase.Authorization.Roles.Administrators.ToString()))
                {
                    IdentityResult result = await _userManager.RemoveFromRoleAsync(user, SeedDatabase.Authorization.Roles.Administrators.ToString());

                    if (result.Succeeded)
                    {
                        TempData["message"] = $"{user.UserName} removed as an administrator";
                        return RedirectToAction("ListAdministrators");
                    }
                    TempData["message"] = "An error occurred when processing your request";
                }
                else
                {
                    TempData["message"] = $"{user.UserName} is not an administrator";
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return RedirectToAction("ListAdministrators");
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
