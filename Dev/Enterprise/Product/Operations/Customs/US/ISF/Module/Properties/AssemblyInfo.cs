using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US ISF Module")]
[assembly: AssemblyDescription("US ISF Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.ISF.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
