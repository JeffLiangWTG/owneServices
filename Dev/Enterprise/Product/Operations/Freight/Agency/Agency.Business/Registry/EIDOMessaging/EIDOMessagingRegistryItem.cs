using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public class EIDOMessagingRegistryItem : StronglyTypedRegistryItem<EIDOMessagingHeader>
	{
		public EIDOMessagingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, new EIDOMessagingRegistryDataType()) { }

		public EIDOMessagingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, EIDOMessagingHeader defaultValue)
			: this(name, category, caption, hint, new EIDOMessagingRegistryDataType(defaultValue)) { }

		public EIDOMessagingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, EIDOMessagingRegistryDataType dataType)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, RegistryStorageFlags.System)) { }
	}
}


