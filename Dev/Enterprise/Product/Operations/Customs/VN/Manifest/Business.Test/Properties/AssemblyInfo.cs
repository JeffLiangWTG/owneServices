using System.Reflection;
using Enterprise.Customs.VN.Manifest.Business;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("VN.Manifest.Business.Test")]
[assembly: AssemblyDescription("Vietnam Manifest Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.VietNam)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(VNCustomsDataRegistry), nameof(VNCustomsDataRegistry.EnableVNManifest))]
