namespace Ecs.Models.ApiServices
{
    public class Point
    {
        public int PointX { get; set; }
        public int PointY { get; set; }

        public Point(int x, int y)
        {
            this.PointX = x;
            this.PointY = y;
        }
    }
}
