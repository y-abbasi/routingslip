using Tiba.Core;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context.Events;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public class NextActivityExecutionResult<TArguments> : CompletedExecutionResult<TArguments>
{
    public NextActivityExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip) : base(context, activity, routingSlip)
    {
    }
    protected override void PublishActivityExecutedEvent(ICommandHandlerContext? commandHandlerContext)
    {
        commandHandlerContext!.RequestContext.EventPublisher
            .Publish(new ActivityExecuted(commandHandlerContext!.RequestContext.CorrelationId, Activity, null)
            {
                CommandId = commandHandlerContext!.RequestContext.CommandId
            });
    }
}

public class NextActivityWithLogExecutionResult<TArguments, TLog> : CompletedExecutionResult<TArguments>
    where TLog : class
{
    private readonly TLog _data;
    private readonly IDictionary<string, object> _variables;

    public NextActivityWithLogExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip, TLog data, IEnumerable<KeyValuePair<string, object>> variables) : base(context,
        activity, routingSlip)
    {
        _data = data;
        _variables = variables.ToDictionary(v => v.Key, v => v.Value);
    }

    protected override void ConfigBuilder(RoutingSlipBuilder builder)
    {
        base.ConfigBuilder(builder);
        builder.AddVariables(_variables);
        builder.AddCompensateLog(Activity, _data);
    }
    protected override void PublishActivityExecutedEvent(ICommandHandlerContext? commandHandlerContext)
    {
        commandHandlerContext!.RequestContext.EventPublisher
            .Publish(new ActivityExecuted(commandHandlerContext!.RequestContext.CorrelationId, Activity, _data)
            {
                CommandId = commandHandlerContext!.RequestContext.CommandId
            });
    }
}