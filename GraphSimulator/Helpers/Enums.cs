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
        DELETE
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
}
