using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Moq;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ForwardingShipmentOrBookingMatcherTest : TestCaseWithFactory
	{
		public void TestMatchFromForwardingShipment()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			Factory.Save();

			var matcher = new ForwardingShipmentOrBookingMatcher(GetMockMatcher(shipment1), GetMockMatcher(null));
			var matchedShipment = matcher.GetBestMatch();

			AssertEquals("Return matched ForwardingShipment", shipment1.PK, matchedShipment.PK);

			matcher = new ForwardingShipmentOrBookingMatcher(GetMockMatcher(shipment1), GetMockMatcher(shipment2));
			matchedShipment = matcher.GetBestMatch();

			AssertEquals("Return matched ForwardingShipment although matched Booking exist", shipment1.PK, matchedShipment.PK);

			matcher = new ForwardingShipmentOrBookingMatcher(GetMockMatcher(null), GetMockMatcher(shipment2));
			matchedShipment = matcher.GetBestMatch();

			AssertEquals("Return fallback booking when no matched ForwardingShipment", shipment2.PK, matchedShipment.PK);

			matcher = new ForwardingShipmentOrBookingMatcher(GetMockMatcher(null), GetMockMatcher(null));
			matchedShipment = matcher.GetBestMatch();

			AssertNull("Return null when no matched ForwardingShipment or Booking", matchedShipment);
		}

		IMatchingBusinessEntityFinder<ForwardingShipment> GetMockMatcher(ForwardingShipment shipmentForReturn)
		{
			var matcher = new Mock<IMatchingBusinessEntityFinder<ForwardingShipment>>();
			matcher.Setup(x => x.GetBestMatch()).Returns(shipmentForReturn);

			return matcher.Object;
		}
	}
}
