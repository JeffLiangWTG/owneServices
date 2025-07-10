using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBookingOrderLinkCollection))]
	public class TrackingBookingOrderLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingBookingOrderLinkCollection>
	{
		protected override TrackingBookingOrderLinkCollection GetCollectionToTest()
		{
			return new TrackingBookingOrderLinkCollection(new TrackingBooking(Factory, new TestHelper(Factory).TestSiteUser));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TrackingBookingOrderLink(new TrackingBooking(Factory, new TestHelper(Factory).TestSiteUser), ZGuid.Empty);
		}
	}
}
