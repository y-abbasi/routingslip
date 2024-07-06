using System.Collections.Immutable;

namespace Tiba.RoutingSlips.Context;

public interface ICompensateContext<out TLog> : ICompensateContext
{
    public TLog Arguments { get; }
    ImmutableDictionary<string, object> Variables { get;  }
}