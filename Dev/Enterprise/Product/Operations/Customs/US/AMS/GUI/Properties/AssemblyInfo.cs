using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US AMS GUI")]
[assembly: AssemblyDescription("US AMS GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.AMS.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
