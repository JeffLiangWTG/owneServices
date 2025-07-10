using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US Customs GUI")]
[assembly: AssemblyDescription("US Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
