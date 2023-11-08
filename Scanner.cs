using System.Collections.Generic;
using System.Linq;
using SharpLox.Tokens;

namespace SharpLox;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private int _start, _current, _line = 1;

    private readonly static Dictionary<string, TokenType> _keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While },
    };

    public Scanner(string source)
    {
        _source = source;
    }

    public IEnumerable<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token { Type = TokenType.Eof, Lexeme = "", Literal = null, Line = _line });
        return _tokens.ToList();
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ':':
                AddToken(TokenType.Colon);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '?':
                AddToken(TokenType.Question);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else if (Match('*'))
                {
                    BlockComment();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                _line++;
                break;
            case '"':
                String();
                break;

            default:
                if (IsDigit(c)) Number();
                else if (IsAlpha(c)) Identifier();
                else Program.Error(_line, "Unexpected character.");
                break;
        }
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Program.Error(_line, "Unterminated string literal");
            return;
        }

        AddToken(TokenType.String, _source.Substring(_start + 1, _current - _start - 1));
        Advance();
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.Number, double.Parse(_source.Substring(_start, _current - _start)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var token = _source.Substring(_start, _current - _start);
        AddToken(_keywords.TryGetValue(token, out var keyword) ? keyword : TokenType.Identifier);
    }

    private void BlockComment()
    {
        var nested = 0;
        var looping = true;
        while (looping)
        {
            if (IsStartBlockComment())
            {
                Advance();
                nested++;
            }
            else if (IsEndBlockComment())
            {
                Advance();
                looping = !(nested-- == 0);
            }
            else if (Peek() == '\n')
            {
                _line++;
            }
            else if (IsAtEnd())
            {
                Program.Error(_line, "Unterminated block comment");
                return;
            }

            Advance();
        }
    }

    private bool IsStartBlockComment()
    {
        return Peek() == '/' && PeekNext() == '*';
    }

    private bool IsEndBlockComment()
    {
        return Peek() == '*' && PeekNext() == '/';
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private bool IsAlpha(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object literal)
    {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token { Type = type, Lexeme = text, Literal = literal, Line = _line });
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }
}