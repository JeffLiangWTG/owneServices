using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ProductionRules.Business")]
[assembly: AssemblyDescription("Enterprise Production Rules")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ProductionRules.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
