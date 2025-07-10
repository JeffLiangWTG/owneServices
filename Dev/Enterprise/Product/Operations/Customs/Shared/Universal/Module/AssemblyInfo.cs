using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Universal Moduel")]
[assembly: AssemblyDescription("Customs Universal Module")]
[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Universal.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
