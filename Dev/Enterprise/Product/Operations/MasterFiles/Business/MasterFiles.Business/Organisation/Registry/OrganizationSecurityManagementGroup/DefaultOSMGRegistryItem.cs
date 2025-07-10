using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultOSMGRegistryItem : StronglyTypedRegistryItem<DefaultOSMG>
	{
		public DefaultOSMGRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DefaultOSMG defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultOSMGDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.DefaultOSMGRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class DefaultOSMGDataType : NonPersistentBusinessObjectRegistryDataType<DefaultOSMG>
	{
		public DefaultOSMGDataType()
		{
		}
	}
}
