using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPropagationTargetsNotIncludesSelf()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var processHandlingInfo = new ForwardingShipmentProcessHandlingInfo(shipment1);
			var log = shipment1.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("No propagation targets", 0, processHandlingInfo.GetPropagationTargets(log).Count());

			shipment1.JS_JS_ColoadMasterShipment = shipment1.PK;
			AssertEquals("No targets on itself", 0, processHandlingInfo.GetPropagationTargets(log).Count());

			shipment1.JS_JS_ColoadMasterShipment = shipment2.PK;
			AssertEquals("One element should be targeted", 1, processHandlingInfo.GetPropagationTargets(log).Count());
		}

		public void TestPropagationTargetsNotIncludesBCNColoadMasterShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var processHandlingInfo = new ForwardingShipmentProcessHandlingInfo(shipment);
			var log = shipment.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("No propagation targets", 0, processHandlingInfo.GetPropagationTargets(log).Count());
		}

		public void TestInternallySendingAttachToConsolEventForShipmentNotImportShipmentIntoTheSameModule()
		{
			var company = Factory.New<GlbCompany>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZAAA";

			company.GC_OH_OrgProxy = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "USMIA";
			consol.JK_RL_NKDischargePort = "CLSAI";
			consol.SetDefaultReceivingForwarderAddress(orgHeader.PK);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "USMIA";
			shipment.JS_RL_NKDestination = "CLSAI";
			var shipmentWorkflowItem = shipment.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = AutoEvents.AttachedCode;

			var notification = shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.ReceivingAgent;

			Factory.Save();

			AssertEquals("SHIPMENT COUNT PRECONDITION", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("CONSOL COUNT PRECONDITION", 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));

			using (Factory.AddDisposableService())
			{
				var dataExport = new ManualDataExport(Factory, shipment, UniversalDataType.UniversalShipment)
				{
					RecipientType = MessageRecipientPartyTypeList.Codes.ReceivingAgent,
					EventCode = AutoEvents.AttachedCode,
				};

				var notifications = new NotificationBuffer();
				dataExport.SendData(notifications);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("dataExport.SendData() notifications", @"
Processing Shipment S00001000
Universal Shipment sent internally for Organization [XYZAAA].
".Trim(), notifications.AsString);

					AssertNotNull(shipment.Logs.MostRecentLogByEventTime(AutoEvents.DataExport));
					AssertNotNull(shipment.Logs.MostRecentLogByEventTime(AutoEvents.Attached));
					AssertNull(shipment.Logs.MostRecentLogByEventTime(AutoEvents.DataImport));

					AssertEquals(1, shipment.Consols.Count);
					AssertEquals(consol.PK, shipment.Consols[0].PK);

					AssertEquals("CONSOL COUNT", 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
					AssertEquals("SHIPMENT COUNT", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				});
			}
		}

		public void TestShipmentEventShouldCascadeEventToSubShipment()
		{
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

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = shipment.CoLoadShipments.AddNew();
			var subShipment2 = shipment.CoLoadShipments.AddNew();

			var subShipment11 = subShipment1.CoLoadShipments.AddNew();
			var subShipment12 = subShipment1.CoLoadShipments.AddNew();

			var subShipment121 = subShipment12.CoLoadShipments.AddNew();

			Factory.Save();

			var shipmentWorkflowItems = shipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Shipment only have one Process Task for Manifest Event", 1, shipmentWorkflowItems.Count());
			var shipmentWorkflowItem = shipmentWorkflowItems.First();

			var subShipment1WorkflowItems = subShipment1.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Sub shipment1 only have one Process Task for Manifest Event", 1, subShipment1WorkflowItems.Count());
			var subShipment1WorkflowItem = subShipment1WorkflowItems.First();

			var subShipment2WorkflowItems = subShipment2.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Sub shipment2 only have one Process Task for Manifest Event", 1, subShipment2WorkflowItems.Count());
			var subShipment2WorkflowItem = subShipment2WorkflowItems.First();

			var subShipment11WorkflowItems = subShipment11.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Sub shipment11 only have one Process Task for Manifest Event", 1, subShipment11WorkflowItems.Count());
			var subShipment11WorkflowItem = subShipment11WorkflowItems.First();

			var subShipment12WorkflowItems = subShipment12.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Sub shipment12 only have one Process Task for Manifest Event", 1, subShipment12WorkflowItems.Count());
			var subShipment12WorkflowItem = subShipment12WorkflowItems.First();

			var subShipment121WorkflowItems = subShipment121.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals("Precondition: Sub shipment121 only have one Process Task for Manifest Event", 1, subShipment121WorkflowItems.Count());
			var subShipment121WorkflowItem = subShipment121WorkflowItems.First();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

			var log = shipment.Logs.AddNew(Events.Manifested);
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, subShipment1WorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, subShipment2WorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, subShipment11WorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, subShipment12WorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, subShipment121WorkflowItem.P9_ActualDate.ToZDateTime());

			AssertEquals("One WTE Event add to consol's process task", 1, shipmentWorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to first shipment's process task(Cascading) ", 1, subShipment1WorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to second shipment's process task(Cascading)", 1, subShipment2WorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to second shipment's process task(Cascading)", 1, subShipment11WorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to second shipment's process task(Cascading)", 1, subShipment12WorkflowItem.Logs.Find(query).Length);
			AssertEquals("One WTE Event add to second shipment's process task(Cascading)", 1, subShipment121WorkflowItem.Logs.Find(query).Length);
		}

		public void TestEventPropagationAndCascadingWithRecursiceSetFieldTrigger()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_SubType1 = "AIR";
			template.P0_GC = Env.CurrentCompany.PK;

			var customField1 = MasterFilesTestHelper.CreateCustomField(template, "CF1");
			var createEventRule1 = new CreateEventRule { EventCode = Events.CustomisableEvent01Code, EventReference = "123", IsEstimate = false, IsEnabled = true };
			var rule1 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule1.SetRules(createEventRule1);
			customField1.XC_XR = rule1.PK;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = "TRG";
			trigger.P9_Description = "TRIGGER1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			trigger.P9_RespondToCascadedEvents = true;
			trigger.ProcessTaskNotifications.AddNew();
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = "<GetCustomField(CF1)>";
			action1.PQ_FieldValue = "<Add(\"<Coalesce(\"<GetCustomField(CF1)>\", \"0\")>\", \"1\")>";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = shipment.CoLoadShipments.AddNew();
			var subShipment2 = shipment.CoLoadShipments.AddNew();

			var subShipment11 = subShipment1.CoLoadShipments.AddNew();
			var subShipment12 = subShipment1.CoLoadShipments.AddNew();
			var subShipment13 = subShipment1.CoLoadShipments.AddNew();
			var subShipment14 = subShipment1.CoLoadShipments.AddNew();

			var subShipment121 = subShipment12.CoLoadShipments.AddNew();
			var subShipment111 = subShipment11.CoLoadShipments.AddNew();

			Factory.Save();

			shipment.Logs.AddNew(new EventValue(Events.CustomisableEvent01));

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "Z01");
			CombineAssertions("1 Z01 event created by custom field, shipments with sub shipments should have 1 propagated Z01 event", () =>
			{
				AssertEquals(2, shipment.Logs.Find(query).Length);
				AssertEquals(2, subShipment1.Logs.Find(query).Length);
				AssertEquals(2, subShipment11.Logs.Find(query).Length);
				AssertEquals(2, subShipment12.Logs.Find(query).Length);

				AssertEquals(1, subShipment2.Logs.Find(query).Length);
				AssertEquals(1, subShipment13.Logs.Find(query).Length);
				AssertEquals(1, subShipment14.Logs.Find(query).Length);
				AssertEquals(1, subShipment121.Logs.Find(query).Length);
				AssertEquals(1, subShipment111.Logs.Find(query).Length);
			});

			CombineAssertions("Trigger should fire once, before recursion is detected", () =>
			{
				AssertEquals("1", shipment.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment1.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment11.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment12.GetCustomField("CF1", AddOnColumnDataType.Codes.String));

				AssertEquals("1", subShipment2.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment13.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment14.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment121.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
				AssertEquals("1", subShipment111.GetCustomField("CF1", AddOnColumnDataType.Codes.String));
			});

			AssertEquals(1, consol.Logs.Find(l => l.SL_Reference == "Propagated: All Shipments" && l.SL_SE_NKEvent == "Z01").Count());
		}

		public void TestBCNShipmentShouldNotCascadeEventToSubShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentProcessTask = shipment.WorkflowItems.Milestones.AddNew();
			shipmentProcessTask.P9_Type = "TRG";
			shipmentProcessTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentProcessTask.P9_RespondToCascadedEvents = true;
			shipmentProcessTask.ProcessTaskNotifications.AddNew();

			var subShipment1 = shipment.CoLoadShipments.AddNew();
			var subShipment2 = shipment.CoLoadShipments.AddNew();

			Factory.Save();

			var shipmentWorkflowItems = shipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals(1, shipmentWorkflowItems.Count());

			var subShipment1WorkflowItems = subShipment1.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals(0, subShipment1WorkflowItems.Count());

			var subShipment2WorkflowItems = subShipment2.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.ManifestedCode);
			AssertEquals(0, subShipment2WorkflowItems.Count());
		}

		public void TestShipmentEventShouldNotCascadeEditEventToSubShipment()
		{
			var shipmentProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			shipmentProcessTaskTemplate.P0_ProcessType = "SHP";
			shipmentProcessTaskTemplate.P0_SubType1 = "AIR";
			shipmentProcessTaskTemplate.P0_GC = Env.CurrentCompany.PK;

			var shipmentProcessTask = shipmentProcessTaskTemplate.WorkflowItems.Milestones.AddNew();
			shipmentProcessTask.P9_Type = "TRG";
			shipmentProcessTask.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			shipmentProcessTask.P9_RespondToCascadedEvents = true;
			shipmentProcessTask.ProcessTaskNotifications.AddNew();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var shipment = consol.Shipments.AddNew();
			var subShipment1 = shipment.CoLoadShipments.AddNew();
			var subShipment2 = shipment.CoLoadShipments.AddNew();

			Factory.Save();

			var shipmentWorkflowItems = shipment.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.EditedARecordCode);
			AssertEquals("Precondition: Shipment only have one Process Task for Edit Event", 1, shipmentWorkflowItems.Count());
			var shipmentWorkflowItem = shipmentWorkflowItems.First();

			var subShipment1WorkflowItems = subShipment1.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.EditedARecordCode);
			AssertEquals("Precondition: Sub shipment1 only have one Process Task for Edit Event", 1, subShipment1WorkflowItems.Count());
			var subShipment1WorkflowItem = subShipment1WorkflowItems.First();

			var subShipment2WorkflowItems = subShipment2.WorkflowItems.OfType<ProcessTask>().Where(w => w.P9_SE_NKMilestoneEvent == Events.EditedARecordCode);
			AssertEquals("Precondition: Sub shipment2 only have one Process Task for Edit Event", 1, subShipment2WorkflowItems.Count());
			var subShipment2WorkflowItem = subShipment2WorkflowItems.First();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log = shipment.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Actual Date was updated for workflow item", log.SL_EventTime, shipmentWorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was not updated for workflow item", ZDateTime.Empty, subShipment1WorkflowItem.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual Date was not updated for workflow item", ZDateTime.Empty, subShipment2WorkflowItem.P9_ActualDate.ToZDateTime());

			AssertEquals("One WTE Event add to consol's process task", 1, shipmentWorkflowItem.Logs.Find(query).Length);
			AssertEquals("None WTE Event add to first shipment's process task(Cascading) ", 0, subShipment1WorkflowItem.Logs.Find(query).Length);
			AssertEquals("None WTE Event add to second shipment's process task(Cascading)", 0, subShipment2WorkflowItem.Logs.Find(query).Length);
		}

		#region GetCascadingTargets - Orders

		public void TestGetCascadingTargets_ShipmentWithLinkedOrder_ReturnOrderAsTarget()
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
			var shipment = Factory.New<ForwardingShipment>();

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

			var handler = new ForwardingShipmentProcessHandlingInfo(shipment);
			var targets = handler.GetCascadingTargets(eventLog).ToArray();

			var orderTarget = targets.Single(t => t.Parent.LogsParentPK == order.PK);

			AssertContainsExactElementsInAnyOrder(string.Format("{0}: Order process tasks", eventToLookup.Code),
				orderMilestones,
				orderTarget.Triggers);
		}

		#endregion

		public void TestFireEventOnShipmenAcceptCascadeAndLocalEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = shipment.CoLoadShipments.AddNew();

			var trigger = subShipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			trigger.P9_RespondToCascadedEvents = true;
			Factory.Save();

			AssertEquals("Precondition: milestone.P9_ActualDate", ZDateTime.Empty, trigger.P9_ActualDate.ToZDateTime());

			var log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Cascade: Should change milestone's actual time", new ZDateTime(2011, 12, 13), trigger.P9_ActualDate.ToZDateTime());

			log = subShipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Local: Should change milestone's actual time", new ZDateTime(2011, 12, 14), trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestFireEventOnConsoleShipmentAcceptLocalEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var subShipment = shipment.CoLoadShipments.AddNew();

			var milestone = Factory.New<ForwardingShipmentProcessTask>() as ProcessTask;
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			milestone.P9_ParentID = subShipment.PK;
			milestone.P9_Type = "MIL";
			milestone.P9_RespondToCascadedEvents = false;

			AssertEquals("Precondition: milestone.P9_ActualDate", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			var log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Cascade: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			log = subShipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Local: Should change milestone's actual time", new ZDateTime(2011, 12, 14), milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawEventOnConsolShipmentAcceptCascadeAndLocalEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = shipment.CoLoadShipments.AddNew();

			var milestone = subShipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			milestone.P9_RespondToCascadedEvents = true;
			Factory.Save();

			var log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 13), milestone.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Cascade: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			log = subShipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 14), milestone.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Local: Should change milestone's actual time", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawEventOnConsolShipmentAcceptLocalEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = shipment.CoLoadShipments.AddNew();

			var trigger = subShipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			trigger.P9_RespondToCascadedEvents = true;
			Factory.Save();

			var log = shipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 13));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 13), trigger.P9_ActualDate.ToZDateTime());

			trigger.P9_RespondToCascadedEvents = false;
			Factory.Save();

			log.Cancel();
			AssertEquals("Cascade: Should not change milestone's actual time", new ZDateTime(2011, 12, 13), trigger.P9_ActualDate.ToZDateTime());

			log = subShipment.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2011, 12, 14));
			AssertEquals("Precondition: milestone.P9_ActualDate", new ZDateTime(2011, 12, 14), trigger.P9_ActualDate.ToZDateTime());

			log.Cancel();
			AssertEquals("Local: Should change milestone's actual time", ZDateTime.Empty, trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestEventIsNotPropagated_PackingCompleted() => AssertEventIsNotPropagatedToConsol(AutoEvents.PackingCompleted);

		public void TestEventIsNotPropagated_UnpackingCompleted() => AssertEventIsNotPropagatedToConsol(AutoEvents.UnpackingCompleted);

		public void TestEventIsNotPropagated_CalculateDeliveryDateWithExceptionsRequested() => AssertEventIsNotPropagatedToConsol(AutoEvents.CalculateDeliveryDateWithExceptionsRequested);

		void AssertEventIsNotPropagatedToConsol(Event shipmentEvent)
		{
			var stmEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, shipmentEvent.Code);
			stmEvent.SE_PropagateToParent = true;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var workflowProvider = (IWorkflowProvider)consol;
			var eventTrigger = workflowProvider.WorkflowItems.AddNew();
			eventTrigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			eventTrigger.TriggerConditions.TriggerEventCode = shipmentEvent.Code;

			shipment.Logs.AddNew(shipmentEvent);
			AssertEquals("Shipment should not propagate event to consol.", ZDateTime.Empty, eventTrigger.P9_ActualDate.ToZDateTime());
			AssertEquals("No event in consol.", 0, consol.Logs.Find(x => x.SL_SE_NKEvent == shipmentEvent.Code).Count());
		}
	}
}
