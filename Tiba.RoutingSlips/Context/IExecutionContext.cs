using Tiba.Core;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context;

public interface IExecutionContext
{
    string ActivityName { get; }
    IRequestContext RequestContext { get; }
    IServiceProvider ServiceProvider { get; }
    IExecutionResult Completed();
    IExecutionResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables);
    IExecutionResult CompletedWithVariable(object variables);
    IExecutionResult Faulted(Exception exception);
    IExecutionResult FaultedWithVariable(Exception exception, object variables);
    Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress);
}

public static class ServiceProviderContextExtensions
{
    public static ICommandHandlerContext GetCommandHandlerContext(this IServiceProvider provider) =>
        (ICommandHandlerContext)provider.GetService(typeof(ICommandHandlerContext))!;
}
public interface ICompensateContext
{
    IRequestContext RequestContext { get; }
    IServiceProvider ServiceProvider { get; }
    ICompensateResult Completed();
    ICompensateResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables);
    ICompensateResult CompletedWithVariable(object variables);
    ICompensateResult Faulted(object variables);
    ICompensateResult FaultedWithVariable(object variables);
    Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress);
}