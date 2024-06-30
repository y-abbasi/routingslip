using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

public interface ICompensableWithdrawActivity : ICompensateActivity<WithdrawArgument, PortfolioTransactionLog>
{
}

public class CompensableWithdrawActivity : ICompensableWithdrawActivity
{
    public Task<IExecutionResult> Execute(IExecutionContext<WithdrawArgument> context)
    {
        Console.WriteLine(context.Arguments.Account);
        return Task.FromResult(context.CompletedWithLog(new PortfolioTransactionLog("REF-NO")));
    }

    public Task<ICompensateResult> Compensate(ICompensateContext<PortfolioTransactionLog> context)
    {
        Console.WriteLine(context.Arguments.ReferenceId);
        return Task.FromResult(context.Completed());
    }
}