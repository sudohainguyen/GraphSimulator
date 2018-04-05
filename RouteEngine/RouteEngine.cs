using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteEngine
{
    public class RouteEngine
    {
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public List<Node> Nodes { get; set; } = new List<Node>();

        public static char GenerateIdentifier(int pos)
        {
            return (char)(pos + 65);
        }
        
        public Dictionary<Node, Route> CalculateMinCost (Node startNode)
        {
            var shortestPaths = new Dictionary<Node, Route>();
            var handledNodes = new List<Node>();

            foreach (var node in Nodes)
            {
                shortestPaths.Add(node, new Route(node.Identifier));
            }

            shortestPaths[startNode].TotalCost = 0;

            while (handledNodes.Count != Nodes.Count)
            {
                var shortestNodes = shortestPaths.OrderBy(n => n.Value.TotalCost).Select(n => n.Key);

                Node nodeToProcess = null;

                foreach (var node in shortestNodes)
                {
                    if (!handledNodes.Contains(node))
                    {
                        if (shortestPaths[node].TotalCost.Equals(int.MaxValue))
                            return shortestPaths;
                        nodeToProcess = node;
                        break;
                    }
                }

                var selectedConnections = Connections.Where(c => c.A == nodeToProcess);

                foreach (var conn in selectedConnections)
                {
                    if (shortestPaths[conn.B].TotalCost > conn.Cost + shortestPaths[conn.A].TotalCost)
                    {
                        shortestPaths[conn.B].Connections = 
                            new List<Connection>(shortestPaths[conn.A].Connections)
                        {
                            conn
                        };
                        shortestPaths[conn.B].TotalCost = conn.Cost + shortestPaths[conn.A].TotalCost;
                    }
                }

                handledNodes.Add(nodeToProcess);
            }

            return shortestPaths;
        }
    }
}
