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
            RouteEngine.Instance.Actions = new List<Action>();

            var set = Helper.InitSetForPrim(startNode.Identity, RouteEngine.Instance.Nodes.Select(p => p.Key));
            var handleNodes = new List<char>();

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);

            RouteEngine.Instance.Actions.Add(() =>
            {
                foreach (var item in RouteEngine.Instance.Nodes.Where(p => p.Key != startNode.Identity).Select(p => p.Value))
                {
                    item.RouteCost = int.MaxValue;
                }
                startNode.RouteCost = 0;
                startNode.NodeStatus = NodeStatus.IsSelected;
            });

            while (handleNodes.Count < RouteEngine.Instance.Nodes.Count - 1)
            {
                var nodeToProcess = queue.Dequeue();
                
                RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.IsBeingProcessed);
                var neighbors = RouteEngine.Instance.Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.ArrowDirection == Direction.None));

                foreach (var item in neighbors)
                {
                    RouteEngine.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsInspecting);
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
                            RouteEngine.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.IsSelected);
                        else
                            RouteEngine.Instance.Actions.Add(() => item.ConnectionStatus = ConnectionStatus.None);
                        continue;
                    }

                    if (set[node].PrevNode.Equals('-'))
                    {
                        queue.Enqueue(node);
                        RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[node].NodeStatus = NodeStatus.IsInQueue);
                    }
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                    {
                        RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Connections
                                                                                   .FirstOrDefault(c => c.A(curNode, node))
                                                                                   .ConnectionStatus = ConnectionStatus.None);
                        continue;
                    }
                    var oldNode = set[node].PrevNode;
                    set[node] = (curNode, item.Cost);
                    RouteEngine.Instance.Actions.Add(() =>
                    {
                        RouteEngine.Instance.Nodes[node].RouteCost = item.Cost;
                        var con = RouteEngine.Instance.Connections.FirstOrDefault(c => c.A(node, oldNode));
                        if (con != null)
                            con.ConnectionStatus = ConnectionStatus.None;
                    });
                }
                handleNodes.Add(nodeToProcess);
                RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed);

                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
                var min = queue.Peek();
                if (min != startNode.Identity)
                {
                    RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Connections
                                                                               .FirstOrDefault(c => c.A(min, set[min].PrevNode))
                                                                               .ConnectionStatus = ConnectionStatus.IsSelected);
                }
            }
            RouteEngine.Instance.Actions.Add(() => RouteEngine.Instance.Nodes[queue.Dequeue()].NodeStatus = NodeStatus.Processed);
            foreach (var item in set)
            {
                yield return new Route(item.Key, item.Value.PrevNode, item.Value.Cost);
            }
        }

        public IEnumerable<Route> ShowResult(Node startNode)
        {
            var set = Helper.InitSetForPrim(startNode.Identity, RouteEngine.Instance.Nodes.Select(p => p.Key));

            var queue = new Queue<char>();
            queue.Enqueue(startNode.Identity);
            startNode.RouteCost = 0;
            var handledNodesCount = 0;
            while (handledNodesCount < RouteEngine.Instance.Nodes.Count - 1)
            {
                var nodeToProcess = queue.Dequeue();

                var neighbors = RouteEngine.Instance.Connections
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
                    if (RouteEngine.Instance.Nodes[node].Processed)
                        continue;

                    if (set[node].PrevNode.Equals('-'))
                        queue.Enqueue(node);
                    else if (set[node].Cost <= item.Cost || !queue.Contains(node))
                        continue;

                    set[node] = (curNode, item.Cost);
                    RouteEngine.Instance.Nodes[node].RouteCost = item.Cost;
                }
                RouteEngine.Instance.Nodes[nodeToProcess].NodeStatus = NodeStatus.Processed;
                handledNodesCount++;
                if (queue.Count > 1)
                    queue = new Queue<char>(set.Where(pair => queue.Contains(pair.Key))
                                                          .OrderBy(pair => pair.Value.Cost)
                                                          .Select(pair => pair.Key));
            }
            RouteEngine.Instance.Nodes[queue.Dequeue()].NodeStatus = NodeStatus.Processed;
            foreach (var item in RouteEngine.Instance.Nodes.Keys.Where(k => !k.Equals(startNode.Identity)))
            {
                RouteEngine.Instance.Connections.FirstOrDefault(c => c.A(item, set[item].PrevNode)).ConnectionStatus = ConnectionStatus.IsSelected;
            }
            foreach (var item in set)
            {
                yield return new Route(item.Key, item.Value.PrevNode, item.Value.Cost);
            }
        }
    }
}
