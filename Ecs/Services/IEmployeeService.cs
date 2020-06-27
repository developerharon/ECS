using Ecs.Entities;
using Ecs.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecs.Services
{
    public interface IEmployeeService
    {
        IQueryable<Employee> Employees { get; }

        Task<IdentityResult> RegisterEmployeeAsync(RegisterModel model);
        Task<IdentityResult> DeleteEmployeeAsync(string id);
        Task<IdentityResult> EditEmployeeAsync(EditModel model);
        Task<IdentityResult> ChangePassword(string employeeId, string password);
        Task<AuthenticatedModel> GetTokenAsync(LoginModel model);
        Task<AuthenticatedModel> RefreshTokenAsync(string token);
        bool RevokeToken(string token);
    }
}
