using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Utilities;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public class NextActivityWithVariablesExecutionResult<TArguments> : CompletedExecutionResult<TArguments>
{
    private readonly IDictionary<string, object> _variables;

    public NextActivityWithVariablesExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip, IEnumerable<KeyValuePair<string, object>> variables) : base(context, activity, routingSlip)
    {
        _variables = variables.ToDictionary();
    }

    public NextActivityWithVariablesExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip, object variables) : base(context, activity, routingSlip)
    {
        _variables = variables.ToDictionary();
    }

    protected override void ConfigBuilder(RoutingSlipBuilder builder)
    {
        base.ConfigBuilder(builder);
        builder.AddVariables(_variables);
    }
}