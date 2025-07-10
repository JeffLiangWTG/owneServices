using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public class BookingVisualizableDocumentSupporterTest : AgencyShipmentVisualizableDocumentSupporterTest<AgencyBooking>
	{
		public override AgencyShipmentVisualizableDocumentSupporter<AgencyBooking> GetVisualizableDocumentSupporter(AgencyBooking shipment)
		{
			return new BookingVisualizableDocumentSupporter(shipment);
		}
	}
}
