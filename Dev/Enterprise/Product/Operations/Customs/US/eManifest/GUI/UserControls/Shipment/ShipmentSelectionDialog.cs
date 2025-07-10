using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class ShipmentItemSelectionDialog : ZChildForm
	{
		public ShipmentItemSelectionDialog(Trip trip, IEnumerable<Shipment> items)
		{
			InitializeComponent();

			BuildTreeView(trip.BH_JobReference, items);
			this.ShipmentTreeView.AfterCheck += ShimentTreeView_AfterCheck;
			SelectAll();
		}

		void SelectAll()
		{
			CheckNodes(ShipmentTreeView.Nodes, true);
		}

		void CheckNodes(TreeNodeCollection nodes, bool isChecked)
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
		public string SeletedItem
		{
			get
			{
				var result = ZString.Empty;

				var totalNodes = ShipmentTreeView.Nodes[0].Nodes.OfType<ZBusinessObjectTreeNode>().Count();
				var count = ShipmentTreeView.Nodes[0].Nodes.OfType<ZBusinessObjectTreeNode>().Count(n => n.Checked);
				result = string.Format(Culture.Invariant, "{0} of {1} Shipment(s) selected.", count, totalNodes);

				return result;
			}
		}

		void BuildTreeView(string manifestNumber, IEnumerable<Shipment> items)
		{
			var root = new TreeNode(FormattableString.Invariant($"Trip(eManifest) - {manifestNumber}"));
			var orderedItems = items.Select(item => new KeyValuePair<string, Shipment>(item.SelectionDescription, item)).OrderBy(x => x.Key);
			foreach (KeyValuePair<string, Shipment> pair in orderedItems)
			{
				root.Nodes.Add(new ZBusinessObjectTreeNode(pair.Value, pair.Key));
			}
			ShipmentTreeView.Nodes.Add(root);
			root.ExpandAll();
		}

		void ShimentTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			CheckNodes(e.Node.Nodes, e.Node.Checked);
			SelectedItemCountLabel.Text = SeletedItem;
			SelectedItemCountLabel.Update();
		}
		public IEnumerable<Shipment> GetSelectedItems()
		{
			return ShipmentTreeView.Nodes[0].Nodes.OfType<ZBusinessObjectTreeNode>()
				.Where(n => n.Checked)
				.Select(n => (Shipment)n.BizO)
				.ToArray();
		}

		public void DeSelectAll()
		{
			CheckNodes(ShipmentTreeView.Nodes, false);
		}

		void ButtonOK_Click(object sender, EventArgs e)
		{
			var count = ShipmentTreeView.Nodes[0].Nodes.OfType<ZBusinessObjectTreeNode>().Count(n => n.Checked);
			if (count > 0)
			{
				if (count == 1 || Globals.Message.Show(string.Format(Culture.Invariant, "You have selected to create {0} Declarations. Would you like to proceed?", count), "Create Declarations", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					Close();
					DialogResult = DialogResult.OK;
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("{A34B3054-6F16-4FF2-974B-425ED1AB5B9D}", "Cannot send as nothing has been selected."));
			}
		}

		void ButtonCancel_Click(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}

		internal ZTreeViewNoDoubleClick ShipmentTreeViewInternal => ShipmentTreeView;
	}
}
