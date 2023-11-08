using System.Text;
using SharpLox.Statement;
using SharpLox.Statement.Visitors;

namespace SharpLox.Expression.Visitors;

public class AstPrinter : IExprVisitor<string>, IStmtVisitor<string>
{
    public string Print(IEnumerable<IStmt> statements)
    {
        var sb = new StringBuilder();
        foreach (var statement in statements)
        {
            sb.Append(statement.Accept(this));
        }

        return sb.ToString();
    }
    
    public string VisitTernaryExpr(Ternary expr)
    {
        return Parenthesize(expr.LeftOperator.Lexeme + expr.RightOperator.Lexeme, expr.Left, expr.Middle, expr.Right);
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
        if (expr.Value is null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Variable expr)
    {
        return expr.Name.Lexeme;
    }

    public string VisitVarStmt(Var stmt)
    {
        return $"var {stmt.Name.Lexeme}{(stmt.Initialiser is not null ? $" = {stmt.Initialiser.Accept(this)}" : "")};\n";
    }

    public string VisitExpressionStmt(Statement.Expression stmt)
    {
        return $"{stmt.ExpressionValue.Accept(this)};\n";
    }

    public string VisitPrintStmt(Print stmt)
    {
        return $"PRINT( {stmt.Expression.Accept(this)} );\n";
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