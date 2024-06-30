namespace Tiba.RoutingSlips.Context;

public static class ExecutionContextExtension
{
    public static Task Forward<T>(this IExecutionContext context, ISendEndpoint endpoint, T message)
    {
        return endpoint.Send(message);
    }

    public static Task Forward<T>(this ICompensateContext context, ISendEndpoint endpoint, T message)
    {
        return endpoint.Send(message);
    }
}