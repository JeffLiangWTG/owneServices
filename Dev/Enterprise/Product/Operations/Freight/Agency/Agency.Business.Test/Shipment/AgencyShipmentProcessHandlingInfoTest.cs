using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestGetCascadingTargets_ShipmentHasContainers_ReturnContainersProcessTasksAsTargets()
		{
			var shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_IsShipping = true;
			var shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_IsShipping = true;
			var container1 = shipment1.ShippingContainers.AddNew();
			var container2 = shipment1.ShippingContainers.AddNew();
			var container3 = shipment2.ShippingContainers.AddNew();
			var processTasks = new[] { //				Test 1		Test 2
			container1.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: true), //	[0]	. . . . . X
 container1.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: true), //	[1]
 container1.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: false), //	[2]
 container1.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: false), //	[3]
 container2.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: true), //	[4] . . . . . X
 container2.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: true), //	[5]
 container2.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: false), //	[6]
 container2.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: false), //	[7]
 container3.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: true), //	[8]
 container3.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: true), //	[9] . . . . . . . . . . . X
 container3.WorkflowItems.AddMilestone(linkedToEvent: "Z00", includingCascaded: false), //	[10]
 container3.WorkflowItems.AddMilestone(linkedToEvent: "Z11", includingCascaded: false), //	[11]
 };
			Factory.Save();
			// Test 1
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_Table = "OrgHeader";
				eventLog.SL_Parent = ZGuid.NewZGuid();
				eventLog.SL_SE_NKEvent = Events.CustomisableEvent00Code;
			}

			var handler = new AgencyShipmentProcessHandlingInfo(shipment1);
			var actualTargets = handler.GetCascadingTargets(eventLog);
			var expectedTargets = new[] { new CascadingLink { Parent = container1, Triggers = new[] { processTasks[0] } }, new CascadingLink { Parent = container2, Triggers = new[] { processTasks[4] } } };
			AssertContainsExactElementsInAnyOrder(CascadingLinkComparer.ByPKs, expectedTargets, actualTargets);
			// Test 2
			eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_Table = "OrgHeader";
				eventLog.SL_Parent = ZGuid.NewZGuid();
				eventLog.SL_SE_NKEvent = Events.CustomisableEvent11Code;
			}

			handler = new AgencyShipmentProcessHandlingInfo(shipment2);
			actualTargets = handler.GetCascadingTargets(eventLog);
			expectedTargets = new[] { new CascadingLink { Parent = container3, Triggers = new[] { processTasks[9] } } };
			AssertContainsExactElementsInAnyOrder(CascadingLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetCascadingTargets_DoNotReturnProcessTasksWithMovementEventsFromContenerisedContainers()
		{
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.FreightUnloaded, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.GateOut, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.GateIn, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.Dehire, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.GateOut, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.GateIn, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.FreightLoaded, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.FreightUnloaded, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.GateOut, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.GateIn, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.Dehire, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.GateOut, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.GateIn, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.FreightLoaded, false);
			AssertProcessTaskCascaded(Constants.ContainerModes.Liquid, Events.Departure, true);
			AssertProcessTaskCascaded(Constants.ContainerModes.FCL, Events.Departure, true);
		}

		void AssertProcessTaskCascaded(string containerMode, Event evnt, bool expectCascading)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_IsShipping = true;
			shipment.JS_PackingMode = containerMode;
			var container = shipment.ShippingContainers.AddNew();
			container.JC_ContainerMode = containerMode;
			var processTask = container.WorkflowItems.AddMilestone(evnt.Code, true);
			Factory.Save();
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_Table = "JobShipment";
				eventLog.SL_Parent = shipment.PK;
				eventLog.SL_SE_NKEvent = evnt.Code;
			}

			var handler = new AgencyShipmentProcessHandlingInfo(shipment);
			var actualTargets = handler.GetCascadingTargets(eventLog);
			if (expectCascading)
			{
				AssertEquals("Targets count", 1, actualTargets.Count());
				AssertEquals("Parent", container.PK, actualTargets.First().Parent.LogsParentPK);
				AssertEquals("Process tasks count", 1, actualTargets.First().Triggers.Length);
				AssertEquals("Process task", processTask.PK, actualTargets.First().Triggers.First().Identifier);
			}
			else
			{
				AssertEquals("Targets count", 0, actualTargets.Count());
			}
		}
	}
}
