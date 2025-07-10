#if NETFRAMEWORK || NETSTANDARD2_0
/*
    This file adds support for the C# 9 feature "init" for .NET 4.8 which otherwise requires .NET 5 or later.
    Example: public string Name { get; init; }

    See: https://github.com/dotnet/roslyn/issues/45510

    It is an opt-in file. To enable it for a specific project, add the following to the <PropertyGroup> of the .csproj:
        <CWEnableIsExternalInitForNetFrameworkOnly>true</CWEnableIsExternalInitForNetFrameworkOnly>

    You will also need to add a reference to WTG.StaticAnalysis.Annotation assembly.
*/
namespace System.Runtime.CompilerServices
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used by compiler")]
	static class IsExternalInit { } // Suspends Error CS0518 Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
}
#endif