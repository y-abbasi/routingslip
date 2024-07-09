using Tiba.Core;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Activities;

public interface IActivity{}
public interface IActivity<TArguments> : IActivity
{
    Task<IExecutionResult> Execute(IExecutionContext<TArguments> context);
}
public interface IGenericActivity<TArguments> : IActivity where TArguments : IMessage
{
    Task<IExecutionResult> Execute(IExecutionContext<TArguments> context);
}