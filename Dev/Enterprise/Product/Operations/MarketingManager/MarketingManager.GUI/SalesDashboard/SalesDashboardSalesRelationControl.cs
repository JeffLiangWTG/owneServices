using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesDashboardSalesRelationControl : SalesRelationControl
	{
		public SalesDashboardSalesRelationControl()
		{
			InitializeComponent();
			mainPanel.Panel2Collapsed = false;
			this.Tree.ItemDrag += new ItemDragEventHandler(this.TreeViewAdv_DragEvent);
			this.Tree.KeyUp += this.Tree_KeyUpSave;

			Tree.AllowOutsideOfParent();
			Tree.AllowOverlap(communicationCheckBoxPanel);
		}

		public int SplitterDistance
		{
			get { return mainPanel.SplitterDistance; }
			set { mainPanel.SplitterDistance = value; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				var newSalesRelationModel = CurrentDataItem as SalesRelationModel;
				SelectTreeNode(newSalesRelationModel.Master);
			}
		}

		protected override void SelectTreeNode(IBusiness bizObjToSelect)
		{
			activityNoteRichTextBox.DataBindings.Clear();
			#if !WINZOR
			activityNoteRichTextBox.Rtf = "";
			#else
			activityNoteRichTextBox.Html = "";
			#endif

			var activity = bizObjToSelect as ISalesRelationActivity;
			if (activity != null && !activity.ActivityNotePropertyName.IsEmpty)
			{
				activityNoteRichTextBox.SetDataBinding(activity, activity.ActivityNotePropertyName);
			}
		}

		protected override bool AllowShowEditFormForMasterActivity => true;

		protected override ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new SalesDashboardSalesRelationTree();
		}

		#region buttons
		protected override void SetupButtons()
		{
			base.SetupButtons();
			this.DetachToolStripButton.Click += this.detachButton_ClickForSave;
		}

		void detachButton_ClickForSave(object sender, EventArgs e)
		{
			Save();
		}

		void Tree_KeyUpSave(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				Save();
			}
		}

		protected void Save()
		{
			var bizObj = this.DataSource as BusinessObject;
			List<ITransactionParticipant> factories = new List<ITransactionParticipant> { bizObj.Factory };
			BusinessObjectFactory.SaveTogether(factories.ToArray());
		}

		protected override EventHandler SaveDataHandler
		{
			get
			{
				return (object sender, EventArgs e) =>
				{
					Save();
				};
			}
		}

		void TreeViewAdv_DragEvent(object sender, ItemDragEventArgs e)
		{
			Save();
		}

		protected override string DialogMessageForDetachNode
		{
			get
			{
				return ResString.GetMultilingualString("75A16258-B592-4C7E-9746-2674F57C173A", "This action will be saved immediately.", NameOfTreeElementsPlural.Caption);
			}
		}
		#endregion
	}
}
