using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.TransportBookings.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class ForwardingShipmentIDtbBookingParentTestCase : IDtbBookingParentTestCase<ForwardingShipment>
	{
		protected override ForwardingShipment GetNewParent()
		{
			return Factory.New<ForwardingShipment>();
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		protected override void ChildCartageTestExtraAssertionsPreAct(ICommonCartage cartage)
		{
			Assert("Check before act - cartage job header should have no local charges address", cartage.Job.JH_OA_LocalChargesAddr.IsEmpty);
		}

		protected override bool CanHaveDirectCartageChild => true;
	}
}
