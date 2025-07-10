using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DistanceCalculationProviderConfigurationRegistryItem : StronglyTypedRegistryItem<DistanceCalculationProviderConfiguration>
	{
		public DistanceCalculationProviderConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DistanceCalculationProviderConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DistanceCalculationProviderConfigurationRegistryDataType(), storage, RegistryOptions.IsOnlyForDevelopers, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.DistanceCalculationProviderConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class DistanceCalculationProviderConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DistanceCalculationProviderConfiguration>
	{
		public DistanceCalculationProviderConfigurationRegistryDataType()
		{
		}
	}
}
