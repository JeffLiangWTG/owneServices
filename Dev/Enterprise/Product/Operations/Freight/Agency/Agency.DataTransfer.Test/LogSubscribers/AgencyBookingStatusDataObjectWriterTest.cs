using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	class AgencyBookingStatusDataObjectWriterTest : AgencyBookingDataObjectWriterTest
	{
		public void TestDataObjectWriter_BookingConfirmation()
		{
			var bookingBO = Factory.NewWithValidTestData<AgencyBooking>();
			bookingBO.JS_TransportMode = "SEA";
			bookingBO.JS_HouseBill = "HOUSE_BILL_NUMBER";

			var shipmentDataObject1 = GetBookingStatusDataWriter(bookingBO, "Booking Confirmation", "TEST_REASON").GetDataObject(bookingBO);

			AssertShipmentDataObject(shipmentDataObject1, "Booking Confirmation", true);

			var shipmentDataObject2 = GetBookingStatusDataWriter(bookingBO, "Booking Confirmation").GetDataObject(bookingBO);
			AssertShipmentDataObject(shipmentDataObject2, "Booking Confirmation");
		}

		public void TestDataObjectWriter_BLData()
		{
			var bookingBO = Factory.NewWithValidTestData<AgencyBooking>();
			bookingBO.JS_TransportMode = "SEA";
			bookingBO.JS_HouseBill = "HOUSE_BILL_NUMBER";

			var shipmentDataObject1 = GetBookingStatusDataWriter(bookingBO, "BL DATA", "TEST_REASON").GetDataObject(bookingBO);
			AssertShipmentDataObject(shipmentDataObject1, "BL DATA", true);

			var shipmentDataObject2 = GetBookingStatusDataWriter(bookingBO, "BL DATA").GetDataObject(bookingBO);
			AssertShipmentDataObject(shipmentDataObject2, "BL DATA");
		}

		void AssertShipmentDataObject(UniversalShipment shipmentDataObject, string expectedDocumentName, bool expectedRejectReasonTextNote = false)
		{
			AssertNotNull("shipmentData", shipmentDataObject);
			AssertEquals("DocumentaryOverride.DocumentName", expectedDocumentName, shipmentDataObject.DataContext.DocumentaryOverride.DocumentName);
			AssertEquals("DocumentaryOverride.Purpose.Code", "MAA", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Code);
			AssertEquals("DocumentaryOverride.Purpose.Description", "Message Accepted", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Description);

			if (expectedRejectReasonTextNote)
			{
				var rejectionNote = shipmentDataObject.NoteCollection
					.Where(x => x.Description.GetValueOrDefault() == "Reason for Rejection")
					.Select(x => x.NoteText.GetValueOrDefault()).FirstOrDefault();

				AssertEquals(1, shipmentDataObject.NoteCollection.Count);
				AssertEquals("Reason for Rejection", "TEST_REASON", rejectionNote);
			}
			else
			{
				AssertNull(shipmentDataObject.NoteCollection);
			}
		}

		AgencyBookingStatusDataObjectWriter GetBookingStatusDataWriter(AgencyBooking booking, string dataContextDocumentName, string rasonForRejectionNoteText = "")
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, booking));

			var writer = new AgencyBookingStatusDataObjectWriter(writeManager)
			{
				DataContextDocumentName = dataContextDocumentName,
				ReasonForRejectionNoteText = rasonForRejectionNoteText,
				Event = ZArchitecture.Business.AutoEvents.MessageAccepted
			};
			return writer;
		}
	}
}
