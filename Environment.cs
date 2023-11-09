using System.Collections.Generic;
using SharpLox.Errors;
using SharpLox.Tokens;

namespace SharpLox;

public class Environment
{
    private readonly Environment? _enclosing;
    private readonly Dictionary<string, object?> _values = new();

    public Environment()
    {
        _enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value)) return value;
        if (_enclosing is not null) return _enclosing.Get(name);
        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)?._values.Get(name);
    }

    private Environment? Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
        {
            environment = environment?._enclosing;
        }

        return environment;
    }

    public object? AssignAt(int distance, Token name, object? value)
    {
        return Ancestor(distance)!._values[name.Lexeme] = value;
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (_enclosing is not null)
        {
            _enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
