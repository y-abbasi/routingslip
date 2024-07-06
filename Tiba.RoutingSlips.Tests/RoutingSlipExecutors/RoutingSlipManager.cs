using NServiceBus;
using NSubstitute;
using Tiba.Application;
using Tiba.Application.Contracts;
using Tiba.Application.Contracts.SchemaValidators;
using Tiba.Application.WorkTranslators;
using Tiba.Core;
using Tiba.Core.Exceptions;
using Tiba.Domain.Model;
using Tiba.Messaging.Contracts;
using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.Events;
using Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

namespace Tiba.RoutingSlips.Tests.RoutingSlipExecutors;

public class RoutingSlipManager
{
    private readonly ICommandHandlerContext commandHandlerContext;
    private readonly Guid correlationId;
    private readonly IRequestContext requestContext;
    private readonly IExceptionConvertorService exceptionConvertorService;
    private ICommandNotificationService commandNotificationService;
    private readonly IMessageService _messageService = Substitute.For<IMessageService>();
    private readonly IGate gate;
    private readonly ISchemaValidator schemaValidator;
    private readonly ISendEndpoint sendEndPoint;

    public RoutingSlipManager()
    {
        sendEndPoint = Substitute.For<ISendEndpoint>();
        commandHandlerContext = Substitute.For<ICommandHandlerContext>();
        correlationId = Guid.NewGuid();
        requestContext = new RequestContext()
        {
            CorrelationId = correlationId,
        };
        commandHandlerContext.RequestContext.Returns(requestContext);

        exceptionConvertorService = Substitute.For<IExceptionConvertorService>();
        commandNotificationService = Substitute.For<ICommandNotificationService>();
        gate = Substitute.For<IGate>();
        schemaValidator = Substitute.For<ISchemaValidator>();
    }

    public RoutingSlipBuilder ScenarioOne() =>
        RoutingSlipBuilder.Default
            .AddActivity<WithdrawActivity>("sample", "Bc1", new WithdrawArgument("ACC-101", 200))
            .AddActivity<DepositActivity>("sample", "Bc1", new DepositArgument("ACC-101", 200));

    public RoutingSlipBuilder ScenarioPortfolio() =>
        RoutingSlipBuilder.Default
            .AddActivity<DebitActivity>("debit1", "Bc1", new DebitCommand("ACC1", 100))
            .AddActivity<DebitActivity>("debit2", "Bc1", new DebitCommand("ACC1", 950))
            .AddActivity<CreditActivity>("credit1", "Bc1", new CreditCommand("ACC2", 1050))
            .AddVariables(new { compMode = 1 });

    public RoutingSlipBuilder ScenarioPortfolioExceptional() =>
        RoutingSlipBuilder.Default
            .AddActivity<DebitActivity>("debit1", "Bc1", new DebitCommand("ACC1", 100))
            .AddActivity<DebitActivity>("debit2", "Bc1", new DebitCommand("ACC1", 1050))
            .AddActivity<CreditActivity>("credit1", "Bc1", new CreditCommand("ACC2", 1150))
            .AddVariables(new { compMode = 1 });

    public async Task Execute(RoutingSlip scenarioOne)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ICommandHandlerContext)).Returns(commandHandlerContext);
        serviceProvider.GetService(typeof(WithdrawActivity)).Returns(new WithdrawActivity());
        serviceProvider.GetService(typeof(DepositActivity)).Returns(new DepositActivity());
        serviceProvider.GetService(typeof(DebitActivity)).Returns(new DebitActivity());
        serviceProvider.GetService(typeof(CreditActivity)).Returns(new CreditActivity());
        serviceProvider.GetService(typeof(ISendEndpoint))
            .Returns(sendEndPoint);

        var handler = new RoutingSlipHandler(commandHandlerContext, commandNotificationService, serviceProvider);
        sendEndPoint.WhenForAnyArgs(endpoint =>
                endpoint.Send(Arg.Any<RoutingSlip>()))
            .Do(info =>
            {
                handler.Handle((RoutingSlip)info.Args()[0], Substitute.For<IMessageHandlerContext>())
                    .GetAwaiter().GetResult();
            });

        await handler.Handle(scenarioOne, Substitute.For<IMessageHandlerContext>());
    }

    public void AssertRoutingSlipFailedMessageNotified()
    {
        commandNotificationService.Received().NotifyToGateway(Arg.Any<RoutingSlipFailed>());
    }

    public void AssertRoutingSlipCompletedMessageNotified()
    {
        commandNotificationService.Received().NotifyToGateway(Arg.Is<RoutingSlipCompleted>(o =>
            o.CorrelationId == commandHandlerContext.RequestContext.CorrelationId &&
            o.CommandId == commandHandlerContext.RequestContext.CommandId));
    }

    public void AssertActivityExecutedEventPublished<T>(int count)
    {
        commandNotificationService.Received(count).NotifyToGateway(Arg.Is<ActivityExecuted>(o =>
            o.CorrelationId == commandHandlerContext.RequestContext.CorrelationId &&
            o.Activity.ActivityType == typeof(T) &&
            o.CommandId == commandHandlerContext.RequestContext.CommandId));
    }    public void AssertActivityExecutedEventPublishedWithLog<T, TLog>(int count, Func<TLog, bool> compairer)
    {
        commandNotificationService.Received(count).NotifyToGateway(Arg.Is<ActivityExecuted>(o =>
            o.CorrelationId == commandHandlerContext.RequestContext.CorrelationId &&
            o.Activity.ActivityType == typeof(T) &&
            o.LogData is TLog &&
            compairer((TLog) o.LogData) &&
            o.CommandId == commandHandlerContext.RequestContext.CommandId));
    }
    public void AssertActivityFailedEventPublished<T, TE>()
    {
        commandNotificationService.Received().NotifyToGateway(Arg.Is<ActivityFailed>(o =>
            o.CorrelationId == commandHandlerContext.RequestContext.CorrelationId &&
            o.Activity.ActivityType == typeof(T) &&
            o.Exception is TE &&
            o.CommandId == commandHandlerContext.RequestContext.CommandId));
    }
}