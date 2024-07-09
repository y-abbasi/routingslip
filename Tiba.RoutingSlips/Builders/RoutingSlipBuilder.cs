using System.Collections.Immutable;
using Tiba.Core;
using Tiba.RoutingSlips.Activities;

namespace Tiba.RoutingSlips.Builders;

public interface IRoutingSlipActivityBuilder
{
    IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, T arguments) where T : IMessage;
    IRoutingSlipWithCompensateBuilder AddActivity<T, TArg>(string name, TArg arguments) where T : IActivity<TArg>;
    IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, object arguments) where T : IActivity;
    IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, string endpointName, object arguments);
}

public interface IRoutingSlipBuilder : IRoutingSlipActivityBuilder
{
    RoutingSlip Build();
    IRoutingSlipBuilder AddVariables(object variables);
    IRoutingSlipBuilder AddVariables(IDictionary<string, object> variables);
    IRoutingSlipBuilder AddCompensateLog<TLog>(RoutingSlipActivity activity, TLog data) where TLog : class;
    IRoutingSlipBuilder AddException(Exception exception);
}

public interface IRoutingSlipWithCompensateBuilder : IRoutingSlipBuilder
{
    IRoutingSlipBuilder WithCompensate<T>();
}

public record RoutingSlipBuilder : IRoutingSlipBuilder, IRoutingSlipWithCompensateBuilder
{
    private RoutingSlipBuilder()
    {
    }

    public RoutingSlipBuilder(RoutingSlip routingSlip, ImmutableList<RoutingSlipActivity> routingSlipActivities)
    {
        RoutingSlipActivities = routingSlipActivities;
        Variables = routingSlip.Variables;
        _exception = routingSlip.Exception;
    }

    public RoutingSlipBuilder(RoutingSlip routingSlip, ImmutableList<RoutingSlipActivity> routingSlipActivities,
        ImmutableStack<CompensateLog> compensateLogs)
        : this(routingSlip, routingSlipActivities)
    {
        CompensateLogs = compensateLogs;
    }

    public static IRoutingSlipBuilder Default => new RoutingSlipBuilder();

    private ImmutableList<RoutingSlipActivity> RoutingSlipActivities { get; init; } =
        ImmutableList<RoutingSlipActivity>.Empty;

    private ImmutableDictionary<string, object> Variables = ImmutableDictionary<string, object>.Empty;
    private Exception? _exception;

    public IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, T arguments) where T : IMessage
    {
        return AddActivity<IGenericActivity<T>>(name, arguments);
    }

    public IRoutingSlipWithCompensateBuilder AddActivity<T, TArg>(string name, TArg arguments) where T : IActivity<TArg>
    {
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities.Add(new RoutingSlipActivity(typeof(T), name, arguments))
        };
    }

    public IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, object arguments) where T : IActivity
    {
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities.Add(new RoutingSlipActivity(typeof(T), name, arguments))
        };
    }

    public IRoutingSlipWithCompensateBuilder AddActivity<T>(string name, string endpointName, object arguments)
    {
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities
                .Add(new RoutingSlipActivity(typeof(T), name, arguments) { EndpointName = endpointName })
        };
    }

    public RoutingSlip Build() =>
        new RoutingSlip(RoutingSlipActivities, Variables, CompensateLogs, _exception);

    public IRoutingSlipBuilder AddVariables(object variables)
    {
        foreach (var property in variables.GetType().GetProperties())
        {
            Variables = Variables.Remove(property.Name)
                .Add(property.Name, property.GetValue(variables)!);
        }

        return this;
    }

    public IRoutingSlipBuilder AddVariables(IDictionary<string, object> variables)
    {
        foreach (var item in variables)
        {
            Variables = Variables.Remove(item.Key).Add(item.Key, item.Value);
        }

        return this;
    }

    public IRoutingSlipBuilder AddCompensateLog<TLog>(RoutingSlipActivity activity, TLog data) where TLog : class
    {
        CompensateLogs = CompensateLogs.Push(new CompensateLog(activity, data));
        return this;
    }

    public ImmutableStack<CompensateLog> CompensateLogs { get; private set; } = ImmutableStack<CompensateLog>.Empty;

    public IRoutingSlipBuilder AddException(Exception exception)
    {
        _exception = exception;
        return this;
    }

    IRoutingSlipBuilder IRoutingSlipWithCompensateBuilder.WithCompensate<T>()
    {
        var last = RoutingSlipActivities.Last() with { CompensateActivityType = typeof(T) };
        return this with
        {
            RoutingSlipActivities = RoutingSlipActivities.RemoveAt(RoutingSlipActivities.Count - 1).Add(last)
        };
    }
}