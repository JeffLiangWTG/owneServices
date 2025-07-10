using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public static class ForwardingContainerExtensions
	{
		public static ZString GetEffectiveITNumber(this ForwardingContainer container, ForwardingShipment shipment, ForwardingConsol consol)
		{
			return !container.ITReferenceNumber.IsEmpty ? container.ITReferenceNumber : shipment.GetEffectiveITNumber(consol);
		}

		public static ZInt GetNoOfPacksOnSpecificShipment(this ForwardingContainer container, ForwardingShipment shipment)
		{
			var result = ZInt.Zero;
			var shipments = shipment.GetShipmentsForBill();
			foreach (PackLine packLine in container.PackLines)
			{
				if (container.HasNewMBOL ? shipments.Any(x => x == packLine.Shipment) : packLine.Shipment == shipment)
				{
					result += packLine.JL_PackageCount;
				}
			}
			return result;
		}
	}
}
