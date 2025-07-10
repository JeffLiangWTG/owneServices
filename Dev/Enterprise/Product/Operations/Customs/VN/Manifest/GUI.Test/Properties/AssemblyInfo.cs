using System.Reflection;
using Enterprise.Customs.VN.Manifest.Business;

[assembly: AssemblyTitle("VN.Manifest.GUI.Test")]
[assembly: AssemblyDescription("VN.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.VietNam)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(VNCustomsDataRegistry), nameof(VNCustomsDataRegistry.EnableVNManifest))]
