using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdateTest : WhsTestCaseWithFactory
	{
		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_ShouldDeferForBOMKitReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var kitReceiveLine = orderLine1.PickLines[0].InventoryLine;

			var strategy = ObjectFactory.Get<IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate>() as IDeferTriggerConditionStrategy;
			AssertEquals("Should not defer if not finalised.", false, strategy.ShouldDeferTrigger(kitReceiveLine));

			kitReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			kitReceiveLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Should defer once changed to finalised.", true, strategy.ShouldDeferTrigger(kitReceiveLine));
		}

		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_IgnoreNormalReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1);

			var strategy = ObjectFactory.Get<IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate>() as IDeferTriggerConditionStrategy;
			AssertEquals("Should not defer for normal receive line.", false, strategy.ShouldDeferTrigger(receiveLine));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Should not defer for normal receive line.", false, strategy.ShouldDeferTrigger(receiveLine));

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_IgnoreTransferLine()
		{
			TestIgnoreLines(Factory.New<WhsTransferLine>());
		}

		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_IgnoreAdjustmentLine()
		{
			TestIgnoreLines(Factory.New<WhsAdjustmentLine>());
		}

		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_IgnoreOrderLine()
		{
			TestIgnoreLines(Factory.New<WhsOrderLine>());
		}

		public void TestWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate_IgnoreWorkOrderLine()
		{
			TestIgnoreLines(Factory.New<WhsWorkOrderLine>());
		}

		void TestIgnoreLines(WhsDocketLine docketLine)
		{
			var strategy = ObjectFactory.Get<IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate>() as IDeferTriggerConditionStrategy;
			AssertEquals(false, strategy.ShouldDeferTrigger(docketLine));

			docketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			AssertEquals(false, strategy.ShouldDeferTrigger(docketLine));
		}
	}
}
