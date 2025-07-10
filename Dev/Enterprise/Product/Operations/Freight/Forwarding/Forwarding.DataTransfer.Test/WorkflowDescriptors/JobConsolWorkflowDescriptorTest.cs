using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration.Customs.BR;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(JobConsolWorkflowDescriptor))]
	public class JobConsolWorkflowDescriptorTest : JobConsolWorkflowDescriptorTestBase<ForwardingConsol, JobConsolWorkflowDescriptor>
	{
		public void TestValidateForCustomsMessagingThenScheduleDeferredMessageSendOnMessageValidationPassed()
		{
			var cusMAWB = Factory.New<IAUCusMAWB>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08132178904";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "GBLHR";
			var consolForWorkflow = (IWorkflowProvider)consol;
			cusMAWB.CM_JK = consol.PK;

			var triggerVCM = consolForWorkflow.WorkflowItems.Triggers.AddNew();
			triggerVCM.P9_Description = "Validate for Customs Messaging";
			triggerVCM.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			triggerVCM.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerActionVCM = triggerVCM.ProcessTaskNotifications.AddNew();
			triggerActionVCM.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging;
			triggerActionVCM.PQ_Calc_TriggerParty = "EML";
			triggerActionVCM.PQ_EmailAddr = "jbbroker@example.com";
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", false, triggerActionVCM.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", false, triggerActionVCM.PQ_EmailAddrInfo.ReadOnly);

			var triggerSDM = consolForWorkflow.WorkflowItems.Triggers.AddNew();
			triggerSDM.P9_Description = "Scheduled Deferred Message Send";
			triggerSDM.TriggerConditions.TriggerEventCode = Events.MessageValidationPassedCode;
			triggerSDM.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerActionSDM = triggerSDM.ProcessTaskNotifications.AddNew();
			triggerActionSDM.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerActionSDM.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerActionSDM.PQ_EmailAddrInfo.ReadOnly);

			consol.Logs.AddNew(Events.Authorised);

			Factory.Save();

			AssertHasWarning(triggerActionVCM.PQ_TriggerTypeInfo, "This is not an import consol into Australia and therefore no trigger action will be taken.");
			AssertHasWarning(triggerActionSDM.PQ_TriggerTypeInfo, "This is not an import consol into Australia and therefore no trigger action will be taken.");

			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUBNE";
			triggerActionVCM.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging;
			triggerActionSDM.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
			Factory.Save();

			AssertNoWarning(triggerActionVCM.PQ_TriggerTypeInfo, "This is not an import consol into Australia and therefore no trigger action will be taken.");
			AssertNoWarning(triggerActionSDM.PQ_TriggerTypeInfo, "This is not an import consol into Australia and therefore no trigger action will be taken.");

			var query = new ZQuery(StmALogSchema.SL_Parent, triggerVCM.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var messageValidationFailedQuery = new ZQuery(StmALogSchema.SL_Parent, cusMAWB.PK);
			messageValidationFailedQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationFailedCode);
			var messageValidationFailedLogs = Factory.Load<StmALog>(messageValidationFailedQuery);
			AssertEquals("Precondition: Should be no MVF log against the consol.", 0, messageValidationFailedLogs.Length);

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerActionVCM, new QueuedLogForTesting(triggerWTELog, triggerVCM));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);
			AssertNull(workflowProcessor as WorkflowDescriptor.LogAction);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			messageValidationFailedLogs = Factory.Load<StmALog>(messageValidationFailedQuery);
			AssertEquals("Should be an MVF log against the consol now the trigger has fired.", 1, messageValidationFailedLogs.Length);

			consol.JK_AgentType = "AGT";
			consol.JK_ConsolMode = "LSE";
			consol.JK_Phase = "ALL";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_PrepaidCollect = "PPD";

			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport.JW_Status = Constants.OrderStatus.Confirmed;
			transport.JW_VoyageFlight = "QF101";
			transport.JW_RL_NKLoadPort = "GBLHR";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_ETD = new ZDateTime(2013, 9, 21, 14, 0, 5);
			transport.JW_ETA = new ZDateTime(2013, 9, 21, 14, 0, 0);
			transport.JW_ATD = new ZDateTime(2013, 9, 21, 14, 0, 5);

			cusMAWB.CM_MAWB = "08132178904";
			cusMAWB.CM_FlightNo = "QF101";
			cusMAWB.CM_ArrivalDate = new ZDateTime(2013, 9, 21, 14, 0, 5);
			cusMAWB.CM_RL_NKLoadPort = "GBLHR";
			cusMAWB.CM_RL_NKDischargePort = "AUBNE";

			consol.Logs.AddNew(Events.Authorised);

			Factory.Save();

			workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerActionVCM, new QueuedLogForTesting(triggerWTELog, triggerVCM));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);
			AssertNull(workflowProcessor as WorkflowDescriptor.LogAction);

			notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			var messageValidationPassedQuery = new ZQuery(StmALogSchema.SL_Parent, cusMAWB.PK);
			messageValidationPassedQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationPassedCode);
			var messageValidationPassedLogs = Factory.Load<StmALog>(messageValidationPassedQuery);

			AssertEquals("Should be an MVP log against the consol now the trigger has fired.", 1, messageValidationPassedLogs.Length);

			var deferredScheduledMessageLogQuery = new ZQuery(StmALogSchema.SL_Parent, cusMAWB.PK);
			deferredScheduledMessageLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DeferredScheduledMessageCode);

			workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerActionSDM, new QueuedLogForTesting(triggerWTELog, triggerSDM));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);
			AssertNull(workflowProcessor as WorkflowDescriptor.LogAction);

			notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			var deferredScheduledMessageLogs = Factory.Load<StmALog>(deferredScheduledMessageLogQuery);
			AssertEquals("Should be a DSM log against the consol now the trigger has fired.", 1, deferredScheduledMessageLogs.Length);
		}

		public void TestGetAction_NoNull()
		{
			var consolForWorkflow = Factory.New<ForwardingConsol>();
			var trigger = consolForWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills;
			AssertType<WorkflowDescriptor.LogAction>(WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(wteLog, trigger)));
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage;
			AssertType<WorkflowDescriptor.LogAction>(WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(wteLog, trigger)));
		}

		public void TestBillToPartiesAppearOnMessageList()
		{
			#region setup

			var consigneeOrg = CreateOrgWithEmailComms("CNEORG", "consignee@example.com");
			var consignorOrg = CreateOrgWithEmailComms("CNRORG", "consignor@example.com");
			var senderOrg = CreateOrgWithEmailComms("SNDORG", "sender@example.com");
			var billToPartyOrg = CreateOrgWithEmailComms("BTPORG", "billtoparty@example.com");

			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_TransportMode = Constants.TransportModes.Sea;
			testConsol.JK_MasterBillNum = "MB3217890";
			testConsol.JK_RL_NKLoadPort = "NZAKL";
			testConsol.JK_RL_NKDischargePort = "USLAX";
			testConsol.JK_OA_SendingForwarderAddress = senderOrg.MainAddress.PK;

			var testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testShipment.JS_RL_NKOrigin = "NZAKL";
			testShipment.JS_RL_NKDestination = "USLAX";
			testShipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeOrg.MainAddress.PK;
			testShipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = testShipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.LocalChargesPK = billToPartyOrg.PK;

			testConsol.Shipments.Add(testShipment);

			#endregion

			Env.OutgoingMailManager.EmailsCreated.Clear();

			testConsol.GetLogs().AddNew(Events.Departure);

			var workflowMilestone = testConsol.WorkflowItems.Milestones.AddNew();
			workflowMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			var notification = workflowMilestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.BillToParty;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			processor.Process(new NotificationBuffer());
			Factory.Save();

			AssertEquals("One outbound email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull(email);

			AssertEquals("Notification recipient is the billtoparty", "billtoparty@example.com", email.Recipients[0].Email);
		}

		public void TestCustomFieldsAdditionalValidation()
		{
			const string expectedWarning =
				@"There is a Custom Field definition in other 'CON' Workflow Template with same Name which may create confusion with phase control.";

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "CON";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "AAA";
			def1.XC_Type = "STR";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "CON";
			var def2 = template2.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "AAA";
			def2.XC_Type = "STR";

			AssertHasWarning(def2.XC_NameInfo, expectedWarning);

			def2.XC_Type = "INT";

			AssertHasWarning(def2.XC_NameInfo, expectedWarning);

			template2.P0_ProcessType = "SHP";
			def2.XC_Type = "INT";

			AssertNoWarning(def2.XC_NameInfo, expectedWarning);
		}

		public void TestCanExportUniversalEventTriggeredByTransportLegDepEvent()
		{
			using (Factory.AddDisposableService())
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "MB3217890";
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "USLAX";

				var transportLeg = consol.Transports[0];

				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
				trigger.P9_RespondToCascadedEvents = true;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				try
				{
					communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
					communicationsMode.EK_Destination = "9CHARCODE";

					Factory.Save();

					CombineAssertions("Preconditions after initial setup", () =>
					{
						AssertEquals("transportLeg.JW_RL_NKLoadPort", "NZAKL", transportLeg.JW_RL_NKLoadPort);
						AssertEquals("transportLeg.JW_RL_NKDiscPort", "USLAX", transportLeg.JW_RL_NKDiscPort);
						AssertEquals("Trigger should not have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);
						AssertEquals("Trigger Reference should have been defaulted.", trigger.ReferenceCode, transportLeg.JW_RL_NKLoadPort + "->" + transportLeg.JW_RL_NKDiscPort);
					});

					var depatureDate = new ZDateTime(2011, 11, 10);
					transportLeg.JW_ATD = depatureDate;

					Factory.Save();

					var departureLogs = transportLeg.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode));
					CombineAssertions("Preconditions after setting the Departure Date on the Leg.", () =>
					{
						AssertNotNull("'DEP' logs on transportLeg", departureLogs);
						AssertEquals("'DEP' logs on transportLeg", 1, departureLogs.Length);
						AssertEquals("Trigger should have an actual date / time recorded.", depatureDate, trigger.P9_ActualDate);
					});

					var departureLog = departureLogs[0];

					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
					query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
					var triggerLogs = Factory.Load<StmALog>(query);
					AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

					var triggerLog = triggerLogs[0];
					var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

					var processor = GetWorkFlowTriggerAction(action, queuedLog);
					var logger = new NotificationsForTesting();
					processor.Process(logger);

					AssertMultilineASCIIEquals("loggger results from processor.Process()", @"
".Trim(), logger.ToString());

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK));
					AssertEquals("EDIMessages linked to Consol", 1, messages.Length);
					var message = messages[0];
					AssertMultilineASCIIEquals("message.EM_MessageText", $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>DEP</Code>
        <Description>Departure</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(departureLog.SL_EventTimeOffset, () => 0)}</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(departureLog.SL_EventTimeOffset, () => 0)}</EventTime>
    <EventType>DEP</EventType>
    <CreatedTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(departureLog.SL_PostedTimeUtc.ToOffset(), () => 0)}</CreatedTime>
    <EventReference>Changed To: 10-Nov-11|FAC=CTO|LOC=NZAKL|MOD=SEA</EventReference>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB3217890</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USLAX</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>".Trim()
						, message.EM_MessageText);
				}
				finally
				{
					orgProxy.EDICommunicationsModes.RemoveAndDelete(communicationsMode);
					Factory.Save();
				}
			}
		}

		public void TestCanExportUniversalEvent()
		{
			using (Factory.AddDisposableService())
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "MB3217890";
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "USLAX";

				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.ServiceCompletedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Invoice;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				try
				{
					communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
					communicationsMode.EK_Destination = "9CHARCODE";

					var logBO = consol.GetLogs().AddNew(AutoEvents.ServiceCompleted, ZDateTimeOffset.Now);
					Factory.Save();

					AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTimeOffset.Empty, trigger.P9_ActualDate);

					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
					query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
					var triggerLogs = Factory.Load<StmALog>(query);
					AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

					var triggerLog = triggerLogs[0];
					AssertContains("triggerLog.SL_Reference", logBO.PK.ToString(), triggerLog.SL_Reference);

					var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

					var processor = GetWorkFlowTriggerAction(action, queuedLog);
					var logger = new NotificationsForTesting();
					processor.Process(logger);
					Factory.Save();

					AssertMultilineASCIIEquals("loggger results from processor.Process()", @"
".Trim(), logger.ToString());

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK));
					AssertEquals("EDIMessages linked to Consol", 1, messages.Length);
					var message = messages[0];
					AssertMultilineASCIIEquals("message.EM_MessageText", $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>INV</Code>
        <Description>Invoice</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>SVC</Code>
        <Description>Service Completed</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0)}</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.EventTimeOffset, () => 0)}</EventTime>
    <EventType>SVC</EventType>
    <CreatedTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0)}</CreatedTime>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB3217890</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USLAX</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>".Trim()
						, message.EM_MessageText);
				}
				finally
				{
					orgProxy.EDICommunicationsModes.RemoveAndDelete(communicationsMode);
					Factory.Save();
				}
			}
		}

		class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			readonly List<string> notifications = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}

		public void TestCanExportUniversalShipment()
		{
			using (Factory.AddDisposableService())
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "08112345675";
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "USLAX";

				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				try
				{
					communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
					communicationsMode.EK_Destination = "9CHARCODE";

					var logBO = consol.GetLogs().AddNew(Events.FreightLoaded, ZDateTimeOffset.UtcNow);
					Factory.Save();

					AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
					query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
					var triggerLogs = Factory.Load<StmALog>(query);
					AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

					var triggerLog = triggerLogs[0];
					var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

					var processor = GetWorkFlowTriggerAction(action, queuedLog);
					var logger = new NotificationsForTesting();
					processor.Process(logger);
					Factory.Save();

					AssertMultilineASCIIEquals("loggger results from processor.Process()", @"
".Trim(), logger.ToString());

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK));
					AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
					var message = messages[0];

					var regex = new Regex("</DataContext>.*</Shipment>", RegexOptions.Singleline | RegexOptions.CultureInvariant);
					var messageTextWithBodyRemoved = regex.Replace(message.EM_MessageText, delegate
					{
						return @"</DataContext>

    $$ Universal Shipment XML Here $$

  </Shipment>";
					});

					AssertMultilineASCIIEquals("message.EM_MessageText", $@"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>FLO</Code>
        <Description>Freight Loaded</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, delegate { return 0; })}</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    $$ Universal Shipment XML Here $$

  </Shipment>
</UniversalShipment>".Trim()
						, messageTextWithBodyRemoved);

					AssertContains("message.EM_MessageText", @"
    <WayBillNumber>081-12345675</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
", message.EM_MessageText);
				}
				finally
				{
					orgProxy.EDICommunicationsModes.RemoveAndDelete(communicationsMode);
					Factory.Save();
				}
			}
		}

		public void TestStartDestinationPortClearanceProcess()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			ProcessTask processTask = consol.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess;
			IProcessor result = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert(result is SendCargoMessageProcessor);
		}

		public void TestStartDestinationPortClearanceProcessSendsSeaCargoMessage()
		{
			ObjectFactory.New<Enterprise.Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AUC";
			company.GC_CustomsRegistrationNo = "1234";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var cusCode = company.OrgProxy.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "14 001 592 650";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AUB";
			branch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			using (SystemDataRegistry.Instance.AutomaticallySendSeaCargoMessage.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.AUCCompanyCertificateData.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 }))
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.DataType.SuspendValidation())
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "pwd"))
			{
				var consol = SeaConsol;
				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test trigger";
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
				trigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ATD.Name; // "JW_ATD";
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				action.PQ_EmailAddr = "abc@email.com";
				action.PQ_EmailText = "Test LWK";
				Factory.Save();

				// generate the log entry
				var transport = consol.Transports[0];
				transport.JW_ATD = new ZDateTime(2020, 11, 10);
				Factory.Save();

				// Process the log
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var logger = new LoggerForTest();
					var lwm = LogWalkerRunner.Master();
					var lwk = LogWalkerRunner.Default();
					lwm.Process(logger, CancellationToken.None);
					lwk.Process(logger, CancellationToken.None);

					var logs = logger.LogEntries;
					Assert(logs.Any(l => l.Contains("Sea Cargo Messages for 32236346553 were sent.")));
				}

				var factory2 = new BusinessObjectFactory();
				var messages = factory2.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_GB, branch.PK));
				AssertNotNull(messages.FirstOrDefault(m => m.EM_MessageText.Contains("RFF+BH:363463634'RFF+MB:32236346553'")));
				AssertEquals(1, messages.Length);
			}
		}

		ForwardingConsol SeaConsol
		{
			get
			{
				if (seaConsol == null)
				{
					seaConsol = Factory.NewWithValidTestData<ForwardingConsol>();
					seaConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					seaConsol.JK_MasterBillNum = "32236346553";
					seaConsol.JK_RL_NKLoadPort = "UAIEV";
					seaConsol.JK_RL_NKDischargePort = "AUSYD";
					seaConsol.JK_PrepaidCollect = "PPD";
					var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
					shippingLine.OH_RL_NKClosestPort = "AUSYD";
					var abn = shippingLine.CustomsCodes.AddNew("ABN", "23112936991");
					var shippingLineAddress = shippingLine.Addresses.AddNew();
					shippingLineAddress.OA_Address1 = "address1";
					seaConsol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;
					var transport = seaConsol.Transports.AddNew();
					transport.JW_VoyageFlight = "VV2346";
					transport.JW_RL_NKLoadPort = "UAIEV";
					transport.JW_RL_NKDiscPort = "AUSYD";
					transport.JW_ETA = ZDateTime.Today;
					transport.JW_ETD = ZDateTime.Today.AddDays(1);
					transport.JW_Vessel = "ADMIRALENGRACHT";
					var container = seaConsol.Containers.AddNew();
					container.JC_ContainerNum = "CONTAINER";
					container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_UniqueConsignRef = "S000001";
					shipment.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
					shipment.JS_PackingMode = "LCL";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "consignor";
					consignor.OH_Code = "CONSNOR";
					consignor.OH_RL_NKClosestPort = "UAIEV";
					shipment.ConsignorPK = consignor.PK;
					var consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "consignee";
					consignee.OH_Code = "CONSNEE";
					consignee.OH_RL_NKClosestPort = "AUSYD";
					shipment.ConsigneePK = consignee.PK;
					shipment.JS_GoodsDescription = "Downsized Developers";
					shipment.JS_HouseBill = "363463634";
					shipment.JS_RL_NKOrigin = "UAIEV";
					shipment.JS_RL_NKDestination = "AUSYD";
					shipment.JS_ActualWeight = 1;
					shipment.JS_OuterPacks = 1;
					shipment.JS_GoodsValue = 1;
					shipment.JS_RX_NKGoodsValueCurr = "AUD";
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
					seaConsol.Shipments.Add(shipment);

					var packline = shipment.OuterPackLines.AddNew();
					packline.JL_JC = container.PK;
				}

				return seaConsol;
			}
		}
		ForwardingConsol seaConsol;

		public void TestClientName()
		{
			AssertEquals("Carrier", WorkflowDescriptor.ClientName);
		}

		public void TestConditionList1()
		{
			foreach (CodeDescriptionPair code in new JobConsolWorkflowCondition1CodeList())
			{
				if (code.Code != JobConsolWorkflowCondition1CodeList.Codes.MainTransport)
				{
					AssertEquals("List should come from " + nameof(JobConsolWorkflowCondition1CodeList), true, WorkflowDescriptor.GetConditionList1(Milestone).ContainsCode(code.Code));
				}
			}
		}

		public void TestConditionList1_IncludesMainTransportConditionWhenCanBeLinkedToTransport()
		{
			Milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("Main Transport workflow condition available when item can be attached to a leg", true, WorkflowDescriptor.GetConditionList1(Milestone).ContainsCode(JobConsolWorkflowCondition1CodeList.Codes.MainTransport));
			Milestone.TriggerConditions.TriggerEventCode = "";

			Milestone.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ETA.Name;
			AssertEquals("Main Transport workflow condition available when item can be attached to a leg", true, WorkflowDescriptor.GetConditionList1(Milestone).ContainsCode(JobConsolWorkflowCondition1CodeList.Codes.MainTransport));

			Milestone.TriggerConditions.TriggerFieldName = "";
			Milestone.TriggerConditions.TriggerEventCode = "";
			AssertEquals("Main Transport workflow condition not available when the workflow item can't be attached to a leg", false, WorkflowDescriptor.GetConditionList1(Milestone).ContainsCode(JobConsolWorkflowCondition1CodeList.Codes.MainTransport));
		}

		public void TestConditionList2()
		{
			AssertEquals(typeof(JobConsolWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(Milestone).GetType());
		}

		public void TestCondition2ValueList()
		{
			ReleaseTypes types = new ReleaseTypes();
			types.Types.RemoveAndDeleteAll();
			types.Types.Add(new ReleaseType() { Code = "A", Description = (NoResString)"Alpha" });
			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);

			types.Types.Add(new ReleaseType() { Code = "B", Description = (NoResString)"Beta" });
			FreightDataRegistry.Instance.ReleaseTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, types);

			types.Types.Add(new ReleaseType() { Code = "C", Description = (NoResString)"Gamma" });
			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, types);

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_GB = ZGuid.Empty;

			ProcessTask task = template.WorkflowItems.AddNew();
			task.TemplateConditions.TemplateCondition2 = JobConsolWorkflowCondition2CodeList.Codes.ReleaseType;

			CombineAssertions(delegate
			{
				CodeDescriptionPairList list = WorkflowDescriptor.GetConditionValueList2(task);
				AssertEquals("No branch specified: A", true, list.ContainsCode("A"));
				AssertEquals("No branch specified: B", true, list.ContainsCode("B"));
				AssertEquals("No branch specified: C", false, list.ContainsCode("C"));
				AssertEquals("No branch specified: D", false, list.ContainsCode("D"));
			});

			template.P0_GB = GlbBranch.CurrentBranch.PK;

			CombineAssertions(delegate
			{
				CodeDescriptionPairList list = WorkflowDescriptor.GetConditionValueList2(task);
				AssertEquals("Branch specified: A", true, list.ContainsCode("A"));
				AssertEquals("Branch specified: B", true, list.ContainsCode("B"));
				AssertEquals("Branch specified: C", true, list.ContainsCode("C"));
				AssertEquals("Branch specified: D", false, list.ContainsCode("D"));
			});

			task.TemplateConditions.TemplateCondition2 = "";
			AssertMultilineASCIIEquals("", "", WorkflowDescriptor.GetConditionValueList2(task).CodesAsString);
		}

		public void TestNotificationEmailSendToMultipleRecipients()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MB3217890";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transportLeg = consol.Transports[0];

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			trigger.P9_RespondToCascadedEvents = true;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = consignee1.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode.EK_Destination = "contact@consignee1.com";

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode2 = consignee2.EDICommunicationsModes.AddNew();
			communicationsMode2.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode2.EK_Destination = "contact@consignee2.com";

			var communicationsMode3 = consignee2.EDICommunicationsModes.AddNew();
			communicationsMode3.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode3.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode3.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode3.EK_Destination = "contact2@consignee2.com";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneePK = consignee1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneePK = consignee2.PK;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var depatureDate = new ZDateTime(2011, 11, 10);
			transportLeg.JW_ATD = depatureDate;

			Factory.Save();

			var departureLogs = transportLeg.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode));
			var departureLog = departureLogs[0];

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			WorkflowTriggerNotification processor = (WorkflowTriggerNotification)workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			AssertEquals("Should have 3 email tasks", 3, processor.Modes.Destinations.Count);
			AssertEquals("contact@consignee1.com", processor.Modes.CommunicationModes[0].EK_Destination);
			AssertEquals("contact@consignee2.com", processor.Modes.CommunicationModes[1].EK_Destination);
			AssertEquals("contact2@consignee2.com", processor.Modes.CommunicationModes[2].EK_Destination);
		}

		public void TestSendDataToMultipleRecipients()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MB3217890";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_Code = "Consignee1";
			var communicationsMode = consignee1.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "contact@consignee1.com";

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_Code = "Consignee2";
			var communicationsMode2 = consignee2.EDICommunicationsModes.AddNew();
			communicationsMode2.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode2.EK_Destination = "contact@consignee2.com";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneePK = consignee1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneePK = consignee2.PK;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var dataExport = new ManualDataExport(Factory, consol, UniversalDataType.UniversalShipment);
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Consignee;
			dataExport.EventCode = Events.AuthorisedCode;

			CombineAssertions(delegate
			{
				var notifications = new NotificationsForTest();
				using (Factory.AddDisposableService())
				{
					dataExport.SendData(notifications);
					Factory.Save();
				}
				AssertMultilineASCIIEquals("dataExport.SendData() resultMessage", @"
Processing Consol C00001000 (Master Bill='MB3217890')
Universal Shipment queued for sending to Organization [Consignee1].
Processing Consol C00001000 (Master Bill='MB3217890')
Universal Shipment queued for sending to Organization [Consignee2].
".Trim(), notifications.Notifications);
			});
		}

		public void TestCheckConsolDeConsolidatorRecipientParty()
		{
			var task = Consol.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");

			consol.JK_TransportMode = Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");
			AssertHasWarning("Has AU MAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists.");

			var mawb = Factory.New<IAUCusMAWB>();
			mawb.CM_JK = consol.PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have AU MAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists.");
		}

		public void TestCheckTemplateDeConsolidatorRecipientParty()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			var task = template.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

			template.P0_SubType1 = Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");

			template.P0_SubType1 = Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");
		}

		public void TestCheckConsolAirCargoResponsiblePartyRecipient()
		{
			var task = Consol.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");

			consol.JK_TransportMode = Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");
			AssertHasWarning("Has AU MAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists.");

			var mawb = Factory.New<IAUCusMAWB>();
			mawb.CM_JK = consol.PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have AU MAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists.");
			AssertHasWarning("Has Responsible Party warning", notification.PQ_Calc_TriggerPartyInfo, "Responsible party is not specified");

			mawb.CM_OH_ResponsibleParty = Factory.New<OrgHeader>().PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have Responsible Party warning", notification.PQ_Calc_TriggerPartyInfo, "Responsible party is not specified");
		}

		public void TestCheckTemplateAirCargoResponsiblePartyRecipient()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			var task = template.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

			template.P0_SubType1 = Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");

			template.P0_SubType1 = Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air consol warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR consol.");
		}

		public void TestCreateCargoReportRecord()
		{
			foreach (var transportMode in new[] { Constants.TransportModes.Air, Constants.TransportModes.Sea })
			{
				var log = new QueuedLogForTesting(Factory);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_RL_NKLoadPort = "UAIEV";
				consol.JK_TransportMode = Constants.TransportCodes.Rail;
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

				var processTask = consol.WorkflowItems.Triggers.AddNew();
				var action = processTask.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateCargoReportRecord;
				AssertHasError("CCR is available for Air or Sea only", action.PQ_TriggerTypeInfo, "Enter a valid Action.");

				consol.JK_TransportMode = transportMode;
				consol.JK_RL_NKDischargePort = "USCHI";
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				action.Validation.ValidatePQ_TriggerType();
				AssertNoError("CCR is available for Air", action.PQ_TriggerTypeInfo, "Enter a valid Action.");
				AssertHasWarning("AU import only", action.PQ_TriggerTypeInfo, "Automated creation of AU Customs Cargo Record is available for AU import consolidations only.");
				AssertType<WorkflowDescriptor.LogAction>("No processor for trigger action with a warning", WorkflowDescriptor.GetWorkflowTriggerAction(action, log));

				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				action.Validation.ValidatePQ_TriggerType();
				AssertNoWarning("Consol is AU import", action.PQ_TriggerTypeInfo, "Automated creation of AU Customs Cargo Record is available for AU import consolidations only.");
				AssertHasWarning("No Receiving Agent", action.PQ_TriggerTypeInfo, "Receiving Agent is required for automated creation of AU Customs Cargo Record.");
				AssertType<WorkflowDescriptor.LogAction>("No processor for trigger action with a warning", WorkflowDescriptor.GetWorkflowTriggerAction(action, log));

				var forwarder = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
				action.Validation.ValidatePQ_TriggerType();
				AssertNoWarning("Receiving Agent is specified", action.PQ_TriggerTypeInfo, "Automated creation of AU Customs Cargo Record is available for AU import consolidations only.");
				AssertHasWarning("No AU company", action.PQ_TriggerTypeInfo, "Receiving Agent entered does not match any Organization of active AU companies or its branches.");
				AssertType<WorkflowDescriptor.LogAction>("No processor for trigger action with a warning", WorkflowDescriptor.GetWorkflowTriggerAction(action, log));

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_OH_OrgProxy = forwarder.PK;
				company.Branches.AddNew();
				action.Validation.ValidatePQ_TriggerType();
				AssertNoWarning("Forwarder is now AU company org proxy", action.PQ_TriggerTypeInfo, "Receiving Agent entered does not match any Organization of active AU companies or its branches.");

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory)); // processors are cached based on notificationPK and QueuedLog.
				AssertNotNull(processor);
				Assert(!(processor is WorkflowDescriptor.LogAction));
				Assert("Processor is CCRProcessor", processor is Enterprise.Integration.Customs.Shared.ICCRProcessor);
			}
		}

		#region CGN Export Notification

		public void TestGetWorkflowTriggerActionTypes_Include_CGNExportNotification_ForTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new JobConsolWorkflowDescriptor();

			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				template.GlobalTemplate = false;
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should contain CGN action when Global Template is false", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));

				template.GlobalTemplate = true;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CGN action when Global Template is true", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));
			}

			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				template.GlobalTemplate = false;
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CGN action when current company is not Netherlands", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));

				template.GlobalTemplate = true;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CGN action when current company is not Netherlands", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));
			}
		}

		public void TestGetWorkflowTriggerActionTypes_Include_CGNExportNotification_ForConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportCodes.Air;
			consol.JK_RL_NKLoadPort = "NLAMS";
			consol.JK_MasterBillNum = "123-12345678";

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new JobConsolWorkflowDescriptor();

			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
				Assert("Should contain CGN action when current company is France", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));
			}

			using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
				Assert("Should not contain CGN action when current company is not Netherlands", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification));
			}
		}

		#endregion

		#region Send Advanced Air Cargo Report

		public void TestGetWorkflowTriggerActionTypesIncludeACR()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";

			var trigger = template.WorkflowItems.Triggers.AddNew();

			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain ACR action", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage));
		}

		public void TestGetWorkflowTriggerAction_ForAirCargoReport_CCT_ValidationPassed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AGENT SIGNATURE";

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationPassed("BRSAO");
			}
		}

		public void TestGetWorkflowTriggerAction_ForAirCargoReport_CCT_ValidationFailed()
		{
			AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationFailed("BRSAO");
		}

		public void TestGetWorkflowTriggerAction_ForAirCargoReport_ACAS_ValidationPassed()
		{
			var proxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>();
			proxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = proxy.PK;

			AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationPassed("USCHI");
		}

		public void TestGetWorkflowTriggerAction_ForAirCargoReport_ACAS_ValidationFailed()
		{
			var proxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>();
			proxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = proxy.PK;

			AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationFailed("USCHI");
		}

		void AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationPassed(string destination)
		{
			var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
			var consol = testHelper.CreateConsolWithoutErrors("08135025185", "C0000011", "AUSYD", destination);

			var transportLeg = consol.Transports[0];

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			trigger.P9_RespondToCascadedEvents = true;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

			Factory.Save();

			transportLeg.JW_ATD = new ZDateTime(2021, 1, 10);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);

			AssertType<SendAirCargoReprotMessageProcessor>(processor);

			var notifications = new NotificationBuffer();
			using (Factory.AddDisposableService())
			{
				processor.Process(notifications);
			}

			var mvpEvent = consol.Logs.MostRecentLogByEventTime(Events.MessageValidationPassed);
			AssertNotNull("MVP event had been added after sending message successfully", mvpEvent);
			CombineAssertions(() =>
			{
				AssertEquals("Event free text", ZString.Empty, mvpEvent.ReferenceFreeText);
				AssertEquals("Message type", "Advanced Air Cargo Report", mvpEvent.Parameters[Params.MessageType]);
				AssertEquals("Event location", destination.Substring(0, 2), mvpEvent.Parameters[Params.Location]);
			});

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
			var logs = ((IStmALogParent)documentDataStorage).Logs;

			AssertNotNull("Message had been sent", logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).FirstOrDefault());

			var message = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).FirstOrDefault()?.RelatedEDIMessage;
			AssertNotNull("EDI message has been created", message);
		}

		void AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationFailed(string destination)
		{
			var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
			var consol = testHelper.CreateConsolWithoutErrors("08135025185", "C0000011", "AUSYD", destination);
			consol.JK_MasterBillNum = string.Empty;

			var transportLeg = consol.Transports[0];

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			trigger.P9_RespondToCascadedEvents = true;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

			Factory.Save();

			transportLeg.JW_ATD = new ZDateTime(2021, 1, 10);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);

			AssertType<SendAirCargoReprotMessageProcessor>(processor);

			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			var mvfEvent = consol.Logs.MostRecentLogByEventTime(Events.MessageValidationFailed);
			AssertNotNull("MVP event had been added after sending message successfully", mvfEvent);
			CombineAssertions(() =>
			{
				AssertEquals("Event free text", ZString.Empty, mvfEvent.ReferenceFreeText);
				AssertEquals("Message type", "Advanced Air Cargo Report", mvfEvent.Parameters[Params.MessageType]);
				AssertEquals("Event location", destination.Substring(0, 2), mvfEvent.Parameters[Params.Location]);
				AssertEquals("Message reason", "Check Advanced Air Cargo Report Electronic Messaging form for errors.", mvfEvent.Parameters[Params.Reason]);
			});

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
			AssertNull("Message had NOT been sent", ((IStmALogParent)documentDataStorage)?.Logs.MostRecentLogByEventTime(Events.MessageSent));
		}

		#endregion

		#region Create US AMS Data(CAM)

		public void TestGetWorkflowTriggerActionTypesIncludeCAM()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			template.P0_SubType1 = Constants.TransportModes.Sea;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			template.P0_SubType1 = Constants.TransportModes.Rail;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			template.P0_SubType1 = Constants.TransportModes.Air;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			template.P0_SubType1 = Constants.TransportModes.Road;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Shouldn't contain CAM action", !triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
			Assert("Shouldn't contain CAM action", !triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			consol.JK_TransportMode = Constants.TransportModes.Air;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, consol);
			Assert("Should contain CAM action", triggerActionTypes.ContainsCode(JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData));
		}

		public void TestCreateUSAMSData()
		{
			foreach (var transportMode in new[] { Constants.TransportModes.Air, Constants.TransportModes.Sea, Constants.TransportModes.Rail })
			{
				var log = new QueuedLogForTesting(Factory);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKDischargePort = "USCHI";
				consol.JK_RL_NKLoadPort = "UAIEV";
				consol.JK_TransportMode = Constants.TransportCodes.Road;
				var processTask = consol.WorkflowItems.Triggers.AddNew();
				var action = processTask.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = JobConsolWorkflowDescriptor.JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData;
				AssertHasError("CAM is available for Air or Sea or Rail only", action.PQ_TriggerTypeInfo, "Enter a valid Action.");

				consol.JK_TransportMode = transportMode;
				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory)); // processors are cached based on notificationPK and QueuedLog.
				AssertNotNull(processor);
				Assert(!(processor is WorkflowDescriptor.LogAction));
				Assert("Processor is CAMProcessor", processor is Enterprise.Integration.Customs.US.ICAMProcessor);
			}
		}

		#endregion

		#region CO2

		protected override ProcessTaskTemplate CreateTaskTemplateForCO2eTests()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			return template;
		}

		protected override ICO2eCalculationSupporter CreateWithValidDataForCO2eTests()
		{
			return (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);
		}

		protected override ICO2eCalculationSupporter CreateWithInvalidDataForCO2eTests()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);
			consol.JK_TotalShipmentActWeightCheck = 0;
			consol.JK_UniqueConsignRef = "C00001000";

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_TransportMode = "SEA";
			transport.JW_VoyageFlight = "VY1";

			return consol;
		}

		#endregion

		#region EstimateDefaultedFromList

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage));

			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA));
		}

		#endregion

		#region Form Customisation

		public void TestForCustomisationSettings()
		{
			AssertEquals(typeof(ForwardingConsolFormCustomisationSettingsProvider), WorkflowDescriptor.FormCustomisationSettings.GetType());
		}

		#endregion

		public void TestValidationToolSettings()
		{
			AssertType<JobConsolValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		#region Implementation

		static IProcessor GetWorkFlowTriggerAction(ProcessTaskNotification action, QueuedLogForTesting queuedLog)
		{
			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);

			return processor;
		}

		OrgHeader CreateOrgWithEmailComms(string orgCode, string emailAddress)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			var commsMode = orgHeader.EDICommunicationsModes.AddNew();
			commsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			commsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			commsMode.EK_Module = "CON";
			commsMode.EK_Destination = emailAddress;

			return orgHeader;
		}

		ProcessTask Milestone
		{
			get { return milestone ?? (milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew()); }
		}
		ProcessTask milestone;

		IWorkflowProvider WorkflowProvider
		{
			get { return Consol; }
		}

		ForwardingConsol Consol
		{
			get { return consol ?? (consol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol consol;

		class NotificationsForTest : INotifications
		{
			public string Notifications
			{
				get { return string.Join(System.Environment.NewLine, notifications); }
			}

			void INotifications.Add(INotification notification)
			{
				if (notification != null && !string.IsNullOrEmpty(notification.Message))
				{
					notifications.Add(notification.Message);
				}
			}

			readonly List<string> notifications = new List<string>();
		}

		#endregion
	}
}
