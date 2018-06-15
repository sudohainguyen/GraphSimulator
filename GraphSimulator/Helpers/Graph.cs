using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSimulator.Helpers
{
    public class Graph
    {
        private static Graph _instance;
        private static readonly object Padlock = new object();

        public static Graph Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance is null)
                        _instance = new Graph();
                }
                return _instance;
            }
        }


        public List<Connection> Connections { get; set; } = new List<Connection>();
        public Dictionary<char, Node> Nodes { get; set; } = new Dictionary<char, Node>();
        public List<Action> Actions { get; set; }

        public bool HasNegativeCost => Connections.Any(c => c.Cost < 0);
        public bool IsUndirected => Connections.All(c => c.ArrowDirection == Direction.None);
        public bool IsFullConnected => Connections.Select(c => c.StartNode)
                                                  .Union(Connections.Select(c => c.DestNode))
                                                  .Distinct().Count() == Nodes.Count && Nodes.Count != 0;

        public void ToUndirectedGraph()
        {
            foreach (var item in Connections)
            {
                item.ArrowDirection = Direction.None;
            }
        }
    }
}
