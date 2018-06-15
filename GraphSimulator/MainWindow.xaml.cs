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
using System.Windows.Media.Animation;
using GraphSimulator.SubWindows;

namespace GraphSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const float MAX_TIMESPAN = 1.5f;

        #region Fields
        private bool _isDirectedGraph = false;
        private int _numberOfNode = 0;
        private int _curStep = -1;
        private bool _isPause = true;
        private bool _canEditGraph = true;
        private bool _isHide = true;

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

                    if (_isDirectedGraph && Graph.Instance.Connections
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
                            new CostInputWindow
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
                            new CostInputWindow
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
                List<UIElement> nodesToDel = new List<UIElement>(GraphContainer.Children.OfType<Node>()
                    .Where(node => node.IsSelected));
                var j = nodesToDel.Count - 1;
                if (j + 1 == 0)
                    return;
                for (; j >= 0; j--)
                {
                    var item = (Node)nodesToDel[j];
                    GraphContainer.Children.Remove(item);
                    var i = Graph.Instance.Connections.Count - 1;
                    for (; i >= 0; i--)
                    {
                        var curCon = Graph.Instance.Connections[i];
                        if (curCon.StartNode.Equals(item.Identity) || curCon.DestNode.Equals(item.Identity))
                        {
                            Graph.Instance.Connections.RemoveAt(i);
                            GraphContainer.Children.Remove(curCon.TextBlockCost);
                            GraphContainer.Children.Remove(curCon);
                            nodesToDel.Add(curCon.TextBlockCost);
                            nodesToDel.Add(curCon);
                        }
                    }

                    Graph.Instance.Nodes.Remove(item.Identity);
                    _numberOfNode--;
                }

                _operationStack.Push((Operation.DELETE, nodesToDel));
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
                                    Graph.Instance.Nodes.Remove(n.Identity);
                                    _numberOfNode--;
                                }
                                else if (c is Connection con)
                                {
                                    Graph.Instance.Connections.Remove(con);
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
                                    Graph.Instance.Nodes.Add(n.Identity, n);
                                    _numberOfNode++;
                                }
                                else if (c is Connection con)
                                {
                                    Graph.Instance.Connections.Add(con);
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
            if (!Algorithm.CanRunWithGraph(Graph.Instance, out var message))
            {
                MessageBox.Show(message, "Warning");
                return;
            }
            if (Algorithm is PrimAlgorithm && !Graph.Instance.IsUndirected)
            {
                if (MessageBox.Show("To find MST, the graph must be converted to undirected graph. Agree ?", "Attention", 
                                    MessageBoxButton.YesNo, 
                                    MessageBoxImage.Question) 
                        == MessageBoxResult.No)
                    return;
                Graph.Instance.ToUndirectedGraph();
            }

            Routes = new ObservableCollection<Route>();
            var root = selectedNodes.ElementAt(0);

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

        //private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    foreach (var item in GraphContainer.Children.OfType<UIElement>())
        //    {
        //        if (item is Node n) n.NodeStatus = NodeStatus.None;
        //        else if (item is Connection con) con.ConnectionStatus = ConnectionStatus.None;
        //    }
        //    if (dataGrid.CurrentItem is Route r)
        //    {
        //        Graph.Instance.Nodes[r.Paths[0].StartNode].NodeStatus = NodeStatus.IsSelected;
        //        foreach (var con in r.Paths)
        //        {
        //            con.ConnectionStatus = ConnectionStatus.IsSelected;
        //            Graph.Instance.Nodes[con.DestNode].NodeStatus = NodeStatus.IsSelected;
        //        }
        //    }
        //}

        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            OpenWorkspace();
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {
            new SamplesWindow
            {
                Owner = this,
                PassData = rawData => LoadGraphFromData(rawData)
            }.ShowDialog();
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

            new SelectGraphTypeWindow
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

            LoadGraphFromData(raw);
        }

        private void LoadGraphFromData(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                return;
            var decodedData = Helper.Base64Decode(rawData).Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
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
                var start = Graph.Instance.Nodes[c.Start];
                var dest = Graph.Instance.Nodes[c.Dest];
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
            Graph.Instance.Connections.Clear();
            Graph.Instance.Nodes.Clear();
            GraphContainer.Children.Clear();
            dataGrid.Items.Clear();
            _startNode = null;
        }

        private void ResetGraphStatus()
        {
            if (Graph.Instance.Actions is null)
                return;
            Graph.Instance.Actions.Clear();
            foreach (var item in GraphContainer.Children)
            {
                if (item is Node n)
                {
                    n.NodeStatus = NodeStatus.None;
                    n.RouteCost = -1;
                }
                else if (item is Connection c)
                {
                    c.ConnectionStatus = ConnectionStatus.None;
                }
                dataGrid.Items.Clear();
            }
        }

        private bool SaveWorkspace(out string err)
        {
            err = null;
            var nodes = Graph.Instance.Nodes.Select(p => p.Value);
            var cons = Graph.Instance.Connections;

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
            Graph.Instance.Nodes.Add(newNode.Identity, newNode);
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
            Graph.Instance.Connections.Add(newCon);

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
            Graph.Instance.Actions[_curStep]();
            if (_curStep.Equals(Graph.Instance.Actions.Count - 1))
            {
                _timer.Tick -= Tick;
                _timer.Stop();
                IsPause = true;
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
            var actions = Graph.Instance.Actions;
            if (actions is null || actions.Count == 0)
                return;
            if (_curStep != Graph.Instance.Actions.Count - 1)
            {
                if (Graph.Instance.BackStack is null)
                    Graph.Instance.BackStack = new List<Dictionary<string, (string status, int? nodeValue)>>();
                var stack = Graph.Instance.BackStack;
                if (_curStep == stack.Count)
                {
                    var curElementsStatus = new Dictionary<string, (string status, int? nodeValue)>();
                    foreach (var element in GraphContainer.Children)
                    {
                        if (element is Node n)
                            curElementsStatus.Add(n.Identity.ToString(), (n.NodeStatus.ToString(), n.RouteCost));
                        else if (element is Connection c)
                            curElementsStatus.Add(c.Identity, (c.ConnectionStatus.ToString(), null));
                    }
                    stack.Add(curElementsStatus);
                }
                if (!btnBack.IsEnabled)
                    btnBack.IsEnabled = true;
                _curStep++;
            }
            else
            {
                btnNext.IsEnabled = false;
            }
            Graph.Instance.Actions[_curStep]();
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
        private void Button_Menu_Click(object sender, RoutedEventArgs e)
        {
            var function = new PowerEase();
            var mode = EasingMode.EaseOut;

            StoryboardLibrary.MenuAnim(gridMenu, _isHide, gridMenu.RenderSize.Width - 55, function, mode, StoryboardLibrary.MoveDirection.RightLeft).Begin();
            _isHide = !_isHide;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((TabControl) sender).SelectedIndex == 0)
            {
                ResetGraphStatus();
            }
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            _curStep--;
            if (!btnNext.IsEnabled)
                btnNext.IsEnabled = true;
            else if (_curStep == 0)
                btnBack.IsEnabled = false;
            foreach (var element in GraphContainer.Children)
            {
                var temp = Graph.Instance.BackStack[_curStep];
                if (element is Node n)
                {
                    n.NodeStatus = (NodeStatus)Enum.Parse(typeof(NodeStatus), temp[$"{n.Identity}"].status);
                    n.RouteCost = temp[$"{n.Identity}"].nodeValue.Value;
                }
                else if (element is Connection c)
                {
                    c.ConnectionStatus = (ConnectionStatus)Enum.Parse(typeof(ConnectionStatus), temp[c.Identity].status);
                }
            }
        }
    }
}
