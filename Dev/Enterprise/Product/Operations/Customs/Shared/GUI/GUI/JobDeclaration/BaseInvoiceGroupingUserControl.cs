using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

//ToDo: TreeViewControl disappears

namespace Enterprise.Customs.GUI
{
	public partial class BaseInvoiceGroupingUserControl : ZUserControl
	{
		public BaseInvoiceGroupingUserControl()
		{
			InitializeComponent();
			CoveringLabel.Visible = false;
			if (!DesignModeFinder.IsDesigning)
			{
				GroupChargeGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
			}
		}

		protected virtual string ColumnTitleForGSTApplies
		{
			get { return BaseCustomsSupplierHeaderUserControl.IsGSTApplicableCaption; }
		}

		public void RemoveContextMenu()
		{
			baseTreeViewUserControl1.RemoveContextMenu();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (baseTreeViewUserControl1 != null && baseTreeViewUserControl1.TreeView != null)
				{
					baseTreeViewUserControl1.TreeView.OnSelectedNodeChanged -= new InvoiceTreeView.SelectedNodeChangedEventHandler(TreeView_OnSelectedNodeChanged);
				}
			}
			base.Dispose(disposing);
		}

		void baseTreeViewUserControl1_Load(object sender, EventArgs e)
		{
			baseTreeViewUserControl1.TreeView.OnSelectedNodeChanged += new InvoiceTreeView.SelectedNodeChangedEventHandler(TreeView_OnSelectedNodeChanged);
		}

		void TreeView_OnSelectedNodeChanged(bool isGroupInvoiceSelected)
		{
			if (isGroupInvoiceSelected)
			{
				CoveringLabel.Visible = false;
				GroupChargeGrid.Visible = true;
			}
			else
			{
				BringCoveringLabelToFront();
				CoveringLabel.Visible = true;
				GroupChargeGrid.Visible = false;
			}
		}

#if DEBUG

		internal bool coveringLabelBroughtToFrontTestingOnly;

#endif
		void BringCoveringLabelToFront()
		{
#if DEBUG
			coveringLabelBroughtToFrontTestingOnly = true;
#endif
			CoveringLabel.BringToFront();
		}
	}
}
