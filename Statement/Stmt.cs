using System.Collections.Generic;
using SharpLox.Expression;
using SharpLox.Tokens;
using SharpLox.Statement.Visitors;

namespace SharpLox.Statement;

public class Block : IStmt
{
    public IEnumerable<IStmt> Statements { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitBlockStmt(this);
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

public class If : IStmt
{
    public IExpr Condition { get; set; }
    public IStmt ThenBranch { get; set; }
    public IStmt? ElseBranch { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitIfStmt(this);
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

public class Var : IStmt
{
    public Token Name { get; set; }
    public IExpr? Initialiser { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitVarStmt(this);
    }
}

