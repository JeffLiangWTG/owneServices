using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderHelperTest : WhsTestCaseWithFactory
	{
		public void TestAddNewInventoryFromWorkOrderLine_InvalidArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var kit = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(kit, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(kit, data.Part2, 1m, "UNT");

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, kit, 2m);
			var pick = Helper.CreatePickNew(workOrder);
			var otherPick = Factory.New<WhsPick>();
			var childComponentLine1 = (WhsWorkOrderLine)workOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK);
			var locationHelper = new WorkOrderStagingLocationHelper(data.Whs1);
			var newReceive = Factory.New<WhsReceive>();
			AssertExceptionThrown(typeof(ArgumentException), "Kit line or Component passed in must be for the same Pick.",
				() => WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(locationHelper, otherPick, newReceive, childComponentLine1, null, 2m, ZDateTimeOffset.Now));
			AssertExceptionThrown(typeof(ArgumentException), "Kit line or Component passed in must be for the same Pick.",
				() => WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(locationHelper, otherPick, newReceive, childComponentLine1, workOrderLine, 2m, ZDateTimeOffset.Now));
		}
	}
}
