using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DynamicWorkOrderEntryForm
	{
		#region Auto

		protected DynamicWorkOrderEntryUserControl DynamicWorkOrderEntryControl;
		private CargoWise.Windows.UI.KPanel PickAndFinalizePanel;
		protected ZButton FinalizeButton;
		protected ZButton PickButton;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 608, true);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 581, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 24, true);
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
			this.BindingSource.DataSourceType = typeof(WhsDynamicWorkOrder);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// DynamicWorkOrderEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 664, true);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsWorkOrder";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 721, true);
			this.Name = "DynamicWorkOrderEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Dynamic Work Order Entry";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.PickAndFinalizePanel = new CargoWise.Windows.UI.KPanel();
			this.FinalizeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PickButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DynamicWorkOrderEntryControl = new DynamicWorkOrderEntryUserControl();
			this.MainTabPage.SuspendLayout();
			this.PickAndFinalizePanel.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DynamicWorkOrderEntryControl);
			this.MainTabPage.Controls.Add(this.PickAndFinalizePanel);
			// 
			// PickAndFinalizePanel
			// 
			this.PickAndFinalizePanel.Controls.Add(this.FinalizeButton);
			this.PickAndFinalizePanel.Controls.Add(this.PickButton);
			this.PickAndFinalizePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PickAndFinalizePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 547);
			this.PickAndFinalizePanel.Name = "PickAndFinalizePanel";
			this.PickAndFinalizePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 34);
			this.PickAndFinalizePanel.TabIndex = 2;
			// 
			// FinalizeButton
			// 
			this.FinalizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FinalizeButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryForm|81a910e1-7310-4cf5-88d4-ac88514f1fb2", "Finalize");
			this.FinalizeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 5);
			this.FinalizeButton.Name = "FinalizeButton";
			this.FinalizeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			this.FinalizeButton.TabIndex = 4;
			this.FinalizeButton.Click += new System.EventHandler(this.FinalizeButton_Click);
			// 
			// PickButton
			// 
			this.PickButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PickButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryForm|3a223e7e-da8e-4cbe-87ce-a22743353e95", "Pick");
			this.PickButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 5);
			this.PickButton.Name = "PickButton";
			this.PickButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23);
			this.PickButton.TabIndex = 3;
			this.PickButton.Click += new System.EventHandler(this.PickButton_Click);
			// 
			// WorkOrderEntryControl
			// 
			this.BindingSource.SetBindingMember(this.DynamicWorkOrderEntryControl, ".");
			this.DynamicWorkOrderEntryControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicWorkOrderEntryControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.DynamicWorkOrderEntryControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 450);
			this.DynamicWorkOrderEntryControl.Name = "DynamicWorkOrderEntryControl";
			this.DynamicWorkOrderEntryControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 547);
			this.DynamicWorkOrderEntryControl.TabIndex = 0;
			this.PickAndFinalizePanel.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(true);
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		#endregion

		#endregion
	}
}
