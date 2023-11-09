using System.Collections.Generic;

namespace SharpLox;

public static class Helpers
{
    public static TValue? Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value)) return value;
        return default;
    }
}
