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
        public List<string> Pseudocode =>
            new List<string>
            {
                "BEGIN",
                "  FOR i = 0,.., n-1 DO",
                "    d(v[i]) ← ∞",
                "  d(v[root]) ← 0",
                "  queue.insert(root, 0)",
                "  WHILE queue ≠ ∅ DO",
                "    u = queue.extractMin()",
                "    FOR ALL (u,w) ∈ E DO",
                "      dist ← d(u) + l(u,w)",
                "      IF d(w) > dist DO",
                "        IF w ∉ queue",
                "          queue.insert(w,dist)",
                "        d(w) = dist",
                "",
                "END"
            };

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
            var actions = Graph.Instance.Action = new List<Func<List<int>>>
            {
                () => new List<int>{ 0 }
            };
            var nodes = Graph.Instance.Nodes;

            var shortestPaths = Helper.InitSetForDijsktra(startNode.Identity, nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            var handledNodes = new List<char>();

            actions.Add(() =>
            {
                foreach (var item in nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
                {
                    item.RouteCost = int.MaxValue;
                }
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.IsSelected;
                return new List<int> { 1, 2, 3, 4 };
            });

            while (queue.Count != 0)
            {
                actions.Add(() => new List<int> { 5 });
                if (queue.Count > 1)
                {
                    queue = new Queue<char>(shortestPaths.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.RouteCost)
                                                          .Select(pair => pair.Key));
                }
                var nodeToProcess = queue.Dequeue();

                actions.Add(() =>
                {
                    nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed;
                    return new List<int> { 6 };
                });

                var neighbors = Graph.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    actions.Add(() =>
                    {
                        item.ConnectionStatus = ConnectionStatus.IsInspecting;
                        return new List<int> { 7, 8 };
                    });

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
                            actions.Add(() =>
                            {
                                nodes[curNeighboringNode].NodeStatus = NodeStatus.IsInQueue;
                                return new List<int> { 10, 11 };
                            });
                            queue.Enqueue(curNeighboringNode);
                        }
                        var temp = new List<Connection>(route.Paths);
                        actions.Add(() =>
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
                            nodes[curNeighboringNode].RouteCost = cost;
                            return new List<int> { 12 };
                        });
                    }
                    else
                    {
                        actions.Add(() =>
                        {
                            item.ConnectionStatus = ConnectionStatus.None;
                            return new List<int> { 13 };
                        });
                    }
                }
                handledNodes.Add(nodeToProcess);
                var quit = queue.Count == 0;
                actions.Add(() =>
                {
                    nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
                    return quit ? new List<int> { 14 } : new List<int> { 5 };
                });
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
