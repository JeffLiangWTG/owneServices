using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerProcessHandlingInfoForBillOfLadingTest : CommonContainerProcessHandlingInfoTest
	{
		public void TestEventsPropagationToBOL_AllBookedContainersHasEvent_EventShouldNotBePropagated()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = billOfLading.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			var container1 = billOfLading.BookedContainers.AddNew();
			var container2 = billOfLading.BookedContainers.AddNew();
			Factory.Save();
			var container1Log = container1.Logs.AddNew(Events.Manifested);
			var container2Log = container2.Logs.AddNew(Events.Manifested);
			Factory.Save();
			AssertEquals("BillOfLading workflow item has an expected actual date", ZDateTime.Empty, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals(typeof(BillOfLading), shipmentWorkflowItem.Parent.GetType());
			var workflowParent = shipmentWorkflowItem.Parent as BillOfLading;
			AssertEquals(2, workflowParent.BookedContainers.Count);
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.BookedContainers[0].GetType());
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.BookedContainers[1].GetType());
		}

		public void TestEventsPropagationToBOL_AllRealContainersHasEvent_EventShouldBePropagated()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = billOfLading.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			var container1 = billOfLading.RealContainers.AddNew();
			var container2 = billOfLading.RealContainers.AddNew();
			Factory.Save();
			var container1Log = container1.Logs.AddNew(Events.Manifested);
			var container2Log = container2.Logs.AddNew(Events.Manifested);
			Factory.Save();
			AssertEquals("BillOfLading workflow item has an expected actual date", container2Log.SL_EventTime, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals(typeof(BillOfLading), shipmentWorkflowItem.Parent.GetType());
			var workflowParent = shipmentWorkflowItem.Parent as BillOfLading;
			AssertEquals(2, workflowParent.RealContainers.Count);
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.RealContainers[0].GetType());
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.RealContainers[1].GetType());
		}

		public void TestEventsPropagationToBOL_FewRealContainersHasEvent_EventShouldNotBePropagated()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = billOfLading.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			var container1 = billOfLading.RealContainers.AddNew();
			var container2 = billOfLading.RealContainers.AddNew();
			Factory.Save();
			var container1Log = container1.Logs.AddNew(Events.Manifested);
			Factory.Save();
			AssertEquals("BillOfLading workflow item has an expected actual date", ZDateTime.Empty, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals(typeof(BillOfLading), shipmentWorkflowItem.Parent.GetType());
			var workflowParent = shipmentWorkflowItem.Parent as BillOfLading;
			AssertEquals(2, workflowParent.RealContainers.Count);
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.RealContainers[0].GetType());
			AssertEquals(typeof(BillOfLadingContainer), workflowParent.RealContainers[1].GetType());
		}

		protected override CommonContainerProcessHandlingInfo GetInstance()
		{
			var container = Factory.New<BillOfLadingContainer>();
			return new AgencyShipmentContainerProcessHandlingInfo(container);
		}
	}
}
