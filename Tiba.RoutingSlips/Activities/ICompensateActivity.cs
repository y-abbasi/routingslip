using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Activities;

public interface ICompensateActivity<TArguments, TLog> : IActivity<TArguments>
{
    Task<ICompensateResult> Compensate(ICompensateContext<TLog> context);
}