using System.Collections.Immutable;
using Tiba.Core;
using Tiba.RoutingSlips.Context.CompensateResults;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context;

public class CompensateContext<TLog> : ICompensateContext<TLog>
{
    public RoutingSlip RoutingSlip { get; }
    public IServiceProvider ServiceProvider { get; init; }
    public IRequestContext RequestContext { get; init; }
    public ImmutableDictionary<string, object> Variables { get; }

    public CompensateContext(RoutingSlip routingSlip, ImmutableDictionary<string,object> variables)
    {
        RoutingSlip = routingSlip;
        Variables = variables;
        Arguments = (TLog)routingSlip.CompensateLogs.Peek().LogData;
    }

    public ICompensateResult Completed()
    {
        return new CompletedCompensateResult(this, RoutingSlip.GetCurrentActivity(), RoutingSlip);
    }

    public ICompensateResult CompletedWithVariable(IEnumerable<KeyValuePair<string, object>> variables)
    {
        throw new NotImplementedException();
    }

    public ICompensateResult CompletedWithVariable(object variables)
    {
        throw new NotImplementedException();
    }

    public ICompensateResult Faulted(object variables)
    {
        throw new NotImplementedException();
    }

    public ICompensateResult FaultedWithVariable(object variables)
    {
        throw new NotImplementedException();
    }

    public Task<ISendEndpoint> GetSendEndpoint(Uri getNextExecuteAddress)
    {
        throw new NotImplementedException();
    }

    public TLog Arguments { get; set; }
}