using Tiba.Core;
using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Context.ExecutionResults;

namespace Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

public static class RoutinSlipBuilderExtension
{
    public static IRoutingSlipBuilder AddDebitActivity(this IRoutingSlipBuilder builder, string name,
        IDebitData debitData)
    {
        return builder.AddActivity<IDebitActivity>(name, debitData)
            .WithCompensate<DebitCompensateActivity>();
    }

    public static IRoutingSlipBuilder AddCreditActivity(this RoutingSlipBuilder builder, string name,
        ICreditData debitData)
    {
        return builder.AddActivity<IDebitActivity>(name, debitData);
    }
}

public interface IDebitActivity : IActivity<IDebitData>
{
}

public class DebitActivity : IDebitActivity
{
    public Task<IExecutionResult> Execute(IExecutionContext<IDebitData> context)
    {
        Guid referenceId = Guid.NewGuid();
        Console.WriteLine(
            $"Debit {context.Arguments.Amount}$ from {context.Arguments.AccountNo}, referenceId is '{referenceId}'");
        if (context.Arguments.Amount > 1000)
            throw
                new Exception(
                    "Amount should be less than 1000"); // return Task.FromResult(context.Faulted(new Exception("Amount should be less than 1000")));
        return Task.FromResult(context.CompletedWithLog(new TransactionLog(referenceId,
            "Debit",
            context.Arguments.Amount,
            context.Arguments.AccountNo)));
    }
}

public class DebitCompensateActivity : ICompensateActivity<TransactionLog>
{
    public Task<ICompensateResult> Compensate(ICompensateContext<TransactionLog> context)
    {
        if (context.Variables["compMode"]?.Equals(1) ?? false)
        {
        }

        var log = context.Arguments;
        context.RequestContext.EventPublisher.Publish(new CreditData(log.AccountNo, log.Amount)
        {
            CorrelationId = context.RequestContext.CorrelationId,
            CommandId = context.RequestContext.CommandId
        });
        Console.WriteLine($"rollback transaction by reference id: {context.Arguments.ReferenceId}");
        return Task.FromResult(context.Completed());
    }
}

public interface ICreditActivity : ICompensateActivity<ICreditData, TransactionLog>
{
}

public class CreditActivity : ICreditActivity
{
    public Task<IExecutionResult> Execute(IExecutionContext<ICreditData> context)
    {
        var referenceId = Guid.NewGuid();
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

public interface IDebitData
{
    public string AccountNo { get; }
    public decimal Amount { get; }
}

public interface ICreditData
{
    public string AccountNo { get; }
    public decimal Amount { get; }
}

public record DebitData(string AccountNo, decimal Amount) : IDebitData, IMessage
{
    public Guid CorrelationId { get; set; }
    public Guid CommandId { get; set; }
}

public record CreditData(string AccountNo, decimal Amount) : ICreditData, IMessage
{
    public Guid CorrelationId { get; set; }
    public Guid CommandId { get; set; }
}