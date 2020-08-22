using Microsoft.AspNetCore.Identity;
using Ecs.Models.ApiModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Xamarin.Essentials;

namespace Ecs.Models.ApiServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<AuthenticationModel> LoginAsync(TokenRequestModel model)
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
                authenticationModel.Email = user.Email;
                authenticationModel.Name = user.Name;
                return authenticationModel;
            }
            authenticationModel.IsAuthenticated = false;
            authenticationModel.Message = $"Incorrect Credentials for user {user.Email}";
            return authenticationModel;
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

        private bool IsEmployeeOnPremises(Location clockLocation)
        {
            LocationService locationService = new LocationService();
            Point[] premises = { new Point(3818628, 353046193), new Point(3818628, 353046193), new Point(3818628, 353046193), new Point(3818628, 353046193) };
            int latitude = (int)clockLocation.Latitude * 10000000;
            int longitude = (int)clockLocation.Longitude * 10000000;
            Point pointLocation = new Point(latitude, longitude);
            bool isEmployeeInsidePremises = locationService.IsInside(premises, premises.Length, pointLocation);
            return isEmployeeInsidePremises;
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