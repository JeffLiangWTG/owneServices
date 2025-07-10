using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("VN.Manifest.Business")]
[assembly: AssemblyDescription("Vietnam Manifest Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.VN.Manifest.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: ApplicationConfiguration("Enterprise.Customs.VN.Manifest.Business", "Enterprise.Customs.VN.Manifest.Business.VNManifestEnterpriseApplicationConfiguration.xml")]
