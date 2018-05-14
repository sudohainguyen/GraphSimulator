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
using System.Collections.ObjectModel;

namespace GraphSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isCreatingConnection = false;
        private bool _isDirectedGraph = false;
        private bool _isRandomCost = false;
        private int _numberOfNode = 0;

        private RunningMode _runningMode = RunningMode.ShowTheResult;

        private Random _randCost = new Random();
        private Node _startNode;
        private Stack<(Operation operation, IEnumerable<UIElement> controls)> _operationStack = new Stack<(Operation, IEnumerable<UIElement>)>();
        private ObservableCollection<Route> _routes = new ObservableCollection<Route>();

        public ObservableCollection<Route> Routes
        {
            get => _routes;
            set
            {
                _routes = value;
                OnPropertyChanged(nameof(Routes));
            }
        }

        public bool IsCreatingConnection
        {
            get => _isCreatingConnection;
            set
            {
                _isCreatingConnection = value;
                OnPropertyChanged(nameof(IsCreatingConnection));
            }
        }

        public bool IsRandomCost
        {
            get => _isRandomCost;
            set
            {
                _isRandomCost = value;
                OnPropertyChanged(nameof(IsRandomCost));
            }
        }

        public bool I { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (x, y) => Keyboard.Focus(GraphContainer);
        }

        private void GraphContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var newPos = e.GetPosition(GraphContainer);

            if (!IsCreatingConnection)
            {
                if (IsOverlapExistedNode(newPos))
                    return;
                if (!Helper.CreateNode(ref _numberOfNode, e.GetPosition(GraphContainer), out var newNode))
                {
                    MessageBox.Show("Ten nodes are enough for a good simulation :)");
                    return;
                }

                GraphContainer.Children.Add(newNode);
                _operationStack.Push((Operation.ADD, new List<UIElement>() { newNode }));
            }
            else
            {
                if (!GetNodeAtCurrentClickPosition(newPos, out var targetNode) && _startNode is null)
                    return;

                if (_startNode is null)
                    _startNode = targetNode;
                else
                {
                    if (targetNode is null)
                    {
                        if (!Helper.CreateNode(ref _numberOfNode, e.GetPosition(GraphContainer), out targetNode))
                        {
                            MessageBox.Show("Ten nodes are enough for a good simulation :)");
                            return;
                        }

                        GraphContainer.Children.Add(targetNode);
                        _operationStack.Push((Operation.ADD, new List<UIElement>() { targetNode }));
                    }
                    else if (targetNode.Identity == _startNode.Identity) return;

                    var newCon = Helper.CreateConnection(_isDirectedGraph, _startNode, targetNode);

                    if (IsRandomCost)
                    {
                        newCon.Cost = _randCost.Next(1, 30);
                    }
                    else
                    {
                        new SubWindows.CostInputWindow
                        {
                            Ok = cost =>
                            {
                                newCon.Cost = cost;
                            }
                        }.ShowDialog();
                    }
                    if (newCon.Cost == -1)
                    {
                        newCon = null;
                    }
                    else
                    {
                        var vec = targetNode.Centre - _startNode.Centre;
                        vec /= 2;
                        var centralPoint = vec + _startNode.Centre;

                        var tblCost = new TextBlock
                        {
                            Text = newCon.Cost.ToString(),
                            FontSize = 17,
                            FontWeight = FontWeights.DemiBold
                        };

                        Canvas.SetZIndex(newCon, -99);

                        Canvas.SetLeft(tblCost, centralPoint.X);
                        Canvas.SetTop(tblCost, centralPoint.Y);

                        GraphContainer.Children.Add(newCon);
                        GraphContainer.Children.Add(tblCost);
                        _operationStack.Push((Operation.ADD, new List<UIElement>() { newCon, tblCost }));
                    }

                    _startNode.IsSelected = targetNode.IsSelected = false;
                    _startNode = null;
                }
            }
        }

        private bool IsOverlapExistedNode(Point clickPos)
        {
            return GraphContainer.Children.OfType<Node>()
                .FirstOrDefault(node =>
                {
                    var d = Node.Diameter;
                    return clickPos.X >= (node.X - d) && clickPos.Y >= (node.Y - d) && clickPos.X <= (node.X + d) && clickPos.Y <= (node.Y + d);
                }) is Node;
        }

        private bool GetNodeAtCurrentClickPosition(Point clickPos, out Node n)
        {
            n = GraphContainer.Children.OfType<Node>()
                .FirstOrDefault(node =>
                {
                    var d = Node.Diameter;
                    return clickPos.X >= (node.X - d) && clickPos.Y >= (node.Y - d) && clickPos.X <= (node.X + d) && clickPos.Y <= (node.Y + d);
                });
            return n is Node;
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
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!SaveWorkspace())
                    MessageBox.Show("Unexpected Error! Try again later.");
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NewWorkspace();
            }
            else if (e.Key == Key.O && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenWorkspace();
            }
        }

        private void Button_New_Click(object sender, RoutedEventArgs e)
        {
            NewWorkspace();
        }

        private void NewWorkspace()
        {
            GraphContainer.Children.Clear();
            _numberOfNode = 0;

            bool? selection = null;

            new SubWindows.SelectGraphTypeWindow
            {
                Ok = undirected => selection = undirected,
            }.ShowDialog();

            if (selection is null)
            {
                coveringPanel.Visibility = Visibility.Visible;
                GraphContainer.IsEnabled = false;
                return;
            }

            _isDirectedGraph = !selection.Value;
            coveringPanel.Visibility = Visibility.Collapsed;
            GraphContainer.IsEnabled = true;
        }

        private void OpenWorkspace()
        {

        }

        private bool SaveWorkspace()
        {
            Routes.Add(new Route { DestNode = 'A' });
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Run_Click(object sender, RoutedEventArgs e)
        {
            var selectedNodes = GraphContainer.Children.OfType<Node>().Where(n => n.IsSelected);
            if ((selectedNodes.Count() & 1) != 1)
            {
                MessageBox.Show("Please select only one node to run the algorithm");
                return;
            }

            var root = selectedNodes.ElementAt(0);

            foreach (var uiel in GraphContainer.Children)
            {
                if (uiel is Node node)
                {
                    RouteEngine.Instance.Nodes.Add(node);
                }
                else if (uiel is Connection con)
                {
                    RouteEngine.Instance.Connections.Add(con);
                }
            }

            var dict = RouteEngine.Instance.RunDijsktra(root);
            Routes = new ObservableCollection<Route>(dict.Values);
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveWorkspace())
                MessageBox.Show("Unexpected Error");
        }
    }
}
