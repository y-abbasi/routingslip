using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace Tiba.RoutingSlips.Utilities;

public static class MessageTypeCache
{
    private static ConcurrentDictionary<Type, ImmutableList<PropertyInfo>> CacheInstance = new();
    static IEnumerable<PropertyInfo> GetOrAdd(Type type)
    {
        return CacheInstance.GetOrAdd(type, _ => type.GetProperties().ToImmutableList());
    }

    public static IEnumerable<PropertyInfo> GetProperties(Type type)
    {
        return GetOrAdd(type);
    }
}