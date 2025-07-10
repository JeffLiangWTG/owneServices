using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class JobTemplateDefaultRegistryItem : StronglyTypedRegistryItem<JobTemplateDefaultCollection>
	{
		public JobTemplateDefaultRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				JobTemplateDefaultCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobTemplateDefaultRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.TransportCommon.GUI.Registry.JobTemplateDefaultRegistryItemEditor, Enterprise.TransportCommon.GUI")]
	public class JobTemplateDefaultRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobTemplateDefaultCollection>
	{
	}
}
