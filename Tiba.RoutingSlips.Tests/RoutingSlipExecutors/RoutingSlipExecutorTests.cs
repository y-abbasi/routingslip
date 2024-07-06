using FluentAssertions;
using Tiba.RoutingSlips.Tests.RoutingSlipBuilders;
using Xunit.Abstractions;

namespace Tiba.RoutingSlips.Tests.RoutingSlipExecutors;

public class RoutingSlipExecutorTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private RoutingSlipManager SutBuilder = new RoutingSlipManager();

    public RoutingSlipExecutorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task execute_with_()
    {
        //Arrange
        var routingSlip = SutBuilder.ScenarioOne().Build();
        //Act
        await SutBuilder.Execute(routingSlip);

        //Assert
    }

    [Fact]
    public async Task execute_portfolio_happy_path()
    {
        //Arrange
        var routingSlip = SutBuilder.ScenarioPortfolio().Build();

        //Act
        await SutBuilder.Execute(routingSlip);

        //Assert
        SutBuilder.AssertRoutingSlipCompletedMessageNotified();
        SutBuilder.AssertActivityExecutedEventPublished<DebitActivity>(2);
        SutBuilder.AssertActivityExecutedEventPublishedWithLog<DebitActivity, TransactionLog>(1,
            log => log.Type == "Debit" &&
                   log.Amount == 100 &&
                   log.AccountNo == "ACC1");
        SutBuilder.AssertActivityExecutedEventPublishedWithLog<DebitActivity, TransactionLog>(1,
            log => log.Type == "Debit" &&
                   log.Amount == 950 &&
                   log.AccountNo == "ACC1");
        SutBuilder.AssertActivityExecutedEventPublishedWithLog<CreditActivity, TransactionLog>(1,
            log => log.Type == "Credit" &&
                   log.Amount == 1050 &&
                   log.AccountNo == "ACC2");
    }

    [Fact]
    public async Task execute_portfolio_compensate()
    {
        //Arrange
        var routingSlip = SutBuilder.ScenarioPortfolioExceptional().Build();

        //Act
        await SutBuilder.Execute(routingSlip);

        //Assert
        SutBuilder.AssertRoutingSlipFailedMessageNotified();
        SutBuilder.AssertActivityExecutedEventPublished<DebitActivity>(1);
        SutBuilder.AssertActivityFailedEventPublished<DebitActivity, Exception>();
    }
}