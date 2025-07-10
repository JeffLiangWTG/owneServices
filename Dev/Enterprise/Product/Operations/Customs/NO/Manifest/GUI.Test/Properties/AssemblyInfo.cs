using System.Reflection;
using Enterprise.Customs.NO.Registry;

[assembly: AssemblyTitle("Enterprise.Customs.NO.Manifest.GUI.Test")]
[assembly: AssemblyDescription("Enterprise.Customs.NO.Manifest.GUI.GUI.Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Norway)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(NOCustomsDataRegistry), nameof(NOCustomsDataRegistry.EnableNOManifests))]
