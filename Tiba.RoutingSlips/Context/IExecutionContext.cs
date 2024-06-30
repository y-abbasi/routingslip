using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context;

public interface IExecutionContext
{
    IServiceProvider ServiceProvider { get; }
    IExecutionResult Completed();
    IExecutionResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables);
    IExecutionResult CompletedWithVariable(object variables);
    IExecutionResult Faulted(Exception exception);
    IExecutionResult FaultedWithVariable(Exception exception, object variables);
    Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress);
}
public interface ICompensateContext
{
    ICompensateResult Completed();
    ICompensateResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables);
    ICompensateResult CompletedWithVariable(object variables);
    ICompensateResult Faulted(object variables);
    ICompensateResult FaultedWithVariable(object variables);
    Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress);
}