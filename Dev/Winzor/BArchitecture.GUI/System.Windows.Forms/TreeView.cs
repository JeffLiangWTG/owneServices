using System.Collections;
using System.Drawing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

public partial class TreeView : Control
{
	internal bool nodesCollectionClear; //this is set when the treeNodeCollection is getting cleared and used by TreeView
	internal TreeNode root;
	readonly DotNetObjectReference<TreeView> dotNetObjectReference;

	public TreeView()
	{
		AfterSelect += (s, e) => ExpandAllParentNodes(e.Node);

		root = new TreeNode(this);
		dotNetObjectReference = DotNetObjectReference.Create(this);
	}

	public override bool UseParentDivForLayout => false;

	public TreeNodeCollection Nodes => nodes ??= new TreeNodeCollection(root);
	TreeNodeCollection? nodes;

	protected internal virtual bool IsNodeNeedHighlight(TreeNode node) => node.IsSelected && node.TreeView.Focused;

	bool selectedNodeChangedWhileHidden;
	public TreeNode? SelectedNode
	{
		get
		{
			if (selectedNode?.TreeView == this)
			{
				return selectedNode;
			}

			return null!;
		}
		set
		{
			if (selectedNode == value)
			{
				return;
			}

			if (value == null)
			{
				selectedNode?.DeselectCore();
				selectedNode = null;
				return;
			}

			var eventArgs = new TreeViewCancelEventArgs(value, false, TreeViewAction);
			OnBeforeSelect(eventArgs);

			if (eventArgs.Cancel)
			{
				return;
			}

			selectedNode?.DeselectCore();
			selectedNode = value;

			selectedNode.SelectCore();

			if (Visible)
			{
				RegisterAfterRenderAction(async () => await (GetJSInterop<ITreeViewJSInterop>()?.ScrollIntoViewAndFocusAsync(selectedNode) ?? Task.CompletedTask));
				OnAfterSelect(new TreeViewEventArgs(selectedNode, TreeViewAction));
			}
			else
			{
				selectedNodeChangedWhileHidden = true;
			}
		}
	}
	TreeNode? selectedNode;

	protected override void WmSetFocus()
	{
		base.WmSetFocus();

		// In WinForms, when TreeView gets focus, the TVN_SELCHANGINGW messsage is sent and this means that the first node has been selected if one is not already selected
		if (SelectedNode == null)
		{
			SelectedNode = Nodes.FirstOrDefault();
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);

		if (Visible && selectedNodeChangedWhileHidden)
		{
			OnAfterSelect(new TreeViewEventArgs(selectedNode));
			selectedNodeChangedWhileHidden = false;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Control | Keys.C) && selectedNode is not null && !selectedNode.ElementReference.Equals(default(ElementReference)))
		{
			InvokeRenderDispatcher(async () => await (GetJSInterop<ITreeViewJSInterop>()?.CopyTreeNodeContentAsync(selectedNode.ElementReference) ?? Task.CompletedTask));
			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	public TreeNode? TopNode { get; }

	public ImageList? ImageList { get; set; }

	public int ImageIndex { get; set; }

	public ImageList? StateImageList { get; set; }

	public Color LineColor { get; set; }

	public TreeViewDrawMode DrawMode { get; set; }

	public bool ShowNodeToolTips { get; set; }

	public bool ShowRootLines { get; set; }

	public bool HideSelection { get; set; }

	public bool LabelEdit { get; set; }

	public int SelectedImageIndex { get; set; }

	public string PathSeparator { get; set; } = "\\";

	public int ItemHeight { get; set; }

	protected object? dragData;

	internal TreeViewAction TreeViewAction = TreeViewAction.Unknown;

	bool sorted;
	public bool Sorted
	{
		get => sorted;
		set
		{
			sorted = value;
			NotifyRenderRequired();
		}
	}

	public void Sort()
	{
		Sorted = true;
		RefreshNodes();
	}

	void Sort(TreeNodeCollection nodes)
	{
		if (nodes.Count > 0)
		{
			ArrayList.Adapter(nodes).Sort(TreeViewNodeSorter ?? DefaultSorter);

			foreach (var treeNode in nodes)
			{
				Sort(treeNode.Nodes);
			}
		}
	}

	public IComparer? TreeViewNodeSorter { get; set; }

	bool checkBoxes;
	public bool CheckBoxes
	{
		get => checkBoxes;
		set
		{
			checkBoxes = value;
			NotifyRenderRequired();
		}
	}

	public BorderStyle BorderStyle
	{
		get => borderStyle;
		set
		{
			if (UpdateProperty(ref borderStyle, value))
			{
				UpdateStyles();

				// Border style might have changed ClientSize
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}

	BorderStyle borderStyle = BorderStyle.Fixed3D;

	public int GetNodeCount(bool includeSubTrees)
	{
		var count = Nodes.Count;

		if (includeSubTrees)
		{
			foreach (var node in Nodes)
			{
				count += node.GetNodeCount(includeSubTrees);
			}
			return count;
		}
		else
		{
			return count;
		}
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	public void CollapseAll()
	{
		foreach (var node in Nodes)
		{
			node.Collapse();
		}
		NotifyRenderRequired();
	}

	public void ExpandAll()
	{
		foreach (var node in Nodes)
		{
			node.ExpandAll();
		}
		NotifyRenderRequired();
	}

	void ExpandAllParentNodes(TreeNode node)
	{
		var parent = node?.Parent;
		if (parent is not null)
		{
			parent.Expand();
			ExpandAllParentNodes(parent);
		}
	}

	protected TreeViewHitTestInfo? currentHitTest;

	public TreeViewHitTestInfo HitTest(Point pt) => currentHitTest ?? new TreeViewHitTestInfo(null, TreeViewHitTestLocations.None);

	public TreeNode GetNodeAt(int x, int y) => GetNodeAt(new Point(x, y));

	public TreeNode GetNodeAt(Point pt)
	{
		return HitTest(pt).Node;
	}

	protected internal virtual async Task OnMouseDownAsync(WebMouseEventArgs e, TreeViewHitTestInfo hitTest)
	{
		currentHitTest = hitTest;
		await OnMouseDownAsync(e);
	}

	protected internal virtual async Task OnContextMenuAsync(WebMouseEventArgs e, TreeViewHitTestInfo hitTest)
	{
		currentHitTest = hitTest;
		await OnContextMenuAsync(e);
	}

	protected internal virtual async Task OnDragDropAsync((WebDragEventArgs, TreeViewHitTestInfo) args)
	{
		var (e, hitTestInfo) = args;
		currentHitTest = hitTestInfo;
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnDragDrop(
				new DragEventArgs(
					new DataObject(dragData ?? this),
					0,
					(int)e.ClientX,
					(int)e.ClientY,
					DragDropEffects.Move,
					DragDropEffects.Move));
		});
	}

	protected virtual void OnAfterExpand(TreeViewEventArgs e)
	{
		AfterExpand?.Invoke(this, e);
	}

	protected virtual void OnBeforeExpand(TreeViewCancelEventArgs e)
	{
		BeforeExpand?.Invoke(this, e);
	}

	protected virtual void OnBeforeCollapse(TreeViewCancelEventArgs e)
	{
		BeforeCollapse?.Invoke(this, e);
	}

	protected virtual void OnAfterCollapse(TreeViewEventArgs e)
	{
		AfterCollapse?.Invoke(this, e);
	}

	protected virtual void OnAfterLabelEdit(NodeLabelEditEventArgs e)
	{
		AfterLabelEdit?.Invoke(this, e);
	}

	protected virtual void OnAfterSelect(TreeViewEventArgs e)
	{
		AfterSelect?.Invoke(this, e);
	}

	protected virtual void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
	{
		NodeMouseClick?.Invoke(this, e);
	}

	protected virtual void OnBeforeSelect(TreeViewCancelEventArgs e)
	{
		BeforeSelect?.Invoke(this, e);
	}

	protected virtual void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
	{
		BeforeLabelEdit?.Invoke(this, e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
	}

	protected virtual void OnAfterCheck(TreeViewEventArgs e)
	{
		AfterCheck?.Invoke(this, e);
	}

	internal void TreeViewAfterCheck(TreeNode node, TreeViewAction actionTaken)
	{
		OnAfterCheck(new TreeViewEventArgs(node, actionTaken));
	}

	public override DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
	{
		if (data is not null)
		{
			dragData = data;
		}
		return base.DoDragDrop(data ?? this, DragDropEffects.None);
	}

	// Just give it an init value. It will be updated when the treeview is first rendered and everytime resized.
	[Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	int nodesPerPage = 10;

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in TreeView.razor")]
	void HandleNavigationKey(Keys key)
	{
		var currentNode = SelectedNode;
		if (currentNode == null)
		{
			SelectedNode = Nodes.Count > 0 ? Nodes[0] : null;
			return;
		}

		switch (key)
		{
			case Keys.Up:
				var prevVisibleNode = currentNode.PrevVisibleNode;
				if (prevVisibleNode != null)
				{
					SelectedNode = prevVisibleNode;
				}
				break;
			case Keys.Down:
				var nextVisibleNode = currentNode.NextVisibleNode;
				if (nextVisibleNode != null)
				{
					SelectedNode = nextVisibleNode;
				}
				break;
			case Keys.PageUp:
				var stepBack = nodesPerPage - 1;
				var prevNode = currentNode.PrevVisibleNode;
				while (stepBack > 0 && prevNode != null)
				{
					stepBack--;
					currentNode = prevNode;
					prevNode = prevNode.PrevVisibleNode;
				}
				SelectedNode = currentNode;
				break;
			case Keys.PageDown:
				var stepForward = nodesPerPage - 1;
				var nextNode = currentNode.NextVisibleNode;
				while (stepForward > 0 && nextNode != null)
				{
					stepForward--;
					currentNode = nextNode;
					nextNode = nextNode.NextVisibleNode;
				}
				SelectedNode = currentNode;
				break;
			case Keys.Left:
				if (currentNode.IsExpanded)
				{
					currentNode.Collapse();
				}
				else if (currentNode.Parent != null)
				{
					SelectedNode = currentNode.Parent;
				}
				break;
			case Keys.Right:
				if (currentNode.IsExpanded)
				{
					SelectedNode = currentNode.FirstNode;
				}
				else if (currentNode.Nodes.Count > 0)
				{
					currentNode.Expand();
				}
				break;
			case Keys.Home:
				if (Nodes.Count > 0 && currentNode != Nodes[0])
				{
					SelectedNode = Nodes[0];
				}
				break;
			case Keys.End:
				var lastNode = Nodes.Count > 0 ? Nodes[^1] : null;
				if (lastNode != null)
				{
					while (lastNode.IsExpanded)
					{
						lastNode = lastNode.LastNode;
					}
					SelectedNode = lastNode;
				}
				break;
			default:
				break;
		}
	}

	public event TreeViewEventHandler? AfterCheck;

	public event NodeLabelEditEventHandler? AfterLabelEdit;

	public event TreeViewEventHandler? AfterSelect;

	public event TreeViewCancelEventHandler? BeforeCollapse;

	public event TreeViewEventHandler? AfterCollapse;

	public event TreeViewCancelEventHandler? BeforeExpand;

	public event TreeViewEventHandler? AfterExpand;

	public event NodeLabelEditEventHandler? BeforeLabelEdit;

	public event TreeViewCancelEventHandler? BeforeSelect;

	public event ItemDragEventHandler? ItemDrag;

	public event TreeNodeMouseClickEventHandler? NodeMouseClick;

	public event TreeNodeMouseClickEventHandler? NodeMouseDoubleClick;

	internal void NotifyToUpdate()
	{
		NotifyRenderRequired();
	}

	// Refresh the nodes by clearing the tree and adding the nodes back again
	void RefreshNodes()
	{
		TreeNode[] nodes = new TreeNode[Nodes.Count];
		Nodes.CopyTo(nodes, 0);

		Nodes.Clear();
		Nodes.AddRange(nodes);
	}

	internal async Task ExpandedNodeScrollIntoViewAsync(TreeNode node)
	{
		await (GetJSInterop<ITreeViewJSInterop>()?.ScrollIntoViewAsync(node.ElementReference) ?? Task.CompletedTask);
	}

	internal void ExpandNode(TreeNode node)
	{
		var eventArgs = new TreeViewCancelEventArgs(node, false, TreeViewAction.Expand);
		OnBeforeExpand(eventArgs);
		if (!eventArgs.Cancel)
		{
			node.ExpandCore();
			OnAfterExpand(new TreeViewEventArgs(node));
		}
	}

	internal void CollapseNode(TreeNode node)
	{
		var eventArgs = new TreeViewCancelEventArgs(node, false, TreeViewAction.Collapse);
		OnBeforeCollapse(eventArgs);
		if (!eventArgs.Cancel)
		{
			node.CollapseCore();
			OnAfterCollapse(new TreeViewEventArgs(node));
		}
	}

	public override Color BackColor
	{
		get
		{
			if (ShouldSerializeBackColor())
			{
				return base.BackColor;
			}

			return SystemColors.Window;
		}
		set => base.BackColor = value;
	}

	protected override Size DefaultSize => new Size(121, 97);

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		NotifyRenderRequired();
	}

	protected override void OnLostFocus(EventArgs e)
		{
		base.OnLostFocus(e);
		NotifyRenderRequired();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		NotifyRenderRequired();
	}

	protected internal override void OnBeforeRender()
	{
		base.OnBeforeRender();
		foreach (var node in Nodes)
		{
			node.OnBeforeRender();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			dotNetObjectReference?.Dispose();
		}
		base.Dispose(disposing);
	}

	internal readonly IComparer DefaultSorter = new DefaultComparer();

	class DefaultComparer : IComparer<TreeNode>, IComparer
	{
		internal DefaultComparer()
		{
		}

		public int Compare(TreeNode? x, TreeNode? y)
		{
			int result;

			if (x == null && y == null)
			{
				result = 0;
			}
			else if (x == null)
			{
				result = -1;
			}
			else if (y == null)
			{
				result = 1;
			}
			else
			{
				result = string.Compare(x.Text, y.Text, StringComparison.Ordinal);
			}

			return result;
		}

		public int Compare(object? x, object? y)
		{
			return Compare((TreeNode?)x, (TreeNode?)y);
		}
	}
}
