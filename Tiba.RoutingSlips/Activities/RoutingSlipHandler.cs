using System.Reflection;
using NServiceBus;
using Tiba.Core;
using Tiba.Core.EventAggregator;
using Tiba.Messaging.Contracts;
using Tiba.Messaging.Contracts.Messages;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.Events;

namespace Tiba.RoutingSlips.Activities;

public class RoutingSlipHandler : ICommunicationService, IService, IHandleMessages<RoutingSlip>,
    IRequestContextContainer
{
    private readonly ICommandNotificationService _commandNotificationService;
    private readonly IServiceProvider _serviceProvider;

    public RoutingSlipHandler(ICommandHandlerContext commandHandlerContext,
        ICommandNotificationService commandNotificationService,
        IServiceProvider serviceProvider)
    {
        _commandNotificationService = commandNotificationService;
        _serviceProvider = serviceProvider;
        commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<RoutingSlipFailed>(evt => 
                _commandNotificationService.NotifyToGateway(evt)));
        commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<RoutingSlipCompleted>(evt => 
                _commandNotificationService.NotifyToGateway(evt)));
       commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<ActivityExecuted>(evt => 
                _commandNotificationService.NotifyToGateway(evt)));
       commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<ActivityFailed>(evt => 
                _commandNotificationService.NotifyToGateway(evt)));
    }

    public async Task Handle(RoutingSlip routingSlip, IMessageHandlerContext context)
    {
        if (routingSlip.IsInFaultState)
        {
            var log = routingSlip.GetNextCompensate();
            dynamic compensateActivityInstance = Activator.CreateInstance(
                typeof(ActivityCompensateMessageHandler<,,>)
                    .MakeGenericType(log.Activity.ActivityType, log.Activity.Arguments.GetType(),
                        log.LogData.GetType()),
                _serviceProvider
            )!;

            await compensateActivityInstance.Handle(routingSlip);
            return;
        }

        var activity = routingSlip.GetCurrentActivity();
        dynamic activityInstance = Activator.CreateInstance(
            typeof(ActivityMessageHandler<,>)
                .MakeGenericType(activity.ActivityType, activity.Arguments.GetType()),
            _serviceProvider
        )!;
        await activityInstance.Handle(routingSlip);
    }

    public IRequestContext RequestContext { get; set; }
}