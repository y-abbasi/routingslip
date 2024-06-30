using Tiba.RoutingSlips.Context;

namespace Tiba.RoutingSlips.Activities;

public interface IMessageHandler<T>
{
    Task Handle(MessageContext<T> context);
}