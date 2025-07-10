using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("TW Customs Module")]
[assembly: AssemblyDescription("TW Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.TW.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
