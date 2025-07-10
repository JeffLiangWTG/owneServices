using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.EIDOMessagingRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	public class EIDOMessagingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EIDOMessagingHeader>
	{
		public EIDOMessagingRegistryDataType() { }

		public EIDOMessagingRegistryDataType(EIDOMessagingHeader defaultValue)
			: base(defaultValue) { }
	}
}


