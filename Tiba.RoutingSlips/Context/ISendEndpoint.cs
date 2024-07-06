using Tiba.Core;
using Tiba.Messaging.Contracts;
using Tiba.RoutingSlips.Activities;

namespace Tiba.RoutingSlips.Context;

public interface ISendEndpoint
{
    Task Send(RoutingSlip message);
}

public class InMemorySendEndpoint : ISendEndpoint
{
    private readonly IMessageService _messageService;
    private readonly IRequestContext _requestContext;

    public InMemorySendEndpoint(IMessageService messageService, IRequestContext requestContext)
    {
        _messageService = messageService;
        _requestContext = requestContext;
    }

    public async Task Send(RoutingSlip message)
    {
        await _messageService.SendLocal(message, _requestContext.GetObjects());
    }
}

public class NServiceBusSendEndpoint : ISendEndpoint
{
    private readonly IMessageService _messageService;
    private readonly IRoutingInfo _routingInfo;
    private readonly ICommandRoutingOverrideService _commandRoutingOverrideService;
    private readonly IHostInfoService _hostInfoService;
    private readonly IRequestContext _requestContext;

    public NServiceBusSendEndpoint(IMessageService messageService, IRoutingInfo routingInfo,
        ICommandRoutingOverrideService commandRoutingOverrideService,
        IHostInfoService hostInfoService,
        IRequestContext requestContext)
    {
        _messageService = messageService;
        _routingInfo = routingInfo;
        _commandRoutingOverrideService = commandRoutingOverrideService;
        _hostInfoService = hostInfoService;
        _requestContext = requestContext;
    }

    public async Task Send(RoutingSlip message)
    {
        var endpointName = _routingInfo.GetEndpointNameForBoundedContext(message.NextBcName());
        await _messageService.Send(message, endpointName, _requestContext.GetObjects())
            .ConfigureAwait(false);
    }
}