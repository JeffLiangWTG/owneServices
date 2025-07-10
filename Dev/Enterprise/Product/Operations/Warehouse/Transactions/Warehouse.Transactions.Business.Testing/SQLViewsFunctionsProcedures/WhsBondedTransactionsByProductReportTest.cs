using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedTransactionsByProductReportTest : WhsTestCaseWithFactory
	{
		public void TestCustomsBalances()
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
			CreatePickAndFinalise(order, ZDate.Today);

			var results = LoadView();
			AssertEquals("Should return 3 results, 2 for receives and 1 for the order.", 3, results.Count);

			var orderResult = results.Single(row => (ZString)row["Type"] == "OUTWARDS");
			AssertCustomsBalances(orderResult, expectedCustomsValueBalance: 0m, expectedCustomsQtyBalance: 0m,
				message: "Value should be calculated correctly.");

			var inwards = results.Where(row => (ZString)row["Type"] == "INWARDS").OrderByDescending(x => x["StockOnHand"]);

			var receiveResult1 = inwards.ToArray()[0];
			AssertCustomsBalances(receiveResult1, expectedCustomsValueBalance: 60m, expectedCustomsQtyBalance: 240m,
				message: "Value should be calculated correctly.");

			var receiveResult2 = inwards.ToArray()[1];
			AssertCustomsBalances(receiveResult2, expectedCustomsValueBalance: 0m, expectedCustomsQtyBalance: 0m,
				message: "Value should be calculated correctly.");
		}

		public void TestFiltersPendingResponses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today,
				bondedInwardsKey, 100m, 400m);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m, ZDateTimeOffset.Today,
				"<PendingCustomsResponse>", 80m, 320m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m,
				"<PendingCustomsResponse>", 999m, 999m);
			CreatePickAndFinalise(order, ZDate.Today);

			var results = LoadView();
			AssertEquals("Should return 1 result", 1, results.Count);
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

		void CreatePickAndFinalise(WhsOrder order, ZDate orderFinalisedDate)
		{
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = new ZDateTimeOffset(orderFinalisedDate);
			Factory.Save();
		}

		void AssertCustomsBalances(DynamicBusinessObject result,
			decimal expectedCustomsValueBalance, decimal expectedCustomsQtyBalance, string message)
		{
			AssertEquals(message, expectedCustomsValueBalance, result["CustomsValueBalance"]);
			AssertEquals(message, expectedCustomsQtyBalance, result["CustomsQtyBalance"]);
		}

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

		#region Implementation

		#region LoadView

		DynamicBusinessObjectCollection LoadView(string filtrPartAttr = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				$"select * from WhsBondedTransactionsByProductReport(null, null, null, null, null, null, null, null, null, null, {(filtrPartAttr == null ? "null" : $"'{filtrPartAttr}'")}, null, null)");
			return result;
		}

		#endregion

		#endregion
	}
}
