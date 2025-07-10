using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	class BillOfLadingStatusDataObjectWriterTest : BillOfLadingDataObjectWriterTest
	{
		public void TestDataObjectWriter_BookingConfirmation()
		{
			var billOfLadingBO = Factory.NewWithValidTestData<BillOfLading>();
			billOfLadingBO.JS_TransportMode = "SEA";
			billOfLadingBO.JS_HouseBill = "HOUSE_BILL_NUMBER";

			var shipmentDataObject1 = GetBillOfLadingStatusDataWriter(billOfLadingBO, "Booking Confirmation", "TEST_REASON").GetDataObject(billOfLadingBO);

			AssertShipmentDataObject(shipmentDataObject1, "Booking Confirmation", true);

			var shipmentDataObject2 = GetBillOfLadingStatusDataWriter(billOfLadingBO, "Booking Confirmation").GetDataObject(billOfLadingBO);
			AssertShipmentDataObject(shipmentDataObject2, "Booking Confirmation");
		}

		public void TestDataObjectWriter_BLData()
		{
			var billOfLadingBO = Factory.NewWithValidTestData<BillOfLading>();
			billOfLadingBO.JS_TransportMode = "SEA";
			billOfLadingBO.JS_HouseBill = "HOUSE_BILL_NUMBER";

			var shipmentDataObject1 = GetBillOfLadingStatusDataWriter(billOfLadingBO, "BL DATA", "TEST_REASON").GetDataObject(billOfLadingBO);
			AssertShipmentDataObject(shipmentDataObject1, "BL DATA", true);

			var shipmentDataObject2 = GetBillOfLadingStatusDataWriter(billOfLadingBO, "BL DATA").GetDataObject(billOfLadingBO);
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

		BillOfLadingStatusDataObjectWriter GetBillOfLadingStatusDataWriter(BillOfLading billOfLading, string dataContextDocumentName, string rasonForRejectionNoteText = "")
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, billOfLading));

			var writer = new BillOfLadingStatusDataObjectWriter(writeManager)
			{
				DataContextDocumentName = dataContextDocumentName,
				ReasonForRejectionNoteText = rasonForRejectionNoteText,
				Event = ZArchitecture.Business.AutoEvents.MessageAccepted
			};
			return writer;
		}
	}
}
