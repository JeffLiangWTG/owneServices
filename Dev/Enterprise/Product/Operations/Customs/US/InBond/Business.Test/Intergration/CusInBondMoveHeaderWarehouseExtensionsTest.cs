using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using SendsMessagesToCustomsShutterUpperer = Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.WarehouseExtensions.Testing
{
	sealed class CusInBondMoveHeaderWarehouseExtensionsTest : WhsDataTestHelper
	{
		public void TestHasWHSOutwardTransactionAndNotCreatedPending()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals(true, moveHeader.HasWHSOutwardTransactionAndNotCreatedPending());
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			AssertEquals(false, moveHeader.HasWHSOutwardTransactionAndNotCreatedPending());
		}

		public void TestPublishAcceptEventForWHSOutwardInADifferentFactory()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			moveHeader.BM_GONumber = "HELLO";
			moveHeader.PublishAcceptEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, moveHeader.HasChanges);
			AssertNull(moveHeader.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
			Factory.Save();
			moveHeader.BM_GONumber = "BYE";
			moveHeader.PublishAcceptEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, moveHeader.HasChanges);
			AssertNotNull(moveHeader.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
		}

		public void TestPublishCancelEventForWHSOutwardInADifferentFactory()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			moveHeader.BM_GONumber = "HELLO";
			moveHeader.PublishCancelEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, moveHeader.HasChanges);
			AssertNull(moveHeader.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
			Factory.Save();
			moveHeader.BM_GONumber = "BYE";
			moveHeader.PublishCancelEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, moveHeader.HasChanges);
			AssertNotNull(moveHeader.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
		}

		public void TestRestoreLatestClearedBondedWarehouseOutwardInADifferentFactory()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002132", "XJ5", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 110m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward();
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				var exportLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
				AssertEquals(2, moveHeader.Logs.Find(exportLogQuery).Length);
				productLine.BY_InvoiceQuantity = 77m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 33m);
				AssertEquals(3, moveHeader.Logs.Find(exportLogQuery).Length);
				moveHeader.BM_GONumber = "HELLO";
				moveHeader.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory();
				AssertEquals(true, moveHeader.HasChanges);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				productLine.Reload();
				AssertEquals(77m, productLine.BY_InvoiceQuantity);
				AssertEquals(5, moveHeader.Logs.Find(exportLogQuery).Length);
			}
		}

		public void TestPublishCancelEventForWHSOutwardAndSaveIfNeeded()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDec = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0893654455", "XJ5", "ENT865", 110m);
				Factory.Save();
				inwardDec.PublishShipmentForWHSInward(false);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT865";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
				moveHeader.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
				var result = moveHeader.PublishCancelEventForWHSOutwardAndSaveIfNeeded();
				var cancelLog = moveHeader.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(false, cancelLog.IsInDatabase);
				moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
				moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
				var factory = new BusinessObjectFactory();
				moveHeader = factory.Load<CusInBondMoveHeader>(moveHeader.PK);
				result = moveHeader.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				cancelLog = moveHeader.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(true, cancelLog.IsInDatabase);
				moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
			}
		}

		public void TestPublishHoldEventForWHSOutwardAndSaveIfNeeded()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_JobReference = "B012321312";
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENT865";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var productLine = container.Commodities.AddNew();
			productLine.BY_PartNumber = Part.OP_PartNum;
			productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
			productLine.BY_WarehouseEntryLineNo = 1;
			productLine.BY_InvoiceQuantity = 66m;
			Factory.Save();
			var result = moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded();
			var cancelLog = moveHeader.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
			AssertEquals(false, cancelLog.IsInDatabase);
			moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
			var factory = new BusinessObjectFactory();
			header = factory.New<CusInBondHeader>();
			header.BH_JobReference = "B0893654455";
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.MessageInitiator = messageInitiator;
			moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENT865";
			moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			productLine = container.Commodities.AddNew();
			productLine.BY_PartNumber = Part.OP_PartNum;
			productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
			productLine.BY_WarehouseEntryLineNo = 1;
			productLine.BY_InvoiceQuantity = 66m;
			factory.Save();
			result = moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
			cancelLog = moveHeader.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
			AssertEquals(true, cancelLog.IsInDatabase);
			moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
		}

		public void TestPublishAcceptEventForWHSOutwardAndSaveIfNeeded()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_JobReference = "B012321312";
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENT865";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var productLine = container.Commodities.AddNew();
			productLine.BY_PartNumber = Part.OP_PartNum;
			productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
			productLine.BY_WarehouseEntryLineNo = 1;
			productLine.BY_InvoiceQuantity = 66m;
			Factory.Save();
			var result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded();
			var acceptLog = moveHeader.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertEquals(false, acceptLog.IsInDatabase);
			moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
			var factory = new BusinessObjectFactory();
			header = factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_JobReference = "B0893654455";
			header.MessageInitiator = messageInitiator;
			moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENT865";
			moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			productLine = container.Commodities.AddNew();
			productLine.BY_PartNumber = Part.OP_PartNum;
			productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
			productLine.BY_WarehouseEntryLineNo = 1;
			productLine.BY_InvoiceQuantity = 66m;
			factory.Save();
			result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
			acceptLog = moveHeader.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertEquals(true, acceptLog.IsInDatabase);
			moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
		}

		public void TestPublishShipmentForWHSOutward()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_JobReference = "B012321312";
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENT865";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var productLine = container.Commodities.AddNew();
			productLine.BY_PartNumber = Part.OP_PartNum;
			productLine.BY_WarehouseEntryNumber = "XJ5-ENT865";
			productLine.BY_WarehouseEntryLineNo = 1;
			productLine.BY_InvoiceQuantity = 66m;
			Factory.Save();
			var result = moveHeader.PublishShipmentForWHSOutward(true);
			var exportLog = moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
			AssertEquals(true, exportLog.IsInDatabase);
		}

		public void TestPublishShipmentForWHSOutwardWithPreAmendmentData()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002132", "XJ5", "ENT324", 110m);
				inwardDeclaration.JE_MergeBy = "NON";
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_PartNo = Part.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 200m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				inwardInvoiceLine2.JI_BondedWhsQuantity = 10m;
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 20000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 110m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-2", 200m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_JobReference = "B00002134";
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine1 = container.Commodities.AddNew();
				productLine1.BY_PartNumber = Part.OP_PartNum;
				productLine1.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine1.BY_WarehouseEntryLineNo = 1;
				productLine1.BY_InvoiceQuantity = 66m;
				var productLine2 = container.Commodities.AddNew();
				productLine2.BY_PartNumber = Part.OP_PartNum;
				productLine2.BY_InvoiceQuantity = 80m;
				productLine2.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine2.BY_WarehouseEntryLineNo = 2;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-2", 120m);
				productLine1.BY_InvoiceQuantity = 55m;
				productLine2.BY_InvoiceQuantity = 100m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m); // Should keep previous quantity on hold
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-2", 100m); // Should add 20 extra to hold
			}
		}

		#region TestGetLastClearedOutwardUniversalShipment
		public void TestGetLastClearedOutwardUniversalShipment()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002132", "XJ5", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 110m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_JobReference = "B00002133";
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				AssertNull(moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR));
				productLine.BY_InvoiceQuantity = 55m;
				Factory.Save();
				moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertNull(moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR));
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				var shipment = moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR);
				AssertNotNull(shipment);
				var commercialInvoiceLine = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals(66m, commercialInvoiceLine.BondedWarehouseQuantity.GetValueOrDefault());
			}
		}

		#endregion
		#region TestPublishAcceptEventForWHSOutward_RealWarehouse
		public void TestPublishAcceptEventForWHSOutward_RealWarehouse()
		{
			TestPublishAcceptEventForWHSOutward_RealWarehouse_Core(withInTransitTransfer: false);
		}

		public void TestPublishAcceptEventForWHSOutward_RealWarehouse_WithInTransitTransfer()
		{
			TestPublishAcceptEventForWHSOutward_RealWarehouse_Core(withInTransitTransfer: true, inTransitTransferFinalised: false);
		}

		public void TestPublishAcceptEventForWHSOutward_RealWarehouse_WithFinalisedInTransitTransfer()
		{
			TestPublishAcceptEventForWHSOutward_RealWarehouse_Core(withInTransitTransfer: true, inTransitTransferFinalised: true);
		}

		void TestPublishAcceptEventForWHSOutward_RealWarehouse_Core(bool withInTransitTransfer, bool inTransitTransferFinalised = false)
		{
			AssertEquals("Can only finalise In-Transit Transfer if one is created.", false, !withInTransitTransfer && inTransitTransferFinalised);
			WhsWarehouse.WW_IsVirtualWarehouse = false;
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002132", "XJ5", "ENT324", 110m);
				Factory.Save();
				var result1 = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result1.ResultType);
				var whsReceive = (IWhsReceive)result1.FindJobIfExists();
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				whsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive = Factory.Load<IWhsReceive>(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				whsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				Factory.Save();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 110m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_JobReference = "B00002133";
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				var result = moveHeader.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				AssertNull(moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR));
				moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertNull(moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR));
				var order = (IWhsOrder)result.FindJobIfExists();
				var pick = Factory.Load<IWhsPick>(order.WD_WP);
				var orderLine = Factory.Load<IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, order.PK)).Single();
				var pickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.PK);
				var pickLine = Factory.Load<IWhsPickLine>(pickLineQuery).Single();
				IWhsDocketLine inTransitTransferLine = null;
				if (withInTransitTransfer)
				{
					inTransitTransferLine = whsHelper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
					if (inTransitTransferFinalised) // Finalised, test we are excluding stock in the dock door
					{
						whsHelper.FinaliseDocketWithoutUserConfirmation(inTransitTransferLine.WE_WD);
					}

					Factory.Save();
				}
				else
				{
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
					using (whsHelper.MockOutboundDockDoorCreator())
					{
						Factory.Save();
						AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, Factory.Load<IWhsPickLine>(pickLineQuery).Single().WZ_WE_OriginalPickedInventoryLine);
					}
				}

				if (withInTransitTransfer && !inTransitTransferFinalised)
				{
					((IDbConnected)Factory).Connection.ExecuteNonQuery($"UPDATE dbo.WhsDocketLine SET WE_PerPackageQty = 1, WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK = '{inTransitTransferLine.PK}'"); // Ensure we are using the receive line for quantity calculation, bypass validation etc.
				}

				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				var shipment = moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR);
				AssertNotNull(shipment);
				var commercialInvoiceLine = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals(66m, commercialInvoiceLine.BondedWarehouseQuantity.GetValueOrDefault());
				AssertEquals("moveDetail.WarehouseDetails.Count", 1, moveDetail.WarehouseDetails.Count);
				var warehouseDetail = moveDetail.WarehouseDetails[0];
				AssertEquals("warehouseDetail.US_WarehouseBondedQuantity", 10m, warehouseDetail.US_WarehouseBondedQuantity);
				AssertEquals("warehouseDetail.US_WarehouseWithdrawQuantity", 6m, warehouseDetail.US_WarehouseWithdrawQuantity);
				AssertEquals("warehouseDetail.WarehouseBondedQuantityBalance", 4m, warehouseDetail.WarehouseBondedQuantityBalance);
			}
		}

		#endregion
		public void TestHandleOutwardOnAmendment_ErrorDelete()
		{
			// Setup Inward Job
			// Update Bonded Whs
			// Setup Outward Job
			// Update Bonded Whs
			// Change Qunatity
			// Send Amendment Messages
			// Create Error Delete Message
			// Check that inventory amount was set back
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				DeclarationTestHelper.SetupForSendMessage();
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0893654455", "XJ5", "ENT865", 110m);
				inwardDeclaration.JE_MergeBy = "NON";
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 200m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				inwardInvoiceLine2.JI_BondedWhsQuantity = 20m;
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 20000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 200m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_JobReference = "B012321312";
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				header.BH_CarrierSCAC = "ABC";
				header.BH_ImportConveyanceName = "APL VESSEL";
				header.BH_VoyageNumber = "1234A";
				header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
				header.BH_PortUnladingDCode = "5687";
				header.BH_FIRMS = "FOD3";
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				moveHeader.InBondNumber = "123456789";
				moveHeader.BM_InBondCarrierSCAC = "OTT1";
				moveHeader.BM_DestinationPortCode = "1234";
				moveHeader.BM_ForeignDestPortKCode = "12345";
				moveHeader.BM_MonetaryValue = 3000m;
				moveHeader.BM_InBondCarrierID = "123456789012";
				moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT865";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				moveDetail.B9_InBoundQty = 166;
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine1 = container.Commodities.AddNew();
				productLine1.BY_PartNumber = Part.OP_PartNum;
				productLine1.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine1.BY_WarehouseEntryLineNo = 1;
				productLine1.BY_InvoiceQuantity = 66m;
				productLine1.BY_ManifestUnitCode = "NO";
				productLine1.BY_MonetaryValue = 1000m;
				productLine1.BY_PieceCount = 66;
				var productLine2 = container.Commodities.AddNew();
				productLine2.BY_PartNumber = Part2.OP_PartNum;
				productLine2.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine2.BY_WarehouseEntryLineNo = 2;
				productLine2.BY_InvoiceQuantity = 100m;
				productLine2.BY_ManifestUnitCode = "NO";
				productLine2.BY_MonetaryValue = 2000m;
				productLine2.BY_PieceCount = 100;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward();
				var exportLog = moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
				AssertEquals(true, exportLog.IsInDatabase);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				productLine1.BY_InvoiceQuantity = 77m;
				productLine2.BY_InvoiceQuantity = 90m;
				Factory.Save();
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				moveHeader.Reload();
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				AssertEquals("moveHeader.Messages.Count", 2, moveHeader.Messages.Count);
				var message1 = moveHeader.Messages[0];
				var message2 = moveHeader.Messages[1];
				if (message2.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDepartureDelete)
				{
					message2 = moveHeader.Messages[0];
					message1 = moveHeader.Messages[1];
				}

				AssertEquals("message1.EM_Status", MQEDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals("message2.EM_Status", MQEDIMessage.Status.Pending, message2.EM_Status);
				var errorDeleteMessage = Factory.New<MQEDIMessage>();
				errorDeleteMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				errorDeleteMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				errorDeleteMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
				errorDeleteMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
				errorDeleteMessage.EM_Status = EDIMessage.Status.Queued;
				errorDeleteMessage.EM_MessageText = ("B011101XJ5QT                                               " + message1.EM_MessageNum).PadRight(80) + string.Format("10D63{0}   OTT1         00000000                                          ", moveHeader.InBondNumber) + "9501008 INBOND DATA INVALID                                                     " + "9501270 TRANSACTION DATA REJECTED                                               " + "Y  1101SV9QT00003";
				errorDeleteMessage.EM_MessageNum = message1.EM_MessageNum;
				Factory.Save();
				CombineAssertions(() =>
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();
					var failSubject = string.Format("InBond Departure Response (Failure) for {0} / {1}", header.BH_JobReference, moveHeader.InBondNumber);
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject == failSubject);
					AssertContains("Previous Stock Release has been restored. (WHS Order: ", email.Body);
					var newFactory = new BusinessObjectFactory();
					moveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
					AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader.Messages.Count", 3, moveHeader.Messages.Count);
					errorDeleteMessage = newFactory.Load<MQEDIMessage>(errorDeleteMessage.PK);
					AssertCollectionContains(errorDeleteMessage, moveHeader.Messages);
					message2 = newFactory.Load<MQEDIMessage>(message2.PK);
					AssertEquals("message2.EM_Status", MQEDIMessage.Status.Cancelled, message2.EM_Status);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				});
			}
		}

		public void TestHandleOutwardOnAmendment_ErrorAdd()
		{
			// Setup Inward Job
			// Update Bonded Whs
			// Setup Outward Job
			// Update Bonded Whs
			// Change Qunatity
			// Send Amendment Messages
			// Create Clear Delete Message
			// Check that inventory is still on hold
			// Create Error Add Message
			// Check that inventory outward has been cancelled
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				DeclarationTestHelper.SetupForSendMessage();
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0893654455", "XJ5", "ENT865", 110m);
				inwardDeclaration.JE_MergeBy = "NON";
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 200m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				inwardInvoiceLine2.JI_BondedWhsQuantity = 20m;
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 20000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 200m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_JobReference = "B012321312";
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				header.BH_CarrierSCAC = "ABC";
				header.BH_ImportConveyanceName = "APL VESSEL";
				header.BH_VoyageNumber = "1234A";
				header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
				header.BH_PortUnladingDCode = "5687";
				header.BH_FIRMS = "FOD3";
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				moveHeader.InBondNumber = "123456789";
				moveHeader.BM_InBondCarrierSCAC = "OTT1";
				moveHeader.BM_DestinationPortCode = "1234";
				moveHeader.BM_ForeignDestPortKCode = "12345";
				moveHeader.BM_MonetaryValue = 3000m;
				moveHeader.BM_InBondCarrierID = "123456789012";
				moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT865";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				moveDetail.B9_InBoundQty = 166;
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine1 = container.Commodities.AddNew();
				productLine1.BY_PartNumber = Part.OP_PartNum;
				productLine1.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine1.BY_WarehouseEntryLineNo = 1;
				productLine1.BY_InvoiceQuantity = 66m;
				productLine1.BY_ManifestUnitCode = "NO";
				productLine1.BY_MonetaryValue = 1000m;
				productLine1.BY_PieceCount = 66;
				var productLine2 = container.Commodities.AddNew();
				productLine2.BY_PartNumber = Part2.OP_PartNum;
				productLine2.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine2.BY_WarehouseEntryLineNo = 2;
				productLine2.BY_InvoiceQuantity = 100m;
				productLine2.BY_ManifestUnitCode = "NO";
				productLine2.BY_MonetaryValue = 2000m;
				productLine2.BY_PieceCount = 100;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward();
				var exportLog = moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
				AssertEquals(true, exportLog.IsInDatabase);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				productLine1.BY_InvoiceQuantity = 77m;
				productLine2.BY_InvoiceQuantity = 90m;
				Factory.Save();
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				moveHeader.Reload();
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				AssertEquals("moveHeader.Messages.Count", 2, moveHeader.Messages.Count);
				var message1 = moveHeader.Messages[0];
				var message2 = moveHeader.Messages[1];
				if (message2.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDepartureDelete)
				{
					message2 = moveHeader.Messages[0];
					message1 = moveHeader.Messages[1];
				}

				AssertEquals("message1.EM_Status", MQEDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals("message2.EM_Status", MQEDIMessage.Status.Pending, message2.EM_Status);
				var clearDeleteMessage = Factory.New<MQEDIMessage>();
				clearDeleteMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				clearDeleteMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				clearDeleteMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
				clearDeleteMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
				clearDeleteMessage.EM_Status = EDIMessage.Status.Queued;
				clearDeleteMessage.EM_MessageText = ("B011101XJ5QT                                               " + message1.EM_MessageNum).PadRight(80) + string.Format("10D63{0}   OTT1         00000000                                          ", moveHeader.InBondNumber) + "9502209 INBOND DELETED                                                          " + "Y  1101XJ5QT00002";
				clearDeleteMessage.EM_MessageNum = message1.EM_MessageNum;
				Factory.Save();
				CombineAssertions(() =>
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject == string.Format("InBond Departure Response for {0} / {1}", header.BH_JobReference, moveHeader.InBondNumber));
					AssertNotContains("(WHS Order: ", email.Body);
					var newFactory = new BusinessObjectFactory();
					moveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
					AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader.Messages.Count", 3, moveHeader.Messages.Count);
					clearDeleteMessage = newFactory.Load<MQEDIMessage>(clearDeleteMessage.PK);
					AssertCollectionContains(clearDeleteMessage, moveHeader.Messages);
					message2 = newFactory.Load<MQEDIMessage>(message2.PK);
					AssertEquals("message2.EM_Status", MQEDIMessage.Status.Queued, message2.EM_Status);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				});
				var errorOriginalMessage = Factory.New<MQEDIMessage>();
				errorOriginalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				errorOriginalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				errorOriginalMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
				errorOriginalMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
				errorOriginalMessage.EM_Status = EDIMessage.Status.Queued;
				errorOriginalMessage.EM_MessageText = ("B011101XJ5QT                                               " + message2.EM_MessageNum).PadRight(80) + string.Format("10D63{0}   OTT1         00000000                                          ", moveHeader.InBondNumber) + "9501008 INBOND DATA INVALID                                                     " + "9501270 TRANSACTION DATA REJECTED                                               " + "Y  1101SV9QT00003";
				errorOriginalMessage.EM_MessageNum = message2.EM_MessageNum;
				Factory.Save();
				CombineAssertions(() =>
				{
					var failSubject = string.Format("InBond Departure Response (Failure) for {0} / {1}", header.BH_JobReference, moveHeader.InBondNumber);
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject == failSubject);
					AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
					var newFactory = new BusinessObjectFactory();
					moveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
					AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader.Messages.Count", 4, moveHeader.Messages.Count);
					errorOriginalMessage = newFactory.Load<MQEDIMessage>(errorOriginalMessage.PK);
					AssertCollectionContains(errorOriginalMessage, moveHeader.Messages);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 200m);
				});
			}
		}

		public void TestHandleOutwardOnAmendment_ClearAdd()
		{
			// Setup Inward Job
			// Update Bonded Whs
			// Setup Outward Job
			// Update Bonded Whs
			// Change Qunatity
			// Send Amendment Messages
			// Create Clear Delete Message
			// Check that inventory is still on hold
			// Create Clear Add Message
			// Check that inventory outward has updated
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				DeclarationTestHelper.SetupForSendMessage();
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0893654455", "XJ5", "ENT865", 110m);
				inwardDeclaration.JE_MergeBy = "NON";
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 200m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				inwardInvoiceLine2.JI_BondedWhsQuantity = 20m;
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 20000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 110m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 200m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_JobReference = "B012321312";
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				header.BH_CarrierSCAC = "ABC";
				header.BH_ImportConveyanceName = "APL VESSEL";
				header.BH_VoyageNumber = "1234A";
				header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
				header.BH_PortUnladingDCode = "5687";
				header.BH_FIRMS = "FOD3";
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
				moveHeader.InBondNumber = "123456789";
				moveHeader.BM_InBondCarrierSCAC = "OTT1";
				moveHeader.BM_DestinationPortCode = "1234";
				moveHeader.BM_ForeignDestPortKCode = "12345";
				moveHeader.BM_MonetaryValue = 3000m;
				moveHeader.BM_InBondCarrierID = "123456789012";
				moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT865";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				moveDetail.B9_InBoundQty = 166;
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine1 = container.Commodities.AddNew();
				productLine1.BY_PartNumber = Part.OP_PartNum;
				productLine1.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine1.BY_WarehouseEntryLineNo = 1;
				productLine1.BY_InvoiceQuantity = 66m;
				productLine1.BY_ManifestUnitCode = "NO";
				productLine1.BY_MonetaryValue = 1000m;
				productLine1.BY_PieceCount = 66;
				var productLine2 = container.Commodities.AddNew();
				productLine2.BY_PartNumber = Part2.OP_PartNum;
				productLine2.BY_WarehouseEntryNumber = "XJ5-ENT865";
				productLine2.BY_WarehouseEntryLineNo = 2;
				productLine2.BY_InvoiceQuantity = 100m;
				productLine2.BY_ManifestUnitCode = "NO";
				productLine2.BY_MonetaryValue = 2000m;
				productLine2.BY_PieceCount = 100;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward();
				var exportLog = moveHeader.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
				AssertEquals(true, exportLog.IsInDatabase);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 44m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				productLine1.BY_InvoiceQuantity = 77m;
				productLine2.BY_InvoiceQuantity = 90m;
				Factory.Save();
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				moveHeader.Reload();
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				AssertEquals("moveHeader.Messages.Count", 2, moveHeader.Messages.Count);
				var message1 = moveHeader.Messages[0];
				var message2 = moveHeader.Messages[1];
				if (message2.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDepartureDelete)
				{
					message2 = moveHeader.Messages[0];
					message1 = moveHeader.Messages[1];
				}

				AssertEquals("message1.EM_Status", MQEDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals("message2.EM_Status", MQEDIMessage.Status.Pending, message2.EM_Status);
				var clearDeleteMessage = Factory.New<MQEDIMessage>();
				clearDeleteMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				clearDeleteMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				clearDeleteMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
				clearDeleteMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
				clearDeleteMessage.EM_Status = EDIMessage.Status.Queued;
				clearDeleteMessage.EM_MessageText = ("B011101XJ5QT                                               " + message1.EM_MessageNum).PadRight(80) + string.Format("10D63{0}   OTT1         00000000                                          ", moveHeader.InBondNumber) + "9502209 INBOND DELETED                                                          " + "Y  1101XJ5QT00002";
				clearDeleteMessage.EM_MessageNum = message1.EM_MessageNum;
				Factory.Save();
				CombineAssertions(() =>
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject == string.Format("InBond Departure Response for {0} / {1}", header.BH_JobReference, moveHeader.InBondNumber));
					AssertNotContains("(WHS Order: ", email.Body);
					var newFactory = new BusinessObjectFactory();
					moveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
					AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader.Messages.Count", 3, moveHeader.Messages.Count);
					clearDeleteMessage = newFactory.Load<MQEDIMessage>(clearDeleteMessage.PK);
					AssertCollectionContains(clearDeleteMessage, moveHeader.Messages);
					message2 = newFactory.Load<MQEDIMessage>(message2.PK);
					AssertEquals("message2.EM_Status", MQEDIMessage.Status.Queued, message2.EM_Status);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 100m);
				});
				var clearOriginalMessage = Factory.New<MQEDIMessage>();
				clearOriginalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				clearOriginalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				clearOriginalMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
				clearOriginalMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
				clearOriginalMessage.EM_Status = EDIMessage.Status.Queued;
				clearOriginalMessage.EM_MessageText = ("B011101XJ5QT                                               " + message2.EM_MessageNum).PadRight(80) + string.Format("10A63{0}   OTT11234     00000323123456789012YY                            ", moveHeader.InBondNumber) + "20W23511  QP FTZ WITHDRAWAL      1521        1101      W235                     " + "30A 0001OTT1XJ5ENT865                                               0000000066  " + "9502220 BILL ACCEPTED FOR INBOND                                                " + "Y  1101SV9QT00004";
				clearOriginalMessage.EM_MessageNum = message2.EM_MessageNum;
				Factory.Save();
				CombineAssertions(() =>
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject == string.Format("InBond Departure Response for {0} / {1}", header.BH_JobReference, moveHeader.InBondNumber));
					AssertContains("Stock Release can be finalized. (WHS Order: ", email.Body);
					var newFactory = new BusinessObjectFactory();
					moveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
					AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
					AssertEquals("moveHeader.Messages.Count", 4, moveHeader.Messages.Count);
					clearOriginalMessage = newFactory.Load<MQEDIMessage>(clearOriginalMessage.PK);
					AssertCollectionContains(clearOriginalMessage, moveHeader.Messages);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-1", 33m);
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT865-2", 110m);
				});
			}
		}

		public void TestWarehouseEndToEndForVirtualWarehouse()
		{
			AssertWarehouseEndToEnd(true);
		}

		public void TestWarehouseEndToEndForRealWarehouse()
		{
			AssertWarehouseEndToEnd(false);
		}

		void AssertWarehouseEndToEnd(bool isVirtualWarehouse)
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			JobDeclaration inwardDeclaration;
			PublishToUniversalResult result;
			IWhsReceive whsReceive;
			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				WhsWarehouse.WW_IsVirtualWarehouse = isVirtualWarehouse;
				inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002131", "XJ5", "ENT323", 1m);
				Factory.Save();
				// Force creation of warehouse
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				if (isVirtualWarehouse)
				{
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT323-1", 1m);
					inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
					AssertInventoryAvailabilityInActualDatabase("XJ5-ENT323-1", 0m);
				}
				else
				{
					inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				}

				// Inward data for outward testing
				inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002134", "XJ5", "ENT324", 100m);
				Factory.Save();
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
			}

			if (isVirtualWarehouse)
			{
				using (whsHelper.UsePutawayEngineManagerMock())
				using (whsHelper.UseAllocationEngineMock())
				{
					AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);
					inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				}
			}
			else
			{
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 0m); // Is still pending
				AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				whsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive = Factory.Load<IWhsReceive>(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				whsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				Factory.Save();
			}

			AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 100m);
			AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);
			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = Importer.MainAddress.PK;
				header.BH_FTZMove = ZBool.True;
				header.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var line = container.Commodities.AddNew();
				line.BY_PartNumber = Part.OP_PartNum;
				line.BY_InvoiceQuantity = 60m;
				line.BY_WarehouseEntryNumber = "XJ5-ENT324";
				line.BY_WarehouseEntryLineNo = 1;
				Factory.Save();
				// Outward data creation
				result = moveHeader.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data is set to be finalised
				result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data is updated with increase in quantity
				line.BY_InvoiceQuantity = 70m;
				Factory.Save();
				result = moveHeader.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 30m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data is updated with decrease in quantity
				moveHeader.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory();
				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 40m);
				AssertEquals(70m, line.BY_InvoiceQuantity);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data is updated with decrease in quantity
				line.BY_InvoiceQuantity = 50m;
				Factory.Save();
				result = moveHeader.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 40m); // should still keep 60 on hold
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				// Outward amendment is cleared; need to publish current shipment follow by accept event
				result = moveHeader.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data cancellation failure: Hold the data and send accept when Customs withdrawal is rejected
				result = moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				IWhsOrder whsOrder2 = null;
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder2 = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder2);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000003", whsOrder2.WD_DocketID);
					AssertEquals("ATP", whsOrder2.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data hold is removed
				result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data cancellation successful: Hold the data and send cancel when Customs withdrawal is accepted
				result = moveHeader.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder2 = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder2);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000003", whsOrder2.WD_DocketID);
					AssertEquals("ATP", whsOrder2.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, moveHeader.BM_WarehouseTransactionStatus);
				// Outward data hold is removed
				result = moveHeader.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000003", whsOrder.WD_DocketID);
					AssertEquals("CAN", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 100m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
				// New outward data
				moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				line = container.Commodities.AddNew();
				line.BY_PartNumber = Part.OP_PartNum;
				line.BY_InvoiceQuantity = 60m;
				line.BY_WarehouseEntryNumber = "XJ5-ENT324";
				line.BY_WarehouseEntryLineNo = 1;
				Factory.Save();
				result = moveHeader.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}

				AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 40m);
			}
		}
	}
}
