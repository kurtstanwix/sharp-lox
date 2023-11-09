using System;

namespace SharpLox.Callable;

public class Return : Exception
{
    public object? Value { get; }

    public Return(object? value)
    {
        Value = value;
    }
}
