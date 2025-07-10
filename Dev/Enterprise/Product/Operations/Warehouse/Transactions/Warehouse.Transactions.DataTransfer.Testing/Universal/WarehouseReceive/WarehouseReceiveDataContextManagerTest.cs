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
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
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
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WarehouseReceiveDataContextManager))]
	sealed class WarehouseReceiveDataContextManagerTest : WarehouseDocketDataContextManagerTestCase<WarehouseReceiveDataContextManager, WhsReceive>
	{
		public void TestHavingNoOrderOnShipmentDoesNotThrowException()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsReceiveToLoad = Factory.NewWithValidTestData<WhsReceive>();
				whsReceiveToLoad.WD_DocketID = "W00001003";

				Factory.SaveForTesting();

				var universalShipmentWithDataTargetAndNoOrder = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseReceive.TestFiles.UniversalShipmentWithDataTargetAndNoOrder.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTargetAndNoOrder);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003.
".Trim(), logNoteText);
				});
			}
		}

		public void TestTryingToChangeReadOnlyFieldsGivesWarning()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
				var whsReceiveToLoad = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				whsReceiveToLoad.WD_DocketID = "W00001003";
				whsReceiveToLoad.WD_DocketSubType = "CUS";
				whsReceiveToLoad.WD_ArrivalDate = new ZDateTimeOffset(2011, 1, 1);
				whsReceiveToLoad.WD_BookingDate = new ZDateTimeOffset(2011, 1, 2);
				whsReceiveToLoad.Client.OH_Code = "CLI";
				whsReceiveToLoad.Client.OH_FullName = "Client";
				whsReceiveToLoad.Warehouse.WW_WarehouseCode = "WSS";
				whsReceiveToLoad.Warehouse.WW_WarehouseName = "CoolShack";

				new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse.WW_WarehouseCode = "WHS";
				warehouse.WW_WarehouseName = "Coolhouse";

				Factory.SaveForTesting();

				var universalShipmentWithDataForReadOnlyFields = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseReceive.TestFiles.UniversalShipmentWithDataForReadOnlyFields.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataForReadOnlyFields);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var readerHelper = new WhsDataObjectReaderHelper(warehouse);
				var expectedOffset = readerHelper.ConvertToZDateTimeOffset(new ZDateTime(2011, 3, 3)).Value;

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", $@"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Warning - Cannot update read-only Field 'Client' [WD_OH_Client]. Cannot change 'CLI' (Client) to 'CRAHOLSYD' (CRACKERJACK HOLDINGS).
Warning - Cannot update read-only Field 'Warehouse' [WD_WW_Whs]. Cannot change 'WSS' (CoolShack) to 'WHS' (Coolhouse).
Warning - Cannot update read-only Field 'Docket Sub Type' [WD_DocketSubType]. Cannot change 'CUS' (CUSTOMS RECEIPT) to 'REC' (GOODS RECEIPT).
Warning - Cannot update read-only Field 'Booking Date' [WD_BookingDate]. Cannot change '02-Jan-11 00:00:00 +10:00' to '{expectedOffset.ToString("dd-MMM-yy 00:00:00 zzz")}'.
Warning - Cannot update read-only Field 'Arrival Date' [WD_ArrivalDate]. Cannot change '01-Jan-11 00:00:00 +10:00' to '{expectedOffset.ToString("dd-MMM-yy 00:00:00 zzz")}'.
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003.
".Trim(), logNoteText);

					var whsReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
					AssertEquals("whsOrder.WD_DocketSubType", "CUS", whsReceive.WD_DocketSubType);
					AssertEquals("whsOrder.WD_ArrivalDate", new ZDateTimeOffset(2011, 1, 1), whsReceive.WD_ArrivalDate);
					AssertEquals("whsOrder.WD_BookingDate", new ZDateTimeOffset(2011, 1, 2), whsReceive.WD_BookingDate);
					AssertEquals("whsOrder.WD_WW_Whs", data.Whs1.PK, whsReceive.WD_WW_Whs);
					AssertEquals("whsOrder.WD_OH_Client", data.Org1.PK, whsReceive.WD_OH_Client);
					AssertEquals("whsOrder.PK", whsReceiveToLoad.PK, whsReceive.PK);
				});
			}
		}

		public void TestRelationshipIsCreatedForAnImportedInternalJob()
		{
			var warehouseClient = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouseClient.OH_IsWarehouseClient = true;

			var forwardingOrder = (BusinessObject)Factory.BOFactory.New<Forwarding.IOrder>();
			forwardingOrder.FillWithValidTestData();
			forwardingOrder[JobOrderHeaderSchema.JD_OrderNumber] = "ORDERME";
			forwardingOrder[JobOrderHeaderSchema.JD_OA_BuyerAddress] = warehouseClient.MainAddress.PK;

			var warehouseAddress = ((IDocAddresses)forwardingOrder).DocAddresses.AddNew(DocAddressType.Warehouse);
			warehouseAddress.E2_OA_Address = warehouse.WW_OA_WarehouseAddress;

			Factory.SaveForTesting();

			var logger = new TestLogger();

			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new[] { RecipientRoleType.WIN }, (IWorkflowProvider)forwardingOrder);
				factory.Save();
			}
			AssertEquals(2, events.Length);

			var receiveEvent = events[0];
			var linkedJobEvent = events[1];
			AssertEquals(Events.DataImportCode, receiveEvent.EventType);
			AssertEquals(Events.JobsLinkedCode, linkedJobEvent.EventType);
			var dataSource1 = linkedJobEvent.GetMatchingDataSource(DataContextType.WarehouseReceive);
			AssertNotNull(dataSource1);
			AssertEquals("W00000001", dataSource1.Key);
			var dataSource2 = linkedJobEvent.GetMatchingDataSource(DataContextType.OrderManagerOrder);
			AssertNotNull(dataSource2);
			AssertEquals("ORDERME~0~WHTEST", dataSource2.Key);

			var receive = (WhsReceive)PublishToUniversalResult.New(events, DataContextType.WarehouseReceive, "").FindJobIfExists();
			var relatedJobs = receive.RelatedJobs;
			AssertEquals(1, relatedJobs.Count);

			var forwardingOrderFromRelatedJobs = (BusinessObject)relatedJobs[0];
			AssertEquals(forwardingOrder.PK, forwardingOrderFromRelatedJobs.PK);

			var newFactory = new BusinessObjectFactory();
			using (newFactory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(newFactory, GlbCompany.CurrentCompany.OrgProxy, new[] { RecipientRoleType.WIN }, (IWorkflowProvider)forwardingOrder);
				newFactory.Save();
			}
			var query = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, receive.PK);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, forwardingOrderFromRelatedJobs.PK);
			AssertEquals("Should not create a second pivot", 1, Factory.Load<WhsDocketJobPivot>(query).Length);
			AssertEquals("Should be updating same job so should not create different pivot", 1, Factory.Load<WhsDocketJobPivot>(new ZQuery()).Length);
		}

		public void TestImportWarehouseReceiveThroughJobNumber()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var whsReceiveToLoad = Factory.NewWithValidTestData<WhsReceive>();

				whsReceiveToLoad.WD_DocketID = "W00001003";
				whsReceiveToLoad.WD_ExternalReference = "DONTORDERME";
				whsReceiveToLoad.Client.OH_IsWarehouseClient = true;

				Factory.SaveForTesting();

				var universalShipmentWithDataTarget = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseReceive.TestFiles.UniversalShipmentWithDataTarget.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDataTarget);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003 with 1 x WhsDocketReference.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
Updated Warehouse Receipt W00001003 from UniversalShipment.
Successfully saved Warehouse Receipt W00001003 with 1 x WhsDocketReference.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "W00001003"));
					AssertEquals("whsReceive.WD_ExternalReference", "ORDERME", whsReceive.WD_ExternalReference);
					AssertEquals("whsReceive.PK", whsReceiveToLoad.PK, whsReceive.PK);
				});
			}
		}

		public void TestCanImportWarehouseReceiveViaUniversalDataBuss()
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
Added Warehouse Receipt from UniversalShipment.
Successfully saved Warehouse Receipt W00000001 with 1 x WhsDocketReference.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsReceive found, creating new WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsDocketReference found, creating new WhsDocketReference.
Populating WhsDocketReference...
Added Warehouse Receipt from UniversalShipment.
Successfully saved Warehouse Receipt W00000001 with 1 x WhsDocketReference.
".Trim(), logNoteText);

					var receive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "ORDERME"));
					AssertNotNull("Warehouse Receipt should exist", receive);
				});
			}
		}

		public void TestCannotImportWarehouseReceiveWithoutValidClientAddress()
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
ERROR - Cannot Import Receipt
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
No matching WhsReceive found, creating new WhsReceive.
Populating WhsReceive...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way]'.
Error - Cannot Import Receipt
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

		public void TestCannotImportWarehouseReceiveWithoutValidWarehouse()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();

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
ERROR - Cannot Import Receipt
Unable to match Warehouse: WHS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
				".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsReceive found, creating new WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot Import Receipt
Unable to match Warehouse: WHS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);
				});
			}
		}

		public void TestImportShipment_ReceivingStarted_PartialAttributeAddingReceiveLineSuccessful()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				Factory.SaveForTesting();
				receive.PopulateASNLines();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", true, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
				CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 1 x WhsReceiveLine.
				".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsReceiveLine found, creating new WhsReceiveLine.
Populating WhsReceiveLine...
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 1 x WhsReceiveLine.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
					var part2Line = whsReceive.Lines.Single(r => r.WE_OP == part2.PK);
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 2, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
					AssertEquals("part2Line.WE_ClientOrderedUnits", 5m, part2Line.WE_ClientOrderedUnits);
					AssertEquals("part2Line.WE_TransactionQuantity", 5m, part2Line.WE_TransactionQuantity);
					AssertEquals("part2Line.WE_PalletID", "TEST2", part2Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingStarted_PartialAttributeEdittingReceiveLineHasNoChanges()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				line.WE_LineNo = 1;
				line.WE_SubLineNo = 1;
				Factory.SaveForTesting();
				receive.PopulateASNLines();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", true, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
				CreateOrderLine(shipmentDataObject, part1, "TEST2", 20m, 20m, 1, 1);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Successfully loaded matching WhsReceiveLine.
Populating WhsReceiveLine...
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single();
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingStarted_PartialAttributeAddingAndEdittingReceiveLineHasNoChanges()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				line.WE_LineNo = 1;
				line.WE_SubLineNo = 1;
				Factory.SaveForTesting();
				receive.PopulateASNLines();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", true, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
				CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);
				CreateOrderLine(shipmentDataObject, part1, "TEST3", 20m, 20m, 1, 1);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Successfully loaded matching WhsReceiveLine.
Populating WhsReceiveLine...
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single();
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingStarted_CompleteAttributeAddingReceiveLineHasNoChanges()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				Factory.SaveForTesting();
				receive.PopulateASNLines();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", true, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Complete);
				CreateOrderLine(shipmentDataObject, part2, "TEST2", 20m, 20m);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single();
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingStarted_CompleteAttributeEdittingReceiveLineHasNoChanges()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				Factory.SaveForTesting();
				receive.PopulateASNLines();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", true, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Complete);
				CreateOrderLine(shipmentDataObject, part1, "TEST2", 20m, 20m);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single();
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingNotStarted_PartialAttributeAddingAndEdittingReceiveLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				line.WE_LineNo = 1;
				line.WE_SubLineNo = 1;
				Factory.SaveForTesting();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
				CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);
				CreateOrderLine(shipmentDataObject, part1, "TEST3", 20m, 20m, 1, 1);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 2 x WhsReceiveLine.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Successfully loaded matching WhsReceiveLine.
Populating WhsReceiveLine...
No matching WhsReceiveLine found, creating new WhsReceiveLine.
Populating WhsReceiveLine...
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 2 x WhsReceiveLine.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 2, whsReceive.Lines.Count);
					var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
					var part2Line = whsReceive.Lines.Single(r => r.WE_OP == part2.PK);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 20m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 20m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST3", part1Line.WE_PalletID);
					AssertEquals("part2Line.WE_ClientOrderedUnits", 5m, part2Line.WE_ClientOrderedUnits);
					AssertEquals("part2Line.WE_TransactionQuantity", 5m, part2Line.WE_TransactionQuantity);
					AssertEquals("part2Line.WE_PalletID", "TEST2", part2Line.WE_PalletID);
				});
			}
		}

		public void TestImportShipment_ReceivingNotStarted_CompleteAttributeAddingAndEdittingReceiveLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var clientAddress = new OrganisationDataObjectReader(
					OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
					new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
				var client = clientAddress.Header;
				client.OH_IsWarehouseClient = true;
				var whs = Helper.CreateWarehouse("WHS");
				var part1 = Helper.CreateProduct("P1", client);
				var part2 = Helper.CreateProduct("P2", client);
				var receive = Helper.CreateWhsReceive(client, whs, "R1");
				receive.WD_DocketID = "R1";
				var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
				line.WE_PalletID = "TEST1";
				line.WE_LineNo = 1;
				line.WE_SubLineNo = 1;
				Factory.SaveForTesting();
				AssertNotNull("Precondition - receive.Lines.", receive.Lines);
				AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
				AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

				var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
				CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);
				CreateOrderLine(shipmentDataObject, part1, "TEST3", 20m, 20m, 1, 1);

				var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 2 x WhsReceiveLine.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Successfully loaded matching WhsReceiveLine.
Populating WhsReceiveLine...
No matching WhsReceiveLine found, creating new WhsReceiveLine.
Populating WhsReceiveLine...
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 2 x WhsReceiveLine.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
					var part2Line = whsReceive.Lines.Single(r => r.WE_OP == part2.PK);
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 2, whsReceive.Lines.Count);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 20m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 20m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST3", part1Line.WE_PalletID);
					AssertEquals("part2Line.WE_ClientOrderedUnits", 5m, part2Line.WE_ClientOrderedUnits);
					AssertEquals("part2Line.WE_TransactionQuantity", 5m, part2Line.WE_TransactionQuantity);
					AssertEquals("part2Line.WE_PalletID", "TEST2", part2Line.WE_PalletID);
				});
			}
		}

		public void TestExportUniversalShipmentTrigger()
		{
			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var whsReceive = factory.NewWithValidTestData<WhsReceive>();
				whsReceive.WD_DocketID = "W100110011";

				var trigger = whsReceive.WorkflowItems.AddNew();
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

				var logBO = whsReceive.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var dataContextManager = new WarehouseReceiveDataContextManager() as IShipmentDataContextManager;

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, whsReceive, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(
					actionInfo
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => dataContextManager.GetShipmentDataObjectWriter(outboundSessionTracker)
					, whsReceive);

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
					AssertEquals("message.EM_LinkUniqueID", whsReceive.PK, message.EM_LinkUniqueID);

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

		public void TestImportShipment_WithPutawayTransfer_PutawayTransferFinalised_CompleteCollectionContentType()
		{
			AssertImportShipment_WithPutawayTransfer(isPutawayTransferFinalised: true, CollectionContent.Complete);
		}

		public void TestImportShipment_WithPutawayTransfer_PutawayTransferFinalised_NullCollectionContentType()
		{
			AssertImportShipment_WithPutawayTransfer(isPutawayTransferFinalised: true, null);
		}

		public void TestImportShipment_WithPutawayTransfer_PutawayTransferNotFinalised_CompleteCollectionContentType()
		{
			AssertImportShipment_WithPutawayTransfer(isPutawayTransferFinalised: false, CollectionContent.Complete);
		}

		public void TestImportShipment_WithPutawayTransfer_PutawayTransferNotFinalised_NullCollectionContentType()
		{
			AssertImportShipment_WithPutawayTransfer(isPutawayTransferFinalised: false, null);
		}

		public void TestImportShipment_WithPutawayTransfer_PartialCollectionContext_AddingNewLine()
		{
			var clientAddress = new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var client = clientAddress.Header;
			client.OH_IsWarehouseClient = true;
			var whs = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1).Locations[0];
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			receive.WD_DocketID = "R1";
			var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			line.WE_PalletID = "TEST1";
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_WL = whs.DefaultInboundDockDoorLocation.PK;
			Factory.SaveForTesting();
			AssertNotNull("Precondition - receive.Lines.", receive.Lines);
			AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
			AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

			var putawayTransfer = Helper.CreateWhsTransfer(client, whs);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, part1, whs.DefaultInboundDockDoorLocation, location, "TEST1", 10m);
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			Assert("Precondition: receive is putting away.", receive.IsPuttingAway);
			Assert("Precondition - line is picked for unload.", line.IsPickedForUnload);
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
			CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);

			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(message));

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 1 x WhsReceiveLine.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
No matching WhsReceiveLine found, creating new WhsReceiveLine.
Populating WhsReceiveLine...
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1 with 1 x WhsReceiveLine.
".Trim(), logNoteText);

				var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
				AssertNotNull("receive.Lines", whsReceive.Lines);
				AssertEquals("receive.Lines.Count", 2, whsReceive.Lines.Count);
				var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
				AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
				AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
				AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				var part2Line = whsReceive.Lines.Single(r => r.WE_OP == part2.PK);
				AssertEquals("part1Line.WE_ClientOrderedUnits", 5m, part2Line.WE_ClientOrderedUnits);
				AssertEquals("part1Line.WE_TransactionQuantity", 5m, part2Line.WE_TransactionQuantity);
				AssertEquals("part1Line.WE_PalletID", "TEST2", part2Line.WE_PalletID);
			});
		}

		public void TestImportShipment_WithPutawayTransfer_PartialCollectionContext_UpdatingAndAddingLines()
		{
			var clientAddress = new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var client = clientAddress.Header;
			client.OH_IsWarehouseClient = true;
			var whs = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1).Locations[0];
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			receive.WD_DocketID = "R1";
			var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			line.WE_PalletID = "TEST1";
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_WL = whs.DefaultInboundDockDoorLocation.PK;
			Factory.SaveForTesting();
			AssertNotNull("Precondition - receive.Lines.", receive.Lines);
			AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
			AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

			var putawayTransfer = Helper.CreateWhsTransfer(client, whs);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, part1, whs.DefaultInboundDockDoorLocation, location, "TEST1", 10m);
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			Assert("Precondition: receive is putting away.", receive.IsPuttingAway);
			Assert("Precondition - line is picked for unload.", line.IsPickedForUnload);
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Partial);
			CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);
			CreateOrderLine(shipmentDataObject, part1, "TEST3", 20m, 20m, 1, 1);

			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(message));

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Successfully loaded matching WhsReceiveLine.
Populating WhsReceiveLine...
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

				var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
				AssertNotNull("receive.Lines", whsReceive.Lines);
				AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
				var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
				AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
				AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
				AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
			});
		}

		public void TestImportShipment_WithPutawayTransfer_NullOrder()
		{
			var clientAddress = new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var client = clientAddress.Header;
			client.OH_IsWarehouseClient = true;
			var whs = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1).Locations[0];
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			receive.WD_DocketID = "R1";
			var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			line.WE_PalletID = "TEST1";
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_WL = whs.DefaultInboundDockDoorLocation.PK;
			Factory.SaveForTesting();
			AssertNotNull("Precondition - receive.Lines.", receive.Lines);
			AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
			AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

			var putawayTransfer = Helper.CreateWhsTransfer(client, whs);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, part1, whs.DefaultInboundDockDoorLocation, location, "TEST1", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			Assert("Precondition - line has putaway transfer.", line.HasPutawayTransfer);
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Complete);
			shipmentDataObject.Order = null;

			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(message));

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1.
".Trim(), logNoteText);

				var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
				AssertNotNull("receive.Lines", whsReceive.Lines);
				AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
				var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
				AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
				AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
				AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
			});
		}

		public void TestImportShipment_WithPutawayTransfer_NullOrderLineCollection()
		{
			var clientAddress = new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var client = clientAddress.Header;
			client.OH_IsWarehouseClient = true;
			var whs = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1).Locations[0];
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			receive.WD_DocketID = "R1";
			var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			line.WE_PalletID = "TEST1";
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_WL = whs.DefaultInboundDockDoorLocation.PK;
			Factory.SaveForTesting();
			AssertNotNull("Precondition - receive.Lines.", receive.Lines);
			AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
			AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

			var putawayTransfer = Helper.CreateWhsTransfer(client, whs);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, part1, whs.DefaultInboundDockDoorLocation, location, "TEST1", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			Assert("Precondition - line has putaway transfer.", line.HasPutawayTransfer);
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Complete);
			shipmentDataObject.Order.SetOrderLineCollection(() => null);

			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(message));

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Updated Warehouse Receipt R1 from UniversalShipment.
Successfully saved Warehouse Receipt R1.
".Trim(), logNoteText);

				var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
				AssertNotNull("receive.Lines", whsReceive.Lines);
				AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
				var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
				AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
				AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
				AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
			});
		}

		public void TestExportUniversalEventTriggerViaEHub()
		{
			var factory = new BusinessObjectFactory();

			using (factory.AddDisposableService())
			{
				var whsReceive = factory.NewWithValidTestData<WhsReceive>();
				whsReceive.WD_DocketID = "W00001000";
				whsReceive.WD_BOLNo = "BILL";
				whsReceive.WD_CustomerReference = "CUSTOMER";
				whsReceive.WD_ExternalReference = "RECEIVEME";
				whsReceive.WD_ExternalReferenceSplit = new ZByte(2);
				whsReceive.WD_TransportReference = "THIS23";

				var reference = whsReceive.References.AddNew();
				reference.WX_RefType = "CAN";
				reference.WX_Reference = "11334455";

				factory.Save();

				var trigger = whsReceive.WorkflowItems.AddNew();
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

				var logBO = whsReceive.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, whsReceive, Lazy.Create<IStmALog>(() => logBO));
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
					AssertEquals("message.EM_LinkUniqueID", whsReceive.PK, message.EM_LinkUniqueID);

					AssertIsXml("message.EM_MessageText", message.EM_MessageText)
						.HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("ReceiveReference")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(whsReceive.WD_ExternalReference)
							)
						).HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("CAN")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(reference.WX_Reference)
							)
						).HavingExactlyOneChildNode("Event/ContextCollection/Context",
							node => node.HavingAtLeastOneChildNode(child =>
								child.WithName("Type")
									 .WithValue("TransportReference")
							)
							.HavingAtLeastOneChildNode(child =>
								child.WithName("Value")
									 .WithValue(whsReceive.WD_TransportReference)
							)
						);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestImportUniversalEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var whsReceive = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_DocketID = "W00001000";

			var nonMatchingReceive = Factory.NewWithValidTestData<WhsReceive>();
			nonMatchingReceive.WD_DocketID = "W00001001";
			nonMatchingReceive.WD_BOLNo = "BILL";
			nonMatchingReceive.WD_CustomerReference = "CUSTOMER";
			nonMatchingReceive.WD_ExternalReference = "RECEIVEME";
			nonMatchingReceive.WD_ExternalReferenceSplit = new ZByte(2);
			nonMatchingReceive.WD_TransportReference = "THIS23";

			var reference = nonMatchingReceive.References.AddNew();
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
Linked Event to Warehouse Receipt W00001000.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Receipt W00001000.
				".Trim(), message.GetLogNoteText());

				var logs = whsReceive.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Receive Reference - RECEIVEME
Receive Reference Split - 2
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

		public void TestImportUniversalEventWithDataTargetDoesNotMatchUpToADocketOfDifferentType()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.WD_DocketID = "W00001000";

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

				var logs = whsOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Authorized event count", 0, logs.Length);
			});
		}

		public void TestImportUniversalEventWithContextInformationOnly()
		{
			var whsReceive = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_DocketID = "W00002000";
			whsReceive.WD_BOLNo = "BILL";
			whsReceive.WD_CustomerReference = "CUSTOMER";
			whsReceive.WD_ExternalReference = "RECEIVEME";
			whsReceive.WD_ExternalReferenceSplit = new ZByte(2);
			whsReceive.WD_TransportReference = "THIS23";

			var reference = whsReceive.References.AddNew();
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
Linked Event to Warehouse Receipt W00002000.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Warehouse Receipt W00002000.
				".Trim(), message.GetLogNoteText());

				var logs = whsReceive.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Receive Reference - RECEIVEME
Receive Reference Split - 2
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

		public void TestContextInformationIsAllThere()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketID = "W00001000";
			receive.WD_BOLNo = "BILL";
			receive.WD_CustomerReference = "CUSTOMER";
			receive.WD_ExternalReference = "RECEIVEME";
			receive.WD_ExternalReferenceSplit = new ZByte(2);
			receive.WD_TransportReference = "THIS23";

			var reference = receive.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "11334455";

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory.OriginalInventoryHeldCode = "HEL";
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			inventory.InDocketLine.Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "DAM"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, "HEL"));
			Factory.SaveForTesting();

			var manager = (IEventDataContextManagerWithTriggeringLog)receive.GetUniversalDataContextManager();
			var receiveLog = receive.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = receiveLog;
			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value));

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
ReceiveReference - RECEIVEME
ReceiveReferenceSplit - 2
ClientReference - CUSTOMER
TransportReference - THIS23
House Bill - BILL
Customs Approval Number - 11334455
InventoryWithChangedHoldCode - 10x P1 changed from 'DAM' to 'HEL'
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
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WarehouseReceive</Type>
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

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.WIN, RecipientRoleType.BWI };

		protected override DataContextType ExpectedDataContextType => DataContextType.WarehouseReceive;

		protected override Type ExpectedDataObjectReaderType => typeof(WhsReceiveDataObjectReader);

		protected override Type ExpectedDataObjectWriterType => typeof(WhsReceiveDataObjectWriter);

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

		string UniversalShipment => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseReceive.TestFiles.UniversalShipment.xml");

		string UniversalEvent => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.Universal.WarehouseReceive.TestFiles.UniversalEvent.xml");

		UniversalShipment CreateShipmentWithEmptyOrderLineCollection(WhsDocket docket, CollectionContent contentType)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.WarehouseReceive, "R1");
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.Order.OrderNumber = docket.WD_ExternalReference;
			shipmentDataObject.Order.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse();
			shipmentDataObject.Order.Warehouse.Code = docket.Warehouse.WW_WarehouseCode;
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = "ConsignorDocumentaryAddress";
			orgAddress.OrganizationCode = docket.Client.OH_Code;
			shipmentDataObject.OrganizationAddressCollection.Add(orgAddress);

			shipmentDataObject.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>());
			shipmentDataObject.Order.OrderLineCollection.Content = contentType;
			return shipmentDataObject;
		}

		OrderLine CreateOrderLine(UniversalShipment shipmentDataObject, OrgSupplierPart product, ZString palletID, ZDecimal orderedQty, ZDecimal expectedQuantity, int? lineNo = null, int? subLineNo = null)
		{
			var orderLine = new OrderLine();
			orderLine.LineNumber = lineNo;
			orderLine.SubLineNumber = subLineNo;
			orderLine.Product = new Product();
			orderLine.Product.Code = product.OP_PartNum;
			orderLine.PalletID = palletID;
			orderLine.OrderedQty = orderedQty;
			orderLine.ExpectedQuantity = expectedQuantity;
			shipmentDataObject.Order.OrderLineCollection.Add(orderLine);
			return orderLine;
		}

		void AssertImportShipment_WithPutawayTransfer(bool isPutawayTransferFinalised, CollectionContent? contentType)
		{
			var clientAddress = new OrganisationDataObjectReader(
				OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var client = clientAddress.Header;
			client.OH_IsWarehouseClient = true;
			var whs = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1).Locations[0];
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			receive.WD_DocketID = "R1";
			var line = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			line.WE_PalletID = "TEST1";
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_WL = whs.DefaultInboundDockDoorLocation.PK;
			Factory.SaveForTesting();
			AssertNotNull("Precondition - receive.Lines.", receive.Lines);
			AssertEquals("Precondition - receive.Lines.Count.", 1, receive.Lines.Count);
			AssertEquals("Precondition - receive.StartedReceiving.", false, receive.StartedReceiving);

			var putawayTransfer = Helper.CreateWhsTransfer(client, whs);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, part1, whs.DefaultInboundDockDoorLocation, location, "TEST1", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			if (isPutawayTransferFinalised)
			{
				putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			}

			Assert("Precondition - line has putaway transfer.", line.HasPutawayTransfer);
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentWithEmptyOrderLineCollection(receive, CollectionContent.Complete);
			shipmentDataObject.Order.OrderLineCollection.Content = contentType;
			CreateOrderLine(shipmentDataObject, part2, "TEST2", 5m, 5m, 1, 2);

			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (Factory.BOFactory.AddDisposableService())
			{
				AssertNoExceptionThrown(() => manager.Process(message));

				CombineAssertions(() =>
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching WhsReceive.
Populating WhsReceive...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Error - Cannot update Receive Line after receiving started.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);

					var whsReceive = new BusinessObjectFactory().LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketID, "R1"));
					AssertNotNull("receive.Lines", whsReceive.Lines);
					AssertEquals("receive.Lines.Count", 1, whsReceive.Lines.Count);
					var part1Line = whsReceive.Lines.Single(r => r.WE_OP == part1.PK);
					AssertEquals("part1Line.WE_ClientOrderedUnits", 10m, part1Line.WE_ClientOrderedUnits);
					AssertEquals("part1Line.WE_TransactionQuantity", 10m, part1Line.WE_TransactionQuantity);
					AssertEquals("part1Line.WE_PalletID", "TEST1", part1Line.WE_PalletID);
				});
			}
		}

		void AssertImportUniversalEvent_ChangeOfInventory(RecipientRoleType changeOfInventoryRole)
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_CustomerReference = "Customer";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { changeOfInventoryRole }.ToRecipientRoleDetails() });
			universalEvent.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.ClientReference), Value = "Customer" } };

			var logger = new TestErrorLogger { TopLevelDataObject = universalEvent };
			var iManager = (IEventDataContextManager)new WarehouseReceiveDataContextManager();
			AssertEquals("Should not find a parent because event is for Change of Inventory only.", 0, iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger).Length);

			universalEvent.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals("Should find a parent as per normal (event is NOT for Change of Inventory).", 1, iManager.GetLogParentsForEvent(universalEvent, Factory.BOFactory, logger).Length);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory));
		WhsTestHelperFunctions helper;
	}
}
