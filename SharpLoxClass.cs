using System.Collections.Generic;
using SharpLox.Callable;
using SharpLox.Visitors;

namespace SharpLox;

public class SharpLoxClass : ISharpLoxCallable
{
    public int Arity => 0;
    public string Name { get; init; }
    public Dictionary<string, SharpLoxFunction> Methods { private get; init; }

    public override string ToString()
    {
        return Name;
    }
    
    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        var instance = new SharpLoxInstance(this);
        return instance;
    }

    public SharpLoxFunction? FindMethod(string name)
    {
        return Methods[name];
    }
}
