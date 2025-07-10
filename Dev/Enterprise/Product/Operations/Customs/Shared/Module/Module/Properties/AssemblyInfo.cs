using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Module")]
[assembly: AssemblyDescription("Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
