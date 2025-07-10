using Enterprise.Freight.Agency.Business.Registry.PortManifest;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortManifestPortCollectionRegistryItem : StronglyTypedRegistryItem<PortManifestPortCollection>
	{
		public PortManifestPortCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new PortManifestPortRegistryImpl(name, category, caption, hint, new PortManifestPortCollectionDataType(), storage))
		{
		}
	}
}
