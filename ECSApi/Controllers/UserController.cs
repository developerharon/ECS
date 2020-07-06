using Ecs.Models.ViewModels;
using ECSApi.Models;
using ECSApi.Models.ApiModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            var result = await _userService.GetTokenAsync(model);
            return Ok(result);
        }
    }
}
