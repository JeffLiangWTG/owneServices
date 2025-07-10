using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.PortMessagingPortRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	public sealed class PortMessagingPortRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PortMessagingPortCollection>
	{
		public PortMessagingPortRegistryDataType(PortMessagingPortCollection defaultValue)
			: base(defaultValue) { }
	}
}


