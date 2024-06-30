using Tiba.RoutingSlips.Activities;

namespace Tiba.RoutingSlips.Context;

public interface ISendEndpoint
{
    Task Send<T>(T message);
}

public class InMemorySendEndpoint : ISendEndpoint
{
    private IServiceProvider _serviceProvider;

    public InMemorySendEndpoint(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Send<T>(T message)
    {
        var handler = new RoutingSlipHandler(_serviceProvider);
        await handler.Handle(new MessageContext<RoutingSlip>(message as RoutingSlip));
    }
}