using SharpLox.Tokens;
using SharpLox.Expression.Visitors;

namespace SharpLox.Expression;

public class Assign : IExpr
{
    public Token Name { get; set; }
    public IExpr Value { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitAssignExpr(this);
    }
}

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

public class Logical : IExpr
{
    public IExpr Left { get; set; }
    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitLogicalExpr(this);
    }
}

public class Ternary : IExpr
{
    public IExpr Left { get; set; }
    public Token LeftOperator { get; set; }
    public IExpr Middle { get; set; }
    public Token RightOperator { get; set; }
    public IExpr Right { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitTernaryExpr(this);
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

public class Variable : IExpr
{
    public Token Name { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitVariableExpr(this);
    }
}

