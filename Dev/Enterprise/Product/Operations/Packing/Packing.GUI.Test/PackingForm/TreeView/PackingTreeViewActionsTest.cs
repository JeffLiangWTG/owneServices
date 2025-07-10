using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI.Testing
{
	public class PackingTreeViewActionsTest : PackingTestCaseWithFactory
	{
		#region TestBreakDownPackages

		public void TestBreakDownPackages()
		{
			int breakDownPackagesFailedCount = 0;

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("CNT");
			var package2 = Data.PackageJob.Packages.AddNew("CNT");

			var actionStrategyNoPackUnpack = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package1, new PackageActionStrategy(package1, PackageAction.PackUnpack, "Cannot breakdown.") }
			};
			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package1, new PackageActionStrategy(package1) }
			};

			using (var treeView = new PackingTreeView())
			using (var node1 = new PackingTreeNode(package1))
			{
				treeView.Populate(Data.PackageJob);
				treeView.SelectedNode = node1;

				var treeViewAction = new PackingTreeViewActions(treeView);

				treeViewAction.BreakDownFailed += (sender, args) =>
				{
					breakDownPackagesFailedCount++;

					AssertEquals(false, args.IsScanPacking);
					AssertEquals("Cannot breakdown.", args.Message);
				};

				Data.Dummy.SetPackageActionResponses(actionStrategyNoPackUnpack);
				treeViewAction.BreakDownPackages();
				AssertEquals("BreakDownPackages was canceled, BreakDownFailed should have fired.", 1, breakDownPackagesFailedCount);

				Data.Dummy.SetPackageActionResponses(actionStrategyNone);
				treeViewAction.BreakDownPackages();
				AssertEquals("BreakDownPackages was not canceled, BreakDownFailed should not have fired.", 1, breakDownPackagesFailedCount);

				Assert("Should load and show BreakDownPackageDialog", ZFormModaliser.LastFormShownDialogForTest is BreakDownPackageDialog);
			}
		}

		#endregion

		#region TestNoAddingInnerPackageToReadOnlyPackage

		public void TestNoAddingInnerPackageToReadOnlyPackage()
		{
			ZInt failedAddInner = 0;
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("CNT");
			package1.ReadOnly = true;

			using (var treeView = new PackingTreeView())
			using (var node1 = new PackingTreeNode(package1))
			{
				treeView.Populate(Data.PackageJob);
				treeView.SelectedNode = node1;

				var treeViewAction = new PackingTreeViewActions(treeView);

				treeViewAction.PackOrUnpackFailed += (sender, args) =>
				{
					failedAddInner++;
					AssertEquals(false, args.IsScanPacking);
					AssertEquals("Cannot Add an Inner Package to the selected Read-only Package.", args.Message);
				};

				treeViewAction.AddNewPackageAsInner();
				AssertEquals("ReadOnly package, PackOrUnpackFailed should have fired.", 1, failedAddInner);
			}
		}

		#endregion

		#region TestAddingNewPackageWhenSelectingRoot

		public void TestAddingNewPackageWhenSelectingRoot()
		{
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			dummyPackingParent.JobNoForPackingParent = "abc";
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(packageJob);
				treeView.SelectedNode = topNode;

				var treeViewAction = new PackingTreeViewActions(treeView);

				AssertNoExceptionThrown(() => treeViewAction.AddNewPackageAsInner()); // menu always call this action even when we select parent (when we select root it will add package not inner package)

				var package = packageJob.Packages.Single();
				treeView.SelectedNode = treeView.FindNodeByPackage(package);
				treeViewAction.AddNewPackageAsInner();
				AssertEquals("One package should be added as an inner package.", 1, package.Packages.Count);
			}
		}

		#endregion

		#region TestPackWithPackableItemParentWithNoPackableItem

		public void TestPackWithPackableItemParentWithNoPackableItem()
		{
			var failedToPack = 0;
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			var line1 = dummyPackingParent.Lines.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				AssertEquals("Packable item parent has packable items when tree view is created.", 1, line1.PackableItems.Count());
				treeView.Populate(packageJob);
				treeView.SelectedNode = topNode;
				((DummyPackableItemParent)line1).RemoveItem((DummyPackableItem)line1.PackableItems.First());
				line1.TotalQty = 0m;

				AssertEquals("Precondition: Packable item parent has no packable items.", 0, line1.PackableItems.Count());
				AssertEquals("Precondition: Packable item parent has 0 quantity.", 0m, ((DummyPackableItemParent)line1).TotalQty);

				var treeViewAction = new PackingTreeViewActions(treeView);
				treeViewAction.PackOrUnpackFailed += (sender, args) =>
				{
					failedToPack++;
					AssertEquals(false, args.IsScanPacking);
					AssertEquals("There are no items to pack. Someone has made changes to the items to pack. Please close and re-open the form.", args.Message);
				};

				AssertNoExceptionThrown("No exception is thrown.", () => treeViewAction.Pack(dummyPackingParent.Lines, null));
				AssertEquals("PackOrUnpackFailed was fired.", 1, failedToPack);
			}
		}

		#endregion

		#region TestPackViaScanWithPackableItemParentWithNoPackableItem

		public void TestPackViaScanWithPackableItemParentWithNoPackableItem()
		{
			var failedToPack = 0;
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			var line1 = dummyPackingParent.Lines.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				// This also populates the packable item parents collection while the packable item parents still have packable items.
				// In the issue on WI00220187, the failure happened while creating the wrappers on populating the PackableItemParentWrapperForBarcodeInformation 
				// during the check if scanned barcode is valid or not.
				AssertEquals("Package job has a packable item parent.", 1, packageJob.PackableItemParents.Count);

				AssertEquals("Packable item parent has packable items when tree view is created.", 1, line1.PackableItems.Count());
				treeView.Populate(packageJob);
				treeView.SelectedNode = topNode;
				((DummyPackableItemParent)line1).RemoveItem((DummyPackableItem)line1.PackableItems.First());
				line1.TotalQty = 0m;

				AssertEquals("Precondition: Packable item parent has no packable items.", 0, line1.PackableItems.Count());
				AssertEquals("Precondition: Packable item parent has 0 quantity.", 0m, ((DummyPackableItemParent)line1).TotalQty);

				var treeViewAction = new PackingTreeViewActions(treeView);
				treeViewAction.PackOrUnpackFailed += (sender, args) =>
				{
					failedToPack++;
					AssertEquals(true, args.IsScanPacking);
					AssertEquals("There are no items to pack. Someone has made changes to the items to pack. Please close and re-open the form.", args.Message);
				};

				AssertNoExceptionThrown("No exception is thrown.", () => treeViewAction.PackViaScan("TEST123", false));
				AssertEquals("PackOrUnpackFailed was fired.", 1, failedToPack);
			}
		}

		#endregion
	}
}
