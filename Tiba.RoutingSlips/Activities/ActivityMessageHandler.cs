using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public class ActivityMessageHandler<TActivity, TArgument>
    where TActivity : IActivity<TArgument>
{
    private readonly IServiceProvider _serviceProvider;

    public ActivityMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(RoutingSlip routingSlip)
    {
        var routingSlipContext = CreateContext(routingSlip);

        if (_serviceProvider.GetService(routingSlip.GetCurrentActivity().ActivityType) is TActivity activity)
        {
            try
            {
                var executionResult = await activity.Execute(routingSlipContext);
                await executionResult.Evaluate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private IExecutionContext<TArgument> CreateContext(RoutingSlip routingSlip)
    {
        return new ExecutionContext<TArgument>(routingSlip, routingSlip.Variables)
            { ServiceProvider = _serviceProvider };
    }
}