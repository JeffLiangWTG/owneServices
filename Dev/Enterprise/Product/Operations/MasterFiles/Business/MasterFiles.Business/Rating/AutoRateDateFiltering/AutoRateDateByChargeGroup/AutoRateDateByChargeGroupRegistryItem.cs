using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class AutoRateDateByChargeGroupRegistryItem : StronglyTypedRegistryItem<AutoRateDateByChargeGroupConfiguration>
	{
		public AutoRateDateByChargeGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AutoRateDateByChargeGroupConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutoRateDateByChargeGroupRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.AutoRateDateByChargeGroupConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class AutoRateDateByChargeGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutoRateDateByChargeGroupConfiguration>
	{
		public AutoRateDateByChargeGroupRegistryDataType()
		{
		}
	}
}
