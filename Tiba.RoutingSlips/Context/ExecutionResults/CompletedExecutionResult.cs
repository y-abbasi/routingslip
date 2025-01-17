﻿using Tiba.Core;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context.Events;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public abstract class CompletedExecutionResult<TArguments> : IExecutionResult
{
    private readonly IExecutionContext _context;
    protected readonly RoutingSlipActivity Activity;
    private readonly RoutingSlip _routingSlip;

    protected CompletedExecutionResult(IExecutionContext context, RoutingSlipActivity activity, RoutingSlip routingSlip)
    {
        _context = context;
        Activity = activity;
        _routingSlip = routingSlip;
    }

    public async Task Evaluate()
    {
        var builder = CreateRoutingSlipBuilder(_routingSlip);
        ConfigBuilder(builder);
        var routingSlip = builder.Build();
        var commandHandlerContext = (ICommandHandlerContext?)_context.ServiceProvider.GetService(typeof(ICommandHandlerContext));
        PublishActivityExecutedEvent(commandHandlerContext);
       if (HasNextStep(routingSlip))
        {
            var endpoint = await _context.GetSendEndpoint(routingSlip.GetCurrentExecuteAddress());
            await _context.Forward(endpoint, routingSlip);
            return;
        }

        commandHandlerContext!.RequestContext.EventPublisher
            .Publish(new RoutingSlipCompleted(commandHandlerContext!.RequestContext.CorrelationId)
            {
                CommandId = commandHandlerContext!.RequestContext.CommandId
            });
    }

    protected abstract void PublishActivityExecutedEvent(ICommandHandlerContext? commandHandlerContext);

    protected virtual void ConfigBuilder(RoutingSlipBuilder builder)
    {
    }

    public virtual bool IsFaulted(out Exception exception)
    {
        exception = default;
        return false;
    }

    private bool HasNextStep(RoutingSlip routingSlip) => routingSlip.RoutingSlipActivities.Count > 0;

    static RoutingSlipBuilder CreateRoutingSlipBuilder(RoutingSlip routingSlip)
    {
        return new RoutingSlipBuilder(routingSlip, routingSlip.RoutingSlipActivities.RemoveAt(0));
    }
}