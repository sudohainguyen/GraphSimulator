using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using GraphSimulator.Helpers;

namespace GraphSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Stack<(Operation operation, IEnumerable<UIElement> controls)> _operationStack = new Stack<(Operation, IEnumerable<UIElement>)>();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (x, y) => Keyboard.Focus(GraphContainer);
        }

        private void GraphContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var newPos = new Point(e.GetPosition(GraphContainer).X, e.GetPosition(GraphContainer).Y);

            if (IsOverlapExistedNode(newPos))
                return;

            //GetNodeAtCurrentClickPosition(e.GetPosition(GraphContainer), out Node n);

            var node = new Node
            {
                X = e.GetPosition(GraphContainer).X,
                Y = e.GetPosition(GraphContainer).Y
            };

            CanvasModify(node);

            GraphContainer.Children.Add(node);
            _operationStack.Push((Operation.ADD, new List<UIElement>() { node }));
        }

        private void CanvasModify(Node node)
        {
            Canvas.SetLeft(node, node.X - Node.Diameter / 2);
            Canvas.SetTop(node, node.Y - Node.Diameter / 2);
        }

        private bool IsOverlapExistedNode(Point clickPos)
        {
            return GraphContainer.Children.Cast<Node>()
                .Any(node => {
                    var d = Node.Diameter;
                    return clickPos.X >= (node.X - d) && clickPos.Y >= (node.Y - d) && clickPos.X <= (node.X + d) && clickPos.Y <= (node.Y + d);
                });
        }

        private bool GetNodeAtCurrentClickPosition(Point clickPos, out Node n)
        {
            n = GraphContainer.Children.Cast<Node>()
                .Where(node => {
                    var r = Node.Diameter / 2;
                    return clickPos.X >= (node.X - r) && clickPos.Y >= (node.Y - r) && clickPos.X <= (node.X + r) && clickPos.Y <= (node.Y + r);
                })
                .FirstOrDefault();
            return n is Node;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in GraphContainer.Children.Cast<Node>())
            {
                //node.FillBrush = new SolidColorBrush(Color.FromRgb(193, 87, 87));
            }
        }

        private void GraphContainer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var nodesToDel = GraphContainer.Children.Cast<Node>()
                    .Where(node => node.IsSelected).ToList();
                if (nodesToDel.Count() == 0)
                    return;
                _operationStack.Push((Operation.DELETE, nodesToDel));
                foreach (var item in nodesToDel)
                {
                    GraphContainer.Children.Remove(item);
                }
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (_operationStack.Count == 0)
                    return;
                var (operation, controls) = _operationStack.Pop();
                if (operation == Operation.ADD)
                {
                    foreach (var c in controls)
                    {
                        GraphContainer.Children.Remove(c);
                    }
                }
                else
                {
                    foreach (var c in controls)
                    {
                        GraphContainer.Children.Add(c);
                        //CanvasModify(c as Node);
                    }
                }
            }
        }
    }
}
