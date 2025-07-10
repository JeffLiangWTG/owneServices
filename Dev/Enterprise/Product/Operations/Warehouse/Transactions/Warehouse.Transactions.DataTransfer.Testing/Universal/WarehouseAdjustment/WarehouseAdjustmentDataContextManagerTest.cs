using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using DocAddressType = Enterprise.MasterFiles.Integration.DocAddressType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseAdjustmentDataContextManager))]
	sealed class WarehouseAdjustmentDataContextManagerTest : WarehouseDocketDataContextManagerTestCase<WarehouseAdjustmentDataContextManager, WhsAdjustment>
	{
		public void TestHavingNoOrderOnShipmentDoesNotThrowException()
		{
			var whsAdjustmentToLoad = Factory.NewWithValidTestData<WhsAdjustment>();
			whsAdjustmentToLoad.WD_DocketID = "W00001003";

			Factory.SaveForTesting();

			var universalShipmentWithDataTargetAndNoOrder = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseAdjustment.TestFiles.UniversalShipmentWithDataTargetAndNoOrder.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTargetAndNoOrder);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsAdjustment.
Populating WhsAdjustment...
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), logNoteText);
			});
		}

		public void TestTryingToChangeReadOnlyFieldsGivesWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var helper = new WhsTestHelperFunctions(Factory.BOFactory);
			Factory.SaveForTesting();
			var whsAdjustmentToLoad = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			whsAdjustmentToLoad.WD_DocketID = "W00001003";
			whsAdjustmentToLoad.WD_ExternalReference = "ADJUSTME";
			whsAdjustmentToLoad.IsUniqueExternalReferenceCreatedOnSave = false;
			whsAdjustmentToLoad.Warehouse.WW_WarehouseCode = "WSS";
			whsAdjustmentToLoad.Warehouse.WW_WarehouseName = "CoolShack";
			whsAdjustmentToLoad.Client.OH_Code = "CLI";
			whsAdjustmentToLoad.Client.OH_FullName = "Client";
			helper.CreateWhsAdjustmentLine(whsAdjustmentToLoad, data.Part1, 5m, data.Whs1.DefaultLocation);
			whsAdjustmentToLoad.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(whsAdjustmentToLoad);

			new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Coolhouse";

			Factory.SaveForTesting();

			var universalShipmentWithDataForReadOnlyFields = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseAdjustment.TestFiles.UniversalShipmentWithDataForReadOnlyFields.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataForReadOnlyFields);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsAdjustment.
Populating WhsAdjustment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Client' [WD_OH_Client]. Cannot change 'CLI' (Client) to 'CRAHOLSYD' (CRACKERJACK HOLDINGS).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (Coolhouse).
Warning - Cannot update read-only Field 'External Reference' [WD_ExternalReference]. Cannot change 'ADJUSTME' to 'ORDERME'.
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), logNoteText);

				var whsAdjustment = Factory.LoadTop1<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
				AssertEquals("whsOrder.WD_ExternalReference", "ADJUSTME", whsAdjustment.WD_ExternalReference);
				AssertEquals("whsOrder.WD_WW_Whs", data.Whs1.PK, whsAdjustment.WD_WW_Whs);
				AssertEquals("whsOrder.WD_OH_Client", data.Org1.PK, whsAdjustment.WD_OH_Client);
				AssertEquals("whsOrder.PK", whsAdjustmentToLoad.PK, whsAdjustment.PK);
			});
		}

		public void TestImportWarehouseAdjustmentThroughJobNumber()
		{
			var whsAdjustmentToLoad = Factory.NewWithValidTestData<WhsAdjustment>();

			whsAdjustmentToLoad.WD_DocketID = "W00001003";
			whsAdjustmentToLoad.WD_ExternalReference = "DONTORDERME";

			Factory.SaveForTesting();
			whsAdjustmentToLoad.WD_ExternalReference = "DONTORDERME";
			Factory.SaveForTesting();

			var universalShipmentWithDataTarget = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseAdjustment.TestFiles.UniversalShipmentWithDataTarget.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTarget);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsAdjustment.
Populating WhsAdjustment...
Updated Warehouse Adjustment W00001003 from UniversalShipment.
Successfully saved Warehouse Adjustment W00001003.
".Trim(), logNoteText);

				var whsAdjustment = new BusinessObjectFactory().LoadTop1<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
				AssertEquals("whsAdjustment.WD_ExternalReference", "ORDERME", whsAdjustment.WD_ExternalReference);
				AssertEquals("whsAdjustment.PK", whsAdjustmentToLoad.PK, whsAdjustment.PK);
			});
		}

		public void TestCanImportWarehouseAdjustmentViaUniversalDataBuss()
		{
			var addressData = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var clientAddress = new OrganisationDataObjectReader(addressData, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			clientAddress.Header.OH_IsWarehouseClient = true;
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Warehouse Adjustment from UniversalShipment.
Successfully saved Warehouse Adjustment W00000001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsAdjustment found, creating new WhsAdjustment.
Populating WhsAdjustment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Added Warehouse Adjustment from UniversalShipment.
Successfully saved Warehouse Adjustment W00000001.
".Trim(), logNoteText);

				var whsOrder = Factory.LoadTop1<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "ORDERME"));
				AssertNotNull("Warehouse Adjustment should exist", whsOrder);
			});
		}

		public void TestCannotImportWarehouseAdjustmentWithoutValidClientAddress()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot Import Adjustment
Unable to match Client Address, please make sure the supplied Client Address is valid. Details were:
Address1: 1804 Fudrucker Way
CompanyName: CRACKERJACK HOLDINGS
OrganizationCode: CRAHOLSYD
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
				".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way]'.
No matching WhsAdjustment found, creating new WhsAdjustment.
Populating WhsAdjustment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way]'.
Error - Cannot Import Adjustment
Unable to match Client Address, please make sure the supplied Client Address is valid. Details were:
Address1: 1804 Fudrucker Way
CompanyName: CRACKERJACK HOLDINGS
OrganizationCode: CRAHOLSYD
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);
			});
		}

		public void TestCannotImportWarehouseAdjustmentWithoutValidWarehouse()
		{
			var addressData = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var clientAddress = new OrganisationDataObjectReader(addressData, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			clientAddress.Header.OH_IsWarehouseClient = true;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot Import Adjustment
Unable to match Warehouse: WHS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
				".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsAdjustment found, creating new WhsAdjustment.
Populating WhsAdjustment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot Import Adjustment
Unable to match Warehouse: WHS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);
			});
		}

		public void TestExportUniversalShipmentTrigger()
		{
			var factory = new BusinessObjectFactory();

			var whsAdjustment = factory.NewWithValidTestData<WhsAdjustment>();
			whsAdjustment.WD_DocketID = "W100110011";

			var trigger = whsAdjustment.WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			var communicationsMode = factory.New<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "9CHARCODE";

			factory.Save();

			var logBO = whsAdjustment.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			var dataContextManager = new WarehouseAdjustmentDataContextManager() as IShipmentDataContextManager;

			var logger = new TestLogger();
			var actionInfo = new ActionWrapper(action, whsAdjustment, Lazy.Create<IStmALog>(() => logBO));
			var processor = new UniversalXmlWorkflowProcessor(
				actionInfo
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
				, (outboundSessionTracker) => dataContextManager.GetShipmentDataObjectWriter(outboundSessionTracker)
				, whsAdjustment);

			using (factory.AddDisposableService())
			{
				processor.Process(logger);
				factory.Save();
			}

			AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
				, @"".Trim()
				, logger.GetAllLogsAsString());

			var messages = factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

				AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
				AssertEquals("message.EM_LinkTable", "WhsDocket", message.EM_LinkTable);
				AssertEquals("message.EM_LinkUniqueID", whsAdjustment.PK, message.EM_LinkUniqueID);

				var adjustmentDataObject = new UniversalShipment();
				var reader = ObjectFactory.Get<IXmlReader>();
				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(message.EM_MessageText)))
				{
					reader.ReadXML(adjustmentDataObject, stream, new TestErrorLogger());
				}

				var dataSource = adjustmentDataObject.DataContext.GetMatchingDataSource(DataContextType.WarehouseAdjustment);
				AssertEquals("Correct Docket ID.", "W100110011", dataSource.Key);
				AssertEquals("Correct Purpose Code.", "APP", adjustmentDataObject.DataContext.ActionPurposeCode);

				AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
			});
		}

		public void TestExportUniversalEventTriggerViaEHub()
		{
			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var whsAdjustment = factory.NewWithValidTestData<WhsAdjustment>();
				whsAdjustment.WD_DocketID = "W00001000";
				whsAdjustment.WD_ExternalReference = "ADJUSTME";

				factory.Save();

				var trigger = whsAdjustment.WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = whsAdjustment.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, whsAdjustment, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(
					actionInfo
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				processor.Process(logger);
				factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "WhsDocket", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", whsAdjustment.PK, message.EM_LinkUniqueID);

					AssertIsXml("message.EM_MessageText", message.EM_MessageText)
						.HavingExactlyOneChildNode("Event/ContextCollection/Context/Value",
							node => node.WithValue(whsAdjustment.WD_ExternalReference)
						).HavingExactlyOneChildNode("Event/DataContext/EventType/Code",
							node => node.WithValue(trigger.TriggerConditions.TriggerEventCode)
						);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestImportUniversalEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var whsAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			whsAdjustment.WD_DocketID = "W00001000";

			var nonMatchingAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			nonMatchingAdjustment.WD_DocketID = "W00001001";
			nonMatchingAdjustment.WD_ExternalReference = "ADJUSTME";

			Factory.SaveForTesting();
			nonMatchingAdjustment.WD_ExternalReference = "ADJUSTME";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Warehouse Adjustment W00001000.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Adjustment W00001000.
				".Trim(), message.GetLogNoteText());

				var logs = whsAdjustment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Adjustment Reference - ADJUSTME
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		public void TestImportUniversalEventWithDataTargetDoesNotMatchUpToADocketOfDifferentType()
		{
			var whsReceive = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_DocketID = "W00001000";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
				".Trim(), message.GetLogNoteText());

				var logs = whsReceive.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Authorized event count", 0, logs.Length);
			});
		}

		public void TestImportUniversalEventWithContextInformationOnly()
		{
			var whsAdjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			whsAdjustment.WD_DocketID = "W00002000";
			whsAdjustment.WD_ExternalReference = "ADJUSTME";

			Factory.SaveForTesting();
			whsAdjustment.WD_ExternalReference = "ADJUSTME";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Warehouse Adjustment W00002000.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Adjustment W00002000.
				".Trim(), message.GetLogNoteText());

				var logs = whsAdjustment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Adjustment Reference - ADJUSTME
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		public void TestContextInformationIsAllThere()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketID = "W00001000";
			adjustment.WD_ExternalReference = "ADJUSTME";
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;

			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation, "HEL");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);

			adjustmentLine.Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "DAM"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, "HEL"));
			Factory.SaveForTesting();

			var manager = (IEventDataContextManagerWithTriggeringLog)adjustment.GetUniversalDataContextManager();
			var adjustmentLog = adjustment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = adjustmentLog;
			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value));

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
AdjustmentReference - ADJUSTME
InventoryWithChangedHoldCode - 10x P1 changed from 'DAM' to 'HEL'
			".Trim(), eventContextValues);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			var addressData = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var clientAddress = new OrganisationDataObjectReader(addressData, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			clientAddress.Header.OH_IsWarehouseClient = true;

			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WarehouseAdjustment</Type>
          <Key>W00001003</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorised</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDate>2011-03-27T11:13:00</TriggerDate>
      <TriggerDescription>Test Trigger</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <Order>
      <OrderNumber>ORDERME</OrderNumber>
      <Warehouse>
        <Code>WHS</Code>
      </Warehouse>
    </Order>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <OrganizationCode>CRAHOLSYD</OrganizationCode>
        <CompanyName>CRACKERJACK HOLDINGS</CompanyName>
        <Address1>1804 Fudrucker Way</Address1>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override DataContextType ExpectedDataContextType => DataContextType.WarehouseAdjustment;

		protected override Type ExpectedDataObjectReaderType => typeof(WhsAdjustmentDataObjectReader);

		protected override Type ExpectedDataObjectWriterType => typeof(WhsAdjustmentDataObjectWriter);

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		string UniversalShipment => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseAdjustment.TestFiles.UniversalShipment.xml");

		string UniversalEvent => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseAdjustment.TestFiles.UniversalEvent.xml");

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory));
		WhsTestHelperFunctions helper;
	}
}
