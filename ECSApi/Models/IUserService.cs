using ECSApi.Models.ApiModels;
using System.Threading.Tasks;

namespace ECSApi.Models
{
    public interface IUserService
    {
        Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model);
    }
}
