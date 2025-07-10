using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteBooking))]
	public sealed class GteBookingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			booking.GBK_ReferenceNumber = "1000";

			AssertEquals("Gate Booking 1000", booking.HumanReadableName);
		}

		public void TestGBK_ReferenceNumberGeneration()
		{
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_BookingType = Constants.BookingTypes.Regular;
			booking.GBK_OH_TransportCompany = transportCompany.PK;
			booking.GBK_ReferenceNumber = ZString.Empty;

			AssertEquals("Precondition: Expected Reference Number to be empty.", ZString.Empty, booking.GBK_ReferenceNumber);

			Factory.Save();

			AssertEquals("Expected GBK_ReferenceNumber to be populated by number fountain if empty.", "GB00001000", booking.GBK_ReferenceNumber);

			booking.GBK_ReferenceNumber = "ReferenceNumber3000";

			Factory.Save();

			AssertEquals("Expected GBK_ReferenceNumber to be unchanged by number fountain if not empty.", "ReferenceNumber3000", booking.GBK_ReferenceNumber);
		}

		public void TestGivenNewGteBooking_WhenOnSaving_ThenCreateLogEvent()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_SourceReferenceNumber = "10000";

			booking.OnSaving();

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			AssertNotNull("Expected a booking pending log on the GteBooking when saved", log);
			AssertEquals("|JOB=10000|RFN=12345", log.SL_Reference);
		}

		public void TestGivenNewGteBookingWithGteGateMovementBooking_WhenOnSaving_ThenCreateLogEvent()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_SourceReferenceNumber = "10000";

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_FacilityTableCode = "YDL";

			booking.OnSaving();

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			AssertNotNull("Expected a booking pending log on the GteBooking when saved", log);
			AssertEquals("|FAC=YDL|JOB=10000|RFN=12345", log.SL_Reference);
		}

		public void TestGivenNewGteBookingWithNoReferenceNumber_WhenOnSaving_ThenCreateLogEventWithNumberFountainReferenceNumber()
		{
			var booking = Factory.New<GteBooking>();

			booking.OnSaving();

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			AssertNotNull("Expected a booking pending log on the GteBooking when saved", log);
			AssertEquals("|RFN=GB00001000", log.SL_Reference);
		}

		public void TestGivenNewGteBookingWithMultipleFacilityTableCodes_WhenOnSaving_ThenCreateLogEventWithAllFacilityCodes()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_SourceReferenceNumber = "10000";

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_FacilityTableCode = "YDL";

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_FacilityTableCode = "TPW";

			booking.OnSaving();

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			AssertNotNull("Expected a booking pending log on the GteBooking when saved", log);
			AssertEquals("|FAC=TPW,YDL|JOB=10000|RFN=12345", log.SL_Reference);
		}

		public void TestGivenNewGteBookingWithDuplicateFacilityTableCodes_WhenOnSaving_ThenCreateLogEventWithUniqueFacilityCodes()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_SourceReferenceNumber = "10000";

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_FacilityTableCode = "YDL";

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_FacilityTableCode = "TPW";

			var gateMovementBooking3 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking3.GBM_GBK_Booking = booking.PK;
			gateMovementBooking3.GBM_FacilityTableCode = "YDL";

			var gateMovementBooking4 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking4.GBM_GBK_Booking = booking.PK;
			gateMovementBooking4.GBM_FacilityTableCode = "TPW";

			var gateMovementBooking5 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking5.GBM_GBK_Booking = booking.PK;
			gateMovementBooking5.GBM_FacilityTableCode = "ABC";

			booking.OnSaving();

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			AssertNotNull("Expected a booking pending log on the GteBooking when saved", log);
			AssertEquals("|FAC=ABC,TPW,YDL|JOB=10000|RFN=12345", log.SL_Reference);
		}

		public void TestPropagateBookingCancellation()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_Source = "VBS";
			booking.GBK_SourceReferenceNumber = "10000";

			var gbm1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gbm2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gbm1.GBM_GBK_Booking = booking.PK;
			gbm2.GBM_GBK_Booking = booking.PK;
			gbm1.GBM_SourceReferenceNumber = "00001";
			gbm2.GBM_SourceReferenceNumber = "00002";

			var gbv1 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			var gbv2 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			var gbv3 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			gbv1.GBV_GBK_Booking = booking.PK;
			gbv2.GBV_GBK_Booking = booking.PK;
			gbv3.GBV_GBK_Booking = booking.PK;

			var gbd1 = Factory.NewWithValidTestData<GteVehicleDriverBooking>();
			var gbd2 = Factory.NewWithValidTestData<GteVehicleDriverBooking>();
			var gbd3 = Factory.NewWithValidTestData<GteVehicleDriverBooking>();
			gbd1.GBD_GBK_Booking = booking.PK;
			gbd2.GBD_GBK_Booking = booking.PK;
			gbd3.GBD_GBK_Booking = booking.PK;

			gbm1.GBM_CancelledReason = "Movement Booking 1 - Cancelled for test";
			gbm1.GBM_CancelledTime = DateTime.Now;
			gbm1.GBM_GS_NKCancelledBy = "US1";
			gbm1.GBM_CancelledSource = "TST";

			AssertNull("Pre-condition: Booking should not have a BKL event", booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingCancelledCode).FirstOrDefault());
			Assert("Should not propagate cancellation if not all gate movement bookings are cancelled.", !booking.PropagateBookingCancellation(gbm1));

			gbm2.GBM_CancelledReason = "Movement Booking 2 - Cancelled for test";
			gbm2.GBM_CancelledTime = DateTime.Now;
			gbm2.GBM_GS_NKCancelledBy = "US2";
			gbm2.GBM_CancelledSource = Constants.DataSources.VehicleBookingSystem;

			Assert("Should propagate cancellation if all gate movement bookings are cancelled.", booking.PropagateBookingCancellation(gbm2));

			var log = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingCancelledCode).FirstOrDefault();
			AssertNotNull("Expected a booking log on the GteBooking when cancelled", log);
			AssertEquals("|JOB=12345|RES=Movement Booking 2 - Cancelled for test|RFN=00001,00002|SRC=VBS", log.SL_Reference);

			CombineAssertions("Correct 'CancelledReason' should be propagated to all related entities", () =>
			{
				AssertEquals("gbv1.GBV_CancelledReason", gbv1.GBV_CancelledReason, gbm2.GBM_CancelledReason);
				AssertEquals("gbv2.GBV_CancelledReason", gbv2.GBV_CancelledReason, gbm2.GBM_CancelledReason);
				AssertEquals("gbv3.GBV_CancelledReason", gbv3.GBV_CancelledReason, gbm2.GBM_CancelledReason);

				AssertEquals("gbd1.GBD_CancelledReason", gbd1.GBD_CancelledReason, gbm2.GBM_CancelledReason);
				AssertEquals("gbd2.GBD_CancelledReason", gbd2.GBD_CancelledReason, gbm2.GBM_CancelledReason);
				AssertEquals("gbd3.GBD_CancelledReason", gbd3.GBD_CancelledReason, gbm2.GBM_CancelledReason);
			});

			CombineAssertions("Correct 'CancelledTime' should be propagated to all related entities", () =>
			{
				AssertEquals("gbv1.GBV_CancelledTime", gbv1.GBV_CancelledTime, gbm2.GBM_CancelledTime);
				AssertEquals("gbv2.GBV_CancelledTime", gbv2.GBV_CancelledTime, gbm2.GBM_CancelledTime);
				AssertEquals("gbv3.GBV_CancelledTime", gbv3.GBV_CancelledTime, gbm2.GBM_CancelledTime);

				AssertEquals("gbd1.GBD_CancelledTime", gbd1.GBD_CancelledTime, gbm2.GBM_CancelledTime);
				AssertEquals("gbd2.GBD_CancelledTime", gbd2.GBD_CancelledTime, gbm2.GBM_CancelledTime);
				AssertEquals("gbd3.GBD_CancelledTime", gbd3.GBD_CancelledTime, gbm2.GBM_CancelledTime);
			});

			CombineAssertions("Correct 'CancelledBy' should be propagated to all related entities", () =>
			{
				AssertEquals("gbv1.GBV_GS_NKCancelledBy", gbv1.GBV_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);
				AssertEquals("gbv2.GBV_GS_NKCancelledBy", gbv2.GBV_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);
				AssertEquals("gbv3.GBV_GS_NKCancelledBy", gbv3.GBV_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);

				AssertEquals("gbd1.GBD_GS_NKCancelledBy", gbd1.GBD_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);
				AssertEquals("gbd3.GBD_GS_NKCancelledBy", gbd3.GBD_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);
				AssertEquals("gbd2.GBD_GS_NKCancelledBy", gbd2.GBD_GS_NKCancelledBy, gbm2.GBM_GS_NKCancelledBy);
			});

			CombineAssertions("Correct 'CancelledSource' should be propagated to all related entities", () =>
			{
				AssertEquals("gbv1.GBV_CancelledSource", gbv1.GBV_CancelledSource, gbm2.GBM_CancelledSource);
				AssertEquals("gbv2.GBV_CancelledSource", gbv2.GBV_CancelledSource, gbm2.GBM_CancelledSource);
				AssertEquals("gbv3.GBV_CancelledSource", gbv3.GBV_CancelledSource, gbm2.GBM_CancelledSource);

				AssertEquals("gbd1.GBD_CancelledSource", gbd1.GBD_CancelledSource, gbm2.GBM_CancelledSource);
				AssertEquals("gbd2.GBD_CancelledSource", gbd2.GBD_CancelledSource, gbm2.GBM_CancelledSource);
				AssertEquals("gbd3.GBD_CancelledSource", gbd3.GBD_CancelledSource, gbm2.GBM_CancelledSource);
			});

			CombineAssertions("Cancellation information for previously cancelled record should not be overriden", () =>
			{
				AssertEquals("Movement Booking 1 - Cancelled for test", gbm1.GBM_CancelledReason);
				AssertEquals("US1", gbm1.GBM_GS_NKCancelledBy);
				AssertEquals("TST", gbm1.GBM_CancelledSource);
			});
		}

		public void TestGteBookingDocumentaryAddressesHasCorrectRestrictions()
		{
			var booking = Factory.New<GteBooking>();
			AssertEquals(typeof(JobDocAddressDependentCollection), booking.DocAddresses.GetType());
			AssertEquals(booking.DocAddresses.Count, 0);

			var bookingAddresses = (IDocAddresses)Factory.New<GteBooking>();
			AssertSequencesEqual(new DocAddressType[] { DocAddressType.BookingPartyDocumentaryAddress }, bookingAddresses.SupportedAddressTypes);

			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals(false, bookingAddresses.CanDeleteAddress(jobDocAddress));
			AssertEquals(Env.Security.None, bookingAddresses.GetCanOverrideCheckpoint(jobDocAddress));
			AssertNull("GetDocAddressRequirement", bookingAddresses.GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress));
			AssertNull("GetOrgHeaderList", bookingAddresses.GetOrgHeaderList(DocAddressType.BookingPartyDocumentaryAddress));
			AssertNull("PiggyBackedDocAddressValidation", bookingAddresses.PiggyBackedDocAddressValidation(jobDocAddress));
		}

		public void TestBookingPartyDocumentaryAddressPropertyIsCorrect()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var bookingPartyJobDocAddress = JobDocAddress.New(booking, DocAddressType.BookingPartyDocumentaryAddress);
			booking.DocAddresses.Add(bookingPartyJobDocAddress);

			AssertEquals("BookingPartyDocumentaryAddress property is the booking party address added into the booking", bookingPartyJobDocAddress, booking.BookingPartyDocumentaryAddress);
		}

		public void TestBookingFacilityFieldCorrectlyMatchesLinkedFacility()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();

			booking.GBK_WW_Facility = facility.PK;
			AssertEquals(facility.PK, booking.Facility.PK);
		}

		public void TestGetMessageRecipientParty_WhenPartyTypeIsBookingParty_ReturnsBookingPartyDocumentaryAddressParty()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("Should not return a Party when there is no BookingPartyDocumentaryAddress", null, booking.GetMessageRecipientParty(MessageRecipientPartyTypeList.Codes.BookingParty).Party);
			booking.DocAddresses.Add(address);
			AssertEquals("Party should match when there is no BookingPartyDocumentaryAddress", address.Organisation, booking.GetMessageRecipientParty(MessageRecipientPartyTypeList.Codes.BookingParty).Party);
		}

		public void TestGetMessageRecipientParty_WhenPartyTypeIsArrivalTransitWarehouse_ReturnsTransitWarehouseOrganisation()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			address.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			facility.WW_OA_WarehouseAddress = address.PK;
			facility.WW_WarehouseType = "PRW";
			booking.GBK_WW_Facility = Guid.Empty;

			AssertEquals("Should not return a Party when there is no Facility", null, booking.GetMessageRecipientParty(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse).Party);
			booking.GBK_WW_Facility = facility.PK;
			AssertEquals("Should not return a Party when Facility is not a Transit Warehouse", null, booking.GetMessageRecipientParty(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse).Party);
			facility.WW_WarehouseType = "TRW";
			AssertEquals("Party should match when there is a Transit Warehouse", address.Header, booking.GetMessageRecipientParty(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse).Party);
		}
	}
}
