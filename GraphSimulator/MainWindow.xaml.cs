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
        private bool _isDirectedGraph = false;
        private int _numberOfNode = 0;
        private Connection _newConnection;
        private Node _startNode, _destNode;
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
                if (!Helper.CreateNode(_numberOfNode, e.GetPosition(GraphContainer), out var newNode))
                {
                    MessageBox.Show("Ten nodes are enough for a good simulation :)");
                    return;
                }

                GraphContainer.Children.Add(newNode);
                _numberOfNode++;
                _operationStack.Push((Operation.ADD, new List<UIElement>() { newNode }));
            }
            else
            {
                if (!GetNodeAtCurrentClickPosition(newPos, out var curNode) && _startNode is null)
                    return;

                if (_startNode is null)
                    _startNode = curNode;
                else
                {
                    if (curNode.Identifier == _startNode.Identifier) return;
                    if (curNode is null)
                    {
                        if (!Helper.CreateNode(_numberOfNode, e.GetPosition(GraphContainer), out curNode))
                        {
                            MessageBox.Show("Ten nodes are enough for a good simulation :)");
                            return;
                        }

                        GraphContainer.Children.Add(curNode);
                        _numberOfNode++;
                        _operationStack.Push((Operation.ADD, new List<UIElement>() { curNode }));
                    }
                    _destNode = curNode;

                    var newCon = Helper.CreateConnection(_isDirectedGraph, _startNode, _destNode);

                    new SubWindows.CostInputWindow
                    {
                        Ok = cost =>
                        {
                            newCon.Cost = cost;
                        }
                    }.ShowDialog();

                    if (newCon.Cost == -1)
                    {
                        newCon = null;
                        return;
                    }
                    else
                    {
                        GraphContainer.Children.Add(newCon);

                        Canvas.SetZIndex(newCon, -99);

                        _operationStack.Push((Operation.ADD, new List<UIElement>() { newCon }));
                    }

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
                .FirstOrDefault(node =>
                {
                    if (node is Node tempNode)
                    {
                        var d = Node.Diameter;
                        return clickPos.X >= (tempNode.X - d) && clickPos.Y >= (tempNode.Y - d) && clickPos.X <= (tempNode.X + d) && clickPos.Y <= (tempNode.Y + d);
                    }
                    return false;
                }) as Node;
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

        private void Button_New_Click(object sender, RoutedEventArgs e)
        {
            GraphContainer.Children.Clear();
        }

        private void ButtonAddConn_Unchecked(object sender, RoutedEventArgs e)
        {
            _isCreatingConnection = false;
        }
    }
}
