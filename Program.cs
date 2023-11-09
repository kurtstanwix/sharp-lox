using System;
using System.IO;
using System.Linq;
using SharpLox.Errors;
using SharpLox.Tokens;
using SharpLox.Visitors;

namespace SharpLox;

class Program
{
    private static readonly Interpreter _interpreter = new Interpreter();
    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;
    
    static int Main(string[] args)
    {
        // var tokens = new List<Token>
        // {
        //     new Token { Type = TokenType.EqualEqual },
        //     new Token { Type = TokenType.BangEqual },
        //     new Token { Type = TokenType.GreaterEqual },
        //     new Token { Type = TokenType.LessEqual },
        //     new Token { Type = TokenType.Eof },
        // };
        //
        // var temp = new Parser(tokens).Expression();
        // return 0;
        
        
        // var expression = new Expression.Binary
        // {
        //     Left = new Expression.Unary
        //     {
        //         Operator = new Tokens.Token { Type = Tokens.TokenType.Minus, Lexeme = "-", Literal = null, Line = 1 },
        //         Right = new Expression.Literal { Value = 123 },
        //     },
        //     Operator = new Tokens.Token { Type = Tokens.TokenType.Star, Lexeme = "*", Literal = null, Line = 1 },
        //     Right = new Expression.Grouping { Expression = new Expression.Literal { Value = 45.67 } }
        // };
        // Console.WriteLine(new AstPrinter().Print(expression));
        // return 0;


        if (args.Length > 1)
        {
            Console.WriteLine(("UsageL sharplox [script]"));
            return 64;
        }
        if (args.Length == 1)
        {
            return RunFile(args[0]);
        }
        return RunPrompt();
    }

    private static int RunFile(string path)
    {
        var fileContents = File.ReadAllText(Path.GetFullPath(path));
        Run(fileContents);

        if (_hadError) return 65;
        if (_hadRuntimeError) return 70;
        
        return 0;
    }

    private static int RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null) break;
            Run(line);
            _hadError = false;
        }

        return 0;
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse()?.ToList();

        if (_hadError || statements is null) return;

        var resolver = new Resolver(_interpreter);
        resolver.Resolve(statements);
        
        if (_hadError) return;

        Console.WriteLine(new AstPrinter().Print(statements));
        _interpreter.Interpret(statements);
    }

    public static void RuntimeError(RuntimeError e)
    {
        Console.Error.WriteLine($"{e.Message}\n[line {e.Token.Line}]");
        _hadRuntimeError = true;
    }
    
    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, $" at '{token.Lexeme}'", message);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        _hadError = true;
    }
}
