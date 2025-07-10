using Enterprise.Registry.Business;

namespace Enterprise.ProcessManagement.Business
{
	[RegistryEditor("Enterprise.ProcessManagement.GUI.JiraCustomFieldMappingRegistryEditor, Enterprise.ProcessManagement.GUI")]
	public class JiraCustomFieldMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JiraCustomFieldMap>
	{
		public JiraCustomFieldMappingRegistryDataType(JiraCustomFieldMap defaultValue)
			: base(defaultValue)
		{
		}
	}
}
