using Enterprise.Registry.Business;

namespace Enterprise.ProcessManagement.Business
{
	[RegistryEditor("Enterprise.ProcessManagement.GUI.RecipientSourceFallbackRegistryEditor, Enterprise.ProcessManagement.GUI")]
	public class RecipientSourceFallbackRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RecipientSourceFallbackHeader>
	{
		public RecipientSourceFallbackRegistryDataType()
			: base(RecipientSourceFallbackHeader.GetDefaultValue())
		{
		}
	}
}
