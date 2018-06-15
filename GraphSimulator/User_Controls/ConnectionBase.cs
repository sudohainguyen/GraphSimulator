using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSimulator.Helpers;

namespace GraphSimulator.User_Controls
{
    public abstract class ConnectionBase : Shape, INotifyPropertyChanged
    {
        protected PathGeometry pathgeo;
        protected PathFigure pathfigLine;
        protected PolyLineSegment polysegLine;

        private PathFigure pathfigHead1;
        private PolyLineSegment polysegHead1;
        private PathFigure pathfigHead2;
        private PolyLineSegment polysegHead2;

        private static readonly Brush SELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(183, 61, 61));
        private static readonly Brush UNSELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        #region Properties
        public char StartNode { get; set; }
        public char DestNode { get; set; }
        public string Identity => $"{StartNode}{DestNode}";

        public int Cost { get; set; } = 0;
        public int ReverseCost { get; set; } = -1;
        public bool IsTwoWay => ArrowDirection == Direction.TwoWay || ArrowDirection == Direction.None;

        public ConnectionStatus ConnectionStatus
        {
            get { return (ConnectionStatus)GetValue(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(ConnectionStatus), typeof(ConnectionBase),
                new FrameworkPropertyMetadata(ConnectionStatus.None, FrameworkPropertyMetadataOptions.AffectsMeasure));



        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(ConnectionBase),
                new FrameworkPropertyMetadata(45.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(ConnectionBase),
                new FrameworkPropertyMetadata(8.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowDirection",
                typeof(Direction), typeof(ConnectionBase),
                new FrameworkPropertyMetadata(Direction.OneWay,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public Direction ArrowDirection
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (Direction)GetValue(ArrowEndsProperty); }
        }

        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty =
            DependencyProperty.Register("IsArrowClosed",
                typeof(bool), typeof(ConnectionBase),
                new FrameworkPropertyMetadata(true,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }

        #endregion

        public ConnectionBase(bool isDirectedGraph, Node startNode, Node destNode) : this()
        {
            ArrowDirection = isDirectedGraph ? Direction.OneWay : Direction.None;
            StartNode = startNode.Identity;
            DestNode = destNode.Identity;
        }

        public ConnectionBase()
        {
            pathgeo = new PathGeometry();

            pathfigLine = new PathFigure();
            polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);

            pathfigHead1 = new PathFigure();
            polysegHead1 = new PolyLineSegment();
            pathfigHead1.Segments.Add(polysegHead1);

            pathfigHead2 = new PathFigure();
            polysegHead2 = new PolyLineSegment();
            pathfigHead2.Segments.Add(polysegHead2);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var count = polysegLine.Points.Count;
                if (count > 0)
                {
                    if ((ArrowDirection & Direction.OneWay) == Direction.OneWay)
                    {
                        var pt1 = count == 1 ? pathfigLine.StartPoint :
                                                    polysegLine.Points[count - 2];
                        var pt2 = polysegLine.Points[count - 1];
                        pathgeo.Figures.Add(CalculateArrow(pathfigHead2, pt1, pt2));
                    }
                    if ((ArrowDirection & Direction.OneWayReserved) == Direction.OneWayReserved)
                    {
                        var pt1 = pathfigLine.StartPoint;
                        var pt2 = polysegLine.Points[0];
                        pathgeo.Figures.Add(CalculateArrow(pathfigHead1, pt2, pt1));
                    }
                }
                return pathgeo;
            }
        }

        private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            var matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            var arrowPoint = Helper.CalActualPointForNewConnection(pt1, pt2);

            var polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();
            matx.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = arrowPoint + vect * matx;
            polyseg.Points.Add(arrowPoint);

            matx.Rotate(-ArrowAngle);
            polyseg.Points.Add(arrowPoint + vect * matx);
            pathfig.IsClosed = IsArrowClosed;

            return pathfig;
        }

        public bool Equals(ConnectionBase conn)
        {
            return StartNode.Equals(conn.StartNode) && DestNode.Equals(conn.DestNode);
        }

        public bool HasTwoVertices(char node1, char node2)
        {
            return StartNode.Equals(node1) && DestNode.Equals(node2)
                || StartNode.Equals(node2) && DestNode.Equals(node1) && ArrowDirection == Direction.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
