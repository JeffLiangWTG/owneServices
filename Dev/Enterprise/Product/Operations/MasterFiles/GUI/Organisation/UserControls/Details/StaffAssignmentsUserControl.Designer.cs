using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class StaffAssignmentsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.StaffAssignmentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.staffAssignmentsControl1 = new Enterprise.MasterFiles.GUI.StaffAssignmentsControl();
			this.StaffAssignmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShowForAllCompaniesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StaffAssignmentsPanel.SuspendLayout();
			this.staffAssignmentsControl1.SuspendLayout();
			this.StaffAssignmentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// StaffAssignmentsPanel
			// 
			this.StaffAssignmentsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StaffAssignmentsPanel.Controls.Add(this.staffAssignmentsControl1);
			this.StaffAssignmentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.StaffAssignmentsPanel.Name = "StaffAssignmentsPanel";
			this.StaffAssignmentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 343, true);
			this.StaffAssignmentsPanel.TabIndex = 0;
			// 
			// StaffAssignmentsGroupBox
			// 
			this.StaffAssignmentsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsUserControl|64ce1246-1029-468b-a15d-7d5388e44308", "Organization Staff Member Assignments");
			this.StaffAssignmentsGroupBox.Controls.Add(this.ShowForAllCompaniesCheckBox);
			this.StaffAssignmentsGroupBox.Controls.Add(this.DescriptionLabel);
			this.StaffAssignmentsGroupBox.Controls.Add(this.StaffAssignmentsPanel);
			this.StaffAssignmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StaffAssignmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StaffAssignmentsGroupBox.Name = "StaffAssignmentsGroupBox";
			this.StaffAssignmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 390, true);
			this.StaffAssignmentsGroupBox.TabIndex = 1;
			this.StaffAssignmentsGroupBox.TabStop = false;
			// 
			// ShowForAllCompaniesCheckBox
			// 
			this.ShowForAllCompaniesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowForAllCompaniesCheckBox.AutoSize = true;
			this.ShowForAllCompaniesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsUserControl|1a932cd6-093d-43d8-ac75-5bb736f72fa4", "Show for all companies");
			this.ShowForAllCompaniesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowForAllCompaniesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowForAllCompaniesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 19, true);
			this.ShowForAllCompaniesCheckBox.Name = "ShowForAllCompaniesCheckBox";
			this.ShowForAllCompaniesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShowForAllCompaniesCheckBox.TabIndex = 2;
			this.ShowForAllCompaniesCheckBox.CheckedChanged += new System.EventHandler(this.ShowForAllCompaniesCheckBox_CheckedChanged);
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("StaffAssignmentsUserControl|d413a29a-6313-4603-887e-28625771d81d", "This module allows you to allocate roles to staff members for this organization.");
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 18, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 13, true);
			this.DescriptionLabel.TabIndex = 1;
			// 
			// staffAssignmentsControl1
			// 
			this.BindingSource.SetBindingMember(this.staffAssignmentsControl1, ".");
			this.staffAssignmentsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.staffAssignmentsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.staffAssignmentsControl1.Name = "staffAssignmentsControl1";
			this.staffAssignmentsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 343, true);
			this.staffAssignmentsControl1.TabIndex = 2;
			// 
			// StaffAssignmentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StaffAssignmentsGroupBox);
			this.Name = "StaffAssignmentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 390, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StaffAssignmentsPanel.ResumeLayout(false);
			this.StaffAssignmentsPanel.PerformLayout();
			this.staffAssignmentsControl1.ResumeLayout(true);
			this.staffAssignmentsControl1.PerformLayout();
			this.StaffAssignmentsGroupBox.ResumeLayout(false);
			this.StaffAssignmentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel StaffAssignmentsPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox StaffAssignmentsGroupBox;
		private StaffAssignmentsControl staffAssignmentsControl1;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ShowForAllCompaniesCheckBox;
	}
}
