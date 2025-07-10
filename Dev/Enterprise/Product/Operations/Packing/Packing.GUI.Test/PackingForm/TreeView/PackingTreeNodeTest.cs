using System.Collections.Generic;
using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	class PackingTreeNodeTest : PackingTestCaseWithFactory
	{
		#region TestPackedItemDivotsCountChangedEvent

		public void TestPackedItemDivotsCountChangedEvent()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				var box = packageJob.Packages.AddNew("BOX");
				AssertEquals("Precondition: Nothing is Packed.", 0, box.PackedItemDivots.Count);
				AssertEquals("Precondition: Nothing is Packed.", 0, box.PackedItems.Count); // need to poke PackedItems collection to hook event handler for updating divot wrappers

				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("1 Box on Package Job.", @"
- Dummy 
  - 1x Box
".TrimStart());

				var divot = Factory.New<PkgPackageItemDivot>();
				divot.KI_PackedQty = 10m;
				divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
				divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
				divot.KI_KP_Package = box.PK;
				treeView.AssertPackingTreeView("Tree should updated automatically with new divot.", @"
- Dummy 
  - 1x Box
    - 10x P1 - TV - Size: 63in
".TrimStart());
			}
		}

		#endregion

		#region TestPackedItemQuantityChangeEvent_NotThrowingException

		public void TestPackedItemQuantityChangeEvent_NotThrowingException()
		{
			Data.CreatePackingData();
			Data.DummyLine1.TotalQty = 2m;
			AssertNotNull(Data.DummyLine1.PackableItems.Single());
			AssertEquals(2m, Data.DummyLine1.PackableItems.Single().Quantity);

			var packableItem2 = Data.DummyLine1.AddNewPackableItem();
			packableItem2.Quantity = 2;
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			using (var topNode = new PackingTreeNode(packageJob))
			{
				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("Only has Package Job Node.", @"
- Dummy
".TrimStart());

				var box = packageJob.Packages.AddNew("BOX");
				var tVsInBox = box.Pack(Data.DummyLine1, 1m).Single();
				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("1 tv inside Box.", @"
- Dummy 
  - 1x Box 2 KG
    - 1x P1 - TV - Size: 63in
".TrimStart());

				var pallet = packageJob.Packages.AddNew("PLT");
				var tVsInPallet = pallet.Pack(Data.DummyLine1, 3m).Single();
				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("1 tv inside Box, 3 tv's inside Pallet.", @"
- Dummy 
  - 1x Box 2 KG
    - 1x P1 - TV - Size: 63in
  - 1x Pallet 6 KG
    - 3x P1 - TV - Size: 63in
".TrimStart());

				// move tv's in box to pallet (pallet already has a tv)
				var palletNode = treeView.TopNode.TreeView.FindNodeByPackage(pallet);
				var tvNodeInsideBox = treeView.TopNode.TreeView.FindNodeByPackage(box).Nodes.Cast<PackingTreeNode>().Single();
				var boxItemsToMove = new List<PackingTreeNode>() { tvNodeInsideBox };
				var drapDropAction = new PackingTreeViewDragDropActions(treeView);
				drapDropAction.HandleDragDropOfNodes(boxItemsToMove.AsReadOnly(), palletNode);
				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("The tv's inside the box should merge with the tv in the pallet.", @"
- Dummy 
  - 1x Box
  - 1x Pallet 8 KG
    - 4x P1 - TV - Size: 63in
".TrimStart());
				AssertEquals("TVsInBox Wrapper Divots were merged into TVsInPallet, so TVsInBox wrapper is now deleted.", true, tVsInBox.IsDeleted);

				// unpack some tv's from the pallet
				AssertNoExceptionThrown("Unpacking 2 tvs from the pallet should not cause exceptions (all events unhooked properly).", () => pallet.Unpack(tVsInPallet, 2m));
				treeView.Populate(Data.PackageJob);
				treeView.AssertPackingTreeView("The tv's inside the pallet should unpack 2 tv's.", @"
- Dummy 
  - 1x Box
  - 1x Pallet 4 KG
    - 2x P1 - TV - Size: 63in
".TrimStart());
			}

			#endregion
		}

		public void TestDisposeUnhooksNotificationsChanged()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeView())
			{
				treeView.Populate(Data.PackageJob);

				using (packageJob.SuspendValidationTesting())
				{
					var topNode = treeView.TopNode;
					packageJob.KJ_GS_NKReleasedByInfo.AddWarning("Some Warning");
					AssertEquals("Image should update when notifications change.", topNode.WarningImageIndex, topNode.ImageIndex);

					packageJob.KJ_GS_NKReleasedByInfo.ClearAllNotifications();
					AssertEquals("Image should update when notifications change.", topNode.NormalImageIndex, topNode.ImageIndex);

					topNode.Dispose();
					treeView.Nodes.Add(topNode); // hack to make tree accessible from the node after Dispose()

					packageJob.KJ_GS_NKReleasedByInfo.AddError("Some Error");
					AssertEquals("Image should not update when node is disposed.", topNode.NormalImageIndex, topNode.ImageIndex);
				}
			}
		}
	}
}
