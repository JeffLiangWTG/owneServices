using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNoteCollectionWithRelatedElements : StmNoteCollectionWithRelatedElements
	{
		public ForwardingShipmentStmNoteCollectionWithRelatedElements(ForwardingShipment shipment)
			: base(shipment)
		{
		}
	}
}
