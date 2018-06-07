using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using GraphSimulator.Helpers;

namespace GraphSimulator.User_Controls
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        private static Brush UNSELECTED_FILL_BRUSH = new SolidColorBrush(Color.FromRgb(152, 198, 234));
        private static Brush SELECTED_FILL_BRUSH = new SolidColorBrush(Color.FromRgb(193, 87, 87));
        private static Brush UNSELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 101, 189));
        private static Brush SELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(183, 61, 61));
        private static Brush UNSELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush SELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        private bool _inDrag = false;
        private Point _anchorPoint;

        public double X { get; set; }
        public double Y { get; set; }
        public Point Centre => new Point(X, Y);

        public char Identity { get; set; } = 'A';

        public static int Diameter => 40;
        public static int Radius => Diameter / 2;

        public Node()
        {
            InitializeComponent();
        }


        public bool IsSelected => NodeStatus == NodeStatus.IsSelected;
        public bool Processed => NodeStatus == NodeStatus.Processed;


        public NodeStatus NodeStatus
        {
            get { return (NodeStatus)GetValue(NodeStatusProperty); }
            set { SetValue(NodeStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NodeStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeStatusProperty =
            DependencyProperty.Register("NodeStatus", typeof(NodeStatus), typeof(Node),
                new FrameworkPropertyMetadata(NodeStatus.None, FrameworkPropertyMetadataOptions.AffectsMeasure));


        public int RouteCost
        {
            get { return (int)GetValue(RouteCostProperty); }
            set { SetValue(RouteCostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RouteCost.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RouteCostProperty =
            DependencyProperty.Register("RouteCost", typeof(int), typeof(Node), 
                new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure));



        public Brush StrokeBrush
        {
            get { return (Brush)GetValue(StrokeBrushProperty); }
            set { SetValue(StrokeBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeBrushProperty =
            DependencyProperty.Register("StrokeBrush", typeof(Brush), typeof(Node),
                new FrameworkPropertyMetadata(UNSELECTED_STROKE_BRUSH, FrameworkPropertyMetadataOptions.AffectsMeasure));


        public Brush IdentifierBrush
        {
            get { return (Brush)GetValue(IdentifierBrushProperty); }
            set { SetValue(IdentifierBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IdentifierBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentifierBrushProperty =
            DependencyProperty.Register("IdentifierBrush", typeof(Brush), typeof(Node),
                new FrameworkPropertyMetadata(UNSELECTED_IDENTIFIER_BRUSH, FrameworkPropertyMetadataOptions.AffectsMeasure));



        public Brush FillBrush
        {
            get { return (Brush)GetValue(FillBrushProperty); }
            set { SetValue(FillBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillBrushProperty =
            DependencyProperty.Register("FillBrush", typeof(Brush), typeof(Node),
                new FrameworkPropertyMetadata(UNSELECTED_FILL_BRUSH, FrameworkPropertyMetadataOptions.AffectsMeasure));



        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSelected)
                NodeStatus = NodeStatus.None;
            else
                NodeStatus = NodeStatus.IsSelected;
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {

            if (_inDrag)
            {
                this.Cursor = Cursors.ScrollAll;

                var parent = this.Parent as Panel;
                var curPoint = e.GetPosition(parent);

                if (curPoint.X < Radius) curPoint.X = Radius;
                else if (curPoint.X > parent.ActualWidth - Radius) curPoint.X = parent.ActualWidth - Radius;

                if (curPoint.Y < Radius) curPoint.Y = Radius;
                else if (curPoint.Y > parent.ActualHeight - Radius) curPoint.Y = parent.ActualHeight - Radius;

                Canvas.SetLeft(this, Canvas.GetLeft(this) + (curPoint.X - _anchorPoint.X));
                Canvas.SetTop(this, Canvas.GetTop(this) + (curPoint.Y - _anchorPoint.Y));
                _anchorPoint = curPoint;

                X = Canvas.GetLeft(this) + Node.Radius;
                Y = Canvas.GetTop(this) + Node.Radius;

                foreach (var con in RouteEngine.Instance.Connections)
                {
                    if (con.StartNode == this.Identity)
                    {
                        con.X1 = X;
                        con.Y1 = Y;
                    }
                    else if (con.DestNode == this.Identity)
                    {
                        con.X2 = X;
                        con.Y2 = Y;
                    }
                    var pointForTblCost = Helper.CalPointForTextBlockCost(new Point(con.X1, con.Y1), new Point(con.X2, con.Y2));
                    Canvas.SetLeft(con.TextBlockCost, pointForTblCost.X - 10);
                    Canvas.SetTop(con.TextBlockCost, pointForTblCost.Y - 10);
                }
                e.Handled = true;
            }
        }

        private void Node_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_inDrag)
            {
                _anchorPoint = e.GetPosition(this.Parent as Panel);
                CaptureMouse();
                _inDrag = true;
                e.Handled = true;
            }
        }

        private void Node_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_inDrag)
            {
                ReleaseMouseCapture();
                _inDrag = false;
                e.Handled = true;
            }
        }
    }
}
