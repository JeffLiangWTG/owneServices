namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProjectAdditionalDetailsControl = new Enterprise.ProcessManagement.GUI.ProjectAdditionalDetailsControl();
			this.RelatedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProjectDetailsControl = new Enterprise.ProcessManagement.GUI.ProjectDetailsControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WorkflowTabPage.SuspendLayout();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.ProjectAdditionalDetailsControl.SuspendLayout();
			this.ProjectDetailsControl.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.RelatedItemsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 638, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ProjectDetailsControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 638, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("c88ac34c-4beb-49f0-aa18-284e0cdeb94d", "Additional Details");
			this.AdditionalDetailsTabPage.Controls.Add(this.ProjectAdditionalDetailsControl);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			this.AdditionalDetailsTabPage.TabIndex = 4;
			// 
			// ProjectAdditionalDetailsControl
			// 
			this.ProjectAdditionalDetailsControl.AllowDrop = true;
			this.ProjectAdditionalDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectAdditionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectAdditionalDetailsControl.Name = "ProjectAdditionalDetailsControl";
			this.ProjectAdditionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			this.ProjectAdditionalDetailsControl.TabIndex = 0;
			// 
			// ProjectDetailsControl
			// 
			this.ProjectDetailsControl.AllowDrop = true;
			this.ProjectDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectDetailsControl.Name = "ProjectDetailsControl";
			this.ProjectDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 617, true);
			this.ProjectDetailsControl.TabIndex = 0;
			// 
			// ProjectForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 694, true);
			this.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 725, true);
			this.Name = "ProjectForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.ProjectAdditionalDetailsControl.ResumeLayout(true);
			this.ProjectAdditionalDetailsControl.PerformLayout();
			this.ProjectDetailsControl.ResumeLayout(true);
			this.ProjectDetailsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZTabPage AdditionalDetailsTabPage;
		protected ProjectDetailsControl ProjectDetailsControl;
		private ProjectAdditionalDetailsControl ProjectAdditionalDetailsControl;
	}
}