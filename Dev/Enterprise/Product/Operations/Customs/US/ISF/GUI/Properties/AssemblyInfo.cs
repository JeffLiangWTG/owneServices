using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US ISF GUI")]
[assembly: AssemblyDescription("US ISF GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.ISF.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
