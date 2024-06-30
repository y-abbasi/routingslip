using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Context.ExecutionResults;

public class FaultedExecutionResult<TArguments> : IExecutionResult
{
    private readonly IExecutionContext _context;
    private readonly RoutingSlipActivity _activity;
    private readonly RoutingSlip _routingSlip;
    private readonly Exception _exception;

    public FaultedExecutionResult(IExecutionContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip, Exception exception)
    {
        _context = context;
        _activity = activity;
        _routingSlip = routingSlip;
        _exception = exception;
    }

    public async Task Evaluate()
    {
        var builder = CreateRoutingSlipBuilder(_routingSlip);

        Build(builder);
        var routingSlip = builder.Build();
        if (HasCompensateStep(routingSlip))
        {
            var endpoint = await _context.GetSendEndpoint(routingSlip.GetCurrentCompensateExecuteAddress());
            await _context.Forward(endpoint, routingSlip);
        }
    }

    private bool HasCompensateStep(RoutingSlip routingSlip)
    {
        return routingSlip.CompensateLogs.Any();
    }

    public bool IsFaulted(out Exception exception)
    {
        exception = _exception;
        return true;
    }
    protected virtual void Build(RoutingSlipBuilder builder)
    {
        builder.AddException(_exception);
    }

    static RoutingSlipBuilder CreateRoutingSlipBuilder(RoutingSlip routingSlip)
    {
        return new RoutingSlipBuilder(routingSlip, routingSlip.RoutingSlipActivities, routingSlip.CompensateLogs);
    }
}