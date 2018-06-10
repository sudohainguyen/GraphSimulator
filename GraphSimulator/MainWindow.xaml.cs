using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using GraphSimulator.Helpers;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using GraphSimulator.Helpers.AlgorithmHelpers;

namespace GraphSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const float MAX_TIMESPAN = 2.5f;

        #region Fields
        private bool _isDirectedGraph = false;
        private int _numberOfNode = 0;
        private int _curStep = -1;
        private bool _isPause = true;
        private bool _canEditGraph = true;

        private DispatcherTimer _timer;

        private Random _randCost;
        private Node _startNode;
        private Stack<(Operation operation, IEnumerable<UIElement> controls)> _operationStack = new Stack<(Operation, IEnumerable<UIElement>)>();
        private ObservableCollection<Route> _routes;

        #endregion

        #region Properties
        public RunningMode RunMode { get; set; } = RunningMode.ShowTheResult;
        public IAlgorithm Algorithm { get; set; } = new DijsktraAlgorithm();
        public ObservableCollection<Route> Routes
        {
            get => _routes;
            set
            {
                _routes = value;
                OnPropertyChanged(nameof(Routes));
            }
        }

        public bool IsCreatingConnection { get; set; } = false;

        public bool IsRandomCost { get; set; } = false;

        public bool IsPause
        {
            get => _isPause;
            set
            {
                _isPause = value;
                OnPropertyChanged(nameof(IsPause));
            }
        }

        public bool CanEditGraph
        {
            get => _canEditGraph;
            set
            {
                _canEditGraph = value;
                OnPropertyChanged(nameof(CanEditGraph));
            }
        }

        #endregion

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
                    else if (targetNode.Identity == _startNode.Identity)
                    {
                        _startNode = null;
                        return;
                    }

                    if (_isDirectedGraph && RouteEngine.Instance.Connections
                                                .FirstOrDefault(c => c.StartNode == targetNode.Identity && c.DestNode == _startNode.Identity)
                                                is Connection con)
                    {
                        //TODO: Seperate a two-way connection to 2 one-way connections ?
                        con.ArrowDirection = Direction.TwoWay;
                        var tblReCost = AddReverseCost(con);
                        _operationStack.Push((Operation.UPDATE, new List<UIElement>() { tblReCost, con }));
                        if (IsRandomCost)
                        {
                            if (_randCost is null) _randCost = new Random();
                            con.ReverseCost = _randCost.Next(1, 30);
                        }
                        else
                        {
                            new SubWindows.CostInputWindow
                            {
                                Ok = cost =>
                                {
                                    con.ReverseCost = cost;
                                },
                                Owner = this
                            }.ShowDialog();
                        }
                    }
                    else
                    {
                        var newCon = new Connection(_isDirectedGraph, _startNode, targetNode);
                        if (IsRandomCost)
                        {
                            if (_randCost is null) _randCost = new Random();
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
                    }
                    
                    _startNode.NodeStatus = targetNode.NodeStatus = NodeStatus.None;
                    _startNode = null;
                }
            }
        }

        private void GraphContainer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var nodesToDel = GraphContainer.Children.OfType<Node>()
                    .Where(node => node.IsSelected).ToList();
                if (nodesToDel.Count() == 0)
                    return;

                _operationStack.Push((Operation.DELETE, nodesToDel));

                foreach (var item in nodesToDel)
                {
                    GraphContainer.Children.Remove(item);
                    RouteEngine.Instance.Nodes.Remove(item.Identity);
                    _numberOfNode--;
                }
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (_operationStack.Count == 0)
                    return;
                var (operation, controls) = _operationStack.Pop();
                switch (operation)
                {
                    case Operation.ADD:
                        {
                            foreach (var c in controls)
                            {
                                GraphContainer.Children.Remove(c);
                                if (c is Node n)
                                {
                                    RouteEngine.Instance.Nodes.Remove(n.Identity);
                                    _numberOfNode--;
                                }
                                else if (c is Connection con)
                                {
                                    RouteEngine.Instance.Connections.Remove(con);
                                }
                            }
                            break;
                        }
                    case Operation.UPDATE:
                        {
                            foreach (var c in controls)
                            {
                                if (c is Connection con)
                                    con.ArrowDirection = Direction.OneWay;
                                else if (c is TextBlock tbl)
                                    GraphContainer.Children.Remove(tbl);
                            }
                            break;
                        }
                    case Operation.DELETE:
                        {
                            foreach (var c in controls)
                            {
                                GraphContainer.Children.Add(c);
                                if (c is Node n)
                                {
                                    RouteEngine.Instance.Nodes.Add(n.Identity, n);
                                    _numberOfNode++;
                                }
                                else if (c is Connection con)
                                {
                                    RouteEngine.Instance.Connections.Add(con);
                                }
                            }
                            break;
                        }
                    default:
                        break;
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

        private void Button_Run_Click(object sender, RoutedEventArgs e)
        {
            var selectedNodes = GraphContainer.Children.OfType<Node>().Where(n => n.IsSelected);
            if ((selectedNodes.Count() & 1) != 1)
            {
                MessageBox.Show("Please select only one node to run the algorithm");
                return;
            }
            Routes = new ObservableCollection<Route>();

            var root = selectedNodes.ElementAt(0);
            if (Algorithm is PrimAlgorithm)
                Helper.ToUndirectedGraph();
            switch (RunMode)
            {
                case RunningMode.ShowTheResult:
                    {
                        Routes = new ObservableCollection<Route>(Algorithm.ShowResult(root));
                        break;
                    }
                case RunningMode.StepByStep:
                    {
                        Routes = new ObservableCollection<Route>(Algorithm.ExtractStepsWithResult(root));
                        MessageBox.Show(@"Press 'next' button to go next step");
                        break;
                    }
                case RunningMode.Visualization:
                    {
                        Routes = new ObservableCollection<Route>(Algorithm.ExtractStepsWithResult(root));
                        _timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(MAX_TIMESPAN - slSpeed.Value)
                        };
                        _timer.Start();
                        _timer.Tick += Tick;
                        IsPause = false;
                        break;
                    }
                default:
                    break;
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (SaveWorkspace(out var err))
                MessageBox.Show("Saved successfully");
            else
                MessageBox.Show(err);
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in GraphContainer.Children.OfType<UIElement>())
            {
                if (item is Node n) n.NodeStatus = NodeStatus.None;
                else if (item is Connection con) con.ConnectionStatus = ConnectionStatus.None;
            }
            if (dataGrid.CurrentItem is Route r)
            {
                RouteEngine.Instance.Nodes[r.Paths[0].StartNode].NodeStatus = NodeStatus.IsSelected;
                foreach (var con in r.Paths)
                {
                    con.ConnectionStatus = ConnectionStatus.IsSelected;
                    RouteEngine.Instance.Nodes[con.DestNode].NodeStatus = NodeStatus.IsSelected;
                }
            }
        }

        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            OpenWorkspace();
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {

        }

        private void slSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _timer.Interval = TimeSpan.FromSeconds(MAX_TIMESPAN - e.NewValue);
        }

        #region Private methods

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
                return;

            _isDirectedGraph = !selection.Value;

            coveringPanel.Visibility = Visibility.Collapsed;
            CanEditGraph = true;
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
            //GraphContainer.IsEnabled = true;

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
                var start = RouteEngine.Instance.Nodes[c.Start];
                var dest = RouteEngine.Instance.Nodes[c.Dest];
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
            var nodes = RouteEngine.Instance.Nodes.Select(p => p.Value);
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
            File.SetAttributes(dialog.FileName, FileAttributes.ReadOnly);

            return true;
        }

        private void AddNewNode(Node newNode)
        {
            RouteEngine.Instance.Nodes.Add(newNode.Identity, newNode);
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

        private TextBlock AddReverseCost(Connection curConn)
        {
            var tblCost = new TextBlock
            {
                Text = curConn.ReverseCost.ToString(),
                FontSize = 17,
                FontWeight = FontWeights.DemiBold
            };
            var pointForTbl = Helper.CalPointForTextBlockCost(new Point(curConn.X2, curConn.Y2), new Point(curConn.X1, curConn.Y1));
            Canvas.SetLeft(tblCost, pointForTbl.X - 10);
            Canvas.SetTop(tblCost, pointForTbl.Y - 10);
            GraphContainer.Children.Add(tblCost);

            return tblCost;
        }

        private void Tick(object obj, EventArgs e)
        {
            _curStep++;
            RouteEngine.Instance.Actions[_curStep]();
            if (_curStep.Equals(RouteEngine.Instance.Actions.Count - 1))
            {
                _timer.Tick -= Tick;
                _timer.Stop();
                IsPause = false;
                MessageBox.Show("Finished");
            }
        }

        #endregion

        #region Nested Classes

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
        #endregion

        #region Property changed

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (_curStep != RouteEngine.Instance.Actions.Count - 1)
            {
                _curStep++;
                btnBack.IsEnabled = true;
            }
            else
            {
                btnNext.IsEnabled = false;
            }
            RouteEngine.Instance.Actions[_curStep]();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Routes[1].RouteCost = 333;
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (IsPause)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }
        }
    }
}
