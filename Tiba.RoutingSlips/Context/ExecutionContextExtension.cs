namespace Tiba.RoutingSlips.Context;

public static class ExecutionContextExtension
{
    public static Task Forward(this IExecutionContext context, ISendEndpoint endpoint, RoutingSlip message)
    {
        return endpoint.Send(message);
    }

    public static Task Forward(this ICompensateContext context, ISendEndpoint endpoint, RoutingSlip message)
    {
        return endpoint.Send(message);
    }
}