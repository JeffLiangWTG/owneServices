using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

public class SenderInfoRegistryItem : StronglyTypedRegistryItem<SenderInfoCollection>
{
	public SenderInfoRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
		: this(name, category, caption, hint, RegistryStorageFlags.Company)
	{
	}

	public SenderInfoRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
		: base(new ManagedAccountCollectionImpl(name, category, caption, hint, storage))
	{
	}

	class ManagedAccountCollectionImpl : RegistryItemImpl
	{
		public ManagedAccountCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new SenderInfoRegistryDataType(), storage)
		{
		}
	}
}

[RegistryEditor("Enterprise.Customs.NL.GUI.SenderInfoRegistryItemEditor, Enterprise.Customs.NL.GUI")]
public class SenderInfoRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SenderInfoCollection>
{
}
