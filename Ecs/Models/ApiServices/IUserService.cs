using Ecs.Models;
using ECS.Models.ApiModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECS.Models.ApiServices
{
    public interface IUserService
    {
        Task<AuthenticationModel> LoginAsync(TokenRequestModel model);
        Task<TimestampResponseModel> ClockInAsync(TimestampModel model);
        Task<TimestampResponseModel> ClockOutAsync(TimestampModel model);
        Task<TimestampResponseModel> GetActiveClockAsync(string email);
        Task<List<Timestamp>> GetAllClocksAsync(string email);
        Task<string> GetProfilePicture(string email);
    }
}