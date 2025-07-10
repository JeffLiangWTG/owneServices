using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	partial class GlbAccreditationJobSkillGroupForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.contentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.saveButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			this.contentPanel.SuspendLayout();
			this.saveButtonsPanel.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 269, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditationJobSkillGroup);
			//
			// topPanel
			//
			this.topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 48, true);
			this.topPanel.TabIndex = 0;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			zTextBoxColumnStyleInfo1.ColumnName = "HS_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "HS_SkillDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			//
			// contentPanel
			//
			this.contentPanel.BackColor = System.Drawing.SystemColors.Control;
			this.contentPanel.Controls.Add(this.topPanel);
			//this.contentPanel.Controls.Add(this.jobSkillsGrid);
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 293, true);
			this.contentPanel.TabIndex = 0;
			//
			// saveButtonsPanel
			//
			this.saveButtonsPanel.Controls.Add(this.ButtonsUserControl);
			this.saveButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.saveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 293, true);
			this.saveButtonsPanel.Name = "saveButtonsPanel";
			this.saveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 37, true);
			this.saveButtonsPanel.TabIndex = 1;
			//
			// ButtonsUserControl
			//
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 25, true);
			this.ButtonsUserControl.TabIndex = 0;
			//
			// GlbAccreditationJobSkillGroupForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 329, true);
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.saveButtonsPanel);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditationJobSkillGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 367, true);
			this.Name = "GlbAccreditationJobSkillGroupForm";
			this.Controls.SetChildIndex(this.saveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.contentPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.contentPanel.ResumeLayout(false);
			this.contentPanel.PerformLayout();
			this.saveButtonsPanel.ResumeLayout(false);
			this.saveButtonsPanel.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZPanel saveButtonsPanel;
		private ZPanel topPanel;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private ZPanel contentPanel;

		#endregion
	}
}
