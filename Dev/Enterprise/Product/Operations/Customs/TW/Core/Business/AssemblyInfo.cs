using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("TW Customs Business")]
[assembly: AssemblyDescription("TW Customs Business")]
[assembly: InternalsVisibleTo("Enterprise.Customs.TW.GUI, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.TW.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.TW.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("DocumentWrappers.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
