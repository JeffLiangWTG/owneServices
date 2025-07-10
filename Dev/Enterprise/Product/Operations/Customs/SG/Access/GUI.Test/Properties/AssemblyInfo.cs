using System.Reflection;
using Enterprise.Customs.SG.Registry;

[assembly: AssemblyTitle("Enterprise.Customs.SG.Access.GUI Test")]
[assembly: AssemblyDescription("Enterprise.Customs.SG.Access.GUI Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Singapore)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(SGCustomsDataRegistry), nameof(SGCustomsDataRegistry.ACCESSEnable))]
