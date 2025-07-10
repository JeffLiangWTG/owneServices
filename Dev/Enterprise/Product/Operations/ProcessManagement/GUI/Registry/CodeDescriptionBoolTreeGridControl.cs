using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class CodeDescriptionBoolTreeGridControl : CodeDescriptionBoolControl
	{
		public CodeDescriptionBoolTreeGridControl()
		{
			InitializeComponent();
			CodeDescriptionBoolGrid.AllowSorting = true;
		}

		public void SetLevel(CodeDescriptionBoolTreeGridControl parent, CodeDescriptionBoolTreeGridControl child)
		{
			this.childGrid = child;
			this.parentGrid = parent;
		}

		protected CodeDescriptionBoolTreeGridControl parentGrid;
		protected CodeDescriptionBoolTreeGridControl childGrid;

		public bool CodeDescriptionBoolGridReadOnly
		{
			get { return CodeDescriptionBoolGrid.ReadOnly; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var allNodes = (CodeDescriptionBoolTreeNodeCollection)dataSource;
			if (allNodes != null)
			{
				var parentView = parentGrid != null ? parentGrid.BindingSource.DataSource as CodeDescriptionBoolTreeView : null;
				dataSource = new CodeDescriptionBoolTreeView(allNodes, parentView == null || parentView.Count == 0 ? ZGuid.Empty : parentView[0].ID);
			}
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null && childGrid != null)
			{
				BindingSource.BindingContext[dataSource].CurrentChanged += Grid_CurrentChanged;
			}
		}

		void Grid_CurrentChanged(object sender, EventArgs e)
		{
			BindingManagerBase bm = (BindingManagerBase)sender;
			var childView = childGrid.BindingSource.DataSource as CodeDescriptionBoolTreeView;
			if (bm.Position >= 0 && bm.Position < bm.Count)
			{
				if (CodeDescriptionBoolGrid.CurrentRowIndex != bm.Position)
				{
					return;
				}

				var currentNode = bm.GetCurrent() as CodeDescriptionBoolTreeNode;
				if (currentNode != null)
				{
					childView.ParentID = currentNode.ID;
				}
				else
				{
					childView.ParentID = ZGuid.Missing;
				}
			}
			else
			{
				childView.ParentID = ZGuid.Missing;
			}
		}
	}
}
