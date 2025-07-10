using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DocumentDtbBooking))]
	public class DocumentDtbBookingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIncludeInDelivery()
		{
			var documentBooking = (DocumentDtbBooking)GetNewBusinessObject();
			AssertEquals(true, documentBooking.IncludeInDelivery);

			documentBooking.IncludeInDelivery = false;
			AssertEquals(false, documentBooking.IncludeInDelivery);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Factory.New<DtbBooking>();
			return new DocumentDtbBooking(booking);
		}
	}
}
