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
    private readonly RoutingSlipActivity _activity;

    public ActivityExecuted(Guid correlationId, RoutingSlipActivity activity) : base(correlationId)
    {
        _activity = activity;
    }
}

public class ActivityFailed : RoutingSlipEvent
{
    public ActivityFailed(Guid correlationId) : base(correlationId)
    {
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