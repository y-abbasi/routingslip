using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

public interface IWithdrawActivity : IActivity<WithdrawArgument>{}
public class WithdrawActivity : IWithdrawActivity
{
    public Task<IExecutionResult> Execute(IExecutionContext<WithdrawArgument> context)
    {
        Console.WriteLine(context.Arguments.Account);
        return Task.FromResult(context.CompletedWithVariable(new { Result = true }));
    }
}public interface IDepositActivity : IActivity<DepositArgument>{}
public class DepositActivity : IDepositActivity
{
    public Task<IExecutionResult> Execute(IExecutionContext<DepositArgument> context)
    {
        Console.WriteLine(context.Arguments.Account);
        return Task.FromResult(context.CompletedWithVariable(new { Result = true }));
    }
}

public abstract class PortfolioArgument(string account, decimal amount)
{
    public string Account { get; set; } = account;
    public decimal Amount { get; set; } = amount;
}
public class WithdrawArgument(string account, decimal amount) : PortfolioArgument(account, amount)
{
}
public class DepositArgument(string account, decimal amount) : PortfolioArgument(account, amount)
{
}

public class PortfolioTransactionLog(string referenceId)
{
    public string ReferenceId { get; set; } = referenceId;
}