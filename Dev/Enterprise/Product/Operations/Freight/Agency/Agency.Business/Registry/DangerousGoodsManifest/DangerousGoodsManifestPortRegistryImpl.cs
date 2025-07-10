using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Registry.DangerousGoodsManifest
{
	internal class DangerousGoodsManifestPortRegistryImpl : RegistryItemImpl
	{
		public DangerousGoodsManifestPortRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(name, category, caption, hint, dataType, storage)
		{
		}

		public DangerousGoodsManifestPortRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, dataType, storage, options)
		{
		}
	}
}
