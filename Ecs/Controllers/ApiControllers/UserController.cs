using Ecs.Models.ApiServices;
using Ecs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost("email")]
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

        [HttpPost("login")]
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            var result = await _userService.GetTokenAsync(model);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _userService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = _userService.RevokeToken(token);

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token Revoked" });
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