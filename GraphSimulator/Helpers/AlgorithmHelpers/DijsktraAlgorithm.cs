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
            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, Graph.Instance.Nodes.Select(p => p.Key));         // Initialisation
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

                var neighbors = Graph.Instance.Connections
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
                        Graph.Instance.Nodes[curNeighboringNode].RouteCost = route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;

                        if (Graph.Instance.Nodes[curNeighboringNode].NodeStatus == NodeStatus.Processed || queue.Contains(curNeighboringNode))
                            continue;
                        queue.Enqueue(curNeighboringNode);
                    }
                }
                Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
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
            Graph.Instance.Actions = new List<Action>();

            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, Graph.Instance.Nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            var handledNodes = new List<char>();

            Graph.Instance.Actions.Add(() =>
            {
                foreach (var item in Graph.Instance.Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
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

                Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);

                var neighbors = Graph.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    Graph.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);

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

                            Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[curNeighboringNode].NodeStatus = NodeStatus.IsInQueue);
                            queue.Enqueue(curNeighboringNode);
                        }
                        var temp = new List<Connection>(route.Paths);
                        Graph.Instance.Actions.Add(() =>
                        {
                            var cost = 0;
                            foreach (var conn in temp1)
                            {
                                Graph.Instance.Connections.FirstOrDefault(c => c.Identity.Equals(conn.Identity))
                                                          .ConnectionStatus = ConnectionStatus.None;
                            }
                            foreach (var conn in temp)
                            {
                                Graph.Instance.Connections.FirstOrDefault(c => c.Identity.Equals(conn.Identity))
                                                          .ConnectionStatus = ConnectionStatus.IsSelected;
                                cost += conn.Cost;
                            }
                            Graph.Instance.Nodes[curNeighboringNode].RouteCost = cost;
                            //var temp2 = new List<Route>(shortestPaths.Values);
                            //action(temp2);
                        });
                    }
                    else
                    {
                        Graph.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.None);
                    }
                }
                handledNodes.Add(nodeToProcess);
                Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);
            }

            return shortestPaths.Values;
        }

        public bool CanRunWithGraph(Graph g, out string message)
        {
            message = "Dijkstra Algorithm cannot work with negative weighted graph";
            return !g.HasNegativeCost;
        }
    }
}
