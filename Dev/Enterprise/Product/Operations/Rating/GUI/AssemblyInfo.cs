using System.Reflection;

[assembly: AssemblyTitle("ediSalesManager and Rating User Interface Components")]
[assembly: AssemblyDescription("ediSalesManager")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.Rating.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.Rating.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
