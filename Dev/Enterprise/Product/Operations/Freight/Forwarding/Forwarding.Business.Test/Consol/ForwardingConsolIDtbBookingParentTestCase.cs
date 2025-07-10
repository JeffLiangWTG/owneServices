using Enterprise.TransportBookings.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsol))]
	sealed class ForwardingConsolIDtbBookingParentTestCase : IDtbBookingParentTestCase<ForwardingConsol>
	{
		protected override ForwardingConsol GetNewParent()
		{
			return Factory.New<ForwardingConsol>();
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		protected override bool CanHaveDirectCartageChild => true;
	}
}
