using System;
using System.Collections.Generic;
using System.Linq;
using SharpLox.Callable;
using SharpLox.Errors;
using SharpLox.Expression;
using SharpLox.Expression.Visitors;
using SharpLox.Statement;
using SharpLox.Statement.Visitors;
using SharpLox.Tokens;

namespace SharpLox.Visitors;

public class Interpreter : IExprVisitor<object?>, IStmtVisitor<object?>
{
    private readonly Environment _globals = new();
    public Environment Globals => _globals;
    private Environment _environment;

    private readonly Dictionary<IExpr, int> _locals = new();

    public Interpreter()
    {
        _environment = _globals;
        _globals.Define("clock", new Clock());
    }
    
    public void Interpret(IEnumerable<IStmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Program.RuntimeError(e);
        }
    }
    
    public object? VisitTernaryExpr(Ternary expr)
    {
        var left = Evaluate(expr.Left);
        switch (expr.LeftOperator.Type)
        {
            case TokenType.Question:
            {
                if (IsTruthy(left)) return Evaluate(expr.Middle);
                return Evaluate(expr.Right);
            }
        }
        return null;
    }
    
    public object? VisitAssignExpr(Assign expr)
    {
        var value = Evaluate(expr.Value);

        if (_locals.TryGetValue(expr, out var distance))
        {
            _environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            Globals.Assign(expr.Name, value);
        }

        return value;
    }
    
    public object? VisitBinaryExpr(Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);
        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
            {
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! - (double)right!;
            }
            case TokenType.Slash:
            {
                CheckNumberOperands(expr.Operator, left, right);
                var res = (double)left! / (double)right!;
                if (double.IsInfinity(res))
                {
                    throw new RuntimeError(expr.Operator, "Divide by zero.");
                }

                return res;
            }
            case TokenType.Star:
            {
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! * (double)right!;
            }
            case TokenType.Plus:
            {
                if (left is double leftDouble && right is double rightDouble)
                {
                    return leftDouble + rightDouble;
                }
                if (left is string || right is string)
                {
                    return Stringify(left) + Stringify(right);
                }

                throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            }
            case TokenType.Greater:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! > (double)right!;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! >= (double)right!;
            case TokenType.Less:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! < (double)right!;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! <= (double)right!;
            
            case TokenType.BangEqual: return !IsEqual(left, right);
            case TokenType.EqualEqual: return IsEqual(left, right);
            case TokenType.Comma: return right;
        }
        return null;
    }

    public object? VisitCallExpr(Call expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = expr.Arguments.Select(Evaluate).ToList();

        if (callee is not ISharpLoxCallable function)
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        if (arguments.Count != function.Arity)
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
        return function.Call(this, arguments);
    }

    public object? VisitGetExpr(Get expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj is SharpLoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError(expr.Name, "Only instances have properties.");
    }

    public object? VisitFunctionExpr(Expression.Function expr)
    {
        return new SharpLoxFunction(null, expr, _environment);
    }

    public object? VisitGroupingExpr(Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Literal expr)
    {
        return expr.Value;
    }
    
    public object? VisitLogicalExpr(Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.Operator.Type == TokenType.And)
        {
            if (!IsTruthy(left)) return left;
        }
        else
        {
            if (IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }
    
    public object? VisitSetExpr(Set expr)
    {
        var obj = Evaluate(expr.Object);

        if (obj is not SharpLoxInstance instance)
        {
            throw new RuntimeError(expr.Name, "Only instances have fields.");
        }
        var value = Evaluate(expr.Value);
        instance.Set(expr.Name, value);

        return value;
    }

    public object? VisitUnaryExpr(Unary expr)
    {
        var right = Evaluate(expr.Right);
        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, right);
                return -(double) right;
            case TokenType.Bang:
                return !IsTruthy(right);
        }
        return null;
    }

    public object? VisitVariableExpr(Variable expr)
    {
        return LookupVariable(expr.Name, expr);
    }

    public object? VisitBlockStmt(Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public object? VisitClassStmt(Class stmt)
    {
        _environment.Define(stmt.Name.Lexeme, null);

        var methods = new Dictionary<string, SharpLoxFunction>();
        foreach (var method in stmt.Methods)
        {
            var func = new SharpLoxFunction(method.Name.Lexeme, method.FunctionExpr, _environment);
            methods[method.Name.Lexeme] = func;
        }
        
        var klass = new SharpLoxClass { Name = stmt.Name.Lexeme, Methods = methods };
        _environment.Assign(stmt.Name, klass);
        return null;
    }

    public object? VisitIfStmt(If stmt)
    {
        var condition = IsTruthy(Evaluate(stmt.Condition));
        Execute(condition ? stmt.ThenBranch : stmt.ElseBranch);
        return null;
    }

    public object? VisitFunctionStmt(Statement.Function stmt)
    {
        var fnName = stmt.Name.Lexeme;
        _environment.Define(fnName, new SharpLoxFunction(fnName, stmt.FunctionExpr, _environment));
        return null;
    }

    public object? VisitReturnStmt(Statement.Return stmt)
    {
        var value = Evaluate(stmt.Value);
        throw new Callable.Return(value);
    }

    public object? VisitWhileStmt(While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.Body);
        return null;
    }

    public object? VisitVarStmt(Var stmt)
    {
        var expr = stmt.Initialiser is not null ? Evaluate(stmt.Initialiser) : null;
        _environment.Define(stmt.Name.Lexeme, expr);
        return null;
    }

    public object? VisitExpressionStmt(Statement.Expression stmt)
    {
        Evaluate(stmt.ExpressionValue);
        return null;
    }

    public object? VisitPrintStmt(Print stmt)
    {
        var expr = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(expr));
        return null;
    }

    public void ExecuteBlock(IEnumerable<IStmt> statements, Environment environment)
    {
        var previousEnv = _environment;
        try
        {
            _environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previousEnv;
        }

    }

    public void Resolve(IExpr expr, int depth)
    {
        _locals[expr] = depth;
    }

    private object? LookupVariable(Token name, IExpr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            return _environment.GetAt(distance, name.Lexeme);
        }

        return Globals.Get(name);
    }

    private object? Evaluate(IExpr? expr)
    {
        return expr?.Accept(this);
    }

    private void Execute(IStmt? stmt)
    {
        stmt?.Accept(this);
    }

    private bool IsTruthy(object? obj)
    {
        if (obj is null) return false;
        if (obj is bool objBool) return objBool;
        return true;
    }

    private bool IsEqual(object? a, object? b)
    {
        if (a is null && b is null) return true;
        return a?.Equals(b) ?? false;
    }

    private void CheckNumberOperand(Token op, object? obj)
    {
        if (obj is double) return;
        throw new RuntimeError(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, object? left, object? right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(op, "Operands must be numbers.");
    }

    private string Stringify(object? obj)
    {
        if (obj is null) return "nil";
        if (obj is double objDouble)
        {
            var text = objDouble.ToString();
            if (text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
            return text;
        }

        return obj.ToString()!;
    }
}
