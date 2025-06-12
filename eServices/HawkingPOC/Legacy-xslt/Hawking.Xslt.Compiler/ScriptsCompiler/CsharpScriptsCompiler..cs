using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Hawking.Xslt.Compiler.ScriptsCompiler;
using Hawking.Xslt.Legacy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Hawking.Xslt.Compiler
{
    public class CsharpScriptsCompiler : ICsharpScriptsCompiler
    {
        public const string DefaultNamespace = "CargoWise.eHub.Core.Transforms.Scripts";
        public const string DefaultClassName = "Embedded";
        public const string DefaultTypeFullName = DefaultNamespace + "." + DefaultClassName;

        public readonly IList<string> UsingNamespaces = new List<string>
        {
            "System",
            "System.Collections.ObjectModel",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Xml",
            "System.Xml.Xsl",
            "System.Xml.XPath"
        };

        const string UsingFormat = "using {0};";

        const string OpeningStatementsFormat = @"
namespace {0}
{{
    public class {1}
    {{
";

        const string ClosingStatements = @"
    }
}
";

        public string CompiledClassTypeFullName { get; private set; }

        public CompilationEmit CompileCsharpCode(
            ref string csharpScripts,
            string classNamespace = DefaultNamespace,
            string className = DefaultClassName)
        {
            var csharpCodeBuilder = new StringBuilder();
            foreach (var usingNamespace in UsingNamespaces)
            {
                csharpCodeBuilder.AppendLine(string.Format(UsingFormat, usingNamespace));
            }

            csharpCodeBuilder.Append(string.Format(OpeningStatementsFormat, classNamespace, className));
            csharpCodeBuilder.Append(csharpScripts);
            csharpCodeBuilder.Append(ClosingStatements);

            csharpScripts = csharpCodeBuilder.ToString();
            CompiledClassTypeFullName = classNamespace + "." + className;

            return CompileCsharpCode(csharpScripts, className);
        }

        public CompilationEmit CompileCsharpCode(string csharpCode, string assemblyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);

            var coreDir = Directory.GetParent(typeof(Enumerable).GetTypeInfo().Assembly.Location);
            MetadataReference[] references =
            {
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Collection<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(XDocument).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(XPathNodeIterator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ArgumentException).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Uri).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Environment).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IXsltLegacy).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Debug));

            var emit = new CompilationEmit(csharpCode);

            var result = compilation.Emit(emit.DllMemoryStream, emit.PdbMemoryStream);
            if (result.Success)
            {
                emit.EmitSuccess = result.Success;
                emit.SeekOrigin();
            }
            else
            {
                var failures =
                    result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                var stringBuilder = new StringBuilder();
                foreach (var diagnostic in failures)
                {
                    stringBuilder.AppendFormat("{0}: {1}{2}", diagnostic.Id, diagnostic.GetMessage(), Environment.NewLine);
                }

                stringBuilder.AppendLine(csharpCode);

                throw new ApplicationException(stringBuilder.ToString());
            }

            return emit;
        }
    }
}
