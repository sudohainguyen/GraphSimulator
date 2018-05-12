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

namespace GraphSimulator.User_Controls
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl, INotifyPropertyChanged
    {
        private static Brush UNSELECTED_FILL_BRUSH = new SolidColorBrush(Color.FromRgb(152, 198, 234));
        private static Brush SELECTED_FILL_BRUSH = new SolidColorBrush(Color.FromRgb(193, 87, 87));
        private static Brush UNSELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 101, 189));
        private static Brush SELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(183, 61, 61));
        private static Brush UNSELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush SELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        //private bool _isSelected = false;
        //private Brush _fillBrush = UNSELECTED_FILL_BRUSH;
        //private Brush _strokeBrush = UNSELECTED_STROKE_BRUSH;
        //private Brush _identifierBrush = UNSELECTED_IDENTIFIER_BRUSH;
        private bool _inDrag = false;
        private Point _anchorPoint;

        public double X { get; set; }
        public double Y { get; set; }

        public Point Centre => new Point(X, Y);

        public char Identifier { get; set; } = 'A';

        public static int Diameter => 40;
        public static int Radius => Diameter / 2;

        //public bool IsSelected
        //{
        //    get => _isSelected;
        //    set
        //    {
        //        _isSelected = value;
        //        FillBrush = _isSelected
        //            ? SELECTED_FILL_BRUSH
        //            : UNSELECTED_FILL_BRUSH;
        //        StrokeBrush = _isSelected
        //            ? SELECTED_STROKE_BRUSH
        //            : UNSELECTED_STROKE_BRUSH;
        //        IdentifierBrush = _isSelected
        //            ? SELECTED_IDENTIFIER_BRUSH
        //            : UNSELECTED_IDENTIFIER_BRUSH;
        //        OnPropertyChanged(nameof(IsSelected));
        //    }
        //}

        //public Brush FillBrush
        //{
        //    get => _fillBrush;
        //    private set
        //    {
        //        _fillBrush = value;
        //        OnPropertyChanged(nameof(FillBrush));
        //    }
        //}

        //public Brush StrokeBrush
        //{
        //    get => _strokeBrush;
        //    private set
        //    {
        //        _strokeBrush = value;
        //        OnPropertyChanged(nameof(StrokeBrush));
        //    }
        //}

        //public Brush IdentifierBrush
        //{
        //    get => _identifierBrush;
        //    private set
        //    {
        //        _identifierBrush = value;
        //        OnPropertyChanged(nameof(IdentifierBrush));
        //    }
        //}

        public Node()
        {
            InitializeComponent();
        }


        public bool IsSelected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Node),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));


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
            IsSelected = !IsSelected; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {

            if (_inDrag)
            {
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
