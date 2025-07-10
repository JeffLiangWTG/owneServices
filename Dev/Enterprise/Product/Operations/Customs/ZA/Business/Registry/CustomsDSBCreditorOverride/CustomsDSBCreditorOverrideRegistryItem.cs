using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class CustomsDSBCreditorOverrideRegistryItem : StronglyTypedRegistryItem<CustomsDSBCreditorOverrideCollection>
	{
		public CustomsDSBCreditorOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, RegistryStorageFlags.Company)
		{
		}

		public CustomsDSBCreditorOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new ManagedAccountCollectionImpl(name, category, caption, hint, storage))
		{
		}

		class ManagedAccountCollectionImpl : RegistryItemImpl
		{
			public ManagedAccountCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new CustomsDSBCreditorOverrideRegistryDataType(), storage)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Customs.ZA.DataRegistry.GUI.CustomsDSBCreditorOverrideRegistryItemEditor, Enterprise.Customs.ZA.GUI")]
	public class CustomsDSBCreditorOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomsDSBCreditorOverrideCollection>
	{
	}
}
