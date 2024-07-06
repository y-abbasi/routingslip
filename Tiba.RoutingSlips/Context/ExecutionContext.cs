using System.Collections.Immutable;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context;

public interface IExecutionContext<out TArguments> : IExecutionContext
{
    public TArguments Arguments { get; }
    IExecutionResult CompletedWithLog<TLog>(TLog log) where TLog : class;
}

public class ExecutionContext<TArguments> : IExecutionContext<TArguments>
{
    public ExecutionContext(RoutingSlip routingSlip, ImmutableDictionary<string, object> variables)
    {
        RoutingSlip = routingSlip;
        Variables = variables;
    }

    private RoutingSlip RoutingSlip { get; }

    public ImmutableDictionary<string, object> Variables { get; }

    public IServiceProvider ServiceProvider { get; init; }

    public IExecutionResult Completed()
    {
        return new NextActivityExecutionResult<TArguments>(this, RoutingSlip.GetCurrentActivity(), RoutingSlip);
    }

    public IExecutionResult CompletedWithLog<TLog>(TLog log) where TLog : class
    {
        return new NextActivityWithLogExecutionResult<TArguments, TLog>(this, RoutingSlip.GetCurrentActivity(),
            RoutingSlip, log, Array.Empty<KeyValuePair<string, object>>());
    }

    public IExecutionResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables)
    {
        return new NextActivityWithVariablesExecutionResult<TArguments>(this, RoutingSlip.GetCurrentActivity(),
            RoutingSlip, variables);
    }

    public IExecutionResult CompletedWithVariable(object variables)
    {
        return new NextActivityWithVariablesExecutionResult<TArguments>(this, RoutingSlip.GetCurrentActivity(),
            RoutingSlip, variables);
    }

    public IExecutionResult Faulted(Exception exception)
    {
        return new FaultedExecutionResult<TArguments>(this, RoutingSlip.GetCurrentActivity(), RoutingSlip, exception);
    }

    public IExecutionResult FaultedWithVariable(Exception exception, object variables)
    {
        throw new NotImplementedException();
    }

    public Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress)
    {
        return Task.FromResult(
            (ISendEndpoint)ServiceProvider.GetService(typeof(ISendEndpoint))!);
    }
    
    public TArguments Arguments => (TArguments)RoutingSlip.GetCurrentActivity().Arguments;
}