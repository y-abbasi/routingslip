using Tiba.Application.Contracts;
using Tiba.Core;
using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlip.Application;

public sealed class GenericActivity<TArguments> : IActivity<TArguments>
    where TArguments : IMessage
{
    private IGate gate;

    public GenericActivity(IGate gate)
    {
        this.gate = gate;
    }
    public async Task<IExecutionResult> Execute(IExecutionContext<TArguments> context)
    {
        var result = await gate.DispatchAsync(context.RequestContext, context.Arguments);
        return context.Completed();
    }
}

public sealed class GenericWithCompensateActivity<TArguments> : ICompensateActivity<TArguments, List<IEvent>> where TArguments : IMessage
{
    private IGate gate;

    public GenericWithCompensateActivity(IGate gate)
    {
        this.gate = gate;
    }

    public async Task<IExecutionResult> Execute(IExecutionContext<TArguments> context)
    {
        var result = await gate.DispatchAsync(context.RequestContext, context.Arguments);
        return context.CompletedWithLog(result);
    }

    public Task<ICompensateResult> Compensate(ICompensateContext<List<IEvent>> context)
    {
        throw new NotImplementedException();
    }
}