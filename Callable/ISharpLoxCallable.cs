using System.Collections.Generic;
using SharpLox.Expression.Visitors;

namespace SharpLox.Callable;

public interface ISharpLoxCallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, IEnumerable<object?> arguments);
}
