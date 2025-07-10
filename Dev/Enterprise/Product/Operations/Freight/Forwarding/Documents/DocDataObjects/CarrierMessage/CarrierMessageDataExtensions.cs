using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class CarrierMessageDataExtensions
	{
		public static IShipment GetMatchedShipmentForPackLine(IShipment shipment, string shipmentID)
		{
			IShipment matchedShipment;

			if (shipment.ShipmentID == shipmentID)
			{
				return shipment;
			}
			else if (shipment.Shipments != null)
			{
				foreach (var subShipment in shipment.Shipments)
				{
					matchedShipment = GetMatchedShipmentForPackLine(subShipment, shipmentID);
					if (matchedShipment != null)
					{
						return matchedShipment;
					}
				}
			}

			return null;
		}
	}
}
