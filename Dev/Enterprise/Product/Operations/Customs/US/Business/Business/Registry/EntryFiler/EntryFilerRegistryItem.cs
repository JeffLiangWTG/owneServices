using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class EntryFilerRegistryItem : StronglyTypedRegistryItem<EntryFiler>
	{
		public EntryFilerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new EntryFilerRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default))
		{
		}
	}
}
