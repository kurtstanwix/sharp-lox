using System;
using System.Collections.Generic;
using System.Linq;
using SharpLox.Expression;
using SharpLox.Statement;
using SharpLox.Tokens;

namespace SharpLox;

public class Parser
{
    private class ParseError : Exception { }
    
    private readonly IEnumerable<Token> _tokens;
    private int _current = 0;

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens;
    }

    public IEnumerable<IStmt>? Parse()
    {
        var statements = new List<IStmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private IStmt Declaration()
    {
        try
        {
            if (Match(TokenType.Fun)) return Function("function");
            if (Match(TokenType.Var)) return VarDeclaration();
            return Statement();
        }
        catch (ParseError)
        {
            Synchronise();
            return null;
        }
    }
    
    private IStmt Function(string kind)
    {
        var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
        Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RightParen))
        {
            if (parameters.Count >= 255)
            {
                Error(Peek(), "Can't have more than 255 arguments.");
            }

            do
            {
                parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        
        Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
        var body = Block();
        return new Function {Name = name, Params = parameters, Body = body };
    }

    private IStmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");
        var decl = new Var { Name = name};
        if (Match(TokenType.Equal))
        {
            decl.Initialiser = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return decl;
    }

    private IStmt Statement()
    {
        if (Match(TokenType.Print)) return Print();
        if (Match(TokenType.LeftBrace)) return new Block { Statements = Block() };
        if (Match(TokenType.If)) return If();
        if (Match(TokenType.While)) return While();
        if (Match(TokenType.Return)) return Return();
        if (Match(TokenType.For)) return For();
        return ExpressionStatement();
    }

    private IStmt Print()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Print { Expression = expr };
    }

    private IEnumerable<IStmt> Block()
    {
        var statements = new List<IStmt>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    private IStmt If()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");
        var thenBranch = Statement();
        var elseBranch = Match(TokenType.Else) ? Statement() : null;
        return new If { Condition = condition, ThenBranch = thenBranch, ElseBranch = elseBranch };
    }

    private IStmt While()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after while condition.");
        var body = Statement();
        return new While { Condition = condition, Body = body };
    }

    private IStmt Return()
    {
        var keyword = Previous();
        var value = Check(TokenType.Semicolon) ? null : Expression();
        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new Return { Keyword = keyword, Value = value };
    }

    private IStmt For()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");
        var initialiser = Match(TokenType.Semicolon) ? null :
            Match(TokenType.Var) ? VarDeclaration() : ExpressionStatement();
        var condition = !Check(TokenType.Semicolon) ? Expression() : null;
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
        var increment = !Check(TokenType.RightParen) ? Expression() : null;
        Consume(TokenType.RightParen, "Expect ')' after for clauses.");
        var body = Statement();
        
        if (increment is not null)
        {
            body = new Block
                { Statements = new List<IStmt> { body, new Statement.Expression { ExpressionValue = increment } } };
        }

        if (condition is null) condition = new Literal { Value = true };
        body = new While { Condition = condition, Body = body };
        
        if (initialiser is not null)
        {
            body = new Block
                { Statements = new List<IStmt> { initialiser, body } };
        }
        return body;
    }

    private IStmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Statement.Expression { ExpressionValue = expr };
    }

    private IExpr Expression()
    {
        return Comma();
    }

    private IExpr Comma()
    {
        var expr = TernaryOrAssignment();

        while (Match(TokenType.Comma))
        {
            var comma = Previous();
            var right = TernaryOrAssignment();
            expr = new Binary { Left = expr, Operator = comma, Right = right };
        }
        return expr;
    }

    private IExpr TernaryOrAssignment()
    {
        var expr = Or();

        if (Match(TokenType.Question))
        {
            var condOp = Previous();
            var left = Equality();
            var orOp = Consume(TokenType.Colon, "Ternary expression must have false case");
            var right = TernaryOrAssignment();
            expr = new Ternary
                { Left = expr, LeftOperator = condOp, Middle = left, RightOperator = orOp, Right = right };
        }
        else if (Match(TokenType.Equal))
        {
            var op = Previous();
            var value = TernaryOrAssignment();
            if (expr is Variable varExpr)
            {
                return new Assign { Name = varExpr.Name, Value = value };
            }

            Error(op, "Invalid assignment target.");
        }

        return expr;
    }

    private IExpr Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = And();
            expr = new Logical { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = Equality();
            expr = new Logical { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr Equality()
    {
        var expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Binary { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr Comparison()
    {
        var expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = Previous();
            var right = Term();
            expr = new Binary { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = Factor();
            expr = new Binary { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Star, TokenType.Slash))
        {
            var op = Previous();
            var right = Unary();
            expr = new Binary { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private IExpr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var op = Previous();
            var right = Unary();
            return new Unary { Operator = op, Right = right };
        }

        return Call();
    }

    private IExpr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen)) expr = FinishCall(expr);
            else break;
        }

        return expr;
    }

    private IExpr FinishCall(IExpr callee)
    {
        var arguments = new List<IExpr>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Equality());
            } while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

        return new Call { Callee = callee, Paren = paren, Arguments = arguments };
    }

    private IExpr Primary()
    {
        if (Match(TokenType.True)) return new Literal { Value = true };
        if (Match(TokenType.False)) return new Literal { Value = false };
        if (Match(TokenType.Nil)) return new Literal { Value = null };
        if (Match(TokenType.Number, TokenType.String))
        {
            return new Literal { Value = Previous().Literal };
        }
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression");
            return new Grouping { Expression = expr };
        }

        if (Match(TokenType.Identifier)) return new Variable { Name = Previous() };

        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string errMessage)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), errMessage);
    }

    private ParseError Error(Token token, string errMessage)
    {
        Program.Error(token, errMessage);
        return new ParseError();
    }

    private void Synchronise()
    {
        Advance();
        
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] types)
    {
        if (types.Any(Check))
        {
            Advance();
            return true;
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return type == _tokens.ElementAt(_current).Type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return _tokens.ElementAt(_current);
    }

    private Token Previous()
    {
        return _tokens.ElementAt(_current - 1);
    }
}
