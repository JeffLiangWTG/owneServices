using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;

[assembly: AssemblyTitle("ACEManifest.Business")]
[assembly: AssemblyDescription("ACEManifest.Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.ACEManifest.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: ApplicationConfiguration("Enterprise.Customs.US.ACEManifest.Business", "Enterprise.Customs.US.ACEManifest.Business.USManifestEnterpriseApplicationConfiguration.xml")]
