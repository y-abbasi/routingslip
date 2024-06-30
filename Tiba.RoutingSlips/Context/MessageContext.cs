namespace Tiba.RoutingSlips.Context;

public class MessageContext<T>
{
    public MessageContext(T message)
    {
        Message = message;
    }

    public T Message { get; }
}