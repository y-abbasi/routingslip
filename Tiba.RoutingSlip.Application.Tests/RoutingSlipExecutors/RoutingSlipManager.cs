using FluentAssertions;
using Newtonsoft.Json.Linq;
using NServiceBus;
using NSubstitute;
using Tiba.Application;
using Tiba.Application.Contracts;
using Tiba.Application.Contracts.SchemaValidators;
using Tiba.Core;
using Tiba.Core.Exceptions;
using Tiba.Messaging.Contracts;
using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.Events;
using IMessage = Tiba.Core.IMessage;

namespace Tiba.RoutingSlip.Application.Tests.RoutingSlipExecutors;

public class RoutingSlipManager
{
    private readonly ICommandHandlerContext _commandHandlerContext;
    private readonly IExceptionConvertorService _exceptionConvertorService;
    private readonly ICommandNotificationServiceFactory _commandNotificationServiceFactory;
    private readonly ICommandNotificationService _commandNotificationService;
    private readonly ISendEndpoint _sendEndPoint;
    private List<IMessage> _messages = new();
    private readonly RequestContext requestContext;

    public RoutingSlipManager()
    {
        _sendEndPoint = Substitute.For<ISendEndpoint>();
        _commandHandlerContext = Substitute.For<ICommandHandlerContext>();
        var correlationId = Guid.NewGuid();
        requestContext = new RequestContext()
        {
            CorrelationId = correlationId,
            CommandId = Guid.NewGuid()
        };
        _commandHandlerContext.RequestContext.Returns(requestContext);

        _exceptionConvertorService = Substitute.For<IExceptionConvertorService>();
        _exceptionConvertorService.Convert(Arg.Any<Exception>(), Arg.Any<string>())
            .Returns(JObject.Parse("{}"));
        _commandNotificationService = Substitute.For<ICommandNotificationService>();
        _commandNotificationServiceFactory = Substitute.For<ICommandNotificationServiceFactory>();
        _commandNotificationServiceFactory.Create(Arg.Any<IRequestContext>()).Returns(_commandNotificationService);
        Substitute.For<IGate>();
        Substitute.For<ISchemaValidator>();
        RegisterEventHandler();
    }

    private void RegisterEventHandler()
    {
        var aggregator = _commandHandlerContext.RequestContext.EventPublisher;
        aggregator.RegisterHandler(new DelegateHandler<IMessage>(msg => _messages.Add(msg)));
    }

    public IRoutingSlipBuilder ScenarioOne() =>
        RoutingSlipBuilder.Default
            .AddActivity<IDebitActivity>("sample", new DebitData("ACC-101", 200))
            .AddActivity<IActivity>("sample", new CreditData("ACC-101", 200));

    public IRoutingSlipBuilder ScenarioPortfolio() =>
        RoutingSlipBuilder.Default
            .AddActivity<IDebitActivity, IDebitData>("debit1", new DebitData("ACC1", 100))
            .AddActivity<IDebitActivity, IDebitData>("debit2", new DebitData("ACC1", 950))
            .AddActivity<ICreditActivity, ICreditData>("credit1", new CreditData("ACC2", 1050))
            .AddVariables(new { compMode = 1 });

    public IRoutingSlipBuilder ScenarioPortfolioExceptional() =>
        RoutingSlipBuilder.Default
            .AddDebitActivity("debit1", new DebitData("ACC1", 100))
            .AddDebitActivity("debit2", new DebitData("ACC1", 1050))
            .AddActivity<ICreditActivity>("credit1", new CreditData("ACC2", 1150))
            .AddVariables(new { compMode = 1 });

    public async Task Execute(RoutingSlips.RoutingSlip scenarioOne)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ICommandHandlerContext)).Returns(_commandHandlerContext);
        serviceProvider.GetService(typeof(IDebitActivity)).Returns(new DebitActivity());
        serviceProvider.GetService(typeof(ICreditActivity)).Returns(new CreditActivity());
        serviceProvider.GetService(typeof(DebitCompensateActivity)).Returns(new DebitCompensateActivity());
        serviceProvider.GetService(typeof(ISendEndpoint))
            .Returns(_sendEndPoint);

        var handler = new RoutingSlipHandler(_commandHandlerContext,
            _commandNotificationServiceFactory,
            serviceProvider,
            _exceptionConvertorService){RequestContext = requestContext };
        _sendEndPoint.WhenForAnyArgs(endpoint =>
                endpoint.Send(Arg.Any<RoutingSlips.RoutingSlip>()))
            .Do(info =>
            {
                handler.Handle((RoutingSlips.RoutingSlip)info.Args()[0], Substitute.For<IMessageHandlerContext>())
                    .GetAwaiter().GetResult();
            });

        await handler.Handle(scenarioOne, Substitute.For<IMessageHandlerContext>());
    }

    public void AssertRoutingSlipFailedMessageNotified()
    {
        _commandNotificationService.Received().NotifyExceptionToGateway(Arg.Any<BoundedContextNotifyMessage>());
    }

    public void AssertRoutingSlipCompletedMessageNotified()
    {
        _commandNotificationService.Received().NotifyToGateway(Arg.Is<BoundedContextNotifyMessage>(o =>
            o.GetEvents<RoutingSlipCompleted>().CorrelationId == _commandHandlerContext.RequestContext.CorrelationId &&
            o.GetEvents<RoutingSlipCompleted>().CommandId == _commandHandlerContext.RequestContext.CommandId));
    }

    public void AssertActivityExecutedEventPublished<T>(int count)
    {
        _messages.OfType<ActivityExecuted>().Count(o => 
                o.CorrelationId == _commandHandlerContext.RequestContext.CorrelationId &&
                o.Activity.ActivityType == typeof(T) &&
                o.CommandId == _commandHandlerContext.RequestContext.CommandId)
            .Should().Be(count);
    }

    public void AssertActivityExecutedEventPublishedWithLog<T, TLog>(int count, Func<TLog, bool> compairer)
    {
        _messages.OfType<ActivityExecuted>().Count(o =>
            o.CorrelationId == _commandHandlerContext.RequestContext.CorrelationId &&
            o.Activity.ActivityType == typeof(T) &&
            o.LogData is TLog &&
            compairer((TLog)o.LogData) &&
            o.CommandId == _commandHandlerContext.RequestContext.CommandId)
            .Should().Be(count);
    }  
    public void AssertEventPublished<TLog>(int count, Func<TLog, bool> compairer) where TLog : IMessage
    {
        _messages.OfType<TLog>().Count(o =>
            o.CorrelationId == _commandHandlerContext.RequestContext.CorrelationId &&
            compairer(o) &&
            o.CommandId == _commandHandlerContext.RequestContext.CommandId)
            .Should().Be(count);
    }

    public void AssertActivityFailedEventPublished<T, TE>()
    {
        _messages.OfType<ActivityFailed>().Count(o => 
            o.CorrelationId == _commandHandlerContext.RequestContext.CorrelationId &&
            o.Activity.ActivityType == typeof(T) &&
            o.Exception is TE &&
            o.CommandId == _commandHandlerContext.RequestContext.CommandId)
            .Should().BeGreaterThanOrEqualTo(1);
    }
}