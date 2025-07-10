using System;
using System.Windows;

namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationResultsViewerForm
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
			this.ResultsViewerUserControl = new Enterprise.MasterData.GUI.DeduplicationResultsViewerDetailsUserControl();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 637, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			// 
			// ResultsViewerUserControl
			// 
			this.ResultsViewerUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsViewerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsViewerUserControl.Name = "ResultsViewerUserControl";
			this.ResultsViewerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 637, true);
			this.ResultsViewerUserControl.TabIndex = 3;
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.ResultsViewerUserControl);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 637, true);
			this.mainPanel.TabIndex = 2;
			// 
			// DeduplicationResultsViewerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 661, true);
			this.Controls.Add(this.mainPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1238, 700, true);
			this.Name = "DeduplicationResultsViewerForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

#endregion

		private ZArchitecture.GUI.ZPanel mainPanel;
		private Enterprise.MasterData.GUI.DeduplicationResultsViewerDetailsUserControl ResultsViewerUserControl;
	}
}
