namespace SharpLox.Expression.Visitors;

public interface IExprVisitor<TReturn>
{
    TReturn VisitBinaryExpr(Binary expr);
    TReturn VisitGroupingExpr(Grouping expr);
    TReturn VisitLiteralExpr(Literal expr);
    TReturn VisitUnaryExpr(Unary expr);
}
