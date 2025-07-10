using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingConfirmationProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestGetParentProcessTasks_BookingConfirmationHasNotBooking_ReturnEmptyCollection()
		{
			var booking = Factory.New<DtbBooking>();
			var bookingInstruction = booking.Instructions.AddNew();
			var bookingConfirmation = bookingInstruction.Confirmations.AddNew();
			var log = bookingConfirmation.Logs.AddNew();
			var handlingInfo = new DtbBookingConfirmationProcessHandlingInfo(bookingConfirmation);

			var actualParents = handlingInfo.GetParentTriggers(log);
			var expectedParents = Array.Empty<IProcessTask>();
			AssertContainsExactElementsInAnyOrder("Parents", expectedParents, actualParents);
		}

		public void TestGetParentProcessTasks_BookingConfirmationHasBooking_ReturnHeaderProcessTasks()
		{
			var booking = Factory.New<DtbBooking>();
			var trigger1 = booking.WorkflowItems.AddNew();
			var trigger2 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: TriggerLineTypes.Codes.DtbBookingConfirmation, p9_SE_NKMilestoneEvent: AutoEvents.ArrivalCode);
			var trigger3 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: TriggerLineTypes.Codes.DtbBookingConfirmation, p9_SE_NKMilestoneEvent: AutoEvents.DepartureCode);
			var trigger4 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: "XXX", p9_SE_NKMilestoneEvent: AutoEvents.DepartureCode);

			var booking2 = Factory.New<DtbBooking>();
			var trigger5 = booking2.WorkflowItems.AddNew().With(p9_LineTriggerType: TriggerLineTypes.Codes.DtbBookingConfirmation, p9_SE_NKMilestoneEvent: AutoEvents.DepartureCode);

			var bookingInstruction = booking.Instructions.AddNew();
			var bookingConfirmation = bookingInstruction.Confirmations.AddNew();
			var evnt = bookingConfirmation.Logs.AddNew(AutoEvents.Departure);
			var handlingInfo = new DtbBookingConfirmationProcessHandlingInfo(bookingConfirmation);

			var actualParents = handlingInfo.GetParentTriggers(evnt);
			var expectedParents = new[] { trigger3 };
			AssertContainsExactElementsInAnyOrder("Parents", expectedParents, actualParents);
		}
	}
}
