using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus;
using Tiba.Application;
using Tiba.Core;
using Tiba.Core.Exceptions;
using Tiba.Core.Extensions;
using Tiba.Core.Logging;
using Tiba.Messaging.Contracts;
using Tiba.RoutingSlips.Context.Events;

namespace Tiba.RoutingSlips.Activities;

public class RoutingSlipHandler : ICommunicationService, IService, IHandleMessages<RoutingSlip>,
    IRequestContextContainer
{
    private readonly ICommandHandlerContext commandHandlerContext;
    private readonly ICommandNotificationServiceFactory commandNotificationServiceFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IExceptionConvertorService exceptionConvertorService;
    private readonly ILogger logger;
    public IRequestContext RequestContext { get; set; }


    public RoutingSlipHandler(ICommandHandlerContext commandHandlerContext,
        ICommandNotificationServiceFactory commandNotificationServiceFactory,
        IServiceProvider serviceProvider,
        IExceptionConvertorService exceptionConvertorService)
    {
        this.commandHandlerContext = commandHandlerContext;
        _serviceProvider = serviceProvider;
        this.exceptionConvertorService = exceptionConvertorService;
        this.commandNotificationServiceFactory = commandNotificationServiceFactory;
    }


    public async Task Handle(RoutingSlip routingSlip, IMessageHandlerContext context)
    {
        var result = new List<Core.IMessage>();
        commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<Core.IMessage>(evt => result.Add(evt)));
        await RegisterEventHandlers(routingSlip);

        if (routingSlip.IsInFaultState)
        {
            await HandleCompensation(routingSlip);
            return;
        }

        await DoWork(routingSlip);
    }

    private async Task RegisterEventHandlers(RoutingSlip routingSlip)
    {
        var commandNotificationService = await commandNotificationServiceFactory.Create(RequestContext);
        var message = new BoundedContextMessage()
        {
            Command = routingSlip.ToString(),
            CommandId = commandHandlerContext.RequestContext.CommandId,
            CommandTypeName = routingSlip.GetType().GetFullName(),
            User = commandHandlerContext.RequestContext.User,
            CorrelationId = commandHandlerContext.RequestContext.CorrelationId,
            RepeatOnFailure = true,
            IsReplyNeeded = false
        };
        commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<RoutingSlipCompleted>(evt =>
            {
                var boundedContextNotifyMessage = new BoundedContextNotifyMessage(message, evt)
                {
                    Results = new List<object> { evt }
                };
                commandNotificationService.NotifyToGateway(boundedContextNotifyMessage);
            }));
        commandHandlerContext.RequestContext.EventPublisher.RegisterHandler(
            new DelegateHandler<RoutingSlipFailed>(evt =>
            {
                var boundedContextNotifyMessage =
                    new BoundedContextNotifyMessage(message, prepareExceptionData(message, evt.Exception));
                commandNotificationService.NotifyExceptionToGateway(boundedContextNotifyMessage);
            }));
    }

    private JObject prepareExceptionData(BoundedContextMessage message, Exception ex)
    {
        var exceptionData =
            exceptionConvertorService.Convert(ex.InnerException ?? ex, message.CorrelationId.ToString());
        if (exceptionData["type"]?.ToString() == "GeneralException")
        {
            logger?.LogCritical(
                $"#{{correlationId}}# Handling Command '{message.CommandTypeName}' Failed",
                x => x
                    .WithProperty("exceptionData", JsonConvert.SerializeObject(exceptionData))
                    .WithProperty("exceptionMessage", ex.ExtractFullMessage())
                    .WithProperty("stackTrace", ex.ExtractFullStackTrace()),
                message.CorrelationId
            );


            return exceptionData;
        }

        logger?.LogError(
            $"#{{correlationId}}# Handling Command '{message.CommandTypeName}' Failed",
            x => x
                .WithProperty("exceptionData", JsonConvert.SerializeObject(exceptionData))
                .WithProperty("exceptionMessage", ex.ExtractFullMessage())
                .WithProperty("stackTrace", ex.ExtractFullStackTrace()),
            message.CorrelationId
        );
        return exceptionData;
    }

    private async Task DoWork(RoutingSlip routingSlip)
    {
        var activity = routingSlip.GetCurrentActivity();
        dynamic activityInstance = Activator.CreateInstance(
            typeof(ActivityMessageHandler<,>)
                .MakeGenericType(activity.ActivityType,
                    activity.ActivityType
                        .GetInterfaces().First(t => t.GetGenericArguments().Count() == 1 && typeof(IActivity<>)
                            .MakeGenericType(t.GetGenericArguments())
                            .IsAssignableFrom(t)).GetGenericArguments().First()),
            _serviceProvider
        )!;
        await activityInstance.Handle(routingSlip, RequestContext);
    }

    private async Task HandleCompensation(RoutingSlip routingSlip)
    {
        var log = routingSlip.GetNextCompensate();
        Debug.Assert(log.Activity.CompensateActivityType is not null);
        
        dynamic compensateActivityInstance = Activator.CreateInstance(
            typeof(ActivityCompensateMessageHandler<,>)
                .MakeGenericType(log.Activity.CompensateActivityType,
                    log.LogData.GetType()),
            _serviceProvider
        )!;
        if (compensateActivityInstance != null)

            await compensateActivityInstance.Handle(routingSlip, RequestContext);
    }
}