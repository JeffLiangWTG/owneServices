using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US AMS Module")]
[assembly: AssemblyDescription("US AMS Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.AMS.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
