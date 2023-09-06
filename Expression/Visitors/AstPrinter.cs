using System.Text;

namespace SharpLox.Expression.Visitors;

public class AstPrinter : IExprVisitor<string>
{
    public string Print(IExpr expr)
    {
        return expr.Accept(this);
    }
    
    public string VisitBinaryExpr(Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Literal expr)
    {
        if (expr is null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params IExpr[] exprs)
    {
        var builder = new StringBuilder();

        builder.Append("(").Append(name);
        foreach (var expr in exprs)
        {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }

        builder.Append(")");

        return builder.ToString();
    }
}