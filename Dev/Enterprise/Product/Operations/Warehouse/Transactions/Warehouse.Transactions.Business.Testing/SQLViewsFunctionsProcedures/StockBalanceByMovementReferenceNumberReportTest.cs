using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class StockBalanceByMovementReferenceNumberReportTest : WhsTestCaseWithFactory
	{
		public void TestFilterJobNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey1 = "LINEENTRYKEY1";
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, new ZDateTimeOffset(2024, 10, 03),
				bondedInwardsKey1, 100m, 400m, 200m);
			receive1.Lines[0].CustomsData.WB_DeclarationReference = "A123456";

			var bondedInwardsKey2 = "LINEENTRYKEY2";
			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, new ZDateTimeOffset(2024, 10, 03),
				bondedInwardsKey2, 100m, 400m, 200m);
			receive2.Lines[0].CustomsData.WB_DeclarationReference = "B123456";

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order1.WD_DocketSubType = OrderType.Codes.Customs;
			order1.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 04);
			var orderLine1 = CreateOrderLineWithCustomsData(order1, data.Part1, 100m, bondedInwardsKey1, 15m, 80m, 20m);
			orderLine1.CustomsData.WB_BondedWhsQty = 5m;
			orderLine1.CustomsData.WB_EntryKey = "ORDERLINEKEY1";
			orderLine1.CustomsData.WB_DeclarationReference = "C123456";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			order2.WD_DocketSubType = OrderType.Codes.Customs;
			order2.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 04);
			var orderLine2 = CreateOrderLineWithCustomsData(order2, data.Part1, 100m, bondedInwardsKey2, 15m, 80m, 20m);
			orderLine2.CustomsData.WB_BondedWhsQty = 5m;
			orderLine2.CustomsData.WB_EntryKey = "ORDERLINEKEY2";
			orderLine2.CustomsData.WB_DeclarationReference = "D123456";

			Factory.Save();

			CreatePickAndFinalise(order1, new ZDateTimeOffset(2024, 10, 05));
			CreatePickAndFinalise(order2, new ZDateTimeOffset(2024, 10, 05));

			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, null)");
			AssertEquals("No filter set, should find both records", 2, results.Count);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, 'B123456', null, null)");
			var row = results.Single(row => (ZString)row["DeclarationReference"] == "B123456");
			AssertNotNull("Filter set: only record with JobNo 'B123456' found", row);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, 'C123456', null, null)");
			AssertEquals("Filter set to order declaration number, should NOT find records", 0, results.Count);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, 'A123456', null, '2024-10-05')");
			row = results.Single(row => (ZString)row["DeclarationReference"] == "A123456");
			AssertNotNull("Filter JobNo and AsAtDate: only record with JobNo 'A123456' found", row);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, 'A123456', null, '2024-10-01')");
			AssertEquals("No entries found", 0, results.Count);
		}

		public void TestFilterAsAtDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey1 = "LINEENTRYKEY";
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, new ZDateTimeOffset(2024, 10, 03),
				bondedInwardsKey1, 100m, 400m, 200m);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order1.WD_DocketSubType = OrderType.Codes.Customs;
			order1.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 04);
			var orderLine1 = CreateOrderLineWithCustomsData(order1, data.Part1, 100m, bondedInwardsKey1, 15m, 80m, 20m);
			orderLine1.CustomsData.WB_BondedWhsQty = 5m;
			orderLine1.CustomsData.WB_EntryKey = "ORDERLINEKEY1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			order1.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 11);
			order2.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine2 = CreateOrderLineWithCustomsData(order2, data.Part1, 100m, bondedInwardsKey1, 15m, 80m, 20m);
			orderLine2.CustomsData.WB_BondedWhsQty = 5m;
			orderLine2.CustomsData.WB_EntryKey = "ORDERLINEKEY2";

			Factory.Save();

			CreatePickAndFinalise(order1, new ZDateTimeOffset(2024, 10, 05));
			CreatePickAndFinalise(order2, new ZDateTimeOffset(2024, 10, 12));

			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, null)");
			var row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeCustomsQty without AsAtDate Filter", 320m, (ZDecimal)row["CalculatedRelativeCustomsQty"]);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, '2024-10-04')");
			row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeCustomsQty with AsAtDate Filter (only Inward)", 400m, (ZDecimal)row["CalculatedRelativeCustomsQty"]);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, '2024-10-05')");
			row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeCustomsQty with AsAtDate Filter", 360m, (ZDecimal)row["CalculatedRelativeCustomsQty"]);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, '05-OCT-24')");
			row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeCustomsQty with AsAtDate Filter, converted correctly", 360m, (ZDecimal)row["CalculatedRelativeCustomsQty"]);

			results.Load($"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, '2024-10-02')");
			AssertEquals("No entries found", 0, results.Count);
		}

		public void TestFunction_CalculatedRelativeInvoiceQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = "LINEENTRYKEY";
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m, 200m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 100m, bondedInwardsKey, 15m, 80m, 20m);
			orderLine.CustomsData.WB_BondedWhsQty = 5m;
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 1 calculated result", 1, results.Count);
			var row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeInvoiceQty", 180m, (ZDecimal)row["CalculatedRelativeInvoiceQty"]);
		}

		public void TestFunction_CalculatedRelativeCustomsQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = "LINEENTRYKEY";
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m, 200m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 100m, bondedInwardsKey, 15m, 80m, 20m);
			orderLine.CustomsData.WB_BondedWhsQty = 5m;
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();
			AssertEquals("Should return 1 calculated result", 1, results.Count);
			var row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeCustomsQty", 360m, (ZDecimal)row["CalculatedRelativeCustomsQty"]);
		}

		public void TestFunction_CalculatedRelativeValueForDuty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = "LINEENTRYKEY";
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m, 200m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLineWithCustomsData(order, data.Part1, 100m, bondedInwardsKey, 15m, 80m, 20m);
			orderLine.CustomsData.WB_BondedWhsQty = 5m;
			orderLine.CustomsData.WB_EntryKey = "ORDERLINEKEY";
			Factory.Save();

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var units = order.Lines[0].PickLines[0].WZ_Units;
			var results = LoadView();
			AssertEquals("Should return 1 calculated result", 1, results.Count);
			var row = results.Single(row => (ZString)row["InwardsEntryNo"] == "LINEENTRYKEY");
			AssertEquals("CalculatedRelativeValueForDuty", 90m, (ZDecimal)row["CalculatedRelativeValueForDuty"]);
		}

		WhsOrderLine CreateOrderLineWithCustomsData(WhsOrder order, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueForDuty, decimal bondedCustomsQty, decimal transactionQty)
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.CustomsData.WB_EntryLineNo = 0;
			orderLine.CustomsData.WB_EntryKey = bondedEntryKey;
			orderLine.CustomsData.WB_ValueForDuty = bondedValueForDuty;
			orderLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			orderLine.WE_BondedEntryKey = bondedEntryKey;
			orderLine.WE_TransactionQuantity = transactionQty;

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

		WhsReceive CreateReceiveWithInventoryAndCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, ZDateTimeOffset finalisedDate, string bondedEntryKey,
			decimal bondedValueOfDuty, decimal bondedCustomsQty, decimal transactionQty)
		{
			var location = warehouse.DefaultLocation;
			var bondedArea = warehouse.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org.PK, warehouse.PK, docketNumber, finalisedDate);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			CreateReceiveLineWithCustomsData(receive, part, units, bondedEntryKey, bondedValueOfDuty, bondedCustomsQty, transactionQty,
				location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			return receive;
		}

		WhsReceiveLine CreateReceiveLineWithCustomsData(WhsReceive receive, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueOfDuty, decimal bondedCustomsQty, decimal transactionQty, WhsLocation location)
		{
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part, units, location);
			receiveLine.CustomsData.WB_EntryLineNo = 0;
			receiveLine.CustomsData.WB_EntryKey = bondedEntryKey;
			receiveLine.CustomsData.WB_ValueForDuty = bondedValueOfDuty;
			receiveLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			receiveLine.WE_BondedEntryKey = bondedEntryKey;
			receiveLine.WE_TransactionQuantity = transactionQty;

			return receiveLine;
		}

		DynamicBusinessObjectCollection LoadView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				$"select * from dbo.StockBalanceByMovementReferenceNumberReport(null, null, null, null, null, null, null, null, null, null)");
			return result;
		}
	}
}
