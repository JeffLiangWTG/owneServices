using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Module.Testing
{
	public abstract class DtbBookingOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		protected internal TransportBookingTestHelper Helper
		{
			get { return new TransportBookingTestHelper(Factory); }
		}

		const string BookingLockdownMessage = "Cannot modify the selected Transport Booking directly, it has been locked down.";

		public void TestApplyCoreDoesNotAddLockdownWarningToLogs_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBoookingIsNotSub()
		{
			AssertApplyCore_ReturnsCorrectBookingLockdownMessage(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				bookingIsSub: false,
				expectedMessage: string.Empty
				);
		}

		public void TestApplyCoreAddsLockdownWarningToLogs_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBoookingIsSub()
		{
			AssertApplyCore_ReturnsCorrectBookingLockdownMessage(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				bookingIsSub: true,
				expectedMessage: "WARNING: " + BookingLockdownMessage
				);
		}

		public void TestApplyCoreAddsLockdownWarningToLogs_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBoookingIsNotSub()
		{
			AssertApplyCore_ReturnsCorrectBookingLockdownMessage(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				bookingIsSub: false,
				expectedMessage: "WARNING: " + BookingLockdownMessage
				);
		}

		public void TestApplyCoreAddsLockdownWarningToLogs_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBoookingIsSub()
		{
			AssertApplyCore_ReturnsCorrectBookingLockdownMessage(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				bookingIsSub: true,
				expectedMessage: "WARNING: " + BookingLockdownMessage
				);
		}

		void AssertApplyCore_ReturnsCorrectBookingLockdownMessage(bool bookingIsBeingManagedByAuthorisedCarrierBookingAgent, bool bookingIsSub, string expectedMessage)
		{
			var potentiallyLockedDownBooking = Helper.CreateBooking();
			var otherBooking = Helper.CreateBooking();

			if (bookingIsBeingManagedByAuthorisedCarrierBookingAgent)
			{
				Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(potentiallyLockedDownBooking);
				potentiallyLockedDownBooking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			}
			else
			{
				AssertEquals("Precondition: Booking should not be being managed by an authorised Carrier Booking Agent.", false, potentiallyLockedDownBooking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
			}

			if (bookingIsSub)
			{
				var masterBooking = Helper.CreateBooking();
				masterBooking.KM_IsMaster = true;
				potentiallyLockedDownBooking.KM_KM_MasterBooking = masterBooking.PK;
			}
			else
			{
				AssertEquals("Precondition: Booking should not be a sub booking.", false, potentiallyLockedDownBooking.IsSub);
			}

			Factory.Save();

			if (string.IsNullOrEmpty(expectedMessage))
			{
				var logMessages = SimulateRun(new DtbBooking[] { potentiallyLockedDownBooking, otherBooking }, false).MessagesString();
				AssertEquals("No warning about being attached to a Master Booking should have been logged.", false, logMessages.Contains(BookingLockdownMessage));
			}
			else
			{
				ApplyApplicator(new DtbBooking[] { potentiallyLockedDownBooking, potentiallyLockedDownBooking }, expectedMessage);
			}
		}
	}
}
