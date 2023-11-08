﻿using SharpLox.Expression;
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

    public IExpr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError error)
        {
            return null;
        }
    }

    private IExpr Expression()
    {
        var expr = Ternary();

        if (Match(TokenType.Comma))
        {
            var comma = Previous();
            var right = Expression();
            expr = new Binary { Left = expr, Operator = comma, Right = right };
        }
        return expr;
    }

    private IExpr Ternary()
    {
        var expr = Equality();

        if (Match(TokenType.Question))
        {
            var condOp = Previous();
            var left = Equality();
            var orOp = Consume(TokenType.Colon, "Ternary expression must have false case");
            var right = Ternary();
            expr = new Binary
                { Left = expr, Operator = condOp, Right = new Binary { Left = left, Operator = orOp, Right = right } };
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

        return Primary();
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