using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("NO Customs Module")]
[assembly: AssemblyDescription("NO Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.NO.Module.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
