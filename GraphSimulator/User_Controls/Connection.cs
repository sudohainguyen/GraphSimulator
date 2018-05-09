using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphSimulator.User_Controls
{
    public class Connection : Shape, INotifyPropertyChanged
    {
        private const double HEAD_WIDTH = 5;
        private const double HEAD_HEIGHT = 10;
        private static Brush SELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(183, 61, 61));
        private static Brush UNSELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));


        private Node _destinationPoint = new Node { X = 100, Y = 200 };
        private bool _isSelected;

        public bool IsDirected { get; set; } = true;
        public Node StartNode { get; set; } = new Node { X = 50, Y = 50 };
        public Node DestinationNode
        {
            get => _destinationPoint;
            set
            {
                _destinationPoint = value;
                OnPropertyChanged(nameof(DestinationNode));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Stroke = _isSelected
                    ? SELECTED_STROKE_BRUSH
                    : UNSELECTED_STROKE_BRUSH;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var geometry = new StreamGeometry
                {
                    FillRule = FillRule.EvenOdd
                };

                using (var context = geometry.Open())
                {
                    InternalDrawArrowGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }

        private void InternalDrawArrowGeometry(StreamGeometryContext context)
        {
            var X1 = StartNode.X;
            var Y1 = StartNode.Y;
            var X2 = DestinationNode.X;
            var Y2 = DestinationNode.Y;

            var alpha = Math.Atan2(Y2 - Y1, X2 - X1);
            var sin_a = Math.Sin(alpha);
            var cos_a = Math.Cos(alpha);

            X2 -= Node.Radius * cos_a;
            Y2 -= Node.Radius * sin_a;

            var pt1 = new Point(X1, Y1);
            var pt2 = new Point(X2, Y2);

            var pt3 = new Point(
                X2 - (HEAD_HEIGHT * cos_a - HEAD_WIDTH * sin_a),
                Y2 - (HEAD_HEIGHT * sin_a + HEAD_WIDTH * cos_a));

            var pt4 = new Point(
                X2 - (HEAD_HEIGHT * cos_a + HEAD_WIDTH * sin_a),
                Y2 + (HEAD_WIDTH * cos_a - HEAD_HEIGHT * sin_a));

            context.BeginFigure(pt1, true, false);
            context.LineTo(pt2, true, true);
            if (IsDirected)
            {
                context.LineTo(pt3, true, true);
                context.LineTo(pt2, true, true);
                context.LineTo(pt4, true, true);
                context.LineTo(pt2, true, true);
            }
            else
            {
                context.LineTo(pt1, true, true);
                context.LineTo(pt2, true, true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
