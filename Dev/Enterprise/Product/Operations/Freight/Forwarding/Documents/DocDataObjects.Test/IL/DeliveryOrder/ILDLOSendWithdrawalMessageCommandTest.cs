using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ILDLOSendWithdrawalMessageCommandTest : ILSendWithdrawalCustomCommandBaseTest
	{
		protected override ILCustomCommandBase CreateCommand(ForwardingShipment shipment)
			=> new ILDLOSendWithdrawalMessageCommand(shipment?.DeliveryOrderProvider);

		protected override DocDataObject GetMessageDocDataObject(ForwardingShipment shipment)
			=> new DeliveryOrderBuilder(shipment).Build();

		protected override string GetDocumentName() => "Delivery Order";

		protected override string GetMessageType() => "DLO";

		protected override string GetMessageSubType() => "120";

		protected override string GetDataContext() => "ILDeliveryOrder";

		protected override void UpdateMessageReference(string messageReference)
		{
			shipment.JS_DLO = messageReference;
		}

		protected override ZString MessageReference => shipment.JS_DLO;
	}
}
