using System.Collections.Immutable;
using FluentAssertions;
using Tiba.RoutingSlips.Builders;

namespace Tiba.RoutingSlips.Tests.RoutingSlipBuilders;

public class RoutingSlipBuilderTests
{
    [Fact]
    public void Create_RoutingSlip_HappyPath()
    {
        //Arrange
        var builder = RoutingSlipBuilder.Default
            .AddActivity<WithdrawActivity>("sample", new WithdrawArgument("ACC-2500", 100));

        //Act
        var routingSlip = builder.Build();

        //Assert
        routingSlip.Should().BeEquivalentTo(new RoutingSlip(ImmutableList<RoutingSlipActivity>.Empty.Add(
                new RoutingSlipActivity(typeof(WithdrawActivity), "sample",
                    new WithdrawArgument("ACC-2500", 100)){ EndpointName = "RoutingSlips"}),
            ImmutableDictionary<string, object>.Empty,
            ImmutableStack<CompensateLog>.Empty,
            null));
    }

    [Fact]
    public void Create_RoutingSlip_WithVariable_HappyPath()
    {
        //Arrange
        var builder = RoutingSlipBuilder.Default
            .AddActivity<IWithdrawActivity>("sample", "Bc1", new WithdrawArgument("ACC-2500", 100))
            .AddVariables(new { Url = "http://arka.net" });

        //Act
        var routingSlip = builder.Build();

        //Assert
        routingSlip.Should().BeEquivalentTo(new RoutingSlip(ImmutableList<RoutingSlipActivity>.Empty.Add(
                new RoutingSlipActivity(typeof(IWithdrawActivity), "sample", new WithdrawArgument("ACC-2500", 100))
                    { EndpointName = "Bc1" }),
            ImmutableDictionary<string, object>.Empty
                .Add("Url", "http://arka.net"),
            ImmutableStack<CompensateLog>.Empty,
            null));
    }
}