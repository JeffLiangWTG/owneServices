using Enterprise.TransportBookings.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipment))]
	internal class AgencyShipmentIDtbBookingParentTestCase : IDtbBookingParentTestCase<AgencyShipment>
	{
		protected override AgencyShipment GetNewParent()
		{
			return Factory.New<BillOfLading>();
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		protected override bool CanHaveDirectCartageChild => true;
	}
}
