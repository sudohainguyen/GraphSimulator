using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphSimulator.Helpers
{
    public class Helper
    {
        public static Point CalActualPointForNewConnection(Point start, Point dest)
        {
            var vector = dest - start;

            double a, b;
            double? x1, x2, y1, y2;
            var c1 = dest.X;
            var c2 = dest.Y;
            if (vector.X == 0)
            {
                x1 = x2 = dest.X;
                (y1, y2) = SolveQuadratic(1, -2 * c2, Math.Pow(c2, 2) - Math.Pow(Node.Radius, 2) + Math.Pow(x1.Value - c1, 2));
                if (y1 is null || y2 is null) return new Point(0, 0);
            }
            else
            {
                a = vector.Y / vector.X;
                b = dest.Y - a * dest.X;
                
                (x1, x2) = SolveQuadratic(a * a + 1, 2 * (a * (b - c2) - c1), Math.Pow(c1, 2) + Math.Pow(b - c2, 2) - Math.Pow(Node.Radius + 0.5, 2));
                if (x1 is null || x2 is null) return new Point(0, 0);
                y1 = a * x1.Value + b;
                y2 = a * x2.Value + b;
            }

            return ChooseSuitablePoint(new Point(x1.Value, y1.Value), new Point(x2.Value, y2.Value), dest, vector);
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

        private static Point ChooseSuitablePoint(Point p1, Point p2, Point dest, Vector mainVector)
        {
            var epsilon = 1e-10;

            var vec1 = dest - p1;
            var vec2 = dest - p2;

            mainVector.Normalize();
            vec1.Normalize();
            vec2.Normalize();

            var vv1 = mainVector + vec1;
            var vv2 = mainVector + vec2;
            if (vv1.Length < epsilon) return p2;
            if (vv2.Length < epsilon) return p1;

            return new Point(0, 0);
        }

        public static bool CreateNode(int curNumberOfNodeInCanvas, Point clickPos, out Node newNode)
        {
            if (curNumberOfNodeInCanvas == 10)
            {
                newNode = null;
                return false;
            }

            newNode = new Node
            {
                X = clickPos.X,
                Y = clickPos.Y,
                Identity = (char)(curNumberOfNodeInCanvas + 65)
            };

            return true;
        }

        //public static Connection CreateConnection(bool isDirectedGraph, Node startNode, Node destNode)
        //{
        //    var newCon = new Connection
        //    {
        //        X1 = startNode.X,
        //        Y1 = startNode.Y,
        //        X2 = destNode.X,
        //        Y2 = destNode.Y,
        //        ArrowDirection = isDirectedGraph ? Direction.OneWay : Direction.None,
        //        StartNode = startNode.Identity,
        //        DestNode = destNode.Identity
        //    };

        //    //TODO: when add to existed one way connection (check evernote for more) ???

        //    return newCon;
        //}

        public static Dictionary<char, Route> InitSetForDijsktra(char startNode, IEnumerable<char> nodes)
        {
            var dict = new Dictionary<char, Route>();
            foreach (var node in nodes)
            {
                dict.Add(node, new Route { DestNode = node } );

            }
            dict[startNode].RouteCost = 0;
            return dict;
        }

        public static Dictionary<char, (char PrevNode, int Cost)> InitSetForPrim(char startNode, IEnumerable<char> nodes)
        {
            var dict = new Dictionary<char, (char prevNode, int cost)>();
            foreach (var node in nodes)
            {
                dict.Add(node, ('-', int.MaxValue));
            }
            dict[startNode] = (startNode, 0);
            return dict;
        }

        public static void ToUndirectedGraph()
        {
            foreach (var item in RouteEngine.Instance.Connections)
            {
                item.ArrowDirection = Direction.None;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static Point CalPointForTextBlockCost(Point startNode, Point destNode)
        {
            var vec = destNode - startNode;
            vec /= 3;
            vec *= 2;

            var mat = new Matrix();
            mat.Rotate(5);
            return startNode + vec * mat;
        }
    }
}
