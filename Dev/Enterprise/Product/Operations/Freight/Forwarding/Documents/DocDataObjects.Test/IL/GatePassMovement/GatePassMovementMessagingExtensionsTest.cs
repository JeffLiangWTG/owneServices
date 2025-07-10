using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class GatePassMovementMessagingExtensionsTest : ILBaseMessagingExtensionsTest<GatePassMovementMessagingExtensions>
	{
		protected override GatePassMovementMessagingExtensions CreateNewMessagingExtension(ForwardingShipment shipment)
			=> new GatePassMovementMessagingExtensions(shipment.GatePassMovementProvider);

		protected override string GetDocumentName() => "Gatepass Movement";

		protected override string GetMessageName() => "Gatepass Movement";

		protected override ZString GetMessageType() => "GPM";

		protected override void UpdateMessageReference(string reference)
		{
			shipment.JS_GMN = reference;
		}

		protected override ZString MessageReference => shipment.JS_GMN;
	}
}
