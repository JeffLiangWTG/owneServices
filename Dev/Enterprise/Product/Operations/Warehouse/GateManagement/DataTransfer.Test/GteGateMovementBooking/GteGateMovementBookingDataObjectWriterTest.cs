using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementBookingDataObjectWriter))]
	class GteGateMovementBookingDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteToDataObject()
		{
			#region Setup Test Data

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "BookingReference";
			booking.GBK_OH_TransportCompany = transportCompany.PK;

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_MovementBookingNumber = "BookingNum1";
			gateMovementBooking1.GBM_SourceReferenceNumber = "VBS001";
			gateMovementBooking1.GBM_BookingReferenceNumber = "gateMovementBooking1";
			gateMovementBooking1.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;
			gateMovementBooking1.GBM_SlotStartTime = new ZDateTimeOffset(2022, 1, 1, 10, 10, 10);
			gateMovementBooking1.GBM_UnitNumber = "Container001";
			gateMovementBooking1.GBM_TransportReference = "TRF_240419170136";
			gateMovementBooking1.GBM_RC_UnitType = refContainer.PK;
			gateMovementBooking1.GBM_Quantity = 1000;

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_MovementBookingNumber = "BookingNum2";
			gateMovementBooking2.GBM_BookingReferenceNumber = "gateMovementBooking2";
			gateMovementBooking2.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Package;
			gateMovementBooking2.GBM_SlotStartTime = new ZDateTimeOffset(2022, 1, 1, 10, 10, 10);
			gateMovementBooking2.GBM_UnitNumber = "Container002";
			gateMovementBooking2.GBM_Quantity = 2000;

			#endregion Setup Test Data

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);

			CombineAssertions(() =>
			{
				var subshipment1 = dataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.Value == "gateMovementBooking1");
				var subshipment2 = dataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.Value == "gateMovementBooking2");

				AssertNotNull("Expected to find a subshipment with BookingConfirmationReference of gateMovementBooking1", subshipment1);
				AssertNotNull("Expected to find a subshipment with BookingConfirmationReference of gateMovementBooking2", subshipment2);

				AssertEquals("Expect that Subshipment 1 contains a PackingLineDataObject", 1, subshipment1.PackingLineCollection.Count);
				AssertEquals("Expect that Subshipment 2 contains a PackingLineDataObject", 1, subshipment2.PackingLineCollection.Count);
				AssertEquals(Core.Constants.PkgUnit.Box, subshipment1.PackingLineCollection.FirstOrDefault().PackType.Code.Value);
				AssertEquals(Core.Constants.PkgUnit.Package, subshipment2.PackingLineCollection.FirstOrDefault().PackType.Code.Value);
				AssertEquals("Expect GBM_Quantity -> PackingLineDataObject.PackQty", gateMovementBooking1.GBM_Quantity, (subshipment1.PackingLineCollection.FirstOrDefault().PackQty ?? 0).ToZInt());
				AssertEquals("Expect GBM_Quantity -> PackingLineDataObject.PackQty", gateMovementBooking2.GBM_Quantity, (subshipment2.PackingLineCollection.FirstOrDefault().PackQty ?? 0).ToZInt());

				AssertEquals(Core.Constants.FacilityJobType.Codes.Container, subshipment1.FacilityJobType.Code);
				AssertEquals(Core.Constants.FacilityJobType.Codes.Cargo, subshipment2.FacilityJobType.Code);
				AssertEquals(gateMovementBooking1.GBM_UnitNumber, subshipment1.ContainerCollection.FirstOrDefault().ContainerNumber);
				AssertEquals(gateMovementBooking2.GBM_UnitNumber, subshipment2.ContainerCollection.FirstOrDefault().ContainerNumber);

				AssertEquals(refContainer.RC_Code, subshipment1.ContainerCollection.FirstOrDefault().ContainerType.Code);
				AssertEquals("Subshipment 2 should not have an associated container type.", null, subshipment2.ContainerCollection.FirstOrDefault().ContainerType.Code);

				AssertEquals("Expect that Subshipment 1 has a data source",1, subshipment1.DataContext.DataSourceCollection.Count());
				AssertEquals("Expect that the data source type of Subshipment 1 is GateMovementBooking", nameof(DataContextType.GateMovementBooking), subshipment1.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expect that the data source of Subshipment 1 is it's GBM_MovementBookingNumber", "BookingNum1", subshipment1.DataContext.DataSourceCollection.FirstOrDefault().Key);

				AssertEquals("Expect that Subshipment 2 has a data source",1, subshipment2.DataContext.DataSourceCollection.Count());
				AssertEquals("Expect that the data source type of Subshipment 2 is GateMovementBooking", nameof(DataContextType.GateMovementBooking), subshipment2.DataContext.DataSourceCollection.FirstOrDefault().Type);
				AssertEquals("Expect that the data source of Subshipment 2 is it's GBM_MovementBookingNumber", "BookingNum2", subshipment2.DataContext.DataSourceCollection.FirstOrDefault().Key);
			});

			CombineAssertions(() =>
			{
				var subshipment1 = dataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.Value == "gateMovementBooking1");
				var transportReference = subshipment1.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.Code == (ZString?)AdditionalReferenceTypes.Codes.TransportReference);
				var bookingPartyReference = subshipment1.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.Code == (ZString?)AdditionalReferenceTypes.Codes.BookingPartyReference);

				AssertEquals("Expect code to equal 'TRF'", AdditionalReferenceTypes.Codes.TransportReference, transportReference.Type.Code);
				AssertEquals("Expect Description to equal 'Transport Reference Number'", AdditionalReferenceTypes.Descriptions.TransportReference, transportReference.Type.Description);
				AssertEquals("Expect Reference number to be 'TRF_240419170136'", "TRF_240419170136", transportReference.ReferenceNumber);
				AssertEquals("Expect that the AdditionalReference for BookingPartyReference matches 'VBS001'", "VBS001", bookingPartyReference.ReferenceNumber);
			});
		}

		public void TestSetTransportBookingDirection()
		{
			#region Setup Test Data

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);

			#endregion Setup Test Data

			gateMovementBooking.GBM_IsPickup = false;
			var dataObjectDLV = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			Assert("Booking UXML should contain subsShipment.", dataObjectDLV.SubShipmentCollection.Count > 0);
			AssertNotNull("Subshipment TransportBookingDirection should be populated.", dataObjectDLV.SubShipmentCollection.FirstOrDefault().TransportBookingDirection);

			gateMovementBooking.GBM_IsPickup = true;
			var dataObjectPIC = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			Assert("Booking UXML should contain subsShipment", dataObjectPIC.SubShipmentCollection.Count > 0);
			AssertNotNull("Subshipment TransportBookingDirection should be populated.", dataObjectPIC.SubShipmentCollection.FirstOrDefault().TransportBookingDirection);

			var deliveryAssertionMsg = $"TransportBookingDirection is set to '{GateManagementConstants.TransportBookingDirections.Codes.Delivery}' if GBM_IsPickup is false.";
			var pickupAssertionMsg = $"TransportBookingDirection is set to '{GateManagementConstants.TransportBookingDirections.Codes.Pickup}' if GBM_IsPickup is true.";

			var actualDLVCode = dataObjectDLV.SubShipmentCollection.FirstOrDefault().TransportBookingDirection.Code;
			var actualPICCode = dataObjectPIC.SubShipmentCollection.FirstOrDefault().TransportBookingDirection.Code;

			AssertEquals(deliveryAssertionMsg, GateManagementConstants.TransportBookingDirections.Codes.Delivery, actualDLVCode);
			AssertEquals(pickupAssertionMsg, GateManagementConstants.TransportBookingDirections.Codes.Pickup, actualPICCode);
		}

		public void TestPackageTypeAndCargoType()
		{
			#region Setup Test Data

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);
			gateMovementBooking.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;
			gateMovementBooking.GBM_RH_NKCargoType = "MNSC";

			#endregion Setup Test Data

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var gateMovementBookingDataObject = bookingDataObject.SubShipmentCollection.FirstOrDefault();
			var packingLineCollectionDataObject = gateMovementBookingDataObject.PackingLineCollection;

			AssertNotNull("Expected packingLineCollection not to be null", packingLineCollectionDataObject);

			var packingLineDataObject = packingLineCollectionDataObject.FirstOrDefault();

			AssertNotNull("Expected packingLine not to be null", packingLineDataObject);
			AssertEquals("Expected packingLine PackType to be 'BOX'", Core.Constants.PkgUnit.Box, packingLineDataObject.PackType.Code);
			AssertEquals("Expected packingLine CargoType to be 'MNSC'", "MNSC", packingLineDataObject.Commodity.Code);
		}

		public void TestGivenSourceReferenceSet_WhenWriteToDataObject_ThenReferenceIsSet()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);
			gateMovementBooking.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;
			gateMovementBooking.GBM_RH_NKCargoType = "MNSC";
			gateMovementBooking.GBM_Source = "VBS";
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS001";

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var gateMovementBookingDataObject = bookingDataObject.SubShipmentCollection.FirstOrDefault();
			var sourceReference = gateMovementBookingDataObject.AdditionalReferenceCollection?.FirstOrDefault(x => (string)x.Type.Code == AdditionalReferenceTypes.Codes.BookingPartyReference);
			AssertNotNull("Expect a BPR to exist", sourceReference);
			AssertEquals("Expect Reference to equal SourceReferenceNumber", "VBS001", sourceReference.ReferenceNumber);
		}

		public void TestGivenSourceReferenceNotSet_WhenWriteToDataObject_ThenReferenceIsNotSet()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);
			gateMovementBooking.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;
			gateMovementBooking.GBM_RH_NKCargoType = "MNSC";

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);
			var gateMovementBookingDataObject = bookingDataObject.SubShipmentCollection.FirstOrDefault();
			var sourceReference = gateMovementBookingDataObject.AdditionalReferenceCollection?.FirstOrDefault(x => (string)x.Type.Code == AdditionalReferenceTypes.Codes.BookingPartyReference);
			AssertNull("Expect no BPR to exist", sourceReference);
		}

		public void TestSlotTimes()
		{
			#region Setup Test Data

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBookingWithoutSlotTimes = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithoutSlotTimes.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithoutSlotTimes.GBM_BookingReferenceNumber = (ZString)"NoSlots";

			var gateMovementBookingWithSlotStart = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithSlotStart.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithSlotStart.GBM_SlotStartTime = ZDateTimeOffset.Now;
			gateMovementBookingWithSlotStart.GBM_BookingReferenceNumber = (ZString)"SlotStartSet";

			var gateMovementBookingWithSlotEnd = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithSlotEnd.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithSlotEnd.GBM_SlotEndTime = ZDateTimeOffset.Now.AddDays(1);
			gateMovementBookingWithSlotEnd.GBM_BookingReferenceNumber = (ZString)"SlotEndSet";

			var gateMovementBookingWithBothSlots = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithBothSlots.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithBothSlots.GBM_SlotStartTime = ZDateTimeOffset.Now;
			gateMovementBookingWithBothSlots.GBM_SlotEndTime = ZDateTimeOffset.Now.AddDays(1);
			gateMovementBookingWithBothSlots.GBM_BookingReferenceNumber = (ZString)"SlotFullySet";

			#endregion Setup Test Data

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);

			var noSlotSetShipment = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "NoSlots");
			var slotStartSetShipment = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "SlotStartSet");
			var slotEndSetShipment = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "SlotEndSet");
			var slotFullySetShipment = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "SlotFullySet");

			CombineAssertions(() =>
			{
				AssertNull("Expected there to be no date collection when booking does not have a slot", noSlotSetShipment.DateCollection);
				AssertNotNull("Expected there to be a date collection when booking has at least one time set", slotStartSetShipment.DateCollection);
				AssertNotNull("Expected there to be a date collection when booking has at least one time set", slotEndSetShipment.DateCollection);
				AssertNotNull("Expected there to be a date collection when booking has at least one time set", slotFullySetShipment.DateCollection);

				Assert("Expected there to be only one date set when only SlotStartTime set", slotStartSetShipment.DateCollection.Count == 1);
				Assert("Expected there to be only one date set when only SlotEndTime set", slotStartSetShipment.DateCollection.Count == 1);
				Assert("Expected there to be two dates set when slot fully booked", slotFullySetShipment.DateCollection.Count == 2);

				Assert("Expected Start date to be set when SlotStartTime set", slotStartSetShipment.DateCollection.Any(date => date.Type == DateType.Start));
				Assert("Expected Start date to be set when SlotStartTime set", slotFullySetShipment.DateCollection.Any(date => date.Type == DateType.Start));

				Assert("Expected End date to be set when SlotEndTime set", slotEndSetShipment.DateCollection.Any(date => date.Type == DateType.End));
				Assert("Expected End date to be set when SlotEndTime set", slotFullySetShipment.DateCollection.Any(date => date.Type == DateType.End));

				AssertEquals("Expected Start date to contain correct value", slotStartSetShipment.DateCollection.First(date => date.Type == DateType.Start).Value, gateMovementBookingWithSlotStart.GBM_SlotStartTime.ToZDateTime());
				AssertEquals("Expected Start date to contain correct value", slotFullySetShipment.DateCollection.First(date => date.Type == DateType.Start).Value, gateMovementBookingWithBothSlots.GBM_SlotStartTime.ToZDateTime());

				AssertEquals("Expected End date to contain correct value", slotEndSetShipment.DateCollection.First(date => date.Type == DateType.End).Value, gateMovementBookingWithSlotEnd.GBM_SlotEndTime.ToZDateTime());
				AssertEquals("Expected End date to contain correct value", slotFullySetShipment.DateCollection.First(date => date.Type == DateType.End).Value, gateMovementBookingWithBothSlots.GBM_SlotEndTime.ToZDateTime());
			});
		}

		public void TestFacilityJobType()
		{
			#region Setup Test Data

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBookingWithoutUnitTypeOrNumber = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithoutUnitTypeOrNumber.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithoutUnitTypeOrNumber.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber000";

			var gateMovementBookingWithoutUnitType = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithoutUnitType.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithoutUnitType.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber001";
			gateMovementBookingWithoutUnitType.GBM_UnitNumber = "Container001";

			var gateMovementBookingWithoutUnitNumber = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithoutUnitNumber.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithoutUnitNumber.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber002";
			gateMovementBookingWithoutUnitNumber.GBM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;

			var gateMovementBookingWithUnitTypeAndNumber = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithUnitTypeAndNumber.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithUnitTypeAndNumber.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber003";
			gateMovementBookingWithUnitTypeAndNumber.GBM_UnitNumber = "Container003";
			gateMovementBookingWithUnitTypeAndNumber.GBM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;

			#endregion Setup Test Data

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);

			var gateMovementBookingUXML0 = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "UniqueBookingReferenceNumber000");
			var gateMovementBookingUXML1 = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "UniqueBookingReferenceNumber001");
			var gateMovementBookingUXML2 = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "UniqueBookingReferenceNumber002");
			var gateMovementBookingUXML3 = bookingDataObject.SubShipmentCollection.FirstOrDefault(s => s.BookingConfirmationReference.ToString() == "UniqueBookingReferenceNumber003");

			CombineAssertions(() =>
			{
				AssertEquals("Expected facility job type of Cargo", FacilityJobType.Codes.Cargo, gateMovementBookingUXML0.FacilityJobType.Code);
				AssertEquals("Expected facility job type of Cargo", FacilityJobType.Codes.Cargo, gateMovementBookingUXML1.FacilityJobType.Code);
				AssertEquals("Expected facility job type of Cargo", FacilityJobType.Codes.Cargo, gateMovementBookingUXML2.FacilityJobType.Code);
				AssertEquals("Expected facility job type of Container", FacilityJobType.Codes.Container, gateMovementBookingUXML3.FacilityJobType.Code);
			});
		}

		public void TestPopulateAdditionalReferenceCollection()
		{
			#region Setup UXML

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_TransportReference = "02";
			gateMovementBooking.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 11);
			gateMovementBooking.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber000";

			var gateMovementBookingWithNoTRF1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithNoTRF1.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithNoTRF1.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 10);
			gateMovementBookingWithNoTRF1.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber001";

			var gateMovementBookingWithNoTRF2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBookingWithNoTRF2.GBM_GBK_Booking = booking.PK;
			gateMovementBookingWithNoTRF2.GBM_SlotStartTime = new ZDateTimeOffset(2023, 1, 1, 10, 10, 12);
			gateMovementBookingWithNoTRF2.GBM_BookingReferenceNumber = "UniqueBookingReferenceNumber002";

			#endregion Setup UXML

			var bookingDataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, booking))).GetDataObject(booking);

			var gateMovementBookingUXML1 = bookingDataObject.SubShipmentCollection.FirstOrDefault(x => x.BookingConfirmationReference == (ZString?)"UniqueBookingReferenceNumber000");
			var gateMovementBookingUXML2 = bookingDataObject.SubShipmentCollection.FirstOrDefault(x => x.BookingConfirmationReference == (ZString?)"UniqueBookingReferenceNumber001");
			var gateMovementBookingUXML3 = bookingDataObject.SubShipmentCollection.FirstOrDefault(x => x.BookingConfirmationReference == (ZString?)"UniqueBookingReferenceNumber002");

			AssertNotNull("Expected there to be a TRF reference in gateMovementBooking1", gateMovementBookingUXML1.AdditionalReferenceCollection?.FirstOrDefault(r => r.Type.Code.Value == "TRF"));
			AssertNull("Expected there to be no TRF reference in gateMovementBooking2", gateMovementBookingUXML2.AdditionalReferenceCollection?.FirstOrDefault(r => r.Type.Code.Value == "TRF"));
			AssertNull("Expected there to be no TRF reference in gateMovementBooking3", gateMovementBookingUXML3.AdditionalReferenceCollection?.FirstOrDefault(r => r.Type.Code.Value == "TRF"));

			AssertEquals("Expected TRF with value", "02", gateMovementBookingUXML1.AdditionalReferenceCollection.FirstOrDefault(r => r.Type.Code.Value == "TRF").ReferenceNumber.Value);
		}
	}
}
