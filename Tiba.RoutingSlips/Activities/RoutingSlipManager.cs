using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public interface IRoutingSlipManager
{
    Task Start(RoutingSlip routingSlip);
}
public class RoutingSlipManager : IRoutingSlipManager
{
    public ISendEndpoint SendEndpoint { get; }

    public RoutingSlipManager(ISendEndpoint sendEndpoint)
    {
        SendEndpoint = sendEndpoint;
    }
    public async Task Start(RoutingSlip routingSlip)
    {
        await SendEndpoint.Send(routingSlip);
    }
}