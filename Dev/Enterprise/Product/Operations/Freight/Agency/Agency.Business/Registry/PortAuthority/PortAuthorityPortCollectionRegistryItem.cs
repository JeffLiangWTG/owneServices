using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthorityPortCollectionRegistryItem : StronglyTypedRegistryItem<PortAuthorityPortCollection>
	{
		public PortAuthorityPortCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PortAuthorityPortCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PortAuthorityPortCollectionDataType(defaultValue), storage))
		{
		}

		public PortAuthorityPortCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PortAuthorityPortCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PortAuthorityPortCollectionDataType(defaultValue), storage, options))
		{
		}
	}
}
