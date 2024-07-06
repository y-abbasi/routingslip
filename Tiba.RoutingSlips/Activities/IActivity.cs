using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Activities;

public interface IActivity{}
public interface IActivity<TArguments> : IActivity
{
    Task<IExecutionResult> Execute(IExecutionContext<TArguments> context);
}