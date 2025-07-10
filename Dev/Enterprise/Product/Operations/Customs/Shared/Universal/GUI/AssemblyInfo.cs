using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Universal GUI")]
[assembly: AssemblyDescription("Customs Universal GUI")]
[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Universal.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
