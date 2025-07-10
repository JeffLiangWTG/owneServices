using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Document.Testing
{
	class DtbBookingDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObject_FromDtbBooking()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			var pickup = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var delivery = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			var provider = new DtbBookingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "title",
				Data = new DtbBookingInstruction[] { pickup, delivery }
			};

			var dataObject = provider.GetDocDataObject(booking, DataContext.CMRConsignmentNote, parameters);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CMRConsignmentNoteDocDataObjectCollection", typeof(CMRConsignmentNoteDocDataObjectCollection), dataObject.GetType());
		}
	}
}
