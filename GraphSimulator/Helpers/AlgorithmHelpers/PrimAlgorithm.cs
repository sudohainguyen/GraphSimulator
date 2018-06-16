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
        public IEnumerable<Route> ExtractStepsWithResult(Node startNode)
        {
            Graph.Instance.Actions = new List<Action>();

            var set = Helper.InitSetForPrim(startNode.Identity, Graph.Instance.Nodes.Select(p => p.Key));
            var handleNodes = new List<char>();

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            Graph.Instance.Actions.Add(() =>
            {
                foreach (var item in Graph.Instance.Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
                {
                    item.RouteCost = int.MaxValue;
                }
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.IsSelected;
            });

            while (handleNodes.Count < Graph.Instance.Nodes.Count - 1)
            {
                var nodeToProcess = queue.Dequeue();
                
                Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);
                var neighbors = Graph.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.ArrowDirection == Direction.None));

                foreach (var item in neighbors)
                {
                    Graph.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);
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
                        if (set[curNode].PrevNode.Equals(node))
                            Graph.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsSelected);
                        else
                            Graph.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.None);
                        continue;
                    }

                    if (set[node].PrevNode.Equals('-'))
                    {
                        queue.Enqueue(node);
                        Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[node].NodeStatus = NodeStatus.IsInQueue);
                    }
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                    {
                        Graph.Instance.Actions.Add(() => Graph.Instance.Connections
                                                                       .FirstOrDefault(c => c.HasTwoVertices(curNode, node))
                                                                       .ConnectionStatus = ConnectionStatus.None);
                        continue;
                    }
                    var oldNode = set[node].PrevNode;
                    set[node] = (curNode, item.Cost);
                    Graph.Instance.Actions.Add(() =>
                    {
                        Graph.Instance.Nodes[node].RouteCost = item.Cost;
                        var con = Graph.Instance.Connections.FirstOrDefault(c => c.HasTwoVertices(node, oldNode));
                        if (con != null)
                            con.ConnectionStatus = ConnectionStatus.None;
                    });
                }
                handleNodes.Add(nodeToProcess);
                Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);

                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
                var min = queue.Peek();
                if (min != startNode.Identity)
                {
                    Graph.Instance.Actions.Add(() => Graph.Instance.Connections
                                                                               .FirstOrDefault(c => c.HasTwoVertices(min, set[min].PrevNode))
                                                                               .ConnectionStatus = ConnectionStatus.IsSelected);
                }
            }
            var lastNode = queue.Dequeue();
            Graph.Instance.Actions.Add(() => Graph.Instance.Nodes[lastNode].NodeStatus = NodeStatus.Processed);
            foreach (var item in set)
            {
                yield return new Route(item.Key, item.Value.PrevNode, item.Value.Cost);
            }
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
