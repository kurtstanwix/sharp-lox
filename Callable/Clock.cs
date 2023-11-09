using System;
using System.Collections.Generic;
using SharpLox.Visitors;

namespace SharpLox.Callable;

public class Clock : ISharpLoxCallable
{
    public int Arity => 0;

    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        return (double) DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
