using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class RecipientSourceFallbackRegistryItem : StronglyTypedRegistryItem<RecipientSourceFallbackHeader>
	{
		public RecipientSourceFallbackRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, RecipientSourceFallbackHeader.GetDefaultValue())
		{
		}

		public RecipientSourceFallbackRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryBusinessObjectTemplate defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RecipientSourceFallbackRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
