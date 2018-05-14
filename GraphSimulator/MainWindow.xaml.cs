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
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;

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
        }

        private void GraphContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(GraphContainer);
            var newPos = e.GetPosition(GraphContainer);

            if (!IsCreatingConnection)
            {
                if (IsOverlapExistedNode(newPos))
                    return;
                if (!Helper.CreateNode(_numberOfNode, e.GetPosition(GraphContainer), out var newNode))
                {
                    MessageBox.Show("Ten nodes are enough for a good simulation :)");
                    return;
                }

                AddNewNode(newNode);
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
                        if (!Helper.CreateNode(_numberOfNode, e.GetPosition(GraphContainer), out targetNode))
                        {
                            MessageBox.Show("Ten nodes are enough for a good simulation :)");
                            return;
                        }
                        AddNewNode(targetNode);
                        _operationStack.Push((Operation.ADD, new List<UIElement>() { targetNode }));
                    }
                    else if (targetNode.Identity == _startNode.Identity) return;

                    var newCon = new Connection(_isDirectedGraph, _startNode, targetNode);

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
                            },
                            Owner = this
                    }.ShowDialog();
                    }
                    if (newCon.Cost == -1)
                    {
                        newCon = null;
                    }
                    else
                    {
                        var tblCost = AddNewConnection(newCon);
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
                        _numberOfNode--;
                    }
                }
                else
                {
                    foreach (var c in controls)
                    {
                        GraphContainer.Children.Add(c);
                    }
                }
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!SaveWorkspace(out var err))
                    MessageBox.Show(err);
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
            ResetWorkspace();

            bool? selection = null;

            new SubWindows.SelectGraphTypeWindow
            {
                Ok = undirected => selection = undirected,
                Owner = this
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
            var dialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            string raw = "";
            if (dialog.ShowDialog(this).Value)
                raw = File.ReadAllText(dialog.FileName);

            if (string.IsNullOrEmpty(raw))
                return;

            var decodedData = Helper.Base64Decode(raw).Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<NodeData> nodesdata = null;
            IEnumerable<ConData> consdata = null;
            try
            {
                _isDirectedGraph = JsonConvert.DeserializeObject<bool>(decodedData[0]);
                nodesdata = JsonConvert.DeserializeObject<IEnumerable<NodeData>>(decodedData[1]);
                consdata = JsonConvert.DeserializeObject<IEnumerable<ConData>>(decodedData[2]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            ResetWorkspace();
            coveringPanel.Visibility = Visibility.Collapsed;
            GraphContainer.IsEnabled = true;

            foreach (var n in nodesdata)
            {
                var newNode = new Node
                {
                    X = n.X,
                    Y = n.Y,
                    Identity = n.Id
                };
                AddNewNode(newNode);
            }
            foreach (var c in consdata)
            {
                var start = RouteEngine.Instance.Nodes.FirstOrDefault(n => n.Identity == c.Start);
                var dest = RouteEngine.Instance.Nodes.FirstOrDefault(n => n.Identity == c.Dest);
                var newCon = new Connection(_isDirectedGraph, start, dest)
                {
                    Cost = c.Cost,
                    ArrowDirection = (Direction)Enum.ToObject(typeof(Direction), c.Dir)
                };
                AddNewConnection(newCon);
            }
        }

        private void ResetWorkspace()
        {
            _numberOfNode = 0;
            RouteEngine.Instance.Connections.Clear();
            RouteEngine.Instance.Nodes.Clear();
            GraphContainer.Children.Clear();
            _startNode = null;
        }

        private bool SaveWorkspace(out string err)
        {
            err = null;
            var nodes = RouteEngine.Instance.Nodes;
            var cons = RouteEngine.Instance.Connections;

            if (nodes.Count() == 0 && cons.Count() == 0)
            {
                err = "Cannot save an empty workspace.";
                return false;
            }

            var nodesdata = new List<NodeData>();
            var consdata = new List<ConData>();

            foreach (var node in nodes)
            {
                var obj = new NodeData
                {
                    Id = node.Identity,
                    X = node.X,
                    Y = node.Y
                };
                nodesdata.Add(obj);
            }

            foreach (var con in cons)
            {
                var obj = new ConData
                {
                    Dest = con.DestNode,
                    Start = con.StartNode,
                    Cost = con.Cost,
                    Dir = (int)con.ArrowDirection
                };
                consdata.Add(obj);
            }
            var isDr = _isDirectedGraph;
            var jsonGraphType = JsonConvert.SerializeObject(isDr);
            var jsonNodes = JsonConvert.SerializeObject(nodesdata);
            var jsonCons = JsonConvert.SerializeObject(consdata);

            var dialog = new SaveFileDialog()
            {
                Filter = "Text file (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                OverwritePrompt = true
            };
            var encodedData = Helper.Base64Encode(jsonGraphType + "--" + jsonNodes + "--" + jsonCons);
            if (!dialog.ShowDialog(this).Value)
            {
                err = "Unexpected Error.";
                return false;
            }
            File.WriteAllText(dialog.FileName, encodedData);

            return true;
        }

        private void AddNewNode(Node newNode)
        {
            RouteEngine.Instance.Nodes.Add(newNode);
            GraphContainer.Children.Add(newNode);
            Canvas.SetLeft(newNode, newNode.X - Node.Radius);
            Canvas.SetTop(newNode, newNode.Y - Node.Radius);
            _numberOfNode++;
        }

        private TextBlock AddNewConnection(Connection newCon)
        {
            var tblCost = new TextBlock
            {
                Text = newCon.Cost.ToString(),
                FontSize = 17,
                FontWeight = FontWeights.DemiBold
            };

            newCon.TextBlockCost = tblCost;
            Canvas.SetZIndex(newCon, -99);

            var pointForTbl = Helper.CalPointForTextBlockCost(new Point(newCon.X1, newCon.Y1), new Point(newCon.X2, newCon.Y2));

            Canvas.SetLeft(tblCost, pointForTbl.X - 10);
            Canvas.SetTop(tblCost, pointForTbl.Y - 10);

            GraphContainer.Children.Add(newCon);
            GraphContainer.Children.Add(tblCost);
            RouteEngine.Instance.Connections.Add(newCon);

            return tblCost;
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

            var dict = RouteEngine.Instance.RunDijsktra(root);
            Routes = new ObservableCollection<Route>(dict.Values);
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveWorkspace(out var err))
                MessageBox.Show(err);
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in GraphContainer.Children.OfType<UIElement>())
            {
                if (item is Node n) n.IsSelected = false;
                else if (item is Connection con) con.IsSelected = false;
            }
            if (dataGrid.CurrentItem is Route r)
            {
                foreach (var con in r.Paths)
                {
                    con.IsSelected = true;
                    RouteEngine.Instance.Nodes.FirstOrDefault(n => n.Identity == con.DestNode).IsSelected = true;
                }
            }
        }

        private class NodeData
        {
            public char Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private class ConData
        {
            public char Start { get; set; }
            public char Dest { get; set; }
            public int Dir { get; set; }
            public int Cost { get; set; }
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenWorkspace();
        }
    }
}
