using NSubstitute;
using Tiba.RoutingSlips.Activities;
using Tiba.RoutingSlips.Builders;
using Tiba.RoutingSlips.Context;
using Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

namespace Tiba.RoutingSlips.Tests.RoutingSlipExecutors;

public class RoutingSlipManager
{
    public RoutingSlipBuilder ScenarioOne() =>
        RoutingSlipBuilder.Default
            .AddActivity<WithdrawActivity>("sample", "Bc1", new WithdrawArgument("ACC-101", 200))
            .AddActivity<DepositActivity>("sample", "Bc1", new DepositArgument("ACC-101", 200))
        ;

    public RoutingSlipBuilder ScenarioPortfolio() =>
        RoutingSlipBuilder.Default
            .AddActivity<DebitActivity>("debit1", "Bc1", new DebitCommand("ACC1", 100))
            .AddActivity<DebitActivity>("debit2", "Bc1", new DebitCommand("ACC1", 950))
            .AddActivity<CreditActivity>("credit1", "Bc1", new CreditCommand("ACC2", 1050))
            .AddVariables(new {compMode = 1})
        ;
    public RoutingSlipBuilder ScenarioPortfolioExceptional() =>
        RoutingSlipBuilder.Default
            .AddActivity<DebitActivity>("debit1", "Bc1", new DebitCommand("ACC1", 100))
            .AddActivity<DebitActivity>("debit2", "Bc1", new DebitCommand("ACC1", 1050))
            .AddActivity<CreditActivity>("credit1", "Bc1", new CreditCommand("ACC2", 1150))
            .AddVariables(new {compMode = 1})
        ;

    public async Task Execute(RoutingSlip scenarioOne)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(WithdrawActivity)).Returns(new WithdrawActivity());
        serviceProvider.GetService(typeof(DepositActivity)).Returns(new DepositActivity());
        serviceProvider.GetService(typeof(DebitActivity)).Returns(new DebitActivity());
        serviceProvider.GetService(typeof(CreditActivity)).Returns(new CreditActivity());
        var handler = new RoutingSlipHandler(serviceProvider);
        await handler.Handle(new MessageContext<RoutingSlip>(scenarioOne));
    }
}