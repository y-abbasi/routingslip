namespace Tiba.RoutingSlips;

public class RoutingSlipDispatcher : IRoutingSlipDispatcher
{
    private readonly IBus _bus;

    public RoutingSlipDispatcher(IBus bus)
    {
        _bus = bus;
    }
    public async Task Dispatch(RoutingSlip routingSlip)
    {
        await _bus.Send(routingSlip.GetCurrentExecuteAddress(), routingSlip);
    }
}