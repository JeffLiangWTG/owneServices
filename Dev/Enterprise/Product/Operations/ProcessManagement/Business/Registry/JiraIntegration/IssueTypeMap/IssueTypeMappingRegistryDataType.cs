using Enterprise.Registry.Business;

namespace Enterprise.ProcessManagement.Business
{
	[RegistryEditor("Enterprise.ProcessManagement.GUI.IssueTypeMappingRegistryEditor, Enterprise.ProcessManagement.GUI")]
	public class IssueTypeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IssueTypeMap>
	{
		public IssueTypeMappingRegistryDataType(IssueTypeMap defaultValue)
			: base(defaultValue)
		{
		}
	}
}
