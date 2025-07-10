using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.DataTransfer.ForwardingShipmentWorkflowDescriptor;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class ExportNotificationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetWorkflowTriggerAction_ForCINExportNotification_ValidationPassed()
		{
			using (Factory.AddDisposableService())
			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = new ExportNotificationMessageTestHelper(Factory).CreateShipment();

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "CIN trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification;

				shipment.Logs.AddNew(Events.CustomisableEvent00);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
				var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

				AssertType<ExportNotificationMessageProcessor>(processor);

				var notifications = new NotificationBuffer();
				processor.Process(notifications);
				var mvpEvent = shipment.Logs.MostRecentLogByEventTime(Events.MessageValidationPassed);
				AssertNotNull("MVP event had been added after sending message successfully", mvpEvent);
				CombineAssertions(() =>
				{
					AssertEquals("Event free text", ZString.Empty, mvpEvent.ReferenceFreeText);
					AssertEquals("Message type", "Export Notification (755)", mvpEvent.Parameters[Params.MessageType]);
					AssertEquals("Event department", "Terminal", mvpEvent.Parameters[Params.Department]);
				});

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, shipment.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
				var logs = ((IStmALogParent)documentDataStorage).Logs;

				AssertNotNull("Message had been sent", logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).FirstOrDefault());

				var message = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).FirstOrDefault()?.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
			}
		}

		public void TestGetWorkflowTriggerAction_ForCINExportNotification_ValidationFailed()
		{
			using (Factory.AddDisposableService())
			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = new ExportNotificationMessageTestHelper(Factory).CreateShipment();
				shipment.JS_OuterPacks = 0;

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "CIN trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification;

				shipment.Logs.AddNew(Events.CustomisableEvent00);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
				var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

				AssertType<ExportNotificationMessageProcessor>(processor);

				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var mvfEvent = shipment.Logs.MostRecentLogByEventTime(Events.MessageValidationFailed);
				AssertNotNull("MVF event had been added after sending message failed", mvfEvent);
				CombineAssertions(() =>
				{
					AssertEquals("Event free text", ZString.Empty, mvfEvent.ReferenceFreeText);
					AssertEquals("Message type", "Export Notification (755)", mvfEvent.Parameters[Params.MessageType]);
					AssertEquals("Event department", "Terminal", mvfEvent.Parameters[Params.Department]);
					AssertEquals("Message reason", "Check Export Notification (755) Report Electronic Messaging form for errors.", mvfEvent.Parameters[Params.Reason]);
				});

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, shipment.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
				AssertNull("Message had NOT been sent", ((IStmALogParent)documentDataStorage)?.Logs.MostRecentLogByEventTime(Events.MessageSent));
			}
		}

		public void TestGetWorkflowTriggerAction_ForCGNExportNotification_ValidationPassed()
		{
			using (Factory.AddDisposableService())
			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Netherlands))
			{
				var consol = new ExportNotificationMessageTestHelper(Factory).CreateConsol();

				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "CGN trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification;

				consol.Logs.AddNew(Events.CustomisableEvent00);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var workflowDescriptor = new JobConsolWorkflowDescriptor();
				var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

				AssertType<ExportNotificationMessageProcessor>(processor);

				var notifications = new NotificationBuffer();
				processor.Process(notifications);
				var mvpEvent = consol.Logs.MostRecentLogByEventTime(Events.MessageValidationPassed);
				AssertNotNull("MVP event had been added after sending message successfully", mvpEvent);
				CombineAssertions(() =>
				{
					AssertEquals("Event free text", ZString.Empty, mvpEvent.ReferenceFreeText);
					AssertEquals("Message type", "Export Notification (755)", mvpEvent.Parameters[Params.MessageType]);
					AssertEquals("Event department", "Terminal", mvpEvent.Parameters[Params.Department]);
				});

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
				var logs = ((IStmALogParent)documentDataStorage).Logs;

				AssertNotNull("Message had been sent", logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).FirstOrDefault());

				var message = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).FirstOrDefault()?.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
			}
		}

		public void TestGetWorkflowTriggerAction_ForCGNExportNotification_ValidationFailed()
		{
			using (Factory.AddDisposableService())
			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Netherlands))
			{
				var consol = new ExportNotificationMessageTestHelper(Factory).CreateConsol();
				consol.JK_MasterBillNum = "";

				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "CGN trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification;

				consol.Logs.AddNew(Events.CustomisableEvent00);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var workflowDescriptor = new JobConsolWorkflowDescriptor();
				var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

				AssertType<ExportNotificationMessageProcessor>(processor);

				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var mvfEvent = consol.Logs.MostRecentLogByEventTime(Events.MessageValidationFailed);
				AssertNotNull("MVF event had been added after sending message failed", mvfEvent);
				CombineAssertions(() =>
				{
					AssertEquals("Event free text", ZString.Empty, mvfEvent.ReferenceFreeText);
					AssertEquals("Message type", "Export Notification (755)", mvfEvent.Parameters[Params.MessageType]);
					AssertEquals("Event department", "Terminal", mvfEvent.Parameters[Params.Department]);
					AssertEquals("Message reason", "Check Export Notification (755) Report Electronic Messaging form for errors.", mvfEvent.Parameters[Params.Reason]);
				});

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
				AssertNull("Message had NOT been sent", ((IStmALogParent)documentDataStorage)?.Logs.MostRecentLogByEventTime(Events.MessageSent));
			}
		}
	}
}
