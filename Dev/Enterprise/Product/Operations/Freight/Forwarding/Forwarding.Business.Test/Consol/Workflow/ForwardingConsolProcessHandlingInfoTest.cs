using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region GetCascadingTargets - Orders

		public void TestGetCascadingTargets_ConsolHasShipmentWithLinkedOrder_ReturnOrderAsTarget()
		{
			CombineAssertions(delegate
			{
				AssertCascadingTargetsFromOrder(Events.Arrival);
				AssertCascadingTargetsFromOrder(Events.Departure);
				AssertCascadingTargetsFromOrder(Events.CargoAvailable);
				AssertCascadingTargetsFromOrder(Events.DeliveryCartageAdvised);
				AssertCascadingTargetsFromOrder(Events.DeliveryCartageCompleteFinalised);
				AssertCascadingTargetsFromOrder(Events.CustomsCommenced);
				AssertCascadingTargetsFromOrder(Events.CustomsCleared);
				AssertCascadingTargetsFromOrder(Events.ExportCustomsCommenced, extraEventToBeReturned: Events.CustomsCommenced);
				AssertCascadingTargetsFromOrder(Events.ExportCustomsCleared, extraEventToBeReturned: Events.CustomsCleared);
			});
		}

		void AssertCascadingTargetsFromOrder(Event eventToLookup, Event extraEventToBeReturned = null)
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;

			var orderMilestones = new List<ProcessTask>();

			foreach (Event orderProcessTaskEvent in new[] { eventToLookup, extraEventToBeReturned }.Where(ev => ev != null))
			{
				var milestone = order.WorkflowItems.Milestones.Cast<ProcessTask>()
					.FirstOrDefault(m => m.P9_SE_NKMilestoneEvent == orderProcessTaskEvent.Code && m.P9_RespondToCascadedEvents);

				if (milestone == null)
				{
					milestone = order.WorkflowItems.Milestones.AddNew();
					milestone.TriggerConditions.TriggerEventCode = orderProcessTaskEvent.Code;
					milestone.P9_RespondToCascadedEvents = true;
				}

				orderMilestones.Add(milestone);
			}

			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_Table = "JobShipment";
				eventLog.SL_Parent = shipment.PK;
				eventLog.SL_SE_NKEvent = eventToLookup.Code;
			}

			var handler = new ForwardingConsolProcessHandlingInfo(consol);
			var targets = handler.GetCascadingTargets(eventLog).ToArray();

			var orderTarget = targets.Single(t => t.Parent.LogsParentPK == order.PK);

			AssertContainsExactElementsInAnyOrder(string.Format("{0}: Order process tasks", eventToLookup.Code),
				orderMilestones,
				orderTarget.Triggers);
		}

		#endregion

		public void TestEventsOnConsolTransportLegsShouldFireShipmentWorkflow()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var consolWorkflowItem = consol.WorkflowItems.AddNew();
			consolWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			consolWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolWorkflowItem.P9_RespondToCascadedEvents = true;
			consolWorkflowItem.ProcessTaskNotifications.AddNew();

			var consolLeg = consol.Transports[0];
			consolLeg.JW_VoyageFlight = "QF253";
			consolLeg.JW_ETD = new ZDateTime(2011, 1, 2);
			consolLeg.JW_RL_NKLoadPort = "AUSYD";
			consolLeg.JW_ETA = new ZDateTime(2011, 1, 3);
			consolLeg.JW_RL_NKDiscPort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FJSUV";
			var shipmentWorkflowItem = shipment.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.ProcessTaskNotifications.AddNew();

			Factory.Save();

			var consolLegLog = consolLeg.Logs.AddNew(Events.Manifested);

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals("Propogate: Consol Workflow Item Actual Date", consolLegLog.SL_EventTime, consolWorkflowItem.P9_ActualDate.ToZDateTime());
				AssertEquals("Cascade: Shipment Workflow Item Actual Date", consolLegLog.SL_EventTime, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());

				var workflowEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				AssertEquals("Propogate: One WTE Event add to consol's process task", 1, consolWorkflowItem.Logs.Find(workflowEventFilter).Length);
				AssertEquals("Cascade: No WTE Event add to shipment's process task", 1, shipmentWorkflowItem.Logs.Find(workflowEventFilter).Length);
			});
		}

		public void TestRecordAdded_RecordEdited_AttachToConsiol_SetInactive_SetToActive_EventsNotPropagated()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FJSUV";

			Factory.Save();

			var shipmentLog = shipment.GetLogs().AddNew(Events.Manifested);

			var logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Manifested event should be propagated", 1, logs.Length);
			AssertEquals("Manifested event type", Events.Manifested.Code, logs[0].SL_SE_NKEvent);

			Factory.Save();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			shipmentLog = shipment.GetLogs().AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Record edited event should not be propagated", 0, logs.Length);

			Factory.Save();
			AssertNotNull(shipment.GetLogs().MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
			logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Record added event should not be propagated", 0, logs.Length);

			Factory.Save();
			shipmentLog = shipment.GetLogs().AddNew(Events.Attached);
			logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Attach to consol event should not be propagated", 0, logs.Length);

			shipmentLog = shipment.GetLogs().AddNew(Events.SetToInactive);
			logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Set to Inactive event should not be propagated", 0, logs.Length);

			shipmentLog = shipment.GetLogs().AddNew(Events.SetToActive);
			logs = consol.GetLogs().LogsNotInDB;
			AssertEquals("Set to Active event should not be propagated", 0, logs.Length);
		}

		public void TestEventOnTransportLegShouldCascadeToConsol()
		{
			var consolProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			consolProcessTaskTemplate.P0_ProcessType = "CON";
			consolProcessTaskTemplate.P0_SubType1 = "AIR";
			consolProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;

			var consolProcessTask = consolProcessTaskTemplate.WorkflowItems.AddNew();
			consolProcessTask.P9_Type = "TRG";
			consolProcessTask.P9_RespondToCascadedEvents = true;
			consolProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolProcessTask.ProcessTaskNotifications.AddNew();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var firstTransport = consol.Transports[0];
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_TransportMode = Constants.TransportModes.Air;

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			var consolWorkflowItems = consol.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Shipment only have one Process Task for Manifest Event", 1, consolWorkflowItems.Count());
			var consolWorkflowItem = consolWorkflowItems.First();
			AssertEquals("Precondition: Trigger nit fired", ZDateTime.Empty, consolWorkflowItem.P9_ActualDate.ToZDateTime());
			firstTransport.Logs.AddNew(Events.Manifested);
			AssertNotEquals("Event from transport leg not cascade to Consol", ZDateTime.Empty, consolWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		public void TestEventOnTransportLegShouldCascadeToConsolAndThenCascadeToShipment()
		{
			var consolProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			consolProcessTaskTemplate.P0_ProcessType = "CON";
			consolProcessTaskTemplate.P0_SubType1 = "AIR";
			consolProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;

			var consolProcessTask = consolProcessTaskTemplate.WorkflowItems.AddNew();
			consolProcessTask.P9_Type = "TRG";
			consolProcessTask.P9_RespondToCascadedEvents = true;
			consolProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolProcessTask.ProcessTaskNotifications.AddNew();

			var shipmentProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			shipmentProcessTaskTemplate.P0_ProcessType = "SHP";
			shipmentProcessTaskTemplate.P0_SubType1 = "AIR";
			shipmentProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;

			var shipmentProcessTask = shipmentProcessTaskTemplate.WorkflowItems.Milestones.AddNew();
			shipmentProcessTask.P9_Type = "TRG";
			shipmentProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentProcessTask.P9_RespondToCascadedEvents = true;
			shipmentProcessTask.ProcessTaskNotifications.AddNew();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var firstShipment = consol.Shipments.AddNew();
			var secondShipment = consol.Shipments.AddNew();

			var firstTransport = consol.Transports[0];
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_TransportMode = Constants.TransportModes.Air;

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			var consolWorkflowItems = consol.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Shipment only have one Process Task for Manifest Event", 1, consolWorkflowItems.Count());
			var consolWorkflowItem = consolWorkflowItems.First();
			AssertEquals("Precondition: Trigger not fired", ZDateTime.Empty, consolWorkflowItem.P9_ActualDate.ToZDateTime());

			var firstShipmentWorkflowItems = firstShipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: First Shipment only have one Process Task for Manifest Event", 1, firstShipmentWorkflowItems.Count());
			var firstShipmentWorkflowItem = firstShipmentWorkflowItems.First();
			AssertEquals("Precondition: Trigger not fired", ZDateTime.Empty, firstShipmentWorkflowItem.P9_ActualDate.ToZDateTime());

			var secondShipmentWorkflowItems = secondShipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Second Shipment only have one Process Task for Manifest Event", 1, secondShipmentWorkflowItems.Count());
			var secondShipmentWorkflowItem = secondShipmentWorkflowItems.First();
			AssertEquals("Precondition: Trigger not fired", ZDateTime.Empty, secondShipmentWorkflowItem.P9_ActualDate.ToZDateTime());

			firstTransport.Logs.AddNew(Events.Manifested);
			AssertNotEquals("Event from transport leg not cascade to Consol", ZDateTime.Empty, consolWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertNotEquals("Event from transport leg not cascade to first shipment throw consol", ZDateTime.Empty, firstShipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertNotEquals("Event from transport leg not cascade to second shipment throw consol", ZDateTime.Empty, secondShipmentWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		public void TestConsolShouldCascadeEventToShipment()
		{
			var consolProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			consolProcessTaskTemplate.P0_ProcessType = "CON";
			consolProcessTaskTemplate.P0_SubType1 = "AIR";
			consolProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;

			var consolProcessTask = consolProcessTaskTemplate.WorkflowItems.AddNew();
			consolProcessTask.P9_Type = "TRG";
			consolProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			consolProcessTask.ProcessTaskNotifications.AddNew();

			var shipmentProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			shipmentProcessTaskTemplate.P0_ProcessType = "SHP";
			shipmentProcessTaskTemplate.P0_SubType1 = "AIR";
			shipmentProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;
			var shipmentProcessTask = shipmentProcessTaskTemplate.WorkflowItems.Milestones.AddNew();
			shipmentProcessTask.P9_Type = "TRG";
			shipmentProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentProcessTask.P9_RespondToCascadedEvents = true;
			shipmentProcessTask.ProcessTaskNotifications.AddNew();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var firstShipment = consol.Shipments.AddNew();
			var secondShipment = consol.Shipments.AddNew();
			Factory.Save();

			var consolWorkflowItems = consol.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Consol only have one Process Task for Manifest Event", 1, consolWorkflowItems.Count());
			var consolWorkflowItem = consolWorkflowItems.First();

			var firstShipmentWorkflowItems = firstShipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: First shipment only have one Process Task for Manifest Event", 1, firstShipmentWorkflowItems.Count());
			var firstShipmentWorkflowItem = firstShipmentWorkflowItems.First();

			var secondShipmentWorkflowItems = secondShipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Second shipment only have one Process Task for Manifest Event", 1, secondShipmentWorkflowItems.Count());
			var secondShipmentWorkflowItem = secondShipmentWorkflowItems.First();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

			var log = consol.Logs.AddNew(Events.Manifested);
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, consolWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, firstShipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, secondShipmentWorkflowItem.P9_ActualDate.ToZDateTime());

			AssertEquals("One WTE Event add to consol's process task", 1, consolWorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to first shipment's process task(Cascading) ", 1, firstShipmentWorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to second shipment's process task(Cascading)", 1, secondShipmentWorkflowItem.Logs.Find(query).Length);
		}

		[TestDate(2020, 1, 1)]
		public void TestIfcActionGetsConsolFieldsForShipments()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_MasterBillNum = Guid.NewGuid().ToString("N").Substring(0, 10);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = ((ForwardingConsol)consol).Transports[0];
			transport.JW_LegOrder = 1;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETA = new ZDateTime(2021, 05, 21);

			var masterShipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			((ForwardingConsol)consol).Shipments.Add((ForwardingShipment)masterShipment);
			var subShipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			subShipment.JS_UniqueConsignRef = "SubshipmentRef";
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			var trigger = ((IWorkflowProvider)masterShipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = @"<JS_E_ARV>";
			action.PQ_FieldValue = @"<AddDurationToDate(""<Consols.MostInterestingTransportForBinding.JW_ETA>"",""DAYS"",""1"")>";

			var trigger2 = ((IWorkflowProvider)subShipment).WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = @"<JS_E_ARV>";
			action2.PQ_FieldValue = @"<AddDurationToDate(""<Consols.MostInterestingTransportForBinding.JW_ETA>"",""DAYS"",""1"")>";

			Factory.Save();

			AssertEquals("Arrival set from consol", masterShipment.JS_E_ARV, transport.JW_ETA);
			AssertEquals("Arrival set from consol", subShipment.JS_E_ARV, transport.JW_ETA);

			((ForwardingShipment)subShipment).Logs.AddNew(new EventValue(Events.CustomisableEvent00, ZDateTimeOffset.Now));

			AssertEquals("Event propates to master shipment", 1, ((ForwardingShipment)subShipment).Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == Events.CustomisableEvent00.Code).Count());
			AssertEquals("Event exists on sub shipment", 1, ((ForwardingShipment)masterShipment).Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == Events.CustomisableEvent00.Code).Count());
			AssertEquals("IFC works on master shipment", transport.JW_ETA.AddDays(1), masterShipment.JS_E_ARV);
			AssertEquals("IFC works on sub shipment", transport.JW_ETA.AddDays(1), subShipment.JS_E_ARV);
		}

		[TestDate(2020, 1, 1)]
		public void TestIfcActionOnConsolTransports()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_MasterBillNum = Guid.NewGuid().ToString("N").Substring(0, 10);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport1 = ((ForwardingConsol)consol).Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_ETA = new ZDateTime(2021, 05, 21);

			var transport2 = ((ForwardingConsol)consol).Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_ETA = new ZDateTime(2021, 05, 21);

			var masterShipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			((ForwardingConsol)consol).Shipments.Add((ForwardingShipment)masterShipment);

			var trigger = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = @"<Transports.Where(""<JW_LegOrder>""==""1"").JW_ETA>";
			action1.PQ_FieldValue = @"<DateTimeAsString('<AddDurationToDate(""<Transports.format(""{JW_ETA}"",, ""{JW_LegOrder}"" == ""1"") > "",""Days"",""1"")>','dd-MMM-yy HH:mm')>";

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = @"<Transports.Where(""<JW_LegOrder>""==""2"").JW_ETA>";
			action2.PQ_FieldValue = @"<DateTimeAsString('<AddDurationToDate(""<Transports.format(""{JW_ETA}"",, ""{JW_LegOrder}"" == ""10"") > "",""Days"",""1"")>','dd-MMM-yy HH:mm')>";

			Factory.Save();

			((ForwardingShipment)masterShipment).Logs.AddNew(new EventValue(Events.CustomisableEvent00, ZDateTimeOffset.Now));

			AssertEquals("Event propates to consol", 1, ((ForwardingConsol)consol).Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == Events.CustomisableEvent00.Code).Count());
			AssertNoWarnings("Action1 has no validation warnings", action1.PQ_FieldValueInfo);
			AssertEquals("IFC works on transport1", new ZDateTime(2021, 05, 22), transport1.JW_ETA);

			// To be fixed in WI00339150 - Add clause support for .Format() macros
			//AssertEquals("This shouldn't work. To be fixed in ", new ZDateTime(2021, 05, 21), transport2.JW_ETA);
			//AssertHasWarning("Action2 has validation warnings as no transport with legOrder = 10 exists", action2.PQ_FieldValueInfo,
			//	@"Field <Transports.format(""{JW_ETA}"",, ""{JW_LegOrder}"" == ""10"")> not found on any of the DataSource Types: [ForwardingConsol], [ForwardingConsolProcessTask], [ProcessTaskNotification].");
		}

		[TestDate(2020, 1, 1)]
		public void TestIfcCantChangeArrivalDateDuringEstEvent()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_MasterBillNum = Guid.NewGuid().ToString("N").Substring(0, 10);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport1 = ((CommonConsol)consol).Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_ETA = new ZDateTime(2021, 05, 21);

			var trigger1 = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.AttachedCode;

			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = @"<Transports.Where(""<JW_LegOrder>""==""1"").JW_ETA>";
			action1.PQ_FieldValue = @"<DateTimeAsString('<AddDurationToDate(""<Transports.format(""{JW_ETA}"",, ""{JW_LegOrder}"" == ""1"") > "",""Days"",""1"")>','dd-MMM-yy HH:mm')>";

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var milestone = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone.P9_Description = "Arival";
			milestone.P9_RecalculateScheduledDate = true;

			var trigger2 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;

			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = @"<JS_E_ARV>";
			action2.PQ_FieldValue = @"";

			Factory.Save();

			((CommonConsol)consol).Shipments.Add((CommonShipment)shipment);
			((CommonConsol)consol).Logs.AddNew(new EventValue(Events.Attached, ZDateTimeOffset.Now));

			AssertEquals("IFC works on transport1", new ZDateTime(2021, 05, 22), transport1.JW_ETA);
			AssertNotEquals("Trigger1 Fired", ZDateTime.Empty, trigger1.LastFiredTime);
			AssertEquals("IFC should be blocked from changing JS_E_ARV while the shipment is raising an EST event", new ZDateTime(2021, 05, 22), shipment.JS_E_ARV);
		}

		public void TestConsolAddEmptyEventShouldNotCallCascadingSP()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_TransportMode = "AIR";
			Factory.Save();

			var hitCountBefore = Factory.GetTableHitCount("ProcessTasks");

			AssertNull(((IProcessHandlingInfoProvider)testConsol).ProcessHandlingInfo.GetCascadingTargets(null));
			AssertNull(((IProcessHandlingInfoProvider)testConsol).ProcessHandlingInfo.GetCascadingTargets(Factory.New<StmALog>()));

			var hitCountAfter = Factory.GetTableHitCount("ProcessTasks");
			AssertEquals("Add empty event to consol should not call cascading SP", hitCountBefore, hitCountAfter);
		}

		public void TestGetCascadingTargets_ConsolContainsContainersWithProcessTasks_ReturnContainersProcessTasksAsTargets()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var container1 = consol1.Containers.AddNew();
			var container2 = consol1.Containers.AddNew();
			var container3 = consol2.Containers.AddNew();

			string eventCode1 = Events.CustomisableEvent10Code;
			string eventCode2 = Events.CustomisableEvent20Code;

			var processTasks = new[]
			{																								//			Test 1		Test 2
				container1.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: true),	// [0]		  X
				container1.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: true),	// [1]
				container1.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: false),	// [2]
				container1.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: false),	// [3]
				container2.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: true),	// [4]		  X
				container2.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: true),	// [5]
				container2.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: false),	// [6]
				container2.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: true),	// [7]		  X
				container2.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: false),	// [8]
				container3.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: true),	// [9]
				container3.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: true),	// [10]					  X
				container3.WorkflowItems.AddMilestone(linkedToEvent: eventCode1, includingCascaded: false),	// [11]
				container3.WorkflowItems.AddMilestone(linkedToEvent: eventCode2, includingCascaded: false),	// [12]
			};

			Factory.Save();

			// Test 1
			var eventLog1 = Factory.New<StmALog>();
			using (eventLog1.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog1.SL_Table = "OrgHeader";
				eventLog1.SL_Parent = ZGuid.NewZGuid();
				eventLog1.SL_SE_NKEvent = eventCode1;
			}

			var handler = new ForwardingConsolProcessHandlingInfo(consol1);
			var actualTargets = handler.GetCascadingTargets(eventLog1);
			var expectedTargets = new[]
			{
				new CascadingLink { Parent = container1, Triggers = new [] { processTasks[0] } },
				new CascadingLink { Parent = container2, Triggers = new [] { processTasks[4], processTasks[7] } }
			};
			AssertContainsExactElementsInAnyOrder(CascadingLinkComparer.ByPKs, expectedTargets, actualTargets);

			// Test 2
			var eventLog2 = Factory.New<StmALog>();
			using (eventLog2.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog2.SL_Table = "OrgHeader";
				eventLog2.SL_Parent = ZGuid.NewZGuid();
				eventLog2.SL_SE_NKEvent = eventCode2;
			}

			handler = new ForwardingConsolProcessHandlingInfo(consol2);
			actualTargets = handler.GetCascadingTargets(eventLog2);
			expectedTargets = new[]
			{
				new CascadingLink { Parent = container3, Triggers = new [] { processTasks[10] } }
			};
			AssertContainsExactElementsInAnyOrder(CascadingLinkComparer.ByPKs, expectedTargets, actualTargets);
		}

		public void TestGetCascadingTargets_SeveralEventsContainingPartialParameter_AreIgnored()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			Action<string> addCascadedMilestone = (eventCode) =>
				{
					var milestone = shipment.WorkflowItems.Milestones.AddNew();
					milestone.TriggerConditions.TriggerEventCode = eventCode;
					milestone.P9_RespondToCascadedEvents = true;
				};

			addCascadedMilestone(Events.FreightLoadedCode);
			addCascadedMilestone(Events.FreightUnloadedCode);
			addCascadedMilestone(Events.ReceivedCode);
			addCascadedMilestone(Events.ReleasedCode);

			Factory.Save();

			Action<string, bool> addEventAndAssertCascadingTargets = (eventCode, ignoreEventLogWithPartialParameter) =>
				{
					var eventLog = Factory.New<StmALog>();
					using (eventLog.LockForUpdatingKeyFieldsForTesting())
					{
						eventLog.SL_Table = "OrgHeader";
						eventLog.SL_Parent = ZGuid.NewZGuid();
						eventLog.SL_SE_NKEvent = eventCode;
						eventLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial] = "1";
					}

					var handler = new ForwardingConsolProcessHandlingInfo(consol);
					var actualTargets = handler.GetCascadingTargets(eventLog);

					if (ignoreEventLogWithPartialParameter)
					{
						AssertNull("Ignores event logs with partial parameter", actualTargets);
					}
					else
					{
						AssertNotNull(actualTargets);
					}

					eventLog = Factory.New<StmALog>();
					using (eventLog.LockForUpdatingKeyFieldsForTesting())
					{
						eventLog.SL_Table = "OrgHeader";
						eventLog.SL_Parent = ZGuid.NewZGuid();
						eventLog.SL_SE_NKEvent = eventCode;
					}

					handler = new ForwardingConsolProcessHandlingInfo(consol);
					actualTargets = handler.GetCascadingTargets(eventLog);

					AssertNotNull("Always works when no partial parameter", actualTargets);
				};

			addEventAndAssertCascadingTargets(Events.FreightLoadedCode, true);
			addEventAndAssertCascadingTargets(Events.FreightUnloadedCode, true);
			addEventAndAssertCascadingTargets(Events.ReceivedCode, true);
			addEventAndAssertCascadingTargets(Events.ReleasedCode, false);
		}

		public void TestFireEventOnConsoleShipmentAcceptCascadeAndLocalEvent()
		{
			var shipment = consol.Shipments.AddNew();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			trigger.P9_RespondToCascadedEvents = true;

			Factory.Save();

			AssertEquals("Precondition: milestone.P9_ActualDate", ZDateTime.Empty, trigger.P9_ActualDate.ToZDateTime());

			var log = consol.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Cascade: Should change milestone's actual time", new ZDateTime(2011, 12, 13), trigger.P9_ActualDate.ToZDateTime());

			log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Local: Should change milestone's actual time", new ZDateTime(2011, 12, 14), trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestFireEventOnConsoleShipmentAcceptLocalEvent()
		{
			var shipment = consol.Shipments.AddNew();
			var milestone = Factory.New<ForwardingShipmentProcessTask>() as ProcessTask;
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			milestone.P9_ParentID = shipment.PK;
			milestone.P9_Type = "MIL";
			milestone.P9_RespondToCascadedEvents = false;

			AssertEquals("Precondition: milestone.P9_ActualDate", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			var log = consol.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Cascade: Should not change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Local: Should change milestone's actual time", new ZDateTime(2011, 12, 14), milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawEventOnConsolShipmentAcceptCascadeAndLocalEvent()
		{
			var shipment = consol.Shipments.AddNew();
			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			milestone.P9_RespondToCascadedEvents = true;

			Factory.Save();

			var log = consol.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 13), milestone.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Cascade: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 14), milestone.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Local: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawEventOnConsolShipmentAcceptLocalEvent()
		{
			var shipment = consol.Shipments.AddNew();
			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			milestone.P9_RespondToCascadedEvents = true;
			Factory.Save();

			var log = consol.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 13), milestone.P9_ActualDate.ToZDateTime());

			milestone.P9_RespondToCascadedEvents = false;
			Factory.Save();

			log.Cancel();
			AssertEquals("Should not change milestone's actual time", new ZDateTime(2011, 12, 13), milestone.P9_ActualDate.ToZDateTime());

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();

			log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 14), milestone.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Local: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestEventPropagationForRDD()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var consol = Factory.New<ForwardingConsol>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Constants.TransportModes.Air;
				Factory.Save();

				var exception = (ForwardingConsolProcessTask)consol.WorkflowItems.Exceptions.AddNew();
				exceptionType.WET_DefaultDurationHours = 50;
				exception.ExceptionTypeCode = ZString.Empty;
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				Factory.Save();

				var rddEvent = consol.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull(rddEvent);
				AssertEquals($"|CHG={nameof(exceptionType)}|JOB={exception.PK}", rddEvent.SL_Reference);

				exception.P9_ExceptionDurationHours = 30;
				Factory.Save();
				rddEvent = consol.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull(rddEvent);
				AssertEquals($"|CHG=Exception Duration Hours Changed|JOB={exception.PK}", rddEvent.SL_Reference);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "RDD");
				CombineAssertions("1 RDD event created, shipments should have 1 propagated RDD event", () =>
				{
					AssertEquals(1, shipment1.Logs.Find(l => l.SL_Reference == $"Propagated: All Consols|CHG=Exception Duration Hours Changed|JOB={exception.PK}" && l.SL_SE_NKEvent == "RDD").Count());
					AssertEquals(1, shipment2.Logs.Find(l => l.SL_Reference == $"Propagated: All Consols|CHG=Exception Duration Hours Changed|JOB={exception.PK}" && l.SL_SE_NKEvent == "RDD").Count());
				});
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
		}
		ForwardingConsol consol;

		#endregion
	}
}
