using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutomatedTariffDescriptionPopulationRegistryItem))]
	sealed class AutomatedTariffDescriptionPopulationRegistryItemTest : StronglyTypedRegistryItemTestCase<AutomatedTariffDescriptionPopulation>
	{
		protected override StronglyTypedRegistryItem<AutomatedTariffDescriptionPopulation, AutomatedTariffDescriptionPopulation> GetNewRegistryItem()
		{
			return new AutomatedTariffDescriptionPopulationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
