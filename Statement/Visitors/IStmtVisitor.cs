namespace SharpLox.Statement.Visitors;

public interface IStmtVisitor<TReturn>
{
    TReturn VisitVarStmt(Var stmt);
    TReturn VisitExpressionStmt(Expression stmt);
    TReturn VisitPrintStmt(Print stmt);
}
