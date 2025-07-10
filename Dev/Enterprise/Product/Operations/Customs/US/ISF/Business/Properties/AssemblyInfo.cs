using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US ISF Business")]
[assembly: AssemblyDescription("US ISF Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.ISF.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
