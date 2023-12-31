﻿namespace SharpLox.Expression.Visitors;

public interface IExprVisitor<TReturn>
{
    TReturn VisitAssignExpr(Assign expr);
    TReturn VisitBinaryExpr(Binary expr);
    TReturn VisitCallExpr(Call expr);
    TReturn VisitGetExpr(Get expr);
    TReturn VisitFunctionExpr(Function expr);
    TReturn VisitGroupingExpr(Grouping expr);
    TReturn VisitLiteralExpr(Literal expr);
    TReturn VisitLogicalExpr(Logical expr);
    TReturn VisitSetExpr(Set expr);
    TReturn VisitThisExpr(This expr);
    TReturn VisitTernaryExpr(Ternary expr);
    TReturn VisitUnaryExpr(Unary expr);
    TReturn VisitVariableExpr(Variable expr);
}
