namespace Tiba.RoutingSlips;

public interface IBus
{
    Task Send(Uri endpoint, RoutingSlip routingSlip);
}