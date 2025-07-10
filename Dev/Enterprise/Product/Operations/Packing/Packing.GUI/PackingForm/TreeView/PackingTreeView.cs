using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.GUI
{
	public class PackingTreeView : ZTreeView
	{
		public event EventHandler DeleteNodes;

		#region Construction

		public PackingTreeView()
		{
			AllowDrop = true;
			HideSelection = false;
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			TreeViewNodeSorter = new PackingTreeNodeSorter();
			LineColor = SystemColors.GrayText; // without this, the ownerdrawn dotted line colour is incorrect
			CurrentArrowMovement = Keys.None;

			InitialiseImages();
			HookEvents();

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		void InitialiseImages()
		{
			ImageList = new ImageList();
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Blank));
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Error));
			ImageList.Images.Add(Icons.GetIcon(IconTypes.MessageError));
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Warning));
		}

		void HookEvents()
		{
			AfterSelectOrDeselect += PackingTreeView_AfterSelectOrDeselect;
		}

		void UnhookEvents()
		{
			AfterSelectOrDeselect -= PackingTreeView_AfterSelectOrDeselect;
			UnhookActionsHelper();
			UnhookDragDropHelper();
		}

		void UnhookActionsHelper()
		{
			if (actionsHelper != null)
			{
				actionsHelper.BreakDownFailed -= ActionsHelper_PackOrUnpackOrBreakDownFailed;
				actionsHelper.PackSucceeded -= ActionsHelper_PackSucceeded;
				actionsHelper.PackOrUnpackFailed -= ActionsHelper_PackOrUnpackOrBreakDownFailed;
				actionsHelper.UnpackSucceeded -= ActionsHelper_UnpackSucceeded;
				actionsHelper = null;
			}
		}

		void UnhookDragDropHelper()
		{
			if (dragDropHelper != null)
			{
				dragDropHelper.DragDropFailed -= new EventHandler<PackingTreeViewDragDropActions.DragDropActionsEventArgs>(DragDropHelper_DragDropFailed);
			}
		}

		bool ParentReadOnly
		{
			get
			{
				var parentPackingControl = GetParentPackageControl();
				return parentPackingControl == null || parentPackingControl.ReadOnly;
			}
		}

		PackingUserControl GetParentPackageControl()
		{
			return this.GetParent<PackingUserControl>();
		}

#if WINZOR
		public override void Refresh()
		{
			base.Refresh();
			NotifyRenderRequired();
		}
#endif

		#endregion

		#region Populate

		public void Populate(PkgPackageJob packageJob)
		{
			try
			{
				BeginUpdate();

				// clear nodes first (do *not* use Tree.Nodes.Clear() because it will not call Dispose on each node)
				RemoveAllNodes();

				if (packageJob != null)
				{
					var topNode = new PackingTreeNode(packageJob);
					using (((PackingTreeNodeSorter)TreeViewNodeSorter).CacheSortingProperties())
					{
						AddNode(topNode, Nodes);

						// build the child nodes
						var treeNodesToAddSorted = new TreeNode[packageJob.Packages.Count];
						var index = 0;

						foreach (var childPackage in packageJob.Packages)
						{
							var node = new PackingTreeNode(childPackage);
							PopulatePackageChildNodes(node);
							treeNodesToAddSorted[index++] = node;
						}

						// Using AddRange() will add the children sorted in an efficient manner.
						// Much faster than calling the TreeView's Sort() method.
						Sorted = true;
						topNode.Nodes.AddRange(treeNodesToAddSorted);

						foreach (var node in topNode.Nodes)
						{
							NodePainter.PaintText((PackingTreeNode)node); // done after adding otherwise text is sometimes chopped off
						}

						NodePainter.RepaintTopNode(); // .NET bug? -- top node text is cut off after sort
					}

					// expand the first two-levels
					topNode.Expand();
					foreach (TreeNode node in topNode.Nodes)
					{
						node.Expand();
					}
				}

				UpdateNotifications(); // pulls notifications from the node's BizOs
			}
			finally
			{
				EndUpdate();
			}
		}

#if WINZOR
		protected override bool IsNodeNeedHighlight(TreeNode node) => NodeSelector.IsSelected((PackingTreeNode)node);
#endif

		public PackingTreeNode PopulateFromPackage(PkgPackage package, TreeNode parentNode)
		{
			try
			{
				BeginUpdate();

				var node = new PackingTreeNode(package);
				PopulatePackageChildNodes(node);
				AddNode(node, parentNode.Nodes);

				return node;
			}
			finally
			{
				EndUpdate();
			}
		}

		public void PopulatePackageChildNodes(PackingTreeNode packageNode)
		{
			try
			{
				BeginUpdate();

				// in case there are existing nodes
				Array.ForEach(packageNode.Nodes.Cast<PackingTreeNode>().ToArray(), node => node.Remove());

				var package = packageNode.Package;

				// add child packages
				foreach (var childPackage in package.Packages)
				{
					PopulateFromPackage(childPackage, packageNode);
				}

				// add items
				foreach (var packedItem in package.PackedItems.Typed)
				{
					var packedItemNode = new PackingTreeNode(packedItem);
					AddNode(packedItemNode, packageNode.Nodes);
				}
			}
			finally
			{
				EndUpdate();
			}
		}

		public PackingTreeNode AddNode(PackingTreeNode node, TreeNodeCollection nodesToAddTo)
		{
			nodesToAddTo.Add(node);
			NodePainter.PaintText(node); // done after adding otherwise text is sometimes chopped off

			return node;
		}

		#endregion

		#region Painting Nodes

		public PackingTreeNodePainter NodePainter
		{
			get
			{
				if (nodePainter == null)
				{
					nodePainter = new PackingTreeNodePainter(this);
					nodePainter.Initialise();
				}
				return nodePainter;
			}
		}

		PackingTreeNodePainter nodePainter;

		#region Drawing the Dotted-Line on Nodes with no image, Drawing the blue Summary Text

#if !WINZOR

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000; // this stops node flicker when the tree is resized.
				return cp;
			}
		}

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			base.OnDrawNode(e);

			e.DrawDefault = true; // tell .NET to draw everything else
			NodePainter.PaintDottedLine(e);
			NodePainter.PaintSummary(e);
		}

#endif

		protected override bool UseDrawNodeFontOverride => false; // base does a bunch of painting we do not want.

		protected override void OnAfterCollapse(TreeViewEventArgs e)
		{
			base.OnAfterCollapse(e);

			if (!CheckBoxes && ImageList != null && e.Node.ImageIndex == 0)
			{
				Invalidate(e.Node.Bounds); // to redraw the dotted lines
			}
		}

		#endregion

		#endregion

		#region Multi-Node Selection

		#region SelectedNode(s)

		public bool IsAnyNodeSelected => NodeSelector.IsAnyNodeSelected;

		public event EventHandler AfterSelectOrDeselect
		{
			add { NodeSelector.AfterSelectOrDeselect += value; }
			remove { NodeSelector.AfterSelectOrDeselect -= value; }
		}

		public new PackingTreeNode TopNode => (PackingTreeNode)Nodes[0];

		public PkgPackageJob PackageJob => (PkgPackageJob)TopNode.BizO;

		/// <summary>
		/// If more than one node is selected this property will return null.
		/// </summary>
		public new PackingTreeNode SelectedNode
		{
			get { return NodeSelector.IsSingleNodeSelected ? NodeSelector.LastSelectedNode : null; }
			set
			{
				NodeSelector.DeselectAllNodes();
				if (value != null)
				{
					NodeSelector.SelectNode(value);

#if WINZOR
					value.Refresh();
#endif
				}
			}
		}

		public ReadOnlyCollection<PackingTreeNode> SelectedNodes => NodeSelector.SelectedNodes;

		PackingTreeNode SelectedNode_DotNet
		{
			get { return (PackingTreeNode)base.SelectedNode; }
			set { base.SelectedNode = value; }
		}

		public new PackingTreeNode GetNodeAt(Point point)
		{
			return (PackingTreeNode)base.GetNodeAt(point);
		}

		#endregion

		#region Selecting Nodes

		public bool AllowMultiSelectFromDifferentParents
		{
			get;
			set;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			var node = GetNodeAt(e.Location);
			if (node != null) // user can click the blank area of the treeview
			{
				var rightMouseButtonWasNotClicked = (e.Button != MouseButtons.Right);
				IsUserNavigatingWithMouse = true;
				IsDotNetSelectedNodeCurrent = false;
				SelectedNode_DotNet = null;

				if (ModifierKeys == Keys.Control)
				{
					if (NodeSelector.IsSelected(node) && rightMouseButtonWasNotClicked) // do not deselect if user is CTRL-selecting and right-clicks
					{
						if (NodeSelector.Count > 1) // cannot deselect last node
						{
							// We do not know whether the user is trying to deselect or drag a node. Setting this flag lets us know that
							// either is being attempted. We will handle the decision later (MouseUp) when we know what the user is doing.
							IsDeselectingNodeWithControlKey = true;
						}
					}
					else if (NodeSelector.LastSelectedNode == null || AllowMultiSelectFromDifferentParents || node.Parent == NodeSelector.LastSelectedNode.Parent)
					{
						NodeSelector.SelectNode(node);
					}
				}
				else if (ModifierKeys == Keys.Shift && rightMouseButtonWasNotClicked && NodeSelector.FirstNodeInShiftSequence != null) // will be null when no nodes are selected
				{
					ShiftSelectNodes(node);
				}
				else if (rightMouseButtonWasNotClicked || !NodeSelector.IsSelected(node)) // make sure selected nodes are not deselected when right-clicking
				{
					if (rightMouseButtonWasNotClicked && NodeSelector.IsSelected(node))
					{
						// We do not know whether the user is trying to select a single node or drag the selected node(s). Setting this flag lets us know that
						// either is being attempted. We will handle the decision later (MouseUp) when we know what the user is doing.
						IsSelectingSingleNode = true;
					}
					else
					{
						NodeSelector.DeselectAllNodes();
						NodeSelector.SelectNode(node);
					}
				}
			}
		}

		bool IsDeselectingNodeWithControlKey;
		bool IsSelectingSingleNode;

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			// If the user did not try to drag the nodes with the Ctrl Key and releases
			// the mouse button, this means that they were trying to deselect the node...
			if (IsDeselectingNodeWithControlKey)
			{
				var node = GetNodeAt(e.Location);
				if (node != null) // user can click the blank area of the treeview
				{
					NodeSelector.DeselectNode(node);
					IsDeselectingNodeWithControlKey = false; // user no longer trying to deselect a node with the Ctrl Key
				}
			}
			// If the user did not try to drag the nodes and releases
			// the mouse button, this means that they were trying to select the single node
			else if (IsSelectingSingleNode)
			{
				var node = GetNodeAt(e.Location);
				if (node != null) // user can click the blank area of the treeview
				{
					NodeSelector.DeselectAllNodes();
					NodeSelector.SelectNode(node);
					IsSelectingSingleNode = false; // user no longer trying to select a single node
				}
			}
		}

		/// <summary>
		/// Finishes the SHIFT-Sequence.
		/// </summary>
		void ShiftSelectNodes(PackingTreeNode lastSelectedNode)
		{
			PackingTreeNode firstNode = NodeSelector.FirstNodeInShiftSequence;
			PackingTreeNode lastNode = lastSelectedNode;

			if (firstNode.Parent != null && // cannot SHIFT-select the packageJob
				firstNode.Parent == lastNode.Parent) // cannot SHIFT-select nodes from other parents
			{
				var shiftStartNode = (lastNode.Index > firstNode.Index) ? firstNode : lastNode;
				var shiftEndNode = (lastNode.Index > firstNode.Index) ? lastNode : firstNode;

				// SHIFT-select the nodes
				var shiftSelectedNodes = shiftEndNode.Parent.Nodes.Cast<PackingTreeNode>().Where(n => n.Index >= shiftStartNode.Index && n.Index <= shiftEndNode.Index).ToArray();
				NodeSelector.SelectNodes(shiftSelectedNodes, shiftSelectedNodes.Length);

				// deselect nodes that are not part of the SHIFT-sequence (eg. if user SHIFT selects 1-5, then changes to 1-3)
				foreach (PackingTreeNode node in shiftStartNode.Parent.Nodes)
				{
					if (!shiftSelectedNodes.Contains(node))
					{
						NodeSelector.DeselectNode(node);
					}
				}
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			var isUserPressingUpOrDown = e.KeyCode == Keys.Up | e.KeyCode == Keys.Down;
			if (isUserPressingUpOrDown)
			{
				CurrentArrowMovement = e.KeyCode;
			}

			IsUserNavigatingWithKB = (isUserPressingUpOrDown | e.KeyCode == Keys.Left | e.KeyCode == Keys.Right);

			// user is moving left or right with shift selected
			if (!isUserPressingUpOrDown && IsUserNavigatingWithKB && ModifierKeys == Keys.Shift)
			{
				e.Handled = true; // prevent tree collapse/expand
			}

			// Ctrl Key: no KB movement can occur when Ctrl is held. Without this change the user can hold Ctrl then press up/down. Though the visible
			// selection does not change, the internal .NET selection is then set. If they then click off the tree the .NET selection would turn grey.
			if (!IsDotNetSelectedNodeCurrent && IsUserNavigatingWithKB && !ModifierKeys.HasFlag(Keys.Control))
			{
				IsDotNetSelectedNodeCurrent = true;
				SelectedNode_DotNet = NodeSelector.LastSelectedNode;

				// When clicking on the summary area (ie not the Node text), after setting the .NET
				// node it is still null. Setting the property a second time fixes this.
				if (SelectedNode_DotNet == null)
				{
					SelectedNode_DotNet = NodeSelector.LastSelectedNode;
				}
			}
			else if (e.KeyCode == Keys.Delete)
			{
				if (DeleteNodes != null)
				{
					DeleteNodes(this, EventArgs.Empty);
				}
			}
			else if (ModifierKeys == Keys.Control && e.KeyCode == Keys.A)
			{
				var parentNode = IsPackageJobSelected || !NodeSelector.IsAnyNodeSelected ? TopNode : NodeSelector.LastSelectedNode.Parent;
				if (parentNode.Nodes.Count > 0) // don't deselect parent for no reason (ie no packages on job).
				{
					NodeSelector.DeselectNode(parentNode);
					NodeSelector.SelectNodes(parentNode.Nodes.Cast<PackingTreeNode>(), parentNode.Nodes.Count);
				}
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			PreventDotNetSelection();
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			base.OnBeforeSelect(e);

			// Don't allow nodes to be selected if we are using the mouse, as we manage selection with the NodeSelector.
			//
			// Secondly, when the tree regains focus .NET attempts to select the very first node if SelectedNode is NULL.
			// Testing for !IsUserNagivatingWithKB and cancelling the selection will stop .NET from doing this.
			if (IsUserNavigatingWithMouse
#if !WINZOR
		|| !IsUserNavigatingWithKB
#endif
			)
			{
				e.Cancel = true;
				IsUserNavigatingWithMouse = false;
			}
		}

		protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);

			// if the user is using the KB, make sure we track the .NET .SelectedNode
			if (e.Action == TreeViewAction.ByKeyboard)
			{
				if (ModifierKeys == Keys.Shift && CurrentArrowMovement != Keys.None)
				{
					ShiftSelectNodes(e);
				}
				else
				{
					NodeSelector.DeselectAllNodes();
					NodeSelector.SelectNode(SelectedNode_DotNet);
				}

				PreventDotNetSelection(); // removing this causes a graphical flicker

				// user action (eg. down arrow) is complete, IsUserNavigatingWithKB should no longer be true.
				// if the tree loses/regains focus .NET will select the top node and if IsUserNavigatingWithKB is not reset, we will not cancel .NET's selection resulting in multiple selected nodes.
				IsUserNavigatingWithKB = false;
			}
		}

		void ShiftSelectNodes(TreeViewEventArgs e)
		{
			if (!IsPackageJobSelected)
			{
				if (e.Node.Parent == NodeSelector.LastSelectedNode.Parent)
				{
					SelectOrDeselectNode((PackingTreeNode)e.Node);
				}
				else
				{
					var lastSelectedNode = NodeSelector.LastSelectedNode;
					var nextNodeToShiftSelect = (PackingTreeNode)(CurrentArrowMovement == Keys.Down ? lastSelectedNode.NextNode : lastSelectedNode.PrevNode);
					if (nextNodeToShiftSelect != null)
					{
						SelectOrDeselectNode(nextNodeToShiftSelect);
					}
				}
			}

			CurrentArrowMovement = Keys.None;
		}

		void SelectOrDeselectNode(PackingTreeNode packingNode)
		{
			if (NodeSelector.IsSelected(packingNode))
			{
				NodeSelector.DeselectNode(NodeSelector.LastSelectedNode, packingNode);
			}
			else
			{
				NodeSelector.SelectNode(packingNode);
			}
		}

		void PackingTreeView_AfterSelectOrDeselect(object sender, EventArgs e)
		{
			PackageJob.Selected.UpdateSelectedPackages(SelectedPackages);
			PackageJob.Selected.UpdateSelectedPackedItems(SelectedPackedItems);
		}

		void PreventDotNetSelection()
		{
			// don't let .NET handle selection (it turns the selection grey when losing focus, we don't want this)
			IsDotNetSelectedNodeCurrent = false;
#if !WINZOR
			SelectedNode_DotNet = null;
#endif
		}

		public MultiNodeSelector NodeSelector => nodeSelector ?? (nodeSelector = new MultiNodeSelector(ForeColor));

		bool IsUserNavigatingWithMouse;
		bool IsUserNavigatingWithKB;
		bool IsDotNetSelectedNodeCurrent;
		MultiNodeSelector nodeSelector;
		Keys CurrentArrowMovement;

		#endregion

		#endregion

		#region ActionsHelper

		public PackingTreeViewActions ActionsHelper
		{
			get
			{
				if (actionsHelper == null)
				{
					actionsHelper = new PackingTreeViewActions(this);
					actionsHelper.BreakDownFailed += ActionsHelper_PackOrUnpackOrBreakDownFailed;
					actionsHelper.PackSucceeded += ActionsHelper_PackSucceeded;
					actionsHelper.PackOrUnpackFailed += ActionsHelper_PackOrUnpackOrBreakDownFailed;
					actionsHelper.UnpackSucceeded += ActionsHelper_UnpackSucceeded;
				}
				return actionsHelper;
			}
		}

		PackingTreeViewActions actionsHelper;

		#endregion

		#region Pack/Unpack Succeeded/Failed

		public event EventHandler PackOrUnpackSucceeded;

		// pack succeeded

		void ActionsHelper_PackSucceeded(object sender, PackEventArgs e)
		{
			UpdateTreeAndGridAfterPackingItems(e.NewPackages, e.IsPackingAlongsideSelectedPackage);
			OnPackOrUnpackSucceeded();
		}

		void UpdateTreeAndGridAfterPackingItems(IEnumerable<PkgPackage> newPackages, bool isPackingAlongsideSelectedPackage)
		{
			bool isAddingNewPackageNodes = (newPackages != null && newPackages.Any());
			if (isAddingNewPackageNodes)
			{
				UpdateTreeAndGridAfterPackingItems_AddNewPackages(newPackages, isPackingAlongsideSelectedPackage);
			}
			else
			{
				UpdateTreeAndGridAfterPackingItems_AddNewPackableItems();
			}
		}

		void UpdateTreeAndGridAfterPackingItems_AddNewPackages(IEnumerable<PkgPackage> newPackages, bool isPackingAlongsideSelectedPackage)
		{
			PackingTreeNode lastInsertedNode = null;
			try
			{
				BeginUpdate();

				#region Rules

				//  1. if nothing selected, new package is put on the job as an Outer
				// 
				//  2. if we have ("[]" denotes selected package):
				//
				//     PLT
				//	     [BOX a]
				//
				//     and we scan a BOX TUN code, the BOX will be inserted alongside the current selection, i.e.:
				//
				//     PLT
				//	      BOX a
				//	     [BOX b]
				//
				// 
				//  3. else, pack into the selected package

				#endregion

				var nodeToInsertInto = (SelectedNode == null || newPackages.First().IsOuter) ? TopNode : isPackingAlongsideSelectedPackage ? SelectedNode.Parent : SelectedNode;

				// if Packing new Outers the Outer will be selected below, so clear existing selections first.
				if (nodeToInsertInto.IsPackageJob)
				{
					NodeSelector.DeselectAllNodes();
				}

				foreach (var package in newPackages)
				{
					var node = PopulateFromPackage(package, nodeToInsertInto);
					node.ExpandAll();
					lastInsertedNode = node;

					// select new Outers
					if (nodeToInsertInto.IsPackageJob)
					{
						NodeSelector.SelectNode(node);
					}
				}

				nodeToInsertInto.Expand();
			}
			finally
			{
				EndUpdate();
			}

			// scroll down the tree so that the newly packed items are onscreen
			if (lastInsertedNode != null)
			{
				lastInsertedNode.EnsureVisible();
			}
		}

		void UpdateTreeAndGridAfterPackingItems_AddNewPackableItems()
		{
			PackingTreeNode lastInsertedNode = null;
			BeginUpdate();
			try
			{
				foreach (var node in SelectedPackagesNodes)
				{
					PopulatePackageChildNodes(node);
					node.Expand();
					lastInsertedNode = node;
				}
			}
			finally
			{
				EndUpdate();
			}

			// scroll down the tree so that the newly packed items are onscreen
			if (lastInsertedNode != null)
			{
				lastInsertedNode.EnsureVisible();
			}
		}

		// pack / unpack succeeded

		void ActionsHelper_UnpackSucceeded(object sender, UnpackEventArgs e)
		{
			UpdateTreeSelectionAfterUnpackingItems(e.ParentOfSelectedNodesPriorToUnpack);
			OnPackOrUnpackSucceeded();
		}

		void UpdateTreeSelectionAfterUnpackingItems(PackingTreeNode parentOfSelectedNodesPriorToUnpack)
		{
			if (!IsAnyNodeSelected)
			{
				// using the keyboard after a select relies on where .NET thinks the node is.
				// this lets .NET know its selected node is no longer correct
				IsDotNetSelectedNodeCurrent = false;
				NodeSelector.SelectNode(parentOfSelectedNodesPriorToUnpack ?? TopNode);
			}
		}

		void OnPackOrUnpackSucceeded()
		{
			if (PackOrUnpackSucceeded != null)
			{
				PackOrUnpackSucceeded(this, EventArgs.Empty);
			}
		}

		// pack/unpack/break down failed

		void ActionsHelper_PackOrUnpackOrBreakDownFailed(object sender, PackingActionFailedEventArgs e)
		{
			if (e.IsScanPacking)
			{
				GetParentPackageControl().ParentForm.ShowScanningMessage(e.Message, NotificationTypes.Error);
			}
			else
			{
				Globals.Message.ShowError(e.Message);
			}
		}

		#endregion

		#region Flags

		public bool IsPackageJobSelected => Nodes.Count > 0 && NodeSelector.IsSelected(TopNode);

		public bool IsPackageSelected => NodeSelector.IsAnyPackageSelected;

		public bool IsPackableItemSelected => NodeSelector.IsPackableItemSelected;

		public bool IsSinglePackageSelected => NodeSelector.IsSingleNodeSelected && IsPackageSelected;

		#endregion

		#region SelectedPackageNodeQtyOrTypeChanged

		public event EventHandler SelectedPackageNodeQtyOrTypeChanged;

		public void OnSelectedPackageNodeQtyOrTypeChanged()
		{
			if (SelectedPackageNodeQtyOrTypeChanged != null)
			{
				SelectedPackageNodeQtyOrTypeChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Drag/Drop

		PackingTreeNode GetDraggedOverNode(DragEventArgs e)
		{
			Argument.NotNull(e, "e");

			return GetDraggedOverNodeCore(e);
		}

		protected virtual PackingTreeNode GetDraggedOverNodeCore(DragEventArgs e)
		{
			var point = PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y)));
			return GetNodeAt(point);
		}

		#region Drag/Drop Events

#if !WINZOR

		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			base.OnItemDrag(e);

			if (e.Button == MouseButtons.Left)
			{
				// without this, OnDraggedOverNodeChanged() is invoked when the user starts dragging (can result in said node remaining highlighted)
				PreviousNodeMouseWasOver = e.Item as PackingTreeNode;
			}
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);

			// When dragging out of the tree, unpaints any hovered over node.
			UnpaintMouseOverNode();
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);

			if (!ParentReadOnly)
			{
				e.Effect = DragDropEffects.Move;
			}
		}

#endif

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);

			if (!ParentReadOnly) // safeguard; should never be here when readonly.
			{
				var destinationNode = GetDraggedOverNode(e);
				if (DragDropHelper.IsNodeValidDropTarget(destinationNode))
				{
					var formats = e.Data?.GetFormats();
					object data = formats?.Length > 0 ? e.Data.GetData(formats[0]) : null;

					ArrayList elements = data switch
					{
						ArrayList arrayList => arrayList,
						IList genericList => new ArrayList(genericList),
						null => throw new NullReferenceException("The data is null."),
						_ => throw new InvalidCastException("The data is not of type ArrayList or List<T>.")
					};

					if (DragDropHelper.IsValidSource(elements))
					{
						// user dropped lines from the packable item parents grid
						if (elements[0] is PackableItemParentWrapper)
						{
							DragDropHelper.HandleDragDropOfGridItems(elements.Cast<PackableItemParentWrapper>(), destinationNode);
						}
						// user dropped nodes
						else if (elements[0] is PkgPackageItemDivotsWrapper || elements[0] is PkgPackage)
						{
							DragDropHelper.HandleDragDropOfNodes(NodeSelector.SelectedNodes, destinationNode);
						}
						// user dropped packageHeader (Package ID)
						else if (elements[0] is PkgPackageJobPackageHeaderPivot)
						{
							// tested in PackageTreeViewDragDropActions.cs
							DragDropHelper.HandleDragDropOfPkgPackageHeader(elements.Cast<PkgPackageJobPackageHeaderPivot>().ToArray(), destinationNode);
						}
					}

					// finally, unpaint the node we've dropped onto (otherwise it remains highlighted)
					if (!NodeSelector.IsSelected(destinationNode))
					{
						NodeSelector.UnpaintNode(destinationNode);
					}
				}
			}
		}

		void DragDropHelper_DragDropFailed(object sender, PackingTreeViewDragDropActions.DragDropActionsEventArgs e)
		{
			Globals.Message.ShowError(e.Message, Res.GetString("0fc9ae91-a769-4b9d-be78-f24ed8b21440", "Packing Error"));
		}

		PackingTreeViewDragDropActions DragDropHelper
		{
			get
			{
				if (dragDropHelper == null)
				{
					dragDropHelper = new PackingTreeViewDragDropActions(this);
					dragDropHelper.DragDropFailed += new EventHandler<PackingTreeViewDragDropActions.DragDropActionsEventArgs>(DragDropHelper_DragDropFailed);
				}
				return dragDropHelper;
			}
		}

		PackingTreeViewDragDropActions dragDropHelper;

		#endregion

		#region Painting the Dragged-Over Node

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);

			if (!ParentReadOnly)
			{
				var draggedOverNode = GetDraggedOverNode(e);

				if (PreviousNodeMouseWasOver != draggedOverNode)
				{
					OnDraggedOverNodeChanged(draggedOverNode);
				}

				PreviousNodeMouseWasOver = draggedOverNode;
			}
		}

		protected void OnDraggedOverNodeChanged(PackingTreeNode draggedOverNode)
		{
			UnpaintMouseOverNode();

			if (DragDropHelper.IsNodeValidDropTarget(draggedOverNode))
			{
				NodeSelector.PaintNode(draggedOverNode);
			}
		}

		void UnpaintMouseOverNode()
		{
			if (PreviousNodeMouseWasOver != null && !NodeSelector.IsSelected(PreviousNodeMouseWasOver))
			{
				NodeSelector.UnpaintNode(PreviousNodeMouseWasOver);
			}
		}

		PackingTreeNode PreviousNodeMouseWasOver;

		#endregion

		#region IsDragging

		protected override void BeforeDragDrop()
		{
			base.BeforeDragDrop();
			IsDragging = true;
			IsDeselectingNodeWithControlKey = false; // user is dragging, thus they are not deselecting the selected node with the Ctrl Key
			IsSelectingSingleNode = false; // user is dragging, thus they are not selecting a single node
		}

		protected override void AfterDragDrop()
		{
			base.AfterDragDrop();
			IsDragging = false;
		}

		public bool IsDragging
		{
			get;
			private set;
		}

		#endregion

		#region IsDraggingFromGrid

		public bool IsDraggingFromGrid
		{
			get
			{
				var userControl = GetParentPackageControl();
				var treeViewUserControl = this.GetParent<PackingTreeViewUserControl>();

				return (userControl != null && userControl.IsGridDragging) || (treeViewUserControl != null && treeViewUserControl.IsPackageIDsGridDragging);
			}
		}

		#endregion

		#endregion

		#region SelectedPackage(s), SelectedPackedItem(s), SelectedNode(s)

		// packages

		public PkgPackage SelectedPackage => (NodeSelector.IsSingleNodeSelected && NodeSelector.LastSelectedNode.IsPackage) ? NodeSelector.LastSelectedNode.Package : null; // packed items will not be PkgPackages

		public PackingTreeNode SelectedPackageNodeOrParentPackageNode
		{
			get
			{
				PackingTreeNode result = null;

				if (IsSinglePackageSelected)
				{
					result = SelectedNode;
				}
				else if (IsAnyNodeSelected)
				{
					var selectedNode = SelectedNodes[0]; // we know that multi-selection is only allowed within the same parent
					if (selectedNode.Parent != null && selectedNode.Parent.IsPackage)
					{
						result = selectedNode.Parent;
					}
				}

				return result;
			}
		}

		public IEnumerable<PkgPackage> SelectedPackages => SelectedPackagesNodes.Select(node => node.Package);

		public IEnumerable<PkgPackage> SelectedPackagesOrAllIfPackageJobSelected => IsPackageJobSelected ? PackageJob.Packages : SelectedPackages;

		public IEnumerable<PackingTreeNode> SelectedPackagesNodes => SelectedNodes.Where(node => node.IsPackage);

		// packed items

		public IEnumerable<PkgPackageItemDivotsWrapper> SelectedPackedItems => SelectedNodes.Where(node => node.IsPackedItem).Select(node => node.PackedItem);

		#endregion

		#region FindNodeByPackage

		public PackingTreeNode FindNodeByPackage(PkgPackage package)
		{
			var outers = Nodes[0].Nodes;
			return FindNodeByPackage(package, outers);
		}

		PackingTreeNode FindNodeByPackage(PkgPackage package, TreeNodeCollection nodes)
		{
			var packageNodes = nodes.Cast<PackingTreeNode>().Where(n => n.IsPackage);

			// more likely to be using only outers so its faster to check these first (hence two loops)
			foreach (var node in packageNodes)
			{
				if (package == node.Package)
				{
					return node;
				}
			}

			foreach (var node in packageNodes)
			{
				var matchingNode = FindNodeByPackage(package, node.Nodes);
				if (matchingNode != null)
				{
					return matchingNode;
				}
			}

			return null;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (isNotFinalizing)
			{
				UnhookEvents();

				PreviousNodeMouseWasOver = null;
				RemoveAllNodes();

				if (nodePainter != null)
				{
					nodePainter.Dispose();
				}

				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		protected void RemoveAllNodes()
		{
			Array.ForEach(this.Nodes.Cast<PackingTreeNode>().ToArray(), node => node.Dispose());
			NodeSelector.DeselectAllNodes();

			if (!this.IsDisposed)
			{
				this.Nodes.Clear();
			}
		}

		#endregion
	}
}
