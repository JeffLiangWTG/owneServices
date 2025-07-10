using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CarrierBookingAgentChangeActionMethodApplicator))]
	class CarrierBookingAgentChangeActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		[TestDate(2019, 1, 1, 0, 0, 0)]
		public void TestAction()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";

			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "TM00002";
			booking2.CarrierBookingAgentDocAddress.E2_OA_Address = org1.MainAddress.PK;

			var booking3 = Helper.CreateBooking();
			booking3.KM_JobID = "TM00003";
			booking3.CarrierBookingAgentDocAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			var bookings = new[] { booking1, booking2, booking3 };

			Applicator.CarrierBookingAgentPK = org2.PK;
			ApplyApplicator(bookings, @"INFO: [HL TM00001] - The action 'Action Name' was successfully processed.
INFO: [HL TM00002] - The action 'Action Name' was successfully processed.
INFO: [HL TM00003] - The action 'Action Name' was successfully processed.");

			AssertEquals(org2, booking1.CarrierBookingAgent);
			AssertEquals(org2, booking2.CarrierBookingAgent);
			AssertEquals(org2, booking3.CarrierBookingAgent);

			AssertEquals("EditedARecord event should be logged", true, IsEditLogEventCreated(booking1));
			AssertEquals("EditedARecord event should be logged", true, IsEditLogEventCreated(booking2));
			AssertEquals("EditedARecord event should not be logged for assigning the same carrier", false, IsEditLogEventCreated(booking3));
		}

		[TestDate(2019, 1, 1, 0, 0, 0)]
		public void TestAction_EmptyOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";
			booking1.CarrierBookingAgentDocAddress.E2_OA_Address = org1.MainAddress.PK;

			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "TM00002";
			booking2.CarrierBookingAgentDocAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			var bookings = new[] { booking1, booking2 };

			Applicator.CarrierBookingAgentPK = ZGuid.Empty;
			ApplyApplicator(bookings, @"INFO: [HL TM00001] - The action 'Action Name' was successfully processed.
INFO: [HL TM00002] - The action 'Action Name' was successfully processed.");

			AssertNull(booking1.CarrierBookingAgent);
			AssertNull(booking2.CarrierBookingAgent);

			AssertEquals("EditedARecord event should be logged", true, IsEditLogEventCreated(booking1));
			AssertEquals("EditedARecord event should be logged", true, IsEditLogEventCreated(booking2));
		}

		[TestDate(2019, 1, 1, 0, 0, 0)]
		public void TestAction_BookingInactive()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";
			booking1.SetReadOnlyIncludingChildren(true);

			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "TM00002";
			booking2.CarrierBookingAgentDocAddress.E2_OA_Address = org1.MainAddress.PK;
			booking2.SetReadOnlyIncludingChildren(true);

			Factory.Save();

			var bookings = new[] { booking1, booking2 };

			Applicator.CarrierBookingAgentPK = org2.PK;
			ApplyApplicator(bookings, @"WARNING: [HL TM00001] - The action 'Action Name' failed as the booking is read only.
WARNING: [HL TM00002] - The action 'Action Name' failed as the booking is read only.");

			AssertEquals(null, booking1.CarrierBookingAgent);
			AssertEquals(org1, booking2.CarrierBookingAgent);

			AssertEquals("EditedARecord event should not be logged", false, IsEditLogEventCreated(booking1));
			AssertEquals("EditedARecord event should not be logged", false, IsEditLogEventCreated(booking2));
		}

		[TestDate(2019, 1, 1, 0, 0, 0)]
		public void TestAction_BadOrgPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";

			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "TM00002";
			booking2.CarrierBookingAgentDocAddress.E2_OA_Address = org1.MainAddress.PK;

			Factory.Save();

			var bookings = new[] { booking1, booking2 };

			Applicator.CarrierBookingAgentPK = ZGuid.NewZGuid();
			ApplyApplicator(bookings, @"ERROR: Failed to load Carrier Booking Agent organization.");

			AssertNull(booking1.CarrierBookingAgent);
			AssertEquals(org1, booking2.CarrierBookingAgent);

			AssertEquals("EditedARecord event should not be logged", false, IsEditLogEventCreated(booking1));
			AssertEquals("EditedARecord event should not be logged", false, IsEditLogEventCreated(booking2));
		}

		bool IsEditLogEventCreated(DtbBooking booking)
		{
			return booking.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecord.Code && l.SL_EventTime.Equals(new ZDateTime(2019, 1, 1)) && l.ReferenceFreeText == "Assigned a Carrier Booking Agent via Operational Action.").Any();
		}

		public void TestCarrierBookingAgents()
		{
			AssertNotNull(Applicator.CarrierBookingAgents);
			AssertType<OrgHeaderCollection>(Applicator.CarrierBookingAgents);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CarrierBookingAgentChangeActionMethodApplicator("Action Name", Factory);
		}

		new CarrierBookingAgentChangeActionMethodApplicator Applicator => (CarrierBookingAgentChangeActionMethodApplicator)base.Applicator;
	}
}
