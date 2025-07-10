using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	public class WhsTotePackageStrategyTest : WhsTestCaseWithFactory
	{
		#region TestIPackingParentGetPackageActionStrategy_ToteJob

		public void TestIPackingParentGetPackageActionStrategy_ToteJob_EmptyPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT");
			package.SetIsTote(true);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_ToteJob_PackedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			package.SetIsTote(true);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be false for a package with packed items", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be false for a package with packed items", false, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_ToteJob_PickedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			package.SetIsTote(true);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true if the package is picked", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true if the package is picked", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_ToteJob_LineOnPackageBeingPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			package.SetIsTote(true);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_IsPicking = true;
			order.Lines[0].PickLines[0].WZ_GS_NKAssignedTo = "AUS";
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_ToteJob_PackageOnlyPartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			package.SetIsTote(true);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.Pack(order.Lines[1].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		#endregion
	}
}
