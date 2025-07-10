using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Packing.GUI
{
	class PackingTreeViewDragDropActions
	{
		public PackingTreeViewDragDropActions(PackingTreeView tree)
		{
			Argument.NotNull(tree, "tree");
			Tree = tree;
		}

		readonly PackingTreeView Tree;

		#region HandleDragDropOfNodes

		// perhaps we can move the logic for both error messages into business?
		public void HandleDragDropOfNodes(IReadOnlyCollection<PackingTreeNode> srcNodes, PackingTreeNode dstNode)
		{
			ZString errorMessage;

			var packageNodes = new List<PackingTreeNode>(srcNodes.Count);
			var itemNodes = new List<PackingTreeNode>(srcNodes.Count);
			var parentPackages = new HashSet<PkgPackage>();

			foreach (var node in srcNodes.Where(n => n != dstNode))
			{
				if (node.IsPackage)
				{
					packageNodes.Add(node);
				}
				else
				{
					itemNodes.Add(node);
				}

				if (node.Parent.IsPackage)
				{
					parentPackages.Add(node.Parent.Package);
				}
			}

			// make sure we dont drag a parent onto a child.
			if (packageNodes.Concat(itemNodes).Any(dstNode.IsChildOf))
			{
				OnDragDropFailed(Res.GetString("9bccdb00-6df1-42ec-8b26-e8b1ed5ac333", "Cannot Pack the selected items into their child Package."));
			}
			// make sure we dont drag packed items onto the top level node (packages-only are allowed here)
			else if (dstNode.IsPackageJob && itemNodes.Count > 0)
			{
				OnDragDropFailed(Res.GetString("4a8986f1-dfe7-48b1-98b2-06348b5b59de", "Only Packages can be dropped at the Job-level."));
			}
			else if (dstNode.IsPackage && !dstNode.Package.IsAvailableForPacking(out errorMessage))
			{
				OnDragDropFailed(errorMessage);
			}
			else if (IsDraggingPackagesValidForInner(packageNodes, dstNode, out errorMessage))
			{
				OnDragDropFailed(errorMessage);
			}
			else
			{
				// get the parent package of each selected node (we want the selected node's parent to show a better error message).
				foreach (var package in parentPackages)
				{
					if (!package.IsAvailableForUnpacking(out errorMessage))
					{
						OnDragDropFailed(errorMessage);
						break;
					}
				}

				if (errorMessage.IsEmpty)
				{
					HandleDragDropOfNodesCore(dstNode, packageNodes.ToArray(), itemNodes.ToArray(), parentPackages);
				}
			}
		}

		bool IsDraggingPackagesValidForInner(IReadOnlyCollection<PackingTreeNode> packageNodes, PackingTreeNode dstNode, out ZString errorMessage)
		{
			var result = false;
			errorMessage = "";

			if (dstNode.IsPackage)
			{
				var localErrorMessage = ZString.Empty;
				var useMessageAppropriateForMultiplePackages = packageNodes.Count > 1;
				result = packageNodes.Any(n => !n.Package.IsValidCandidateForInner(out localErrorMessage, useMessageAppropriateForMultiplePackages));
				errorMessage = localErrorMessage;
			}

			return result;
		}

		void HandleDragDropOfNodesCore(PackingTreeNode dstNode, PackingTreeNode[] packageNodes, PackingTreeNode[] itemNodes, IReadOnlyCollection<PkgPackage> parentPackages)
		{
			Tree.BeginUpdate();
			try
			{
				using (packageNodes.Length > 1 ? Tree.PackageJob.InitiateMassPackageProcess() : null)
				// the only sort property changed during drag drop is potentially sequence which is set before any sorting occurs
				using (((PackingTreeNodeSorter)Tree.TreeViewNodeSorter).CacheSortingProperties())
				using (Tree.SuspendNotificationsChanged())
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Tree.PackageJob.Factory))
				{
					if (packageNodes.Length > 0)
					{
						MovePackages(dstNode, packageNodes, parentPackages);
					}

					if (itemNodes.Length > 0)
					{
						MovePackedItems(dstNode, itemNodes);
					}
				}

				dstNode.Expand();
			}
			finally
			{
				Tree.EndUpdate();
			}
		}

		static void MovePackages(PackingTreeNode dstNode, PackingTreeNode[] packageNodes, IReadOnlyCollection<PkgPackage> parentPackages)
		{
			var packagesToSuspendWeightPainting = dstNode.IsPackage
				? parentPackages.Append(dstNode.Package)
				: parentPackages;

			var countOfElements = dstNode.IsPackage ? parentPackages.Count + 1 : parentPackages.Count;
			var disposableList = new DisposableList(countOfElements * 2);

			foreach (var package in packagesToSuspendWeightPainting)
			{
				// Suspend Weight Value Changed as it will cause the Parent to be repainted multiple times
				disposableList.Add(package.SuspendOnValueChanged(nameof(PkgPackage.KP_Weight)));
				// RefreshBinding() on Weight to repaint the parent once at the end
				disposableList.Add(new DisposableAction(package.KP_WeightInfo.RefreshBinding));
			}

			using (disposableList)
			{
				using (AddingNodesSuspender.SuspendAddingNodes(dstNode.BizO.Factory))
				{
					dstNode.Packages.AddRange(packageNodes.Select(n => n.Package)); // biz
				}

				dstNode.MoveNodesTo(packageNodes); // gui
			}
		}

		static void MovePackedItems(PackingTreeNode dstNode, PackingTreeNode[] itemNodes)
		{
			var itemNodesToMove = new List<PackingTreeNode>(itemNodes.Length);
			for (var i = itemNodes.Length - 1; i >= 0; i--)
			{
				var itemNode = itemNodes[i];
				if (MovePackedItemNode(itemNode, dstNode))
				{
					itemNodesToMove.Add(itemNode);
				}
			}

			dstNode.MoveNodesTo(itemNodesToMove.ToArray()); // gui
		}

		static bool MovePackedItemNode(PackingTreeNode srcNode, PackingTreeNode dstNode)
		{
			var result = false;
			var packedItem = srcNode.PackedItem;
			var dstPackage = dstNode.Package;

			// make sure the packed item is being dropped onto a different package
			if (packedItem.ParentPackage != dstPackage)
			{
				// move the packed item
				packedItem.MovePackedItem(dstPackage); // biz

				var isSrcNodeDeleted = srcNode.TreeView == null; // ie merged with a dst node
				if (!isSrcNodeDeleted)
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region HandleDragDropOfGridItems

		public void HandleDragDropOfGridItems(IEnumerable<PackableItemParentWrapper> packableItemParentsDropped, PackingTreeNode destinationNode)
		{
			// Select the dropped node. The selection will be used for packability checks and GUI updating (for performance).
			Tree.NodeSelector.DeselectAllNodes();
			Tree.NodeSelector.SelectNode(destinationNode);

			var existingPackages = !destinationNode.IsPackageJob ? new[] { destinationNode.Package } : Array.Empty<PkgPackage>();
			Tree.ActionsHelper.Pack(packableItemParentsDropped.Select(w => w.PackableItemParent), existingPackages);
		}

		#endregion

		#region HandleDragDropOfPkgPackageHeader

		public void HandleDragDropOfPkgPackageHeader(IReadOnlyCollection<PkgPackageJobPackageHeaderPivot> packageHeadersDropped, PackingTreeNode destinationNode)
		{
			Argument.NotNull(destinationNode, nameof(destinationNode));
			Argument.NotNull(packageHeadersDropped, nameof(packageHeadersDropped));

			var packageJob = destinationNode.Package.PackageJob;
			if (packageJob != null)
			{
				var package = destinationNode.Package;
				var unusedPackageHeaders = packageHeadersDropped.Where(h => !packageJob.IsPackageIDAlreadyAssigned(h.PackageHeader)).Select(p => p.PackageHeader).ToArray();

				if (unusedPackageHeaders.Any() && package.KP_PackageQty > 0)
				{
					packageJob.AssignPackageIDs(package, unusedPackageHeaders);
				}
			}

			Tree.NodeSelector.DeselectAllNodes();
			Tree.NodeSelector.SelectNode(destinationNode);
		}

		#endregion

		#region DragDropFailed

		public event EventHandler<DragDropActionsEventArgs> DragDropFailed;

		void OnDragDropFailed(ZString message)
		{
			if (DragDropFailed != null)
			{
				DragDropFailed(this, new DragDropActionsEventArgs(message));
			}
		}

		public class DragDropActionsEventArgs : EventArgs
		{
			public DragDropActionsEventArgs(ZString message)
			{
				Message = message;
			}

			public readonly ZString Message;
		}

		#endregion

		#region IsNodeValidDropTarget

		public bool IsNodeValidDropTarget(PackingTreeNode node)
		{
			return (node != null
				&& (Tree.IsDraggingFromGrid || !Tree.NodeSelector.IsSelected(node))
				&& (node.IsPackage || node.IsPackageJob));
		}

		#endregion

		#region IsValidSource

		/// <summary>
		/// Ensures the source is a valid Package object, and that it does not belong to another package job / form (!).
		/// </summary>
		public bool IsValidSource(ArrayList elements)
		{
			bool result = false;

			if (elements.Count > 0)
			{
				if (elements[0] is PackableItemParentWrapper)
				{
					result = ((PackableItemParentWrapper)elements[0]).ParentJob == Tree.PackageJob.ParentJob;
				}
				else if (elements[0] is PkgPackageItemDivotsWrapper)
				{
					result = ((PkgPackageItemDivotsWrapper)elements[0]).ParentPackage.PackageJob == Tree.PackageJob;
				}
				else if (elements[0] is PkgPackage)
				{
					result = ((PkgPackage)elements[0]).PackageJob == Tree.PackageJob;
				}
				else if (elements[0] is PkgPackageJobPackageHeaderPivot)
				{
					var packageJob = Tree.PackageJob;
					result = elements.Cast<PkgPackageJobPackageHeaderPivot>().Any(h => !packageJob.IsPackageIDAlreadyAssigned(h.PackageHeader));
				}
			}

			return result;
		}

		#endregion
	}
}
