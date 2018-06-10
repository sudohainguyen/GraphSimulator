namespace GraphSimulator.Helpers
{
    public enum RunningMode
    {
        ShowTheResult,
        StepByStep,
        Visualization
    }

    public enum Direction
    {
        None = 0,
        OneWay,
        OneWayReserved,
        TwoWay
    }

    public enum Operation
    {
        ADD,
        DELETE,
        UPDATE
    }

    public enum NodeStatus
    {
        None,
        IsSelected,
        IsInQueue,
        IsBeingProcessed,
        Processed
    }

    public enum ConnectionStatus
    {
        None,
        IsSelected,
        IsInspecting
    }

    public enum Algorithms
    {
        Dijsktra,
        BellmanFord,
        Prim,
        Kruskal
    }
}
