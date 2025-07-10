using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class BranchManagementCodeDescriptionBoolNewRegistryItem : CodeDescriptionBoolRegistryItem
	{
		public BranchManagementCodeDescriptionBoolNewRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, BranchManagementCodeDescriptionBoolCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BranchManagementCodeDescriptionBoolRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}
	}

	class BranchManagementCodeDescriptionBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BranchManagementCodeDescriptionBoolCollection>
	{
		public BranchManagementCodeDescriptionBoolRegistryDataType()
		{
		}
	}
}
