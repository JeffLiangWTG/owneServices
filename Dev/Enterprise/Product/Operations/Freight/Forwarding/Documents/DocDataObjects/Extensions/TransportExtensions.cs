using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class TransportExtensions
	{
		public static bool IsLegFromNonDirectConsolAttachedToDirectShipment(this Freight.Business.Transport transport, ForwardingShipment shipment)
		{
			return shipment.IsDirectShipment
				   && transport.Parent is ForwardingConsol consol
				   && !consol.IsDirect;
		}
	}
}
