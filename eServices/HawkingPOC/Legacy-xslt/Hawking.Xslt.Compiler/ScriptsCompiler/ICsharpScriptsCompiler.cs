using Hawking.Xslt.Compiler.ScriptsCompiler;

namespace Hawking.Xslt.Compiler
{
    public interface ICsharpScriptsCompiler
    {
        CompilationEmit CompileCsharpCode(ref string csharpScripts, string classNamespace, string className);
        CompilationEmit CompileCsharpCode(string csharpCode, string assemblyName);
        string CompiledClassTypeFullName { get; }
    }
}
