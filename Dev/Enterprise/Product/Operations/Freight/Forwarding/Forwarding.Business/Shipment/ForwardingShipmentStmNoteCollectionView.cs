using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNoteCollectionView : StmNoteCollectionView
	{
		public ForwardingShipmentStmNoteCollectionView(ForwardingShipment shipment)
			: base(shipment, typeof(ForwardingShipmentStmNote))
		{
		}
	}
}
