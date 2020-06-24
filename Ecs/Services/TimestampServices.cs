using Ecs.Contexts;
using Ecs.Entities;
using Ecs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Ecs.Services
{
    public class TimestampServices : ITimestampServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Employee> _userManager;

        public TimestampServices(ApplicationDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<TimestampResultModel> ClockInAsync(TimestampModel model)
        {
            var timestampResult = new TimestampResultModel();

            bool isEmployeeOnPremises = IsEmployeeOnPremises(model.TimeStampLocation);

            if (!isEmployeeOnPremises)
            {
                timestampResult.Succeded = false;
                timestampResult.Message = "You are not on the work premises";
                return timestampResult;
            }

            var user = await _userManager.FindByIdAsync(model.EmployeeId);

            if (user == null)
            {
                timestampResult.Succeded = false;
                timestampResult.Message = "Employee not found";
                return timestampResult;
            }

            // Create a new Timestamp and set it as active
            var timestamp = new TimeStamp
            {
                In = model.TimeStampTime,
                InWhileOnPremises = isEmployeeOnPremises,
                IsActive = true,
            };
            timestampResult.Succeded = true;
            timestampResult.Message = "Timestamp created";
            user.TimeStamps.Add(timestamp);
            _context.Update(user);
            _context.SaveChanges();
            return timestampResult;
        }

        public async Task<TimestampResultModel> ClockOutAsync(TimestampModel model)
        {
            var timestampResult = new TimestampResultModel();

            var isEmployeeOnPremises = IsEmployeeOnPremises(model.TimeStampLocation);

            if (!isEmployeeOnPremises)
            {
                timestampResult.Succeded = false;
                timestampResult.Message = "You are not on work premises";
                return timestampResult;
            }

            var user = await _userManager.FindByIdAsync(model.EmployeeId);

            if (user == null)
            {
                timestampResult.Succeded = false;
                timestampResult.Message = "User Not Found";
                return timestampResult;
            }

            var timestamp = user.TimeStamps.Single(timestamp => timestamp.IsActive);
            timestamp.Out = model.TimeStampTime;
            timestamp.OutWhileOnPremises = isEmployeeOnPremises;
            timestamp.IsActive = false;
            _context.Update(user);
            _context.SaveChanges();

            timestampResult.Succeded = true;
            timestampResult.Message = "Timestamp updated";
            return timestampResult;
        }

        public async Task<List<TimeStamp>> GetEmployeeTimestampsAsync(string employeeId)
        {
            var user = await _userManager.FindByIdAsync(employeeId);

            if (user == null)
                return new List<TimeStamp>();

            return user.TimeStamps;
        }

        public async Task<ApplicationStateModel> GetApplicationStateAsync(string employeeId)
        {
            var result = new ApplicationStateModel();

            var user = await _userManager.FindByIdAsync(employeeId);

            if (user == null)
                return result;

            result.FirstName = user.FirstName;
            result.LastName = user.LastName;
            result.HasActiveTimestamp = user.TimeStamps.Any(timestamp => timestamp.IsActive);

            return result;
        }

        private bool IsEmployeeOnPremises(Location location)
        {
            // Logic to determine if the employee is on work premises
            return true;
        }
    }
}
