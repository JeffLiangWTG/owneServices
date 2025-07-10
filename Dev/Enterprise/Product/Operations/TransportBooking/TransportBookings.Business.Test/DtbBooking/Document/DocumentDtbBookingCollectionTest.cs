
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DocumentDtbBookingCollection))]
	public class DocumentDtbBookingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentDtbBookingCollection>
	{
		public void TestSelectOrUnSelectAll()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			var booking3 = Helper.CreateBooking(consolidation);

			var documentBookings = new DocumentDtbBookingCollection(consolidation.Bookings);
			var documentBooking1 = documentBookings[0];
			var documentBooking2 = documentBookings[1];
			var documentBooking3 = documentBookings[2];
			documentBooking1.IncludeInDelivery = true;
			documentBooking2.IncludeInDelivery = false;
			documentBooking3.IncludeInDelivery = false;
			AssertEquals("Precondition", true, documentBooking1.IncludeInDelivery);
			AssertEquals("Precondition", false, documentBooking2.IncludeInDelivery);
			AssertEquals("Precondition", false, documentBooking3.IncludeInDelivery);

			documentBookings.SelectOrUnSelectAll(true);
			AssertEquals(true, documentBooking1.IncludeInDelivery);
			AssertEquals(true, documentBooking2.IncludeInDelivery);
			AssertEquals(true, documentBooking3.IncludeInDelivery);

			documentBookings.SelectOrUnSelectAll(false);
			AssertEquals(false, documentBooking1.IncludeInDelivery);
			AssertEquals(false, documentBooking2.IncludeInDelivery);
			AssertEquals(false, documentBooking3.IncludeInDelivery);
		}

		protected override DocumentDtbBookingCollection GetCollectionToTest()
		{
			var bookings = new DtbBookingCollection(Factory);
			return new DocumentDtbBookingCollection(bookings);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = Factory.New<DtbBooking>();
			return new DocumentDtbBooking(booking);
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
