using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ILDLOSendMessageCommandTest : ILSendCustomCommandBaseTest
	{
		protected override ILCustomCommandBase CreateCommand(ForwardingShipment shipment)
			=> new ILDLOSendMessageCommand(shipment?.DeliveryOrderProvider);

		protected override DocDataObject GetMessageDocDataObject(ForwardingShipment shipment)
			=> new DeliveryOrderBuilder(shipment).Build();

		protected override string GetDocumentName() => "Delivery Order";

		protected override string GetMessageType() => "DLO";

		protected override string GetMessageSubType() => "120";

		protected override string GetDataContext() => "ILDeliveryOrder";

		protected override void UpdateMessageReference(string reference)
		{
			shipment.JS_DLO = reference;
		}

		protected override ZString MessageReference => shipment.JS_DLO;
	}
}
