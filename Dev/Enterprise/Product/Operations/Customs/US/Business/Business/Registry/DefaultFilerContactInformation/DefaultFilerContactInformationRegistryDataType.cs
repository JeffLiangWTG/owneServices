using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.DefaultFilerContactInformationRegistryItemEditor, Enterprise.Customs.US.GUI")]
	class DefaultFilerContactInformationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultFilerContactInformation>
	{
		public DefaultFilerContactInformationRegistryDataType()
		{
		}
	}
}
