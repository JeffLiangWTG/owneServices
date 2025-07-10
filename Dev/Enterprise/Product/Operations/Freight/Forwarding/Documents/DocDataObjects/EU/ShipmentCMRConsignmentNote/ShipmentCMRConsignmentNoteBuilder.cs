using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ShipmentCMRConsignmentNoteBuilder : ICMRConsignmentNoteBuilder
	{
		readonly ForwardingShipment shipment;

		public ShipmentCMRConsignmentNoteBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		public CMRConsignmentNoteDocDataObjectCollection Build()
		{
			var shipmentCmr = new ShipmentCMRConsignmentNote(shipment);
			return new CMRConsignmentNoteDocDataObjectCollection([new CMRConsignmentNoteDocDataObject(shipmentCmr)]);
		}
	}
}
