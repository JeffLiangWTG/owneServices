using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs GUI")]
[assembly: AssemblyDescription("Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
