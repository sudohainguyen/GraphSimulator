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
        private static Brush SELECTED_STROKE_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush UNSELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush SELECTED_IDENTIFIER_BRUSH = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        private bool _isSelected = false;
        private Brush _fillBrush = UNSELECTED_FILL_BRUSH;
        private Brush _strokeBrush = UNSELECTED_STROKE_BRUSH;
        private Brush _identifierBrush = UNSELECTED_IDENTIFIER_BRUSH;

        public double X { get; set; }
        public double Y { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                FillBrush = _isSelected
                    ? SELECTED_FILL_BRUSH
                    : UNSELECTED_FILL_BRUSH;
                StrokeBrush = _isSelected
                    ? SELECTED_STROKE_BRUSH
                    : UNSELECTED_STROKE_BRUSH;
                IdentifierBrush = _isSelected
                    ? SELECTED_IDENTIFIER_BRUSH
                    : UNSELECTED_IDENTIFIER_BRUSH;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public char Identifier { get; set; } = 'A';

        public Brush FillBrush
        {
            get =>_fillBrush;
            private set
            {
                _fillBrush = value;
                OnPropertyChanged(nameof(FillBrush));
            }
        }

        public int Diameter => 40;

        public Brush StrokeBrush
        {
            get => _strokeBrush;
            private set
            {
                _strokeBrush = value;
                OnPropertyChanged(nameof(StrokeBrush));
            }
        }
        
        public Brush IdentifierBrush
        {
            get => _identifierBrush;
            private set
            {
                _identifierBrush = value;
                OnPropertyChanged(nameof(IdentifierBrush));
            }
        }

        public Node()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsSelected = !IsSelected;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
