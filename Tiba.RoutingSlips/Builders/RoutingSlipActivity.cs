namespace Tiba.RoutingSlips.Builders;

public record RoutingSlipActivity(Type ActivityType, string ActivityName, object Arguments)
{
    public string EndpointName { get; init; } = ActivityType.Assembly.GetName().Name!.Split('.').Skip(1).Take(1).First();
    public Type? CompensateActivityType { get; init; }
}