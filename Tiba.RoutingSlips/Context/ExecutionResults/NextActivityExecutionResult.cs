using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public class NextActivityExecutionResult<TArguments, TLog> :
    CompletedExecutionResult<TArguments>
    where TArguments : class
    where TLog : class
{
    private TLog _data;
    public NextActivityExecutionResult(IExecutionContext context, RoutingSlipActivity activity, RoutingSlip routingSlip, TLog data)
        : base(context, activity, routingSlip)
    {
        _data = data;
    }

    protected override void ConfigBuilder(RoutingSlipBuilder builder)
    {
        base.ConfigBuilder(builder);
        builder.AddCompensateLog(Activity, _data);
    }
}