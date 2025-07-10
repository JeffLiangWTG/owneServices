using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedTransactionsReportTest : WhsTestCaseWithFactory
	{
		#region TestOP_PartNum_IsNotTruncated

		public void TestOP_PartNum_IsNotTruncated()
		{
			var testData = new TestDataForBondedEntries(Factory, processBondedInwardDuringConstruction: true);
			testData.Part1.OP_PartNum = "12345678901234567890123456789012345";

			Factory.Save();
			var results = LoadView();
			AssertEquals(3, results.Count);
		}

		#endregion

		#region TestFunction_ReleaseCapturedAttributes

		[TestDate(2022, 02, 03, 00, 00, 00)]
		public void TestFunction_ReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "ENTRY-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receive.WD_FinalisedDate = inventory.InDocketLine.WE_FinalisedDate;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "ENTRY-123", "DummyOutwards");
			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.PartAttribute3 = "PLASTIC";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute3 = "METAL";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			orderLine.Order.WD_FinalisedDate = orderLine.WE_FinalisedDate;
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);
			AssertLineMatch(results, inventory.InDocketLine, 10m, "", "BATCH123", "", "");
			AssertLineMatch(results, orderLine, -6m, "RED", "BATCH123", "PLASTIC", "");
			AssertLineMatch(results, orderLine, -4m, "BLUE", "BATCH123", "METAL", "");

			results = LoadView("");
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);
			AssertLineMatch(results, inventory.InDocketLine, 10m, "", "BATCH123", "", "");
			AssertLineMatch(results, orderLine, -6m, "RED", "BATCH123", "PLASTIC", "");
			AssertLineMatch(results, orderLine, -4m, "BLUE", "BATCH123", "METAL", "");

			results = LoadView("E");
			AssertEquals("There should be two lines as we specified filter by part attribute.", 2, results.Count);
			AssertLineMatch(results, orderLine, -6m, "RED", "BATCH123", "PLASTIC", "");
			AssertLineMatch(results, orderLine, -4m, "BLUE", "BATCH123", "METAL", "");

			results = LoadView("R");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine, -6m, "RED", "BATCH123", "PLASTIC", "");

			results = LoadView("M");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine, -4m, "BLUE", "BATCH123", "METAL", "");
		}

		[TestDate(2022, 02, 03, 00, 00, 00)]
		public void TestFunction_ReleaseCapturedAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "", "", "ENTRY-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receive.WD_FinalisedDate = inventory.InDocketLine.WE_FinalisedDate;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.SerialNumber = "SSERNT";

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.SerialNumber = "S89";

			var releaseLine3 = orderLine3.ReleaseLines[0];
			releaseLine3.PartAttribute1 = "GREEN";
			releaseLine3.SerialNumber = "HYT";

			var releaseLine4 = orderLine4.ReleaseLines[0];
			releaseLine4.PartAttribute1 = "PURPLE";
			releaseLine4.SerialNumber = "L28";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			Factory.Save();

			var results = LoadView();
			AssertEquals("Should be 5 results, 1 receive Line + 4 Release Captured Attributes.", 5, results.Count);
			AssertLineMatch(results, inventory.InDocketLine, 10m, "", "", "", "");
			AssertLineMatch(results, orderLine1, -1m, "RED", "", "", "SSERNT");
			AssertLineMatch(results, orderLine2, -1m, "BLUE", "", "", "S89");
			AssertLineMatch(results, orderLine3, -1m, "GREEN", "", "", "HYT");
			AssertLineMatch(results, orderLine4, -1m, "PURPLE", "", "", "L28");

			results = LoadView("");
			AssertEquals("Should be 5 results, 1 receive Line + 4 Release Captured Attributes.", 5, results.Count);
			AssertLineMatch(results, inventory.InDocketLine, 10m, "", "", "", "");
			AssertLineMatch(results, orderLine1, -1m, "RED", "", "", "SSERNT");
			AssertLineMatch(results, orderLine2, -1m, "BLUE", "", "", "S89");
			AssertLineMatch(results, orderLine3, -1m, "GREEN", "", "", "HYT");
			AssertLineMatch(results, orderLine4, -1m, "PURPLE", "", "", "L28");

			results = LoadView("SERN");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine1, -1m, "RED", "", "", "SSERNT");

			results = LoadView("S");
			AssertEquals("There should be two lines as we specified filter by part attribute.", 2, results.Count);
			AssertLineMatch(results, orderLine1, -1m, "RED", "", "", "SSERNT");
			AssertLineMatch(results, orderLine2, -1m, "BLUE", "", "", "S89");

			results = LoadView("HY");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine3, -1m, "GREEN", "", "", "HYT");

			results = LoadView("T");
			AssertEquals("There should be two lines as we specified filter by part attribute.", 2, results.Count);
			AssertLineMatch(results, orderLine3, -1m, "GREEN", "", "", "HYT");
			AssertLineMatch(results, orderLine1, -1m, "RED", "", "", "SSERNT");

			results = LoadView("89");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine2, -1m, "BLUE", "", "", "S89");

			results = LoadView("PURPLE");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, orderLine4, -1m, "PURPLE", "", "", "L28");
		}

		void AssertLineMatch(DynamicBusinessObjectCollection results, WhsDocketLine docketLine, ZDecimal expectedQty,
			ZString expectedPartAttrib1, ZString expectedPartAttrib2, ZString expectedPartAttrib3,
			ZString expectedSerialNumber)
		{
			var row = results.Single(r =>
				(ZDecimal)r["RelativeInvoiceQty"] == expectedQty &&
				(ZString)r["SerialNumberValue"] == expectedSerialNumber);
			var docket = docketLine.Docket;

			var isOrder = docket.WD_DocketType == DocketType.Codes.Order;
			var expectedType = isOrder ? "OUTWARDS" : "INWARDS";

			var expectedInwardsEntry =
				new EntryNumber(docketLine.CustomsData.WB_EntryKey, docketLine.CustomsData.WB_EntryLineNo);
			var expectedOutwardsEntryNo = ZString.Empty;
			var expectedOutwardsEntryLineNo = ZShort.Zero;

			if (isOrder)
			{
				expectedInwardsEntry = WhsBondedWarehouseAttribute.BreakUpKey(docketLine.WE_BondedEntryKey);
				expectedOutwardsEntryNo = docketLine.CustomsData.WB_EntryKey;
				expectedOutwardsEntryLineNo = docketLine.CustomsData.WB_EntryLineNo;
			}

			AssertEquals("InwardsEntryNo", expectedInwardsEntry.EntryKey, row["InwardsEntryNo"]);
			AssertEquals("InwardsEntryLineNo", expectedInwardsEntry.EntryLineNo, row["InwardsEntryLineNo"]);
			AssertEquals("OutwardsEntryNo", expectedOutwardsEntryNo, row["OutwardsEntryNo"]);
			AssertEquals("OutwardsEntryLineNo", expectedOutwardsEntryLineNo, row["OutwardsEntryLineNo"]);
			AssertEquals("ProdCode", docketLine.ProductCode, row["ProdCode"]);
			AssertEquals("ProdDesc", docketLine.ProductDesc, row["ProdDesc"]);
			AssertEquals("Type", expectedType, row["Type"]);

			var expectedEntryDate = docketLine.WE_FinalisedDate;
			AssertEquals("EntryDate", expectedEntryDate.ToString("dd-MMM-yyyy HH:mm"), ((ZDateTime)row["EntryDate"]).ToString("dd-MMM-yyyy HH:mm"));
			AssertEquals("InvoiceQty", Math.Abs(expectedQty), row["InvoiceQty"]);
			AssertEquals("PartAttrib1", expectedPartAttrib1.IsEmpty ? "" : "Attribute 1: " + expectedPartAttrib1,
				row["PartAttrib1"]);
			AssertEquals("PartAttrib2", expectedPartAttrib2.IsEmpty ? "" : "Attribute 2: " + expectedPartAttrib2,
				row["PartAttrib2"]);
			AssertEquals("PartAttrib3", expectedPartAttrib3.IsEmpty ? "" : "Attribute 3: " + expectedPartAttrib3,
				row["PartAttrib3"]);
			AssertEquals("PartAttrib1", expectedPartAttrib1, row["PartAttrib1Value"]);
			AssertEquals("PartAttrib2", expectedPartAttrib2, row["PartAttrib2Value"]);
			AssertEquals("PartAttrib3", expectedPartAttrib3, row["PartAttrib3Value"]);
			AssertEquals("SerialNumber", expectedSerialNumber, row["SerialNumberValue"]);
			AssertEquals("WarehousePK", docket.WD_WW_Whs, row["WarehousePK"]);
			AssertEquals("ClientPK", docket.WD_OH_Client, row["ClientPK"]);
			AssertEquals("ProductPK", docketLine.WE_OP, row["ProductPK"]);
			AssertEquals("StockOnHand", docketLine.WE_StockOnHand, row["StockOnHand"]);
			AssertEquals("ClientName", docket.Client.OH_FullName, row["ClientName"]);
			AssertEquals("WarehouseCode", docket.Warehouse.WW_WarehouseCode, row["WarehouseCode"]);
		}

		#endregion

		#region TestWarehouseAddressCanBeUsedToFindTransactions

		public void TestWarehouseAddressCanBeUsedToFindTransactions()
		{
			var whsOrg1 = Helper.CreateClient("C1");
			var whsOrg2 = Helper.CreateClient("C2");
			var client = Helper.CreateClient("C3");

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "1 BOND ST";
			address1.OA_OH = whsOrg1.PK;

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "2 BOND ST";
			address2.OA_OH = whsOrg2.PK;

			var whs1 = Helper.CreateWarehouse("WH1");
			whs1.WW_OA_WarehouseAddress = address1.PK;
			Helper.EnableWarehouseForBond(whs1, true);

			var whs2 = Helper.CreateWarehouse("WH2");
			whs2.WW_OA_WarehouseAddress = address2.PK;
			Helper.EnableWarehouseForBond(whs2, true);

			var product = Helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceive(client, whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, whs1.DefaultLocation);
			receive.RunPreSaveValidation();
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = inventory.InDocketLine;

			var bondedAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			bondedAttribute.WB_EntryKey = "ABC123";
			bondedAttribute.SetParent(receiveLine);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);

			var parameterForAddress1 = ZSqlParameter.New("@WarehouseAddressPK", address1.PK,
				WhsWarehouseSchema.WW_OA_WarehouseAddress);
			result.Load(
				"select * from WhsBondedTransactionsReport(@WarehouseAddressPK, null, null, null, null, null, null, null, null, null, null, null ,null, null, null)",
				new[] { parameterForAddress1 });
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			var parameterForAddress2 = ZSqlParameter.New("@WarehouseAddressPK", address2.PK,
				WhsWarehouseSchema.WW_OA_WarehouseAddress);
			result.Load(
				"select * from WhsBondedTransactionsReport(@WarehouseAddressPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null)",
				new[] { parameterForAddress2 });
			AssertEquals("There should be no lines as we specified address of a different warehouse.", 0, result.Count);

			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");
			AssertEquals("There should be one lines as we didn't specified filter by address.", 1, result.Count);
		}

		#endregion

		#region TestOnlyOrdersAreShowsAsOutwards

		public void TestOnlyOrdersAreShowsAsOutwards()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON");
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			adjustmentOut.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -4m, data.Whs1.DefaultLocation, bondedInwardsKey,
				"", 0);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOut);

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			adjustmentIn.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 3m, data.Whs1.DefaultLocation, bondedInwardsKey,
				"", 0);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentIn);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, bondedInwardsKey, "DummyOutwards");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_CustomerReference = "TestOutwardsEntry#";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null ,null, null, null)");

			AssertEquals(4, result.Count);

			AssertEquals("There should be 3 inwards rows: 1 receive + 1 adjustment in + 1 adjustment out.", 3,
				result.Count(row => (ZString)row["Type"] == "INWARDS"));
			Assert("For all Inwards rows Inwards EntryNo must be specified.",
				result.Where(row => (ZString)row["Type"] == "INWARDS")
					.All(row => (ZString)row["InwardsEntryNo"] == bondedInwardsKey));
			Assert("For all Inwards rows Outwards EntryNo must NOT be specified.",
				result.Where(row => (ZString)row["Type"] == "INWARDS")
					.All(row => (ZString)row["OutwardsEntryNo"] == ""));

			AssertEquals(1, result.Count(row => (ZString)row["Type"] == "OUTWARDS"));
			var outwardsRow = result.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertEquals("Only OrderLine should be OUTWARDS.", -5m, outwardsRow["RelativeInvoiceQty"]);
			AssertEquals("Inwards EntryNo must not be empty for Outwards rows.", bondedInwardsKey,
				outwardsRow["InwardsEntryNo"]);
			AssertEquals("Outwards EntryNo must be non-empty for Outwards rows.", "DUMMYOUTWARDS",
				outwardsRow["OutwardsEntryNo"]);
		}

		#endregion

		#region TestInternalTransfersAreExcludedFromTheReport

		public void TestInternalTransfersAreExcludedFromTheReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON");
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1,
				10m, bondedInwardsKey, data.Whs1.DefaultLocation, "PLT-123", false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var tranferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation, bondedInwardsKey);
			tranferLine.WE_TransferFromPalletId = "PLT-123";
			tranferLine.WE_PalletID = "PLT-123";
			tranferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is fully committed.", 10m,
				tranferLine.GetQtyCommittedIncludingMatchingLines());

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.Lines[0].WE_BondedEntryKey = bondedInwardsKey;
			order.Lines[0].CustomsData.WB_EntryKey = bondedInwardsKey;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");

			AssertEquals(2, result.Count);

			AssertEquals("There should be 1 inwards row: 1 receive.", 1,
				result.Count(row => (ZString)row["Type"] == "INWARDS"));
			AssertEquals("Inwards RelativeInvoiceQty", 10m,
				result.Single(row => (ZString)row["Type"] == "INWARDS")["RelativeInvoiceQty"]);
			AssertEquals("There should be 1 outwards row: 1 order.", 1,
				result.Count(row => (ZString)row["Type"] == "OUTWARDS"));
			AssertEquals("Outwards RelativeInvoiceQty", -10m,
				result.Single(row => (ZString)row["Type"] == "OUTWARDS")["RelativeInvoiceQty"]);
		}

		#endregion

		#region Test_InterWarehouseSourceTransfersAreNegativeAndOutwards_InterWarehouseDestinationTransfersArePositiveAndInwards

		public void
			Test_InterWarehouseSourceTransfersAreNegativeAndOutwards_InterWarehouseDestinationTransfersArePositiveAndInwards()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("2", "A", 1, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1,
				10m, data.Whs1.DefaultLocation, "PLT-123", false, false);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m,
				data.Whs1.DefaultLocation.WLV_LocationString, "PLT-123", warehouse2.PK,
				warehouse2.DefaultLocation.WLV_LocationString, "PLT-123", ZDateTimeOffset.Today);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 10m,
				transferLine.GetQtyCommittedIncludingMatchingLines());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			// It's not normally possible to have these records on non bonded records
			// but just to be robust in case InterWarehouse transfers were created somehow.
			new CargoWise.Database.TestFramework.ObjectModel.WhsBondedWarehouseAttribute(transferLine.PK.ToGuid(), "WE")
			{
				WB_CustomsQty = 10.1m,
				WB_CustomsSecondQuantity = 11.2m,
				WB_CustomsThirdQuantity = 12.3m,
				WB_ValueForDuty = 14
			}.Insert(TestConnection);
			new CargoWise.Database.TestFramework.ObjectModel.WhsBondedWarehouseAttribute(transferLine.ChildTransferLine.PK.ToGuid(), "WE")
			{
				WB_CustomsQty = 0.1m,
				WB_CustomsSecondQuantity = 1.2m,
				WB_CustomsThirdQuantity = 2.3m,
				WB_ValueForDuty = 4
			}.Insert(TestConnection);

			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");

			AssertEquals(2, result.Count);

			AssertEquals("There should be 1 inwards row: 1 inter-warehouse dest transfer.", 1,
				result.Count(row => (ZString)row["Type"] == "INWARDS"));
			AssertEquals("The Dest Transfer should be Positive.", 1,
				result
					.Where(row => (ZString)row["Type"] == "INWARDS")
					.Count(row => (ZDecimal)row["RelativeInvoiceQty"] == 10m
								  && (ZDecimal)row["RelativeCustomsQty"] == 0.1m
								  && (ZDecimal)row["RelativeSecondCustomsQty"] == 1.2m
								  && (ZDecimal)row["RelativeThirdCustomsQty"] == 2.3m
								  && (ZDecimal)row["RelativeValueForDuty"] == 4m));
			AssertEquals("There should be 1 outwards row: 1 inter-warehouse source transfer.", 1,
				result.Count(row => (ZString)row["Type"] == "OUTWARDS"));
			AssertEquals("The Source Transfer should be Negative.", 1,
				result
					.Where(row => (ZString)row["Type"] == "OUTWARDS")
					.Count(row => (ZDecimal)row["RelativeInvoiceQty"] == -10m
								  && (ZDecimal)row["RelativeCustomsQty"] == -10.1m
								  && (ZDecimal)row["RelativeSecondCustomsQty"] == -11.2m
								  && (ZDecimal)row["RelativeThirdCustomsQty"] == -12.3m
								  && (ZDecimal)row["RelativeValueForDuty"] == -14m));
		}

		#endregion

		#region TestCustomsAdjustmentSubTypeWillDisplayAdjustmentReason

		public void TestCustomsAdjustmentSubTypeWillDisplayAdjustmentReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var adjustmentCUS = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN_CUS");
			adjustmentCUS.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLineCUS = Helper.CreateWhsAdjustmentLine(adjustmentCUS, data.Part1, 3m,
				data.Whs1.DefaultLocation, bondedInwardsKey, "", 0);
			adjustmentLineCUS.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.CustomsAmendment;
			adjustmentCUS.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentCUS);

			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");

			AssertEquals(2, result.Count);
			var adjCUSRow = result.Single(row => (ZDecimal)row["RelativeInvoiceQty"] == 3m);
			AssertEquals("For automatic adjustments we display reason code.", "AMD", adjCUSRow["ReasonCode"]);
		}

		#endregion

		#region TestCalculatedRatioForOrders

		public void TestCalculatedRatioForOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 80m, 40m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 5m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receive and 1 for the order.", 2, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 40m, expectedCustomsQty: 20m,
				expectedRelValueForDuty: -40m, expectedRelCustomsQty: -20m,
				message:
				"Value should be calculated based on released units over inventory quantity ratio, not from the WhsBondedWarehouseAttribute values linked to it.");

			var receiveResult = results.Single(row => (ZString)row["Type"] == "INWARDS");
			AssertCalculatedValueFromRatio(receiveResult, expectedValueForDuty: 80m, expectedCustomsQty: 40m,
				expectedRelValueForDuty: 80m, expectedRelCustomsQty: 40m,
				message:
				"Value should not be a calculated value and values are from the WhsBondedWarehouseAttribute linked to it.");
		}

		public void TestCalculatedRatioForOrders_WithOutboundTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 80m, 40m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 5m,
				bondedInwardsKey, 999m, 999m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receive and 1 for the order.", 2, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 40m, expectedCustomsQty: 20m,
				expectedRelValueForDuty: -40m, expectedRelCustomsQty: -20m,
				message:
				"Value should be calculated based on released units over inventory quantity ratio, not from the WhsBondedWarehouseAttribute values linked to it.");

			var receiveResult = results.Single(row => (ZString)row["Type"] == "INWARDS");
			AssertCalculatedValueFromRatio(receiveResult, expectedValueForDuty: 80m, expectedCustomsQty: 40m,
				expectedRelValueForDuty: 80m, expectedRelCustomsQty: 40m,
				message:
				"Value should not be a calculated value and values are from the WhsBondedWarehouseAttribute linked to it.");
		}

		public void TestCalculatedRatioForOrders_MultiplePickLinesAndInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 3 results, 2 for receives and 1 for the order.", 3, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 120m, expectedCustomsQty: 480m,
				expectedRelValueForDuty: -120m, expectedRelCustomsQty: -480m,
				message: "Value should be calculated based on released units over inventory quantity ratio.");
		}

		public void
			TestCalculatedRatioForOrders_MultiplePickLinesAndInventories_DifferentValueForDutyAndCustomsQtyValues_SameProportionWithOtherReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m, ZDateTimeOffset.Today,
				bondedInwardsKey, 80m, 320m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 3 results, 2 for receives and 1 for the order.", 3, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 120m, expectedCustomsQty: 480m,
				expectedRelValueForDuty: -120m, expectedRelCustomsQty: -480m,
				message: "Value should be calculated correctly.");
		}

		[TestDate(2018, 12, 04, 12, 35, 55)]
		public void TestCalculatedRatioForOrders_DecimalValueRounding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var finaliseDate = ZDateTimeOffset.Now;
			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 3m, finaliseDate,
				bondedInwardsKey, 100m, 100m);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 3m,
				finaliseDate.AddMinutes(5), bondedInwardsKey, 100m, 100m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 3m,
				bondedInwardsKey, 999m, 999m);
			var pick = Helper.CreatePickNew(order);

			// alter allocated inventories to get 2/3 from 1 inventory and 1/3 from another inventory
			// to simulate 0.666666 + 0.333333 = 0.999999 scenario
			// returned value should be 100.000 since the function returns the rounded value
			var availablePickInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single()
				.AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var allocatedInventory = availablePickInventories.Single(inventory => inventory.PickLineQuantity != 0);
			allocatedInventory.PickLineQuantity = 2m; // update allocated inventory quantity from 3 to 2
			var otherAvailableInventory =
				availablePickInventories.Single(inventory => inventory.PickLineQuantity != 2m);
			otherAvailableInventory.Allocate = true; // allocated the remaining 1 unit

			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = finaliseDate.AddMinutes(10);
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 3 results, 2 for receives and 1 for the order.", 3, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 100m, expectedCustomsQty: 100m,
				expectedRelValueForDuty: -100m, expectedRelCustomsQty: -100m,
				message: "Value should be calculated correctly.");
		}

		public void
			TestCalculatedRatioForOrders_MultiplePickLinesAndInventories_DifferentValueForDutyAndCustomsQtyValues_DifferentProportionWithOtherReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m,
				ZDateTimeOffset.Today, bondedInwardsKey, 100m, 400m);

			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m,
				bondedInwardsKey, 999m, 999m);
			var pick = Helper.CreatePickNew(order);

			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m,
				ZDateTimeOffset.Today, bondedInwardsKey, 160m, 480m);
			order.Pick.ClearAllInventoriesCache();
			order.Pick.ClearOrderedInventoriesCache();
			pick.AutoAllocateItemsWithMock(); // Add receives one by one for deterministic order.
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = ZDateTimeOffset.Today; // For consistency with old code / CreatePickAndFinalise
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 3 results, 2 for receives and 1 for the order.", 3, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCalculatedValueFromRatio(orderResult, expectedValueForDuty: 140m, expectedCustomsQty: 520m,
				expectedRelValueForDuty: -140m, expectedRelCustomsQty: -520m,
				message: "Value should be calculated correctly.");
		}

		public void TestCalculatedRatioForOrders_InventoryWithNoBondedWhsAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 1 result only - order.", 1, results.Count);

			var result = results.Single();
			AssertCalculatedValueFromRatio(result, expectedValueForDuty: 0m, expectedCustomsQty: 0m,
				expectedRelValueForDuty: 0m, expectedRelCustomsQty: 0m, message: "Value should be 0.");
		}

		public void TestCalculatedRatioForOrders_ProductsWithReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "BATCH123", "", bondedInwardsKey);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = bondedInwardsKey;
			receiveLine.CustomsData.WB_ValueForDuty = 100m;
			receiveLine.CustomsData.WB_CustomsQty = 400m;
			receiveLine.InDocketLine.WE_BondedEntryKey = bondedInwardsKey;
			receive.FinaliseDocketWithoutUserConfirmation();

			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = bondedInwardsKey;
			orderLine.WE_BondedEntryKey = bondedInwardsKey;

			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.PartAttribute3 = "PLASTIC";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.PartAttribute3 = "METAL";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);

			var rcaResult1 = results.Single(row => (ZDecimal)row["InvoiceQty"] == 4m);
			AssertCalculatedValueFromRatio(rcaResult1, expectedValueForDuty: 40m, expectedCustomsQty: 160m,
				expectedRelValueForDuty: -40m, expectedRelCustomsQty: -160m,
				message: "Value should be calculated correctly.");

			var rcaResult2 = results.Single(row => (ZDecimal)row["InvoiceQty"] == 6m);
			AssertCalculatedValueFromRatio(rcaResult2, expectedValueForDuty: 60m, expectedCustomsQty: 240m,
				expectedRelValueForDuty: -60m, expectedRelCustomsQty: -240m,
				message: "Value should be calculated correctly.");
		}

		public void TestCalculatedRatioForOrders_ProductsWithReleaseCapturedAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);

			var bondedInwardsKey1 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "", "", bondedInwardsKey1);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = bondedInwardsKey1;
			receiveLine.CustomsData.WB_ValueForDuty = 100m;
			receiveLine.CustomsData.WB_CustomsQty = 400m;
			receiveLine.InDocketLine.WE_BondedEntryKey = bondedInwardsKey1;
			receive.FinaliseDocketWithoutUserConfirmation();

			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = bondedInwardsKey1;
			orderLine.WE_BondedEntryKey = bondedInwardsKey1;

			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.PartAttribute3 = "PLASTIC";
			releaseLine1.SerialNumber = "SERN1";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute3 = "METAL";
			releaseLine2.SerialNumber = "SERN2";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);

			var rcaResult1 = results.Single(row =>
				(ZDecimal)row["InvoiceQty"] == 1m && (ZString)row["SerialNumberValue"] == "SERN1");
			AssertCalculatedValueFromRatio(rcaResult1, expectedValueForDuty: 10m, expectedCustomsQty: 40m,
				expectedRelValueForDuty: -10m, expectedRelCustomsQty: -40m,
				message: "Value should be calculated correctly.");

			var rcaResult2 = results.Single(row =>
				(ZDecimal)row["InvoiceQty"] == 1m && (ZString)row["SerialNumberValue"] == "SERN2");
			AssertCalculatedValueFromRatio(rcaResult2, expectedValueForDuty: 10m, expectedCustomsQty: 40m,
				expectedRelValueForDuty: -10m, expectedRelCustomsQty: -40m,
				message: "Value should be calculated correctly.");
		}

		public void TestCalculatedRatioForOrders_EdgeCase_InventoryWithZeroTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m,
				ZDateTimeOffset.Today, bondedInwardsKey, 100m, 400m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 10m,
				bondedInwardsKey, 999m, 999m);
			Helper.CreatePickNew(order);
			Factory.Save();

			// Setting the receive line's TransactionQuantity to 0 to simulate possible defect (calculating ratio, divide by WE_TransactionQuantity)
			// Setting the pickline's picked units to 0 first in order to reduce the receive line's transaction quantity to 0
			// This behaviour may not be possible functionally but there are no constraints that disallows this in the DB
			order.Lines[0].PickLines[0].WZ_Units = 0;
			receive.Lines[0].WE_TransactionQuantity = 0m;
			Factory.Save();

			DynamicBusinessObjectCollection results = null;
			AssertNoExceptionThrown("No sql exception - divide by zero is thrown.", () => results = LoadView());
			AssertEquals(
				"Should return 1 result only - for receive, no results for order as the order is not finalised.", 1,
				results.Count);
		}

		void AssertCalculatedValueFromRatio(DynamicBusinessObject result, decimal expectedValueForDuty,
			decimal expectedCustomsQty, decimal expectedRelValueForDuty, decimal expectedRelCustomsQty, string message)
		{
			AssertEquals(message, expectedValueForDuty, result["ValueForDuty"]);
			AssertEquals(message, expectedCustomsQty, result["CustomsQty"]);
			AssertEquals(message, expectedRelValueForDuty, result["RelativeValueForDuty"]);
			AssertEquals(message, expectedRelCustomsQty, result["RelativeCustomsQty"]);
		}

		#endregion

		#region TestPartAttributeNameAndValueLengthMaxed

		public void TestPartAttributeNameAndValueLengthMaxed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "12345678901234567890";
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "", "", "ENTRY-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receive.WD_FinalisedDate = inventory.InDocketLine.WE_FinalisedDate;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "ENTRY-123", "DummyOutwards");
			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "1234567890123456789012345";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute1 = "1234567890123456789067890";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			orderLine.Order.WD_FinalisedDate = orderLine.WE_FinalisedDate;
			Factory.Save();

			//Simulates Issue 01149761 - String or binary data would be truncated
			AssertNoExceptionThrown("No Exception is thrown in calling the function.", () => LoadView());
			var results = LoadView();
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);
			AssertEquals("PartAttrib1 should return the correct value.", true,
				results.Any(result =>
					(ZString)(result["PartAttrib1"]) == "12345678901234567890: 1234567890123456789012345"));
			AssertEquals("PartAttrib1 should return the correct value.", true,
				results.Any(result =>
					(ZString)(result["PartAttrib1"]) == "12345678901234567890: 1234567890123456789067890"));
		}

		#endregion

		#region TestAdjustout

		public void TestAdjustout()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();

			var line = receive.Lines.Single();
			line.CustomsData.WB_CustomsQty = 100m;
			line.CustomsData.WB_ValueForDuty = 100m;

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			adjustmentOut.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -4m,
				data.Whs1.DefaultLocation, bondedInwardsKey, "", 0);
			line.CustomsData.WB_CustomsQty = 100m;
			line.CustomsData.WB_ValueForDuty = 100m;

			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOut);
			Factory.Save();

			var results = LoadView();
			AssertEquals("2 results must be returned.", 2, results.Count);

			var adjustedOutLine = results.Single(r => (ZDecimal)r["InvoiceQty"] < 0);
			AssertCalculatedValueFromRatio(adjustedOutLine, expectedValueForDuty: -40m, expectedCustomsQty: -40m,
				expectedRelValueForDuty: -40m, expectedRelCustomsQty: -40m,
				message: "Value should be calculated correctly.");
			AssertEquals("InvoiceQty", -4m, adjustedOutLine["InvoiceQty"]);

			var receiveLine = results.Single(r => (ZDecimal)r["InvoiceQty"] > 0);
			AssertCalculatedValueFromRatio(receiveLine, expectedValueForDuty: 100m, expectedCustomsQty: 100m,
				expectedRelValueForDuty: 100m, expectedRelCustomsQty: 100m,
				message: "Value should be calculated correctly.");
			AssertEquals("InvoiceQty", 10m, receiveLine["InvoiceQty"]);
		}

		public void TestAdjustout_EdgeCase_ZeroBondedWhsQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();

			var line = receive.Lines.Single();
			line.CustomsData.WB_CustomsQty = 100m;
			line.CustomsData.WB_ValueForDuty = 100m;

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			adjustmentOut.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -4m,
				data.Whs1.DefaultLocation, bondedInwardsKey, "", 0);

			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOut);
			Factory.Save();

			// Setting the adjustment out line's customs data - WB_BondedWhsQty to 0 to simulate reported defect (calculating ratio, divide by WB_BondedWhsQty)
			// This behaviour may not be possible functionally but there are no constraints that disallows this in the DB
			adjustmentLine.CustomsData.WB_BondedWhsQty = 0;
			Factory.Save();

			DynamicBusinessObjectCollection results = null;
			AssertNoExceptionThrown("No sql exception - divide by zero is thrown.", () => results = LoadView());
			AssertEquals("2 results must be returned.", 2, results.Count);

			var adjustedOutLine = results.Single(r => (ZDecimal)r["InvoiceQty"] < 0);
			AssertCalculatedValueFromRatio(adjustedOutLine, expectedValueForDuty: -100m, expectedCustomsQty: -100m,
				expectedRelValueForDuty: -100m, expectedRelCustomsQty: -100m,
				message: "Value based on WE_TransactionQty when WB_BondedWhsQty = 0.");
			AssertEquals("InvoiceQty", -4m, adjustedOutLine["InvoiceQty"]);

			var receiveLine = results.Single(r => (ZDecimal)r["InvoiceQty"] > 0);
			AssertCalculatedValueFromRatio(receiveLine, expectedValueForDuty: 100m, expectedCustomsQty: 100m,
				expectedRelValueForDuty: 100m, expectedRelCustomsQty: 100m,
				message: "Value should be calculated correctly.");
			AssertEquals("InvoiceQty", 10m, receiveLine["InvoiceQty"]);
		}

		#endregion

		#region Implementation

		#region LoadView

		DynamicBusinessObjectCollection LoadView(string filtrPartAttr = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				$"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, {(filtrPartAttr == null ? "null" : $"'{filtrPartAttr}'")}, null, null, null, null)");
			return result;
		}

		#endregion

		WhsReceive CreateReceiveWithInventoryAndCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, ZDateTimeOffset finalisedDate, string bondedEntryKey,
			decimal bondedValueOfDuty, decimal bondedCustomsQty)
		{
			var location = warehouse.DefaultLocation;
			var bondedArea = warehouse.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org.PK, warehouse.PK, docketNumber, finalisedDate);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			CreateReceiveLineWithCustomsData(receive, part, units, bondedEntryKey, bondedValueOfDuty, bondedCustomsQty,
				location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			return receive;
		}

		WhsReceiveLine CreateReceiveLineWithCustomsData(WhsReceive receive, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueOfDuty, decimal bondedCustomsQty, WhsLocation location)
		{
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part, units, location);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = bondedEntryKey;
			receiveLine.CustomsData.WB_ValueForDuty = bondedValueOfDuty;
			receiveLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			receiveLine.WE_BondedEntryKey = bondedEntryKey;

			return receiveLine;
		}

		void CreateReceiveWithInventoryNoCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, ZDecimal units, ZDateTimeOffset finalisedDate)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(org, warehouse, docketNumber, part, units, finalise: false);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = finalisedDate;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Factory.Save();
		}

		WhsOrder CreateOrderWithOrderLineAndCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, string bondedEntryKey, decimal bondedValueForDuty,
			decimal bondedCustomsQty)
		{
			var order = Helper.CreateWhsOrder(org, warehouse, docketNumber);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			CreateOrderLineWithCustomsData(order, part, units, bondedEntryKey, bondedValueForDuty, bondedCustomsQty);
			Factory.Save();
			return order;
		}

		WhsOrderLine CreateOrderLineWithCustomsData(WhsOrder order, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueForDuty, decimal bondedCustomsQty)
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = bondedEntryKey;
			orderLine.CustomsData.WB_ValueForDuty = bondedValueForDuty;
			orderLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			orderLine.WE_BondedEntryKey = bondedEntryKey;

			return orderLine;
		}

		void CreatePickAndFinalise(WhsOrder order, ZDateTimeOffset orderFinalisedDate)
		{
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = orderFinalisedDate;
			Factory.Save();
		}

		#endregion

		#region TestFunction_SerialNumber

		[TestDate(2022, 02, 03, 00, 00, 00)]
		public void TestFunction_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK,
				"1", entryKey: "ENTRY-123");
			receiveLine1.WE_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK,
				"1", entryKey: "ENTRY-123");
			receiveLine2.WE_SerialNumber = "SN2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			//Set because WB_EntryDate is SmallDateTime remove if this is ever changed.
			receive.WD_FinalisedDate = receiveLine1.WE_FinalisedDate;
			receiveLine1.WE_FinalisedDate = receiveLine1.WE_FinalisedDate;
			receiveLine2.WE_FinalisedDate = receiveLine1.WE_FinalisedDate;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			orderLine1.WE_SerialNumber = "SN1";
			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);
			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			orderLine1.Order.WD_FinalisedDate = orderLine1.WE_FinalisedDate;
			orderLine1.WE_FinalisedDate = orderLine1.WE_FinalisedDate;
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should be 3 results, 2 receive Lines + 1 order Line.", 3, results.Count);
			AssertLineMatch(results, receiveLine1, 1m, "", "", "", "SN1");
			AssertLineMatch(results, receiveLine2, 1m, "", "", "", "SN2");
			AssertLineMatch(results, orderLine1, -1m, "", "", "", "SN1");

			results = LoadView("SN1");
			AssertEquals("There should be two lines as we specified filter by part attribute.", 2, results.Count);
			AssertLineMatch(results, receiveLine1, 1m, "", "", "", "SN1");
			AssertLineMatch(results, orderLine1, -1m, "", "", "", "SN1");

			results = LoadView("SN2");
			AssertEquals("There should be one line as we specified filter by part attribute.", 1, results.Count);
			AssertLineMatch(results, receiveLine2, 1m, "", "", "", "SN2");

			results = LoadView("SN");
			AssertEquals("Should be 3 results, 2 receive Lines + 1 order Line.", 3, results.Count);
			AssertLineMatch(results, receiveLine1, 1m, "", "", "", "SN1");
			AssertLineMatch(results, receiveLine2, 1m, "", "", "", "SN2");
			AssertLineMatch(results, orderLine1, -1m, "", "", "", "SN1");

			results = LoadView("2");
			AssertEquals("Should be 1 result, 1 receive Line.", 1, results.Count);
			AssertLineMatch(results, receiveLine2, 1m, "", "", "", "SN2");
		}

		#endregion

		#region TestCustomsDeadLineFilter

		public void TestCustomsDeadLineFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			attribute.WB_CustomsDeadline = ZDate.Today;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m,
				bondedInwardsKey2, 999m, 999m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			attribute2.WB_CustomsDeadline = ZDate.Today.AddYears(-1);

			Factory.Save();

			var startDate = ZSqlParameter.New("@p792", ZDate.Today.AddMonths(-1),
				WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline);
			var endDate = ZSqlParameter.New("@p793", ZDate.Today.AddMonths(1),
				WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , null, null, null, null) where (CustomsDeadline >= @p792 AND CustomsDeadline < @p793)",
				new[] { startDate, endDate });
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			AssertEquals("LINEENTRYKEY", (ZString)result.Single()["InwardsEntryNo"]);
			AssertEquals((ZShort)1, (ZShort)result.Single()["InwardsEntryLineNo"]);
		}

		#endregion

		#region TestEntryDateFilter

		public void TestEntryDateFilter_SameFinalisedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, bondedInwardsKey, "DummyOutwards");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_CustomerReference = "TestOutwardsEntry#";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receive and 1 for the order.", 2, results.Count);

			var startDate = ZSqlParameter.New("@StartDate", ZDate.Today.AddMonths(-1), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			var endDate = ZSqlParameter.New("@EndDate", ZDate.Today.AddMonths(1), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load("select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , null, null, @StartDate, @EndDate)",
				new[] { startDate, endDate });
			AssertEquals("Should return 2 results, 1 for receive and 1 for the order.", 2, result.Count);
		}

		public void TestEntryDateFilter_DifferentFinalisedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, bondedInwardsKey, "DummyOutwards");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_CustomerReference = "TestOutwardsEntry#";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = ZDateTimeOffset.Today.AddMonths(5);
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receive and 1 for the order.", 2, results.Count);

			var startDate = ZSqlParameter.New("@StartDate", ZDate.Today.AddMonths(-1), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			var endDate = ZSqlParameter.New("@EndDate", ZDate.Today.AddMonths(1), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load("select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , null, null, @StartDate, @EndDate)",
				new[] { startDate, endDate });
			AssertEquals("Should return 1 results, 1 for receive and 0 for the order.", 1, result.Count);

			startDate = ZSqlParameter.New("@StartDate", ZDate.Today.AddMonths(4), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			endDate = ZSqlParameter.New("@EndDate", ZDate.Today.AddMonths(6), WhsBondedWarehouseAttributeSchema.WB_EntryDate);
			result = new DynamicBusinessObjectCollection(Factory);
			result.Load("select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , null, null, @StartDate, @EndDate)",
				new[] { startDate, endDate });
			AssertEquals("There should be 0 line related to whs1. Because there isn't any receive line in this date range", 0, result.Count);
		}

		#endregion

		#region TestInwardStyleFilter

		public void TestInwardStyleFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardStyle = "ABC";
			attribute.WB_InwardStyle = inwardStyle;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m,
				bondedInwardsKey2, 999m, 999m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardStyle2 = "123";
			attribute2.WB_InwardStyle = inwardStyle2;

			Factory.Save();

			var parameterForInwardStyle = ZSqlParameter.New("@CustomsInwardStyle", inwardStyle,
				WhsBondedWarehouseAttributeSchema.WB_InwardStyle);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , @CustomsInwardStyle, null, null, null)",
				new[] { parameterForInwardStyle });
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			AssertEquals("LINEENTRYKEY", (ZString)result.Single()["InwardsEntryNo"]);
			AssertEquals((ZShort)1, (ZShort)result.Single()["InwardsEntryLineNo"]);
		}

		#endregion

		#region TestInwardProcedureFilter

		public void TestInwardProcedureFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m,
				bondedInwardsKey, 999m, 999m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardProcedure = "ABC";
			attribute.WB_InwardProcedure = inwardProcedure;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryNoCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m,
				bondedInwardsKey2, 999m, 999m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardProcedure2 = "123";
			attribute2.WB_InwardProcedure = inwardProcedure2;

			Factory.Save();

			var parameterForInwardStyle = ZSqlParameter.New("@CustomsInwardProcedure", inwardProcedure2,
				WhsBondedWarehouseAttributeSchema.WB_InwardProcedure);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null , null, @CustomsInwardProcedure, null, null)",
				new[] { parameterForInwardStyle });
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			AssertEquals("LINEENTRYKEY", (ZString)result.Single()["InwardsEntryNo"]);
			AssertEquals((ZShort)2, (ZShort)result.Single()["InwardsEntryLineNo"]);
		}

		#endregion

		#region TestFunctionWithMaxValues

		public void TestFunctionWithMaxValues_ValueForDuty()
		{
			TestFunctionWithMaxValues_CustomsDataCore((p) => p.WB_ValueForDuty = 922337203685477.58m);
		}

		public void TestFunctionWithMaxValues_CustomsQty()
		{
			TestFunctionWithMaxValues_CustomsDataCore((p) => p.WB_CustomsQty = 99999999999999.99m);
		}

		public void TestFunctionWithMaxValues_CustomsSecondQuantity()
		{
			TestFunctionWithMaxValues_CustomsDataCore((p) => p.WB_CustomsSecondQuantity = 99999999999999.99m);
		}

		public void TestFunctionWithMaxValues_CustomsThirdQuantity()
		{
			TestFunctionWithMaxValues_CustomsDataCore((p) => p.WB_CustomsThirdQuantity = 99999999999999.99m);
		}

		void TestFunctionWithMaxValues_CustomsDataCore(Action<WhsBondedWarehouseAttribute> maxValueSetter)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				ZDate.Empty, ZDate.Empty, "", "", "", "ENTRY-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			maxValueSetter(inventory.InDocketLine.CustomsData);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "ENTRY-123", "DummyOutwards");
			Helper.CreatePickNew(order);
			Assert("Precondition: Order must be picked.", order.IsAttachedToPick);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);
			Factory.Save();

			AssertNoExceptionThrown("No exception is thrown.", () => LoadView());
		}

		public void TestFunctionWithMaxValues_TransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);

			// setting product volume and weight to small values to not exceed max value for docket total weight and volume
			data.Part1.OP_Cubic = 0.000001m;
			data.Part1.OP_Weight = 0.000001m;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 99999999999999.99m,
				data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "ENTRY-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			AssertNoExceptionThrown("No exception is thrown.", () => LoadView());
		}

		#endregion

		#region TestFunction_RelativeCustomsQty

		public void TestFunction_Receive_RelativeCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 80m, 40m);

			var results = LoadView();
			AssertEquals("Receive RelativeCustomsQty correct", 40m, (ZDecimal)results.Single()["RelativeCustomsQty"]);
		}

		public void TestFunction_Receive_RelativeSecondCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var warehouse = data.Whs1;
			var location = warehouse.DefaultLocation;
			var bondedArea = warehouse.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, warehouse.PK, "O1", ZDateTimeOffset.Today);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine =
				CreateReceiveLineWithCustomsData(receive, data.Part1, 10m, bondedInwardsKey, 11m, 12m, location);
			receiveLine.CustomsData.WB_CustomsSecondQuantity = 15m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			var results = LoadView();
			AssertEquals("Receive RelativeSecondCustomsQty correct", 15m,
				(ZDecimal)results.Single()["RelativeSecondCustomsQty"]);
		}

		public void TestFunction_Receive_RelativeThirdCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var warehouse = data.Whs1;
			var location = warehouse.DefaultLocation;
			var bondedArea = warehouse.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, warehouse.PK, "O1", ZDateTimeOffset.Today);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine =
				CreateReceiveLineWithCustomsData(receive, data.Part1, 10m, bondedInwardsKey, 11m, 12m, location);
			receiveLine.CustomsData.WB_CustomsThirdQuantity = 17m;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			var results = LoadView();
			AssertEquals("Receive RelativeThirdCustomsQty correct", 17m,
				(ZDecimal)results.Single()["RelativeThirdCustomsQty"]);
		}

		public void TestFunction_Order_RelativeCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 100m, bondedInwardsKey, 15m, 80m);
			orderLine.CustomsData.WB_BondedWhsQty = 5m;
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receives and 1 for the order.", 2, results.Count);
			var orderRow = results.Single(row => (ZString)row["OutwardsEntryNo"] == "ORDERLINEKEY");
			AssertEquals("Order RelativeCustomsQty correct", -400m, (ZDecimal)orderRow["RelativeCustomsQty"]);
		}

		public void TestFunction_Order_RelativeSecondCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m,
				ZDateTimeOffset.Today, bondedInwardsKey, 100m, 400m);
			receive.Lines[0].CustomsData.WB_CustomsSecondQuantity = 134m;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 88m, bondedInwardsKey, 15m, 80m);
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receives and 1 for the order.", 2, results.Count);
			var orderRow = results.Single(row => (ZString)row["OutwardsEntryNo"] == "ORDERLINEKEY");
			AssertEquals("Order RelativeSecondCustomsQty correct", -134m,
				(ZDecimal)orderRow["RelativeSecondCustomsQty"]);
		}

		public void TestFunction_Order_RelativeThirdCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m,
				ZDateTimeOffset.Today, bondedInwardsKey, 100m, 400m);
			receive.Lines[0].CustomsData.WB_CustomsThirdQuantity = 9m;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 100m, bondedInwardsKey, 15m, 80m);
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 2 results, 1 for receives and 1 for the order.", 2, results.Count);
			var orderRow = results.Single(row => (ZString)row["OutwardsEntryNo"] == "ORDERLINEKEY");
			AssertEquals("Order RelativeThirdCustomsQty correct", -9m, (ZDecimal)orderRow["RelativeThirdCustomsQty"]);
		}

		public void TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyGreaterThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyGreaterThanZeroCore("RelativeCustomsQty",
				(attribute, amount) => attribute.WB_CustomsQty = amount);
		}

		public void TestFunction_Adjustment_RelativeSecondCustomsQty_WE_TransactionQtyGreaterThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyGreaterThanZeroCore("RelativeSecondCustomsQty",
				(attribute, amount) => attribute.WB_CustomsSecondQuantity = amount);
		}

		public void TestFunction_Adjustment_RelativeThirdCustomsQty_WE_TransactionQtyGreaterThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyGreaterThanZeroCore("RelativeThirdCustomsQty",
				(attribute, amount) => attribute.WB_CustomsThirdQuantity = amount);
		}

		public void TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyGreaterThanZeroCore(string columnToTest,
			Action<WhsBondedWarehouseAttribute, ZDecimal> setColumnToTest)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var adjustmentCUS = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN_CUS");
			adjustmentCUS.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLineCUS = Helper.CreateWhsAdjustmentLine(adjustmentCUS, data.Part1, 3m,
				data.Whs1.DefaultLocation, bondedInwardsKey, "", 0);
			setColumnToTest(adjustmentLineCUS.CustomsData, 7m);

			adjustmentCUS.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentCUS);

			Factory.Save();

			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");

			AssertEquals(2, results.Count);
			var adjCUSRow = results.Single(row => (ZDecimal)row["RelativeInvoiceQty"] == 3m);
			AssertEquals("RelativeCustomsQty should be correct", 7m, adjCUSRow[columnToTest]);
		}

		public void TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyLessThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyLessThanZeroCore("RelativeCustomsQty",
				(attribute) => attribute.WB_CustomsQty = 70m, -21m);
		}

		public void TestFunction_Adjustment_RelativeSecondCustomsQty_WE_TransactionQtyLessThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyLessThanZeroCore("RelativeSecondCustomsQty",
				(attribute) => attribute.WB_CustomsSecondQuantity = 50m, -15m);
		}

		public void TestFunction_Adjustment_RelativeThirdCustomsQty_WE_TransactionQtyLessThanZero()
		{
			TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyLessThanZeroCore("RelativeThirdCustomsQty",
				(attribute) => attribute.WB_CustomsThirdQuantity = 35m, -10.5m);
		}

		public void TestFunction_Adjustment_RelativeCustomsQty_WE_TransactionQtyLessThanZeroCore(string columnToTest,
			Action<WhsBondedWarehouseAttribute> setColumnToTest, ZDecimal expectedValue)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.Lines[0].WE_WL = data.Whs1.DefaultLocationInBondedArea.PK;
			setColumnToTest(receive.Lines[0].CustomsData);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var adjustmentCUS = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN_CUS");
			adjustmentCUS.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLineCUS = Helper.CreateWhsAdjustmentLine(adjustmentCUS, data.Part1, -3m,
				data.Whs1.DefaultLocationInBondedArea, bondedInwardsKey, "", 0);
			adjustmentCUS.RunPreSaveValidation();
			adjustmentCUS.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentCUS);

			Factory.Save();

			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(
				"select * from WhsBondedTransactionsReport(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)");

			AssertEquals(2, results.Count);
			var adjCUSRow = results.Single(row => (ZDecimal)row["RelativeInvoiceQty"] == -3m);
			AssertEquals($"{columnToTest} should be correct", expectedValue, adjCUSRow[columnToTest]);
		}

		#endregion
	}
}
