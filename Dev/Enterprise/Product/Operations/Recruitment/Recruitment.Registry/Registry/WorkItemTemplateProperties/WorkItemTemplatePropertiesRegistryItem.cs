using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruitment.Registry
{
	public class WorkItemTemplatePropertiesRegistryItem : StronglyTypedRegistryItem<WorkItemTemplatePropertiesCollection>
	{
		public WorkItemTemplatePropertiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			   : this(name, category, caption, hint, storage, new WorkItemTemplatePropertiesCollection())
		{
		}

		public WorkItemTemplatePropertiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, WorkItemTemplatePropertiesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkItemTemplatePropertiesRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
