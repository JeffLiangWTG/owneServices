using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class PackageVersionOverrideRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<PackageVersionOverrideCollection, PackageVersionOverrideCollection>
	{
		public PackageVersionOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PackageVersionOverrideCollection defaultValue, RegistryOptions registryOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new PackageVersionOverrideDataType(), storage, registryOptions, defaultValue))
		{
		}

		public override int MaxLength => 256;
	}
}
