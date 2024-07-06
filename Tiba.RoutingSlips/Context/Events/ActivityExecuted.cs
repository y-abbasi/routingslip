using Tiba.Messaging.Contracts;
using Tiba.Messaging.Contracts.Messages;
using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Context.Events;

public class RoutingSlipEvent : BoundedContextNotifyMessage
{
    public RoutingSlipEvent(Guid correlationId) => this.CorrelationId = correlationId;
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
    public RoutingSlipFailed(Guid correlationId) : base(correlationId)
    {
    }
}