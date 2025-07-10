using Enterprise.Freight.Agency.Business.Registry.DangerousGoodsManifest;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class DangerousGoodsManifestPortCollectionRegistryItem : StronglyTypedRegistryItem<DangerousGoodsManifestPortCollection>
	{
		public DangerousGoodsManifestPortCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new DangerousGoodsManifestPortRegistryImpl(name, category, caption, hint, new DangerousGoodsManifestPortCollectionDataType(), storage))
		{
		}

		public DangerousGoodsManifestPortCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new DangerousGoodsManifestPortRegistryImpl(name, category, caption, hint, new DangerousGoodsManifestPortCollectionDataType(), storage, options))
		{
		}
	}
}
