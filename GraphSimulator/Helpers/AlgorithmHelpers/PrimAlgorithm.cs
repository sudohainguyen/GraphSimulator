using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSimulator.User_Controls;

namespace GraphSimulator.Helpers.AlgorithmHelpers
{
    public class PrimAlgorithm : IAlgorithm
    {
        public List<string> Pseudocode =>
            new List<string>
            {
                "BEGIN",
                "  handledNode ← ∅",
                "  FOR i ← 0, ..., n-1 DO",
                "    d(v[i]) ← ∞, prev(v[i]) ← NULL",
                "  d(v[root]) ← 0, prev(v[1]) ← v[root]",
                "  queue.insert(v[root])",
                "  WHILE queue.count > 1 DO",
                "    u ← queue.extractMin()",
                "    FOR ALL { u, w } ∈ E do",
                "      IF w ∈ handledNode THEN",
                "        continue",
                "      IF prev(w) = NULL THEN",
                "        queue.insert(w)",
                "      ELSE IF w ∉ queue AND l(u, w) >= d(w) THEN",
                "        continue",
                "      d(w) ← l(u, w), prev(w) ← u",
                "    handledNode.insert(u)",
                "  last ← queue.dequeue()",
                "  handledNode.insert(last)",
                "END"
            };

        public IEnumerable<Route> ExtractStepsWithResult(Node startNode)
        {
            var actions = Graph.Instance.Action = new List<Func<List<int>>>
            {
                () => new List<int>{ 0 }
            };

            var set = Helper.InitSetForPrim(startNode.Identity, Graph.Instance.Nodes.Select(p => p.Key));
            var handleNodes = new List<char>();

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            actions.Add(() =>
            {
                foreach (var item in Graph.Instance.Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
                {
                    item.RouteCost = int.MaxValue;
                }
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.IsSelected;
                return new List<int> { 1, 2, 3, 4, 5 };
            });

            while (handleNodes.Count < Graph.Instance.Nodes.Count - 1)
            {
                actions.Add(() => new List<int> { 6 });
                var nodeToProcess = queue.Dequeue();
                
                actions.Add(() =>
                {
                    Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed;
                    return new List<int> { 7 };
                });
                var neighbors = Graph.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.ArrowDirection == Direction.None));

                foreach (var item in neighbors)
                {
                    actions.Add(() =>
                    {
                        item.ConnectionStatus = ConnectionStatus.IsInspecting;
                        return new List<int> { 8 };
                    });
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
                    {
                        var stt = set[curNode].PrevNode.Equals(node)
                                                ? ConnectionStatus.IsSelected
                                                : ConnectionStatus.None;
                        actions.Add(() =>
                        {
                            item.ConnectionStatus = stt;
                            return new List<int> { 9, 10 };
                        });
                        continue;
                    }

                    if (set[node].PrevNode.Equals('-'))
                    {
                        queue.Enqueue(node);
                        actions.Add(() =>
                        {
                            Graph.Instance.Nodes[node].NodeStatus = NodeStatus.IsInQueue;
                            return new List<int> { 11, 12 };
                        });
                    }
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                    {
                        actions.Add(() =>
                        {
                            Graph.Instance.Connections
                                          .FirstOrDefault(c => c.HasTwoVertices(curNode, node))
                                          .ConnectionStatus = ConnectionStatus.None;
                            return new List<int> { 13, 14 };
                        });
                        continue;
                    }
                    var oldNode = set[node].PrevNode;
                    set[node] = (curNode, item.Cost);
                    actions.Add(() =>
                    {
                        Graph.Instance.Nodes[node].RouteCost = item.Cost;
                        var con = Graph.Instance.Connections.FirstOrDefault(c => c.HasTwoVertices(node, oldNode));
                        if (con != null)
                            con.ConnectionStatus = ConnectionStatus.None;
                        return new List<int> { 15 };
                    });
                }
                handleNodes.Add(nodeToProcess);
                actions.Add(() =>
                {
                    Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
                    return new List<int> { 16 };
                });

                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
                var min = queue.Peek();
                if (min != startNode.Identity)
                {
                    actions.Add(() =>
                    {
                        Graph.Instance.Connections.FirstOrDefault(c => c.HasTwoVertices(min, set[min].PrevNode))
                                                                    .ConnectionStatus = ConnectionStatus.IsSelected;
                        return new List<int> { 16 };
                    });
                }
            }
            var lastNode = queue.Dequeue();
            actions.Add(() =>
            {
                Graph.Instance.Nodes[lastNode].NodeStatus = NodeStatus.Processed;
                return new List<int> { 17, 18 };
            });
            foreach (var item in set)
            {
                yield return new Route(item.Key, item.Value.PrevNode, item.Value.Cost);
            }
            actions.Add(() => new List<int> { 19 });
        }

        public IEnumerable<Route> ShowResult(Node startNode)
        {
            var set = Helper.InitSetForPrim(startNode.Identity, Graph.Instance.Nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);
            startNode.RouteCost = 0;
            var handledNodesCount = 0;
            while (handledNodesCount < Graph.Instance.Nodes.Count - 1)
            {
                var nodeToProcess = queue.Dequeue();

                var neighbors = Graph.Instance.Connections
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
                    if (Graph.Instance.Nodes[node].Processed)
                        continue;

                    if (set[node].PrevNode.Equals('-'))
                        queue.Enqueue(node);
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                        continue;

                    set[node] = (curNode, item.Cost);
                    Graph.Instance.Nodes[node].RouteCost = item.Cost;
                }
                Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
                handledNodesCount++;
                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
            }
            Graph.Instance.Nodes[queue.Dequeue()].NodeStatus = NodeStatus.Processed;
            foreach (var item in Graph.Instance.Nodes.Keys.Where(k => !k.Equals(startNode.Identity)))
            {
                Graph.Instance.Connections.FirstOrDefault(c => c.HasTwoVertices(item, set[item].PrevNode)).ConnectionStatus = ConnectionStatus.IsSelected;
            }
            foreach (var item in set)
            {
                yield return new Route(item.Key, item.Value.PrevNode, item.Value.Cost);
            }
        }
        public bool CanRunWithGraph(Graph g, out string message)
        {
            message = "All nodes must be full connected to find minimum spanning tree";
            return g.IsFullConnected;
        }
    }
}
