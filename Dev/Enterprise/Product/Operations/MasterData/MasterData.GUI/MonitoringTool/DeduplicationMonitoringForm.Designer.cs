using CargoWise.Windows.UI;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationMonitoringForm
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
			MonitoringObjects.Clear();
			DeduplicationUtils.DebuggerHubInstance.Clear();
			DeduplicationMonitoringUserControl?.Dispose();
			DeduplicationMonitoringUserControl = null;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.DeduplicationMonitoringUserControl = new Enterprise.MasterData.GUI.DeduplicationMonitoringUserControl();
			this.ToggleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterDropdownList = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.ButtonFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ExportDiagnosticsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeduplicationMonitoringUserControl.SuspendLayout();
			this.ButtonFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 423, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 24, true);
			// 
			// DeduplicationMonitoringUserControl
			// 
			this.DeduplicationMonitoringUserControl.AllowDrop = true;
			this.DeduplicationMonitoringUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeduplicationMonitoringUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.DeduplicationMonitoringUserControl.Name = "DeduplicationMonitoringUserControl";
			this.DeduplicationMonitoringUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 384, true);
			// 
			// ToggleButton
			// 
			this.ToggleButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.ToggleButton.ForeColor = System.Drawing.Color.White;
			this.ToggleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ToggleButton.Name = "ToggleButton";
			this.ToggleButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ToggleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 30, true);
			this.ToggleButton.TabIndex = 0;
			this.ToggleButton.Text = "Start Tracking Results";
			this.ToggleButton.ToolTipCaption = null;
			this.ToggleButton.UseVisualStyleBackColor = false;
			this.ToggleButton.Click += new System.EventHandler(this.ToggleButton_Click);
			// 
			// ButtonClear
			// 
			this.ButtonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonClear.BackColor = System.Drawing.Color.Salmon;
			this.ButtonClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 3, true);
			this.ButtonClear.Name = "ButtonClear";
			this.ButtonClear.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 30, true);
			this.ButtonClear.TabIndex = 5;
			this.ButtonClear.Text = "Clear";
			this.ButtonClear.ToolTipCaption = null;
			this.ButtonClear.UseVisualStyleBackColor = false;
			this.ButtonClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.ExportButton.ForeColor = System.Drawing.Color.White;
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 3, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 30, true);
			this.ExportButton.TabIndex = 4;
			this.ExportButton.Text = "Export Results";
			this.ExportButton.ToolTipCaption = null;
			this.ExportButton.UseVisualStyleBackColor = false;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.ImportButton.ForeColor = System.Drawing.Color.White;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 3, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 30, true);
			this.ImportButton.TabIndex = 3;
			this.ImportButton.Text = "Import Results";
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.UseVisualStyleBackColor = false;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// FilterButton
			// 
			this.FilterButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.FilterButton.ForeColor = System.Drawing.Color.White;
			this.FilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 4, true);
			this.FilterButton.Name = "FilterButton";
			this.FilterButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 30, true);
			this.FilterButton.TabIndex = 1;
			this.FilterButton.Text = "Filter Results";
			this.FilterButton.ToolTipCaption = null;
			this.FilterButton.UseVisualStyleBackColor = false;
			this.FilterButton.Click += new System.EventHandler(this.FilterButton_Click);
			// 
			// FilterDropdownList
			//
			this.FilterDropdownList.BindingItems = null;
			this.FilterDropdownList.CheckOnClick = true;
			this.FilterDropdownList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 34);
			this.FilterDropdownList.Name = "FilterDropdownList";
			this.FilterDropdownList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 56);
			this.FilterDropdownList.Visible = false;
			// 
			// ButtonFlowLayoutPanel
			// 
			this.ButtonFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonFlowLayoutPanel.Controls.Add(this.ButtonClear);
			this.ButtonFlowLayoutPanel.Controls.Add(this.ExportButton);
			this.ButtonFlowLayoutPanel.Controls.Add(this.ImportButton);
			this.ButtonFlowLayoutPanel.Controls.Add(this.ExportDiagnosticsButton);
			this.ButtonFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.ButtonFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 0, true);
			this.ButtonFlowLayoutPanel.Name = "ButtonFlowLayoutPanel";
			this.ButtonFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 38, true);
			// 
			// ExportDiagnosticsButton
			// 
			this.ExportDiagnosticsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportDiagnosticsButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.ExportDiagnosticsButton.ForeColor = System.Drawing.Color.White;
			this.ExportDiagnosticsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 3, true);
			this.ExportDiagnosticsButton.Name = "ExportDiagnosticsButton";
			this.ExportDiagnosticsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExportDiagnosticsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 30, true);
			this.ExportDiagnosticsButton.TabIndex = 2;
			this.ExportDiagnosticsButton.Text = "Export Diagnostics";
			this.ExportDiagnosticsButton.ToolTipCaption = null;
			this.ExportDiagnosticsButton.Enabled = false;
			this.ExportDiagnosticsButton.UseVisualStyleBackColor = false;
			this.ExportDiagnosticsButton.Click += new System.EventHandler(this.ExportDiagnosticsButton_Click);
			// 
			// DeduplicationMonitoringForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 447, true);
			this.Controls.Add(this.ButtonFlowLayoutPanel);
			this.Controls.Add(this.FilterButton);
			this.Controls.Add(this.ToggleButton);
			this.Controls.Add(this.DeduplicationMonitoringUserControl);
			this.Controls.Add(this.FilterDropdownList);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 447, true);
			this.Name = "DeduplicationMonitoringForm";
			this.Text = "Deduplication Results Monitoring";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.DeduplicationMonitoringUserControl, 0);
			this.Controls.SetChildIndex(this.ToggleButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FilterButton, 0);
			this.Controls.SetChildIndex(this.FilterDropdownList, 0);
			this.Controls.SetChildIndex(this.ButtonFlowLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeduplicationMonitoringUserControl.ResumeLayout(true);
			this.DeduplicationMonitoringUserControl.PerformLayout();
			this.ButtonFlowLayoutPanel.ResumeLayout(false);
			this.ButtonFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal DeduplicationMonitoringUserControl DeduplicationMonitoringUserControl;
		internal ZArchitecture.GUI.ZButton ToggleButton;
		private ZArchitecture.GUI.ZButton ButtonClear;
		private ZArchitecture.GUI.ZButton ExportButton;
		internal ZArchitecture.GUI.ZButton ImportButton;
		internal ZArchitecture.GUI.ZButton FilterButton;
		internal Enterprise.ZArchitecture.GUI.ZCheckedListBox FilterDropdownList;
		private KFlowLayoutPanel ButtonFlowLayoutPanel;
		internal ZArchitecture.GUI.ZButton ExportDiagnosticsButton;
	}
}
