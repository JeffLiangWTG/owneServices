using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Universal")]
[assembly: AssemblyDescription("Customs Universal")]
[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Universal.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
