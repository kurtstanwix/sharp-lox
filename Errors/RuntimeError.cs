using SharpLox.Tokens;

namespace SharpLox.Errors;

public class RuntimeError : Exception
{
    public Token Token { get; }
    
    public RuntimeError(Token token, string errMessage) : base(errMessage)
    {
        Token = token;
    }
}
