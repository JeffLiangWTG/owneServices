using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerManagerForm
	{
		new void InitializeComponent()
		{
			this.movementsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.movementsControl = new Enterprise.Freight.Agency.GUI.ContainerManagerMovementsControl();
			this.detailsControl = new Enterprise.Freight.Agency.GUI.ContainerManagerDetailControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.movementsTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.movementsTab);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 546, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.movementsTab, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.detailsControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 530, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 530, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 546, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.RefContainerStock);
			// 
			// movementsTab
			// 
			this.movementsTab.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerForm|7d557d85-cb23-48b0-bf19-922dc2dd839a", "Movements");
			this.movementsTab.Controls.Add(this.movementsControl);
			this.movementsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.movementsTab.Name = "movementsTab";
			this.movementsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.movementsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 519, true);
			this.movementsTab.TabIndex = 3;
			this.movementsTab.UseVisualStyleBackColor = true;
			// 
			// movementsControl
			// 
			this.movementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.movementsControl, ".");
			this.movementsControl.CaptionResourceString = null;
			this.movementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.movementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.movementsControl.Name = "movementsControl";
			this.movementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 513, true);
			this.movementsControl.TabIndex = 0;
			// 
			// detailsControl
			// 
			this.detailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.detailsControl, ".");
			this.detailsControl.CaptionResourceString = null;
			this.detailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsControl.Name = "detailsControl";
			this.detailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 530, true);
			this.detailsControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerForm|1c62e297-4904-4e1c-8d3c-794ba81f3fae", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 530, true);
			this.WorkflowTabPage.TabIndex = 7;
			// 
			// ContainerManagerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 602, true);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.RefContainerStock);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 661, true);
			this.Name = "ContainerManagerForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "RefContainerStock";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.movementsTab.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZTabPage movementsTab;
		private ContainerManagerMovementsControl movementsControl;
		private ContainerManagerDetailControl detailsControl;
		private ZWorkflowTabPage WorkflowTabPage;
	}
}
