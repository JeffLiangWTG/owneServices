using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public class CMMFlagsRegistryItem : StronglyTypedRegistryItem<CMMFlags>
	{
		public CMMFlagsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CMMFlagsRegistryDataType(), storage, RegistryOptions.NotCached)) { }

		public CMMFlagsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CMMFlags defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CMMFlagsRegistryDataType(defaultValue), storage, RegistryOptions.Default)) { }
	}
}


