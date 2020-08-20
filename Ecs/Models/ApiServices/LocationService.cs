using System;

namespace Ecs.Models.ApiServices
{
    public class LocationService
    {
        private const int INF = 10000;
        private const int IsPointsColinear = 0;
        private const int IsPointsClockwise = 1;
        private const int IsPointsCounterClockwise = 2;

        private bool OnSegment(Point p, Point q, Point r)
        {
            if (q.PointX <= Math.Max(p.PointX, r.PointX) && q.PointX >= Math.Min(p.PointX, r.PointX) && q.PointY <= Math.Max(p.PointY, r.PointY) && q.PointY >= Math.Min(p.PointY, r.PointY))
            {
                return true;
            }
            return false;
        }

        private int Orientation(Point p, Point q, Point r)
        {
            int val = (q.PointY - p.PointY) * (r.PointX - q.PointX) - (q.PointX - p.PointX) * (r.PointY - q.PointY);

            if (val == 0)
                return IsPointsColinear;

            return (val > 0) ? IsPointsClockwise : IsPointsCounterClockwise;
        }

        public bool DoPointsIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            int orientation1 = Orientation(p1, q1, p2);
            int orientation2 = Orientation(p1, q1, q2);
            int orientation3 = Orientation(p2, q2, p1);
            int orientation4 = Orientation(p2, q2, q1);

            if (orientation1 != orientation2 && orientation3 != orientation4)
                return true;

            if (orientation1 == IsPointsColinear && OnSegment(p1, p2, q1))
                return true;

            if (orientation2 == IsPointsColinear && OnSegment(p1, q2, q1))
                return true;

            if (orientation3 == IsPointsColinear && OnSegment(p2, p1, q2))
                return true;

            if (orientation4 == IsPointsColinear && OnSegment(p2, q1, q2))
                return true;

            return false;
        }

        public bool IsInside(Point[] polygon, int n, Point p)
        {
            if (n < 3)
                return false;

            Point extreme = new Point(INF, p.PointY);

            int count = 0, i = 0;

            do
            {
                int next = (i + 1) % n;

                if (DoPointsIntersect(polygon[i], polygon[next], p, extreme))
                {
                    if (Orientation(polygon[i], p, polygon[next]) == IsPointsColinear)
                    {
                        return OnSegment(polygon[i], p, polygon[next]);
                    }

                    count++;
                }
                i = next;
            } while (i != 0);

            return (count % 2 == 1);
        }
    }
}
