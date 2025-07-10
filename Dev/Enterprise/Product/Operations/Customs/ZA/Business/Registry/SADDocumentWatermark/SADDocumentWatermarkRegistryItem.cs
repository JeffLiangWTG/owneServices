using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class SADDocumentWatermarkRegistryItem : StronglyTypedRegistryItem<SADDocumentWatermarkCollection>
	{
		public SADDocumentWatermarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, RegistryStorageFlags.Company)
		{
		}

		public SADDocumentWatermarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new ManagedAccountCollectionImpl(name, category, caption, hint, storage))
		{
		}

		class ManagedAccountCollectionImpl : RegistryItemImpl
		{
			public ManagedAccountCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new SADDocumentWatermarkRegistryDataType(), storage)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Customs.ZA.DataRegistry.GUI.SADDocumentWatermarkRegistryItemEditor, Enterprise.Customs.ZA.GUI")]
	public class SADDocumentWatermarkRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SADDocumentWatermarkCollection>
	{
	}
}
