using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingProcessTaskCollection))]
	class DtbBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbBookingProcessTaskCollection>
	{
		protected override DtbBookingProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbBookingProcessTaskCollection(Helper.CreateBooking());
		}

		public void TestCollectionFilter()
		{
			var booking = Factory.New<DtbBooking>();

			var trigger1 = booking.WorkflowItems.AddNew();
			var trigger2 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: TriggerLineTypes.Codes.DtbBookingConfirmation, p9_SE_NKMilestoneEvent: AutoEvents.ArrivalCode);
			var trigger3 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: TriggerLineTypes.Codes.DtbBookingConfirmation, p9_SE_NKMilestoneEvent: AutoEvents.DepartureCode);
			var trigger4 = booking.WorkflowItems.AddNew().With(p9_LineTriggerType: "XXX", p9_SE_NKMilestoneEvent: AutoEvents.DepartureCode);

			var bookingInstruction = booking.Instructions.AddNew();
			var bookingConfirmation = bookingInstruction.Confirmations.AddNew();
			var evnt = bookingConfirmation.Logs.AddNew(AutoEvents.Departure);

			var filter = new ZQuery();
			filter.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, TriggerLineTypes.Codes.DtbBookingConfirmation);
			filter.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, evnt.SL_SE_NKEvent);

			var bookingConfirmations = new DtbBookingProcessTaskCollection(booking, filter);
			bookingConfirmations.Load();

			AssertEquals(1, bookingConfirmations.Count);
			AssertContainsExactElementsInAnyOrder(trigger3, bookingConfirmations);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
