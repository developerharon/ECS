using Ecs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecs.Models.ApiServices;
using Ecs.Models.ApiModels;

namespace ECS.ApiControllers.Controllers
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
        public async Task<AuthenticationModel> LoginAsync(TokenRequestModel model)
        {
            var result = await _userService.LoginAsync(model);
            return result;
        }

        [HttpPost("get-clock")]
        public async Task<List<Timestamp>> GetAllClocksAsync([FromBody] string email)
        {
            var result = await _userService.GetAllClocksAsync(email);
            return result;
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> GetProfilePicture([FromBody] string email)
        {
            string profilePicture = await _userService.GetProfilePicture(email);
            return Ok(new { pic = profilePicture });
        }

        [HttpPost("clock-in")]
        public async Task<IActionResult> ClockInAsync(TimestampModel model)
        {
            var result = await _userService.ClockInAsync(model);
            return Ok(result);
        }

        [HttpPost("clock-out")]
        public async Task<IActionResult> ClockOutAsync(TimestampModel model)
        {
            var result = await _userService.ClockOutAsync(model);
            return Ok(result);
        }

        [HttpPost("application-state")]
        public async Task<IActionResult> GetApplicationStateAsync([FromBody] string email)
        {
            var result = await _userService.GetActiveClockAsync(email);
            return Ok(result);
        }
    }
}