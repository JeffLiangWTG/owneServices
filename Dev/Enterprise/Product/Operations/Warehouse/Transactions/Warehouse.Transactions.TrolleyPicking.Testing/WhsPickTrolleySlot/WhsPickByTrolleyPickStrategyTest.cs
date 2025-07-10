using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	class WhsPickByTrolleyPickStrategyTest : WhsTestCaseWithFactory
	{
		#region CanDetachOrder

		public void TestIsOrderActionAllowed_DetachOrder_OrderHasActiveTrolleyJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be false", false,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be true", true,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			AssertEquals("PickByTrolleyPickStrategy should not allow the order to be detached", false, strategy.IsOrderActionAllowed(order, PickOrderAction.DetachOrder, out var reasonNotAllowed));
			AssertNotNull("Error notification should not be null", reasonNotAllowed);
			AssertEquals("Error notification should contain the expected error message",
				"Error: Cannot perform this operation because the order has packages assigned to a trolley job.",
				reasonNotAllowed.Message);
		}

		public void TestIsOrderActionAllowed_DetachOrder_OrderHasNoActiveTrolleyJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be false", false,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			AssertEquals("PickByTrolleyPickStrategy should allow the order to be detached", true, strategy.IsOrderActionAllowed(order, PickOrderAction.DetachOrder, out var reasonNotAllowed));
			AssertNull("ErrorNotification should be null", reasonNotAllowed);
		}

		#endregion

		#region TestAnyPackageHasActiveTrolleyJob

		public void TestAnyPackageHasActiveTrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("AnyPackageHasActiveTrolleyJob should be false", false, strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			var trolleySlot = (WhsPickTrolleySlot)Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("AnyPackageHasActiveTrolleyJob should be true", true, strategy.HasAnyPackageAssignedToATrolleyJob(order));

			trolleyJob.WTJ_Status = "FIN";
			Factory.Save();
			AssertEquals("AnyPackageHasActiveTrolleyJob should be true", true, strategy.HasAnyPackageAssignedToATrolleyJob(order));

			trolleySlot.Delete();
			Factory.Save();
			AssertEquals("AnyPackageHasActiveTrolleyJob should be false", false, strategy.HasAnyPackageAssignedToATrolleyJob(order));
		}

		#endregion

		#region TestIsPackageAssignedToTrolleyJob

		public void TestIsPackageAssignedToTrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("AnyPackageHasActiveTrolleyJob should be false", false, strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();
			var trolleySlot = (WhsPickTrolleySlot)Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Factory.Save();

			AssertEquals("IsPackageAssignedToTrolleyJob should be true", true, strategy.IsPackageAssignedToTrolleyJob(order, package1));
			AssertEquals("IsPackageAssignedToTrolleyJob should be false", false, strategy.IsPackageAssignedToTrolleyJob(order, package2));

			trolleyJob.WTJ_Status = "FIN";
			Factory.Save();
			AssertEquals("IsPackageAssignedToTrolleyJob should be true", true, strategy.IsPackageAssignedToTrolleyJob(order, package1));
			AssertEquals("IsPackageAssignedToTrolleyJob should be false", false, strategy.IsPackageAssignedToTrolleyJob(order, package2));

			trolleySlot.Delete();
			Factory.Save();
			AssertEquals("IsPackageAssignedToTrolleyJob should be false", false, strategy.IsPackageAssignedToTrolleyJob(order, package1));
			AssertEquals("IsPackageAssignedToTrolleyJob should be false", false, strategy.IsPackageAssignedToTrolleyJob(order, package2));
		}

		#endregion

		#region TestHasAnyPackagesAssignedToATrolleyJob

		public void TestHasAnyPackagesAssignedToATrolleyJob_NoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var strategy = new WhsPickByTrolleyPickStrategy();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertNoExceptionThrown(() => { _ = strategy.HasAnyPackageAssignedToATrolleyJob(order); });
		}

		#endregion

		#region TestCancelPick_OrderHasActiveTrolleyJob

		public void TestCancelPick_OrderHasActiveTrolleyJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be false", false,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be true", true,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			AssertNoExceptionThrown("No exception is thrown.", () => pick.CancelPick());
			AssertEquals("Pick is not cancelled.", false, pick.IsCancelled);
		}

		#endregion
	}
}
