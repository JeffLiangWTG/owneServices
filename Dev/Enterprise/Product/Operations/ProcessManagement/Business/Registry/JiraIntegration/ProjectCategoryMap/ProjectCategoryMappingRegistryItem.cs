using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectCategoryMappingRegistryItem : StronglyTypedRegistryItem<ProjectCategoryMap>
	{
		public ProjectCategoryMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ProjectCategoryMap defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ProjectCategoryMappingRegistryDataType(defaultValue), storage, options))
		{
		}
	}
}
