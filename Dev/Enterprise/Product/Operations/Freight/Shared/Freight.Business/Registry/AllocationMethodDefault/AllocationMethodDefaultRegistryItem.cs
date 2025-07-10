using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public sealed class AllocationMethodDefaultRegistryItem : StronglyTypedRegistryItem<AllocationMethodDefaultHeader>
	{
		public AllocationMethodDefaultRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, new AllocationMethodDefaultRegistryDataType()) { }

		public AllocationMethodDefaultRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, AllocationMethodDefaultHeader defaultValue)
			: this(name, category, caption, hint, new AllocationMethodDefaultRegistryDataType(defaultValue)) { }

		public AllocationMethodDefaultRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, AllocationMethodDefaultRegistryDataType dataType)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, RegistryStorageFlags.System)) { }
	}
}
