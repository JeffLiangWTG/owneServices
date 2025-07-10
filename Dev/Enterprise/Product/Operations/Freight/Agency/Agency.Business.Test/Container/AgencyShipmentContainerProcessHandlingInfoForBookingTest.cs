using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerProcessHandlingInfoForBookingTest : CommonContainerProcessHandlingInfoTest
	{
		public void TestEventsPropagationToBookings_NotSupported()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = booking.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			var container1 = booking.BookedContainers.AddNew();
			var container2 = booking.BookedContainers.AddNew();
			Factory.Save();
			var container1Log = container1.Logs.AddNew(Events.Manifested);
			var container2Log = container2.Logs.AddNew(Events.Manifested);
			Factory.Save();
			AssertEquals("Event has not been propagated to booking: bookings are not supported in propagation", ZDateTime.Empty, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals(typeof(AgencyBooking), shipmentWorkflowItem.Parent.GetType());
			var workflowParent = shipmentWorkflowItem.Parent as AgencyBooking;
			AssertEquals(2, workflowParent.BookedContainers.Count);
			AssertEquals(typeof(AgencyBookingContainer), workflowParent.BookedContainers[0].GetType());
			AssertEquals(typeof(AgencyBookingContainer), workflowParent.BookedContainers[1].GetType());
		}

		protected override CommonContainerProcessHandlingInfo GetInstance()
		{
			var container = Factory.New<AgencyBookingContainer>();
			return new AgencyShipmentContainerProcessHandlingInfo(container);
		}
	}
}
