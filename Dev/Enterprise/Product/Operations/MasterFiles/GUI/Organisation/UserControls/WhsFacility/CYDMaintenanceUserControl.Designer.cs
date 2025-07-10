using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class CYDMaintenanceUserControl
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
			this.CedexRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MercRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MNRGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MNRGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CedexRadioButton
			//
			this.CedexRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDMaintenanceUserControl|ff6cdd51-07ce-4722-bbdf-c434f59ef6df", "CEDEX");
			this.CedexRadioButton.AutoCheck = false;
			this.CedexRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 30, true);
			this.CedexRadioButton.Name = "CedexRadioButton";
			this.CedexRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 24, true);
			this.CedexRadioButton.TabIndex = 0;
			this.CedexRadioButton.TabStop = true;
			this.CedexRadioButton.UseVisualStyleBackColor = true;
			this.CedexRadioButton.CheckedChanged += new System.EventHandler(this.MRCodeGroupRadioButton_CheckedChanged);
			// 
			// MercRadioButton
			//
			this.MercRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDMaintenanceUserControl|d974776a-7802-450b-89cc-5232629baac3", "MERC");
			this.MercRadioButton.AutoCheck = false;
			this.MercRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 30, true);
			this.MercRadioButton.Name = "MercRadioButton";
			this.MercRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 24, true);
			this.MercRadioButton.TabIndex = 1;
			this.MercRadioButton.TabStop = true;
			this.MercRadioButton.UseVisualStyleBackColor = true;
			this.MercRadioButton.CheckedChanged += new System.EventHandler(this.MRCodeGroupRadioButton_CheckedChanged);
			// 
			// MNRGroupBox
			//
			this.MNRGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDMaintenanceUserControl|d4bbbbaa-7bf1-4d4a-9d63-a143ac789582", "M&&R code group");
			this.MNRGroupBox.Controls.Add(this.CedexRadioButton);
			this.MNRGroupBox.Controls.Add(this.MercRadioButton);
			this.MNRGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MNRGroupBox.Name = "MNRGroupBox";
			this.MNRGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 86, true);
			this.MNRGroupBox.TabIndex = 2;
			this.MNRGroupBox.TabStop = false;
			// 
			// CYDMaintenanceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MNRGroupBox);
			this.Name = "CYDMaintenanceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 565, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MNRGroupBox.ResumeLayout(false);
			this.MNRGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZRadioButton CedexRadioButton;
		private ZRadioButton MercRadioButton;
		private ZGroupBox MNRGroupBox;
	}
}
