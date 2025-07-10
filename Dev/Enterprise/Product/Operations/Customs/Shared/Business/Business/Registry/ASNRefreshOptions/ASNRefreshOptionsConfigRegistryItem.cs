using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class ASNRefreshOptionsConfigRegistryItem : StronglyTypedRegistryItem<ASNRefreshOptionsConfigCollection>
	{
		public ASNRefreshOptionsConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ASNRefreshOptionsConfigCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ASNRefreshOptionsConfigRegistryItemDataType(), storage, defaultValue))
		{
		}

		public ASNRefreshOptionsConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ASNRefreshOptionsConfigCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ASNRefreshOptionsConfigRegistryItemDataType(), storage, options, defaultValue))
		{
		}
	}
}
