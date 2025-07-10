using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ILGPMSendMessageCommandTest : ILSendCustomCommandBaseTest
	{
		protected override ILCustomCommandBase CreateCommand(ForwardingShipment shipment)
			=> new ILGPMSendMessageCommand(shipment?.GatePassMovementProvider);

		protected override DocDataObject GetMessageDocDataObject(ForwardingShipment shipment)
			=> new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

		protected override string GetDocumentName() => "Gatepass Movement";

		protected override string GetMessageType() => "GPM";

		protected override string GetMessageSubType() => "130";

		protected override string GetDataContext() => "ILGatePassMovement";

		protected override void UpdateMessageReference(string reference)
		{
			shipment.JS_GMN = reference;
		}

		protected override ZString MessageReference => shipment.JS_GMN;
	}
}
