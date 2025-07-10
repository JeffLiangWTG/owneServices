using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	public class PackageFinderTest : WhsTestCaseWithFactory
	{
		#region TestArgumentException

		public void TestArgumentNullException_Factory()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageFinder(null, "PACKAGE-1", ZGuid.NewZGuid(), GlbStaff.CurrentUser));
		}

		public void TestArgumentException_Staff()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageFinder(new BusinessObjectFactory(), "PACKAGE-1", ZGuid.NewZGuid(), null));
		}

		public void TestArgumentException_PackageID()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => new PackageFinder(new BusinessObjectFactory(), null, ZGuid.NewZGuid(), GlbStaff.CurrentUser));
			AssertExceptionThrown(typeof(ArgumentException), () => new PackageFinder(new BusinessObjectFactory(), "", ZGuid.NewZGuid(), GlbStaff.CurrentUser));
		}

		#endregion

		#region TestPackagePK

		public void TestPackagePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var packingHelper = new Packing.Business.Testing.PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);

			Factory.Save();

			AssertEquals("Should find the package.", true, new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser).FoundPackage);
			AssertEquals("When package not found it should return null.", false, new PackageFinder(Factory, "PACKAGE-2", data.Whs1.PK, GlbStaff.CurrentUser).FoundPackage);
		}

		#endregion

		#region TestPickDockDoorLocationPK

		public void TestPickDockDoorLocationPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var packingHelper = new Packing.Business.Testing.PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);
			Factory.Save();

			AssertEquals("Should find the package and related dock door location.", pick.WP_WL_DockDoor, new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser).PickDockDoorLocationPK);
			AssertEquals("Should not find package and return null without exception.", ZGuid.Empty, new PackageFinder(Factory, "PACKAGE-2", data.Whs1.PK, GlbStaff.CurrentUser).PickDockDoorLocationPK);
		}

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var packingHelper = new Packing.Business.Testing.PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);

			Factory.Save();

			AssertEquals("Should find the package.", package, new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser).Package);
			AssertNull("Should not find package and return null without exception.", new PackageFinder(Factory, "PACKAGE-2", data.Whs1.PK, GlbStaff.CurrentUser).Package);
		}

		#endregion

		#region TestPackageIsPickedAndPutaway

		public void TestPackageIsPickedAndPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var packingHelper = new Packing.Business.Testing.PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine = order.Lines[0];
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, orderLine.PickLines[0]);
			Factory.Save();

			var finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("Is not picked and putaway.", false, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", true, finder.FoundPackage);

			helper.PickAndMakeInTransitTransfer(orderLine.PickLines[0], ZDateTimeOffset.Now);
			Factory.Save();

			finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("Is picked and not putaway.", false, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", true, finder.FoundPackage);

			var dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			dockDoorTransferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(dockDoorTransferLine);
			Factory.Save();

			finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("Is picked and putaway.", true, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", false, finder.FoundPackage);
		}

		public void TestPackageIsPicked_PartiallyPickAndPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			var packingHelper = new Packing.Business.Testing.PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, orderLine.PickLines[0]);
			packingHelper.CreatePackageDivot(package, orderLine.PickLines[1]);
			Factory.Save();

			var finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("Is not picked and putaway.", false, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", true, finder.FoundPackage);

			helper.PickAndMakeInTransitTransfer(orderLine.PickLines[0], ZDateTimeOffset.Now);
			Factory.Save();

			var dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			dockDoorTransferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(dockDoorTransferLine);
			Factory.Save();

			finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("There is line is not picked and putaway.", false, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", true, finder.FoundPackage);

			helper.PickAndMakeInTransitTransfer(orderLine.PickLines[1], ZDateTimeOffset.Now);
			Factory.Save();

			finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("There is line is picked but not putaway.", false, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", true, finder.FoundPackage);

			dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => !l.IsFinalised);
			dockDoorTransferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(dockDoorTransferLine);
			Factory.Save();

			finder = new PackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("All items are picked and putaway.", true, finder.IsPickedAndPutaway);
			AssertEquals("Should find the package.", false, finder.FoundPackage);
		}

		#endregion
	}
}
