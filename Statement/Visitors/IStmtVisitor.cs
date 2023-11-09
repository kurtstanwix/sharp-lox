namespace SharpLox.Statement.Visitors;

public interface IStmtVisitor<TReturn>
{
    TReturn VisitBlockStmt(Block stmt);
    TReturn VisitExpressionStmt(Expression stmt);
    TReturn VisitIfStmt(If stmt);
    TReturn VisitPrintStmt(Print stmt);
    TReturn VisitVarStmt(Var stmt);
}
