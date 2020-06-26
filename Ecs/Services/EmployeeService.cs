using Ecs.Contexts;
using Ecs.Entities;
using Ecs.Models;
using Ecs.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

        public async Task<IdentityResult> RegisterEmployeeAsync(RegisterModel model)
        {
            var employee = new Employee
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Department = model.Department,
                UserName = model.Username,
                Email = model.Email
            };

            IdentityResult result = await _userManager.CreateAsync(employee, model.Password);

            return result;
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

                if (user.RefreshTokens.Any(a => a.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                    authenticateModel.RefreshToken = activeRefreshToken.Token;
                    authenticateModel.RefreshTokenExpiration = activeRefreshToken.Expires;
                }
                else
                {
                    var refreshToken = CreateRefreshToken();
                    authenticateModel.RefreshToken = refreshToken.Token;
                    authenticateModel.RefreshTokenExpiration = refreshToken.Expires;
                    user.RefreshTokens.Add(refreshToken);
                    _context.Update(user);
                    _context.SaveChanges();
                }
                return authenticateModel;
            }

            authenticateModel.IsAuthenticated = false;
            authenticateModel.Message = $"Incorrect Credentials for user {user.Email}";
            return authenticateModel;
        }

        public async Task<AuthenticatedModel> RefreshTokenAsync(string token)
        {
            var authenticateModel = new AuthenticatedModel();
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
            {
                authenticateModel.IsAuthenticated = false;
                authenticateModel.Message = $"Token did not match any users.";
                return authenticateModel;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                authenticateModel.IsAuthenticated = false;
                authenticateModel.Message = $"Token Not Active";
                return authenticateModel;
            }

            // Revoke Current Refresh Token
            refreshToken.Revoked = DateTime.UtcNow;

            // Generate new Refresh Token and save to the database
            var newRefreshToken = CreateRefreshToken();
            _context.Update(user);
            _context.SaveChanges();

            // Generate new JWT
            authenticateModel.IsAuthenticated = true;
            JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
            authenticateModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authenticateModel.Email = user.Email;
            authenticateModel.Username = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            authenticateModel.Roles = rolesList.ToList();
            authenticateModel.RefreshToken = newRefreshToken.Token;
            authenticateModel.RefreshTokenExpiration = newRefreshToken.Expires;
            return authenticateModel;
        }

        public bool RevokeToken(string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // Return false if no user found with the token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // Return false if the token is not active
            if (!refreshToken.IsActive) return false;

            // Revoke token then save
            refreshToken.Revoked = DateTime.UtcNow;
            _context.Update(user);
            _context.SaveChanges();

            return true;
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

        private RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var generator = new RNGCryptoServiceProvider())
            {
                generator.GetBytes(randomNumber);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    Expires = DateTime.UtcNow.AddDays(10),
                    Created = DateTime.UtcNow
                };
            }
        }
    }
}
