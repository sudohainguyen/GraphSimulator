using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
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
        public int SelectingAlgorithm { get; set; }

        public Dictionary<char, Route> RunDijsktra(Node startNode)
        {
            var shortestPaths = Helper.InitResults(startNode.Identity, Nodes.Select(p => p.Key), Connections);         // Initialisation
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
                        //foreach (var p in route.Paths)
                        //{
                        //    p.IsSelected = false;
                        //}
                        route.Paths = new List<Connection>(shortestPaths[nodeToProcess].Paths)
                        {
                            item
                        };
                        Nodes[curNeighboringNode].RouteCost = route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        //foreach (var p in route.Paths)
                        //{
                        //    p.IsSelected = true;
                        //}

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
                item.IsSelected = true;
            }

            return shortestPaths;
        }
        public Dictionary<char, Route> RunDijsktraWithTimer(Node startNode)
        {
            if (Actions is null)
                Actions = new List<Action>();

            Actions.Add(() => startNode.NodeStatus = NodeStatus.IsSelected);

            var shortestPaths = Helper.InitResults(startNode.Identity, Nodes.Select(p => p.Key), Connections);

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            var handledNodes = new List<char>();
            
            Actions.Add(() =>
            {
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.Processed;
            });                                                            // Initialisation

            while (queue.Count != 0)
            {
                if (queue.Count > 1)
                {
                    queue = new Queue<char>(shortestPaths.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.RouteCost)
                                                          .Select(pair => pair.Key));
                }
                var nodeToProcess = queue.Dequeue();                                                                 

                Actions.Add(() => Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);         // Process next node

                var neighbors = Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);                                                                          // Inspect the neighbors of the current node.

                    var curNeighboringNode = item.DestNode;                                                                    
                    var route = shortestPaths[curNeighboringNode];
                    if (route.RouteCost > item.Cost + shortestPaths[nodeToProcess].RouteCost)
                    {
                        foreach (var p in route.Paths)
                        {
                            p.IsSelected = false;
                        }
                        route.Paths = new List<Connection>(shortestPaths[nodeToProcess].Paths)
                        {
                            item
                        };
                        Nodes[curNeighboringNode].RouteCost = route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        //////////// có thể có lỗi
                        Actions.Add(() =>
                        {
                            foreach (var conn in route.Paths)
                            {
                                conn.ConnectionStatus = ConnectionStatus.IsSelected;
                            }
                        });
                    }


                    if (handledNodes.Contains(curNeighboringNode) || queue.Contains(curNeighboringNode))
                        continue;

                    Actions.Add(() => Nodes[curNeighboringNode].NodeStatus = NodeStatus.IsInQueue);
                    queue.Enqueue(curNeighboringNode);
                }
                handledNodes.Add(nodeToProcess);
                Actions.Add(() => Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);
            }

            return shortestPaths;
        }
    }
}
