using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region TestPropagationTargets

		public void TestPropagationTargets()
		{
			var booking = Helper.CreateBooking("TB123");
			var consignmentConsol = Helper.CreateConsolidation(booking);
			var consignment1 = Helper.CreateBookingConsignment(consignmentConsol);
			var consignment2 = Helper.CreateBookingConsignment(consignmentConsol);

			var bookingWorkflowItem = ((IWorkflowProvider)booking).WorkflowItems.AddNew();
			bookingWorkflowItem.P9_Type = Constants.Workflow.WorkflowTriggerType;
			bookingWorkflowItem.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;
			//bookingWorkflowItem.P9_RespondToCascadedEvents = true;
			//bookingWorkflowItem.ProcessTaskNotifications.AddNew();

			// only 1 of 2 consignments has the event, should not propagate to the booking
			consignment1.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("Only 1 of 2 Consignments has the event, should not propagate to the parent Booking.", true, bookingWorkflowItem.P9_ActualDate.IsEmpty);

			// all consignments have the event, should propagate
			var consignment2Log = consignment2.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("All Outers have the FLB event, should propagate to the parent dummy.", consignment2Log.SL_EventTime, bookingWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		#endregion

		#region TestPropagationTargets_WithInvalidParent

		public void TestPropagationTargets_WithInvalidParent()
		{
			// deleted consignment
			var deletedConsignment = Helper.CreateBookingConsignment();
			var logFromDeletedConsignment = deletedConsignment.Logs.AddNew(Events.FreightLoaded);
			deletedConsignment.Delete();
			AssertEquals("Consignment is deleted.", 0, new DtbBookingConsignmentProcessHandlingInfo(deletedConsignment).GetPropagationTargets(logFromDeletedConsignment).Count());

			// no consolidation (should not actually occur in production)
			var consignmentWithNoConsol = Factory.New<DtbBookingConsignment>();
			AssertEquals("Consignment has no Consol.", 0,
				new DtbBookingConsignmentProcessHandlingInfo(consignmentWithNoConsol).GetPropagationTargets(consignmentWithNoConsol.Logs.AddNew(Events.FreightLoaded)).Count());

			// no parent Booking
			var consignmentWithNoBooking = Helper.CreateBookingConsignment();
			AssertEquals("Consignment has no parent Booking.", 0,
				new DtbBookingConsignmentProcessHandlingInfo(consignmentWithNoBooking).GetPropagationTargets(consignmentWithNoBooking.Logs.AddNew(Events.FreightLoaded)).Count());
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
