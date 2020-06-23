using Microsoft.AspNetCore.Mvc;

namespace Ecs.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
