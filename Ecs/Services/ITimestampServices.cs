using Ecs.Entities;
using Ecs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecs.Services
{
    public interface ITimestampServices
    {
        Task<TimestampResultModel> ClockInAsync(TimestampModel model);
        Task<TimestampResultModel> ClockOutAsync(TimestampModel model);
        Task<List<TimeStamp>> GetEmployeeTimestampsAsync(string employeeId);

        Task<ApplicationStateModel> GetApplicationStateAsync(string employeeId);
    }
}
