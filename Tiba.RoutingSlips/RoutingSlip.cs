using System.Collections.Immutable;
using Tiba.Core;
using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips;

public record RoutingSlip(
    ImmutableList<RoutingSlipActivity> RoutingSlipActivities,
    ImmutableDictionary<string, object> Variables,
    ImmutableStack<CompensateLog> CompensateLogs,
    Exception? Exception) : IMessage
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

    public string NextBcName()
    {
        if (IsInFaultState)
        {
            var a = CompensateLogs.Peek();
            return a.Activity.ActivityType.Assembly.GetName().Name!.Split('.').Skip(1).Take(1).First();
        }

        var currentItem = RoutingSlipActivities[0];
        return currentItem.ActivityType.Assembly.GetName().Name!.Split('.').Skip(1).Take(1).First();
    }

    public Guid CorrelationId { get; set; }
    public Guid CommandId { get; set; }
}