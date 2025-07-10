using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class AutomatedTariffDescriptionPopulationRegistryItem : StronglyTypedRegistryItem<AutomatedTariffDescriptionPopulation>
	{
		public AutomatedTariffDescriptionPopulationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new AutomatedTariffDescriptionPopulationRegistryDataType(), storage))
		{
		}

		public AutomatedTariffDescriptionPopulationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, AutomatedTariffDescriptionPopulation defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutomatedTariffDescriptionPopulationRegistryDataType(), storage, registryOptions, defaultValue))
		{
		}
	}
}
