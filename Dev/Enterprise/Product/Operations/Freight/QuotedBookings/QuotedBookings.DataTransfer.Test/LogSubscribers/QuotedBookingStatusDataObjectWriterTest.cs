using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	public class QuotedBookingStatusDataObjectWriterTest : ForwardingBookingDataObjectWriterTest
	{
		public void TestDataObjectWriter_BookingConfirmation()
		{
			var bookingBO = CreateBooking();
			var shipmentBO = bookingBO.Booking;

			var shipmentDataObject1 = GetBookingStatusDataWriter(bookingBO, "Booking Confirmation", true, "TEST_REASON").GetDataObject(shipmentBO);

			AssertShipmentDataObject(shipmentDataObject1, "Booking Confirmation", true, true);

			var shipmentDataObject2 = GetBookingStatusDataWriter(bookingBO, "Booking Confirmation").GetDataObject(shipmentBO);
			AssertShipmentDataObject(shipmentDataObject2, "Booking Confirmation");
		}

		public void TestDataObjectWriter_BLData()
		{
			var bookingBO = CreateBooking();
			var shipmentBO = bookingBO.Booking;

			var shipmentDataObject1 = GetBookingStatusDataWriter(bookingBO, "BL DATA", false, "TEST_REASON").GetDataObject(shipmentBO);
			AssertShipmentDataObject(shipmentDataObject1, "BL DATA", false, true);

			var shipmentDataObject2 = GetBookingStatusDataWriter(bookingBO, "BL DATA").GetDataObject(shipmentBO);
			AssertShipmentDataObject(shipmentDataObject2, "BL DATA");
		}

		void AssertShipmentDataObject(UniversalShipment shipmentDataObject, string expectedDocumentName, bool expectedTransportLegCollection = false, bool expectedRejectReasonTextNote = false)
		{
			AssertNotNull("shipmentData", shipmentDataObject);
			AssertEquals("DocumentaryOverride.DocumentName", expectedDocumentName, shipmentDataObject.DataContext.DocumentaryOverride.DocumentName);
			AssertEquals("DocumentaryOverride.Purpose.Code", "MAA", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Code);
			AssertEquals("DocumentaryOverride.Purpose.Description", "Message Accepted", shipmentDataObject.DataContext.DocumentaryOverride.Purpose.Description);

			AssertEquals("CoLoadMasterBillNumber", "HOUSE_BILL_NUMBER", shipmentDataObject.CoLoadMasterBillNumber);

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

			if (expectedTransportLegCollection)
			{
				var transportDataObject = shipmentDataObject.TransportLegCollection.FirstOrDefault();

				AssertEquals(1, shipmentDataObject.TransportLegCollection.Count);

				AssertEquals("transportDataObject.LegOrder.Value", (ZByte)1, transportDataObject.LegOrder);
				AssertEquals("transportDataObject.TransportMode", new TransportModeConverter().ToEnumValue(Core.Constants.TransportModes.Sea), transportDataObject.TransportMode);
				AssertEquals("transportDataObject.LegType", new LegTypeConverter().ToEnumValue(Core.Constants.TransportPlanningType.MainVessel), transportDataObject.LegType);
				AssertEquals("transportDataObject.VesselName", "VESSEL_A", transportDataObject.VesselName);
				AssertEquals("transportDataObject.VoyageFlightNo", "VOYAGE_A", transportDataObject.VoyageFlightNo);
				AssertEquals("transportDataObject.PortOfLoading.Code", "AUSYD", transportDataObject.PortOfLoading.Code);
				AssertEquals("transportDataObject.PortOfDischarge.Code", "NZAKL", transportDataObject.PortOfDischarge.Code);

				AssertEquals("transportDataObject.EstimatedDeparture", new ZDateTime(2020, 5, 10), transportDataObject.EstimatedDeparture);
				AssertEquals("transportDataObject.ActualDeparture", new ZDateTime(2020, 5, 11), transportDataObject.ActualDeparture);
				AssertEquals("transportData.FCLCutOff", new ZDateTime(2020, 5, 12), transportDataObject.FCLCutOff);
				AssertEquals("transportData.DocumentCutOff", new ZDateTime(2020, 5, 13), transportDataObject.DocumentCutOff);
				AssertEquals("transportData.VGMCutOff", new ZDateTime(2020, 5, 14), transportDataObject.VGMCutOff);

				AssertEquals("transportDataObject.EstimatedArrival", new ZDateTime(2020, 5, 15), transportDataObject.EstimatedArrival);
				AssertEquals("transportDataObject.ActualArrival", new ZDateTime(2020, 5, 16), transportDataObject.ActualArrival);

				AssertEquals("transportData.LCLReceivalCommences", new ZDateTime(2020, 5, 17), transportDataObject.LCLReceivalCommences);
				AssertEquals("transportData.LCLCutOff", new ZDateTime(2020, 5, 18), transportDataObject.LCLCutOff);
			}
			else
			{
				AssertNull(shipmentDataObject.TransportLegCollection);
			}
		}

		QuotedBooking CreateBooking()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL_A";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "VOYAGE_A";
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = jobVoyage.PK;
			origin.JA_E_DEP = new ZDateTime(2020, 05, 10);
			origin.JA_A_DEP = new ZDateTime(2020, 05, 11);
			origin.JA_CutOff = new ZDateTime(2020, 05, 12);
			origin.JA_DocumentaryCutoff = new ZDateTime(2020, 05, 13);
			origin.JA_VGMCutOff = new ZDateTime(2020, 05, 14);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_JV = jobVoyage.PK;
			destination.JB_E_ARV = new ZDateTime(2020, 05, 15);
			destination.JB_A_ARV = new ZDateTime(2020, 05, 16);

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2020, 05, 17);
			sailing.JX_DepotCutOff = new ZDateTime(2020, 05, 18);

			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			bookingBO.Booking.JS_TransportMode = "SEA";
			bookingBO.Booking.JS_HouseBill = "HOUSE_BILL_NUMBER";

			((ISailingChooserParent)bookingBO).SailingJX = sailing.PK;

			Factory.SaveForTesting();

			return bookingBO;
		}

		QuotedBookingStatusDataObjectWriter GetBookingStatusDataWriter(QuotedBooking booking, string dataContextDocumentName, bool shouldPopulateTransportLegCollection = false, string rasonForRejectionNoteText = "")
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, booking));

			var writer = new QuotedBookingStatusDataObjectWriter(writeManager, booking)
			{
				DataContextDocumentName = dataContextDocumentName,
				ReasonForRejectionNoteText = rasonForRejectionNoteText,
				ShouldPopulateTransportLegCollection = shouldPopulateTransportLegCollection,
				Event = AutoEvents.MessageAccepted
			};
			return writer;
		}

		protected override BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO)
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, topLevelBO));
			return new QuotedBookingStatusDataObjectWriter(writerManager, bookingBO)
			{
				Event = AutoEvents.MessageAccepted
			};
		}
	}
}
