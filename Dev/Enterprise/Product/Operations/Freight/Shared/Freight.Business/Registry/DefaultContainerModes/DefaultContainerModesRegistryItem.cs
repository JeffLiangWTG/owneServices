using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public class DefaultContainerModesRegistryItem : StronglyTypedRegistryItem<DefaultContainerModesCollection>
	{
		public DefaultContainerModesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultContainerModesRegistryDataType(), storage))
		{
		}

		public DefaultContainerModesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultContainerModesRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.GUI.DefaultContainerModesRegistryItemEditor, Enterprise.Freight.GUI")]
	class DefaultContainerModesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultContainerModesCollection>
	{
	}
}
