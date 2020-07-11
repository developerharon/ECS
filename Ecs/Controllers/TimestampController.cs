using Ecs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecs.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class TimestampController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimestampController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult List() => View(_context.Users);
    }
}
