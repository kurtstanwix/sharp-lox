using System.Collections.Generic;
using System.Linq;
using SharpLox.Expression;
using SharpLox.Tokens;
using SharpLox.Visitors;

namespace SharpLox.Callable;

public class SharpLoxFunction : ISharpLoxCallable
{
    public int Arity { get; }

    private readonly string _name;
    private readonly Function _declaration;
    private readonly Environment _closure;

    public SharpLoxFunction(string name, Function declaration, Environment closure)
    {
        _name = name;
        _declaration = declaration;
        _closure = closure;
        Arity = _declaration.Params.Count();
    }
    
    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        var environment = new Environment(_closure);

        foreach ((Token Param, object? Arg) paramArgs in _declaration.Params.Zip(arguments))
        {
            environment.Define(paramArgs.Param.Lexeme, paramArgs.Arg);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return ret)
        {
            return ret.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn{(_name is null ? "" : $" {_name}")}>";
    }
}
