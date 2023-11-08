using SharpLox.Expression;
using SharpLox.Tokens;
using SharpLox.Statement.Visitors;

namespace SharpLox.Statement;

public class Var : IStmt
{
    public Token Name { get; set; }
    public IExpr? Initialiser { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitVarStmt(this);
    }
}

public class Expression : IStmt
{
    public IExpr ExpressionValue { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}

public class Print : IStmt
{
    public IExpr Expression { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitPrintStmt(this);
    }
}

