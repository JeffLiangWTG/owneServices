using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderEntryForm
	{
#if DEBUG
		public
#endif
			ZTemplateTabControl TabControl;

		private ZTabPage EntryTabPage;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		private ZButton ReleaseButton;
		private ZButton PickButton;
		private OrderEntryUserControl OrderEntryUserControl;
		private ZWorkflowTabPage WorkflowTabPage;
		private KPanel bottomPanel1;
		private ZButton EditLineButton;

		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.TabControl = new ZTemplateTabControl();
			this.EntryTabPage = new ZTabPage();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 776, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(498);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(499);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsOrder);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.EntryTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 750, true);
			this.TabControl.TabIndex = 0;
			// 
			// EntryTabPage
			// 
			this.EntryTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryForm|4ff347ad-bab2-4c83-a0e3-07609d396152", "Entry");
			this.EntryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryTabPage.Name = "EntryTabPage";
			this.EntryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 607, true);
			this.EntryTabPage.TabIndex = 0;
			this.EntryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.EntryTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 607, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 607, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 607, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(737, 751, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 25, true);
			this.PostingButtonsUserControl.TabIndex = 5;
			// 
			// OrderEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 800, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsOrder);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsOrder";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 725, true);
			this.Name = "OrderEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
