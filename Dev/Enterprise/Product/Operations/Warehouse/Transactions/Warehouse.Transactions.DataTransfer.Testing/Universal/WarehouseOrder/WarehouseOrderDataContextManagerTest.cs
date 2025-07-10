using System;
using System.Collections.Generic;
using System.Linq;
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
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using DocAddressType = Enterprise.MasterFiles.Integration.DocAddressType;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseOrderDataContextManager))]
	sealed class WarehouseOrderDataContextManagerTest : WarehouseDocketDataContextManagerTestCase<WarehouseOrderDataContextManager, WhsOrder>
	{
		public void TestUseUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse.WW_WarehouseCode = "WHS";
				Factory.SaveForTesting();

				var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
				var universalShipmentWithUnmatchedOrganisations = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalShipmentWithUnmatchedOrganisations.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithUnmatchedOrganisations);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Warehouse Order from UniversalShipment.
Successfully saved Warehouse Order W00000001 with 1 x WhsDocketReference.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
No matching WhsOrder found, creating new WhsOrder.
Populating WhsOrder...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
Added Warehouse Order from UniversalShipment.
Successfully saved Warehouse Order W00000001 with 1 x WhsDocketReference.
".Trim(), logNoteText);
				});

				var reloadedOrder = Factory.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "ORDERME"));
				AssertNotNull("Warehouse Order should exist", reloadedOrder);

				var unmatchedOrgNotes = reloadedOrder.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
				var unmatchedOrgNote = unmatchedOrgNotes[0];
				AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @"
Organisation Type: WarehouseClient
Owner Code: 
EDI Code: 
Organisation Name: VAPOUR CORPORATION
Address Line 1: UNIT 0, -1 FANTASY LANE
Address Line 2: 
City: FAKE HILL
Post Code: 2987
State or Province: NSW
Country: AU
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: 
Organisation Name: SUBLIME CORPORATION
Address Line 1: UNIT 99, 42 ILLUSION LANE
Address Line 2: 
City: NOT REAL HILL
Post Code: 2987
State or Province: NSW
Country: AU
Doc Address Type: 
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);

				AssertEquals("reloadedOrder.WD_OH_Client", unmatchedOrgPK, reloadedOrder.WD_OH_Client);

				var docAddressTypesPresent = reloadedOrder.DocAddresses
					.OfType<JobDocAddress>()
					.Select(a => a.DocAddressType.ToString())
					.OrderBy(t => t)
					.ToArray();
				AssertEquals("docAddressTypesPresent should include 'Consignee', but should not include the 'Client' address or any other 'Extras' not present in the incoming XML", "ConsigneeAddress", string.Join("\r\n", docAddressTypesPresent));
			}
		}

		public void TestHavingNoOrderOnShipmentDoesNotThrowException()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsOrderToLoad = Factory.NewWithValidTestData<WhsOrder>();
				whsOrderToLoad.WD_DocketID = "W00001003";

				Factory.SaveForTesting();
				var universalShipmentWithDataTargetAndNoOrder = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalShipmentWithDataTargetAndNoOrder.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTargetAndNoOrder);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsOrder.
Populating WhsOrder...
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003.
".Trim(), logNoteText);
				});
			}
		}

		public void TestTryingToChangeReadOnlyFieldsGivesWarning()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
				var helper = new WhsTestHelperFunctions(Factory.BOFactory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.SaveForTesting();

				var whsOrderToLoad = helper.CreateWhsOrder(data.Org1, data.Whs1);
				whsOrderToLoad.WD_DocketID = "W00001003";
				whsOrderToLoad.WD_DocketSubType = "CUS";
				whsOrderToLoad.WD_WhsOrderFulfillmentRule = "ALL";
				whsOrderToLoad.WD_PickOption = "MAT";
				whsOrderToLoad.Warehouse.WW_WarehouseCode = "WSS";
				whsOrderToLoad.Warehouse.WW_WarehouseName = "CoolShack";
				whsOrderToLoad.Client.OH_Code = "CLI";
				whsOrderToLoad.Client.OH_FullName = "Client";
				var line = helper.CreateWhsOrderLine(whsOrderToLoad, data.Part1, 10m);
				var poke = line.WE_ShortfallQuantityCached; // to have no shortfall.

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(whsOrderToLoad);
				pick.AutoAllocateItemsWithMock();
				pick.FinaliseAllOrders();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(whsOrderToLoad);

				new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse.WW_WarehouseCode = "WHS";
				warehouse.WW_WarehouseName = "Coolhouse";

				Factory.SaveForTesting();

				var universalShipmentWithDataForReadOnlyFields = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalShipmentWithDataForReadOnlyFields.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataForReadOnlyFields);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsOrder.
Populating WhsOrder...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Client' [WD_OH_Client]. Cannot change 'CLI' (Client) to 'CRAHOLSYD' (CRACKERJACK HOLDINGS).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (Coolhouse).
Warning - Cannot update read-only Field 'Docket Sub Type' [WD_DocketSubType]. Cannot change 'CUS' (CUSTOMS RELEASE) to 'ORD' (ORDER).
Warning - Cannot update read-only Field 'Pick Option' [WD_PickOption]. Cannot change 'MAT' (Manual Pick With Auto-Allocate) to 'AUT' (Auto Pick).
Warning - Cannot update read-only Field 'Fulfillment Rule' [WD_WhsOrderFulfillmentRule]. Cannot change 'ALL' (All Lines in Full - No Shortfalls) to 'NON' (None).
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003.
".Trim(), logNoteText);

					var whsOrder = Factory.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
					AssertEquals("whsOrder.WD_DocketSubType", "CUS", whsOrder.WD_DocketSubType);
					AssertEquals("whsOrder.WD_WhsOrderFulfillmentRule", "ALL", whsOrder.WD_WhsOrderFulfillmentRule);
					AssertEquals("whsOrder.WD_PickOption", "MAT", whsOrder.WD_PickOption);
					AssertEquals("whsOrder.WD_WW_Whs", data.Whs1.PK, whsOrder.WD_WW_Whs);
					AssertEquals("whsOrder.WD_OH_Client", data.Org1.PK, whsOrder.WD_OH_Client);
					AssertEquals("whsOrder.PK", whsOrderToLoad.PK, whsOrder.PK);
				});
			}
		}

		public void TestRelationshipNotCreatedIfOrderLimitExceededOnShipment()
		{
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var shipment = (BusinessObject)Factory.BOFactory.New<Forwarding.IForwardingShipment>();
				shipment[JobShipmentSchema.JS_HouseBill] = "BACON PANCAKES";

				AttachOrderToShipment(shipment);
				AttachOrderToShipment(shipment);

				var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
				whsOrder.WD_BOLNo = "BACON PANCAKES";

				Factory.SaveForTesting();

				var logger = new TestLogger();

				var factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					var result = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[] { RecipientRoleType.FOR }, whsOrder);
					AssertEquals(AutoEvents.DataImportFailureCode, result[0].EventType);
				}
			}
		}

		public void TestRelationshipIsCreatedForAnImportedInternalJob()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_BOLNo = "HOUSE";
			order.WD_DocketID = "W001";

			Factory.SaveForTesting();

			var logger = new TestLogger();

			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[] { RecipientRoleType.FOR }, order);
				factory.Save();
			}

			AssertEquals(2, events.Length);

			var shipmentEvent = events[0];
			var linkedJobEvent = events[1];
			AssertEquals(Events.DataImportCode, shipmentEvent.EventType);
			AssertEquals(Events.JobsLinkedCode, linkedJobEvent.EventType);
			var dataSource1 = linkedJobEvent.GetMatchingDataSource(DataContextType.WarehouseOrder);
			AssertNotNull(dataSource1);
			AssertEquals("W001", dataSource1.Key);
			var dataSource2 = linkedJobEvent.GetMatchingDataSource(DataContextType.ForwardingShipment);
			AssertNotNull(dataSource2);
			AssertEquals("S00001000", dataSource2.Key);

			var relatedJobs = order.RelatedJobs;
			AssertEquals(1, relatedJobs.Count);

			var shipment = relatedJobs[0];
			AssertEquals(ControllerIDs.JobShipment, shipment.ControllerID);
			AssertEquals("Shipment", shipment.JobDescription);
			AssertEquals("S00001000", shipment.JobNumber);

			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;

			var newFactory = new BusinessObjectFactory();
			using (newFactory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(newFactory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[] { RecipientRoleType.FOR }, order);
				newFactory.Save();
			}

			var query = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, ((BusinessObject)shipment).PK);
			AssertEquals("Should not create a second pivot", 1, Factory.Load<WhsDocketJobPivot>(query).Length);
			AssertEquals("Should be updating same job so should not create different pivot", 1, Factory.Load<WhsDocketJobPivot>(new ZQuery()).Length);
		}

		public void TestImportWarehouseOrderThroughJobNumber()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsOrderToLoad = Factory.NewWithValidTestData<WhsOrder>();

				whsOrderToLoad.WD_DocketID = "W00001003";
				whsOrderToLoad.WD_ExternalReference = "DONTORDERME";

				Factory.SaveForTesting();
				var universalShipmentWithDataTarget = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalShipmentWithDataTarget.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTarget);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003 with 2 x WhsDocketReference.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsOrder.
Populating WhsOrder...
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
Updated Warehouse Order W00001003 from UniversalShipment.
Successfully saved Warehouse Order W00001003 with 2 x WhsDocketReference.
".Trim(), logNoteText);

					var whsOrder = new BusinessObjectFactory().LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
					AssertEquals("whsOrder.WD_ExternalReference", "ORDERME", whsOrder.WD_ExternalReference);
					AssertEquals("whsOrder.PK", whsOrderToLoad.PK, whsOrder.PK);
				});
			}
		}

		public void TestCanImportWarehouseOrderViaUniversalDataBuss()
		{
			using (Factory.BOFactory.AddDisposableService())
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
Added Warehouse Order from UniversalShipment.
Successfully saved Warehouse Order W00000001 with 2 x WhsDocketReference.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsOrder found, creating new WhsOrder.
Populating WhsOrder...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
Added Warehouse Order from UniversalShipment.
Successfully saved Warehouse Order W00000001 with 2 x WhsDocketReference.
".Trim(), logNoteText);

					var whsOrder = Factory.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "ORDERME"));
					AssertNotNull("Warehouse Order should exist", whsOrder);
				});
			}
		}

		public void TestCannotImportWarehouseOrderWithoutValidClientAddress()
		{
			using (Factory.BOFactory.AddDisposableService())
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
ERROR - Cannot Import Order
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
No matching WhsOrder found, creating new WhsOrder.
Populating WhsOrder...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way]'.
Error - Cannot Import Order
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
		}

		public void TestCannotImportWarehouseOrderWithoutValidWarehouse()
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
ERROR - Cannot Import Order
Unable to match Warehouse: WHS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
				".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsOrder found, creating new WhsOrder.
Populating WhsOrder...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot Import Order
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

			using (factory.AddDisposableService())
			{
				var whsOrder = factory.NewWithValidTestData<WhsOrder>();
				whsOrder.WD_DocketID = "W100110011";

				var trigger = whsOrder.WorkflowItems.AddNew();
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

				var logBO = whsOrder.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var dataContextManager = new WarehouseOrderDataContextManager() as IShipmentDataContextManager;

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, whsOrder, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(
					actionInfo
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => dataContextManager.GetShipmentDataObjectWriter(outboundSessionTracker)
					, whsOrder);

				processor.Process(logger);
				factory.Save();

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
					AssertEquals("message.EM_LinkUniqueID", whsOrder.PK, message.EM_LinkUniqueID);

					AssertIsXml("message.EM_MessageText", message.EM_MessageText)
						.HavingExactlyOneChildNode("Shipment/DataContext/ActionPurpose/Code",
							node => node.WithValue(action.PQ_MessagePurpose)
						).HavingExactlyOneChildNode("Shipment/DataContext/EventType/Code",
							node => node.WithValue(trigger.TriggerConditions.TriggerEventCode)
						);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestExportUniversalEventTriggerViaEHub()
		{
			var factory = new BusinessObjectFactory();

			using (factory.AddDisposableService())
			{
				var whsOrder = factory.NewWithValidTestData<WhsOrder>();
				whsOrder.WD_DocketID = "W00001000";
				whsOrder.WD_BOLNo = "BILL";
				whsOrder.WD_CustomerReference = "CUSTOMER";
				whsOrder.WD_ExternalReference = "ORDERME";
				whsOrder.WD_ExternalReferenceSplit = new ZByte(2);
				whsOrder.WD_TransportReference = "THIS23";

				var reference = whsOrder.References.AddNew();
				reference.WX_RefType = "CAN";
				reference.WX_Reference = "11334455";

				factory.Save();

				var trigger = whsOrder.WorkflowItems.AddNew();
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

				var logBO = whsOrder.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, whsOrder, Lazy.Create<IStmALog>(() => logBO));
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

				var interchange = factory.LoadTop1<XmlEDIInterchange>(new ZQuery());
				var messages = factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "WhsDocket", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", whsOrder.PK, message.EM_LinkUniqueID);

					AssertIsXml("message.EM_MessageText", message.EM_MessageText)
						.HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("OrderNumber")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(whsOrder.WD_ExternalReference)
							)
						).HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("ClientReference")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(whsOrder.WD_CustomerReference)
							)
						).HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("TransportReference")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(whsOrder.WD_TransportReference)
							)
						);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestImportUniversalEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
				whsOrder.WD_DocketID = "W00001000";

				var nonMatchingOrder = Factory.NewWithValidTestData<WhsOrder>();
				nonMatchingOrder.WD_DocketID = "W00001001";
				nonMatchingOrder.WD_BOLNo = "BILL";
				nonMatchingOrder.WD_CustomerReference = "CUSTOMER";
				nonMatchingOrder.WD_ExternalReference = "ORDERME";
				nonMatchingOrder.WD_ExternalReferenceSplit = new ZByte(2);
				nonMatchingOrder.WD_TransportReference = "THIS23";

				var reference = nonMatchingOrder.References.AddNew();
				reference.WX_RefType = "CAN";
				reference.WX_Reference = "11334455";

				Factory.SaveForTesting();

				var message = GetQueuedUniversalEventMessage(UniversalEvent);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Warehouse Order W00001000.
				".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Order W00001000.
				".Trim(), message.GetLogNoteText());

					var logs = whsOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
					AssertEquals("[BKD] - Booked event count", 1, logs.Length);
					var log = logs[0];

					var contextItems = log.SourceInfoItems;
					var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
					AssertMultilineASCIIEquals("Context Items on Event", @"
Order Number - ORDERME
Order Number Split - 2
Client Reference - CUSTOMER
Transport Reference - THIS23
House Bill - BILL
Customs Approval Number - 11334455
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
				});
			}
		}

		public void TestImportUniversalEventWithDataTargetDoesNotMatchUpToADocketOfDifferentType()
		{
			using (Factory.BOFactory.AddDisposableService())
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
		}

		public void TestImportUniversalEventWithContextInformationOnly()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
				whsOrder.WD_DocketID = "W00002000";
				whsOrder.WD_BOLNo = "BILL";
				whsOrder.WD_CustomerReference = "CUSTOMER";
				whsOrder.WD_ExternalReference = "ORDERME";
				whsOrder.WD_ExternalReferenceSplit = new ZByte(2);
				whsOrder.WD_TransportReference = "THIS23";

				var reference = whsOrder.References.AddNew();
				reference.WX_RefType = "CAN";
				reference.WX_Reference = "11334455";

				Factory.SaveForTesting();

				var message = GetQueuedUniversalEventMessage(UniversalEvent);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Warehouse Order W00002000.
				".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Order W00002000.
				".Trim(), message.GetLogNoteText());

					var logs = whsOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
					AssertEquals("[BKD] - Booked event count", 1, logs.Length);
					var log = logs[0];

					var contextItems = log.SourceInfoItems;
					var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
					AssertMultilineASCIIEquals("Context Items on Event", @"
Order Number - ORDERME
Order Number Split - 2
Client Reference - CUSTOMER
Transport Reference - THIS23
House Bill - BILL
Customs Approval Number - 11334455
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
				});
			}
		}

		public void TestContextInformationIsAllThere()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.WD_DocketID = "W00001000";
			whsOrder.WD_BOLNo = "BILL";
			whsOrder.WD_CustomerReference = "CUSTOMER";
			whsOrder.WD_ExternalReference = "ORDERME";
			whsOrder.WD_ExternalReferenceSplit = new ZByte(2);
			whsOrder.WD_TransportReference = "THIS23";

			var reference = whsOrder.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			var manager = (IEventDataContextManager)whsOrder.GetUniversalDataContextManager();

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value));

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
OrderNumber - ORDERME
OrderNumberSplit - 2
ClientReference - CUSTOMER
TransportReference - THIS23
House Bill - BILL
Customs Approval Number - 11334455
			".Trim(), eventContextValues);
		}

		public void TestImportUniversalEvent_ChangeOfInventory_ChangeOfOwnership()
		{
			AssertImportUniversalEvent_ChangeOfInventory(RecipientRoleType.BCO);
		}

		public void TestImportUniversalEvent_ChangeOfInventory_ChangeOfRegime()
		{
			AssertImportUniversalEvent_ChangeOfInventory(RecipientRoleType.BCR);
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
          <Type>WarehouseOrder</Type>
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

    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <WayBillNumber>FRED235478923</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <Order>
      <OrderNumber>ORDERME</OrderNumber>
      <OrderNumberSplit>1</OrderNumberSplit>
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

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new RecipientRoleType[] { RecipientRoleType.WAR, RecipientRoleType.BWR };

		protected override DataContextType ExpectedDataContextType => DataContextType.WarehouseOrder;

		protected override Type ExpectedDataObjectReaderType => typeof(WhsOrderDataObjectReader);

		protected override Type ExpectedDataObjectWriterType => typeof(WhsOrderDataObjectWriter);

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

		string UniversalShipment => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalShipment.xml");

		string UniversalEvent => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseOrder.TestFiles.UniversalEvent.xml");

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory));
		WhsTestHelperFunctions helper;

		void AttachOrderToShipment(BusinessObject shipment)
		{
			var order = (BusinessObject)Factory.BOFactory.New<Forwarding.IOrder>();
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = Factory.NewWithValidTestData<OrgAddress>().PK;
			order[JobOrderHeaderSchema.JD_JS] = shipment.PK;
		}

		void AssertImportUniversalEvent_ChangeOfInventory(RecipientRoleType changeOfInventoryRole)
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_CustomerReference = "Customer";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { changeOfInventoryRole }.ToRecipientRoleDetails() });
			universalEvent.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.ClientReference), Value = "Customer" } };

			var logger = new TestErrorLogger { TopLevelDataObject = universalEvent };
			var iManager = (IEventDataContextManager)new WarehouseOrderDataContextManager();
			AssertEquals("Should not find a parent because event is for Change of Inventory only.", 0, iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger).Length);

			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR }.ToRecipientRoleDetails() });
			AssertEquals("Should find a parent as per normal (event is NOT for Change of Inventory).", 1, iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger).Length);
		}
	}
}
