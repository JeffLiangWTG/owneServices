using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.DetentionAdviceDeliveryRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	public class DetentionAdviceDeliveryRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DetentionAdviceDelivery>
	{
		public DetentionAdviceDeliveryRegistryDataType()
			: base(new DetentionAdviceDelivery()) { }

		public DetentionAdviceDeliveryRegistryDataType(DetentionAdviceDelivery defaultValue)
			: base(defaultValue) { }
	}
}


