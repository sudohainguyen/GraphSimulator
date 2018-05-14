using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public List<Node> Nodes { get; set; } = new List<Node>();


        public Dictionary<char, Route> RunDijsktra(Node startNode)
        {
            var shortestPaths = Helper.InitResults(startNode.Identity, Nodes.Select(n => n.Identity), Connections);
            var queueProcessingNode = new Queue<char>();
            queueProcessingNode.Enqueue(startNode.Identity);

            while (queueProcessingNode.Count != 0)
            { 
                var nodeToProcess = queueProcessingNode.Dequeue();
                //Nodes.FirstOrDefault(n => n.Identity == nodeToProcess).IsProcessed = true;
                var neighbors = Connections
                    .Where(c => c.StartNode == nodeToProcess || (c.DestNode == nodeToProcess && c.IsTwoWay));

                foreach (var item in neighbors)
                {
                    var route = shortestPaths[item.DestNode];
                    if (route.RouteCost > item.Cost + shortestPaths[nodeToProcess].RouteCost)
                    {
                        route.Paths = new List<Connection>(shortestPaths[nodeToProcess].Paths)
                        {
                            item
                        };
                        route.RouteCost = item.Cost + shortestPaths[nodeToProcess].RouteCost;
                    }
                    queueProcessingNode.Enqueue(item.DestNode);
                }
            }

            return shortestPaths;
        }
    }

    public class Route : INotifyPropertyChanged
    {
        private List<Connection> _path = new List<Connection>();

        public char DestNode { get; set; }
        public int RouteCost { get; set; } = int.MaxValue;
        public List<Connection> Paths
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Paths));
            }
        }
        public char PreviousNode => Paths.Count == 0 ? '-' : Paths.LastOrDefault().StartNode;


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
