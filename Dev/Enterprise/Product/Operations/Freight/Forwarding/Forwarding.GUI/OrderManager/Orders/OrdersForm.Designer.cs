using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	partial class OrdersForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage OrdersTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage NotesTabPage;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl OrderTabControl;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zEventTabPage1;
		private ZWorkflowTabPage WorkflowTabPage;

		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.OrdersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrdersUserControl = new Enterprise.Freight.Forwarding.Orders.GUI.OrdersUserControlDecider();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.OrderTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.zEventTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.OrderTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 621, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1001);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.Order);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 611, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersForm|5278b102-e4af-4a54-a38b-31def5038e5b", "Order");
			this.OrdersTabPage.Controls.Add(this.OrdersUserControl);
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Name = "OrdersTabPage";
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 560, true);
			this.OrdersTabPage.TabIndex = 0;
			// 
			// OrdersUserControl
			// 
			this.OrdersUserControl.AllowDrop = true;
			this.OrdersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersUserControl.Name = "OrdersUserControl";
			this.OrdersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 560, true);
			this.OrdersUserControl.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 560, true);
			this.NotesTabPage.TabIndex = 4;
			// 
			// OrderTabControl
			// 
			this.OrderTabControl.Controls.Add(this.OrdersTabPage);
			this.OrderTabControl.Controls.Add(this.WorkflowTabPage);
			this.OrderTabControl.Controls.Add(this.NotesTabPage);
			this.OrderTabControl.Controls.Add(this.zEventTabPage1);
			this.OrderTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 19, true);
			this.OrderTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderTabControl.Name = "OrderTabControl";
			this.OrderTabControl.SelectedIndex = 0;
			this.OrderTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			this.OrderTabControl.TabIndex = 1;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersForm|cd63fce5-965a-418c-b26e-2fd9efd91683", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 560, true);
			this.WorkflowTabPage.TabIndex = 7;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 581, true);
			this.zEventTabPage1.TabIndex = 5;
			// 
			// OrdersForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 660, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OrderTabControl);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.Order);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Orders.Business.Order";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 702, true);
			this.Name = "OrdersForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrderTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.OrderTabControl.ResumeLayout(false);
			this.OrderTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
