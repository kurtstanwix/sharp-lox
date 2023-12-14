using System.Collections.Generic;
using SharpLox.Callable;
using SharpLox.Visitors;

namespace SharpLox;

public class SharpLoxClass : ISharpLoxCallable
{
    public int Arity
    {
        get
        {
            var initialiser = FindMethod("init");
            if (initialiser is null) return 0;
            return initialiser.Arity;
        }
    }

    public string Name { get; init; }
    public Dictionary<string, SharpLoxFunction> Methods { private get; init; }

    public override string ToString()
    {
        return Name;
    }
    
    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        var instance = new SharpLoxInstance(this);
        var initialiser = FindMethod("init");
        if (initialiser is not null)
            initialiser.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    public SharpLoxFunction? FindMethod(string name)
    {
        return Methods[name];
    }
}
