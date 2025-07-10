namespace Enterprise.ProcessManagement.GUI
{
	partial class JiraProjectSelectorForm
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
		protected new void InitializeComponent()
		{
			this.JiraProjectSelectorControl = new Enterprise.ProcessManagement.GUI.JiraProjectSelectorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JiraProjectSelectorControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 546, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.ProjectsToImportViewModel);
			// 
			// JiraProjectSelectorControl
			// 
			this.JiraProjectSelectorControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JiraProjectSelectorControl, ".");
			this.JiraProjectSelectorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JiraProjectSelectorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JiraProjectSelectorControl.Name = "JiraProjectSelectorControl";
			this.JiraProjectSelectorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 546, true);
			this.JiraProjectSelectorControl.TabIndex = 1;
			// 
			// JiraProjectSelectorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("b7db30b3-aee8-4a71-ab94-da3ae7f1b23e", "Import Jira Projects");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 570, true);
			this.Controls.Add(this.JiraProjectSelectorControl);
			this.DataSourceType = typeof(Enterprise.ProcessManagement.Business.ProjectsToImportViewModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 330, true);
			this.Name = "JiraProjectSelectorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.JiraProjectSelectorControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JiraProjectSelectorControl.ResumeLayout(true);
			this.JiraProjectSelectorControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private JiraProjectSelectorUserControl JiraProjectSelectorControl;
	}
}