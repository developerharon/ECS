using Ecs.Models;
using Ecs.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUserValidator<ApplicationUser> userValidator, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userValidator = userValidator;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            DashboardViewModel model = GetDashboardSummary();
            return View(model);
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

                string uniqueProfilePicUrl = GetProfilePictureUrl(model.ProfilePicture);

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Name = model.Name,
                    Department = model.Department,
                    ProfilePicture = uniqueProfilePicUrl
                };

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

                    string uniqueProfilePicUrl = GetProfilePictureUrl(model.ProfilePicture);

                    if (uniqueProfilePicUrl != null)
                    {
                        DeleteProfilePicture(user.ProfilePicture);
                        user.ProfilePicture = uniqueProfilePicUrl;
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

        private string GetProfilePictureUrl(IFormFile profilePicture)
        {
            string uniqueUrl = null;

            if (profilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                var fileName = Path.GetFileName(profilePicture.FileName);
                uniqueUrl = Guid.NewGuid().ToString() + "-" + fileName;
                var filePath = Path.Combine(uploadsFolder, uniqueUrl);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    profilePicture.CopyTo(fileStream);
                }
            }

            return uniqueUrl;
        }

        private void DeleteProfilePicture(string profilePictureUrl)
        {
            if (profilePictureUrl != null)
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", profilePictureUrl);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
        }

        private DashboardViewModel GetDashboardSummary()
        {
            var result = new DashboardViewModel();
            var employees = _userManager.Users;

            result.TotalNumberOfEmployees = employees.Count();

            foreach (ApplicationUser user in employees)
            {
                foreach (Timestamp clock in user.Timestamps)
                {
                    if (clock.IsActive && clock.In.Date == DateTime.Now.Date)
                    {
                        result.TotalNumberOfActiveClocks += 1;
                        continue;
                    }

                    if (!clock.IsActive && clock.In.Date == DateTime.Now.Date)
                    {
                        result.TotalNumberOfClosedClocks += 1;
                    }
                }
            }

            result.Averages.WeeklyAverages = GetWeeklyAverage();
            result.Averages.MonthlyAverages = GetMonthlyAverage();
            result.Averages.YearlyAverages = GetYearlyDouble();

            return result;
        }

        private double GetWeeklyAverage()
        {
            double result = 0.0;
            var users = _userManager.Users;

            double sum = 0;

            foreach(var user in users)
            {
                foreach (Timestamp stamp in user.Timestamps)
                {
                    if (stamp.Out.AddDays(7) >= DateTime.Now)
                    {
                        sum += stamp.Out.Hour - stamp.In.Hour;
                    }
                }
            }

            result = sum / 7;
            return result;
        }

        private double GetMonthlyAverage()
        {
            double result = 0.0;
            var users = _userManager.Users;

            double sum = 0;

            foreach (var user in users)
            {
                foreach (Timestamp stamp in user.Timestamps)
                {
                    if (stamp.Out.AddDays(28) >= DateTime.Now)
                    {
                        sum += stamp.Out.Hour - stamp.In.Hour;
                    }
                }
            }

            result = sum / 7;
            return result;
        }

        private double GetYearlyDouble()
        {
            double result = 0.0;
            var users = _userManager.Users;

            double sum = 0;

            foreach (var user in users)
            {
                foreach (Timestamp stamp in user.Timestamps)
                {
                    if (stamp.Out.AddDays(365) >= DateTime.Now)
                    {
                        sum += stamp.Out.Hour - stamp.In.Hour;
                    }
                }
            }

            result = sum / 7;
            return result;
        }
    }
}
