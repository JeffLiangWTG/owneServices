using Enterprise.Registry.Business;

namespace Enterprise.ProcessManagement.Business
{
	[RegistryEditor("Enterprise.ProcessManagement.GUI.ProjectCategoryMappingRegistryEditor, Enterprise.ProcessManagement.GUI")]
	public class ProjectCategoryMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ProjectCategoryMap>
	{
		public ProjectCategoryMappingRegistryDataType(ProjectCategoryMap defaultValue)
			: base(defaultValue)
		{
		}
	}
}
