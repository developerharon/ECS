using Microsoft.AspNetCore.Identity;
using Ecs.Models;
using Microsoft.Extensions.Options;
using ECSApi.Models.ApiModels;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Internal;
using Xamarin.Essentials;

namespace ECSApi.Models
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model)
        {
            var authenticationModel = new AuthenticationModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"No Accounts Registered with {model.Email}";
                return authenticationModel;
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authenticationModel.IsAuthenticated = true;
                JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authenticationModel.Email = user.Email;
                authenticationModel.Name = user.Name;


                if (user.RefreshTokens.Any(a => a.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                    authenticationModel.RefreshToken = activeRefreshToken.Token;
                    authenticationModel.RefreshTokenExpiration = activeRefreshToken.Expires;
                }
                else
                {
                    var refreshToken = CreateRefreshToken();
                    authenticationModel.RefreshToken = refreshToken.Token;
                    authenticationModel.RefreshTokenExpiration = refreshToken.Expires;
                    user.RefreshTokens.Add(refreshToken);
                    _context.Update(user);
                    _context.SaveChanges();
                }
                return authenticationModel;
            }
            authenticationModel.IsAuthenticated = false;
            authenticationModel.Message = $"Incorrect Credentials for user {user.Email}";
            return authenticationModel;
        }

        public async Task<AuthenticationModel> RefreshTokenAsync(string token)
        {
            var authenticationModel = new AuthenticationModel();
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"Token did not match any users.";
                return authenticationModel;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"Token Not Active";
                return authenticationModel;
            }

            // Revoke the current refresh token
            refreshToken.Revoked = DateTime.UtcNow;

            // Generate new Refresh Token and save to database
            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            _context.SaveChanges();

            // Generate new JWT
            authenticationModel.IsAuthenticated = true;
            JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
            authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authenticationModel.Email = user.Email;
            authenticationModel.Name = user.Name;
            authenticationModel.RefreshToken = newRefreshToken.Token;
            authenticationModel.RefreshTokenExpiration = newRefreshToken.Expires;
            return authenticationModel;
        }

        public bool RevokeToken(string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // Return false if no user is found with the token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // Return false if token is not active
            if (!refreshToken.IsActive) return false;

            // Revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            _context.Update(user);
            _context.SaveChanges();

            return true;
        }

        public async Task<TimestampResponseModel> ClockInAsync(TimestampModel model)
        {
            var response = new TimestampResponseModel();

            // Use the email to get the user object from the database
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                response.Succeeded = false;
                response.Message = "User Not Found";
                return response;
            }

            // Create a timestamp object and add it to the list associated with the user's
            var timestamp = new Timestamp
            {
                In = model.ClockTime,
                InWhileOnPremises = IsEmployeeOnPremises(model.ClockLocation),
                IsActive = true
            };
            user.Timestamps.Add(timestamp);
            _context.Update(user);
            _context.SaveChanges();

            // Generate a response for the user
            response.Succeeded = true;
            response.Message = "Clocked in successfully";
            return response;
        }

        public async Task<TimestampResponseModel> ClockOutAsync(TimestampModel model)
        {
            var response = new TimestampResponseModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                response.Succeeded = false;
                response.Message = "No User Found";
                return response;
            }

            // Find an active timestamp
            var timestamp = user.Timestamps.Single(timestamp => timestamp.IsActive);

            if (timestamp == null)
            {
                response.Succeeded = false;
                response.Message = "No Active Timestamp";
                return response;
            }

            timestamp.IsActive = false;
            timestamp.Out = model.ClockTime;
            timestamp.OutWhileOnPremises = IsEmployeeOnPremises(model.ClockLocation);
            _context.Update(user);
            _context.SaveChanges();

            // Generate a response for the user
            response.Succeeded = true;
            response.Message = "Clocked out successfully";
            return response;
        }

        public async Task<TimestampResponseModel> GetActiveClockAsync(string email)
        {
            var response = new TimestampResponseModel();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                response.Succeeded = false;
                response.IsClockActive = false;
                response.Message = "No User Found";
                return response;
            }

            var anyActiveTimestamp = user.Timestamps.Any(timestamp => timestamp.IsActive);

            if (anyActiveTimestamp)
            {
                response.Succeeded = true;
                response.IsClockActive = true;
                response.Message = "Active clock found";
                return response;
            }
            else
            {
                response.Succeeded = true;
                response.IsClockActive = false;
                response.Message = "No Active Clock Found";
                return response;
            }
        }

        public async Task<List<Timestamp>> GetAllClocksAsync(string email)
        {
            var response = new List<Timestamp>();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }
            else
            {
                response = user.Timestamps;
                return response;
            }
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
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
                    Created = DateTime.Now
                };
            }
        }

        private bool IsEmployeeOnPremises(Location clockLocation)
        {
            // Location Logic here
            return true;
        }

        public async Task<string> GetProfilePicture(string email)
        {
            if (email == null) return null;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return null;

            if (user.ProfilePicture == null) return null;

            return user.ProfilePicture;
        }
    }
}
