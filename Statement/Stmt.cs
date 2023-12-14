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

public class Class : IStmt
{
    public Token Name { get; set; }
    public IEnumerable<Function> Methods { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitClassStmt(this);
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

public class Function : IStmt
{
    public Token Name { get; set; }
    public SharpLox.Expression.Function FunctionExpr { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitFunctionStmt(this);
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

public class Return : IStmt
{
    public Token Keyword { get; set; }
    public IExpr? Value { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitReturnStmt(this);
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

public class While : IStmt
{
    public IExpr Condition { get; set; }
    public IStmt Body { get; set; }

    public TReturn Accept<TReturn>(IStmtVisitor<TReturn> visitor)
    {
        return visitor.VisitWhileStmt(this);
    }
}

