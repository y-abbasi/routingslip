using System.Collections.Immutable;
using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips;

public record RoutingSlip(
    ImmutableList<RoutingSlipActivity> RoutingSlipActivities,
    Dictionary<string, object> Variables,
    ImmutableStack<CompensateLog> CompensateLogs,
    Exception? Exception)
{
    public RoutingSlip GetNext()
    {
        return this with { RoutingSlipActivities = RoutingSlipActivities.RemoveAt(0)};
    }

    public RoutingSlipActivity GetCurrentActivity()
    {
        return RoutingSlipActivities[0];
    }

    public Uri GetCurrentExecuteAddress()
    {
        return new Uri($"rabbitmq://{RoutingSlipActivities[0].EndpointName}") ;
    }
    public Uri GetCurrentCompensateExecuteAddress()
    {
        return new Uri($"rabbitmq://{GetNextCompensate().Activity.EndpointName}") ;
    }

    public bool IsInFaultState { get; init; } = Exception is not null;

    public CompensateLog GetNextCompensate()
    {
        return CompensateLogs.Peek();
    }
}