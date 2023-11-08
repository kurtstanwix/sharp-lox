using SharpLox.Errors;
using SharpLox.Tokens;

namespace SharpLox;

public class Environment
{
    private readonly Dictionary<string, object?> _values = new();

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value)) return value;
        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
