using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
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
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.DataTransfer.ForwardingShipmentWorkflowDescriptor;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using IAUCusHAWB = Enterprise.Integration.Customs.AU.ICusHAWB;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingShipmentWorkflowDescriptor))]
	public class ForwardingShipmentWorkflowDescriptorTest : ForwardingShipmentWorkflowDescriptorTestBase<ForwardingShipment, ForwardingShipmentWorkflowDescriptor>
	{
		public void TestCanSendUniversalXMLToImportBroker()
		{
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = importBroker.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "9CHARCODE";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HB3217890";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ImportBroker = importBroker.PK;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.ImportBroker;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var logBO = shipment.GetLogs().AddNew(Events.Authorised, ZDateTimeOffset.UtcNow);
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
			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", "", logger.ToString());

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK));
			AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
			var message = messages[0];
			AssertContains("message.EM_MessageText", @"
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRI</Code>
          <Description>Import Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>".Trim()
				, message.EM_MessageText);
		}

		public void TestCanSendUniversalXMLToExportBroker()
		{
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = exportBroker.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "9CHARCODE";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HB3217890";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.ExportBroker;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var logBO = shipment.GetLogs().AddNew(Events.Authorised, ZDateTimeOffset.UtcNow);
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
			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", "", logger.ToString());

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK));
			AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
			var message = messages[0];
			AssertContains("message.EM_MessageText", @"
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRE</Code>
          <Description>Export Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>".Trim()
				, message.EM_MessageText);
		}

		public void TestCustomFieldsAdditionalValidation()
		{
			const string expectedWarning =
				@"There is a Custom Field definition in other 'SHP' Workflow Template with same Name which may create confusion with phase control.";

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "AAA";
			def1.XC_Type = "STR";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SHP";
			var def2 = template2.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "AAA";
			def2.XC_Type = "STR";

			AssertHasWarning(def2.XC_NameInfo, expectedWarning);

			def2.XC_Type = "INT";

			AssertHasWarning(def2.XC_NameInfo, expectedWarning);

			template2.P0_ProcessType = "CON";
			def2.XC_Type = "INT";

			AssertNoWarning(def2.XC_NameInfo, expectedWarning);
		}

		public void TestSupportedMessageRecipientParties_DeConsolidator()
		{
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var shipment = Factory.New<ForwardingShipment>();
			Assert("Should have DeConsolidator available.", workflowDescriptor.SupportedMessageRecipientParties(shipment.WorkflowItems.AddNew(), shipment).HasFlag(MessageRecipientPartyType.DeConsolidator));
		}

		public void TestSupportedMessageRecipientParties_WarehouseInwards()
		{
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var shipment = Factory.New<ForwardingShipment>();
			Assert("Should have DeConsolidator available.", workflowDescriptor.SupportedMessageRecipientParties(shipment.WorkflowItems.AddNew(), shipment).HasFlag(MessageRecipientPartyType.WarehouseInwards));
		}

		public void TestHVLVAirAndSeaClearanceAgentsAvailable()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			Assert("Should have HVLVAirClearanceAgent available.", workflowDescriptor.SupportedMessageRecipientParties(shipment.WorkflowItems.AddNew(), shipment).HasFlag(MessageRecipientPartyType.HVLVAirClearanceAgent));
			Assert("Should have HVLVSeaClearanceAgent available.", workflowDescriptor.SupportedMessageRecipientParties(shipment.WorkflowItems.AddNew(), shipment).HasFlag(MessageRecipientPartyType.HVLVSeaClearanceAgent));
		}

		public void TestHVLVAirAndSeaUniversalShipmentWillBeSentInternally()
		{
			AssertUniversalShipmentWillBeSentInternally(MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent, Constants.TransportModes.Air);
			AssertUniversalShipmentWillBeSentInternally(MessageRecipientPartyTypeList.Codes.HVLVSeaClearanceAgent, Constants.TransportModes.Sea);
		}

		void AssertUniversalShipmentWillBeSentInternally(ZString recipient, ZString transportMode)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "HB3217890";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.HVLVReadyCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_Calc_TriggerParty = recipient;

			var logBO = shipment.GetLogs().AddNew(Events.HVLVReady, ZDateTimeOffset.UtcNow);
			Factory.Save();

			AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			var logger = new NotificationsForTesting();

			var universalXMLProcessor = processor as UniversalXmlWorkflowProcessor;
			AssertNotNull("universalXMLProcessor", universalXMLProcessor);
			var communicationsModes = ((IMessageProcessor)universalXMLProcessor).GetDestinations();
			AssertEquals("Should find valid recipient for the Universal Shipment", 1, communicationsModes.Destinations.Count);
			var communicationsMode = communicationsModes.Destinations[0] as IEDICommunicationsMode;
			AssertEquals("Should send the Universal Shipment internally through UniversalDataBuss", EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, communicationsMode.EK_CommunicationsTransport);
		}

		public void TestCanExportUniversalEvent()
		{
			using (Factory.AddDisposableService())
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HB3217890";
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USLAX";

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				var logBO = shipment.GetLogs().AddNew(Events.Authorised, ZDateTimeOffset.Now);
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

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK));
				AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
				var message = messages[0];
				AssertMultilineASCIIEquals("message.EM_MessageText", $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
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

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0)}</EventTime>
    <EventType>ATH</EventType>
    <CreatedTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0)}</CreatedTime>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>HB3217890</Value>
      </Context>
      <Context>
        <Type>HAWBOriginIATAAirportCode</Type>
        <Value>AKL</Value>
      </Context>
      <Context>
        <Type>HAWBDestinationIATAAirportCode</Type>
        <Value>LAX</Value>
      </Context>
      <Context>
        <Type>HBOLOriginUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
      <Context>
        <Type>HBOLDestinationUNLOCO</Type>
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
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HB3217890";
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USLAX";

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.CustomsCommencedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				var logBO = shipment.GetLogs().AddNew(Events.CustomsCommenced, ZDateTimeOffset.UtcNow);
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

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK));
				AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
				var message = messages[0];

				var regex = new Regex("</DataContext>.*</Shipment>", RegexOptions.Singleline | RegexOptions.CultureInvariant);
				var messageTextWithBodyRemoved = regex.Replace(message.EM_MessageText, delegate
				{
					return @"</DataContext>

    $$ Universal Shipment XML Here $$

  </Shipment>";
				});

				AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(@"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
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
        <Code>CCC</Code>
        <Description>Customs Commenced</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
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
</UniversalShipment>"
						, SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, delegate
						{ return 0; })).Trim()
					, messageTextWithBodyRemoved);

				AssertContains("message.EM_MessageText", @"
    <WayBillNumber>HB3217890</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
", message.EM_MessageText);
			}
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("Consolidation milestones are combined with the Shipment milestones below to produce the complete list.", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Shipment, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestConditionList1()
		{
			AssertEquals(typeof(JobShipmentWorkflowCondition1CodeList), WorkflowDescriptor.GetConditionList1(null).GetType());
		}

		public void TestConditionList2()
		{
			AssertEquals(typeof(JobShipmentWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(null).GetType());
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
			task.TemplateConditions.TemplateCondition2 = JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType;

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

		public void TestGetAdditionalRootTypeForTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var workItem = template.WorkflowItems.AddNew();
			workItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			var triggerConditions = workItem as IBaseTrigger;
			var templateConditions = new TemplateConditionsViewModel(workItem, template);

			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(null, null));
			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(null, workItem.TriggerConditions));
			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(templateConditions, null));
			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(templateConditions, workItem.TriggerConditions));

			templateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(templateConditions, workItem.TriggerConditions));
			templateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			AssertEquals(null, WorkflowDescriptor.GetAdditionalRootType(templateConditions, workItem.TriggerConditions));

			triggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			AssertEquals(typeof(BaseJobDeclaration), WorkflowDescriptor.GetAdditionalRootType(templateConditions, workItem.TriggerConditions));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				triggerConditions.TriggerCompany = GlbCompany.CurrentCompany.PK;
				var type = WorkflowDescriptor.GetAdditionalRootType(templateConditions, workItem.TriggerConditions);
				AssertEquals("Enterprise.Customs.AU.Declaration.Business.JobDeclaration", type.FullName);
			}
		}

		public void TestValidationToolSettings()
		{
			AssertType<ForwardingShipmentValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		public void TestMacroTypesForProcessTask()
		{
			var task = Factory.New<ProcessTask>();
			AssertSequencesEqual("Task has no Parent Job and no extra macro type", new[] { task.GetType() }, WorkflowDescriptor.MacroTypes(task));

			var taskparent = Factory.New<ProcessTask>();
			task.P9_ParentID = taskparent.PK;
			AssertSequencesEqual("Task's Parent Job is not shipment and no extra macro type", new[] { task.GetType() }, WorkflowDescriptor.MacroTypes(task));

			var shipment = Factory.New<ForwardingShipment>();
			var shipmentTask = shipment.WorkflowItems.Triggers.AddNew();
			AssertSequencesEqual("Task's Parent Job is shipment but shipment has no attached declaration and no extra macro type", new[] { shipmentTask.GetType() }, WorkflowDescriptor.MacroTypes(shipmentTask));

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DNZ";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GC = company.PK;
			AssertNotEquals("Precondition", declaration.JE_GC, shipmentTask.TriggerConditions.TriggerCompany);
			AssertSequencesEqual("Task's Parent Job is shipment and shipment has attached declaration which company is not the same as the context, no extra macro type", new[] { shipmentTask.GetType() }, WorkflowDescriptor.MacroTypes(shipmentTask));

			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals("Precondition", declaration.JE_GC, shipmentTask.TriggerConditions.TriggerCompany);
			AssertSequencesEqual("Task's Parent Job is shipment and shipment has attached declaration which company is the same as the context, has extra macro type(type of declaration)", new[] { shipmentTask.GetType(), declaration.GetType() }, WorkflowDescriptor.MacroTypes(shipmentTask));
		}

		public void TestMacroTypesForProcessTaskNotication()
		{
			var action = Factory.New<ProcessTaskNotification>();
			AssertSequencesEqual("Action has no Parent and no extra macro type", new[] { action.GetType() }, WorkflowDescriptor.MacroTypes(action));

			var task = Factory.New<ProcessTask>();
			action.PQ_P9 = task.PK;
			AssertSequencesEqual("Action's Parent Job is not shipment and no extra macro type", new[] { action.GetType() }, WorkflowDescriptor.MacroTypes(action));

			var shipment = Factory.New<ForwardingShipment>();
			var shipmentTask = shipment.WorkflowItems.Triggers.AddNew();
			action.PQ_P9 = shipmentTask.PK;
			AssertSequencesEqual("Action's Parent Job is shipment but shipment has no attached declaration and no extra macro type", new[] { action.GetType() }, WorkflowDescriptor.MacroTypes(action));

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DNZ";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GC = company.PK;
			AssertNotEquals("Precondition", declaration.JE_GC, shipmentTask.TriggerConditions.TriggerCompany);
			AssertSequencesEqual("Action's Parent Job is shipment and shipment has attached declaration which company is not the same as the context, no extra macro type", new[] { action.GetType() }, WorkflowDescriptor.MacroTypes(action));

			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals("Precondition", declaration.JE_GC, shipmentTask.TriggerConditions.TriggerCompany);
			AssertSequencesEqual("Action's Parent Job is shipment and shipment has attached declaration which company is the same as the context, has extra macro type(type of declaration)", new[] { action.GetType(), declaration.GetType() }, WorkflowDescriptor.MacroTypes(action));
		}

		public void TestCheckConsolDeConsolidatorRecipientParty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var task = shipment.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");
			AssertHasWarning("Has AU HAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists.");

			var mawb = Factory.New<IAUCusMAWB>();
			var hawb = Factory.New<IAUCusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_JS = shipment.PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have AU HAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists.");
		}

		public void TestCheckTemplateDeConsolidatorRecipientParty()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			var task = template.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

			template.P0_SubType1 = Enterprise.Core.Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");

			template.P0_SubType1 = Enterprise.Core.Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");
		}

		public void TestCheckConsolAirCargoResponsibleParty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var task = shipment.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");
			AssertHasWarning("Has AU HAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists.");

			var mawb = Factory.New<IAUCusMAWB>();
			var hawb = Factory.New<IAUCusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_JS = shipment.PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have AU HAWB warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists.");
			AssertHasWarning("Has Responsible Party warning", notification.PQ_Calc_TriggerPartyInfo, "Responsible party is not specified");

			mawb.CM_OH_ResponsibleParty = Factory.New<OrgHeader>().PK;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have Responsible Party warning", notification.PQ_Calc_TriggerPartyInfo, "Responsible party is not specified");
		}

		public void TestCheckTemplateAirCargoResponsibleParty()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			var task = template.WorkflowItems.Triggers.AddNew();
			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

			template.P0_SubType1 = Enterprise.Core.Constants.TransportModes.Sea;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Has air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");

			template.P0_SubType1 = Enterprise.Core.Constants.TransportModes.Air;
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("Doesn't have air shipment warning", notification.PQ_Calc_TriggerPartyInfo, "This recipient is valid only for AIR shipment.");
		}

		public void TestScheduleB3MessageAction_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				AssertCollectionNotContains("Trigger action Schedule CAD Message is valid", "SB3", triggerAction.Lookups.WorkflowTriggerActionTypes);
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
				AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
				AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

				shipment.Logs.AddNew(Events.MessageValidationPassed);
				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertType<WorkflowDescriptor.LogAction>("Should not do the trigger action for a US declaration", workflowProcessor);
			}
		}

		public void TestScheduleB3MessageAction_CA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_JS = shipment.PK;
				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				AssertCollectionNotContains("Trigger action Schedule CAD Message is valid", "SB3", triggerAction.Lookups.WorkflowTriggerActionTypes);
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
				AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
				AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

				shipment.Logs.AddNew(Events.MessageValidationPassed);
				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor", workflowProcessor.GetType().FullName);

				AssertNoExceptionThrown(() =>
				{
					var notification = new NotificationBuffer();
					workflowProcessor.Process(notification);
					Factory.Save();
				});

				var logger = new BatchProcessor.LoggingInformation();
				AssertNoExceptionThrown(() =>
				{
					var autoSendMessageProcessor = new Customs.Business.BatchProcessor.AutoSendCustomsMessagingBatchProcessor(logger);
					autoSendMessageProcessor.ExecuteBatch();
				});
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				var logs = new ZStringBuilder();
				var enumerator = logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				AssertContains("Processing: Declaration S00001000", logs.ToStringWithNewLineBetweenAppends());
				AssertContains("Sending CAD message for Declaration S00001000 is not allowed", logs.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestSubmitAVSQueryAction_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				AssertCollectionNotContains("Trigger action Submit AVS Query is valid", "AVS", triggerAction.Lookups.WorkflowTriggerActionTypes);
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
				AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
				AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

				shipment.Logs.AddNew(Events.MessageValidationPassed);
				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertType<WorkflowDescriptor.LogAction>("Should not do the trigger action for a US declaration", workflowProcessor);
			}
		}

		public void TestSubmitAVSQueryAction_CA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				AssertCollectionNotContains("Trigger action Submit AVS Query is valid", "AVS", triggerAction.Lookups.WorkflowTriggerActionTypes);
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
				AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
				AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

				shipment.Logs.AddNew(Events.MessageValidationPassed);
				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.CA.Business.SubmitAVSQueryProcessor", workflowProcessor.GetType().FullName);
			}
		}

		public void TestSendBrokerageDocument_WhenNoBrokerageStillReturnsAction()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			AssertCollectionNotContains("Trigger action Submit AVS Query is valid", "AVS", triggerAction.Lookups.WorkflowTriggerActionTypes);
			triggerAction.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument;
			shipment.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			var triggerWTELog = triggerWTELogs[0];

			AssertNotNull(new ForwardingShipmentWorkflowDescriptor().GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger)));
		}

		public void TestSupportedLineTriggerTypes() => AssertContainsExactElementsInAnyOrder(new[] { TriggerLineTypes.Codes.CusExitReport, TriggerLineTypes.Codes.CusEntryHeader }, ((ForwardingShipmentWorkflowDescriptor)WorkflowDescriptor).SupportedTriggerLineTypes);

		#region Send AU Cargo Report

		public void TestGetWorkflowTriggerActionTypesIncludeAARandASR()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			var trigger1 = template.WorkflowItems.Triggers.AddNew();

			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_SubType1 = TransportModes.Sea;
			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = "AUSYD";
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = "AUSYD";
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: true);

			template.P0_SubType1 = TransportModes.Air;
			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = "AUSYD";
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = "AUSYD";
			AssertContainTriggerAction(CountryCodes.Australia, template, null, trigger1, containAirTriggerAction: true, containSeaTriggerAction: false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: true);

			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertContainTriggerAction(CountryCodes.Australia, null, shipment, trigger2, containAirTriggerAction: true, containSeaTriggerAction: false);
		}

		public void TestProcess_AUAirCargoReportTrigger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertProcess_CargoReportTriggers(TransportModes.Air, "AUSYD", WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport);
			}
		}

		public void TestProcess_AUSeaCargoReportTrigger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertProcess_CargoReportTriggers(TransportModes.Sea, "AUSYD", WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport);
			}
		}

		#endregion

		#region Send NZ Cargo Report

		public void TestGetWorkflowTriggerActionTypesIncludeNACandNSC()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var trigger1 = template.WorkflowItems.Triggers.AddNew();

			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_SubType1 = TransportModes.Sea;
			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = "NZAKL";
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: true);

			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = "NZAKL";
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: true);

			template.P0_SubType1 = TransportModes.Air;
			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: false, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = "NZAKL";
			template.P0_DischargePortCountry = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: true, containSeaTriggerAction: false);

			template.P0_LoadPortCountry = String.Empty;
			template.P0_DischargePortCountry = "NZAKL";
			AssertContainTriggerAction(CountryCodes.NewZealand, template, null, trigger1, containAirTriggerAction: true, containSeaTriggerAction: false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: true);

			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: true);

			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: false, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = String.Empty;
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: true, containSeaTriggerAction: false);

			shipment.JS_RL_NKOrigin = String.Empty;
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertContainTriggerAction(CountryCodes.NewZealand, null, shipment, trigger2, containAirTriggerAction: true, containSeaTriggerAction: false);
		}

		void AssertContainTriggerAction(string countryCode, ProcessTaskTemplate template, ForwardingShipment shipment, ProcessTask trigger, bool containAirTriggerAction, bool containSeaTriggerAction)
		{
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			ICodeDescriptionPairList triggerActionTypes = null;
			if (template == null)
			{
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
			}
			else
			{
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			}

			if (countryCode.Equals(CountryCodes.Australia))
			{
				AssertEquals(triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport), containAirTriggerAction);
				AssertEquals(triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport), containSeaTriggerAction);
			}
			else if (countryCode.Equals(CountryCodes.NewZealand))
			{
				AssertEquals(triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport), containAirTriggerAction);
				AssertEquals(triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport), containSeaTriggerAction);
			}
		}

		public void TestProcess_NZAirCargoReportTrigger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				AssertProcess_CargoReportTriggers(TransportModes.Air, "NZAKL", WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport);
			}
		}

		public void TestProcess_NZSeaCargoReportTrigger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				AssertProcess_CargoReportTriggers(TransportModes.Sea, "NZAKL", WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport);
			}
		}

		void AssertProcess_CargoReportTriggers(string transportMode, string destination, string actionType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = transportMode;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment.FillWithValidTestData();
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;

			var item = Factory.New<IHVLVItem>() as BusinessObject;
			item.FillWithValidTestData();
			item[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment.PK;

			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";
			item[HVLVItemSchema.HVI_ContainerNumber.Name] = container.ContainerNumberForBinding;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			trigger.ProcessTaskNotifications.Add(action);
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;
			action.PQ_TriggerType = actionType;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			shipment.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var genPivot = Factory.LoadTop1<GenPivot>(new ZQuery());
			AssertNull("No data exists in table GenPivot", genPivot);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			var notifications = new NotificationCollection();
			processor.Process(notifications);

			var genPivotCollection = Factory.Load<GenPivot>(new ZQuery());
			AssertEquals("A log about the trigger action process should be created in table GenPivot", 1, genPivotCollection.Length);
		}

		#endregion

		#region Create H7 Declaration

		public void TestGetWorkflowTriggerActionTypesIncludeCH7_WorkflowModule()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();

			CombineAssertions(() =>
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should NOT contain CH7 action with empty Origin and empty Destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				template.P0_LoadPortCountry = CountryCodes.Ireland;
				template.P0_DischargePortCountry = CountryCodes.Ireland;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should NOT contain CH7 action with the same Origin and Destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				template.P0_LoadPortCountry = "IEDUK";
				template.P0_DischargePortCountry = "IEDUB";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should NOT contain CH7 action with Origin and Destination in the same country", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				template.P0_LoadPortCountry = CountryCodes.France;
				template.P0_DischargePortCountry = CountryCodes.Australia;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should NOT contain CH7 action with non-EU Destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				template.P0_LoadPortCountry = "AUSYD";
				template.P0_DischargePortCountry = "ESMAD";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should contain CH7 action with another country Origin port and EU Destination port", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				var euCountryCodes = CountryCodes.GetAll().Where(c => CountryCodes.IsInEuropeanCustomsUnion(c) || c == CountryCodes.UnitedKingdom || c == CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);

				foreach (var countryCode in euCountryCodes)
				{
					template.P0_LoadPortCountry = CountryCodes.Australia;
					template.P0_DischargePortCountry = countryCode;
					triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
					Assert($"Should contain CH7 action with another country Origin and {countryCode} Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

					template.P0_LoadPortCountry = ZString.Empty;
					template.P0_DischargePortCountry = countryCode;
					triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
					Assert($"Should contain CH7 action with empty Origin and {countryCode} Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));
				}
			});
		}

		public void TestGetWorkflowTriggerActionTypesIncludeCH7_ShipmentModule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = string.Empty;
			shipment.JS_RL_NKDestination = string.Empty;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();

			CombineAssertions(() =>
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should NOT contain CH7 action with empty Origin and empty Destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				shipment.JS_RL_NKOrigin = "IEDUK";
				shipment.JS_RL_NKDestination = "IEDUB";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should not contain CH7 action with Origin and Destination in the same country", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				shipment.JS_RL_NKOrigin = "IEDUK";
				shipment.JS_RL_NKDestination = "AUSYD";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should NOT contain CH7 action with non-EU Destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				shipment.JS_RL_NKOrigin = string.Empty;
				shipment.JS_RL_NKDestination = "ESMAD";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain CH7 action with empty Origin and EU Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GBLON";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain CH7 action with another country Origin and EU Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration));
			});
		}
		#endregion

		#region Create HVLV US Truck e-Manifest

		public void TestGetWorkflowTriggerActionTypesIncludeUTE()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

			template.P0_SubType1 = TransportModes.Road;
			template.P0_DischargePortCountry = CountryCodes.Australia;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should not contain UTE action as template is not US destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));

			template.P0_SubType1 = TransportModes.Road;
			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.UnitedStates;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should not contain UTE action as template is not Non-US origin", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));

			template.P0_SubType1 = TransportModes.Sea;
			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should not contain UTE action as template is not ROA transport mode", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));

			template.P0_SubType1 = TransportModes.Road;
			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain UTE action for ROA, Non-US Origin and US Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));

			template.P0_SubType1 = TransportModes.Road;
			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = ZString.Empty;
			triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
			Assert("Should contain UTE action for ROA, empty Origin and US Destination", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));
		}

		public void TestShipmentTriggerActionTypesIncludeUTE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				AssertEquals("Precondition: direction is EXP", Directions.Export, shipment.JobDirection);

				var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should not contain UTE action as shipment is not IMP and US destination", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);

				CombineAssertions(() =>
				{
					AssertEquals("Direction is IMP", Directions.Import, shipment.JobDirection);
					Assert("Should not contain UTE action as template is not ROA transport mode", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));
				});

				shipment.JS_TransportMode = "ROA";
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain UTE action", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest));
			}
		}

		#endregion

		#region Send Advanced Air Cargo Report

		public void TestGetWorkflowTriggerActionTypesIncludeACR()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var trigger = template.WorkflowItems.Triggers.AddNew();

			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
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
			using (Factory.AddDisposableService())
			{
				var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
				var shipment = testHelper.CreateShipmentWithoutErrors("08135025185", "C0000011", "AUSYD", destination);

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage;
				triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

				shipment.Logs.AddNew(Events.Arrival);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
				var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

				AssertType<SendAirCargoReprotMessageProcessor>(processor);

				var notifications = new NotificationBuffer();
				processor.Process(notifications);
				var mvpEvent = shipment.Logs.MostRecentLogByEventTime(Events.MessageValidationPassed);
				AssertNotNull("MVP event had been added after sending message successfully", mvpEvent);
				CombineAssertions(() =>
				{
					AssertEquals("Event free text", ZString.Empty, mvpEvent.ReferenceFreeText);
					AssertEquals("Message type", "Advanced Air Cargo Report", mvpEvent.Parameters[Params.MessageType]);
					AssertEquals("Event location", destination.Substring(0, 2), mvpEvent.Parameters[Params.Location]);
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

		void AssertGetWorkflowTriggerAction_ForAirCargoReport_ValidationFailed(string destination)
		{
			var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
			var shipment = testHelper.CreateShipmentWithoutErrors("08135025185", "C0000011", "AUSYD", destination);
			shipment.JS_HouseBill = string.Empty;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

			shipment.Logs.AddNew(Events.Arrival);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

			AssertType<SendAirCargoReprotMessageProcessor>(processor);

			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			var mvfEvent = shipment.Logs.MostRecentLogByEventTime(Events.MessageValidationFailed);
			AssertNotNull("MVP event had been added after sending message successfully", mvfEvent);
			CombineAssertions(() =>
			{
				AssertEquals("Event free text", ZString.Empty, mvfEvent.ReferenceFreeText);
				AssertEquals("Message type", "Advanced Air Cargo Report", mvfEvent.Parameters[Params.MessageType]);
				AssertEquals("Event location", destination.Substring(0, 2), mvfEvent.Parameters[Params.Location]);
				AssertEquals("Message reason", "Check Advanced Air Cargo Report Electronic Messaging form for errors.", mvfEvent.Parameters[Params.Reason]);
			});

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, shipment.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
			AssertNull("Message had NOT been sent", ((IStmALogParent)documentDataStorage)?.Logs.MostRecentLogByEventTime(Events.MessageSent));
		}

		public void TestGetSendAirCargoReportMessageProcessor_WhenShipmentIsHVLV()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage;
			var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for SendACASReport should be HVLVValidateAndSendOriginalACASReportProcessor ", "HVLVValidateAndSendOriginalACASReportProcessor", resultProcessor.GetType().Name);
		}

		#endregion

		#region EstimateDefaultedFromList

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage));

			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA));

			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA));
		}

		#endregion

		#region Event Context

		#region TestGetFollowingContextSteps

		public void TestGetFollowingContextSteps()
		{
			var masterClassifiers = new ForwardingShipmentWorkflowEventContextMasterClassifiers();
			var masterTypes = new ForwardingShipmentWorkflowEventContextMasterTypes();

			var allContexts =
				new[]
				{
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),

					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]),

					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment]),
				};

			AssertFollowingContextSteps(null, allContexts);
			AssertFollowingContextSteps(Enumerable.Empty<WorkflowEventContextPair>(), allContexts);
			AssertFollowingContextSteps(
				new[]
				{
					new WorkflowEventContextPair(new CodeDescriptionPair("XYZ", null), masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment]),
				},
				allContexts);
			AssertFollowingContextSteps(
				new[]
				{
					new WorkflowEventContextPair(new CodeDescriptionPair("XYZ", null), new CodeDescriptionPair("XYZ", null)),
					new WorkflowEventContextPair(new CodeDescriptionPair("XYZ", null), masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment]),
				},
				allContexts);

			AssertFollowingContextSteps(
				new[]
				{
					new WorkflowEventContextPair(new CodeDescriptionPair("XYZ", null), masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]),
				},
				new[]
				{
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
					new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
				});

			AssertFollowingContextSteps(
				new[]
				{
					new WorkflowEventContextPair(new CodeDescriptionPair("XYZ", null), masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]),
				},
				Enumerable.Empty<WorkflowEventContextPair>());
		}

		void AssertFollowingContextSteps(IEnumerable<WorkflowEventContextPair> contextPath, IEnumerable<WorkflowEventContextPair> expectedSteps)
		{
			AssertContainsExactElementsInAnyOrder(
				new LambdaComparer<WorkflowEventContextPair>((a, b) => a.MasterClassifier.Code == b.MasterClassifier.Code && a.MasterType.Code == b.MasterType.Code, a => HashCodeHelper.GetCompositeHashCode(new[] { a.MasterClassifier.Code, a.MasterType.Code })),
				expectedSteps,
				WorkflowDescriptor.GetFollowingContextSteps(contextPath));
		}

		#endregion

		#region TestIsRelatedEntityInContextCore

		public void TestIsRelatedEntityInContextCore_EmptyContext()
		{
			var bizo1 = Factory.New<ForwardingShipment>();
			var bizo2 = Factory.New<ForwardingShipment>();

			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo2, null));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo1, null));

			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo2, ""));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo1, ""));
		}

		public void TestIsRelatedEntityInContextCore_MasterShipment()
		{
			var bizo1 = Factory.New<ForwardingShipment>();
			var bizo2 = Factory.New<ForwardingShipment>();
			var bizo3 = Factory.New<ForwardingShipment>();

			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo2, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));

			bizo1.JS_JS_ColoadMasterShipment = bizo2.PK;

			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo2, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(bizo2, bizo1, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));

			bizo2.JS_JS_ColoadMasterShipment = bizo3.PK;

			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo2, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo3, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo2, bizo3, ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(bizo1, bizo3,
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment + "," + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment));
		}

		public void TestIsRelatedEntityInContextCore_Consol()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consol1 = shipment.Consols.AddNew();
			consol1.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol1.Transports[0].JW_ETA = ZDateTime.Now.AddDays(2);

			var consol2 = shipment.Consols.AddNew();
			consol2.Transports[0].JW_ETD = ZDateTime.Now.AddDays(3);
			consol2.Transports[0].JW_ETA = ZDateTime.Now.AddDays(4);

			var consol3 = shipment.Consols.AddNew();
			consol3.Transports[0].JW_ETD = ZDateTime.Now.AddDays(5);
			consol3.Transports[0].JW_ETA = ZDateTime.Now.AddDays(6);

			const string everyConsol = ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol;
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol1, everyConsol));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol2, everyConsol));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol3, everyConsol));

			const string departureConsol = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol;
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol1, departureConsol));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol2, departureConsol));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol3, departureConsol));

			const string transhipConsol = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol1, transhipConsol));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol2, transhipConsol));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol3, transhipConsol));

			const string arrivalConsol = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol1, arrivalConsol));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol2, arrivalConsol));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, consol3, arrivalConsol));
		}

		public void TestIsRelatedEntityInContextCore_ShipmentLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_ETD = ZDateTime.Now.AddDays(1);
			transport1.JW_ETA = ZDateTime.Now.AddDays(2);

			var transport2 = shipment.Consols.AddNew().Transports[0];
			transport2.JW_ETD = ZDateTime.Now.AddDays(3);
			transport2.JW_ETA = ZDateTime.Now.AddDays(4);

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_ETD = ZDateTime.Now.AddDays(5);
			transport3.JW_ETA = ZDateTime.Now.AddDays(6);

			const string leg = ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, leg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, leg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, leg));

			const string departureLeg = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, departureLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, departureLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, departureLeg));

			const string transshipLeg = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, transshipLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, transshipLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, transshipLeg));

			const string arrivalLeg = ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, arrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, arrivalLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, arrivalLeg));
		}

		public void TestIsRelatedEntityInContextCore_ConsolLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			var transportS1 = shipment.Transports.AddNew();
			transportS1.JW_ETD = ZDateTime.Now.AddDays(1);
			transportS1.JW_ETA = ZDateTime.Now.AddDays(2);

			var transport1 = consol.Transports[0];
			transport1.JW_ETD = ZDateTime.Now.AddDays(3);
			transport1.JW_ETA = ZDateTime.Now.AddDays(4);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = ZDateTime.Now.AddDays(5);
			transport2.JW_ETA = ZDateTime.Now.AddDays(6);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_ETD = ZDateTime.Now.AddDays(7);
			transport3.JW_ETA = ZDateTime.Now.AddDays(8);

			var transportS2 = shipment.Transports.AddNew();
			transportS2.JW_ETD = ZDateTime.Now.AddDays(9);
			transportS2.JW_ETA = ZDateTime.Now.AddDays(10);

			const string leg = ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(consol, transportS1, leg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(consol, transport1, leg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(consol, transport2, leg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(consol, transport3, leg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(consol, transportS2, leg));

			const string consolLeg =
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol + "," +
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS1, consolLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, consolLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, consolLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, consolLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS2, consolLeg));

			const string consolDepartureLeg =
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol + "," +
				ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS1, consolDepartureLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, consolDepartureLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, consolDepartureLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, consolDepartureLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS2, consolDepartureLeg));

			const string consolTranshipLeg =
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol + "," +
				ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS1, consolTranshipLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, consolTranshipLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, consolTranshipLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, consolTranshipLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS2, consolTranshipLeg));

			const string consolArrivalLeg =
				ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol + "," +
				ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS1, consolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport1, consolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport2, consolArrivalLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transport3, consolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS2, consolArrivalLeg));
		}

		public void TestIsRelatedEntityInContextCore_DepartureConsolArrivalLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var transportS1 = shipment.Transports.AddNew();
			transportS1.JW_ETD = ZDateTime.Now.AddDays(1);
			transportS1.JW_ETA = ZDateTime.Now.AddDays(2);

			var transportC11 = consol1.Transports[0];
			transportC11.JW_ETD = ZDateTime.Now.AddDays(3);
			transportC11.JW_ETA = ZDateTime.Now.AddDays(4);

			var transportC12 = consol1.Transports.AddNew();
			transportC12.JW_ETD = ZDateTime.Now.AddDays(5);
			transportC12.JW_ETA = ZDateTime.Now.AddDays(6);

			var transportC21 = consol2.Transports[0];
			transportC21.JW_ETD = ZDateTime.Now.AddDays(7);
			transportC21.JW_ETA = ZDateTime.Now.AddDays(8);

			var transportC22 = consol2.Transports.AddNew();
			transportC22.JW_ETD = ZDateTime.Now.AddDays(9);
			transportC22.JW_ETA = ZDateTime.Now.AddDays(10);

			var transportS2 = shipment.Transports.AddNew();
			transportS2.JW_ETD = ZDateTime.Now.AddDays(11);
			transportS2.JW_ETA = ZDateTime.Now.AddDays(12);

			const string departureConsolArrivalLeg =
				ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol + "," +
				ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival + " " + ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg;

			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS1, departureConsolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportC11, departureConsolArrivalLeg));
			AssertEquals(true, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportC12, departureConsolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportC21, departureConsolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportC22, departureConsolArrivalLeg));
			AssertEquals(false, WorkflowDescriptor.IsRelatedEntityInContext(shipment, transportS2, departureConsolArrivalLeg));
		}

		#endregion

		#endregion

		#region Lookups WorkflowTriggerFieldNames

		public void TestWorkflowTriggerFieldNames()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var trigger = shipment.WorkflowItems.Triggers.AddNew();

			AssertEquals("Trigger Condition List", "JW_RL_NKLoadPort, JW_RL_NKDiscPort, JW_Vessel, JW_VoyageFlight, JW_ETD, JW_ETA, JW_ATD, JW_ATA, JS_HouseBill, JS_E_ARV, JS_E_DEP, JK_MasterBillNum, JP_EstimatedDelivery, JP_EstimatedPickup, JE_EntryAuthorisationDate, JE_HouseBill, JE_VesselName, JE_VoyageFlightNo, JE_DateAtOrigin, JE_DateAtFinalDestination, JE_ExportDate, JE_DateOfArrival, JE_MasterBill, JE_EntrySubmittedDate, JE_WarehouseReleaseDate, JE_DateOfFirstArrival, JE_EntryDate, JE_RL_NKPortOfLoading, JE_RL_NKPortOfFirstArrival, JE_RL_NKPortOfArrival, JE_RL_NKFinalDestination, JE_LandedPieces, JE_TotalNoOfPacks, CH_BondAcquittedDate, CH_BondValidToDate", (new TriggerConditionsViewModel(trigger)).Lookups.WorkflowTriggerFieldNames.CodesAsString);
		}

		#endregion

		#region Helper Methods

		static IProcessor GetWorkFlowTriggerAction(ProcessTaskNotification action, QueuedLogForTesting queuedLog)
		{
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);

			return processor;
		}

		#endregion

		#region CO2

		protected override ProcessTaskTemplate CreateTaskTemplateForCO2eTests()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			return template;
		}

		protected override ICO2eCalculationSupporter CreateWithValidDataForCO2eTests()
		{
			return (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
		}

		protected override ICO2eCalculationSupporter CreateWithInvalidDataForCO2eTests()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			shipment.JS_ActualWeight = 0;
			shipment.JS_UniqueConsignRef = "S00001000";

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_TransportMode = "SEA";
			transport.JW_VoyageFlight = "VY1";

			return shipment;
		}

		#endregion

		#region CIN Export Notification

		public void TestGetWorkflowTriggerActionTypes_Include_CINExportNotification_ForTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();

			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				template.GlobalTemplate = false;
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should contain CIN action when Global Template is false", triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));

				template.GlobalTemplate = true;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CIN action when Global Template is true", !triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));
			}

			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				template.GlobalTemplate = false;
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CIN action when current company is not France", !triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));

				template.GlobalTemplate = true;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("Should not contain CIN action when current company is not France", !triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));
			}
		}

		public void TestGetWorkflowTriggerActionTypes_Include_CINExportNotification_ForShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportCodes.Air;
			shipment.JS_RL_NKLoadPort = "FR222";
			shipment.JS_RL_NKDischargePort = "HKHKG";

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();

			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain CIN action when current company is France", triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));
			}

			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should not contain CIN action when current company is not France", !triggerActionTypes.ContainsCode(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification));
			}
		}

		#endregion
	}
}
