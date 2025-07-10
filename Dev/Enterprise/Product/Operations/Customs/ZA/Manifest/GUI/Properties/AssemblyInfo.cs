using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ZA.Manifest.GUI")]
[assembly: AssemblyDescription("ZA.Manifest.GUI")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ZA.Manifest.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
