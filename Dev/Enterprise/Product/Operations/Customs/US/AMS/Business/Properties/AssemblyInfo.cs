using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US AMS Business")]
[assembly: AssemblyDescription("US AMS Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.AMS.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
