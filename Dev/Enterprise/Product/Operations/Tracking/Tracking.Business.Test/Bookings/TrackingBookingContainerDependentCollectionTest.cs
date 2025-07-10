using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business.Test;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBookingContainerDependentCollection))]
	public class TrackingBookingContainerDependentCollectionTest : QuotedBookingContainerDependentCollectionTest<TrackingContainer>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var booking = Factory.New<ForwardingShipment>();

			return new TrackingBookingContainerDependentCollection(new TrackingBooking(booking, Factory, CreateTestContactAndLogin()));
		}

		TrackingSiteUser CreateTestContactAndLogin()
		{
			var helper = new TestHelper(Factory);

			return helper.TestSiteUser;
		}
	}
}
