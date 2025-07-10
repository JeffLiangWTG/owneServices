using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Telematics Business Module")]
[assembly: AssemblyDescription("Telematics Business Module")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Telematics.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
