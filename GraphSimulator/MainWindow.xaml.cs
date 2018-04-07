using GraphSimulator.User_Controls;
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

namespace GraphSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GraphContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var node = new Node
            {
                X = e.GetPosition(GraphContainer).X,
                Y = e.GetPosition(GraphContainer).Y
            };

            Canvas.SetLeft(node, node.X - node.Diameter / 2);
            Canvas.SetTop(node, node.Y - node.Diameter / 2);

            GraphContainer.Children.Add(node);
        }
    }
}
