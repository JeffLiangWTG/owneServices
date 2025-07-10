using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentWorkflowTriggerTest : TestCaseWithFactory
	{
		public void TestCorrectConsolIsExported_XUS()
		{
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Japan");
			RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				triggerAction.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;
			});

			var message = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(message.Length, 1);
		}

		public void TestSetField()
		{
			var (shipment, a) = RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				triggerAction.PQ_FieldName = "<JS_AdditionalTerms>";
				triggerAction.PQ_FieldValue = "Jenk";
			});

			shipment.Reload();
			AssertEquals("Jenk", shipment.JS_AdditionalTerms);
		}

		public void TestMail()
		{
			RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				triggerAction.PQ_Calc_TriggerParty = "EML";
				triggerAction.PQ_EmailAddr = "big@bog.com";
			});
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestVCM()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var (_, log) = RunLWKWithMultipleConsols(trigger =>
				{
					var triggerAction = trigger.ProcessTaskNotifications.AddNew();
					triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
					triggerAction.PQ_Calc_TriggerParty = "EML";
					triggerAction.PQ_EmailAddr = "big@bog.com";
				});
				AssertContains("Could not find a Parent BO for Trigger Action", log);
			}
		}

		public void TestAVS()
		{
			var (_, log) = RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
			});
			AssertContains("No declaration was found for the shipment, so could not submit AVS query.", log);
		}

		public void TestCOS()
		{
			var (_, log) = RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts;
			});
			AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", log);
		}

		public void TestB3()
		{
			var (_, log) = RunLWKWithMultipleConsols(trigger =>
			{
				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			});
			AssertContains("No declaration was found for the shipment, so could not create the Schedule CAD message.", log);
		}

		(ForwardingShipment, string) RunLWKWithMultipleConsols(Action<ProcessTask> withTask)
		{
			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var communicationMode = orgProxy.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Destination = "boop";
			communicationMode.EK_Module = "SHP";

			var shipment = Factory.New<ForwardingShipment>();

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL1";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CONSOL2";

			var transport_consol1 = consol1.Transports.AddNew();
			transport_consol1.JW_RL_NKLoadPort = "NZWLG";
			transport_consol1.JW_RL_NKDiscPort = "ECARE";

			var transport_consol2 = consol2.Transports.AddNew();
			transport_consol2.JW_RL_NKLoadPort = "ECARE";
			transport_consol2.JW_RL_NKDiscPort = "REANN";

			var trigger = shipment.WorkflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.TriggerConditions.TriggerConditionValue = "LOC=<LastLeg.Destination>,FAC=CTO";

			withTask(trigger);

			Factory.Save();

			var logs = shipment.GetLogs();
			logs.AddNew(Events.Arrival, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "REANN"), new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, "CTO"));
			Factory.Save();

			return (shipment, ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker());
		}

		public void TestCorrectConsolIsExportedFromSubShipment_EventAddedToASMLinkedConsol()
		{
			RunLWKForSubShipments(shouldUseConsolOnASM: true);

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("We want to include the ASM in this export", "ASM_SHIPMENT", messages[0].EM_MessageText, true);
			AssertContains("DataSource needs to contain Consol 1",
@"        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>CONSOL1</Key>
        </DataSource>",
				messages[0].EM_MessageText);
		}

		public void TestCorrectConsolIsExportedFromSubShipment_EventAddedToSubShipmentLinkedConsol()
		{
			RunLWKForSubShipments(shouldUseConsolOnASM: false);

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertNotContains("We don't want to include the ASM in this export", "ASM_SHIPMENT", messages[0].EM_MessageText, true);
			AssertContains("DataSource needs to contain Consol 2",
@"        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>CONSOL2</Key>
        </DataSource>",
	messages[0].EM_MessageText);
		}

		void RunLWKForSubShipments(bool shouldUseConsolOnASM)
		{
			var asmShipment = Factory.New<ForwardingShipment>();
			asmShipment.JS_TransportMode = "AIR";
			asmShipment.JS_UniqueConsignRef = "ASM_SHIPMENT";

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = asmShipment.PK;
			subShipment.JS_TransportMode = "ROA";
			subShipment.JS_UniqueConsignRef = "SUB_SHIPMENT";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "CONSOL1";
			asmShipment.Consols.Add(consol1);
			subShipment.Consols.Add(consol1);

			var consol2 = subShipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CONSOL2";

			AssertEquals("PreCondition: ASM has just 1 consol", 1, asmShipment.Consols.Count);
			AssertEquals("PreCondition: subShipment has both consols attached", 2, subShipment.Consols.Count);

			var trigger = subShipment.WorkflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.GateOutCode;
			trigger.P9_RespondToCascadedEvents = true;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			triggerAction.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var communicationMode = orgProxy.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Destination = "boop";
			communicationMode.EK_Module = "SHP";

			Factory.Save();

			var logs = shouldUseConsolOnASM ? consol1.GetLogs() : consol2.GetLogs();
			logs.AddNew(Events.GateOut);
			Factory.Save();

			var message = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
		}
	}
}
