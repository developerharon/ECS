using Ecs.Models;
using ECSApi.Models;
using ECSApi.Models.ApiModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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

        [HttpPost("email")]
        public async Task<List<Timestamp>> GetAllClocksAsync([FromBody] string email)
        {
            var result = await _userService.GetAllClocksAsync(email);
            return result;
        }

        [HttpPost("profile-picture")]
        public async Task<HttpResponseMessage> GetProfilePictureAsync([FromBody] string email)
        {
            var profilePic = await _userService.GetProfilePictureAsync(email);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(profilePic);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            return result;
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            var result = await _userService.GetTokenAsync(model);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody]string refreshToken)
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
        public async Task<IActionResult> GetApplicationStateAsync([FromBody]string email)
        {
            var result = await _userService.GetActiveClockAsync(email);
            return Ok(result);
        }
    }
}
