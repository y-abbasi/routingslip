using Tiba.Core;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Activities;

public class ActivityMessageHandler<TActivity, TArgument>
    where TActivity : IActivity<TArgument>
{
    private readonly IServiceProvider _serviceProvider;

    public ActivityMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(RoutingSlip routingSlip, IRequestContext requestContext)
    {
        var routingSlipContext = CreateContext(routingSlip, requestContext);

        if (_serviceProvider.GetService(routingSlip.GetCurrentActivity().ActivityType) is TActivity activity)
        {
            IExecutionResult executionResult = routingSlipContext.Faulted(null!);
            try
            {
                executionResult = await activity.Execute(routingSlipContext) ?? executionResult;
                await executionResult.Evaluate().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!executionResult!.IsFaulted(out var faultException) || faultException != e)
                    executionResult = routingSlipContext.Faulted(e);
                
                await executionResult.Evaluate().ConfigureAwait(false);
            }
        }
    }

    private IExecutionContext<TArgument> CreateContext(RoutingSlip routingSlip, IRequestContext requestContext)
    {
        return new ExecutionContext<TArgument>(routingSlip, routingSlip.Variables)
        {
            ServiceProvider = _serviceProvider,
            RequestContext = requestContext
        };
    }
}