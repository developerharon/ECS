using Ecs.Entities;
using Ecs.Models;
using Ecs.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecs.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimestampController : ControllerBase
    {
        private readonly ITimestampServices _timestampServices;

        public TimestampController(ITimestampServices services)
        {
            _timestampServices = services;
        }

        [HttpGet("{id}")]
        public async Task<List<TimeStamp>> GetEmployeeClocksAsync(string employeeId)
        {
            var result = await _timestampServices.GetEmployeeTimestampsAsync(employeeId);

            return result;
        }

        [HttpGet("application-state")]
        public async Task<ApplicationStateModel> GetEmployeesApplicationState(string employeeId)
        {
            var result = await _timestampServices.GetApplicationStateAsync(employeeId);
            return result;
        }

        [HttpPost("in")]
        public async Task<IActionResult> In(TimestampModel model)
        {
            TimestampResultModel result = await _timestampServices.ClockInAsync(model);

            if (result.Succeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("out")]
        public async Task<IActionResult> Out(TimestampModel model)
        {
            TimestampResultModel result = await _timestampServices.ClockOutAsync(model);

            if (result.Succeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
