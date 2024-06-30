using System.Reflection;
using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public class RoutingSlipHandler : IMessageHandler<RoutingSlip>
{
    private readonly IServiceProvider _serviceProvider;

    public RoutingSlipHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(MessageContext<RoutingSlip> context)
    {
        var routingSlip = context.Message;
        if (routingSlip.IsInFaultState)
        {
            var log = routingSlip.GetNextCompensate();
            dynamic compensateActivityInstance = Activator.CreateInstance(
                typeof(ActivityCompensateMessageHandler<,,>)
                    .MakeGenericType(log.Activity.ActivityType, log.Activity.Arguments.GetType(), log.LogData.GetType()),
                _serviceProvider
            )!;
            await compensateActivityInstance.Handle(context);
            return;
        }
        var activity = routingSlip.GetCurrentActivity();
        dynamic activityInstance = Activator.CreateInstance(
            typeof(ActivityMessageHandler<,>)
                .MakeGenericType(activity.ActivityType, activity.Arguments.GetType()),
            _serviceProvider
        )!;
        await activityInstance.Handle(context);
    }
}