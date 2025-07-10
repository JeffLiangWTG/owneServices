
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(TransportBookingAdditionalReferenceCollection))]
	public class TransportBookingAdditionalReferenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransportBookingAdditionalReferenceCollection>
	{
		protected override TransportBookingAdditionalReferenceCollection GetCollectionToTest()
		{
			return new TransportBookingAdditionalReferenceCollection(Helper.CreateBooking());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = Helper.CreateBooking();
			return new TransportBookingAdditionalReference(booking, booking.AdditionalReferenceNumbers.AddNew());
		}

		public void TestBuildCollection_WhenNoConsolidationSingleJob_DoesNotThrowException()
		{
			var booking = Factory.New<DtbBooking>();

			AssertEquals("Precondition: ConsolidationSingleJob is null", null, booking.ConsolidationSingleJob);
			var collection = new TransportBookingAdditionalReferenceCollection(booking);
			AssertNoExceptionThrown("Should not throw if ConsolidationSingleJob is null", () =>
			{
				collection.BuildCollection();
			});
			AssertEquals("Collection should be empty", 0, collection.Count);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
