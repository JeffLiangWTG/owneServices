using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ARTermsRegistryItem : StronglyTypedRegistryItem<ARTermsCollection>
	{
		public ARTermsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ARTermsCollection defaultValue)
				: base(new RegistryItemImpl(name, category, caption, hint, new ARTermsRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.SettlementDetailsRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class ARTermsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ARTermsCollection>
	{
		public ARTermsRegistryDataType()
		{
		}
	}
}
