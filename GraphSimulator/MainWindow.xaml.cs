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
        private bool _isCreatingConnection = false;
        private Node _startNode;
        private Connection _newConnection;
        private Stack<(Operation operation, IEnumerable<UIElement> controls)> _operationStack = new Stack<(Operation, IEnumerable<UIElement>)>();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (x, y) => Keyboard.Focus(GraphContainer);
        }

        private void GraphContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var newPos = e.GetPosition(GraphContainer);

            if (IsOverlapExistedNode(newPos))
                return;

            var node = new Node
            {
                X = e.GetPosition(GraphContainer).X,
                Y = e.GetPosition(GraphContainer).Y
            };
            Canvas.SetLeft(node, node.X - Node.Radius);
            Canvas.SetTop(node, node.Y - Node.Radius);

            GraphContainer.Children.Add(node);
            _operationStack.Push((Operation.ADD, new List<UIElement>() { node }));
        }

        private bool IsOverlapExistedNode(Point clickPos)
        {
            return GraphContainer.Children.Cast<UIElement>()
                .Any(node => {
                    if (node is Node tempNode)
                    {
                        var d = Node.Diameter;
                        return clickPos.X >= (tempNode.X - d) && clickPos.Y >= (tempNode.Y - d) && clickPos.X <= (tempNode.X + d) && clickPos.Y <= (tempNode.Y + d);
                    }
                    return false;
                });
        }

        private bool GetNodeAtCurrentClickPosition(Point clickPos, out Node n)
        {
            n = GraphContainer.Children.Cast<UIElement>()
                .Where(node => {
                    if (node is Node tempNode)
                    {
                        var d = Node.Diameter;
                        return clickPos.X >= (tempNode.X - d) && clickPos.Y >= (tempNode.Y - d) && clickPos.X <= (tempNode.X + d) && clickPos.Y <= (tempNode.Y + d);
                    }
                    return false;
                })
                .Cast<Node>()
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

        private void GraphContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isCreatingConnection)
            {
                var curPoint = e.GetPosition(GraphContainer);
                if (curPoint.X < Node.Radius) curPoint.X = Node.Radius;
                else if (curPoint.X > GraphContainer.ActualWidth - Node.Radius) curPoint.X = GraphContainer.ActualWidth - Node.Radius;

                if (curPoint.Y < Node.Radius) curPoint.Y = Node.Radius;
                else if (curPoint.Y > GraphContainer.ActualHeight - Node.Radius) curPoint.Y = GraphContainer.ActualHeight - Node.Radius;

                if (GetNodeAtCurrentClickPosition(curPoint, out var desNode))
                {
                    _newConnection.DestinationNode = desNode;
                }
                else
                {
                    _newConnection.DestinationNode.X = curPoint.X;
                    _newConnection.DestinationNode.Y = curPoint.Y;
                }
            }
        }

        private void GraphContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isCreatingConnection)
            {
                _isCreatingConnection = false;
                if (_newConnection is null) return;
                GraphContainer.Children.Add(_newConnection);
                _newConnection = null;
            }
        }

        private void ButtonAddConn_Checked(object sender, RoutedEventArgs e)
        {
            _isCreatingConnection = true;
        }

        private void ButtonAddConn_Unchecked(object sender, RoutedEventArgs e)
        {
            _isCreatingConnection = false;
        }
    }
}
