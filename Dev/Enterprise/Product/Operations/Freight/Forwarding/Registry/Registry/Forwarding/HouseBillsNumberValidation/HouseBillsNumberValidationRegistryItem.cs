using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HouseBillsNumberValidationRegistryItem : StronglyTypedRegistryItem<HouseBillsNumberValidationCollection>
	{
		public HouseBillsNumberValidationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, HouseBillsNumberValidationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new HouseBillsNumberValidationRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.HouseBillsNumberValidationRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	public class HouseBillsNumberValidationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HouseBillsNumberValidationCollection>
	{
		public HouseBillsNumberValidationRegistryDataType()
		{
		}
	}
}
