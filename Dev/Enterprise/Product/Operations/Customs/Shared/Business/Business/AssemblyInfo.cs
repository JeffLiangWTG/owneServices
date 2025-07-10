using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Business")]
[assembly: AssemblyDescription("Customs Business")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
