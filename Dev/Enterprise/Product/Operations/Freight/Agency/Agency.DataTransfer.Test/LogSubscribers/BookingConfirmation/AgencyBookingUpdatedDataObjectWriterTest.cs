using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Test.LogSubscribers.BookingConfirmation
{
	class AgencyBookingUpdatedDataObjectWriterTest : AgencyBookingDataObjectWriterTest
	{
		public void TestDataObjectWriter()
		{
			var bookingBO = Factory.NewWithValidTestData<AgencyBooking>();
			bookingBO.JS_TransportMode = "SEA";
			bookingBO.JS_HouseBill = "HOUSE_BILL_NUMBER";

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, bookingBO));
			var writer = new AgencyBookingUpdatedDataObjectWriter(writeManager);

			var shipmentDataObject = writer.GetDataObject(bookingBO);

			AssertNotNull("shipmentData", shipmentDataObject);
			AssertEquals("DocumentaryOverride.DocumentName", "Booking Confirmation", shipmentDataObject.DataContext.DocumentaryOverride.DocumentName);
			AssertEquals("DocumentaryOverride.Purpose.Code", "MAA", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Code);
			AssertEquals("DocumentaryOverride.Purpose.Description", "Message Accepted", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Description);

			AssertNull(shipmentDataObject.NoteCollection);
		}
	}
}
