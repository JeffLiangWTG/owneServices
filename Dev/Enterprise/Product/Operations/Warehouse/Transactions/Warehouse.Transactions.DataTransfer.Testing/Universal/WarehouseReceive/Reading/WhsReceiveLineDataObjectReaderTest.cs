using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing.WhsBondedChangeOfInventoryDataObjectReaderTest;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsReceiveLineDataObjectReaderTest : WhsDocketLineDataObjectReaderTest<WhsReceive, WhsReceiveLine, WhsReceiveLineDataObjectReader>
	{
		#region TestChangeOfInventory

		public void TestChangeOfInventory_ChangeOfOwnership()
		{
			var dataForChangeOfInventory = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var docket = GetNewDocket(dataForChangeOfInventory.Orgs.CRAHOLSYD, dataForChangeOfInventory.GetOrCreateWarehouseInDB());
			var locationDO = new Location { Row = "A", Column = 2, Level = 3, Tray = 4 };
			var productDO = new Product { Code = "P1" };
			var extraDetails = new ExtraOrderLineDetails(null, Array.Empty<ZString>(), null, "W1", "Red", "Small", "S1234", "SERNUM", null);
			var docketLineDataObject = new OrderLine { Product = productDO, Location = locationDO, PartAttribute1 = "A1", PartAttribute2 = "A2", PartAttribute3 = "A3", SerialNumber = "SN" };
			docketLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();

			var reader1 = new WhsReceiveLineDataObjectReader(docketLineDataObject, Logger, Factory, docket, null, extraDetails);
			var docketLine1 = reader1.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("P1", docketLine1.ProductCode);
			AssertEquals("A1", docketLine1.WE_PartAttrib1);
			AssertEquals("A2", docketLine1.WE_PartAttrib2);
			AssertEquals("A3", docketLine1.WE_PartAttrib3);
			AssertEquals("SN", docketLine1.WE_SerialNumber);
			AssertEquals("KEYZOR-5", docketLine1.WE_BondedEntryKey);
			AssertEquals("", docketLine1.LocationString);

			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB(createWarehouse: false, createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } });
			var docket2 = GetNewDocket(dataForChangeOfInventory.Orgs.WUFSHIJNB, dataForChangeOfInventory.GetOrCreateWarehouseInDB());
			var reader2 = new WhsReceiveLineDataObjectReader(docketLineDataObject, Logger, Factory, docket2, null, extraDetails);

			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true))
			{
				var docketLine2 = reader2.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("W1", docketLine2.ProductCode);
				AssertEquals("Red", docketLine2.WE_PartAttrib1);
				AssertEquals("Small", docketLine2.WE_PartAttrib2);
				AssertEquals("S1234", docketLine2.WE_PartAttrib3);
				AssertEquals("SERNUM", docketLine2.WE_SerialNumber);
				AssertEquals("KEYZOR-5", docketLine2.WE_BondedEntryKey);
				AssertEquals("A-2-3-4", docketLine2.LocationString);
			}
		}

		public void TestChangeOfInventory_ChangeOfRegime()
		{
			var dataForChangeOfInventory = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: true, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCR } });
			Factory.SaveForTesting();

			var productDO = new Product { Code = "P1" };
			var extraDetails = new ExtraOrderLineDetails(null, Array.Empty<ZString>(), null, "W1", "Red", "Small", "S1234", "SERNUM", null);
			var docketLineDataObject = new OrderLine { Product = productDO, PartAttribute1 = "A1", PartAttribute2 = "A2", PartAttribute3 = "A3", SerialNumber = "SN" };
			docketLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();

			var docket = GetNewDocket(dataForChangeOfInventory.Orgs.CRAHOLSYD, dataForChangeOfInventory.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: true));

			var reader = new WhsReceiveLineDataObjectReader(docketLineDataObject, Logger, Factory, docket, null, extraDetails);

			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false))
			{
				var docketLine = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("P1", docketLine.ProductCode);
				AssertEquals("A1", docketLine.WE_PartAttrib1);
				AssertEquals("A2", docketLine.WE_PartAttrib2);
				AssertEquals("A3", docketLine.WE_PartAttrib3);
				AssertEquals("SN", docketLine.WE_SerialNumber);
				AssertEquals("KEYZOR-5", docketLine.WE_BondedEntryKey);
				AssertEquals("", docketLine.LocationString);
			}
		}

		public void TestChangeOfInventory_ChangeOfRegime_MissingCustomsData()
		{
			var dataForChangeOfInventory = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var locationDO = new Location { Row = "A", Column = 2, Level = 3, Tray = 4 };
			var productDO = new Product { Code = "P1" };
			var extraDetails = new ExtraOrderLineDetails(null, Array.Empty<ZString>(), null, "W1", "Red", "Small", "S1234", "SERNUM", null);
			var docketLineDataObject = new OrderLine { Product = productDO, Location = locationDO, PartAttribute1 = "A1", PartAttribute2 = "A2", PartAttribute3 = "A3", SerialNumber = "SN" };

			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB(createWarehouse: false, createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCR } });
			var docket = GetNewDocket(dataForChangeOfInventory.Orgs.CRAHOLSYD, dataForChangeOfInventory.GetOrCreateWarehouseInDB());
			var reader = new WhsReceiveLineDataObjectReader(docketLineDataObject, Logger, Factory, docket, null, extraDetails);

			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false))
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), "Bonded Entry Key must be provided for Change of Regime import.", () => reader.ReadIntoBusinessObject(), assertStartsWith: true);
			}
		}

		#endregion

		#region TestChangeOfInventory_LocationOutOfRange

		public void TestChangeOfInventory_LocationOutOfRange()
		{
			var dataForChangeOfInventory = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
			dataForChangeOfInventory.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var docket = GetNewDocket(dataForChangeOfInventory.Orgs.CRAHOLSYD, dataForChangeOfInventory.GetOrCreateWarehouseInDB());
			var locationDO = new Location { Row = "A", Column = 99, Level = 99, Tray = 99 };
			var productDO = new Product { Code = "P1" };
			var extraDetails = new ExtraOrderLineDetails(null, Array.Empty<ZString>(), null, "W1", "Red", "Small", "S1234", "SERN", null);
			var docketLineDataObject = new OrderLine { Product = productDO, Location = locationDO, PartAttribute1 = "A1", PartAttribute2 = "A2", PartAttribute3 = "A3", SerialNumber = "SN" };

			var reader = new WhsReceiveLineDataObjectReader(docketLineDataObject, Logger, Factory, docket, null, extraDetails);
			var docketLine = reader.ReadIntoBusinessObject();
			AssertNull("Should not have imported location.", docketLine.Location);
		}

		#endregion

		#region TestPackageGroupId

		public void TestPackageGroupId()
		{
			var shipment = new Shipment();
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var warehouse = Data.GetOrCreateWarehouseInDB(isBondedWarehouse: true);
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var product = Data.Product; // poke to save to database
			Factory.SaveForTesting();

			var docket = GetNewDocket(client, warehouse);
			((ISupportDataImporting)docket).IsImportingData = true; // set docket property like in normal imports
			try
			{
				var docketLineDataObject = new OrderLine { Product = new Product { Code = product.OP_PartNum }, PackageGroupId = "ABC" };
				var reader1 = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				var docketLine1 = reader1.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("W00000001-001", docketLine1.WE_PackageGroupId);
				AssertEquals("W00000001", docket.WD_DocketID);

				var reader2 = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				var docketLine2 = reader2.ReadIntoBusinessObject();
				docket.WD_ExternalReference = "002"; //update docket like all imports do
				Factory.SaveForTesting();
				AssertEquals("W00000001-001", docketLine2.WE_PackageGroupId);

				docketLineDataObject.PackageGroupId = "XYZ";
				var reader3 = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				var docketLine3 = reader3.ReadIntoBusinessObject();
				docket.WD_ExternalReference = "003"; //update docket like all imports do
				Factory.SaveForTesting();
				AssertEquals("W00000001-002", docketLine3.WE_PackageGroupId);

				docketLineDataObject.PackageGroupId = "";
				var reader4 = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				var docketLine4 = reader4.ReadIntoBusinessObject();
				docket.WD_ExternalReference = "004"; //update docket like all imports do
				Factory.SaveForTesting();
				AssertEquals(docketLine3, docketLine4);
				AssertEquals("W00000001-002", docketLine4.WE_PackageGroupId);
			}
			finally
			{
				((ISupportDataImporting)docket).IsImportingData = false;
			}
		}

		#endregion

		#region TestNoExceptionIsThrownWhenValidProductCodeIsSupplied

		public void TestNoExceptionIsThrownWhenValidProductCodeIsSupplied()
		{
			var whsReceive = GetReceiveLineParent(Factory);

			Factory.SaveForTesting();

			WhsReceiveLine line = null;
			var inventoryLineDataObject = new OrderLine();
			inventoryLineDataObject.Link = 3;
			inventoryLineDataObject.Product = new Product { Code = "BOWLHAT" };

			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, whsReceive, Array.Empty<WhsReceiveLine>());
			AssertNoExceptionThrown("ReceiveLine reads in fine.", () => line = reader.ReadIntoBusinessObject());
			AssertEquals("line.SupplierPart.OP_PartNum", "BOWLHAT", line.SupplierPart.OP_PartNum);
			AssertEquals("line.SupplierPart.OP_Desc", "Bowler Hat", line.SupplierPart.OP_Desc);
		}

		#endregion

		#region TestExceptionIsThrownWhenUpdatingExistingLineAfterStartingReceiving

		public void TestExceptionIsThrownWhenUpdatingExistingLineAfterStartingReceiving()
		{
			var receive = GetReceiveLineParent(Factory);
			var inventoryLineDataObject = SetupInventoryLine();
			inventoryLineDataObject.PalletID = "State1";
			Factory.SaveForTesting();

			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has 1 line attached.", 1, receive.Lines.Count);
			AssertEquals("Precondition - Receive palletID is set correctly", "State1", receive.Lines[0].WE_PalletID);

			var shipment = new Shipment();
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			var matcherStub = new Mock<IWhsReceiveLineMatcher>();
			inventoryLineDataObject.PalletID = "State2";
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(receive, inventoryLineDataObject, inventoryLineDataObject.Product, Array.Empty<WhsReceiveLine>())).Returns(originalReceiveLine);

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				var receiveLine2 = reader.ReadIntoBusinessObject();
				AssertEquals("Should read into existing line.", originalReceiveLine, receiveLine2);
				AssertEquals("Line should be updated after reading.", "State2", receiveLine2.WE_PalletID);
			}

			Factory.SaveForTesting();
			receive.PopulateASNLines();
			AssertEquals("Populating ASN Lines should be called.", true, receive.PopulateASNHasBeenCalled_TestsOnly);
			AssertEquals("Receive has started receiving products", true, receive.StartedReceiving);
			inventoryLineDataObject.PalletID = "State3";
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(receive, inventoryLineDataObject, inventoryLineDataObject.Product, Array.Empty<WhsReceiveLine>())).Returns(originalReceiveLine);

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				AssertExceptionThrown("Cannot update Receive Line after receiving started.", typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestNewProductIsCreatedWhenProductCodeSuppliedIsInvalidWithRegistryOn

		protected override ILineAttributes GetLineAttributes(WhsReceiveLine receiveLine)
		{
			receiveLine.Inventory.Load();
			return receiveLine.Inventory[0];
		}

		#endregion

		#region TestBasicReceiveLineLevelFieldMappings

		public void TestBasicReceiveLineLevelFieldMappings()
		{
			var consigneeAddress = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeAddress)), Logger, Factory).GetMatchedOrNewForTesting();
			Logger.ClearLogs();
			var receive = GetReceiveLineParent(Factory);
			Helper.EnableWarehouseForBond(receive.Warehouse, true);
			receive.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			EnableAllAttributeUse(receive.Client, "BOWLHAT");

			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var receiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull("whsReceiveLineBO", receiveLine);

			var inventoryLine = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Inventory[0];
			AssertNotNull(inventoryLine);

			receiveLine = new BusinessObjectFactory().Load<WhsReceiveLine>(receiveLine.PK);
			AssertEquals("Inventory on Docket and Lines are the same", inventoryLine.PK, receiveLine.Inventory[0].PK);

			CombineAssertions(() =>
			{
				AssertContents(inventoryLine);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Information - Matching 'ConsigneeAddress':- Matched to 'WUFSHIJNB' by code, address '' (only address).
Information - Successfully saved Docket Line.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestNullPutAwayAreaDoesNotThrowException

		public void TestNullPutAwayAreaDoesNotThrowException()
		{
			var whsReceive = GetReceiveLineParent(Factory);

			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			inventoryLineDataObject.PutAwayArea = null;

			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, whsReceive, Array.Empty<WhsReceiveLine>());
			AssertNoExceptionThrown("ReceiveLine reads in fine.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestErrorIfSuppliedOrderedQtyNotEqualToSuppliedExpectedQty

		public void TestErrorIfSuppliedOrderedQtyNotEqualToSuppliedExpectedQty()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			var expectedQty = 15m;
			var orderQty = 30m;

			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.ExpectedQuantity = expectedQty;
			receiveLineDataObject.OrderedQty = orderQty;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			AssertExceptionThrown("Cannot import Receive Line if Ordered Qty, Expected Quantity or Package Qty (when converted into appropriate units) have a value greater than 0 that are not equal.", typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());

			receiveLineDataObject.ExpectedQuantity = orderQty;
			receiveLineDataObject.OrderedQty = orderQty;

			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(orderQty, readLine.WE_ClientOrderedUnits);
			AssertEquals(orderQty, readLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestErrorIfSuppliedOrderedQtyAndExpectedQtyAreBothNegative

		public void TestErrorIfSuppliedOrderedQtyAndExpectedQtyAreBothNegative()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.OrderedQty = -1;
			receiveLineDataObject.ExpectedQuantity = -1;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			AssertExceptionThrown("Cannot Import Receive Line 1:\r\nQuantity cannot be negative.", typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestSuppliedOrderedQtyAndExpectedQtyAreBothZero

		public void TestSuppliedOrderedQtyAndExpectedQtyAreBothZero()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.OrderedQty = 0;
			receiveLineDataObject.ExpectedQuantity = 0;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(0m, readLine.WE_ClientOrderedUnits);
			AssertEquals(0m, readLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestSuppliedOrderedQtyIsZeroAndExpectedQtyIsNegative

		public void TestSuppliedOrderedQtyIsZeroAndExpectedQtyIsNegative()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.OrderedQty = 0;
			receiveLineDataObject.ExpectedQuantity = -1;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(0m, readLine.WE_ClientOrderedUnits);
			AssertEquals(0m, readLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestSuppliedOrderedQtyIsNegativeAndExpectedQtyIsZero

		public void TestSuppliedOrderedQtyIsNegativeAndExpectedQtyIsZero()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.OrderedQty = -1;
			receiveLineDataObject.ExpectedQuantity = 0;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(0m, readLine.WE_ClientOrderedUnits);
			AssertEquals(0m, readLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestOrderedQuantityFallback

		public void TestOrderedQuantityFallback()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			var qty = 15m;

			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.ExpectedQuantity = qty;
			receiveLineDataObject.OrderedQty = null;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_TransactionQuantity);

			receiveLineDataObject.OrderedQty = qty;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_TransactionQuantity);

			receiveLineDataObject.OrderedQty = 0;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_TransactionQuantity);

			receiveLineDataObject.ExpectedQuantity = 0;
			receiveLineDataObject.OrderedQty = 0;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(ZDecimal.Zero, readLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestExpectedQuantityFallback

		public void TestExpectedQuantityFallback()
		{
			var receiveLine = GetReceiveLineParent(Factory);
			var qty = 15m;
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };
			receiveLineDataObject.OrderedQty = qty;
			receiveLineDataObject.ExpectedQuantity = null;

			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_ClientOrderedUnits);

			receiveLineDataObject.ExpectedQuantity = qty;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_ClientOrderedUnits);

			receiveLineDataObject.ExpectedQuantity = 0;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(qty, readLine.WE_ClientOrderedUnits);

			receiveLineDataObject.ExpectedQuantity = 0;
			receiveLineDataObject.OrderedQty = 0;
			readLine = reader.ReadIntoBusinessObject();
			AssertEquals(ZDecimal.Zero, readLine.WE_ClientOrderedUnits);
		}

		#endregion

		#region TestPackUQFallback

		public void TestPackUQFallback()
		{
			var receive = GetReceiveLineParent(Factory);
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			part.OP_StockKeepingUnit = "TST";
			Factory.SaveForTesting();

			var receiveLineDataObject = CreateReceiveLineDataObject(3m, 1m, "", "");
			var reader = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();
			AssertEquals("Since there was no value for PackageQtyUnit on the dataObject, the value of WE_F3_NKPackType should have been defaulted from the Product", part.OP_StockKeepingUnit, readLine.WE_F3_NKPackType);
		}

		#endregion

		#region TestRequiredByFallback

		public void TestRequiredByFallback()
		{
			var shipment = new Shipment();
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			shipment.LocalProcessing.DeliveryRequiredBy = new ZDateTime(2011, 1, 3);
			var receiveLine = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var receiveLineDataObject = new OrderLine();
			receiveLineDataObject.Product = new Product { Code = "BOWLHAT" };

			var reader1 = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine1 = reader1.ReadIntoBusinessObject();
			AssertEquals("No Shipment - Should not blow up", ZDateTimeOffset.Empty, readLine1.WE_RequiredByDate);

			Logger.TopLevelDataObject = shipment;
			var reader2 = new WhsReceiveLineDataObjectReader(receiveLineDataObject, Logger, Factory, receiveLine, Array.Empty<WhsReceiveLine>());
			var readLine2 = reader2.ReadIntoBusinessObject();
			AssertEquals("No Line Required By, fallback to header Required By", new ZDateTimeOffset(2011, 1, 3), readLine2.WE_RequiredByDate);

			receiveLineDataObject.RequiredBy = new ZDateTimeOffset(2011, 1, 4);
			var readLine3 = reader2.ReadIntoBusinessObject();
			AssertEquals("Has Line Required By, so use that", new ZDateTimeOffset(2011, 1, 4), readLine3.WE_RequiredByDate);
		}

		#endregion

		#region TestInvalidDocketLineStatus

		public void TestInvalidDocketLineStatus()
		{
			var inventoryParent = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Product = new Product { Code = "BOWLHAT" };
			orderLineDataObject.Status = new CodeDescriptionPair { Code = "PLC" };
			// PLC is exported as status from Order Manager orders with Req. In Store entered

			var reader = new WhsReceiveLineDataObjectReader(orderLineDataObject, Logger, Factory, inventoryParent, Array.Empty<WhsReceiveLine>());
			var readLine = reader.ReadIntoBusinessObject();

			AssertNotEquals("Should not populate docket line status with an invalid code", "PLC", readLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestHoldCode

		public void TestHoldCodeReason()
		{
			Logger.ClearLogs();
			var receive = GetReceiveLineParent(Factory);
			EnableAllAttributeUse(receive.Client, "BOWLHAT");
			Factory.SaveForTesting();

			var line = SetupInventoryLine();
			line.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Foo" };
			line.CurrentHoldReason = "Alien sighting";

			var reader = new WhsReceiveLineDataObjectReader(line, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var lineBizO = reader.ReadIntoBusinessObject();

			AssertEquals("Should have set OriginalHoldCode.", lineBizO.WE_WHC_NKOriginalInventoryHeldCode, "HEL");
			AssertEquals("Should have set CurrentHoldCode, as this is an unfinalised receive.", lineBizO.WE_WHC_NKCurrentInventoryHeldCode, "HEL");
			AssertEquals("Should have set CurrentHoldReason, as this is an unfinalised receive.", lineBizO.WE_CurrentHoldReason, "Alien sighting");
		}

		public void TestHoldCodeValidImport()
		{
			ReadLineWithHoldCode("HEL");
			var log = @"Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
";
			AssertMultilineASCIIEquals("Should not add Warning about Hold Code.", log, Logger.Logs);
		}

		public void TestHoldCodeValidImport_EmptyHoldCode()
		{
			ReadLineWithHoldCode("");
			var log = @"Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
";
			AssertMultilineASCIIEquals("Should not add Warning about Hold Code.", log, Logger.Logs);
		}

		public void TestHoldCodeValidImport_CustomHoldCode()
		{
			Helper.CreateInventoryHeldCode("CUSTOM", "Custom Hold code");
			ReadLineWithHoldCode("CUSTOM");
			var log = @"Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
";
			AssertMultilineASCIIEquals("Should not add Warning about Hold Code.", log, Logger.Logs);
		}

		public void TestHoldCodeInvalidImport()
		{
			ReadLineWithHoldCode("FOO");
			var log = @"Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Warning - Imported Hold Code 'FOO' is not valid on this system.
";
			AssertMultilineASCIIEquals("Should create Warning about invalid Hold Code.", log, Logger.Logs);
		}

		public void TestHoldCodeInvalidImport_NoHoldCodeInDataObject()
		{
			ReadLineWithHoldCode(null);
			var log = @"Information - No matching WhsReceiveLine found, creating new WhsReceiveLine.
Information - Populating WhsReceiveLine...
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
";
			AssertMultilineASCIIEquals("Should not add Warning about Hold Code.", log, Logger.Logs);
		}

		void ReadLineWithHoldCode(string holdCode)
		{
			Logger.ClearLogs();
			var receive = GetReceiveLineParent(Factory);
			EnableAllAttributeUse(receive.Client, "BOWLHAT");
			Factory.SaveForTesting();

			var line = SetupInventoryLine();
			line.OriginalHoldCode = holdCode == null ? null : new CodeDescriptionPair9Char { Code = holdCode, Description = "Foo" };

			var reader = new WhsReceiveLineDataObjectReader(line, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var lineBizO = reader.ReadIntoBusinessObject();

			AssertEquals("Should have set OriginalHoldCode.", lineBizO.WE_WHC_NKOriginalInventoryHeldCode, holdCode ?? "");
			AssertEquals("Should have set CurrentHoldCode, as this is an unfinalised receive.", lineBizO.WE_WHC_NKCurrentInventoryHeldCode, holdCode ?? "");
		}

		#endregion

		#region TestHoldCodeAndHoldReason_NotImportedWhenFinalized

		public void TestHoldCodeAndHoldReason_NotImportedWhenFinalized()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, receive.IsInDatabase);

			var inventoryLineDataObject = SetupInventoryLine();
			var reader1 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader1.ReadIntoBusinessObject();
			originalReceiveLine.WE_WL = receive.Warehouse.DefaultOutboundDockDoorLocation.PK;
			originalReceiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Factory.SaveForTesting();
			AssertEquals("Precondition", true, originalReceiveLine.IsInDatabase);
			AssertEquals("Precondition", 1, receive.Lines.Count);

			var shipment = new Shipment();
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			receive.WD_ArrivalDate = ZDateTimeOffset.Today; // Receive must have Arrival Date in order to be finalized.
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition", "", originalReceiveLine.WE_WHC_NKOriginalInventoryHeldCode);
			Factory.SaveForTesting();

			var matcherStub = new Mock<IWhsReceiveLineMatcher>();
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(receive, inventoryLineDataObject, inventoryLineDataObject.Product, Array.Empty<WhsReceiveLine>())).Returns(originalReceiveLine);

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				inventoryLineDataObject.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };
				inventoryLineDataObject.CurrentHoldReason = "Blah";
				var reader2 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());

				var receiveLine2 = reader2.ReadIntoBusinessObject();
				AssertEquals("Should read into existing line.", originalReceiveLine, receiveLine2);
				AssertEquals("Should not have set HoldCode.", "", originalReceiveLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Should not have set HoldCode.", "", originalReceiveLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Should not have set HoldReason.", "", originalReceiveLine.WE_CurrentHoldReason);
			}
		}

		#endregion

		#region TestOrderedQtyPopulatingFromPackageQty

		public void TestOrderedQtyPopulatingFromPackageQty()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery()); // Get part created in GetNewDocketLineParent
			var partUnits = part.PartUnits;
			AddPartUnits(partUnits, "UNT", 5, "BOX");
			AddPartUnits(partUnits, "KG", 25, "UNT");

			var receiveLineDataObject = CreateReceiveLineDataObject(0m, 2m, "TST", "Test");
			var receiveLineBO = GetReceiveLineBO(receive, receiveLineDataObject);
			AssertQuantityAndUnits(receiveLineBO, "TST", 2m, 2m);

			var receiveLineDataObject1 = CreateReceiveLineDataObject(0m, 0m, "TST", "Test");
			var receiveLineBO1 = GetReceiveLineBO(receive, receiveLineDataObject1);
			AssertQuantityAndUnits(receiveLineBO1, "TST", 0m, 0m);

			var receiveLineDataObject3 = CreateReceiveLineDataObject(0m, 50m, "KG", "Kilograms");
			var receiveLineBO3 = GetReceiveLineBO(receive, receiveLineDataObject3);
			AssertQuantityAndUnits(receiveLineBO3, "KG", 2m, 50m);

			var receiveLineDataObject4 = CreateReceiveLineDataObject(0m, 2m, "BOX", "Box");
			var receiveLineBO4 = GetReceiveLineBO(receive, receiveLineDataObject4);
			AssertQuantityAndUnits(receiveLineBO4, "BOX", 10m, 2m);

			var receiveLineDataObject5 = CreateReceiveLineDataObject(0m, 0m, "BOX", "Box");
			var receiveLineBO5 = GetReceiveLineBO(receive, receiveLineDataObject5);
			AssertQuantityAndUnits(receiveLineBO5, "BOX", 0m, 0m);
		}

		WhsReceiveLine GetReceiveLineBO(WhsReceive receive, OrderLine orderLineDataObject)
		{
			var reader = new WhsReceiveLineDataObjectReader(orderLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var receiveLineBO = reader.ReadIntoBusinessObject();
			return receiveLineBO;
		}

		OrderLine CreateReceiveLineDataObject(decimal? orderedQty, decimal packageQty, string packageQtyUnit, string packageQtyUnitDesc)
		{
			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.OrderedQty = orderedQty;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.PackageQty = packageQty;
			orderLineDataObject.PackageQtyUnit = new PackageType { Code = packageQtyUnit, Description = packageQtyUnitDesc };

			return orderLineDataObject;
		}

		OrgPartUnit AddPartUnits(OrgPartUnitCollection partUnits, string packType, int quantityInParent, string parentPackType)
		{
			var partUnit = partUnits.AddNew();
			partUnit.OF_PackType = packType;
			partUnit.OF_QuantityInParent = quantityInParent;
			partUnit.OF_ParentPackType = parentPackType;
			return partUnit;
		}

		void AssertQuantityAndUnits(WhsReceiveLine receiveLineBO, string packType, decimal wEUnits, decimal wEPackQuantity)
		{
			AssertNotNull("receiveLineBO", receiveLineBO);

			CombineAssertions(delegate
			{
				AssertEquals("receiveLineBO.WE_F3_NKPackType", packType, receiveLineBO.WE_F3_NKPackType);
				AssertEquals("receiveLineBO.WE_PackQuantity", wEPackQuantity, receiveLineBO.WE_PackQuantity);
				AssertEquals("receiveLineBO.WE_TransactionQuantity", wEUnits, receiveLineBO.WE_TransactionQuantity);
			});
		}

		#endregion

		#region TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine

		public void TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var shipment = new Shipment();
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			Assert("Precondition", receive.IsInDatabase);
			AssertEquals("Precondition", 1, receive.Lines.Count);
			var matcherStub = new Mock<IWhsReceiveLineMatcher>();
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(receive, inventoryLineDataObject, inventoryLineDataObject.Product, Array.Empty<WhsReceiveLine>())).Returns(originalReceiveLine);

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				var receiveLine2 = reader.ReadIntoBusinessObject();
				AssertEquals("Should read into existing line.", originalReceiveLine, receiveLine2);
			}
		}

		#endregion

		#region TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine_ButDoesNotFindTheSameLineTwice

		public void TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine_ButDoesNotFindTheSameLineTwice()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Assert("Precondition.", receive.IsInDatabase);
			AssertEquals("Precondition.", 1, receive.Lines.Count);

			var reader2 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var newReceiveLine1 = reader2.ReadIntoBusinessObject();
			AssertEquals("Matched, so nothing added.", 1, receive.Lines.Count);
			AssertEquals("No Matched lines yet, so match with original.", originalReceiveLine, newReceiveLine1);
			Factory.SaveForTesting();

			var reader3 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, new[] { originalReceiveLine });
			var newReceiveLine2 = reader3.ReadIntoBusinessObject();
			AssertEquals("New Line Added.", 2, receive.Lines.Count);
			AssertNotEquals("Even though the line should match, the Matcher ignored it cause it was matched already.", originalReceiveLine, newReceiveLine2);
			Factory.SaveForTesting();
		}

		#endregion

		#region TestMatcherUsesProductFromExtraDetails

		public void TestMatcherUsesProductFromExtraDetails()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			Assert("Precondition.", receive.IsInDatabase);
			AssertEquals("Precondition.", 1, receive.Lines.Count);

			var data = new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
			data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } });

			var extraDetails = new ExtraOrderLineDetails(null, null, null, inventoryLineDataObject.Product.Code, null, null, null, null, null);
			inventoryLineDataObject.Product.Code = "XXX"; // make product on line invalid
			var reader2 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>(), extraDetails);

			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true))
			{
				var newReceiveLine = reader2.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("Matched, so nothing added.", 1, receive.Lines.Count);
				AssertEquals("Match using Product from extra Details.", originalReceiveLine, newReceiveLine);
			}
		}

		#endregion

		#region TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine_ParamsByWhsAndClient

		public void TestWhenReceiveIsInDatabaseLineMatcherIsUsedToFindLine_ParamsByWhsAndClient()
		{
			var receive = GetReceiveLineParent(Factory);
			Factory.SaveForTesting();

			var inventoryLineDataObject = SetupInventoryLine();
			var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Assert("Precondition.", receive.IsInDatabase);
			AssertEquals("Precondition.", 1, receive.Lines.Count);

			var reader2 = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var newReceiveLine1 = reader2.ReadIntoBusinessObject();
			AssertEquals("Matched, so nothing added.", 1, receive.Lines.Count);
			AssertEquals("No Matched lines yet, so match with original.", originalReceiveLine, newReceiveLine1);
			Factory.SaveForTesting();
		}

		#endregion

		#region TestReadOnlyFields

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForUnfinalizedReceive()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var receive = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var receiveLineBefore = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLineBefore, Logger, receive, useCleanFactory: false);
			var docketLineBizO = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Receive line must be imported into BO.", docketLineBizO);

			Factory.SaveForTesting();

			var receiveLineAfter = SetupInventoryLine();
			ChangeInventoryLineDataValues(receiveLineAfter);

			var readerLineChanged = GetNewReader(receiveLineAfter, Logger, receive, useCleanFactory: false);
			var changedReceiveLineBO = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", docketLineBizO.PK, changedReceiveLineBO.PK);
			AssertReadOnlyFields(receiveLineBefore, receiveLineAfter, changedReceiveLineBO, isCustoms: false);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForFinalizedReceive()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var receive = GetNewDocketLineParent(Factory, whs1.PK);
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			Factory.SaveForTesting();

			var receiveLineBefore = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLineBefore, Logger, receive, useCleanFactory: false);
			var receiveLineBO1 = readerLine.ReadIntoBusinessObject();
			receiveLineBO1.WE_WL = whs1.FindLocation("A-1").PK;

			AssertNotNull("Precondition: Receive line must be imported into BO.", receiveLineBO1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			Assert("Receive must be finalized", receive.IsFinalised);
			Assert("Receive line must be finalized", receiveLineBO1.IsFinalised);

			var receiveLineAfter = SetupInventoryLine();
			ChangeInventoryLineDataValues(receiveLineAfter);

			var readerLineChanged = GetNewReader(receiveLineAfter, Logger, receive, useCleanFactory: false);
			var receiveLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", receiveLineBO1.PK, receiveLineBO2.PK);
			AssertReadOnlyFields(receiveLineBefore, receiveLineAfter, receiveLineBO2, isCustoms: false);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForCancelledReceive()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var receive = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var receiveLineBefore = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLineBefore, Logger, receive, useCleanFactory: false);
			var receiveLineBO1 = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Receive line must be imported into BO.", receiveLineBO1);

			Factory.SaveForTesting();

			receive.CancelReactivateDocket();
			Assert("Receive must be cancelled", receive.IsCancelled);
			Assert("Receive Line must be cancelled", receiveLineBO1.IsDocketCancelled);

			var receiveLineAfter = SetupInventoryLine();
			ChangeInventoryLineDataValues(receiveLineAfter);

			var readerLineChanged = GetNewReader(receiveLineAfter, Logger, receive, useCleanFactory: false);
			var receiveLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", receiveLineBO1.PK, receiveLineBO2.PK);
			AssertReadOnlyFields(receiveLineBefore, receiveLineAfter, receiveLineBO2, isCustoms: false);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadOnlyFieldsForCustomsReceive()
		{
			TestDateAttribute.UseUNLOCO = true;
			var shipmentDataObject = new Shipment();
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DEC123");
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI } } });
			Logger.TopLevelDataObject = shipmentDataObject;
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var receive = GetNewDocketLineParent(Factory, whs1.PK);
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			Factory.SaveForTesting();

			var receiveLineBefore = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLineBefore, Logger, receive, useCleanFactory: false);
			var receiveLineBO1 = readerLine.ReadIntoBusinessObject();
			receiveLineBO1.WE_WL = whs1.FindLocation("A-1").PK;

			AssertNotNull("Precondition: Receive line must be imported into BO.", receiveLineBO1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			Assert("Receive must be finalized", receive.IsFinalised);
			Assert("Receive line must be finalized", receiveLineBO1.IsFinalised);

			var receiveLineAfter = SetupInventoryLine();
			ChangeInventoryLineDataValues(receiveLineAfter);

			var readerLineChanged = GetNewReader(receiveLineAfter, Logger, receive, useCleanFactory: false);
			var receiveLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", receiveLineBO1.PK, receiveLineBO2.PK);
			AssertReadOnlyFields(receiveLineBefore, receiveLineAfter, receiveLineBO2, isCustoms: true);
		}

		#endregion

		#region TestCustomsSource_ImportWhsBondedWarehouseAttribute

		public void TestCustomsSource_ImportWhsBondedWarehouseAttribute()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var receive = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var receiveLine = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLine, Logger, receive, useCleanFactory: false);
			var docketLineBizO = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Receive line must be imported into BO.", docketLineBizO);

			CombineAssertions(() => WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(docketLineBizO.CustomsData));
		}

		#endregion

		#region TestCheckProduct_BondedReceive

		public void TestCheckProduct_BondedReceive_WarehouseIsVirtualAndWithReleaseCapturedSerialNumber()
		{
			TestCheckProduct_BondedReceive_SerialNumberCore(isVirtualWarehouse: true, isBondedReceive: true, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: true);
		}

		public void TestCheckProduct_BondedReceive_WarehouseIsNotVirtualAndWithReleaseCapturedSerialNumber()
		{
			TestCheckProduct_BondedReceive_SerialNumberCore(isVirtualWarehouse: false, isBondedReceive: true, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: false);
		}

		public void TestCheckProduct_BondedReceive_WarehouseIsVirtualAndWithoutReleaseCapturedSerialNumber()
		{
			TestCheckProduct_BondedReceive_SerialNumberCore(isVirtualWarehouse: true, isBondedReceive: true, isPartAttribReleaseCaptured: false, expectedErrorsOnProduct: false);
		}

		public void TestCheckProduct_NotBondedReceive_WarehouseIsVirtualAndWithReleaseCapturedSerialNumber()
		{
			TestCheckProduct_BondedReceive_SerialNumberCore(isVirtualWarehouse: true, isBondedReceive: false, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: false);
		}

		void TestCheckProduct_BondedReceive_SerialNumberCore(bool isVirtualWarehouse, bool isBondedReceive, bool isPartAttribReleaseCaptured, bool expectedErrorsOnProduct)
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			data.Org1.MiscServ.OM_IMUseSerialNumber = isPartAttribReleaseCaptured;
			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseSerialNumber = isPartAttribReleaseCaptured;
			orgPartRelation.OU_IsSerialNumberReleaseCaptured = isPartAttribReleaseCaptured;

			factory.Save();
			var receive = GetNewDocketLineParent(Factory, data.Whs1.PK);
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_DocketSubType = isBondedReceive ? ReceiveType.Codes.Customs : ReceiveType.Codes.Receipt;
			Factory.SaveForTesting();

			var receiveLine = SetupInventoryLine();
			receiveLine.Product = new Product { Code = data.Part1.OP_PartNum, Description = "Bowler Hat" };
			var readerLine = GetNewReader(receiveLine, Logger, receive, useCleanFactory: false);
			if (expectedErrorsOnProduct)
			{
				AssertExceptionThrown("Product: Part P1 cannot be received as Bonded receive cannot be processed in a virtual warehouse: Warehouse 1 with release captured attributes.", typeof(DataObjectReadFailureException), () => readerLine.ReadIntoBusinessObject());
			}
			else
			{
				var docketLineBizO = readerLine.ReadIntoBusinessObject();
				AssertNotNull("Precondition: Receive line must be imported into BO.", docketLineBizO);
				CombineAssertions(() => WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(docketLineBizO.CustomsData));
			}
		}

		public void TestCheckProduct_BondedReceive_WarehouseIsVirtualAndWithReleaseCapturedAttributes()
		{
			TestCheckProduct_BondedReceive_PartAttribCore(isVirtualWarehouse: true, isBondedReceive: true, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: true);
		}

		public void TestCheckProduct_BondedReceive_WarehouseIsNotVirtualAndWithReleaseCapturedAttributes()
		{
			TestCheckProduct_BondedReceive_PartAttribCore(isVirtualWarehouse: false, isBondedReceive: true, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: false);
		}

		public void TestCheckProduct_BondedReceive_WarehouseIsVirtualAndWithoutReleaseCapturedAttributes()
		{
			TestCheckProduct_BondedReceive_PartAttribCore(isVirtualWarehouse: true, isBondedReceive: true, isPartAttribReleaseCaptured: false, expectedErrorsOnProduct: false);
		}

		public void TestCheckProduct_NotBondedReceive_WarehouseIsVirtualAndWithReleaseCapturedAttributes()
		{
			TestCheckProduct_BondedReceive_PartAttribCore(isVirtualWarehouse: true, isBondedReceive: false, isPartAttribReleaseCaptured: true, expectedErrorsOnProduct: false);
		}

		void TestCheckProduct_BondedReceive_PartAttribCore(bool isVirtualWarehouse, bool isBondedReceive, bool isPartAttribReleaseCaptured, bool expectedErrorsOnProduct)
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1";
			data.Org1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgPartRelation.OU_UsePartAttrib1 = isPartAttribReleaseCaptured;
			orgPartRelation.OU_IsPartAttrib1ReleaseCaptured = isPartAttribReleaseCaptured;

			factory.Save();
			var receive = GetNewDocketLineParent(Factory, data.Whs1.PK);
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_DocketSubType = isBondedReceive ? ReceiveType.Codes.Customs : ReceiveType.Codes.Receipt;
			Factory.SaveForTesting();

			var receiveLine = SetupInventoryLine();
			receiveLine.Product = new Product { Code = data.Part1.OP_PartNum, Description = "Bowler Hat" };
			var readerLine = GetNewReader(receiveLine, Logger, receive, useCleanFactory: false);
			if (expectedErrorsOnProduct)
			{
				AssertExceptionThrown("Product: Part P1 cannot be received as Bonded receive cannot be processed in a virtual warehouse: Warehouse 1 with release captured attributes.", typeof(DataObjectReadFailureException), () => readerLine.ReadIntoBusinessObject());
			}
			else
			{
				var docketLineBizO = readerLine.ReadIntoBusinessObject();
				AssertNotNull("Precondition: Receive line must be imported into BO.", docketLineBizO);
				CombineAssertions(() => WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(docketLineBizO.CustomsData));
			}
		}

		[TestDate(2024, 5, 23, 12, 34, 56)]
		public void TestCustomsSource_ImportWhsBondedWarehouseAttributeForZA()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var branch = Helper.CreateGlbBranch("BR1");
			var address = OrgAddress.New(factory);
			address.Address1 = "TEST";
			address.OA_OH = data.Org1.PK;
			address.OA_RN_NKCountryCode = CountryCodes.SouthAfrica;
			var whs = Helper.CreateWarehouse("WHS", address, branch);
			var arrivalDate = ZDateTime.Now.AddDays(-1);
			var mockDutyAndTaxCalculator = WhsBondedWarehouseAttributeReadingHelperTest.SetupMockDutyAndTaxCalculator(arrivalDate);
			var receive = GetNewDocketLineParent(Factory, whs.PK);
			receive.WD_ArrivalDate = new ZDateTimeOffset(arrivalDate);
			factory.Save();

			var receiveLine = SetupInventoryLine();
			var readerLine = GetNewReader(receiveLine, Logger, receive, useCleanFactory: false);
			var docketLineBizO = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Receive line must be imported into BO.", docketLineBizO);

			CombineAssertions(() =>
			{
				WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(docketLineBizO.CustomsData);
				AssertEquals(10m, docketLineBizO.CustomsData.WB_AllDutiesAmount);
				AssertEquals(2.3m, docketLineBizO.CustomsData.WB_VATAmount);
				mockDutyAndTaxCalculator.VerifyAll();
			});
		}

		#endregion

		#region TestASNLineCreationOnNewLinesImport

		public void TestASNLineCreationOnNewLinesImport_DocketStartedReceiving()
		{
			TestASNLineCreationOnNewLinesImportCore(receivingStarted: true);
		}

		public void TestASNLineCreationOnNewLinesImport_DocketNotStartedReceiving()
		{
			TestASNLineCreationOnNewLinesImportCore(receivingStarted: false);
		}

		void TestASNLineCreationOnNewLinesImportCore(bool receivingStarted)
		{
			var receive = GetReceiveLineParent(Factory);
			var origInventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var reader = new WhsReceiveLineDataObjectReader(origInventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			if (receivingStarted)
			{
				receive.PopulateASNLines();
			}

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has 1 line attached.", 1, receive.Lines.Count);
			AssertEquals("Precondition - StartedReceiving.", receivingStarted, receive.StartedReceiving);
			AssertEquals("Precondition - Receive has ASN lines.", receivingStarted ? 1 : 0, receive.AsnLines.Count);

			var newInventoryLineDataObject = SetupInventoryLine();
			newInventoryLineDataObject.LineNumber++;
			var reader2 = new WhsReceiveLineDataObjectReader(newInventoryLineDataObject, Logger, Factory, receive, new[] { originalReceiveLine });
			var newReceiveLine = reader2.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Receive has 2 lines attached after import.", 2, receive.Lines.Count);
			AssertEquals("New receive line's expected quantity is assigned correctly.", 22.2m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("ASN lines after import.", receivingStarted ? 2 : 0, receive.AsnLines.Count);

			if (receivingStarted)
			{
				AssertEquals("1 ASN line has WN_AddedAfterReceiveStarted = true.", true, receive.AsnLines.Cast<WhsAsnLine>().Any(asnLine => asnLine.WN_AddedAfterReceiveStarted));
				AssertEquals("New ASN line's quantity is correct.", 22.2m, receive.AsnLines.Cast<WhsAsnLine>().Single(asnLine => asnLine.WN_AddedAfterReceiveStarted).WN_Quantity);
				AssertEquals("1 ASN line has WN_AddedAfterReceiveStarted = false.", true, receive.AsnLines.Cast<WhsAsnLine>().Any(asnLine => !asnLine.WN_AddedAfterReceiveStarted));
			}
		}

		public void TestASNLineCreationOnNewLinesImport_SameLineNumber()
		{
			var receive = GetReceiveLineParent(Factory);
			var origInventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var reader = new WhsReceiveLineDataObjectReader(origInventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			receive.PopulateASNLines();

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has 1 line attached.", 1, receive.Lines.Count);
			AssertEquals("Precondition - StartedReceiving.", true, receive.StartedReceiving);
			AssertEquals("Precondition - Receive has ASN lines.", 1, receive.AsnLines.Count);

			var newInventoryLineDataObject = SetupInventoryLine();
			var reader2 = new WhsReceiveLineDataObjectReader(newInventoryLineDataObject, Logger, Factory, receive, new[] { originalReceiveLine });
			var newReceiveLine = reader2.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Receive lines have the same line number.", newReceiveLine.WE_LineNo, originalReceiveLine.WE_LineNo);
			AssertEquals("Receive has 2 lines attached after import.", 2, receive.Lines.Count);
			AssertEquals("New receive line's expected quantity is assigned correctly.", 22.2m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("ASN lines after import.", 2, receive.AsnLines.Count);
		}

		public void TestASNLineCreationOnNewLinesImport_ReImportingNewLine()
		{
			var receive = GetReceiveLineParent(Factory);
			var origInventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var reader = new WhsReceiveLineDataObjectReader(origInventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			receive.PopulateASNLines();

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has 1 line attached.", 1, receive.Lines.Count);
			AssertEquals("Precondition - StartedReceiving.", true, receive.StartedReceiving);
			AssertEquals("Precondition - Receive has ASN lines.", 1, receive.AsnLines.Count);

			var newInventoryLineDataObject = SetupInventoryLine();
			newInventoryLineDataObject.LineNumber++;
			var reader2 = new WhsReceiveLineDataObjectReader(newInventoryLineDataObject, Logger, Factory, receive, new[] { originalReceiveLine });
			var newReceiveLine = reader2.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Receive has 2 lines attached after import.", 2, receive.Lines.Count);
			AssertEquals("New receive line's expected quantity is assigned correctly.", 22.2m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("ASN lines after import.", 2, receive.AsnLines.Count);

			AssertExceptionThrown("Cannot update Receive Line after receiving started.", typeof(DataObjectReadFailureException), () => reader2.ReadIntoBusinessObject());
		}

		public void TestASNLineCreationOnNewLinesImport_DocketStartedReceiving_SameLineNumber()
		{
			var receive = GetReceiveLineParent(Factory);
			var origInventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var reader = new WhsReceiveLineDataObjectReader(origInventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var originalReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			receive.PopulateASNLines();

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has 1 line attached.", 1, receive.Lines.Count);
			AssertEquals("Precondition - StartedReceiving.", true, receive.StartedReceiving);
			AssertEquals("Precondition - Receive has ASN lines.", 1, receive.AsnLines.Count);

			var newInventoryLineDataObject = SetupInventoryLine();
			var reader2 = new WhsReceiveLineDataObjectReader(newInventoryLineDataObject, Logger, Factory, receive, new[] { originalReceiveLine });
			var newReceiveLine = reader2.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Receive lines have the same line number.", newReceiveLine.WE_LineNo, originalReceiveLine.WE_LineNo);
			AssertEquals("Receive has 2 lines attached after import.", 2, receive.Lines.Count);
			AssertEquals("New receive line's expected quantity is assigned correctly.", 22.2m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("ASN lines after import.", 2, receive.AsnLines.Count);

			AssertExceptionThrown("Cannot update Receive Line after receiving started.", typeof(DataObjectReadFailureException), () => reader2.ReadIntoBusinessObject());
		}

		#endregion

		#region TestTransactionQuantity

		public void TestTransactionQuantity_ReceiveLoadedInRF()
		{
			var receive = GetReceiveLineParent(Factory);
			var origInventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receive.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Today);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.SaveForTesting();

			Assert("Precondition - Receive is saved in database.", receive.IsInDatabase);
			AssertEquals("Precondition - Receive has RF logs.", true, receive.Logs.Find(log => log.SL_Reference.StartsWith("RF", System.StringComparison.InvariantCultureIgnoreCase)).Any());

			var newInventoryLineDataObject = SetupInventoryLine();
			AssertEquals("Precondition - data object's ordered qty is > 0.", true, newInventoryLineDataObject.OrderedQty > 0);
			var reader = new WhsReceiveLineDataObjectReader(newInventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
			var newReceiveLine = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Imported new receive line's transaction qty is 0.", 0m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals("Imported new receive line's stock on hand is 0.", 0m, newReceiveLine.WE_StockOnHand);
			AssertEquals("Imported new receive line's client ordered units is not 0.", 22.2m, newReceiveLine.WE_ClientOrderedUnits);
			AssertEquals("New ASN line's quantity should not be 0.", false, receive.AsnLines.Cast<WhsAsnLine>().Any(asnLine => asnLine.WN_Quantity == 0));
		}

		#endregion

		#region TestUsesCorrectMatcher

		public void TestUsesCorrectMatcher()
		{
			var receive = GetReceiveLineParent(Factory);
			var inventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var matcherStub = new Mock<IWhsReceiveLineMatcher>();
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<Product>(), It.IsAny<IEnumerable<WhsReceiveLine>>())).Returns((WhsReceiveLine)null).Verifiable();
			matcherStub.Setup(stub => stub.FindExistingBizOForNonCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<IEnumerable<WhsReceiveLine>>())).Returns((WhsReceiveLine)null).Verifiable();

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
				reader.ReadIntoBusinessObject();
				matcherStub.Verify(s => s.FindExistingBizOForCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<Product>(), It.IsAny<IEnumerable<WhsReceiveLine>>()), Times.Never);
				matcherStub.Verify(s => s.FindExistingBizOForNonCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<IEnumerable<WhsReceiveLine>>()));
			}
			Assert(true);
		}

		public void TestUsesCorrectMatcherForCustoms()
		{
			var matcherStub = new Mock<IWhsReceiveLineMatcher>();
			matcherStub.Setup(stub => stub.FindExistingBizOForCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<Product>(), It.IsAny<IEnumerable<WhsReceiveLine>>())).Returns((WhsReceiveLine)null).Verifiable();
			matcherStub.Setup(stub => stub.FindExistingBizOForNonCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<IEnumerable<WhsReceiveLine>>())).Returns((WhsReceiveLine)null).Verifiable();

			var receive = GetReceiveLineParent(Factory);
			var inventoryLineDataObject = SetupInventoryLine();
			Factory.SaveForTesting();

			var shipment = new Shipment();
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute(matcherStub.Object))
			{
				var reader = new WhsReceiveLineDataObjectReader(inventoryLineDataObject, Logger, Factory, receive, Array.Empty<WhsReceiveLine>());
				reader.ReadIntoBusinessObject();

				matcherStub.Verify(s => s.FindExistingBizOForCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<Product>(), It.IsAny<IEnumerable<WhsReceiveLine>>()));
				matcherStub.Verify(s => s.FindExistingBizOForNonCustoms(It.IsAny<WhsReceive>(), It.IsAny<OrderLine>(), It.IsAny<IEnumerable<WhsReceiveLine>>()), Times.Never);
			}
			Assert(true);
		}

		#endregion

		#region Implementation

		internal static OrderLine SetupInventoryLine(bool includeManufacturer = false)
		{
			var inventoryLineDataObject = new OrderLine();
			inventoryLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			inventoryLineDataObject.Consignee = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeAddress));
			inventoryLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo(includeManufacturer);
			inventoryLineDataObject.CrossDockOrderNumber = "ORDERME";
			inventoryLineDataObject.ExpectedQuantity = 22.2m;
			inventoryLineDataObject.ExpiryDate = new ZDate(2011, 1, 2);
			inventoryLineDataObject.LineNumber = new ZShort(3);
			inventoryLineDataObject.OrderedQty = 22.2m;
			inventoryLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			inventoryLineDataObject.PackageQty = 33.3m;
			inventoryLineDataObject.PackageQtyUnit = new PackageType { Code = "CTN", Description = "Carton" };
			inventoryLineDataObject.PackingDate = new ZDate(2011, 1, 3);
			inventoryLineDataObject.PalletID = "PALLET~1";
			inventoryLineDataObject.PartAttribute1 = "Colour";
			inventoryLineDataObject.PartAttribute2 = "Size";
			inventoryLineDataObject.PartAttribute3 = "Batch Number";
			inventoryLineDataObject.PerPackageQty = 0.2m;
			inventoryLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			inventoryLineDataObject.PutAwayArea = "RARA";
			inventoryLineDataObject.RequiredBy = new ZDateTimeOffset(2011, 1, 4);
			inventoryLineDataObject.ReservedQuantity = 44.4m;
			inventoryLineDataObject.SplitQuantity = 55.5m;
			inventoryLineDataObject.Status = new CodeDescriptionPair { Code = "PND", Description = "Pending Receipt" };
			inventoryLineDataObject.SubLineNumber = new ZShort(6);

			return inventoryLineDataObject;
		}

		internal static void ChangeInventoryLineDataValues(OrderLine inventoryLineDataObject)
		{
			inventoryLineDataObject.CrossDockOrderNumber = "ChangedORDNUM";
			inventoryLineDataObject.ExpiryDate = new ZDateTime(2015, 12, 31);
			inventoryLineDataObject.LineComment = "ChangedComment";
			inventoryLineDataObject.OrderedQty = 22.2m;
			inventoryLineDataObject.PackageQty = 30.3m;
			inventoryLineDataObject.PackageQtyUnit = new PackageType { Code = "BOX", Description = "Box" };
			inventoryLineDataObject.PackingDate = new ZDateTime(2015, 11, 30);
			inventoryLineDataObject.PalletID = "PALLET_CHG";
			inventoryLineDataObject.PartAttribute1 = "Changed01";
			inventoryLineDataObject.PartAttribute2 = "Changed02";
			inventoryLineDataObject.PartAttribute3 = "Changed03";
			inventoryLineDataObject.SerialNumber = "Changed04";
			inventoryLineDataObject.RequiredBy = new ZDateTimeOffset(2015, 10, 31);
			inventoryLineDataObject.ReservedQuantity = 40.4m;
			inventoryLineDataObject.SplitQuantity = 50.5m;
		}

		internal static void AssertContents(WhsInventoryView inventory, bool includeManufacturerAddress = false)
		{
			AssertEquals("inventoryLineBO.WE_BondedEntryKey should be taken from CustomsInfo.EntryNum, not Inwards. Inwards is used for Orders.", "KEYZOR-5", inventory.WI_BondedEntryKey);
			AssertEquals("inventoryLineBO.WI_CrossDockQuantity", 0m, inventory.WI_CrossDockQuantity);
			AssertEquals("inventoryLineBO.WI_ExpectedReceiptQuantity", 22.2m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("inventoryLineBO.WI_ExpiryDate", new ZDateTime(2011, 1, 2), inventory.WI_ExpiryDate);
			AssertEquals("inventoryLineBO.WI_F3_NKPackType", "CTN", inventory.WI_F3_NKPackType);
			AssertEquals("inventoryLineBO.WI_InDocketLineUnits", 22.2m, inventory.WI_InDocketLineUnits);
			AssertEquals("inventoryLineBO.WI_InventoryStatus", "PND", inventory.WI_InventoryStatus);
			AssertEquals("inventoryLineBO.WI_LineNo", new ZShort(3), inventory.WI_LineNo);
			AssertEquals("inventoryLineBO.WI_OP_PartNum", "BOWLHAT", inventory.WI_OP_PartNum);
			AssertEquals("inventoryLineBO.WI_OP_Desc", "Bowler Hat", inventory.WI_OP_Desc);
			AssertEquals("inventoryLineBO.WI_PackingDate", new ZDateTime(2011, 1, 3), inventory.WI_PackingDate);
			AssertEquals("inventoryLineBO.InDocketLine.WE_PackQuantity", 22.2m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("inventoryLineBO.WI_PalletID", "PALLET~1", inventory.WI_PalletID);
			AssertEquals("inventoryLineBO.WI_PartAttrib1", "Colour", inventory.WI_PartAttrib1);
			AssertEquals("inventoryLineBO.WI_PartAttrib2", "Size", inventory.WI_PartAttrib2);
			AssertEquals("inventoryLineBO.WI_PartAttrib3", "Batch Number", inventory.WI_PartAttrib3);
			AssertEquals("inventoryLineBO.PerPackageQty", 0.2m, inventory.PerPackageQty);
			AssertEquals("inventoryLineBO.WI_ReceiveCrossDockOrderNo", "ORDERME", inventory.WI_ReceiveCrossDockOrderNo);
			AssertEquals("inventoryLineBO.WI_SplitQuantity", 0m, inventory.WI_SplitQuantity);
			AssertEquals("inventoryLineBO.WI_SubLineNo", new ZShort(6), inventory.WI_SubLineNo);
			AssertEquals("inventoryLineBO.WI_UnitsUQ", "UNT", inventory.WI_UnitsUQ);
			AssertEquals("inventoryLineBO.CommodityCode", "CMM", inventory.CommodityCode);

			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			receiveLine.ConsigneeDocAddress.Requirement.GetRegistrationNumberResult = (JobDocAddress docAddress) =>
			{
				return new RegistrationNumberResult(docAddress.Factory, true, () =>
				{
					return new MasterFiles.Business.RegistrationNumber { Number = "TAXME", NumberType = "SAM" };
				});
			};
			AssertJobDocAddressContentMatches_WUFSHIJNB(receiveLine.ConsigneeDocAddress);

			var customsData = inventory.CustomsData;
			WhsBondedWarehouseAttributeReadingHelperTest.AssertContents(customsData, includeManufacturerAddress);
		}

		void AssertReadOnlyFields(OrderLine originalDataObject, OrderLine changedDataObject, WhsReceiveLine modifiedDocketLine, bool isCustoms)
		{
			CombineAssertions("Following assertions failed for receipt line specific fields:", () =>
			{
				AssertReadOnlyFieldIsUnchanged(originalDataObject.CrossDockOrderNumber, changedDataObject.CrossDockOrderNumber, modifiedDocketLine.WE_ReceiveCrossDockOrderNoInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.ExpiryDate, changedDataObject.ExpiryDate, modifiedDocketLine.WE_ExpiryDateInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.OrderedQty, changedDataObject.OrderedQty, modifiedDocketLine.WE_TransactionQuantityInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PackageQtyUnit.Code, changedDataObject.PackageQtyUnit.Code, modifiedDocketLine.WE_F3_NKPackTypeInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PackingDate, changedDataObject.PackingDate, modifiedDocketLine.WE_PackingDateInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PalletID, changedDataObject.PalletID, modifiedDocketLine.WE_PalletIDInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute1, changedDataObject.PartAttribute1, modifiedDocketLine.WE_PartAttrib1Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute2, changedDataObject.PartAttribute2, modifiedDocketLine.WE_PartAttrib2Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute3, changedDataObject.PartAttribute3, modifiedDocketLine.WE_PartAttrib3Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.SerialNumber, changedDataObject.SerialNumber, modifiedDocketLine.WE_SerialNumberInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.RequiredBy, changedDataObject.RequiredBy, modifiedDocketLine.WE_RequiredByDateInfo, isCustoms);
			});
		}

		internal static WhsReceive GetReceiveLineParent(UniversalObjectFactory factory)
		{
			var whsReceive = factory.NewWithValidTestData<WhsReceive>();
			whsReceive.WD_OH_Client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), factory).GetMatchedOrNewForTesting().Header.PK;
			whsReceive.WD_DocketSubType = "CUS";

			var warehouse = whsReceive.Warehouse;
			var area = warehouse.Areas.AddNew();
			area.WA_Name = "RARA";

			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "BOWLHAT";
			part.OP_Desc = "Bowler Hat";
			part.OP_StockKeepingUnit = "UNT";
			part.OP_RH_NKCommodityCode = "CMM";

			var owner = part.RelatedOrganisations.AddNew();
			owner.OU_OH = whsReceive.WD_OH_Client;
			owner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			owner.OU_OP = part.PK;

			return whsReceive;
		}

		void EnableAllAttributeUse(OrgHeader client, string partNum)
		{
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNum));
			AssertNotNull($"Could not load product from database: {partNum}", part);
			Helper.SetClientAllAttributeType(client, false);
			Helper.SetProductAllAttributeUse(client, part, true);
		}

		protected override string GetDocketType()
		{
			return "Receipt";
		}

		protected override WhsReceive GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			return Helper.CreateWhsReceive(client, warehouse);
		}

		protected override WhsReceiveLineDataObjectReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, WhsReceive parent, bool useCleanFactory = true)
		{
			var cleanFactory = new UniversalObjectFactory();
			var parentInCleanFactory = useCleanFactory ? cleanFactory.Load<WhsReceive>(parent.PK) : parent;
			return new WhsReceiveLineDataObjectReader(docketLineDataObject, logger, useCleanFactory ? cleanFactory : Factory, parentInCleanFactory, Array.Empty<WhsReceiveLine>());
		}

		protected override bool SupportsProductCreation => true;

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
