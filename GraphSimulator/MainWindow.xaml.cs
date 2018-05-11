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
        private Node _startNode, _destNode;
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

            if (!_isCreatingConnection)
            {
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
            else
            {
                if (!GetNodeAtCurrentClickPosition(newPos, out var curNode) && _startNode is null)
                    return;
                
                if (_startNode is null)
                    _startNode = curNode;
                else 
                {
                    if (curNode is null)
                    {
                        curNode = new Node
                        {
                            X = e.GetPosition(GraphContainer).X,
                            Y = e.GetPosition(GraphContainer).Y
                        };
                        Canvas.SetLeft(curNode, curNode.X - Node.Radius);
                        Canvas.SetTop(curNode, curNode.Y - Node.Radius);
                        GraphContainer.Children.Add(curNode);
                        _operationStack.Push((Operation.ADD, new List<UIElement>() { curNode }));
                    }
                    _destNode = curNode;

                    var actualDest = Helper.CalActualPointForNewConnection(_startNode.Centre, _destNode.Centre);

                    var newCon = new Connection()
                    {
                        X1 = _startNode.X,
                        Y1 = _startNode.Y,
                        X2 = actualDest.X,
                        Y2 = actualDest.Y
                    };
                                        
                    GraphContainer.Children.Add(newCon);

                    Canvas.SetZIndex(newCon, -99);

                    _operationStack.Push((Operation.ADD, new List<UIElement>() { newCon }));
                    _startNode.IsSelected = _destNode.IsSelected = false;
                    _startNode = _destNode = null;
                }
            }
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
            foreach (var con in GraphContainer.Children.Cast<Connection>())
            {
                con.IsSelected = true;
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
