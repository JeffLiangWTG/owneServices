using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US Customs Module")]
[assembly: AssemblyDescription("US Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
