using GraphSimulator.User_Controls;
using System.Collections.Generic;

namespace GraphSimulator.Helpers.AlgorithmHelpers
{
    public interface IAlgorithm
    {
        List<string> Pseudocode { get; }
        //List<string> Explaination { get; }
        IEnumerable<Route> ShowResult(Node startNode);
        IEnumerable<Route> ExtractStepsWithResult(Node startNode);
        bool CanRunWithGraph(Graph g, out string message);
    }
}
