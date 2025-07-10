using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US Customs DIS Module")]
[assembly: AssemblyDescription("US Customs DIS Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.DIS.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
