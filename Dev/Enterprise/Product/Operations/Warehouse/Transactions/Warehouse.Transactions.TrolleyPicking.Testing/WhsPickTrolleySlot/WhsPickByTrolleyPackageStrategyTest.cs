using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	public class WhsPickByTrolleyPackageStrategyTest : WhsTestCaseWithFactory
	{
		#region TestIPackingParentGetPackageActionStrategy

		public void TestIPackingParentGetPackageActionStrategy_ActiveTrolleyJob_EmptyPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			var packageStrategy = new WhsPickByTrolleyPackageStrategy();

			var packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, true, true, package.KP_F3_NKPackType);

			trolleyJob.WTJ_Status = "FIN";
			Factory.Save();

			packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, true, true, package.KP_F3_NKPackType);
		}

		public void TestIPackingParentGetPackageActionStrategy_ActiveTrolleyJob_PackedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			var packageStrategy = new WhsPickByTrolleyPackageStrategy();

			var packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, false, false, package.KP_F3_NKPackType);

			trolleyJob.WTJ_Status = "FIN";
			Factory.Save();

			packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, false, false, package.KP_F3_NKPackType);

			var package2 = order.PackageJob.Packages.AddNew();
			packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package2);
			AssertNull("WhsPickByTrolleyPackageStrategy should return null if the package is not associated with a trolley job.", packageActionStrategy);
		}

		public void TestIPackingParentGetPackageActionStrategy_ActiveTrolleyJob_PickedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var packageStrategy = new WhsPickByTrolleyPackageStrategy();
			var packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, true, true, package.KP_F3_NKPackType);
		}

		public void TestIPackingParentGetPackageActionStrategy_ActiveTrolleyJob_LineOnPackageBeingPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			order.Lines[0].PickLines[0].WZ_IsPicking = true;
			order.Lines[0].PickLines[0].WZ_GS_NKAssignedTo = "AUS";
			Factory.Save();

			var packageStrategy = new WhsPickByTrolleyPackageStrategy();
			var packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, false, false, package.KP_F3_NKPackType);
		}

		public void TestIPackingParentGetPackageActionStrategy_ActiveTrolleyJob_PackageOnlyPartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.Pack(order.Lines[1].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var packageStrategy = new WhsPickByTrolleyPackageStrategy();
			var packageActionStrategy = packageStrategy.GetPackageActionStrategy(order, package);
			AssertPackageActionStrategy(packageActionStrategy, false, false, package.KP_F3_NKPackType);
		}

		void AssertPackageActionStrategy(IPackageActionStrategy packageActionStrategy, bool allowPackUnpack, bool allowDelete, ZString packType)
		{
			AssertEquals("packageActionStrategy.AllowDelete should be " + allowDelete, allowDelete, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be " + allowPackUnpack, allowPackUnpack, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			if (!allowDelete)
			{
				AssertEquals("ReasonForNotAllowingAction doesn't match", string.Format("Cannot modify the selected {0} because it has Trolley Job and packed items that are not all picked.", packType), packageActionStrategy.ReasonForNotAllowingAction);
			}
		}

		#endregion

		#region TestIPackingParentOnDelete_PackageAssignedToActiveTrolley

		public void TestIPackingParentOnDelete_PackageAssignedToActiveTrolley_HasPackedItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			var slot = Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			package.Delete();
			AssertEquals(false, ((BusinessObject)slot).IsDeleted);
			AssertEquals(false, package.IsDeleted);

			trolleyJob.WTJ_Status = "FIN";
			Factory.Save();

			package.ClearActionStrategyCacheIncludingChildren();
			package.Delete();
			AssertEquals(false, package.IsDeleted);
			AssertEquals(false, ((BusinessObject)slot).IsDeleted);
		}

		public void TestIPackingParentOnDelete_PackageAssignedToActiveTrolley_NoPackedItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			var slot = Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			package.Delete();
			AssertEquals(true, package.IsDeleted);
			AssertEquals(true, ((BusinessObject)slot).IsDeleted);
		}

		#endregion
	}
}
