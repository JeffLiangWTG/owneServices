using System;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsCustomsHelperTest : WhsTestCaseWithFactory
	{
		public void TestCanReleaseWithoutCustomsClearance_RegistryRespected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK);
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Should return false if registry value is false.", false,
					WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(order));
			}

			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Should return false if registry value is true.", true,
					WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(order));
			}
		}

		public void TestSplitCustomsValuesAndQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 11m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = 1;
			customsData.WB_CustomsQty = 11m;
			customsData.WB_BondedWhsQty = 11m;
			customsData.WB_ValueForDuty = 17m;
			customsData.WB_TILV = 99.99m;
			customsData.WB_CustomsSecondQuantity = 25m;
			customsData.WB_CustomsThirdQuantity = 34m;
			Factory.Save();

			receiveLine.WE_TransactionQuantity = 7m;
			WhsCustomsHelper.SplitCustomsValuesAndQuantities(receiveLine, 11m);
			AssertEquals("WB_CustomsQty", 7m, customsData.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 7m, customsData.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 10.8182m, customsData.WB_ValueForDuty);
			AssertEquals("WB_TILV", 63.63m, customsData.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 15.90909m, customsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 21.63636m, customsData.WB_CustomsThirdQuantity);
		}

		public void TestSplitCustomsValuesAndQuantities_NewQtyLargerThanOriginalAfterSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = 1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_BondedWhsQty = 10m;
			customsData.WB_ValueForDuty = 100m;
			customsData.WB_TILV = 100m;
			customsData.WB_CustomsSecondQuantity = 20m;
			customsData.WB_CustomsThirdQuantity = 30m;
			Factory.Save();

			receiveLine.WE_TransactionQuantity = 12m;
			AssertExceptionThrown<InvalidOperationException>("Exception is thrown.",
				"New quantity cannot be larger than the original quantity after splitting.",
				() => WhsCustomsHelper.SplitCustomsValuesAndQuantities(receiveLine, 10m));
		}

		public void TestSplitCustomsValuesAndQuantities_ZeroOriginalQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = 1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_BondedWhsQty = 10m;
			customsData.WB_ValueForDuty = 100m;
			customsData.WB_TILV = 100m;
			customsData.WB_CustomsSecondQuantity = 20m;
			customsData.WB_CustomsThirdQuantity = 30m;
			Factory.Save();

			receiveLine.WE_TransactionQuantity = 6m;
			AssertExceptionThrown<InvalidOperationException>("Exception is thrown.", "Original quantity cannot be 0.",
				() => WhsCustomsHelper.SplitCustomsValuesAndQuantities(receiveLine, 0m));
		}

		public void TestSplitCustomsValuesAndQuantities_NotCustomsTransaction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			receiveLine.WE_TransactionQuantity = 6m;
			AssertEquals("Precondition: Docket is not a customs transaction.", false, receiveLine.IsCustomsTransaction);
			AssertExceptionThrown<InvalidOperationException>("Exception is thrown.",
				"You cannot split customs values if the docket is not a customs transaction.",
				() => WhsCustomsHelper.SplitCustomsValuesAndQuantities(receiveLine, 10m));
		}

		public void TestSafelyRemoveSuffixValue()
		{
			AssertEquals("B31544646", WhsCustomsHelper.SafelyRemoveSuffixValue("B31544646-23"));
			AssertEquals("B464464646", WhsCustomsHelper.SafelyRemoveSuffixValue("B464464646"));
			AssertEquals("", WhsCustomsHelper.SafelyRemoveSuffixValue(""));
		}
	}
}
