using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraCustomFieldMappingRegistryItem : StronglyTypedRegistryItem<JiraCustomFieldMap>
	{
		public JiraCustomFieldMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, JiraCustomFieldMap defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JiraCustomFieldMappingRegistryDataType(defaultValue), storage, options))
		{
		}
	}
}
