using System.Collections;
using WinzorFramework;

namespace System.Windows.Forms;

public class TreeNodeCollection : WrappedList<TreeNode>
{
	readonly TreeNode owner;

	internal TreeNodeCollection(TreeNode owner)
	{
		this.owner = owner;
	}

	//this index is used to optimize performance of AddRange
	//items are added from last to first after this index
	//(to work around TV_INSertItem comctl32 perf issue with consecutive adds in the end of the list)
	int fixedIndex = -1;

	internal int FixedIndex
	{
		get
		{
			return fixedIndex;
		}
		set
		{
			fixedIndex = value;
		}
	}

	public TreeNode[] Find(string key, bool searchAllChildren)
	{
		var matches = this.Where(t => t.Name == key);
		if (searchAllChildren)
		{
			matches = matches.Concat(this.SelectMany(t => t.Nodes.Find(key, searchAllChildren)));
		}
		return matches.ToArray();
	}

	public virtual TreeNode? this[string key] => this.Find(key, searchAllChildren: false).FirstOrDefault();

	public virtual bool ContainsKey(string key) => this[key] != null;

	public int AddSorted(TreeNode node)
	{
		int index = 0;
		int iMin, iLim, iT;

		if (Count > 0)
		{
			var treeView = owner.TreeView;
			var sorter = treeView?.TreeViewNodeSorter ?? treeView?.DefaultSorter;

			if (sorter != null)
			{
				for (iMin = 0, iLim = Count; iMin < iLim;)
				{
					iT = (iMin + iLim) / 2;
					if (sorter.Compare(this[iT], node) <= 0)
					{
						iMin = iT + 1;
					}
					else
					{
						iLim = iT;
					}
				}
				index = iMin;
			}
		}

		node.SortChildren(owner.TreeView);
		base.Insert(index, node);

		return index;
	}

	public override void Add(TreeNode node)
	{
		node.Parent = owner;

		if (owner.TreeView is TreeView t && t.Sorted)
		{
			AddSorted(node);
		}
		else
		{
			base.Add(node);
		}
	}

	public virtual TreeNode Add(string text)
	{
		var node = new TreeNode(text);
		Add(node);
		return node;
	}

	public virtual TreeNode Add(string key, string text)
	{
		var node = new TreeNode(text) { Name = key };
		Add(node);
		return node;
	}

	int AddInternal(TreeNode node, int delta)
	{
		if (node == null)
		{
			throw new ArgumentNullException(nameof(node));
		}
		if (node.handle != IntPtr.Zero)
		{
			throw new ArgumentException(string.Format(SR.OnlyOneControl, node.Text), nameof(node));
		}

		// Check for ParentingCycle
		owner.CheckParentingCycle(node);

		// If the TreeView is sorted, index is ignored
		TreeView tv = owner.TreeView;
		if (tv != null && tv.Sorted)
		{
			return AddSorted(node);
		}
		node.Parent = owner;
		int fixedIndex = owner.Nodes.FixedIndex;
		if (fixedIndex != -1)
		{
			node.index = fixedIndex + delta;
		}
		else
		{
			//if fixedIndex != -1 capacity was ensured by AddRange
			owner.EnsureCapacity(1);
			node.index = owner.GetNodeCount(false);
		}
		while (node.index >= Count)
		{
			inner = inner.Add(null!);
		}
		this[node.index] = node;

		if (tv != null && node == tv.SelectedNode)
		{
			tv.SelectedNode = node; // communicate this to the handle
		}

		if (tv != null && tv.TreeViewNodeSorter != null)
		{
			tv.Sort();
		}

		return node.index;
	}

	public virtual void AddRange(TreeNode[] nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException(nameof(nodes));
		}
		if (nodes.Length == 0)
		{
			return;
		}

		TreeView tv = owner.TreeView;
		if (tv != null && nodes.Length > TreeNode.MAX_TREENODES_OPS)
		{
			tv.BeginUpdate();
		}
		owner.Nodes.FixedIndex = owner.GetNodeCount(false);
		owner.EnsureCapacity(nodes.Length);
		for (int i = nodes.Length - 1; i >= 0; i--)
		{
			AddInternal(nodes[i], i);
		}
		owner.Nodes.FixedIndex = -1;
		if (tv != null && nodes.Length > TreeNode.MAX_TREENODES_OPS)
		{
			tv.EndUpdate();
		}
	}

	public new void Insert(int index, TreeNode node)
	{
		node.Parent = owner;

		if (owner.TreeView is TreeView t && t.Sorted)
		{
			AddSorted(node);
		}
		else
		{
			base.Insert(index, node);
		}
	}

	public TreeNode Insert(int index, string text)
	{
		var node = new TreeNode(text);
		Insert(index, node);
		return node;
	}

	protected override void OnAdd(TreeNode node, int index)
	{
		node.Parent = owner;
		owner.TreeView?.NotifyToUpdate();
	}
	protected override void OnRemove(TreeNode node)
	{
		node.Parent = null;
		owner.TreeView?.NotifyToUpdate();
	}

	internal void ReplaceAll(TreeNode[] nodes)
	{
		inner = inner.Clear();
		inner = inner.AddRange(nodes);
	}

	/// <summary>
	///  Remove all nodes from the tree view.
	/// </summary>
	public override void Clear()
	{
		base.Clear();
		owner.Clear();
	}
}
