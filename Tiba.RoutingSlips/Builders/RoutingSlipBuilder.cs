using System.Collections.Immutable;

namespace Tiba.RoutingSlips.Builders;

public record RoutingSlipBuilder
{
    private RoutingSlipBuilder()
    {
    }

    public RoutingSlipBuilder(RoutingSlip routingSlip, ImmutableList<RoutingSlipActivity> routingSlipActivities)
    {
        RoutingSlipActivities = routingSlipActivities;
        Variables = routingSlip.Variables;
    }

    public RoutingSlipBuilder(RoutingSlip routingSlip, ImmutableList<RoutingSlipActivity> routingSlipActivities,
        ImmutableStack<CompensateLog> compensateLogs)
        : this(routingSlip, routingSlipActivities)
    {
        CompensateLogs = compensateLogs;
    }

    public static RoutingSlipBuilder Default => new RoutingSlipBuilder();

    private ImmutableList<RoutingSlipActivity> RoutingSlipActivities { get; init; } =
        ImmutableList<RoutingSlipActivity>.Empty;

    private ImmutableDictionary<string, object> Variables = ImmutableDictionary<string, object>.Empty;
    private Exception _exception;

    public RoutingSlipBuilder AddActivity<T>(string name,  object arguments)
    {
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities.Add(new RoutingSlipActivity(typeof(T), name, arguments))
        };
    }
    public RoutingSlipBuilder AddActivity<T>(string name, string endpointName, object arguments)
    {
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities
                .Add(new RoutingSlipActivity(typeof(T), name, arguments){EndpointName = endpointName})
        };
    }

    public RoutingSlip Build() =>
        new RoutingSlip(RoutingSlipActivities, Variables, CompensateLogs, _exception);

    public RoutingSlipBuilder AddVariables(object variables)
    {
        foreach (var property in variables.GetType().GetProperties())
        {
            Variables = Variables.Remove(property.Name)
                .Add(property.Name, property.GetValue(variables)!);
        }

        return this;
    }

    public RoutingSlipBuilder AddVariables(IDictionary<string, object> variables)
    {
        foreach (var item in variables)
        {
            Variables = Variables.Remove(item.Key).Add(item.Key, item.Value);
        }

        return this;
    }

    public RoutingSlipBuilder AddCompensateLog<TLog>(RoutingSlipActivity activity, TLog data) where TLog : class
    {
        CompensateLogs = CompensateLogs.Push(new CompensateLog(activity, data));
        return this;
    }

    public ImmutableStack<CompensateLog> CompensateLogs { get; private set; } = ImmutableStack<CompensateLog>.Empty;

    public RoutingSlipBuilder AddException(Exception exception)
    {
        _exception = exception;
        return this;
    }
}