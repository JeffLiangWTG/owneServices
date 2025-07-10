using System.Collections;
using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Components;

#nullable disable

namespace System.Windows.Forms;

public class TreeNode
{
	internal IntPtr handle;

	internal const int MAX_TREENODES_OPS = 200;

	internal int childCount => GetNodeCount(false);

	internal bool nodesCleared;

	internal TreeNode[] children;

	internal int index;                  // our index into our parents child array

	public virtual ElementReference ElementReference
	{
		get
		{
			return reference;
		}
		protected internal set
		{
			reference = value;
			IsElementReferenceCaptured = !value.Equals(default(ElementReference));
		}
	}

	public Guid TreeNodeKey
	{
		get;
		private set;
	}

	public bool IsElementReferenceCaptured
	{
		get;
		private set;
	}

	ElementReference reference;

	public TreeNode()
	{
		TreeNodeKey = Guid.NewGuid();
	}

	public TreeNode(string text) : this()
	{
		Text = text;
	}

	public TreeNode(string text, int imageIndex, int selectedImageIndex) : this(text)
	{
		ImageIndex = imageIndex;
	}

	internal TreeNode(TreeView treeView) : this()
	{
		this.treeView = treeView;
	}

	public TreeNode(string text, TreeNode[] treeNodes) : this(text)
	{
		this.Nodes.AddRange(treeNodes);
	}

	public TreeView TreeView => treeView ??= FindTreeView();
	internal TreeView treeView;

	internal TreeView FindTreeView()
	{
		var node = this;
		while (node.parent is not null)
		{
			node = node.parent;
		}
		return node.treeView;
	}

	void GetFullPath(StringBuilder path, string pathSeparator)
	{
		if (parent is not null)
		{
			parent.GetFullPath(path, pathSeparator);
			if (parent.parent is not null)
			{
				path.Append(pathSeparator);
			}

			path.Append(text);
		}
	}

	public bool IsDescendantOf(TreeNode inNode)
	{
		var node = this;
		while (node.parent is not null)
		{
			if (node == inNode)
			{
				return true;
			}
			node = node.parent;
		}
		return false;
	}

	public string Text
	{
		get => text;
		set
		{
			UpdateProperty(ref text, value);
		}
	}
	string text = string.Empty;

	public TreeNode Parent
	{
		get
		{
			var tv = TreeView;

			// Don't expose the virtual root publicly
			if (tv != null && parent == tv.root)
			{
				return null;
			}

			return parent;
		}
		internal set
		{
			parent = value;
		}
	}
	TreeNode parent;

	/// <summary>
	/// The next sibling node.
	/// </summary>
	public TreeNode NextNode
	{
		get
		{
			var currentIndex = Index;
			var parentNodes = GetParentNodes();
			return parentNodes != null && currentIndex >= 0 && currentIndex + 1 < parentNodes.Count
				? parentNodes[currentIndex + 1]
				: null;
		}
	}

	/// <summary>
	/// The previous sibling node.
	/// </summary>
	public TreeNode PrevNode
	{
		get
		{
			var currentIndex = Index;
			var parentNodes = GetParentNodes();
			return parentNodes != null && currentIndex > 0 && currentIndex <= parentNodes.Count
				? parentNodes[currentIndex - 1]
				: null;
		}
	}

	/// <summary>
	/// The previous visible node.  It may be a parent, sibling,
	/// or a node from another branch.
	/// </summary>
	public TreeNode PrevVisibleNode
	{
		get
		{
			var prevNode = PrevNode;
			if (prevNode == null)
			{
				return Parent;
			}

			while (prevNode.IsExpanded)
			{
				prevNode = prevNode.LastNode;
			}
			return prevNode;
		}
	}

	/// <summary>
	/// The first child node of this node.
	/// </summary>
	public TreeNode FirstNode
	{
		get
		{
			return GetNodeCount(false) > 0 ? Nodes[0] : null;
		}
	}

	/// <summary>
	/// The last child node of this node.
	/// </summary>
	public TreeNode LastNode
	{
		get
		{
			return GetNodeCount(false) > 0 ? Nodes[^1] : null;
		}
	}

	/// <summary>
	/// The next visible node.  It may be a child, sibling,
	/// or a node from another branch.
	/// </summary>
	public TreeNode NextVisibleNode
	{
		get
		{
			var firstChildNode = FirstNode;
			if (isExpanded && firstChildNode != null)
			{
				return firstChildNode;
			}

			var firstSiblingNode = NextNode;
			if (firstSiblingNode != null)
			{
				return firstSiblingNode;
			}

			var parentNode = Parent;
			if (parentNode == null)
			{
				return null;
			}

			var parentSiblingNode = parentNode.NextNode;
			while (parentSiblingNode == null && parentNode != null)
			{
				parentSiblingNode = parentNode.NextNode;
				parentNode = parentNode.Parent;
			}

			return parentSiblingNode;
		}
	}

	public string Name
	{
		get
		{
			return name ?? string.Empty;
		}
		set
		{
			this.name = value;
		}
	}
	string name;

	public int Level { get; }

	public string ToolTipText { get; set; }

	public object Tag { get; set; }

	public int Index {
		get
		{
			var parentNodes = GetParentNodes();
			if (parentNodes is not null && parentNodes.Count > 0)
			{
				return parentNodes.IndexOf(this);
			}
			return 0;
		}
	}

	internal TreeNodeCollection GetParentNodes()
	{
		if (Parent is not null)
		{
			return parent.Nodes;
		}
		if (TreeView is not null)
		{
			return TreeView.Nodes;
		}
		return null;
	}

	public int ImageIndex { get; set; }

	public int SelectedImageIndex { get; set; }

	public int StateImageIndex { get; set; }

	public Color BackColor { get; set; }

	public Color ForeColor { get; set; }

	public Font NodeFont { get; set; }

	internal List<string> NodeTokens { get; set; }
	protected internal virtual List<string> SummaryTokens { get; }

	protected internal virtual string SummaryStatus { get; }
	internal string Status { get; set; } = "";

	protected internal Color TokensColor { get; set; }

	protected internal Font TokensCaptionFont { get; set; }

	protected internal Font TokensFont { get; set; }

	protected internal Color StatusColor { get; set; }

	protected internal Font StatusFont { get; set; }

	public virtual ContextMenu ContextMenu { get; set; }

	public TreeNodeCollection Nodes => nodes ??= new TreeNodeCollection(this);
	TreeNodeCollection nodes;

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

	public string FullPath
	{
		get
		{
			var treeView = TreeView;
			if (treeView != null)
			{
				var stringBuilder = new StringBuilder();
				GetFullPath(stringBuilder, treeView.PathSeparator);
				return stringBuilder.ToString();
			}

			throw new InvalidOperationException(SR.TreeNodeNoParent);
		}
	}

	public Rectangle Bounds { get; }

	public bool Checked
	{
		get => isChecked;
		set
		{
			UpdateProperty(ref isChecked, value);
			TreeView?.TreeViewAfterCheck(this, TreeViewAction.Unknown);
		}
	}
	bool isChecked;

	internal async Task CheckedChangedAsync(ChangeEventArgs e)
	{
		if (e.Value is null || !(e.Value is bool isChecked))
		{
			return;
		}

		await (TreeView?.InvokeWinzorDispatcherAsync(() => Checked = isChecked) ?? Task.CompletedTask);
	}

	protected bool UpdateProperty<T>(ref T field, T value)
	{
		bool changed;
		if (field is Color fieldColor && value is Color valueColor)
		{
			changed = fieldColor.ToArgb() != valueColor.ToArgb();
		}
		else
		{
			changed = !field?.Equals(value) ?? value != null;
		}
		field = value;
		if (changed)
		{
			TreeView?.NotifyRenderRequired();
		}

		return changed;
	}

	public bool IsEditing { get; }

	public bool IsSelected
	{
		get => isSelected;
		private set
		{
			UpdateProperty(ref isSelected, value);
		}
	}
	bool isSelected;

	public void Refresh()
	{
		TreeView?.NotifyToUpdate();
	}

	public void EndEdit(bool cancel)
	{
	}

	public bool IsExpanded
	{
		get => GetNodeCount(false) > 0 && isExpanded;
		private set
		{
			UpdateProperty(ref isExpanded, value);
		}
	}
	bool isExpanded;

	public void Expand()
	{
		if (GetNodeCount(false) > 0)
		{
			TreeView?.ExpandNode(this);
		}
	}

	internal void ExpandCore()
	{
		// restore the expanded state of child nodes if this node is being expanded now
		if (!IsExpanded)
		{
			foreach (var node in Nodes)
			{
				if (expandedState.TryGetValue(node.TreeNodeKey, out var isExpanded) && isExpanded)
				{
					node.ExpandCore();
				}
			}
		}

		IsExpanded = true;
	}

	public void ExpandAll()
	{
		Expand();
		foreach (var node in Nodes)
		{
			node.ExpandAll();
		}
	}

	public void Toggle()
	{
		if (IsExpanded)
		{
			Collapse();
		}
		else
		{
			Expand();
		}
	}

	public void Collapse()
	{
		TreeView?.CollapseNode(this);
	}

	/// <summary>
	/// The expanded state of child nodes before this node is collapsed.
	/// Used to restore the expanded state of child nodes when this node is expanded again.
	/// </summary>
	readonly Dictionary<Guid, bool> expandedState = new();

	internal void CollapseCore()
	{
		// store expanded state for child nodes before collapsing them.
		foreach (var node in Nodes)
		{
			expandedState[node.TreeNodeKey] = node.IsExpanded;
			if (node.IsExpanded)
			{
				node.CollapseCore();
			}
		}

		IsExpanded = false;
	}

	public void CollapseAll()
	{
		Collapse();
		foreach (var node in Nodes)
		{
			node.IsExpanded = false;
			node.Collapse();
		}
	}

	internal void SelectCore()
	{
		IsSelected = true;
	}

	internal void DeselectCore()
	{
		IsSelected = false;
	}

	public void EnsureVisible()
	{
	}

	public void Remove()
	{
		Remove(notify: true);
	}

	internal void Remove(bool notify)
	{
		foreach (var node in Nodes)
		{
			node.Remove(notify: false);
		}

		if (notify)
		{
			var parentNodes = GetParentNodes();
			if (parentNodes is not null && parentNodes.Count > 0)
			{
				parentNodes.Remove(this);
				parent = null;
			}
		}

		if (TreeView == null || TreeView.IsDisposed)
		{
			return;
		}
		treeView = null;
	}

	public virtual object Clone()
	{
		var clone = new TreeNode(TreeView)
		{
			Text = Text,
			Name = Name,
			Tag = Tag,
			ImageIndex = ImageIndex,
			SelectedImageIndex = SelectedImageIndex,
			ForeColor = ForeColor,
			NodeFont = NodeFont,
			Checked = Checked,
			IsExpanded = IsExpanded,
			Parent = Parent
		};

		foreach (var node in Nodes)
		{
			clone.Nodes.Add((TreeNode)node.Clone());
		}

		return clone;
	}

	/// <summary>
	///  Check for any circular reference in the ancestors chain.
	/// </summary>
	internal void CheckParentingCycle(TreeNode candidateToAdd)
	{
		TreeNode node = this;

		while (node != null)
		{
			if (node == candidateToAdd)
			{
				throw new ArgumentException(SR.TreeNodeCircularReference);
			}
			node = node.parent;
		}
	}

	/// <summary>
	///  Makes sure there is enough room to add n children
	/// </summary>
	internal void EnsureCapacity(int num)
	{
		int size = num;
		if (size < 4)
		{
			size = 4;
		}
		if (children == null)
		{
			children = new TreeNode[size];
		}
		else if (childCount + num > children.Length)
		{
			int newSize = childCount + num;
			if (num == 1)
			{
				newSize = childCount * 2;
			}
			TreeNode[] bigger = new TreeNode[newSize];
			System.Array.Copy(children, 0, bigger, 0, childCount);
			children = bigger;
		}
	}

	/// <summary>
	///  Called by the tree node collection to clear all nodes.  We optimize here if
	///  this is the root node.
	/// </summary>
	internal void Clear()
	{
		// This is a node that is a child of some other node.  We have
		// to selectively remove children here.
		//
		bool isBulkOperation = false;
		TreeView tv = TreeView;

		try
		{
			if (tv != null)
			{
				tv.nodesCollectionClear = true;

				if (tv != null && childCount > MAX_TREENODES_OPS)
				{
					isBulkOperation = true;
					tv.BeginUpdate();
				}
			}

			while (childCount > 0)
			{
				Nodes[childCount - 1].Remove(true);
			}
			children = null;

			if (tv != null && isBulkOperation)
			{
				tv.EndUpdate();
			}
		}
		finally
		{
			if (tv != null)
			{
				tv.nodesCollectionClear = false;
			}
			nodesCleared = true;
		}
	}

	internal void SortChildren(TreeView parentTreeView)
	{
		var children = Nodes;
		var childCount = children.Count;

		if (childCount > 0 && parentTreeView is not null)
		{
			TreeNode[] newOrder = new TreeNode[childCount];
			IComparer sorter = parentTreeView.TreeViewNodeSorter ?? parentTreeView.DefaultSorter;
			for (int i = 0; i < childCount; i++)
			{
				int min = -1;
				for (int j = 0; j < childCount; j++)
				{
					if (children[j] == null)
					{
						continue;
					}

					if (min == -1)
					{
						min = j;
						continue;
					}
					if (sorter.Compare(children[j] /*previous*/, children[min] /*current*/) <= 0)
					{
						min = j;
					}
				}

				newOrder[i] = children[min];
				children[min] = null;
				newOrder[i].index = i;
				newOrder[i].SortChildren(parentTreeView);
			}
			nodes.ReplaceAll(newOrder);
		}
	}

	internal void OnBeforeRender()
	{
		Status = SummaryStatus;
		NodeTokens = SummaryTokens;

		foreach (var node in Nodes)
		{
			node.OnBeforeRender();
		}
	}
}
