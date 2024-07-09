using Tiba.Core;
using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public class ActivityCompensateMessageHandler<TActivity, TLog>
    where TActivity : ICompensateActivity< TLog>
{
    private readonly IServiceProvider _serviceProvider;

    public ActivityCompensateMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(RoutingSlip routingSlip, IRequestContext requestContext)
    {
        var routingSlipContext = CreateContext(routingSlip, requestContext);
        if (_serviceProvider.GetService(routingSlip.GetNextCompensate().Activity.CompensateActivityType!) is TActivity activity)
        {
            var executionResult = await activity.Compensate(routingSlipContext);
            await executionResult.Evaluate();
        }
    }

    private ICompensateContext<TLog> CreateContext(RoutingSlip routingSlip, IRequestContext requestContext)
    {
        return new CompensateContext<TLog>(routingSlip, routingSlip.Variables)
        {
            ServiceProvider = _serviceProvider,
            RequestContext = requestContext
        };
    }
}