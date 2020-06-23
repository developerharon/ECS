using Ecs.Models;
using System.Threading.Tasks;

namespace Ecs.Services
{
    public interface IEmployeeService
    {
        Task<string> RegisterAsync(RegisterModel model);
        Task<AuthenticatedModel> GetTokenAsync(LoginModel model);
    }
}
