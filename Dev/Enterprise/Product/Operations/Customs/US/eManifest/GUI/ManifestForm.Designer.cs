namespace Enterprise.Customs.US.eManifest.GUI
{
	using Enterprise.MasterFiles.GUI;
	using Enterprise.ZArchitecture.GUI;

	partial class ManifestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ManifestUserControl = new Enterprise.Customs.US.eManifest.GUI.ManifestUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.StmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestUserControl.TabControl.SuspendLayout();
			this.ManifestUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.ManifestUserControl);
			
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// ManifestUserControl
			// 
			this.BindingSource.SetBindingMember(this.ManifestUserControl, ".");
			this.ManifestUserControl.CaptionResourceString = null;
			this.ManifestUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true); 
			this.ManifestUserControl.Name = "ManifestUserControl";
			this.ManifestUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 486, true);
			// 
			// ManifestUserControl.TabControl
			// 
			this.ManifestUserControl.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ManifestUserControl.TabControl.Controls.Add(this.WorkflowTabPage);
			this.ManifestUserControl.TabControl.Controls.Add(this.StmNoteTabPage);
			this.ManifestUserControl.TabControl.Controls.Add(this.LogsTabPage);
			this.ManifestUserControl.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestUserControl.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestUserControl.TabControl.Name = "TabControl";
			this.ManifestUserControl.TabControl.SelectedIndex = 0;
			this.ManifestUserControl.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 486, true);
			this.ManifestUserControl.TabControl.TabIndex = 2;
			this.ManifestUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 459, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 459, true);
			this.StmNoteTabPage.TabIndex = 5;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 459, true);
			this.LogsTabPage.TabIndex = 6;
			// 
			// ManifestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 542, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);

			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1072, 725, true);
			this.Name = "ManifestForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestUserControl.TabControl.ResumeLayout(false);
			this.ManifestUserControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private ManifestUserControl ManifestUserControl;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZStmNoteTabPage StmNoteTabPage;
		private ZLogsTabPage LogsTabPage;

		#endregion
	}
}
