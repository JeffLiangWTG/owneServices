using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	class WhsBondedCalculatorTest : WhsTestCaseWithFactory
	{
		#region TestGetBondedWhsQty

		public void TestGetBondedWhsQty()
		{
			SetupData();
			var calculator1 = new WhsBondedCalculator(Factory, "E11AA1", 1);
			AssertEquals(50m, calculator1.GetAvailableBondedWhsQty());
			var calculator2 = new WhsBondedCalculator(Factory, "E11AA1", 2);
			AssertEquals(5m, calculator2.GetAvailableBondedWhsQty());
		}

		#endregion

		#region TestGetAvailableBondedWhsQtyWith_Adjustments

		public void TestGetAvailableBondedWhsQtyWith_AdjustingOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();
			var bondedLocation = data.Whs1.FindLocation("RR1");
			var receive1 = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive1, data.Part1, 10, "A-1", bondedLocation);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			var receive2 = CreateCustomReceive(data.Org1, data.Whs1, "R2");
			CreateCustomsReceiveLine(receive2, data.Part1, 10, "B-1", bondedLocation);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			var receive3 = CreateCustomReceive(data.Org1, data.Whs1, "R3");
			CreateCustomsReceiveLine(receive3, data.Part1, 10, "A-2", bondedLocation);
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive3);
			var receive4 = CreateCustomReceive(data.Org1, data.Whs1, "R4");
			CreateCustomsReceiveLine(receive4, data.Part1, 10, "C-3", bondedLocation);
			receive4.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive4);
			Factory.Save();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateCustomsAdjustmentLine(adjustment, data.Part1, -1, bondedLocation, "A-1");
			CreateCustomsAdjustmentLine(adjustment, data.Part1, -2, bondedLocation, "A-1");
			CreateCustomsAdjustmentLine(adjustment, data.Part1, -3, bondedLocation, "A-2");
			CreateCustomsAdjustmentLine(adjustment, data.Part1, -4, bondedLocation, "B-1");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);
			var calculator = new WhsBondedCalculator(Factory, "A", 1);
			AssertEquals(7m, calculator.GetAvailableBondedWhsQty());
		}

		public void TestGetAvailableBondedWhsQtyWith_AdjustingIn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;
			Factory.Save();
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateCustomsAdjustmentLine(adjustment, data.Part1, 1, data.Whs1.DefaultLocation, "A-1");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);
			var calculator = new WhsBondedCalculator(Factory, "A", 1);
			AssertEquals(1m, calculator.GetAvailableBondedWhsQty());
		}

		void CreateCustomsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, decimal units,
			WhsLocation location, string entryKey)
		{
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, part, units, location);
			adjustmentLine.CustomsData.WB_EntryKey = entryKey;
			adjustmentLine.WE_BondedEntryKey = entryKey;
		}

		WhsReceive CreateCustomReceive(OrgHeader org, WhsWarehouse warehouse, string docketNumber)
		{
			var receive = Helper.CreateWhsReceive(org, warehouse, docketNumber);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			return receive;
		}

		void CreateCustomsReceiveLine(WhsReceive receive, OrgSupplierPart part, decimal units, string entryKey,
			WhsLocation location)
		{
			var receiveLine =
				Helper.CreateWhsReceiveInventoryLine(receive, part, units,
					location ?? receive.Warehouse.DefaultLocation);
			receiveLine.CustomsData.WB_EntryKey = entryKey;
			receiveLine.CustomsData.WB_BondedWhsQty = units;
			receiveLine.InDocketLine.WE_BondedEntryKey = entryKey;
		}

		#endregion

		#region TestGetAvailableBondedWhsQtyWith_Orders

		public void TestGetAvailableBondedWhsQtyWith_Orders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();
			var bondedLocation = data.Whs1.FindLocation("RR1");
			var receive1 = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive1, data.Part1, 10, "A-1", bondedLocation);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			var receive2 = CreateCustomReceive(data.Org1, data.Whs1, "R2");
			CreateCustomsReceiveLine(receive2, data.Part1, 10, "B-1", bondedLocation);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			var receive3 = CreateCustomReceive(data.Org1, data.Whs1, "R3");
			CreateCustomsReceiveLine(receive3, data.Part1, 10, "A-2", bondedLocation);
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive3);
			var receive4 = CreateCustomReceive(data.Org1, data.Whs1, "R4");
			CreateCustomsReceiveLine(receive4, data.Part1, 10, "C-3", bondedLocation);
			receive4.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive4);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			CreateCustomsOrderLine(order, data.Part1, 1m, "A-1");
			CreateCustomsOrderLine(order, data.Part1, 2m, "A-1");
			CreateCustomsOrderLine(order, data.Part1, 3m, "A-2");
			CreateCustomsOrderLine(order, data.Part1, 4m, "B-1");
			Factory.Save();
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();
			var calculator = new WhsBondedCalculator(Factory, "A", 1);
			AssertEquals(7m, calculator.GetAvailableBondedWhsQty());
		}

		WhsOrderLine CreateCustomsOrderLine(WhsOrder order, OrgSupplierPart part, decimal units, string entryKey)
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.CustomsData.WB_EntryKey = entryKey;
			orderLine.WE_BondedEntryKey = entryKey; // used to match to inventory entry key
			return orderLine;
		}

		#endregion

		#region Implementation

		void SetupData(bool processBondedInwardDuringConstruction = true)
		{
			Data = new TestDataForBondedEntriesWithVOC(Factory,
				processBondedInwardDuringConstruction: processBondedInwardDuringConstruction);
			Factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupData")]
		TestDataForBondedEntriesWithVOC Data;

		#endregion
	}
}
