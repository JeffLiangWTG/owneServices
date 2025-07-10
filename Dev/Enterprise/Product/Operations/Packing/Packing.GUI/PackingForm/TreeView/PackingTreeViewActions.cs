using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.Packing.GUI
{
	#region PackingActionFailedEventArgs

	public class PackingActionFailedEventArgs : EventArgs
	{
		public PackingActionFailedEventArgs(ZString message, bool isScanPacking)
		{
			Message = message;
			IsScanPacking = isScanPacking;
		}

		public readonly ZString Message;
		public readonly bool IsScanPacking;
	}

	#endregion

	#region PackEventArgs

	public class PackEventArgs : EventArgs
	{
		public PackEventArgs(IEnumerable<PkgPackage> newPackages, bool isPackingAlongsideSelectedPackage)
		{
			NewPackages = newPackages;
			IsPackingAlongsideSelectedPackage = isPackingAlongsideSelectedPackage;
		}

		public readonly IEnumerable<PkgPackage> NewPackages;
		public readonly bool IsPackingAlongsideSelectedPackage;
	}

	#endregion

	#region UnpackEventArgs

	public class UnpackEventArgs : EventArgs
	{
		public UnpackEventArgs(PackingTreeNode parentOfSelectedNodesPriorToUnpack)
		{
			ParentOfSelectedNodesPriorToUnpack = parentOfSelectedNodesPriorToUnpack;
		}

		public readonly PackingTreeNode ParentOfSelectedNodesPriorToUnpack;
	}

	#endregion

	#region ScanningNotificationEventArgs

	public class ScanningNotificationEventArgs : EventArgs
	{
		public ScanningNotificationEventArgs(ZString message, NotificationTypes notifyType, bool showOkButton = true)
		{
			Message = message;
			NotifyType = notifyType;
			ShowOkButton = showOkButton;
		}

		public readonly ZString Message;
		public readonly NotificationTypes NotifyType;
		public readonly bool ShowOkButton;
	}

	#endregion

	public class PackingTreeViewActions
	{
		public PackingTreeViewActions(PackingTreeView tree)
		{
			Tree = Argument.NotNull(tree, "tree");
		}

		readonly PackingTreeView Tree;

		PkgPackageJob PackageJob => Tree.PackageJob;

		#region Add New Package

		public void AddNewPackage()
		{
			AddPackageCore();
		}

		public void AddNewPackageAsInner(bool nestNewPackage = true)
		{
			var selectedNode = Tree.SelectedPackageNodeOrParentPackageNode;
			if (selectedNode?.Package?.ReadOnly ?? false)
			{
				OnPackOrUnpackFailed(Res.GetString("c3f1d4f9-e02c-47bc-be31-68c658e11f93", "Cannot Add an Inner Package to the selected Read-only Package."), isScanPacking: false);
			}
			else
			{
				AddPackageCore(selectedNode, nestNewPackage);
			}
		}

		void AddPackageCore(PackingTreeNode packageNode = null, bool nestNewPackage = true)
		{
			PkgPackage selectedPackage = null;
			if (packageNode != null)
			{
				if (!packageNode.IsPackage)
				{
					throw new ArgumentException("Cannot pass a AddPackageCore(package) was invoked with a node that is not a package.");
				}

				// fetch the parent package node if we don't want to nest packages
				if (!nestNewPackage && !packageNode.Package.IsOuter)
				{
					packageNode = packageNode.Parent;
				}

				selectedPackage = packageNode.Package;
			}

			ZString errorMessage;
			if (selectedPackage != null && !selectedPackage.IsAvailableForPacking(out errorMessage))
			{
				OnPackOrUnpackFailed(errorMessage, isScanPacking: false);
			}
			else
			{
				var parentPackages = (selectedPackage != null) ? selectedPackage.Packages : PackageJob.Packages;
				var parentNode = (selectedPackage != null) ? packageNode : Tree.TopNode;

				PkgPackage newPackage;
				using (AddingNodesSuspender.SuspendAddingNodes(parentPackages.Factory))
				{
					newPackage = parentPackages.AddNew();
				}

				var newNode = new PackingTreeNode(newPackage);

				// add and select the new Package node
				Tree.AddNode(newNode, parentNode.Nodes);
				if (!parentNode.IsExpanded)
				{
					parentNode.Expand();
				}
				Tree.SelectedNode = newNode;
				Tree.SelectedNode.EnsureVisible();
				parentNode.Paint(); // update the parent package node's text
			}
		}

		#endregion

		#region Pack / Unpack

		#region AutoPack

		public void AutoPack(INotifications notify)
		{
			using (AddingNodesSuspender.SuspendAddingNodes(PackageJob.Factory))
			{
				PackageJob.AutoPack(notify);
			}

			Tree.Populate(PackageJob);
		}

		#endregion

		#region Pack

		public void Pack(IEnumerable<IPackableItemParent> itemParentsToPack, IReadOnlyList<PkgPackage> existingPackages)
		{
			var packableItemParentsWithPackableItems = itemParentsToPack.Where(itemParentToPack => itemParentToPack.PackableItems.Any());
			if (packableItemParentsWithPackableItems.Any())
			{
				PackCore(packableItemParentsWithPackableItems, existingPackages);
			}
			else
			{
				var packItemsBizO = new PackItemsBusinessObject(PackageJob, itemParentsToPack, existingPackages);
				OnPackOrUnpackFailed(packItemsBizO.IsPackableErrorMessage, isScanPacking: false);
			}
		}

		void PackCore(IEnumerable<IPackableItemParent> itemParentsToPack, IReadOnlyList<PkgPackage> existingPackages)
		{
			var packItemsBizO = new PackItemsBusinessObject(PackageJob, itemParentsToPack, existingPackages);
			if (!packItemsBizO.IsPackable)
			{
				OnPackOrUnpackFailed(packItemsBizO.IsPackableErrorMessage, isScanPacking: false);
			}
			else if (ShowPackForm(packItemsBizO))
			{
				OnPackSucceeded(packItemsBizO);
			}
		}

		public bool PackViaScan(ZString barcode, bool isUserScanningQty)
		{
			var result = true;
			var allPackableItemParents = PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent);
			var packableItemParentsWithPackableItems = allPackableItemParents.Where(packableItemParent => packableItemParent.PackableItems.Any());

			if (packableItemParentsWithPackableItems.Any())
			{
				result = PackViaScanCore(packableItemParentsWithPackableItems, barcode, isUserScanningQty);
			}
			else
			{
				var packItemsBizO = new PackItemsViaScanBusinessObject(barcode, PackageJob, allPackableItemParents, null, isUserScanningQty);
				OnPackOrUnpackFailed(packItemsBizO.IsPackableErrorMessage, isScanPacking: true);
			}

			return result;
		}

		bool PackViaScanCore(IEnumerable<IPackableItemParent> packableItemParents, ZString barcode, bool isUserScanningQty)
		{
			var selectedPackages = PackageJob.Selected.SelectedPackages;
			var packItemsBizO = new PackItemsViaScanBusinessObject(barcode, PackageJob, packableItemParents, selectedPackages, isUserScanningQty);
			bool isValidBarcode = packItemsBizO.HasItemMatchingBarcode;
			if (isValidBarcode)
			{
				if (!packItemsBizO.IsPackable)
				{
					OnPackOrUnpackFailed(packItemsBizO.IsPackableErrorMessage, isScanPacking: true);
				}
				// if the user is not scanning the Qty, and there are *not* multiple packable item matches, auto-pack the units
				else if (packItemsBizO.CanAutoApplyChanges)
				{
					using (AddingNodesSuspender.SuspendAddingNodes(packItemsBizO.Factory))
					{
						if (packItemsBizO.RunValidationAndApplyChanges())
						{
							OnPackSucceeded(packItemsBizO);
						}
						else
						{
							ErrorReporter.ReportOnce("PackingTreeViewActions|PackViaScan", "When CanAutoApplyChanges is true, RunValidationAndApplyChanges should always succeed but it failed.");
						}
					}
				}
				// user must scan or manually choose the attribute to be packed
				else if (ShowPackForm(packItemsBizO, isUserScanningQty: isUserScanningQty))
				{
					OnPackSucceeded(packItemsBizO);
				}
			}

			return isValidBarcode;
		}

		bool ShowPackForm(PackItemsBusinessObject packItemsBizO, bool isUserScanningQty = false)
		{
			using (AddingNodesSuspender.SuspendAddingNodes(packItemsBizO.Factory))
			using (var form = new PackOrUnpackItemsDialog(packItemsBizO, packItemsBizO.IsScanPacking, isUserScanningQty))
			{
				return form.ShowDialog() == DialogResult.OK;
			}
		}

		#endregion

		#region Unpack

		public bool Unpack()
		{
			var validItemsForUnpack = PackageJob.Selected.ItemsToUnpackBasedOnSelected();
			bool unpackSuccess = validItemsForUnpack.Packages.Any() || validItemsForUnpack.PackedItemsAndBarcodes.Any();

			if (unpackSuccess)
			{
				UnpackCore(validItemsForUnpack.Packages, validItemsForUnpack.PackedItemsAndBarcodes, isScanPacking: false, isUserScanningQty: false);
			}
			else
			{
				OnPackOrUnpackFailed(validItemsForUnpack.ErrorMessage, isScanPacking: false);
			}

			return unpackSuccess;
		}

		public bool UnpackViaScan(ZString barcode, bool isUserScanningQty)
		{
			ZString errorMessage;
			var itemsToUnpack = PackageJob.Selected.ItemsToUnpackBasedOnSelected(barcode, out errorMessage, isUserScanningQty).ToArray(); // force evaluation to prevent LINQ re-evaluation
			bool isValidProductBarcode = itemsToUnpack.Any();

			if (isValidProductBarcode)
			{
				// cannot scan-unpack multiple packages so don't pass in any
				UnpackCore(Array.Empty<PkgPackage>(), itemsToUnpack, isScanPacking: true, isUserScanningQty: isUserScanningQty);
			}
			else
			{
				OnPackOrUnpackFailed(errorMessage, isScanPacking: true);
			}

			return isValidProductBarcode || !errorMessage.IsEmpty; // returning true instructs the caller to not continue identifying the barcode.
		}

		void UnpackCore(IEnumerable<PkgPackage> packagesToUnpack, IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> itemsToUnpack, bool isScanPacking, bool isUserScanningQty = false)
		{
			// store the parent node so that we can later select it if all selected packages are removed
			var selectedPackageOrParentOfSelectedPackedItem = SelectedPackageOrParentOfSelectedPackedItem;
			var selectedPackageOrParentOfSelectedPackedItem_Parent = selectedPackageOrParentOfSelectedPackedItem?.Parent;

			// setup unpack
			var unpackItemsBizO = isScanPacking ? new UnpackItemsViaScanBusinessObject(PackageJob, itemsToUnpack, isUserScanningQty)
												: new UnpackItemsBusinessObject(PackageJob, itemsToUnpack, packagesToUnpack);

			// auto-unpack if scanning-all 1 item, or if there are only packages to remove
			if (unpackItemsBizO.CanAutoApplyChanges)
			{
				unpackItemsBizO.RunValidationAndApplyChanges();
			}
			else // user must scan or manually choose the attribute and/or qty to be unpacked
			{
				ShowUnpackForm(unpackItemsBizO, isScanPacking, isUserScanningQty);
			}

			// if the Package was deleted select its parent instead.
			if (selectedPackageOrParentOfSelectedPackedItem != null && selectedPackageOrParentOfSelectedPackedItem.IsPackage && selectedPackageOrParentOfSelectedPackedItem.Package.IsDeleted)
			{
				selectedPackageOrParentOfSelectedPackedItem = selectedPackageOrParentOfSelectedPackedItem_Parent;
			}

			OnUnpackSucceeded(selectedPackageOrParentOfSelectedPackedItem);
		}

		void ShowUnpackForm(UnpackItemsBusinessObject unpackItemsBizO, bool isScanPacking = false, bool isUserScanningQty = false)
		{
			using (var form = new PackOrUnpackItemsDialog(unpackItemsBizO, isScanPacking, isUserScanningQty))
			{
				form.ShowDialog();
			}
		}

		PackingTreeNode SelectedPackageOrParentOfSelectedPackedItem
		{
			get
			{
				PackingTreeNode result = null;
				if (Tree.NodeSelector.IsPackableItemSelected)
				{
					result = Tree.NodeSelector.SelectedNodes[0].Parent;
				}
				else if (Tree.NodeSelector.IsAnyNodeSelected)
				{
					result = Tree.NodeSelector.SelectedNodes[0];
				}
				return result;
			}
		}

		#endregion

		#region Events

		public event EventHandler<PackEventArgs> PackSucceeded;
		public event EventHandler<UnpackEventArgs> UnpackSucceeded;
		public event EventHandler<PackingActionFailedEventArgs> PackOrUnpackFailed;
		public event EventHandler<PackingActionFailedEventArgs> BreakDownFailed;

		void OnPackSucceeded(PackItemsBusinessObject bizO)
		{
			if (PackSucceeded != null)
			{
				PackSucceeded(this, new PackEventArgs(bizO.NewPackages, bizO.IsPackingAlongsideExistingPackage));
			}
		}

		void OnUnpackSucceeded(PackingTreeNode parentNodeOfSelection)
		{
			UnpackSucceeded?.Invoke(this, new UnpackEventArgs(parentNodeOfSelection));
		}

		void OnPackOrUnpackFailed(string errorMessage, bool isScanPacking)
		{
			if (PackOrUnpackFailed != null && !string.IsNullOrWhiteSpace(errorMessage))
			{
				PackOrUnpackFailed(this, new PackingActionFailedEventArgs(errorMessage, isScanPacking));
			}
		}

		void OnBreakDownPackagesFailed(string errorMessage)
		{
			if (BreakDownFailed != null && !string.IsNullOrWhiteSpace(errorMessage))
			{
				BreakDownFailed(this, new PackingActionFailedEventArgs(errorMessage, isScanPacking: false));
			}
		}

		#region ScanningNotification

		public event EventHandler<ScanningNotificationEventArgs> ScanningNotification;

		void OnScanningNotification(ZString message, NotificationTypes notifyType, bool showOkButton = true)
		{
			if (ScanningNotification != null)
			{
				ScanningNotification(this, new ScanningNotificationEventArgs(message, notifyType, showOkButton));
			}
		}

		#endregion

		#endregion

		#endregion

		#region NonSystemBarcodeScanned

		public void NonSystemBarcodeScanned(BarcodeScanEventArgs e, bool isPackageIdBoxFocused, bool isPacking, bool isUserScanningQty)
		{
			bool isValidProductBarcode = isPacking ? PackViaScan(e.Barcode, isUserScanningQty)
													 : UnpackViaScan(e.Barcode, isUserScanningQty);

			isValidProductBarcode |= SelectPackageOrAddNewIfSSCCViaScan(e.Barcode);

			if (!isValidProductBarcode)
			{
				if (isPackageIdBoxFocused)
				{
					e.Handled = false; // send the barcode to the Package ID textbox (user may be scanning a pre-printed label ID).
				}
				else
				{
					OnScanningNotification(
							Res.GetString("9314de7c-7610-4e7e-a144-3d28f6b207d5", "Barcode {0} is not part of this {1}.",
							e.Barcode, PackageJob.ParentJob.JobDescription), NotificationTypes.Error);
				}
			}
		}

		#endregion

		#region Select Package via Scan

		public bool SelectPackageOrAddNewIfSSCCViaScan(ZString barcode)
		{
			PkgPackage package;
			using (AddingNodesSuspender.SuspendAddingNodes(PackageJob.Factory))
			{
				package = PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC(barcode);
			}

			if (package != null)
			{
				Tree.SelectedNode = Tree.FindNodeByPackage(package);
				if (Tree.SelectedNode == null) // new package has no node yet
				{
					Tree.SelectedNode = Tree.AddNode(new PackingTreeNode(package), Tree.TopNode.Nodes);
				}
			}

			return (package != null);
		}

		#endregion

		#region Break Down Packages

		public void BreakDownPackages()
		{
			var selectedPackages = Tree.SelectedPackages;

			var errorMessage = ZString.Empty;
			if (selectedPackages != null && selectedPackages.Any(p => !p.IsBreakDownAllowed(out errorMessage)))
			{
				OnBreakDownPackagesFailed(errorMessage);
			}
			else
			{
				using (var dialog = new BreakDownPackageDialog(selectedPackages))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						var splitQty = dialog.DataSource.SplitQuantity;
						BreakDownPackagesCore(splitQty);
					}
				}
			}
		}

		void BreakDownPackagesCore(int splitQty)
		{
			var selectedPackages = Tree.SelectedPackages;
			var selectedPackagesNodes = Tree.SelectedPackagesNodes;

			try
			{
				Tree.BeginUpdate();

				using (AddingNodesSuspender.SuspendAddingNodes(PackageJob.Factory))
				{
					foreach (var package in selectedPackages)
					{
						// split the package
						var newPackages = package.BreakDownIntoIndividualPackages(splitQty);

						// add new packages nodes to the tree
						var packageParentNode = selectedPackagesNodes.First(node => node.Package == package).Parent;
						foreach (var newPackage in newPackages.Where(p => p != package))
						{
							var packageNode = new PackingTreeNode(newPackage);
							Tree.AddNode(packageNode, packageParentNode.Nodes);
						}
					}
				}
			}
			finally
			{
				Tree.EndUpdate();
			}
		}

		#endregion

		#region Customize View

		public void CustomizeView()
		{
			LoadTreeSummaryLayout();

			// show the column designer
			using (var customiseForm = new ZColumnsCustomise(Tree.NodePainter.Summary.Tokens, null, false, new ZGridCustomiseBizObj(LayoutKey, LoadLayout())))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(customiseForm) == DialogResult.OK)
				{
					// update the summary tokens, repaint, then save changes
					Tree.NodePainter.UpdateSummaryAndRepaint(customiseForm.Result.Cast<PackingTreeNodeSummaryToken>().ToArray());
					SaveTreeSummaryLayout();
				}
			}
		}

		public void LoadTreeSummaryLayout(bool updateTree = false)
		{
			var layout = LoadLayout();
			if (layout != null)
			{
				var savedColumns = ZColumnsCustomise.GetVisibleColumns(PackageJob.Factory, layout, Tree.NodePainter.Summary.Tokens).Cast<PackingTreeNodeSummaryToken>().ToArray();
				if (savedColumns.Length > 0)
				{
					Tree.NodePainter.UpdateSummaryAndRepaint(savedColumns, updateTree);
				}
			}
		}

		void SaveTreeSummaryLayout()
		{
			var dataGridLayoutManager = new DataGridLayoutManager();
			var columnsLayoutModification = new ZColumnsLayoutModification(Tree.NodePainter.Summary.Tokens, new BusinessObjectFactory(), LoadLayout(), LayoutKey);
			dataGridLayoutManager.SavePreconfiguredLayout(columnsLayoutModification, LayoutName, false, false, SaveColumnLayout.Yes);
		}

		StmModuleFilter LoadLayout()
		{
			return new StmModuleFilter.Loader(new BusinessObjectFactory()).FindTop1ByIDAndName(LayoutKey, LayoutName, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Identifier.")]
		string LayoutName => "Packing Layout";

		string LayoutKey => LayoutName + "|" + Tree.FindForm().Name;

		#endregion

	}
}
