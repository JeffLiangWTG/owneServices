using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ViewTrackingBookingCollection))]
	public class ViewTrackingBookingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			ViewTrackingBookingCollection viewTrackingBookingCollection = new ViewTrackingBookingCollection(Factory);
			Assert("Can't Add new", !viewTrackingBookingCollection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ViewTrackingBookingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ViewTrackingBooking>();
		}
	}
}
