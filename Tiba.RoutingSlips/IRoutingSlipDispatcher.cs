namespace Tiba.RoutingSlips;

public interface IRoutingSlipDispatcher
{
    Task Dispatch(RoutingSlip routingSlip);
}