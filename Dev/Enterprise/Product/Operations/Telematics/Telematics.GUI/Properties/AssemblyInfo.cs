using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Telematics GUI")]
[assembly: AssemblyDescription("Telematics GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Telematics.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
