namespace Enterprise.MasterFiles.GUI.Workflow
{
	partial class WorkflowDiagnosticForm
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
			this.workflowDiagnosticUserControl1 = new Enterprise.MasterFiles.GUI.Workflow.WorkflowDiagnosticUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.workflowDiagnosticUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 709, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.Workflow.WorkflowDiagnosticViewModel);
			// 
			// workflowDiagnosticUserControl1
			// 
			this.workflowDiagnosticUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workflowDiagnosticUserControl1, ".");
			this.workflowDiagnosticUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowDiagnosticUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.workflowDiagnosticUserControl1.Name = "workflowDiagnosticUserControl1";
			this.workflowDiagnosticUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 709, true);
			this.workflowDiagnosticUserControl1.TabIndex = 1;
			// 
			// WorkflowDiagnosticForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 733, true);
			this.Controls.Add(this.workflowDiagnosticUserControl1);
			this.DataSourceType = typeof(Enterprise.MasterFiles.GUI.Workflow.WorkflowDiagnosticViewModel);
			this.Name = "WorkflowDiagnosticForm";
			this.Text = "WorkflowDiagnosticForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.workflowDiagnosticUserControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.workflowDiagnosticUserControl1.ResumeLayout(true);
			this.workflowDiagnosticUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WorkflowDiagnosticUserControl workflowDiagnosticUserControl1;
	}
}
