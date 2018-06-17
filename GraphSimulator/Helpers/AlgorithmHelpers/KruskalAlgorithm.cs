using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSimulator.User_Controls;

namespace GraphSimulator.Helpers.AlgorithmHelpers
{
    public class KruskalAlgorithm : IAlgorithm
    {
        public List<string> Pseudocode =>
            new List<string>
            {
                "BEGIN",
                "  T ← ∅",
                "    queue ← sort {u, v} edges of E using l.",
                "    FOREACH v in G.V",
                "      make-tree(v);",
                "  WHILE handledEdges <  - 1 DO",
                "    {u, v} ← queue.extractMin()",
                "    IF !(T ∪ {{u, v}} has cycle)",
                "      T.add({u, v})",
                "      union(tree-of(u), tree-of(v))",
                "",
                "END"
            };

        public IEnumerable<Route> ExtractStepsWithResult(Node startNode)
        {
            var actions = Graph.Instance.Action = new List<Func<List<int>>>
            {
                () => new List<int>{ 0 }
            };
            var parents = new Dictionary<char, char>();
            var rank = new Dictionary<char, int>();
            foreach (var n in Graph.Instance.Nodes)
            {
                parents.Add(n.Key, n.Key);
                rank.Add(n.Key, 0);
            }
            var queue = new Queue<Connection>(Graph.Instance.Connections.OrderBy(c => c.Cost));
            var handledConnCount = 0;

            actions.Add(() => new List<int> { 1, 2, 3, 4 });

            while (handledConnCount < Graph.Instance.Nodes.Count - 1)
            {
                var con = queue.Dequeue();
                actions.Add(() =>
                {
                    con.ConnectionStatus = ConnectionStatus.IsInspecting;
                    return new List<int> { 6 };
                });
                var x = FindParent(parents, con.StartNode);
                var y = FindParent(parents, con.DestNode);

                if (x != y)
                {
                    Union(parents, rank, x, y);
                    actions.Add(() =>
                    {
                        con.ConnectionStatus = ConnectionStatus.IsSelected;
                        Graph.Instance.Nodes[x].NodeStatus = NodeStatus.Processed;
                        Graph.Instance.Nodes[y].NodeStatus = NodeStatus.Processed;
                        return new List<int> { 7, 8, 9 };
                    });
                    handledConnCount++;
                    yield return new Route(con);
                }
                else
                {
                    actions.Add(() =>
                    {
                        con.ConnectionStatus = ConnectionStatus.None;
                        return new List<int> { 10 };
                    });
                }
            }
            actions.Add(() => new List<int> { 11 });
        }

        public IEnumerable<Route> ShowResult(Node startNode)
        {
            var parents = new Dictionary<char, char>();
            var rank = new Dictionary<char, int>();
            foreach (var n in Graph.Instance.Nodes)
            {
                parents.Add(n.Key, n.Key);
                rank.Add(n.Key, 0);
            }
            var queue = new Queue<Connection>(Graph.Instance.Connections.OrderBy(c => c.Cost));
            var handledConnCount = 0;
            while (handledConnCount < Graph.Instance.Nodes.Count - 1)
            {
                var con = queue.Dequeue();
                var x = FindParent(parents, con.StartNode);
                var y = FindParent(parents, con.DestNode);

                if (x != y)
                {
                    Union(parents, rank, x, y);
                    con.ConnectionStatus = ConnectionStatus.IsSelected;
                    Graph.Instance.Nodes[x].NodeStatus = NodeStatus.IsSelected;
                    Graph.Instance.Nodes[y].NodeStatus = NodeStatus.IsSelected;
                    handledConnCount++;
                    yield return new Route(con);
                }
            }
        }

        private char FindParent(Dictionary<char,char> parents, char node)
        {
            if (parents[node].Equals(node))
                return node;
            return FindParent(parents, parents[node]);
        }

        private void Union(Dictionary<char, char> parents, Dictionary<char, int> rank, char x, char y)
        {
            var xp = FindParent(parents, x);
            var yp = FindParent(parents, y);

            if (rank[xp] < rank[yp])
            {
                parents[xp] = yp;
            }
            else if (rank[yp] < rank[xp])
            {
                parents[yp] = xp;
            }
            else
            {
                parents[yp] = xp;
                rank[xp]++;
            }
        }
        public bool CanRunWithGraph(Graph g, out string message)
        {
            message = "All nodes must be full connected to find minimum spanning tree";
            return Graph.Instance.IsFullConnected;
        }
    }
}
