using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphSimulator.Helpers
{
    public class Helper
    {
        public static Point CalActualPointForNewConnection(Point start, Point dest)
        {
            var vector = dest - start;
            //vector.Normalize();
            var a = vector.Y / vector.X;
            var b = dest.Y - a * dest.X;
            var c1 = dest.X;
            var c2 = dest.Y;

            var (x1, x2) = SolveQuadratic(a * a + 1, 2 * (a * (b - c2) - c1), Math.Pow(c1, 2) + Math.Pow(b - c2, 2) - Math.Pow(Node.Radius + 0.5, 2));

            if (x1 is null || x2 is null)
                return new Point(0, 0);

            var y1 = a * x1.Value + b;
            var y2 = a * x2.Value + b;

            var p1 = new Point(x1.Value, y1);
            var p2 = new Point(x2.Value, y2);
            var vec1 = dest - p1;
            var vec2 = dest - p2;

            vector.Normalize();
            vec1.Normalize();
            vec2.Normalize();
            if (vector == vec1) return p1;
            else if (vector == vec2) return p2;

            return new Point(0, 0);
        }

        public static (double? x1, double? x2) SolveQuadratic(double a, double b, double c)
        {
            var d = b * b - 4 * a * c;
            double x1, x2;
            if (d == 0)
            {
                x1 = -b / (2.0 * a);
                x2 = x1;
                return (x1, x2);
            }
            else if (d < 0)
            {
                return (null, null);
            }

            x1 = (-b + Math.Sqrt(d)) / (2 * a);
            x2 = (-b - Math.Sqrt(d)) / (2 * a);

            return (x1, x2);
        }
    }
}
