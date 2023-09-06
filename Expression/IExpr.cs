using SharpLox.Expression.Visitors;

namespace SharpLox.Expression;

public interface IExpr
{
    TReturn Accept<TReturn>(IExprVisitor<TReturn> visitor);
}
