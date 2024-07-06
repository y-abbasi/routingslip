using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public class ActivityCompensateMessageHandler<TActivity, TArgument, TLog>
    where TActivity : ICompensateActivity<TArgument, TLog>
{
    private readonly IServiceProvider _serviceProvider;

    public ActivityCompensateMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(RoutingSlip routingSlip)
    {
        var routingSlipContext = CreateContext(routingSlip);
        if (_serviceProvider.GetService(routingSlip.GetNextCompensate().Activity.ActivityType) is TActivity activity)
        {
            var executionResult = await activity.Compensate(routingSlipContext);
            await executionResult.Evaluate();
        }
    }

    private ICompensateContext<TLog> CreateContext(RoutingSlip routingSlip)
    {
        return new CompensateContext<TLog>(routingSlip, routingSlip.Variables)
        {
            ServiceProvider = _serviceProvider,
        };
    }
}