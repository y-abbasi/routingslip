using Tiba.RoutingSlips.Context.CompensateResults;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Context;

public class CompensateContext<TLog> : ICompensateContext<TLog>
{
    public RoutingSlip RoutingSlip { get; }
    public Dictionary<string, object> Variables { get; }

    public CompensateContext(RoutingSlip routingSlip, Dictionary<string,object> variables)
    {
        RoutingSlip = routingSlip;
        Variables = variables;
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