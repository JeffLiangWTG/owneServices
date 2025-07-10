using System.Linq;
using Enterprise.Warehouse.Environment.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAutoCycleCountTaskCreatorTest : WhsTestCaseWithFactory
	{
		public void TestCycleCountPriority_CreateInventoryAccuracyManagementTasksForAutoTouchCount()
		{
			var cycleCountLocationCreatorMock = new Mock<IWhsCycleCountLocationCreator>();
			var autoCycleCountTaskCreator = new WhsAutoCycleCountTaskCreator(cycleCountLocationCreatorMock.Object);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 2;
			location.WLV_FinalisedPickCount = 2;

			var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location);
			receiveForOrg1.FinaliseDocket();
			Factory.Save();

			var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m);

			var finalisedPick = Helper.CreatePickNew(orderForOrg1);
			var orderedInventories = finalisedPick.OrderedInventories;
			var pickAvailableInventory = orderedInventories[0].AvailableInventories[0];
			pickAvailableInventory.VerifiedNonEmpty = true;
			var groupedAvailableInventoryLines = new[] { pickAvailableInventory }.Where(l => l.Allocate && l.Location.IsMaximumTouchCountUsed).GroupBy(l => l.Location);
			autoCycleCountTaskCreator.CreateInventoryAccuracyManagementTasksForAutoTouchCount(groupedAvailableInventoryLines, data.Whs1);
			cycleCountLocationCreatorMock.Verify(mock => mock.CreateCycleCountLocation(It.IsAny<WhsLocation>(), 0));
			cycleCountLocationCreatorMock.VerifyAll();
		}

		public void TestCycleCountPriority_CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation()
		{
			var cycleCountLocationCreatorMock = new Mock<IWhsCycleCountLocationCreator>();
			var autoCycleCountTaskCreator = new WhsAutoCycleCountTaskCreator(cycleCountLocationCreatorMock.Object);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 2;
			location.WLV_FinalisedPickCount = 2;

			var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location);
			receiveForOrg1.FinaliseDocket();
			Factory.Save();

			var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m);

			var finalisedPick = Helper.CreatePickNew(orderForOrg1);
			var orderedInventories = finalisedPick.OrderedInventories;
			var pickAvailableInventory = orderedInventories[0].AvailableInventories[0];
			pickAvailableInventory.VerifiedNonEmpty = true;
			var groupedAvailableInventoryLines = new[] { pickAvailableInventory }.Where(l => l.Allocate && l.Location.IsMaximumTouchCountUsed).GroupBy(l => l.Location);
			autoCycleCountTaskCreator.CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(groupedAvailableInventoryLines, data.Whs1);
			cycleCountLocationCreatorMock.Verify(mock => mock.CreateCycleCountLocation(It.IsAny<WhsLocation>(), 1));
			cycleCountLocationCreatorMock.VerifyAll();
		}
	}
}
