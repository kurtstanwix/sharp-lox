using System;
using System.IO;
using SharpLox.Expression.Visitors;

namespace SharpLox;

class Program
{
    private static bool HadError = false;
    
    static int Main(string[] args)
    {
        var expression = new Expression.Binary
        {
            Left = new Expression.Unary
            {
                Operator = new Tokens.Token { Type = Tokens.TokenType.Minus, Lexeme = "-", Literal = null, Line = 1 },
                Right = new Expression.Literal { Value = 123 },
            },
            Operator = new Tokens.Token { Type = Tokens.TokenType.Star, Lexeme = "*", Literal = null, Line = 1 },
            Right = new Expression.Grouping { Expression = new Expression.Literal { Value = 45.67 } }
        };
        Console.WriteLine(new AstPrinter().Print(expression));
        return 0;
        // if (args.Length > 1)
        // {
        //     Console.WriteLine(("UsageL sharplox [script]"));
        //     return 64;
        // }
        // if (args.Length == 1)
        // {
        //     return RunFile(args[0]);
        // }
        // return RunPrompt();
    }

    private static int RunFile(string path)
    {
        var fileContents = File.ReadAllText(Path.GetFullPath(path));
        Run(fileContents);

        if (HadError) return 65;
        
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
            HadError = false;
        }

        return 0;
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}
