using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

public class DebitActivity : ICompensateActivity<DebitCommand, TransactionLog>
{
    public Task<IExecutionResult> Execute(IExecutionContext<DebitCommand> context)
    {
        Guid referenceId = Guid.NewGuid();
        Console.WriteLine(
            $"Debit {context.Arguments.Amount}$ from {context.Arguments.AccountNo}, referenceId is '{referenceId}'");
        if (context.Arguments.Amount > 1000)
            return Task.FromResult(context.Faulted(new Exception("Amount should be less than 1000")));
        return Task.FromResult(context.CompletedWithLog(new TransactionLog(referenceId, 
            "Debit",
            context.Arguments.Amount,
            context.Arguments.AccountNo)));
    }

    public Task<ICompensateResult> Compensate(ICompensateContext<TransactionLog> context)
    {
        if (context.Variables["compMode"]?.Equals(1) ?? false)
        {
        }

        Console.WriteLine($"rollback transaction by reference id: {context.Arguments.ReferenceId}");
        return Task.FromResult(context.Completed());
    }
}

public class CreditActivity : ICompensateActivity<CreditCommand, TransactionLog>
{
    public Task<IExecutionResult> Execute(IExecutionContext<CreditCommand> context)
    {
        Guid referenceId = Guid.NewGuid();
        Console.WriteLine(
            $"Credit {context.Arguments.Amount}$ from {context.Arguments.AccountNo}, referenceId is '{referenceId}'");
        return Task.FromResult(context.CompletedWithLog(new TransactionLog(referenceId, 
            "Credit",
            context.Arguments.Amount,
            context.Arguments.AccountNo)));
    }

    public Task<ICompensateResult> Compensate(ICompensateContext<TransactionLog> context)
    {
        Console.WriteLine($"rollback transaction by reference id: {context.Arguments.ReferenceId}");
        return Task.FromResult(context.Completed());
    }
}

public record TransactionLog(Guid ReferenceId, string Type, decimal Amount, string AccountNo);

public record DebitCommand(string AccountNo, decimal Amount);

public record CreditCommand(string AccountNo, decimal Amount);