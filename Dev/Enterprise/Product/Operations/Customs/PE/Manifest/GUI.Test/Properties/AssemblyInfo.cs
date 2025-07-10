using System.Reflection;
using Enterprise.Customs.PE.Manifest.Business;

[assembly: AssemblyTitle("PE.Manifest.GUI.Test")]
[assembly: AssemblyDescription("PE.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Peru)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(PECustomsDataRegistry), nameof(PECustomsDataRegistry.EnablePEManifests))]
