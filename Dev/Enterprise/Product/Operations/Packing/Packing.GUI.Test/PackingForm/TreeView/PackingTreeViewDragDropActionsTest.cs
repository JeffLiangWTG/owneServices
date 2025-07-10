using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	class PackingTreeViewDragDropActionsTest : PackingTestCaseWithFactory
	{
		#region TestIsValidSource

		#region PackableItemParentWrapper

		public void TestIsValidSource_PackableItemParentWrapper()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var packableItemParentWrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
				var elements_PackageItemParentWrapper = new ArrayList(packableItemParentWrapper);
				AssertEquals(false, dragDropAction.IsValidSource(elements_PackageItemParentWrapper));

				var packableItemParentWrapper2 = Data.DummyLine1Wrapper;
				AssertEquals(packageJob.ParentJob, packableItemParentWrapper2.ParentJob);
				var elements_PackageItemParentWrapper2 = new ArrayList(packableItemParentWrapper2);
				AssertEquals(true, dragDropAction.IsValidSource(elements_PackageItemParentWrapper2));
			}
		}

		#endregion

		#region PkgPackageItemDivotsWrapper

		public void TestIsValidSource_PkgPackageItemDivotsWrapper()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var package = Factory.New<PkgPackage>();
				var divot = package.PackedItemDivots.AddNew();
				var packableItemParent = Factory.New<DummyPackableItemParent>();
				var divotWrapper = PkgPackageItemDivotsWrapper.New(divot, packableItemParent);
				var elements_PkgPackageItemDivotsWrapper = new ArrayList(divotWrapper);
				AssertEquals(false, dragDropAction.IsValidSource(elements_PkgPackageItemDivotsWrapper));

				package.KP_KJ_ParentPackageJob = packageJob.PK;
				AssertEquals(packageJob, divotWrapper.ParentPackage.PackageJob);
				AssertEquals(true, dragDropAction.IsValidSource(elements_PkgPackageItemDivotsWrapper));
			}
		}

		#endregion

		#region PkgPackage

		public void TestIsValidSource_PkgPackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var package = Factory.New<PkgPackage>();
				var elements_PkgPackage = new ArrayList(package);
				AssertEquals(false, dragDropAction.IsValidSource(elements_PkgPackage));

				package.KP_KJ_ParentPackageJob = packageJob.PK;
				AssertEquals(packageJob, package.PackageJob);
				AssertEquals(true, dragDropAction.IsValidSource(elements_PkgPackage));
			}
		}

		#endregion

		#region PkgPackageHeader

		public void TestIsValidSource_PkgPackageHeader()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var packageHeader = Helper.CreatePackageHeader(packageJob, "123");
				var packageHeaderPivot = Helper.CreatePackageHeaderPivot(packageJob, packageHeader);
				var elements_PkgPackageHeaderPivot = new ArrayList(packageHeaderPivot);
				AssertEquals(true, dragDropAction.IsValidSource(elements_PkgPackageHeaderPivot));

				var package = packageJob.Packages.AddNew();
				packageJob.AssignPackageIDs(package, new[] { packageHeader });
				AssertEquals(false, dragDropAction.IsValidSource(elements_PkgPackageHeaderPivot));
			}
		}

		#endregion

		#region TestIsValidSource_WhenThereAreNoElements

		public void TestIsValidSource_WhenThereAreNoElements()
		{
			using (var treeView = new PackingTreeView())
			{
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);
				AssertEquals(false, dragDropAction.IsValidSource(new ArrayList()));
			}
		}

		#endregion

		#endregion

		#region TestHandleDragDropOfPkgPackage

		public void TestHandleDragDropOfPkgPackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);

				var pallet = packageJob.Packages.AddNew("PLT", 10);
				var carton = packageJob.Packages.AddNew("CTN", 10);
				var box = packageJob.Packages.AddNew("BOX", 10);
				var keg = packageJob.Packages.AddNew("KEG", 10);
				carton.KP_Weight = 1m;
				box.KP_Weight = 0.5m;
				keg.KP_Weight = 2m;

				var palletNode = treeView.PopulateFromPackage(pallet, treeView.TopNode);
				var cartonNode = treeView.PopulateFromPackage(carton, treeView.TopNode);
				var boxNode = treeView.PopulateFromPackage(box, treeView.TopNode);
				var kegNode = treeView.PopulateFromPackage(keg, treeView.TopNode);
				AssertEquals("Precondition: Outer Nodes have been added.", 4, treeView.TopNode.Nodes.Count);

				var destinationNode = palletNode;
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var packageDataChangedHitCount = 0;
				var massPackageProcessFinishedHitCount = 0;
				var listChangedHitCount = 0;
				Data.PackageJob.PackageDataChanged += (sender, e) =>
				{
					if (!Data.PackageJob.IsMassPackageProcessRunning)
					{
						packageDataChangedHitCount++;
					}
				};

				Data.PackageJob.MassPackageProcessFinished += (sender, e) => massPackageProcessFinishedHitCount++;
				((IBindingList)pallet.Packages).ListChanged += (sender, e) => listChangedHitCount++;

				var listChangedDuringDragDrop = 0;
				carton.KP_KP_ParentPackageInfo.ValueChanged += (sender, e) =>
				{
					listChangedDuringDragDrop = listChangedHitCount;

					using (carton.SuspendValidationTesting())
					{
						carton.KP_GS_NKReleasedByInfo.AddError("Some Error");
					}
				};

				AssertPersistentPropertiesHitCount("Many hits are related to Error Checking. This is just to ensure the Sort Properties are cached.", 251,
					() => dragDropAction.HandleDragDropOfNodes(new[] { cartonNode, boxNode, kegNode }, destinationNode));

				AssertEquals("Package Data Changed should not fire during Drag Drop.", 0, packageDataChangedHitCount);
				AssertEquals("Mass Package Process Finished should fire after Drag Drop.", 1, massPackageProcessFinishedHitCount);
				AssertEquals("Notifications Changed should be Suspended for the Tree.", cartonNode.NormalImageIndex, cartonNode.ImageIndex);
				AssertEquals("Notifications Changed should be Suspended for the Tree.", palletNode.NormalImageIndex, palletNode.ImageIndex);
				AssertEquals("Notifications Changed should be Suspended for the Tree.", treeView.TopNode.NormalImageIndex, treeView.TopNode.ImageIndex);
				AssertEquals("List Change should be suspended during Drag Drop.", 0, listChangedDuringDragDrop);
				AssertEquals("List Change should be called after Drag Drop.", 1, listChangedHitCount);
			}
		}

		public void TestHandleDragDropOfPkgPackage_OnlyOnePackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);

				var pallet = packageJob.Packages.AddNew("PLT", 10);
				var carton = packageJob.Packages.AddNew("CTN", 10);
				carton.KP_Weight = 1m;

				var palletNode = treeView.PopulateFromPackage(pallet, treeView.TopNode);
				var cartonNode = treeView.PopulateFromPackage(carton, treeView.TopNode);
				AssertEquals("Precondition: Outer Nodes have been added.", 2, treeView.TopNode.Nodes.Count);

				var destinationNode = palletNode;
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);

				var packageDataChangedHitCount = 0;
				var massPackageProcessFinishedHitCount = 0;
				Data.PackageJob.PackageDataChanged += (sender, e) =>
				{
					if (!Data.PackageJob.IsMassPackageProcessRunning)
					{
						packageDataChangedHitCount++;
					}
				};

				Data.PackageJob.MassPackageProcessFinished += (sender, e) => massPackageProcessFinishedHitCount++;

				dragDropAction.HandleDragDropOfNodes(new[] { cartonNode }, destinationNode);

				AssertEquals("Package Data Changed should fire during Drag Drop for just one Package.", 1, packageDataChangedHitCount);
				AssertEquals("Mass Package Process should not get fired for just one package.", 0, massPackageProcessFinishedHitCount);
			}
		}

		#endregion

		#region TestHandleDragDropOfPkgPackageHeader

		#region TestHandleDragDropOfPkgPackageHeader

		public void TestHandleDragDropOfPkgPackageHeader_NumberOfIDsLessThanQty()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(Data.PackageJob);

				var packageHeader1 = Helper.CreatePackageHeader(packageJob, "123");
				var packageHeaderPivot1 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader1);
				var pallet = packageJob.Packages.AddNew("PLT");
				pallet.KP_PackageQty = 10;
				AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet.KP_PackageID);

				treeView.Populate(Data.PackageJob);
				AssertEquals("Pre-condition: Number of packages on this job should be 1.", 1, packageJob.Packages.Count);

				var destinationNode = treeView.TopNode.TreeView.FindNodeByPackage(pallet);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);
				var packageHeadersPivotToMove = new List<PkgPackageJobPackageHeaderPivot>() { packageHeaderPivot1 };

				dragDropAction.HandleDragDropOfPkgPackageHeader(packageHeadersPivotToMove.AsReadOnly(), destinationNode);

				AssertEquals("Number of packages after assigning Package IDs should be 2.", 2, packageJob.Packages.Count);
				AssertEquals("", pallet.KP_PackageID);
				AssertEquals(9, pallet.KP_PackageQty);

				var package123 = packageJob.Packages.Single(p => p.KP_PackageID == "123");
				AssertEquals("Package ID: 123 has been assigned.", 1, package123.KP_PackageQty);
				AssertEquals("Destination tree node should be selected.", true, destinationNode.IsSelected);
			}
		}

		#endregion

		#region TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs

		public void TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(Data.PackageJob);

				var packageHeader1 = Helper.CreatePackageHeader(packageJob, "001");
				var packageHeaderPivot1 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader1);
				var packageHeader2 = Helper.CreatePackageHeader(packageJob, "002");
				var packageHeaderPivot2 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader2);
				var pallet = packageJob.Packages.AddNew("PLT");
				pallet.KP_PackageQty = 10;
				AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet.KP_PackageID);

				treeView.Populate(Data.PackageJob);
				AssertEquals("Pre-condition: Number of packages on this job should be 1.", 1, packageJob.Packages.Count);

				var destinationNode = treeView.TopNode.TreeView.FindNodeByPackage(pallet);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);
				var packageHeaderPivotsToMove = new List<PkgPackageJobPackageHeaderPivot>() { packageHeaderPivot1, packageHeaderPivot2 };

				dragDropAction.HandleDragDropOfPkgPackageHeader(packageHeaderPivotsToMove.AsReadOnly(), destinationNode);

				AssertEquals("Number of packages after assigning Package IDs should be 3.", 3, packageJob.Packages.Count);
				AssertEquals("Package ID: 001 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "001").KP_PackageQty);
				AssertEquals("Package ID: 002 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "002").KP_PackageQty);

				var remainderPackage = packageJob.Packages.Single(p => p.KP_PackageID.IsEmpty);
				AssertEquals("Remaining Qty of unassigned package should be 8.", 8, remainderPackage.KP_PackageQty);
			}
		}

		#endregion

		#region TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs_IncludeAlreadyAssignedID

		public void TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs_IncludeAlreadyAssignedID()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(Data.PackageJob);

				var packageHeader1 = Helper.CreatePackageHeader(packageJob, "001");
				var packageHeaderPivot1 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader1);

				var packageHeader2 = Helper.CreatePackageHeader(packageJob, "002");
				var packageHeaderPivot2 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader2);

				var packageHeader3 = Helper.CreatePackageHeader(packageJob, "003");
				var packageHeaderPivot3 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader3);

				// pallet001 --> packageHeader1 ("001")
				var pallet001 = packageJob.Packages.AddNew("PLT", 1);
				pallet001.KP_KPH_PackageHeader = packageHeader1.PK;

				var pallet = packageJob.Packages.AddNew("PLT");
				pallet.KP_PackageQty = 10;
				AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet.KP_PackageID);

				treeView.Populate(Data.PackageJob);

				var palletNode = treeView.TopNode.TreeView.FindNodeByPackage(pallet);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);
				var packageHeaderPivotsToMove = new List<PkgPackageJobPackageHeaderPivot>() { packageHeaderPivot1, packageHeaderPivot2, packageHeaderPivot3 };

				dragDropAction.HandleDragDropOfPkgPackageHeader(packageHeaderPivotsToMove.AsReadOnly(), palletNode);

				AssertEquals("Number of packages after assigning Package IDs should be 4.", 4, packageJob.Packages.Count);
				AssertEquals("Package ID: 001 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "001").KP_PackageQty);
				AssertEquals("Package ID: 002 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "002").KP_PackageQty);
				AssertEquals("Package ID: 003 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "003").KP_PackageQty);

				var remainderPackage = packageJob.Packages.Single(p => p.KP_PackageID.IsEmpty);
				AssertEquals("Remaining Qty of unassigned package should be 8.", 8, remainderPackage.KP_PackageQty);
			}
		}

		#endregion

		#region TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs_IDsCountMoreThanPackageQty

		public void TestHandleDragDropOfPkgPackageHeader_MultiplePackageIDs_IDsCountMoreThanPackageQty()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(Data.PackageJob);

				var packageHeader1 = Helper.CreatePackageHeader(packageJob, "001");
				var packageHeaderPivot1 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader1);

				var packageHeader2 = Helper.CreatePackageHeader(packageJob, "002");
				var packageHeaderPivot2 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader2);

				var packageHeader3 = Helper.CreatePackageHeader(packageJob, "003");
				var packageHeaderPivot3 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader3);

				AssertEquals(3, packageJob.LoosePackageIDs.Count);

				var pallet = packageJob.Packages.AddNew("PLT");
				pallet.KP_PackageQty = 2;
				AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet.KP_PackageID);

				treeView.Populate(Data.PackageJob);

				var palletNode = treeView.TopNode.TreeView.FindNodeByPackage(pallet);
				var dragDropAction = new PackingTreeViewDragDropActions(treeView);
				var packageHeaderPivotsToMove = new List<PkgPackageJobPackageHeaderPivot>() { packageHeaderPivot1, packageHeaderPivot2, packageHeaderPivot3 };

				dragDropAction.HandleDragDropOfPkgPackageHeader(packageHeaderPivotsToMove.AsReadOnly(), palletNode);

				AssertEquals("Number of packages should be 2.", 2, packageJob.Packages.Count);
				AssertEquals("Package ID: 001 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "001").KP_PackageQty);
				AssertEquals("Package ID: 002 has been assigned.", 1, packageJob.Packages.Single(p => p.KP_PackageID == "002").KP_PackageQty);

				AssertEquals("Package ID: 003 should not be assigned.", 0, packageJob.Packages.Count(p => p.KP_PackageID == "003"));
				AssertEquals("Should have 1 Package ID left to be assigned.", 1, packageJob.LoosePackageIDs.Count);
			}
		}

		#endregion

		#endregion
	}
}
