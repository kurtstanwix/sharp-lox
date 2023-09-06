using SharpLox.Tokens;
using SharpLox.Expression.Visitors;

namespace SharpLox.Expression;

public class Binary : IExpr
{
    public IExpr Left { get; set; }
    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }
}

public class Grouping : IExpr
{
    public IExpr Expression { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }
}

public class Literal : IExpr
{
    public object Value { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }
}

public class Unary : IExpr
{
    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }
}

