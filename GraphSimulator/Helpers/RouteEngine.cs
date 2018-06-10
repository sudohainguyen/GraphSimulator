using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSimulator.Helpers
{
    public class RouteEngine
    {
        private static RouteEngine _instance;
        private static readonly object Padlock = new object();

        public static RouteEngine Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance is null)
                        _instance = new RouteEngine();
                }
                return _instance;
            }
        }

        public List<Connection> Connections { get; set; } = new List<Connection>();
        public Dictionary<char, Node> Nodes { get; set; } = new Dictionary<char, Node>();
        public List<Action> Actions { get; set; }


        public IEnumerable<Route> RunDijsktra(Node startNode, RunningMode mode, Action<IEnumerable<Route>> action = null)
        {
            switch (mode)
            {
                case RunningMode.ShowTheResult:
                    return RunDijsktraShowResult(startNode, action);
                case RunningMode.StepByStep:
                    //return RunDijsktraWithTimer(startNode, action);
                case RunningMode.Visualization:
                    return RunDijsktraWithTimer(startNode, action);
                default:
                    return null;
            }
        }

        public IEnumerable<Route> RunDijsktraShowResult(Node startNode, Action<IEnumerable<Route>> action)
        {
            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, Nodes.Select(p => p.Key));         // Initialisation
            startNode.RouteCost = 0;

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            while (queue.Count != 0)
            {
                if (queue.Count > 1)
                {
                    queue = new Queue<char>(shortestPaths.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.RouteCost)
                                                          .Select(pair => pair.Key));
                }
                var nodeToProcess = queue.Dequeue();                                                                 // Process next node

                var neighbors = Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    var curNeighboringNode = item.DestNode;                                                                    // Inspect the neighbors of the current node.
                    var route = shortestPaths[curNeighboringNode];
                    if (route.RouteCost > item.Cost + shortestPaths[nodeToProcess].RouteCost)
                    {
                        route.Paths = new List<Connection>(shortestPaths[nodeToProcess].Paths)
                        {
                            item
                        };
                        Nodes[curNeighboringNode].RouteCost = route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        if (Nodes[curNeighboringNode].NodeStatus == NodeStatus.Processed || queue.Contains(curNeighboringNode))
                            continue;
                        queue.Enqueue(curNeighboringNode);
                    }
                }
                Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
            }

            foreach (var item in shortestPaths.Select(p => p.Value)
                                                .Select(r => r.Paths)
                                                .SelectMany(c => c).Distinct())
            {
                item.ConnectionStatus = ConnectionStatus.IsSelected;
            }

            //action(shortestPaths.Values);
            return shortestPaths.Values;
        }
        public IEnumerable<Route> RunDijsktraWithTimer(Node startNode, Action<IEnumerable<Route>> action)
        {
            if (Actions is null)
                Actions = new List<Action>();

            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, Nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            var handledNodes = new List<char>();
            
            Actions.Add(() =>
            {
                foreach (var item in Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
                {
                    item.RouteCost = int.MaxValue;
                }
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.IsSelected;
                //var temp = new List<Route>(shortestPaths.Values);
                //action(temp);
            });                                                            

            while (queue.Count != 0)
            {
                if (queue.Count > 1)
                {
                    queue = new Queue<char>(shortestPaths.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.RouteCost)
                                                          .Select(pair => pair.Key));
                }
                var nodeToProcess = queue.Dequeue();                                                                 

                Actions.Add(() => Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);         

                var neighbors = Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);

                    var curNeighboringNode = item.DestNode;                                                                    
                    var route = shortestPaths[curNeighboringNode];
                    if (route.RouteCost > item.Cost + shortestPaths[nodeToProcess].RouteCost)
                    {
                        var temp1 = new List<Connection>(route.Paths);
                        route.Paths = new List<Connection>(shortestPaths[nodeToProcess].Paths)
                        {
                            item
                        };
                        route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        if (!handledNodes.Contains(curNeighboringNode) && !queue.Contains(curNeighboringNode))
                        {

                            Actions.Add(() => Nodes[curNeighboringNode].NodeStatus = NodeStatus.IsInQueue);
                            queue.Enqueue(curNeighboringNode);
                        }
                        var temp = new List<Connection>(route.Paths);
                        Actions.Add(() =>
                        {
                            var cost = 0;
                            foreach (var conn in temp1)
                            {
                                Connections.FirstOrDefault(c => c.Equals(conn)).ConnectionStatus = ConnectionStatus.None;
                            }
                            foreach (var conn in temp)
                            {
                                Connections.FirstOrDefault(c => c.Equals(conn)).ConnectionStatus = ConnectionStatus.IsSelected;
                                cost += conn.Cost;
                            }
                            Nodes[curNeighboringNode].RouteCost = cost;
                            //var temp2 = new List<Route>(shortestPaths.Values);
                            //action(temp2);
                        });
                    }
                    else
                    {
                        Actions.Add(() => item.ConnectionStatus = ConnectionStatus.None);
                    }
                }
                handledNodes.Add(nodeToProcess);
                Actions.Add(() => Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);
            }

            return shortestPaths.Values;
        }
        public void RunPrim(Node startNode)
        {
            var set = Helper.InitSetForPrim(startNode.Identity, Nodes.Select(p => p.Key));
            var handleNodes = new List<char>();
            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);
            startNode.RouteCost = 0;
            while (queue.Count != 0)
            {
                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
                var nodeToProcess = queue.Dequeue();

                var neighbors = Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.ArrowDirection == Direction.None));

                foreach (var item in neighbors)
                {
                    char node, curNode;
                    if (item.StartNode == nodeToProcess)
                    {
                        node = item.DestNode;
                        curNode = item.StartNode;
                    }
                    else
                    {
                        node = item.StartNode;
                        curNode = item.DestNode;
                    }
                    if (handleNodes.Contains(node))
                        continue;

                    if (set[node].PrevNode.Equals('-'))
                        queue.Enqueue(node);
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                        continue;

                    set[node] = (curNode, item.Cost);
                    Nodes[node].RouteCost = item.Cost;
                }
                handleNodes.Add(nodeToProcess);
                Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
            }
            foreach (var item in Nodes.Keys.Where(k => !k.Equals(startNode.Identity)))
            {
                Connections.FirstOrDefault(c => c.A(item, set[item].PrevNode)).ConnectionStatus = ConnectionStatus.IsSelected;
            }
        }
    }
}
