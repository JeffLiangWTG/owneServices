using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityProfileRegistryItem : StronglyTypedRegistryItem<OrgSecurityProfileCollection>
	{
		public OrgSecurityProfileRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, OrgSecurityProfileCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrgSecurityProfileDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.OrgSecurityProfileRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class OrgSecurityProfileDataType : NonPersistentBusinessObjectRegistryDataType<OrgSecurityProfileCollection>
	{
		public OrgSecurityProfileDataType()
		{
		}
	}
}
