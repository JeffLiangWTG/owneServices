using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Packing.GUI
{
	/// <summary>
	/// Manages multi-selection of Nodes for a TreeView.
	/// </summary>
	public class MultiNodeSelector
	{
		public MultiNodeSelector(Color unselectedForeColour)
		{
			UnselectedForeColour = unselectedForeColour;
			SelectedNodesInternal = new List<PackingTreeNode>();
			SelectedNodesHash = new HashSet<PackingTreeNode>();
		}

		public readonly Color UnselectedForeColour;

		#region AfterSelectOrDeselect

		public event EventHandler AfterSelectOrDeselect;

		void FireAfterNodeSelectOrDeselect()
		{
			if (AfterSelectOrDeselect != null)
			{
				AfterSelectOrDeselect(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Selecting

		public void SelectNode(PackingTreeNode node)
		{
			Argument.NotNull(node, "node");

			if (!IsSelected(node))
			{
				SelectAndPaintNode(node);
				FireAfterNodeSelectOrDeselect();
			}
		}

		void SelectAndPaintNode(PackingTreeNode node)
		{
			SelectedNodesInternal.Add(node);
			SelectedNodesHash.Add(node);
			PaintNode(node);

			if (IsSingleNodeSelected)
			{
				FirstNodeInShiftSequence = node;
			}

			LastSelectedNode = node;
		}

		public void SelectNodes(IEnumerable<PackingTreeNode> nodes, int nodeCount)
		{
			Argument.NotNull(nodes, "nodes");

			if (SelectedNodesInternal.Capacity < nodeCount)
			{
				SelectedNodesInternal.Capacity = nodeCount + SelectedNodesInternal.Capacity;
			}

			foreach (var node in nodes)
			{
				if (!IsSelected(node))
				{
					SelectAndPaintNode(node);
				}
			}

			FireAfterNodeSelectOrDeselect();
		}

		public void DeselectNode(PackingTreeNode node, PackingTreeNode lastSelectedNode = null)
		{
			Argument.NotNull(node, "node");

			if (IsSelected(node))
			{
				SelectedNodesInternal.Remove(node);
				SelectedNodesHash.Remove(node);
				UnpaintNode(node);

				// make any previously-selected node the first / last selection. doesn't really matter which node it is
				if (FirstNodeInShiftSequence == node)
				{
					FirstNodeInShiftSequence = IsAnyNodeSelected ? SelectedNodesInternal[0] : null;
				}
				if (LastSelectedNode == node)
				{
					LastSelectedNode = IsAnyNodeSelected ? lastSelectedNode ?? SelectedNodesInternal[0] : null;
				}

				FireAfterNodeSelectOrDeselect();
			}
		}

		public void DeselectAllNodes()
		{
			UnpaintAllNodes();
			SelectedNodesInternal.Clear();
			SelectedNodesHash.Clear();
			LastSelectedNode = null;
			FirstNodeInShiftSequence = null;
		}

		public bool IsSelected(PackingTreeNode node)
		{
			return SelectedNodesHash.Contains(node);
		}

		#endregion

		#region Painting

		public void PaintNode(PackingTreeNode node)
		{
			Argument.NotNull(node, "node");

			node.ForeColor = SystemColors.HighlightText;
			node.BackColor = SystemColors.Highlight;
		}

		public void UnpaintNode(PackingTreeNode node)
		{
			Argument.NotNull(node, "node");

			node.ForeColor = UnselectedForeColour;
			node.BackColor = Color.Empty; // do not use a real colour (eg. white) -- it causes the selecting of nodes to flicker as .net will repaint with .empty
		}

		void UnpaintAllNodes()
		{
			foreach (var node in SelectedNodesInternal)
			{
				if (node != null)
				{
					UnpaintNode(node);
				}
			}
		}

		#endregion

		#region Count

		public int Count => SelectedNodesInternal.Count;

		#endregion

		#region Flags

		public bool IsSingleNodeSelected => Count == 1;

		public bool IsAnyNodeSelected => Count > 0;

		public bool IsAnyPackageSelected => SelectedNodesInternal.Any(node => node.IsPackage);

		public bool IsPackableItemSelected => SelectedNodesInternal.Any(node => node.IsPackedItem);

		#endregion

		#region SelectedNodes

		public ReadOnlyCollection<PackingTreeNode> SelectedNodes => SelectedNodesInternal.AsReadOnly();

		public PackingTreeNode LastSelectedNode
		{
			get;
			private set;
		}

		public PackingTreeNode FirstNodeInShiftSequence;

		readonly List<PackingTreeNode> SelectedNodesInternal;
		readonly HashSet<PackingTreeNode> SelectedNodesHash;

		#endregion
	}
}
