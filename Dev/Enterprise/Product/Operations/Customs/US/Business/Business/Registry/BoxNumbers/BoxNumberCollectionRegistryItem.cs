using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class BoxNumberCollectionRegistryItem : StronglyTypedRegistryItem<BoxNumberCollection>
	{
		public BoxNumberCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BoxNumberCollection defaultValue)
			: base(new BoxNumberCollectionImpl(name, category, caption, hint, RegistryStorageFlags.Branch, RegistryOptions.MustOverrideDefaultValue))
		{
		}

		class BoxNumberCollectionImpl : RegistryItemImpl
		{
			public BoxNumberCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new BoxNumberCollectionRegistryDataType(), storage, options)
			{
			}
		}
	}
}
