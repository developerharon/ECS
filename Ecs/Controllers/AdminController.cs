using Ecs.Entities;
using Ecs.Models;
using Ecs.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Ecs.Controllers
{
    public class AdminController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public AdminController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            return View(_employeeService.Employees);
        }

        public ViewResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _employeeService.RegisterEmployeeAsync(model);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach(IdentityError error in result.Errors)
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
            IdentityResult result = await _employeeService.DeleteEmployeeAsync(id);
            if (result != null && result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (result.Errors.ToList().Count() != 0)
                    AddErrorsFromResult(result);

                ModelState.AddModelError("", "User Not Found");
            }
            return View("Index", _employeeService.Employees);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var employee = _employeeService.Employees.Single(e => e.Id == id);

            if (employee != null)
                return View(employee);

            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> Edit(RegisterModel model)
        //{

        //}

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
