namespace Tiba.RoutingSlips.Context.ExecutionResults;

public interface ICompensateResult
{
    Task Evaluate();

    bool IsFaulted(out Exception exception);
}