using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class IssueTypeMappingRegistryItem : StronglyTypedRegistryItem<IssueTypeMap>
	{
		public IssueTypeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, IssueTypeMap defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IssueTypeMappingRegistryDataType(defaultValue), storage, options))
		{
		}
	}
}
