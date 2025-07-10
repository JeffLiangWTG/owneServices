using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentOrBookingMatcher : IMatchingBusinessEntityFinder<ForwardingShipment>
	{
		public ForwardingShipmentOrBookingMatcher(IMatchingBusinessEntityFinder<ForwardingShipment> shipmentMatcher, IMatchingBusinessEntityFinder<ForwardingShipment> bookingMatcher)
		{
			this.shipmentMatcher = shipmentMatcher;
			this.bookingMatcher = bookingMatcher;
		}

		readonly IMatchingBusinessEntityFinder<ForwardingShipment> shipmentMatcher;
		readonly IMatchingBusinessEntityFinder<ForwardingShipment> bookingMatcher;

		public ForwardingShipment GetBestMatch()
		{
			return shipmentMatcher.GetBestMatch() ?? bookingMatcher.GetBestMatch();
		}
	}
}
