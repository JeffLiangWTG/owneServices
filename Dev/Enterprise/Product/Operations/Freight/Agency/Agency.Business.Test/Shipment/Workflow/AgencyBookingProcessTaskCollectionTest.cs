using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingProcessTaskCollection))]
	internal class AgencyBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<AgencyBookingProcessTaskCollection>
	{
		public void TestParent()
		{
			AssertEquals(typeof(AgencyBooking), Collection.Parent.GetType());
		}

		#region Implementation
		protected override AgencyBookingProcessTaskCollection GetCollectionToTestCore()
		{
			return new AgencyBookingProcessTaskCollection(Booking);
		}

		AgencyBooking Booking
		{
			get
			{
				return booking ?? (booking = Factory.NewWithValidTestData<AgencyBooking>());
			}
		}

		AgencyBooking booking;
		#endregion
	}
}
