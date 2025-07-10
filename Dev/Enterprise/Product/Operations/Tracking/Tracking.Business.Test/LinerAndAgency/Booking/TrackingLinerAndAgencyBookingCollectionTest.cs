using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingLinerAndAgencyBookingCollection))]
	sealed class TrackingLinerAndAgencyBookingCollectionTest : ActiveBusinessObjectCollectionTestCase<TrackingLinerAndAgencyBookingCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingLinerAndAgencyBooking));
		}

		protected override TrackingLinerAndAgencyBookingCollection GetCollectionToTest()
		{
			return new TrackingLinerAndAgencyBookingCollection(Factory);
		}
	}
}
