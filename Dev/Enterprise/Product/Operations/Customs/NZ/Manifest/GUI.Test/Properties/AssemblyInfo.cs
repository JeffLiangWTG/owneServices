using System.Reflection;
using Enterprise.Customs.NZ.Registry;

[assembly: AssemblyTitle("Enterprise.Customs.NZ.Manifest.GUI.Test")]
[assembly: AssemblyDescription("Enterprise.Customs.NZ.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.NewZealand)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(NZCustomsDataRegistry), new[] { nameof(NZCustomsDataRegistry.EnableInwardCargoReportManifest) })]
