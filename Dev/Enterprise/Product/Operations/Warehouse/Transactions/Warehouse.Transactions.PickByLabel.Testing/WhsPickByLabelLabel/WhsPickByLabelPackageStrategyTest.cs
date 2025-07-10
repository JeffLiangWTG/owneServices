using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	public class WhsPickByLabelPackageStrategyTest : WhsTestCaseWithFactory
	{
		#region TestIPackingParentGetPackageActionStrategy_PickByLabelJob

		public void TestIPackingParentGetPackageActionStrategy_PickByLabelJob_EmptyPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT");
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
		}

		public void TestIPackingParentGetPackageActionStrategy_PickByLabelJob_PackedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();
			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be false when there are packed items", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("ReasonForNotAllowingAction doesn't match", string.Format("Cannot modify the selected {0} because it has Pick By Label Job and packed items that are not all picked.", package.KP_F3_NKPackType), packageActionStrategy.ReasonForNotAllowingAction);

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should still be false", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("ReasonForNotAllowingAction doesn't match", string.Format("Cannot modify the selected {0} because it has Pick By Label Job and packed items that are not all picked.", package.KP_F3_NKPackType), packageActionStrategy.ReasonForNotAllowingAction);
		}

		public void TestIPackingParentGetPackageActionStrategy_PickByLabelJob_PickedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true when package is empty", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true when package is empty", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be true for a picked package with packed items", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for a picked package with packed items", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_PickByLabelJob_LineOnPackageBeingPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);

			AssertEquals("packageActionStrategy.AllowDelete should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be true for an empty package", true, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));

			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_GS_NKAssignedTo = "AUS";
			order.Lines[0].PickLines[0].WZ_IsPicking = true;
			Factory.Save();

			packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
			AssertEquals("packageActionStrategy.AllowDelete should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("packageActionStrategy.AllowPackUnpack should be false for a package that is being picked", false, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
		}

		public void TestIPackingParentGetPackageActionStrategy_PickByLabelJob_PackageOnlyPartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("PLT");
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
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

		#region TestIPackingParentOnDelete_PackageAssignedToActivePickByLabelJob

		public void TestIPackingParentOnDelete_PackageAssignedToActivePickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			package.Delete();
			AssertEquals("pickByLabelJob NOT deleted.", false, pickByLabelJob.IsDeleted);
			AssertEquals("pickByLabelJob labels collection is NOT empty.", 1, pickByLabelJob.Labels.Count);
			AssertEquals("package NOT deleted.", false, package.IsDeleted);

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Should have finalised the pick by label job.", false, pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			Factory.Save();

			package.ClearActionStrategyCacheIncludingChildren();
			package.Delete();
			AssertEquals("pickByLabelJob is NOT deleted.", false, pickByLabelJob.IsDeleted);
			AssertEquals("package is NOT deleted.", false, package.IsDeleted);
			AssertEquals("pickByLabelJob labels collection is NOT empty.", 1, pickByLabelJob.Labels.Count);

			package.Unpack(package.PackedItems[0], 5m);
			Factory.Save();

			package.ClearActionStrategyCacheIncludingChildren();
			package.Delete();
			AssertEquals("pickByLabelJob is NOT deleted.", false, pickByLabelJob.IsDeleted);
			AssertEquals("package is deleted.", true, package.IsDeleted);
			AssertEquals("pickByLabelJob labels collection is empty.", 0, pickByLabelJob.Labels.Count);
		}

		#endregion
	}
}
