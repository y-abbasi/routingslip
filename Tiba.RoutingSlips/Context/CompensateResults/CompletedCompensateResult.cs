using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context.CompensateResults;

public class CompletedCompensateResult : ICompensateResult
{
    private readonly ICompensateContext _context;
    protected readonly RoutingSlipActivity Activity;
    private readonly RoutingSlip _routingSlip;

    public CompletedCompensateResult(ICompensateContext context, RoutingSlipActivity activity,
        RoutingSlip routingSlip)
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
        if (HasNextStep(routingSlip))
        {
            var endpoint = await _context.GetSendEndpoint(routingSlip.GetCurrentExecuteAddress());
            await _context.Forward(endpoint, routingSlip);
        }
    }

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
        return new RoutingSlipBuilder(routingSlip, routingSlip.RoutingSlipActivities, routingSlip.CompensateLogs.Pop());
    }
}