using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CopyCustomsStrategyTest : WhsTestCaseWithFactory
	{
		public void TestCopyCustomsStrategy_Adjustment()
		{
			var docketLine = ApplyCopyCustomsStrategy<WhsAdjustmentLine>(new AdjustmentCopyCustomsStrategy());
			var customsData = docketLine.CustomsData;
			AssertEquals("CustomsData.WB_EntryLineNo, Should always copied.", (short)1, customsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should always copied.", "DEF", customsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 19m, customsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData.WB_CustomsUnitOfQty);
		}

		public void TestCopyCustomsStrategy_Order()
		{
			var docketLine = ApplyCopyCustomsStrategy<WhsOrderLine>(new OrderCopyCustomsStrategy());
			var customsData = docketLine.CustomsData;
			AssertEquals("CustomsData.WB_EntryLineNo, Should not be copied.", (short)0, customsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should not be copied.", "", customsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 19m, customsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData.WB_CustomsUnitOfQty);
		}

		public void TestCopyCustomsStrategy_Transfer()
		{
			var docketLine = ApplyCopyCustomsStrategy<WhsTransferLine>(new DefaultCopyCustomsStrategy());
			var customsData = docketLine.CustomsData;
			AssertEquals("CustomsData.WB_EntryLineNo, Should always copied.", (short)1, customsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should always copied.", "DEF", customsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", ZString.Empty, customsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 0m, customsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", ZString.Empty, customsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 0m, customsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", ZString.Empty, customsData.WB_CustomsUnitOfQty);
		}

		TDocketLine ApplyCopyCustomsStrategy<TDocketLine>(ICopyCustomsStrategy strategy) where TDocketLine : WhsDocketLine
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = "CUS";

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var invCustomsData = inventory.InDocketLine.CustomsData;
			invCustomsData.WB_EntryLineNo = 1;
			invCustomsData.WB_EntryKey = "DEF";
			invCustomsData.WB_AddInfo = "addinfo";
			invCustomsData.WB_BondedWhsQty = 19;
			invCustomsData.WB_BondedWhsUnitOfQty = "KG";
			invCustomsData.WB_CustomsQty = 3;
			invCustomsData.WB_CustomsUnitOfQty = "LB";

			var docketLine = inventory.Factory.New<TDocketLine>();
			strategy.CopyCustomsData(docketLine, inventory.InDocketLine);
			return docketLine;
		}
	}
}
