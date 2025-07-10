using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVShipmentItemCollection : BusinessObjectCollection<HVLVItem>, IHVLVItemCollectionForDocument
	{
		public HVLVShipmentItemCollection(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		IHVLVItem IHVLVItemCollection.this[int i] => (IHVLVItem)Elements[i];

		IHVLVItemForDocument IHVLVItemCollectionForDocument.this[int i] => (IHVLVItemForDocument)Elements[i];

		protected override ZQuery CreateRelationshipFilter()
		{
			return shipment.HVLVItemQuery;
		}
	}
}
