using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExprBuilder;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1) {
            Console.Error.WriteLine("Usage: generate_ast <output directory> <tokens namespace>");
            Environment.Exit(64);
        }
        var outputDir = args[0];
        var namespaces = args.Skip(1);
        DefineAst(outputDir, "Expr", namespaces, new()
        {
            $"Binary   : IExpr Left, Token Operator, IExpr Right",
            $"Grouping : IExpr Expression",
            $"Literal  : object Value",
            $"Unary    : Token Operator, IExpr Right"
        });
    }

    private static void DefineAst(string dir, string name, IEnumerable<string> namespaces, List<string> definitions)
    {
        var interfaceName = $"I{name}";

        CreateInterface(dir, interfaceName);
        CreateTypes(dir, name, namespaces, definitions);
        CreateVisitorInterface(dir, name, definitions);
    }

    private static void CreateInterface(string dir, string interfaceName)
    {
        var filePath = Path.ChangeExtension(Path.Combine(dir, interfaceName), ".cs");
        var folderName = Path.GetFileName(dir);
        var rootName = Path.GetFileName(Path.GetFullPath(Path.Combine(dir, "..")));
        Directory.CreateDirectory(dir);
        var writer = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.UTF8);
        writer.WriteLine($"using {rootName}.{folderName}.Visitors;");
        writer.WriteLine();
        writer.WriteLine($"namespace {rootName}.{folderName};");
        writer.WriteLine();
        writer.WriteLine($"public interface {interfaceName}");
        writer.WriteLine("{");
        
        writer.WriteLine($"    TReturn Accept<TReturn>({interfaceName}Visitor<TReturn> visitor);");
        
        writer.WriteLine("}");
        writer.Dispose();
    }

    private static void CreateVisitorInterface(string dir, string name, IEnumerable<string> definitions)
    {
        var interfaceName = $"I{name}";
        var visitorName = $"{interfaceName}Visitor";
        var visitorPath = Path.Combine(dir, "Visitors");
        var filePath = Path.ChangeExtension(Path.Combine(visitorPath, visitorName), ".cs");
        var folderName = Path.GetFileName(dir);
        var rootName = Path.GetFileName(Path.GetFullPath(Path.Combine(dir, "..")));
        Directory.CreateDirectory(visitorPath);
        var writer = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.UTF8);
        writer.WriteLine($"namespace {rootName}.{folderName}.Visitors;");
        writer.WriteLine();
        writer.WriteLine($"public interface {visitorName}<TReturn>");
        writer.WriteLine("{");

        foreach (var type in definitions)
        {
            var typeName = type.Split(":")[0].Trim();
            writer.WriteLine($"    TReturn Visit{typeName}{name}({typeName} {name.ToLower()});");
        }

        writer.WriteLine("}");
        writer.Dispose();
    }
    
    private static void CreateTypes(string dir, string fileName, IEnumerable<string> namespaces,
        List<string> definitions)
    {
        var filePath = Path.ChangeExtension(Path.Combine(dir, fileName), ".cs");
        var folderName = Path.GetFileName(dir);
        var rootName = Path.GetFileName(Path.GetFullPath(Path.Combine(dir, "..")));
        var writer = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.UTF8);
        foreach (var usingNamespace in namespaces)
        {
            writer.WriteLine($"using {usingNamespace};");
        }
        writer.WriteLine($"using {rootName}.{folderName}.Visitors;");
        writer.WriteLine();
        writer.WriteLine($"namespace {rootName}.{folderName};");
        writer.WriteLine();
        foreach (var definition in definitions)
        {
            var className = definition.Split(':')[0].Trim();
            var properties = definition.Split(':')[1].Trim();
            DefineType(writer, fileName, className, properties);
        }
        writer.Dispose();
    }

    private static void DefineType(StreamWriter writer, string baseName, string className, string properties)
    {
        writer.WriteLine($"public class {className} : I{baseName}");
        writer.WriteLine( "{");

        var classProperties = properties.Split(", ");
        foreach (var property in classProperties)
        {
            var type = property.Split(' ')[0];
            var name = property.Split(' ')[1];
            writer.WriteLine($"    public {type} {name} {{ get; set; }}");
        }
        writer.WriteLine();
        writer.WriteLine($"    public TReturn Accept<TReturn>(I{baseName}Visitor<TReturn> visitor)");
        writer.WriteLine( "    {");
        writer.WriteLine($"        return visitor.Visit{className}{baseName}(this);");
        writer.WriteLine( "    }");
        
        writer.WriteLine( "}");
        writer.WriteLine();
    }
}
