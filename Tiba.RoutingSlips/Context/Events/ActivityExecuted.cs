using Tiba.Core;
using Tiba.Messaging.Contracts;
using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Context.Events;

public class RoutingSlipEvent : IEvent
{
    public RoutingSlipEvent(Guid correlationId) => this.CorrelationId = correlationId;
    public Guid CorrelationId { get; set; }
    public Guid CommandId { get; set; }
    public Guid EventId { get; set; }
}

public class ActivityExecuted : RoutingSlipEvent
{
    public RoutingSlipActivity Activity { get; }
    public object LogData { get; }

    public ActivityExecuted(Guid correlationId, RoutingSlipActivity activity, object LogData) : base(correlationId)
    {
        Activity = activity;
        this.LogData = LogData;
    }
}

public class ActivityFailed : RoutingSlipEvent
{
    public RoutingSlipActivity Activity { get; }
    public Exception Exception { get; }

    public ActivityFailed(Guid correlationId, RoutingSlipActivity activity, Exception exception) : base(correlationId)
    {
        Activity = activity;
        Exception = exception;
    }
}

public class RoutingSlipCompleted : RoutingSlipEvent
{
    public RoutingSlipCompleted(Guid correlationId) : base(correlationId)
    {
    }
}
public class RoutingSlipFailed : RoutingSlipEvent
{
    public Exception Exception { get; }
    public RoutingSlipFailed(Guid correlationId, Exception exception) : base(correlationId)
    {
        Exception = exception;
    }
}