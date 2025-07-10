using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AirCargoWorkflowDescriptor))]
	sealed class AirCargoWorkflowDescriptorTest : WorkflowDescriptorTestCase<AirCargoWorkflowDescriptor>
	{
		public void TestDestinationDepotWithUnderbond()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				orgCusCode.OK_CustomsRegNo = "9999Z";
				orgCusCode.OK_OH = orgHeader.PK;

				var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

				var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
				underbond.C4_ParentID = cusMAWB.PK;
				underbond.C4_ParentTableCode = cusMAWB.TablePrefix;
				underbond.C4_MovementReason = "DCL"; //CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
				underbond.C4_DestinationPremiseID = "9999Z";

				var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

				cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Message Sent", 1, messages.Length);
				AssertEquals("Correct Destination", "DDP_DummyDestination", messages[0].Interchange.EI_To);
			}
		}

		public void TestARPWithResponsibleParty()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "ARP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				cusMAWB.CM_OH_ResponsibleParty = orgHeader.PK;
				var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

				var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

				cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Message Sent", 1, messages.Length);
				AssertEquals("Correct Destination", "ARP_DummyDestination", messages[0].Interchange.EI_To);
			}
		}

		public void TestARPWithResponsiblePartyID()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "ARP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				orgCusCode.OK_CustomsRegNo = "41065894724";
				orgCusCode.OK_OH = orgHeader.PK;

				var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				cusMAWB.CM_ResponsiblePartyID = "41065894724";
				var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

				var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

				cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Message Sent", 1, messages.Length);
				AssertEquals("Correct Destination", "ARP_DummyDestination", messages[0].Interchange.EI_To);
			}
		}

		public void TestCFSWithArrivalTransitWarehouse()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress.OA_Code = "SYDBB";

				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "CFS_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				cusMAWB.CM_OA_UnpackDepotAddress = orgAddress.PK;
				cusMAWB.UnpackDepotAddress.OA_OH = orgHeader.PK;
				var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

				var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
				notification.PQ_TriggerPartyService = ServiceCodesList.Codes.TransitWarehouseReceive;

				cusMAWB.Logs.AddNew(Events.BookingConfirmed);

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Precondition - No message has been sent yet.", 0, messages.Length);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var factoryAfterProcessingEvent = new BusinessObjectFactory() { RefreshEnabled = false };
				var messagesAfterProcessingEvent = factoryAfterProcessingEvent.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Message Sent", 1, messagesAfterProcessingEvent.Length);
				AssertEquals("Correct Destination", "CFS_DummyDestination", messagesAfterProcessingEvent[0].Interchange.EI_To);
			}
		}

		public void TestCFSWithArrivalTransitWarehouse_MissingCFSShouldNotThrowAnException()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "SYDBB";

			var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationMode.EK_Destination = "CFS_DummyDestination";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = WorkflowDescriptor.Code;

			var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
			var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

			var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
			notification.PQ_TriggerPartyService = ServiceCodesList.Codes.TransitWarehouseReceive;

			cusMAWB.Logs.AddNew(Events.BookingConfirmed);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
			AssertEquals("Precondition - No message has been sent yet.", 0, messages.Length);

			AssertNoExceptionThrown(() =>
			{
				cusMAWB.RunPreSaveValidation();
				Factory.Save();
			});
		}

		public void TestScheduleDeferredMessageSend()
		{
			var cusMAWB = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
			var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

			var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
			AssertEquals("triggerAction.PQ_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var messageSendLogQuery = new ZQuery(StmALogSchema.SL_Parent, cusMAWB.PK);
			messageSendLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DeferredScheduledMessageCode);
			var messageSendLogs = Factory.Load<StmALog>(messageSendLogQuery);
			AssertEquals("Precondition: Should be no DSM log against the CusMAWB.", 0, messageSendLogs.Length);

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			messageSendLogs = Factory.Load<StmALog>(messageSendLogQuery);
			AssertEquals("Should be a DSM log against the CusMAWB now the trigger has fired.", 1, messageSendLogs.Length);
		}

		public void TestSupportsValidateForCustomsMessagingTriggerAction()
		{
			var idx = 0;
			foreach (var countryCode in new[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.NewZealand })
			{
				idx++;
				var expectedNumberOfTriggerActions = countryCode == Core.Constants.CountryCodes.Australia ? 2 : 1;
				var supportedCountry = Factory.New<GlbCompany>();
				supportedCountry.GC_RN_NKCountryCode = countryCode;
				supportedCountry.GC_Code = countryCode + idx;
				supportedCountry.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				var supportedBranch = supportedCountry.Branches.AddNew();
				supportedBranch.GB_Code = countryCode + idx;
				supportedBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var usCompany = Factory.New<GlbCompany>();
				usCompany.GC_Code = "US" + idx;
				usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				var usBranch = usCompany.Branches.AddNew();
				usBranch.GB_Code = "US" + idx;
				usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				Factory.Save();
				AssertEquals(0, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(null, null).Count());

				var mawb = (Integration.Customs.Shared.ICusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				mawb.CM_GB = supportedBranch.PK;
				var provider = (IWorkflowProvider)mawb;
				var task = provider.WorkflowItems.Triggers.AddNew();
				task.P9_GC = supportedCountry.PK;
				AssertEquals(expectedNumberOfTriggerActions, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());
				Assert(WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Contains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
				mawb = Factory.New<Integration.Customs.NZ.ICusMAWB>();
				mawb.CM_GB = usBranch.PK;
				provider = (IWorkflowProvider)mawb;
				task = provider.WorkflowItems.Triggers.AddNew();
				task.P9_GC = usCompany.PK;
				AssertEquals(0, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());

				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_GC = supportedCountry.PK;
				task = template.WorkflowItems.Triggers.AddNew();
				AssertEquals("Template belongs to company with supported country", expectedNumberOfTriggerActions, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());

				template.GlobalTemplate = true;
				AssertEquals("Template is a Global Template should be unsupported unless DischargePortCountry is a supported country", 0, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());

				template.P0_DischargePortCountry = countryCode;
				AssertEquals("DischargePortCountry is a supported country", expectedNumberOfTriggerActions, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());
				Assert(WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Contains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));

				template.P0_DischargePortCountry = Core.Constants.CountryCodes.Niger;
				AssertEquals("DischargePortCountry is not supported", 0, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());

				template.P0_GB = supportedBranch.PK;
				AssertEquals("Template belongs to a branch with a company with supported country", expectedNumberOfTriggerActions, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());
				Assert(WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Contains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));

				template.P0_GB = usBranch.PK;
				AssertEquals("Template belongs to a branch with a company with an unsupported country", 0, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, task.GetJob()).Count());
			}
		}

		public override void TestWorkflowProviderType()
		{
			AssertWorkflowProviderType(Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusMAWB>());
			AssertWorkflowProviderType(Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>());
			AssertWorkflowProviderType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusMAWB>());

			AssertWorkflowProviderType(Core.Constants.CountryCodes.SouthAfrica, typeof(CusMAWB));
		}

		void AssertWorkflowProviderType(string countryCode, Type expectedMAWBType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var provider = (IWorkflowProvider)Factory.New(expectedMAWBType);
				Assert("IsAssignable", WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(expectedMAWBType));
				AssertEquals("Code", WorkflowDescriptor.Code, provider.WorkflowType);
			}
		}

		public void TestControllerID()
		{
			AssertControllerID(Core.Constants.CountryCodes.Australia, ControllerIDs.Customs.AU.AirCargo);
			AssertControllerID(Core.Constants.CountryCodes.NewZealand, ControllerIDs.Customs.NZ.ExpressECI);
			AssertControllerID(Core.Constants.CountryCodes.UnitedKingdom, ControllerIDs.Customs.GB.CcsukAirInventory);

			AssertControllerID(Core.Constants.CountryCodes.SouthAfrica, ControllerIDs.Customs.BaseAirCargo);
		}

		void AssertControllerID(string countryCode, ControllerID expectedControllerID)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AssertEquals("ControllerID", expectedControllerID, WorkflowDescriptor.ControllerID);
			}
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawb = Factory.New<CusMAWB>();
				var task = ((IWorkflowProvider)mawb).WorkflowItems.Triggers.AddNew();
				AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(task, mawb));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var mawb = Factory.New<CusMAWB>();
				var task = ((IWorkflowProvider)mawb).WorkflowItems.Triggers.AddNew();
				AssertEquals(false, WorkflowDescriptor.SupportsSetFieldTriggerAction(task, mawb));
			}
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.CusMAWB.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.CusMAWB.Description, WorkflowDescriptor.Description);
		}

		[StressTest] // AirlineList loads many RefAirline objects
		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Airline", "Airline", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertEquals(false, WorkflowDescriptor.SubTypeInformation[0].List.ContainsCode(""));
		}

		[StressTest] // AirlineList loads many RefAirline objects
		public new void TestArrayPropertiesDoNotReturnNull()
		{
			base.TestArrayPropertiesDoNotReturnNull();
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CusMAWB, WorkflowDescriptor.DocumentBusinessContext[0]);
			AssertEquals(BusinessContext.CusHAWB, WorkflowDescriptor.DocumentBusinessContext[1]);
		}

		public void TestMessageRecipientPartiesForDifferentCountries()
		{
			AssertEquals(MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.ArrivalTransitWarehouse, WorkflowDescriptor.SupportedMessageRecipientParties(null, null));

			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_Code = "AUC"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "AUB"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			Factory.Save();
			var mawb = (Integration.Customs.Shared.ICusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
			mawb.CM_GB = auBranch.PK;
			var provider = (IWorkflowProvider)mawb;
			var task = provider.WorkflowItems.Triggers.AddNew();
			task.P9_GC = auCompany.PK;
			AssertEquals(expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.DeConsolidator |
									 MessageRecipientPartyType.AirCargoResponsibleParty |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, mawb));

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "USC"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			Factory.Save();
			mawb = Factory.New<Integration.Customs.NZ.ICusMAWB>();
			provider = (IWorkflowProvider)mawb;
			task = provider.WorkflowItems.Triggers.AddNew();
			task.P9_GC = usCompany.PK;
			mawb.CM_GB = usBranch.PK;
			AssertEquals(expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, mawb));

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			task = template.WorkflowItems.Triggers.AddNew();
			AssertEquals("Template belongs to company with supported country",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.DeConsolidator |
									 MessageRecipientPartyType.AirCargoResponsibleParty |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));

			template.GlobalTemplate = true;
			AssertEquals("Template is a Global Template should be unsupported unless DischargePortCountry is a supported country",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));

			template.P0_DischargePortCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("DischargePortCountry is a supported country",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.DeConsolidator |
									 MessageRecipientPartyType.AirCargoResponsibleParty |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));

			template.P0_DischargePortCountry = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("DischargePortCountry is not supported",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));

			template.P0_GB = auBranch.PK;
			AssertEquals("Template belongs to a branch with a company with supported country",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.DeConsolidator |
									 MessageRecipientPartyType.AirCargoResponsibleParty |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));

			template.P0_GB = usBranch.PK;
			AssertEquals("Template belongs to a branch with a company with an unsupported country",
						 expected: MessageRecipientPartyType.Email |
									 MessageRecipientPartyType.OrgProxy |
									 MessageRecipientPartyType.ArrivalTransitWarehouse,
						 actual: WorkflowDescriptor.SupportedMessageRecipientParties(task, template));
		}

		public void TestAdditionalWorkflowTriggerActionTypeListForDifferentCountries()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(null, null));
			AssertEquals(false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals(false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			auCompany.GC_Code = "AUC"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "AUB"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			Factory.Save();
			var mawb = (Integration.Customs.Shared.ICusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
			mawb.CM_GB = auBranch.PK;
			var provider = (IWorkflowProvider)mawb;
			var task = provider.WorkflowItems.Triggers.AddNew();
			task.P9_GC = auCompany.PK;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, mawb));
			AssertEquals(true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals(true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "USC"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB"; // Need to set unique codes for auCompany and usCompany, otherwise Factory.Save() will fail for usCompany
			Factory.Save();
			mawb = Factory.New<Integration.Customs.NZ.ICusMAWB>();
			provider = (IWorkflowProvider)mawb;
			task = provider.WorkflowItems.Triggers.AddNew();
			task.P9_GC = usCompany.PK;
			mawb.CM_GB = usBranch.PK;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, mawb));
			AssertEquals(false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals(false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			task = template.WorkflowItems.Triggers.AddNew();
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("Template belongs to company with supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("Template belongs to company with supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			template.GlobalTemplate = true;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("Template is a Global Template should be unsupported unless DischargePortCountry is a supported country", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("Template is a Global Template should be unsupported unless DischargePortCountry is a supported country", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			template.P0_DischargePortCountry = Core.Constants.CountryCodes.Australia;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("DischargePortCountry is a supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("DischargePortCountry is a supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			template.P0_DischargePortCountry = Core.Constants.CountryCodes.NewZealand;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("DischargePortCountry is not supported", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("DischargePortCountry is not supported", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			template.P0_GB = auBranch.PK;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("Template belongs to a branch with a company with supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("Template belongs to a branch with a company with supported country", true, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));

			template.P0_GB = usBranch.PK;
			list = new CodeDescriptionPairList();
			list.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, template));
			AssertEquals("Template belongs to a branch with a company with an unsupported country", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage));
			AssertEquals("Template belongs to a branch with a company with an unsupported country", false, list.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage));
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Email |
						 MessageRecipientPartyType.OrgProxy |
						 MessageRecipientPartyType.DeConsolidator |
						 MessageRecipientPartyType.AirCargoResponsibleParty |
						 MessageRecipientPartyType.ArrivalTransitWarehouse;
			}
		}

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			var mawb = Factory.New<Integration.Customs.AU.ICusMAWB>();
			mawb.CM_GB = auBranch.PK;
			var provider = (IWorkflowProvider)mawb;

			JobHeader.Loader jobLoader = new JobHeader.Loader((IJobHeaderParent)provider);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			return new[] { provider };
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			switch (partyTypeCode)
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					{
						var mawb = workflowProvider as CusMAWB;
						var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
						underbond.C4_ParentID = mawb.PK;
						underbond.C4_ParentTableCode = mawb.TablePrefix;
						underbond.C4_MovementReason = "DCL"; //CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
						underbond.C4_DestinationPremiseID = "9999Z";

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
						orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "9999Z";
						orgCusCode.OK_OH = orgHeader.PK;
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					{
						var mawb = workflowProvider as CusMAWB;
						mawb.CM_ResponsiblePartyID = "41065894724";

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
						orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "41065894724";
						orgCusCode.OK_OH = orgHeader.PK;
					}
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					{
						var mawb = workflowProvider as CusMAWB;

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var orgMode = orgHeader.EDICommunicationsModes.AddNew();
						orgMode.EK_Module = WorkflowDescriptor.Code;
						orgMode.EK_FileFormat = "NTF";
						orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						orgMode.EK_Destination = "@notificationemail.cargowise.com";

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
						orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "41065894724";
						orgCusCode.OK_OH = orgHeader.PK;

						mawb.CM_OA_UnpackDepotAddress = orgHeader.MainAddress.PK;
					}
					break;
			}
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Australia;
	}
}
