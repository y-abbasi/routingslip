using FluentAssertions;
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
    }
}