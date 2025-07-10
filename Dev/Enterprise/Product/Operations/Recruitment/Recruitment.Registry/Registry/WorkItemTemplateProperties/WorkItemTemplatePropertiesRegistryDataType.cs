using Enterprise.Registry.Business;

namespace Enterprise.Recruitment.Registry
{
	[RegistryEditor("Enterprise.Recruitment.Registry.WorkItemTemplatePropertiesRegistryItemEditor, Enterprise.Recruitment.Module")]
	public class WorkItemTemplatePropertiesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WorkItemTemplatePropertiesCollection>
	{
		public WorkItemTemplatePropertiesRegistryDataType()
		{ }
	}
}
