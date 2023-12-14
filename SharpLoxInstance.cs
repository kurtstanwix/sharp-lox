using System.Collections.Generic;
using SharpLox.Callable;
using SharpLox.Errors;
using SharpLox.Tokens;
using SharpLox.Visitors;

namespace SharpLox;

public class SharpLoxInstance
{
    private readonly SharpLoxClass _class;
    private readonly Dictionary<string, object> _fields = new();

    public SharpLoxInstance(SharpLoxClass klass)
    {
        _class = klass;
    }

    public override string ToString()
    {
        return _class.Name + " instance";
    }

    public object Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }
        if (_class.FindMethod(name.Lexeme) is { } method)
        {
            return method;
        }

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
    }

    public void Set(Token name, object value)
    {
        _fields[name.Lexeme] = value;
    }
}
