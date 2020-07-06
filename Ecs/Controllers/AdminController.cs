using Ecs.Models;
using Ecs.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ecs.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserValidator<ApplicationUser> _userValidator;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUserValidator<ApplicationUser> userValidator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userValidator = userValidator;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListEmployees(string searchParameter = null)
        {
            if (!string.IsNullOrWhiteSpace(searchParameter))
                return View(_userManager.Users.Where(user => ((user.UserName.Contains(searchParameter) || user.Name.Contains(searchParameter) || user.Department.Contains(searchParameter) || user.Email.Contains(searchParameter)))));
            return View(_userManager.Users);
        }

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

        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                return View(new EditViewModel { Id = user.Id, Name = user.Name, Email = user.Email, Department = user.Department });
            } else
            {
                return RedirectToAction("ListEmployees");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(model.Id);

                if (user != null)
                {
                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.Department = model.Department;

                    if (model.ProfilePicture != null)
                    {
                        using (var datastream = new MemoryStream())
                        {
                            await model.ProfilePicture.CopyToAsync(datastream);
                            user.ProfilePicture = datastream.ToArray();
                        }
                    }

                    IdentityResult validUser = await _userValidator.ValidateAsync(_userManager, user);

                    if (!validUser.Succeeded)
                    {
                        AddErrorsFromResult(validUser);
                    }

                    IdentityResult result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListEmployees");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
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
