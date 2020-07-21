using Ecs.Models;
using ECSApi.Models.ApiModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ECSApi.Models
{
    public interface IUserService
    {
        Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model);
        Task<AuthenticationModel> RefreshTokenAsync(string token);
        bool RevokeToken(string token);
        Task<TimestampResponseModel> ClockInAsync(TimestampModel model);
        Task<TimestampResponseModel> ClockOutAsync(TimestampModel model);
        Task<TimestampResponseModel> GetActiveClockAsync(string email);
        Task<List<Timestamp>> GetAllClocksAsync(string email);
    }
}
