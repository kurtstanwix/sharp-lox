using System.Collections.Generic;
using SharpLox.Statement;
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

public class Call : IExpr
{
    public IExpr Callee { get; set; }
    public Token Paren { get; set; }
    public IEnumerable<IExpr> Arguments { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitCallExpr(this);
    }
}

public class Get : IExpr
{
    public IExpr Object { get; set; }
    public Token Name { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitGetExpr(this);
    }
}

public class Function : IExpr
{
    public IEnumerable<Token> Params { get; set; }
    public IEnumerable<IStmt> Body { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitFunctionExpr(this);
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

public class Set : IExpr
{
    public IExpr Object { get; set; }
    public Token Name { get; set; }
    public IExpr Value { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitSetExpr(this);
    }
}

public class This : IExpr
{
    public Token Keyword { get; set; }

    public TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor)
    {
        return visitor.VisitThisExpr(this);
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

