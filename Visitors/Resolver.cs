using System.Collections.Generic;
using System.Linq;
using SharpLox.Expression;
using SharpLox.Expression.Visitors;
using SharpLox.Statement;
using SharpLox.Statement.Visitors;
using SharpLox.Tokens;
using Function = SharpLox.Expression.Function;

namespace SharpLox.Visitors;

public class Resolver : IExprVisitor<object?>, IStmtVisitor<object?>
{
    private enum FunctionType
    {
        None, Function,
    }
    
    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;

    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public void Resolve(IEnumerable<IStmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }
    
    public object? VisitAssignExpr(Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(Call expr)
    {
        Resolve(expr.Callee);
        foreach (var arg in expr.Arguments)
        {
            Resolve(arg);
        }

        return null;
    }

    public object? VisitFunctionExpr(Function expr)
    {
        ResolveFunction(expr, FunctionType.Function);
        return null;
    }

    public object? VisitGroupingExpr(Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitTernaryExpr(Ternary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Middle);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitUnaryExpr(Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(Variable expr)
    {
        if (_scopes.Count != 0 && _scopes.Peek().Get(expr.Name.Lexeme) == false)
        {
            Program.Error(expr.Name, "Can't read local variable in its own initialiser");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBlockStmt(Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitExpressionStmt(Statement.Expression stmt)
    {
        Resolve(stmt.ExpressionValue);
        return null;
    }

    public object? VisitFunctionStmt(Statement.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt.FunctionExpr, FunctionType.Function);
        return null;
    }

    public object? VisitIfStmt(If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        Resolve(stmt.ElseBranch);
        return null;
    }

    public object? VisitPrintStmt(Print stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object? VisitReturnStmt(Return stmt)
    {
        if (_currentFunction == FunctionType.None) Program.Error(stmt.Keyword, "Can't return from top-level code.");
        Resolve(stmt.Value);
        return null;
    }

    public object? VisitVarStmt(Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initialiser is not null) Resolve(stmt.Initialiser);
        Define(stmt.Name);
        return null;
    }

    public object? VisitWhileStmt(While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    private void BeginScope()
    {
        _scopes.Push(new());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;
        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.Lexeme)) Program.Error(name, "Already a variable with this name in this scope");
        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(IExpr expr, Token name)
    {
        var hops = 0;
        foreach (var scope in _scopes.Reverse())
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, hops);
                return;
            }

            hops++;
        }
    }

    private void ResolveFunction(Function function, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;
        
        BeginScope();
        foreach (var param in function.Params)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();
        _currentFunction = enclosingFunction;
    }

    private void Resolve(IStmt? stmt)
    {
        stmt?.Accept(this);
    }

    private void Resolve(IExpr? expr)
    {
        expr?.Accept(this);
    }
}
