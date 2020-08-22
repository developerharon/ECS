using Ecs.Models.ApiServices;
using Xunit;

namespace Ecs.Tests
{
    public class UserServiceTests
    {
        private readonly LocationService _locationService = new LocationService();
        Point[] _premises = { new Point(3818628, 353046193), new Point(3818628, 353046193), new Point(3818628, 353046193), new Point(3818628, 353046193) };

        [Fact]
        public void Operator_Precedence_Causes_Validation_To_Fail()
        {
            UserService userService = new UserService(null, null);

            bool isUserOnPremises = userService.IsEmployeeOnPremises(0.38186283, 35.3046193);

            Assert.True(isUserOnPremises);
        }

        [Fact]
        public void Can_Validate_Users_Location()
        {
            int latitude = (int)(0.38186283 * 10000000);
            int longitude = (int)(35.3046193 * 10000000);
            Point usersLocation = new Point(latitude, longitude);

            bool isUserOnPremises = _locationService.IsInside(_premises, _premises.Length, usersLocation);

            Assert.True(isUserOnPremises);
        }
    }
}
