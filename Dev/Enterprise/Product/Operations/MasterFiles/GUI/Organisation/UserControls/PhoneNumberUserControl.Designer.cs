namespace Enterprise.MasterFiles.GUI
{
	partial class PhoneNumberUserControl
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
			this.NumberTextBox = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl.PhoneNumberTextBox();
			this.PhoneDiallerControl = new Enterprise.MasterFiles.GUI.PhoneDiallerUserControl();
			this.PublishCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LocalNumberLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PhoneDiallerControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.PhoneNumber);
			// 
			// NumberTextBox
			// 
			this.NumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.NumberTextBox, "FormattedForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.PhoneNumber)(null)).FormattedForBinding)));
			this.NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 0, true);
			this.NumberTextBox.Name = "NumberTextBox";
			this.NumberTextBox.PhoneNumberProperty = null;
			this.NumberTextBox.PhoneNumberTooltipProperty = null;
			this.NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.NumberTextBox.TabIndex = 0;
			this.NumberTextBox.Enter += NumberTextBox_Enter;
			this.NumberTextBox.Leave += NumberTextBox_Leave;
			this.NumberTextBox.Validated += NumberTextBox_Validated;
			this.NumberTextBox.TextChanged += NumberTextBox_TextChanged;
			// 
			// PhoneDiallerControl
			// 
			this.PhoneDiallerControl.AllowDrop = true;
			this.PhoneDiallerControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PhoneDiallerControl, "FormattedForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.PhoneNumber)(null)).FormattedForBinding)));
			this.PhoneDiallerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 2, true);
			this.PhoneDiallerControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			this.PhoneDiallerControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			this.PhoneDiallerControl.Name = "PhoneDiallerControl";
			this.PhoneDiallerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			this.PhoneDiallerControl.TabIndex = 1;
			// 
			// PublishCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.PublishCheckEdit, "IsPublishedForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.PhoneNumber)(null)).IsPublishedForBinding)));
			this.PublishCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PhoneNumberUserControl|Publish", "Publish", "Publish on Client facing documents");
			this.PublishCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PublishCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 2, true);
			this.PublishCheckEdit.Name = "PublishCheckEdit";
			this.PublishCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.PublishCheckEdit.TabIndex = 3;
			// 
			// LocalNumberLinkLabel
			// 
			this.BindingSource.SetBindingMember(this.LocalNumberLinkLabel, "FormattedLocalNumberIfLoggedInSameCountryForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.PhoneNumber)(null)).FormattedLocalNumberIfLoggedInSameCountryForBinding)));
			this.LocalNumberLinkLabel.IsFontBold = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalNumberLinkLabel, false);
			this.LocalNumberLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 3, true);
			this.LocalNumberLinkLabel.Name = "LocalNumberLinkLabel";
			this.LocalNumberLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			this.LocalNumberLinkLabel.TabIndex = 2;
			this.LocalNumberLinkLabel.TabStop = false;
			this.LocalNumberLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LocalNumberLinkLabel_LinkClicked);
			// 
			// PhoneNumberUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberTextBox);
			this.Controls.Add(this.PhoneDiallerControl);
			this.Controls.Add(this.PublishCheckEdit);
			this.Controls.Add(this.LocalNumberLinkLabel);
			this.Name = "PhoneNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PhoneDiallerControl.ResumeLayout(true);
			this.PhoneDiallerControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal PhoneNumberTextBox NumberTextBox;
		protected PhoneDiallerUserControl PhoneDiallerControl;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox PublishCheckEdit;
		protected ZArchitecture.GUI.ZLinkLabel LocalNumberLinkLabel;
	}
}
