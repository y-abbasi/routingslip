namespace Tiba.RoutingSlips.Context;

public interface ICompensateContext<out TLog> : ICompensateContext
{
    public TLog Arguments { get; }
    Dictionary<string, object> Variables { get;  }
}