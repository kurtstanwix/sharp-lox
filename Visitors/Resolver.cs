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
        None, Function, Initialiser, Method,
    }

    private enum VariableState
    {
        Declared, Defined, Read,
    }

    private enum ClassType
    {
        None, Class,
    }

    private class Variable
    {
        public Token Name { get; init; }
        public VariableState State { get; set; }
    }
    
    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, Variable>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

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
        ResolveLocal(expr, expr.Name, true);
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

    public object? VisitGetExpr(Get expr)
    {
        Resolve(expr.Object);
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

    public object? VisitSetExpr(Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitThisExpr(This expr)
    {
        if (_currentClass == ClassType.None)
        {
            Program.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }
        ResolveLocal(expr, expr.Keyword, true);
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

    public object? VisitVariableExpr(Expression.Variable expr)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out var variable) &&
            variable.State == VariableState.Declared)
        {
            Program.Error(expr.Name, "Can't read local variable in its own initialiser");
        }

        ResolveLocal(expr, expr.Name, true);
        return null;
    }

    public object? VisitBlockStmt(Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitClassStmt(Class stmt)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;
        
        Declare(stmt.Name);
        Define(stmt.Name);
        
        BeginScope();
        var thisVar = new Token { Lexeme = "this" };
        Declare(thisVar);
        Define(thisVar);
        Read(thisVar);

        foreach (var method in stmt.Methods)
        {
            var declaration = FunctionType.Method;
            if (method.Name.Lexeme == "init") declaration = FunctionType.Initialiser;
            ResolveFunction(method.FunctionExpr, declaration);
        }

        EndScope();

        _currentClass = enclosingClass;
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
        if (stmt.Value is not null)
        {
            if (_currentFunction == FunctionType.Initialiser)
                Program.Error(stmt.Keyword, "Can't return a value from an initialiser.");

            Resolve(stmt.Value);
        }

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
        var unusedVars = _scopes.Pop().Where(val => val.Value.State != VariableState.Read);

        foreach (var unusedVar in unusedVars)
        {
            Program.Error(unusedVar.Value.Name, "Local variable is not used.");
        }
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;
        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.Lexeme)) Program.Error(name, "Already a variable with this name in this scope");
        scope[name.Lexeme] = new Variable { Name = name, State = VariableState.Declared };
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.Lexeme].State = VariableState.Defined;
    }

    private void Read(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.Lexeme].State = VariableState.Read;
    }

    private void ResolveLocal(IExpr expr, Token name, bool isRead)
    {
        var hops = 0;
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name.Lexeme, out var variable))
            {
                _interpreter.Resolve(expr, hops);

                if (isRead) variable.State = VariableState.Read;
                
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
