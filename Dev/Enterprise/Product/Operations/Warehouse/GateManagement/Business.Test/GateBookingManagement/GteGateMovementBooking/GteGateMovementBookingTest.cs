using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementBooking))]
	public sealed class GteGateMovementBookingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGivenNewGteGateMovementBooking_WhenOnSaving_ThenCreateLogEvent()
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";
			booking.GBK_SourceReferenceNumber = "10000";

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_UnitNumber = "001";
			gateMovementBooking1.GBM_RC_UnitType = refContainer.PK;
			gateMovementBooking1.GBM_FacilityTableCode = "YDL";

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_UnitNumber = "002";
			gateMovementBooking2.GBM_RC_UnitType = refContainer.PK;
			gateMovementBooking2.GBM_FacilityTableCode = "TPW";

			gateMovementBooking1.OnSaving();
			gateMovementBooking2.OnSaving();

			var gateMovementBooking1Log = gateMovementBooking1.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();
			var gateMovementBooking2Log = gateMovementBooking2.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingPendingCode).FirstOrDefault();

			AssertNotNull("Expected a booking pending log on the gateMovementBooking1 when saved", gateMovementBooking1Log);
			AssertNotNull("Expected a booking pending log on the gateMovementBooking2 when saved", gateMovementBooking2Log);
			AssertEquals("|EQN=001|FAC=YDL|JOB=10000|RFN=12345", gateMovementBooking1Log.SL_Reference);
			AssertEquals("|EQN=002|FAC=TPW|JOB=10000|RFN=12345", gateMovementBooking2Log.SL_Reference);
		}

		public void TestGivenCancelledOnSave_SetCancellationFields_AndCancellationLogic()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_Source = Constants.DataSources.VehicleBookingSystem;
			gateMovementBooking.GBM_SourceReferenceNumber = "100000";

			CombineAssertions("Pre-conditions", () =>
			{
				AssertEquals("Cancelled source is empty", string.Empty, gateMovementBooking.GBM_CancelledSource);
				AssertEquals("Cancelled by is empty", string.Empty, gateMovementBooking.GBM_GS_NKCancelledBy);
				AssertEquals("Cancelled time is empty", ZDateTimeOffset.Empty, gateMovementBooking.GBM_CancelledTime);
			});

			Factory.Save();

			gateMovementBooking.GBM_CancelledReason = "Cancelled for Test";
			gateMovementBooking.OnSaving();

			AssertEquals("GBM_CancelledSource", Constants.DataSources.VehicleBookingSystem, gateMovementBooking.GBM_CancelledSource);
			AssertEquals("GBM_GS_NKCancelledBy", GlbStaff.CurrentUser.GS_Code, gateMovementBooking.GBM_GS_NKCancelledBy);
			AssertNotEquals("GBM_CancelledTime", ZDateTimeOffset.Empty, gateMovementBooking.GBM_CancelledTime);
		}

		public void TestGivenHasMovementBookingNumber_WhenOnSaving_ThenMovementBookingNumberUnchanged()
		{
			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "GBM001";

			gateMovementBooking.OnSaving();

			AssertEquals("Expected GBM_MovementBookingNumber to be unchanged", "GBM001", gateMovementBooking.GBM_MovementBookingNumber);
		}

		public void TestGivenHasNoMovementBookingNumber_WhenOnSaving_ThenMovementBookingNumberSet()
		{
			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_MovementBookingNumber = "";
			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_MovementBookingNumber = "";

			gateMovementBooking1.OnSaving();
			gateMovementBooking2.OnSaving();

			AssertEquals("Expected GBM_MovementBookingNumber to be populated on first MovementBooking", "GBM00000001", gateMovementBooking1.GBM_MovementBookingNumber);
			AssertEquals("Expected GBM_MovementBookingNumber to be populated on second MovementBooking", "GBM00000002", gateMovementBooking2.GBM_MovementBookingNumber);
		}
	}
}
