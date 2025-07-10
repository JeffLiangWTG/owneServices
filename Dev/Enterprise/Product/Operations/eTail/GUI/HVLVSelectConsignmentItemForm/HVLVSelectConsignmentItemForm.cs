using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public abstract partial class HVLVSelectConsignmentItemForm : ZChildForm
	{
		protected HVLVSelectConsignmentItemForm(HVLVConsignment consignment)
		{
			InitializeComponent();
			BuildTreeView(consignment);
			ConsignmentTreeView.AfterCheck += ConsignmentsTreeView_AfterCheck;
		}

		#region Build Tree View

		void BuildTreeView(HVLVConsignment consignment)
		{
			var root = BuildTreeNodes(consignment);
			ConsignmentTreeView.Nodes.Add(root);
			root.ExpandAll();
		}

		protected abstract TreeNode BuildTreeNodes(HVLVConsignment consignment);

		#endregion

		#region Select Nodes

		void ConsignmentsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			CheckChildNodes(e.Node.Nodes, e.Node.Checked);
		}

		void CheckChildNodes(TreeNodeCollection nodes, bool isChecked)
		{
			foreach (TreeNode rootNode in nodes)
			{
				rootNode.Checked = isChecked;

				foreach (TreeNode subNode in rootNode.Nodes)
				{
					subNode.Checked = isChecked;
				}
			}
		}

		int TotalSelectedItems
		{
			get
			{
				return ConsignmentTreeView.Nodes[0].Nodes
					.OfType<ZBusinessObjectTreeNode>()
					.Count(c => c.Checked);
			}
		}

		public HVLVItem[] SelectedItems
		{
			get
			{
				return ConsignmentTreeView.Nodes[0].Nodes
					.OfType<ZBusinessObjectTreeNode>()
					.Where(c => c.Checked).Select(n => (HVLVItem)n.BizO)
					.ToArray();
			}
		}

		#endregion

		#region Buttons

		void ButtonClick_OK(object sender, EventArgs e)
		{
			var caption = ResString.GetMultilingualString("14f36ef6-f4b1-46cf-9daa-b8881d03e033", "Selected HVLV Items");
			var message = ResString.GetMultilingualString("b2656b68-6ff9-42ef-9022-9b94a8a1148d", "You have selected {0} HVLV Item(s). Do you wish to proceed?", TotalSelectedItems);

			if (TotalSelectedItems == 0)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("209f32c2-1573-4010-8671-813e43f7db6b", "No HVLV Items were selected."));
			}
			else if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
			{
				Close();
				DialogResult = DialogResult.OK;
			}
		}

		void ButtonClick_SelectAll(object sender, EventArgs e)
		{
			CheckChildNodes(ConsignmentTreeView.Nodes, true);
		}

		void ButtonClick_DeselectAll(object sender, EventArgs e)
		{
			CheckChildNodes(ConsignmentTreeView.Nodes, false);
		}

		#endregion
	}
}
