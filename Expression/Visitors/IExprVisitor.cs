namespace SharpLox.Expression.Visitors;

public interface IExprVisitor<TReturn>
{
    TReturn VisitTernaryExpr(Ternary expr);
    TReturn VisitBinaryExpr(Binary expr);
    TReturn VisitGroupingExpr(Grouping expr);
    TReturn VisitLiteralExpr(Literal expr);
    TReturn VisitUnaryExpr(Unary expr);
    TReturn VisitVariableExpr(Variable expr);
}
