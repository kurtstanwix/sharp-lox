using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLox.Expression;
using SharpLox.Expression.Visitors;
using SharpLox.Statement;
using SharpLox.Statement.Visitors;
using SharpLox.Tokens;

namespace SharpLox.Visitors;

public class AstPrinter : IExprVisitor<string>, IStmtVisitor<string>
{
    private int _level = 0;
    
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

    public string VisitAssignExpr(Assign expr)
    {
        return Parenthesize("=", expr.Name, expr.Value);
    }
    
    public string VisitBinaryExpr(Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }
    
    public string VisitCallExpr(Call expr)
    {
        return
            $"call {expr.Callee.Accept(this)} ({string.Join(", ", expr.Arguments.Select(arg => arg.Accept(this)))})";
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
    
    public string VisitLogicalExpr(Logical expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Variable expr)
    {
        return expr.Name.Lexeme;
    }

    public string VisitBlockStmt(Block stmt)
    {
        var sb = PrintBlock(stmt.Statements, new StringBuilder());

        return sb.ToString();
    }

    public string VisitIfStmt(If stmt)
    {
        var sb = new StringBuilder();
        sb.Append($"if ({stmt.Condition.Accept(this)})\n");
        var thenBranch = stmt.ThenBranch.Accept(this);
        if (stmt.ThenBranch is not Block)
        {
            _level++;
            thenBranch = Prepend(thenBranch);
            _level--;
        }

        sb.Append(thenBranch);

        if (stmt.ElseBranch is not null)
        {
            sb.Append("else\n");
            var elseBranch = stmt.ElseBranch.Accept(this);
            if (stmt.ElseBranch is not Block)
            {
                _level++;
                elseBranch = Prepend(elseBranch);
                _level--;
            }

            sb.Append(elseBranch);
        }

        return sb.ToString();
    }

    public string VisitFunctionStmt(Statement.Function stmt)
    {
        return PrintFunctionDefinition(stmt.FunctionExpr, stmt.Name.Lexeme);
    }

    public string VisitFunctionExpr(Expression.Function expr)
    {
        return PrintFunctionDefinition(expr);
    }

    public string VisitReturnStmt(Return stmt)
    {
        return $"{stmt.Keyword.Lexeme}{(stmt.Value is null ? "" : $" {stmt.Value?.Accept(this)}")};\n";
    }

    public string VisitWhileStmt(While stmt)
    {
        var sb = new StringBuilder();
        sb.Append($"while ({stmt.Condition.Accept(this)})\n");
        var body = stmt.Body.Accept(this);
        if (stmt.Body is not Block)
        {
            _level++;
            body = Prepend(body);
            _level--;
        }

        sb.Append(body);

        return sb.ToString();
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

    private string PrintFunctionDefinition(Expression.Function functionExpr, string? name = null)
    {
        var sb = new StringBuilder();
        sb.Append(
            $"fun {(name is null ? "" : $"{name} ")}({string.Join(", ", functionExpr.Params.Select(t => t.Lexeme))})\n");
        sb = PrintBlock(functionExpr.Body, sb);

        return sb.ToString();
    }

    private StringBuilder PrintBlock(IEnumerable<IStmt> statements, StringBuilder sb)
    {
        try
        {
            sb.Append(Prepend("{\n"));
            _level++;
            foreach (var statement in statements)
            {
                var stmtText = statement.Accept(this);
                if (statement is not Block) stmtText = Prepend(stmtText);
                sb.Append(stmtText);
            }
        }
        finally
        {
            _level--;
            sb.Append(Prepend("}\n"));
        }

        return sb;
    }

    private string Parenthesize(string name, params object[] values)
    {
        var builder = new StringBuilder();

        builder.Append("(").Append(name);
        foreach (var value in values)
        {
            builder.Append(" ");
            var val = value is IExpr exprValue ? exprValue.Accept(this) :
                value is Token tokenValue ? tokenValue.Lexeme : value;
            builder.Append(val);
        }

        builder.Append(")");

        return builder.ToString();
    }

    private string Prepend(string value)
    {
        return value.PadLeft(value.Length + _level * 2, ' ');
    }
}