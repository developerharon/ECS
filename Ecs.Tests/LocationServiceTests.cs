using Ecs.Models.ApiServices;
using Xunit;

namespace Ecs.Tests
{
    public class LocationServiceTests
    {
        private static readonly LocationService _locationService = new LocationService();
        private static readonly Point[] _polygon = { new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(0, 10) };

        [Fact]
        public void Can_Validate_Point_Is_Inside_Polygon()
        {
            Point p = new Point(5, 5);

            bool isPointInside = _locationService.IsInside(_polygon, _polygon.Length, p);

            Assert.True(isPointInside);
        }

        [Fact]
        public void Can_Validate_Point_Is_Ouside_Polygon()
        {
            Point p = new Point(20, 20);

            bool isPointInside = _locationService.IsInside(_polygon, _polygon.Length, p);

            Assert.False(isPointInside);
        }

        [Fact]
        public void Can_Validate_Points_On_Polygon_Edges()
        {
            Point point1 = new Point(10, 10);
            Point point2 = new Point(10, 5);

            bool isPoint1OnTheEdge = _locationService.IsInside(_polygon, _polygon.Length, point1);
            bool isPoint2OnTheEdge = _locationService.IsInside(_polygon, _polygon.Length, point2);

            Assert.True(isPoint1OnTheEdge);
            Assert.True(isPoint2OnTheEdge);
        }
    }
}
