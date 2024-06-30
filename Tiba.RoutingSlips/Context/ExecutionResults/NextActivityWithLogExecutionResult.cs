using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public class NextActivityExecutionResult<TArguments> : CompletedExecutionResult<TArguments>
{
    public NextActivityExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip) : base(context, activity, routingSlip)
    {
    }
}

public class NextActivityWithLogExecutionResult<TArguments,TLog> : CompletedExecutionResult<TArguments> where TLog : class
{
    private readonly TLog _data;
    private readonly IDictionary<string, object> _variables;

    public NextActivityWithLogExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip, TLog data, IEnumerable<KeyValuePair<string, object>> variables) : base(context, activity, routingSlip)
    {
        _data = data;
        _variables = variables.ToDictionary();
    }
    
    protected override void ConfigBuilder(RoutingSlipBuilder builder)
    {
        base.ConfigBuilder(builder);
        builder.AddVariables(_variables);
        builder.AddCompensateLog(Activity, _data);
    }
}