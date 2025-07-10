using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class DeliveryOrderMessagingExtensionsTest : ILBaseMessagingExtensionsTest<DeliveryOrderMessagingExtensions>
	{
		protected override DeliveryOrderMessagingExtensions CreateNewMessagingExtension(ForwardingShipment shipment)
			=> new DeliveryOrderMessagingExtensions(shipment.DeliveryOrderProvider);

		protected override string GetDocumentName() => "Delivery Order";

		protected override string GetMessageName() => "Delivery Order";

		protected override ZString GetMessageType() => "DLO";

		protected override void UpdateMessageReference(string reference)
		{
			shipment.JS_DLO = reference;
		}

		protected override ZString MessageReference => shipment.JS_DLO;
	}
}
