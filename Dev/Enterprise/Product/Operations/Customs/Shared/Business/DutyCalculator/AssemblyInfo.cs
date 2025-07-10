using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Duty Calculator")]
[assembly: AssemblyDescription("Customs Duty Calculator")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.DutyCalculator.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
