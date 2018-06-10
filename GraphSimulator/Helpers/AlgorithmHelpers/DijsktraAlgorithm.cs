using GraphSimulator.Helpers.AlgorithmHelpers;
using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSimulator.Helpers.AlgorithmHelpers
{
    public class DijsktraAlgorithm : IAlgorithm
    {
        public IEnumerable<Route> ShowResult(Node startNode)
        {
            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, RouteEngine.Instance.Nodes.Select(p => p.Key));         // Initialisation
            //startNode.RouteCost = 0;

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

                var neighbors = RouteEngine.Instance.Connections
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
                        RouteEngine.Instance.Nodes[curNeighboringNode].RouteCost = route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        if (RouteEngine.Instance.Nodes[curNeighboringNode].NodeStatus == NodeStatus.Processed || queue.Contains(curNeighboringNode))
                            continue;
                        queue.Enqueue(curNeighboringNode);
                    }
                }
                RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
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

        public IEnumerable<Route> ExtractStepsWithResult(Node startNode)
        {
            RouteEngine.Instance.Actions = new List<Action>();

            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, RouteEngine.Instance.Nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            var handledNodes = new List<char>();

            RouteEngine.Instance.Actions.Add(() =>
            {
                foreach (var item in RouteEngine.Instance.Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
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

                RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);

                var neighbors = RouteEngine.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    RouteEngine.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);

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

                            RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[curNeighboringNode].NodeStatus = NodeStatus.IsInQueue);
                            queue.Enqueue(curNeighboringNode);
                        }
                        var temp = new List<Connection>(route.Paths);
                        RouteEngine.Instance.Actions.Add(() =>
                        {
                            var cost = 0;
                            foreach (var conn in temp1)
                            {
                                RouteEngine.Instance.Connections.FirstOrDefault(c => c.Equals(conn)).ConnectionStatus = ConnectionStatus.None;
                            }
                            foreach (var conn in temp)
                            {
                                RouteEngine.Instance.Connections.FirstOrDefault(c => c.Equals(conn)).ConnectionStatus = ConnectionStatus.IsSelected;
                                cost += conn.Cost;
                            }
                            RouteEngine.Instance.Nodes[curNeighboringNode].RouteCost = cost;
                            //var temp2 = new List<Route>(shortestPaths.Values);
                            //action(temp2);
                        });
                    }
                    else
                    {
                        RouteEngine.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.None);
                    }
                }
                handledNodes.Add(nodeToProcess);
                RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);
            }

            return shortestPaths.Values;
        }
    }

}
