using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SupplyTypeConfigurationByChargeGroupRegistryItem : StronglyTypedRegistryItem<SupplyTypeConfigurationByChargeGroupCollection>
	{
		public SupplyTypeConfigurationByChargeGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, SupplyTypeConfigurationByChargeGroupCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SupplyTypeConfigurationByChargeGroupRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.SupplyTypeConfigurationByChargeGroupRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class SupplyTypeConfigurationByChargeGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SupplyTypeConfigurationByChargeGroupCollection>
	{
		public SupplyTypeConfigurationByChargeGroupRegistryDataType()
		{
		}
	}
}
