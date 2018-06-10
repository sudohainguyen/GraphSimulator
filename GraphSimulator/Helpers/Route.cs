using GraphSimulator.User_Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GraphSimulator.Helpers
{
    public class Route : INotifyPropertyChanged
    {
        private List<Connection> _path = new List<Connection>();
        private char _previousNode;
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

        public char PreviousNode
        {
            get => _previousNode == '\0' ? (Paths.Count == 0 ? '-' : Paths.LastOrDefault().StartNode) : _previousNode;
            set => _previousNode = value;
        }

        public Route()
        {

        }

        public Route(char node, char prevNode, int routeCost)
        {
            DestNode = node;
            PreviousNode = prevNode;
            RouteCost = routeCost;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
