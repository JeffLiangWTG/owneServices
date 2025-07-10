using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Registry.PortManifest
{
	internal class PortManifestPortRegistryImpl : RegistryItemImpl
	{
		public PortManifestPortRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(name, category, caption, hint, dataType, storage)
		{
		}

		public PortManifestPortRegistryImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, dataType, storage, options)
		{
		}
	}
}
