using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace Tiba.RoutingSlips.Utilities;

public static class ConvertObject
{
    public static ImmutableDictionary<string, object> ToDictionary(this object values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var dictionary = new Dictionary<string, object>();

        IEnumerable<PropertyInfo> properties = MessageTypeCache
            .GetProperties(values.GetType())
            .Where(x => x.CanRead && x.GetMethod.IsPublic);

        foreach (var property in properties)
            AddPropertyToDictionary(property, values, dictionary);

        return dictionary.ToImmutableDictionary();
    }
    static void AddPropertyToDictionary(PropertyInfo property, object source, IDictionary<string, object> dictionary)
    {
        var value = property.GetValue(source);

        var key = property.Name;
        if (char.IsUpper(key[0]))
        {
            var chars = key.ToCharArray();
            chars[0] = char.ToLower(chars[0], CultureInfo.InvariantCulture);

            key = new string(chars);
        }

        dictionary.Add(key, value);
    }
}