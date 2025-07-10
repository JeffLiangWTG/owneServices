using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerProcessHandlingInfoTest : CommonContainerProcessHandlingInfoTest
	{
		public void TestEventsOnContainerShouldFireConsolAndShipmentWorkflow()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var consolWorkflowItem = consol.WorkflowItems.AddNew();
			consolWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			consolWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolWorkflowItem.ProcessTaskNotifications.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = shipment.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();

			var container = Factory.New<ForwardingContainer>();
			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_ActualVolume = 10;

			container.AddPackLine(packLine);
			consol.Containers.Add(container);
			shipment.OuterPackLines.Add(packLine);

			Factory.Save();

			var containerLog = container.Logs.AddNew(Events.Manifested);

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("Propogate: Consol Workflow Item Actual Date", containerLog.SL_EventTime, consolWorkflowItem.P9_ActualDate.ToZDateTime());

				var workflowEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				AssertEquals("Propogate: One WTE Event add to consol's process task", 1, consolWorkflowItem.Logs.Find(workflowEventFilter).Length);
				AssertEquals("Cascade: No WTE Event add to shipment's process task", 1, shipmentWorkflowItem.Logs.Find(workflowEventFilter).Length);
			});
		}

		public void TestEventOnContainerShouldFireConsolWorkflowOnlyWhenOtherContainersHaveSameEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var consolWorkflowItem = consol.WorkflowItems.AddNew();
			consolWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			consolWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolWorkflowItem.ProcessTaskNotifications.AddNew();

			var container1 = Factory.New<ForwardingContainer>();
			var container2 = Factory.New<ForwardingContainer>();
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			Factory.Save();

			var container1Log = container1.Logs.AddNew(Events.Manifested);

			AssertEquals("Propogate: Event only on first consol. No propagation.", ZDateTime.Empty, consolWorkflowItem.P9_ActualDate.ToZDateTime());

			var container2Log = container2.Logs.AddNew(Events.Manifested);

			AssertEquals("Propogate: Consol Workflow Item Actual Date", container2Log.SL_EventTime, consolWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		public void TestEventOnDeclarationContainerShouldFireDeclarationWorkflow_GOU()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;

			var cusContainer2 = cusContainers.AddNew();
			cusContainer2[CusContainerSchema.CO_JC] = container2.PK;

			Factory.Save();

			AssertHasEvent(container1, "Should not have the GOU event for a new Container", AutoEvents.GateOutCode, false);
			AssertHasEvent(container2, "Should not have the GOU event for a new Container", AutoEvents.GateOutCode, false);
			AssertHasEvent(container3, "Should not have the GOU event for a new Container", AutoEvents.GateOutCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: A new Declaration does not have the GOU event", AutoEvents.GateOutCode, false);

			container1.JC_FCLWharfGateOut = ZDateTime.Now;
			Factory.Save();

			AssertHasEvent(container1, "Should have the GOU event as the JC_FCLWharfGateOut is changed", AutoEvents.GateOutCode, true);
			AssertHasEvent(container2, "Should not have the GOU event as nothing has changed", AutoEvents.GateOutCode, false);
			AssertHasEvent(container3, "Should not have the GOU event as nothing has changed", AutoEvents.GateOutCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: Container2 doesnt have GOU event", AutoEvents.GateOutCode, false);

			container2.JC_FCLWharfGateOut = ZDateTime.Now;
			Factory.Save();

			AssertHasEvent(container1, "Should have the GOU event as the JC_FCLWharfGateOut is changed", AutoEvents.GateOutCode, true);
			AssertHasEvent(container2, "Should have the GOU event as the JC_FCLWharfGateOut is changed", AutoEvents.GateOutCode, true);
			AssertHasEvent(container3, "Should not have the GOU event as nothing has changed", AutoEvents.GateOutCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: All Declaration Related Containers have GOU event", AutoEvents.GateOutCode, true);
		}

		public void TestEventOnDeclarationContainerShouldFireDeclarationWorkflow_DHR()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;

			var cusContainer2 = cusContainers.AddNew();
			cusContainer2[CusContainerSchema.CO_JC] = container2.PK;

			Factory.Save();

			AssertHasEvent(container1, "Should not have the DHR event for a new Container", AutoEvents.DehireCode, false);
			AssertHasEvent(container2, "Should not have the DHR event for a new Container", AutoEvents.DehireCode, false);
			AssertHasEvent(container3, "Should not have the DHR event for a new Container", AutoEvents.DehireCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: A new Declaration does not have the DHR event", AutoEvents.DehireCode, false);

			container1.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Now;
			Factory.Save();

			AssertHasEvent(container1, "Should have the DHR event as the JC_ContainerYardEmptyReturnGateIn is changed", AutoEvents.DehireCode, true);
			AssertHasEvent(container2, "Should not have the DHR event as nothing has changed", AutoEvents.DehireCode, false);
			AssertHasEvent(container3, "Should not have the DHR event as nothing has changed", AutoEvents.DehireCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: Container2 doesnt have DHR event", AutoEvents.DehireCode, false);

			container2.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Now;
			Factory.Save();

			AssertHasEvent(container1, "Should have the DHR event as the JC_ContainerYardEmptyReturnGateIn is changed", AutoEvents.DehireCode, true);
			AssertHasEvent(container2, "Should have the DHR event as the JC_ContainerYardEmptyReturnGateIn is changed", AutoEvents.DehireCode, true);
			AssertHasEvent(container3, "Should not have the DHR event as nothing has changed", AutoEvents.DehireCode, false);
			AssertHasEvent((IStmALogParent)declaration, "Propagate: All Declaration Related Containers have DHR event", AutoEvents.DehireCode, true);
		}

		void AssertHasEvent(IStmALogParent logParent, string message, string eventCode, bool shoudHave)
		{
			var logs = logParent.Logs;
			AssertEquals(message, shoudHave, logs.HasLogWith(c => c.SL_SE_NKEvent == eventCode));
		}

		public void TestGetPropagationTargets_ConsolAndDeclaration()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;

			var cusContainer2 = cusContainers.AddNew();
			cusContainer2[CusContainerSchema.CO_JC] = container2.PK;

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container1);
			var log = container1.Logs.AddNew(AutoEvents.FreightUnloaded);

			var expectedTargets = new[]
			{
				new PropagationLink(consol, new [] { container1, container2, container3 }, "Consol Containers"),
				new PropagationLink((IStmALogParent)declaration, new [] { container1, container2 }, "Declaration Containers")
			};

			var actualTargets = handlingInfo.GetPropagationTargets(log);

			AssertContainsExactElementsInAnyOrder(PropagationLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetPropagationTargets_DeclarationWhichIsCancelled()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;

			var cusContainer2 = cusContainers.AddNew();
			cusContainer2[CusContainerSchema.CO_JC] = container2.PK;

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container1);
			var log = container1.Logs.AddNew(AutoEvents.FreightUnloaded);

			var consolLink = new PropagationLink(consol, new[] { container1, container2, container3 }, "Consol Containers");
			var declarationLink = new PropagationLink((IStmALogParent)declaration, new[] { container1, container2 }, "Declaration Containers");

			var expectedTargets = new[] { consolLink, declarationLink };
			var actualTargets = handlingInfo.GetPropagationTargets(log);

			AssertContainsExactElementsInAnyOrder("Should contain consol and declaration propagation links", PropagationLinkComparer.ByPKs, expectedTargets, actualTargets);

			declaration.IsCancelled = true;
			expectedTargets = new[] { consolLink };
			actualTargets = handlingInfo.GetPropagationTargets(log);

			AssertContainsExactElementsInAnyOrder("Should not contain declaration link as the declaration is cancelled", PropagationLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetPropagationTargets_ConsolAndShipment()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.OuterPackLines.AddNew().JL_JC = container1.PK;
			shipment1.OuterPackLines.AddNew().JL_JC = container1.PK;
			shipment1.OuterPackLines.AddNew().JL_JC = container2.PK;
			shipment1.OuterPackLines.AddNew().JL_JC = container2.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.OuterPackLines.AddNew().JL_JC = container1.PK;
			shipment2.OuterPackLines.AddNew().JL_JC = container1.PK;
			shipment2.OuterPackLines.AddNew().JL_JC = container2.PK;
			shipment2.OuterPackLines.AddNew().JL_JC = container2.PK;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.OuterPackLines.AddNew().JL_JC = container2.PK;
			shipment3.OuterPackLines.AddNew().JL_JC = container2.PK;

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container1);
			var log = container1.Logs.AddNew(Events.FreightUnloaded);
			var expectedTargets = new[]
			{
				new PropagationLink(consol,    new [] { container1, container2, container3 }, "Consol Containers"),
				new PropagationLink(shipment1, new [] { container1, container2 }, "Shipment Containers"),
				new PropagationLink(shipment2, new [] { container1, container2 }, "Shipment Containers")
			};

			var actualTargets = handlingInfo.GetPropagationTargets(log);

			AssertContainsExactElementsInAnyOrder(PropagationLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetPropagationTargets_SingleDeclaration()
		{
			var container1 = Factory.New<ForwardingContainer>();
			var container2 = Factory.New<ForwardingContainer>();
			var container3 = Factory.New<ForwardingContainer>();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;

			var cusContainer2 = cusContainers.AddNew();
			cusContainer2[CusContainerSchema.CO_JC] = container2.PK;

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container1);
			var log = container1.Logs.AddNew(AutoEvents.FreightUnloaded);

			var expectedTargets = new[]
			{
				new PropagationLink((IStmALogParent)declaration, new [] { container1, container2 }, "Declaration Containers")
			};

			var actualTargets = handlingInfo.GetPropagationTargets(log);

			AssertContainsExactElementsInAnyOrder(PropagationLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetEventParametersToPropagate_GateIn_GateOut()
		{
			foreach (var gateEvent in new[] { Events.GateIn, Events.GateOut })
			{
				AssertContainsExactElementsInAnyOrder("Only facility should be considered during propagation parameters matching",
					new[] { CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility },
					GetInstance().GetEventParametersToMatchDuringPropagation(gateEvent.Code));

				var consol = Factory.New<ForwardingConsol>();

				var container1 = consol.Containers.AddNew();
				var container2 = consol.Containers.AddNew();
				var container3 = consol.Containers.AddNew();

				var log = container1.Logs.AddNew(gateEvent);
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.ContainerYard;
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = "";
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department] = "SOMETHING";
				}

				log = container2.Logs.AddNew(gateEvent);
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.ContainerYard;
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = "AUSYD";
				}

				log = container3.Logs.AddNew(gateEvent);
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
				}

				AssertEquals($"{gateEvent.Code} - {gateEvent.Description} event not propagated: not all facilities are matched", 0,
					consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, gateEvent.Code)).Length);

				log = container3.Logs.AddNew(gateEvent);
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.ContainerYard;
				}

				AssertEquals($"{gateEvent.Code} - {gateEvent.Description} event propagated: all facilities are matched", 1,
					consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, gateEvent.Code)).Length);
			}
		}

		#region Cascading

		public void TestPopulateCascadingTargets_ToShipment()
		{
			var consol1 = Factory.New<ForwardingConsol>();

			var shipment1 = consol1.Shipments.AddNew();
			var shipment2 = consol1.Shipments.AddNew();

			var masterShipment1 = consol1.Shipments.AddNew();
			shipment1.JS_JS_ColoadMasterShipment = masterShipment1.PK;

			var masterShipment2 = consol1.Shipments.AddNew();
			shipment2.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var container1 = consol1.Containers.AddNew();
			var container2 = consol1.Containers.AddNew();

			var pack1 = shipment1.OuterPackLines.AddNew();
			pack1.Containers.Add(container1);

			var pack2 = shipment2.OuterPackLines.AddNew();
			pack2.Containers.Add(container2);

			Factory.Save();

			var triggerForShipment1 = CreateTrigger(shipment1);
			var triggerForShipment2 = CreateTrigger(shipment2);
			var triggerForMasterShipment1 = CreateTrigger(masterShipment1);
			var triggerForMasterShipment2 = CreateTrigger(masterShipment2);

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
			}

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container1);
			var targets = handlingInfo.GetCascadingTargets(eventLog);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1.PK, masterShipment1.PK }, targets.Select(x => ((ForwardingShipment)x.Parent).PK));

			eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
			}

			handlingInfo = new ForwardingContainerProcessHandlingInfo(container2);
			targets = handlingInfo.GetCascadingTargets(eventLog);

			AssertContainsExactElementsInAnyOrder(new[] { shipment2.PK, masterShipment2.PK }, targets.Select(x => ((ForwardingShipment)x.Parent).PK));
		}

		public void TestPopulateCascadingTargets_ToDeclaration()
		{
			var container = Factory.New<ForwardingContainer>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer = cusContainers.AddNew();
			cusContainer[CusContainerSchema.CO_JC] = container.PK;

			Factory.Save();

			var trigger = ((IWorkflowProvider)declaration).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_RespondToCascadedEvents = true;

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ArrivalCode;
			}

			var handlingInfo = new ForwardingContainerProcessHandlingInfo(container);
			var targets = handlingInfo.GetCascadingTargets(eventLog);

			AssertEquals(declaration.PK, ((Enterprise.Integration.Customs.IBaseJobDeclaration)targets.FirstOrDefault().Parent).PK);
		}

		public void TestPopulateCascadingTargets_ShipmentAndContainerWithSameEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.Containers.Add(container1);

			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.Containers.Add(container2);

			Factory.Save();

			var triggerForShipment = CreateTrigger(shipment);
			var triggerForContainer = CreateTrigger(container1);

			var logTime = ZDateTime.Now;
			var log = container1.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
				log.SL_EventTime = logTime;
			}

			Factory.Save();

			AssertEquals("Trigger on Shipment is fired.", logTime, triggerForShipment.P9_ActualDate);
			AssertEquals("Trigger on Container is fired.", logTime, triggerForContainer.P9_ActualDate);
		}

		ProcessTask CreateTrigger(IWorkflowProvider parent)
		{
			var trigger = parent.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;
			trigger.P9_RespondToCascadedEvents = true;

			return trigger;
		}

		#endregion

		#region Implementation

		protected override CommonContainerProcessHandlingInfo GetInstance()
		{
			var container = Factory.New<ForwardingContainer>();

			return new ForwardingContainerProcessHandlingInfo(container);
		}

		#endregion
	}
}
