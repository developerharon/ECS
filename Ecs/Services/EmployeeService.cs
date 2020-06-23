using Ecs.Contexts;
using Ecs.Entities;
using Ecs.Models;
using Ecs.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ecs.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly UserManager<Employee> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly ApplicationDbContext _context;

        public EmployeeService(UserManager<Employee> userManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _context = context;
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            var employee = new Employee
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Department = model.Department,
                UserName = model.Username,
                Email = model.Email
            };

            var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
            
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(employee, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(employee, Authorization.default_role.ToString());
                }
                return $"User Registered with username {employee.UserName}";
            }
            else
            {
                return $"Email {employee.Email} is already registered.";
            }
        }

        public async Task<AuthenticatedModel> GetTokenAsync(LoginModel model)
        {
            var authenticateModel = new AuthenticatedModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                authenticateModel.IsAuthenticated = false;
                authenticateModel.Message = $"No Accounts Registered with {model.Email}";
                return authenticateModel;
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authenticateModel.IsAuthenticated = true;
                JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                authenticateModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authenticateModel.Email = user.Email;
                authenticateModel.Username = user.UserName;
                var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                authenticateModel.Roles = rolesList.ToList();
                return authenticateModel;
            }

            authenticateModel.IsAuthenticated = false;
            authenticateModel.Message = $"Incorrect Credentials for user {user.Email}";
            return authenticateModel;
        }


        private async Task<JwtSecurityToken> CreateJwtToken(Employee user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }.Union(userClaims).Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }
    }
}
