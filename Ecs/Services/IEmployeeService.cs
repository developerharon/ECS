using Ecs.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Ecs.Services
{
    public interface IEmployeeService
    {
        Task<IdentityResult> RegisterEmployeeAsync(RegisterModel model);
        Task<AuthenticatedModel> GetTokenAsync(LoginModel model);
        Task<AuthenticatedModel> RefreshTokenAsync(string token);
        bool RevokeToken(string token);
    }
}
