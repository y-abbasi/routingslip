namespace Tiba.RoutingSlips.Context.ExecutionResults;

public interface IExecutionResult
{
    Task Evaluate();

    bool IsFaulted(out Exception exception);
}