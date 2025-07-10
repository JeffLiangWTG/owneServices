using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	class PackingTreeViewTest : PackingTestCaseWithFactory
	{
		#region TestIsDraggingFromGrid

		public void TestIsDraggingFromGrid_PackingUserControl()
		{
			var treeViewForTest = new PackingTreeViewForTest();
			AssertEquals(false, treeViewForTest.IsDraggingFromGrid);

			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var form = new ZForm())
			{
				var packingUserControl = new PackingUserControl();
				packingUserControl.Controls.Add(treeViewForTest);
				form.Controls.Add(packingUserControl);
				form.Show();

				var gridInPackingUserControl = (ZGrid)packingUserControl.Controls.Find("Grid", true).Single();
				AssertEquals("Pre-condition: Grid.IsDragging", false, gridInPackingUserControl.IsDragging);
				AssertEquals("IsDraggingFromGrid", false, treeViewForTest.IsDraggingFromGrid);

				gridInPackingUserControl.IsDragging = true;
				AssertEquals(true, treeViewForTest.IsDraggingFromGrid);
			}
		}

		public void TestIsDraggingFromGrid_PackingTreeViewUserControl()
		{
			var treeViewForTest = new PackingTreeViewForTest();
			AssertEquals(false, treeViewForTest.IsDraggingFromGrid);

			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (var form = new ZForm())
			{
				var packingTreeViewUserControl = new PackingTreeViewUserControl();
				packingTreeViewUserControl.Controls.Add(treeViewForTest);
				form.Controls.Add(packingTreeViewUserControl);
				form.Show();

				AssertEquals("Pre-condition: Grid.IsDragging", false, packingTreeViewUserControl.PackageIDsGrid.IsDragging);
				AssertEquals("IsDraggingFromGrid", false, treeViewForTest.IsDraggingFromGrid);

				packingTreeViewUserControl.PackageIDsGrid.IsDragging = true;
				AssertEquals(true, treeViewForTest.IsDraggingFromGrid);
			}
		}

		#endregion

		#region TestOnDragDrop

		[RequiresSTA]
		public void TestOnDragDrop_PackageHeader()
		{
			Data.CreatePackingData();
			var pallet1 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet2 = Data.PackageJob.Packages.AddNew("PLT");
			pallet1.KP_PackageQty = 1;
			pallet2.KP_PackageQty = 1;
			AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet1.KP_PackageID);
			AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet2.KP_PackageID);

			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeViewForTest())
			using (var packingTreeViewUserControl = new PackingTreeViewUserControl())
			using (var packingUserControl = new PackingUserControl())
			using (var form = new ZForm())
			{
				packingTreeViewUserControl.Controls.Add(treeView);
				packingUserControl.Controls.Add(packingTreeViewUserControl);
				form.Controls.Add(packingUserControl);
				form.Show();

				packingUserControl.SetDataBinding(packageJob, "");
				treeView.Populate(packageJob);
				AssertNotNull(treeView.TopNode);
				AssertEquals("Pre-condition: Number of packages on this job should be 2.", 2, packageJob.Packages.Count);

				var destinationNode = treeView.FindNodeByPackage(pallet1);
				treeView.NodeSelector.SelectNode(destinationNode);

				var packageHeader1 = Helper.CreatePackageHeader(Data.PackageJob, "123");
				var packageHeaderPivot1 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader1);
				var headerPivots1 = new ArrayList();
				headerPivots1.Add(packageHeaderPivot1);
				var dataObject1 = new DataObject(headerPivots1);
				var args1 = new DragEventArgs(dataObject1, 0, destinationNode.Bounds.X, destinationNode.Bounds.Y, DragDropEffects.Move, DragDropEffects.None);

				treeView.DragDropTreeNodeForTest = destinationNode;
				packingTreeViewUserControl.PackageIDsGrid.IsDragging = true;

				treeView.OnDragDrop(args1);

				AssertEquals("After drag and drop, package id has been assigned.", "123", pallet1.KP_PackageID);

				destinationNode = treeView.FindNodeByPackage(pallet2);
				treeView.NodeSelector.SelectNode(destinationNode);

				var packageHeader2 = Helper.CreatePackageHeader(Data.PackageJob, "456");
				var packageHeaderPivot2 = Helper.CreatePackageHeaderPivot(packageJob, packageHeader2);
				var headerPivots2 = new List<PkgPackageJobPackageHeaderPivot>();
				headerPivots2.Add(packageHeaderPivot2);
				var dataObject2 = new DataObject(headerPivots2);
				var args2 = new DragEventArgs(dataObject2, 0, destinationNode.Bounds.X, destinationNode.Bounds.Y, DragDropEffects.Move, DragDropEffects.None);

				treeView.DragDropTreeNodeForTest = destinationNode;

				treeView.OnDragDrop(args2);

				AssertEquals("After drag and drop, package id has been assigned.", "456", pallet2.KP_PackageID);
			}
		}

		public void TestOnDragDrop_PackageHeader_WhenDataObjectNotProper()
		{
			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			pallet.KP_PackageQty = 1;
			AssertEquals("Pre-condition: Package ID should be empty.", ZString.Empty, pallet.KP_PackageID);

			var packageJob = Data.PackageJob;

			using (var treeView = new PackingTreeViewForTest())
			using (var packingTreeViewUserControl = new PackingTreeViewUserControl())
			using (var packingUserControl = new PackingUserControl())
			using (var form = new ZForm())
			{
				packingTreeViewUserControl.Controls.Add(treeView);
				packingUserControl.Controls.Add(packingTreeViewUserControl);
				form.Controls.Add(packingUserControl);
				form.Show();

				packingUserControl.SetDataBinding(packageJob, "");
				treeView.Populate(packageJob);
				AssertNotNull(treeView.TopNode);
				AssertEquals("Pre-condition: Number of packages on this job should be 1.", 1, packageJob.Packages.Count);

				var destinationNode = treeView.FindNodeByPackage(pallet);
				treeView.NodeSelector.SelectNode(destinationNode);

				var packageHeader = Helper.CreatePackageHeader(Data.PackageJob, "123");
				var packageHeaderPivot = Helper.CreatePackageHeaderPivot(packageJob, packageHeader);
				var headerPivots1 = new HashSet<PkgPackageJobPackageHeaderPivot>();
				headerPivots1.Add(packageHeaderPivot);
				var dataObject = new DataObject(headerPivots1);
				var args = new DragEventArgs(dataObject, 0, destinationNode.Bounds.X, destinationNode.Bounds.Y, DragDropEffects.Move, DragDropEffects.None);

				treeView.DragDropTreeNodeForTest = destinationNode;
				packingTreeViewUserControl.PackageIDsGrid.IsDragging = true;

				AssertExceptionThrown("On drag and drop, dataObject should be of not supported type.", typeof(InvalidCastException), () => treeView.OnDragDrop(args));

				dataObject = null;
				args = new DragEventArgs(dataObject, 0, destinationNode.Bounds.X, destinationNode.Bounds.Y, DragDropEffects.Move, DragDropEffects.None);

				AssertExceptionThrown("On drag and drop, dataObject should be of null type.", typeof(NullReferenceException), () => treeView.OnDragDrop(args));
			}
		}

		#endregion
	}

	class PackingTreeViewForTest : PackingTreeView
	{
		public new void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);
		}

		internal PackingTreeNode DragDropTreeNodeForTest { get; set; }

		protected override PackingTreeNode GetDraggedOverNodeCore(DragEventArgs e)
		{
			return DragDropTreeNodeForTest ?? base.GetDraggedOverNodeCore(e);
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (DragDropTreeNodeForTest != null)
			{
				DragDropTreeNodeForTest.Dispose();
			}
		}
	}
}
