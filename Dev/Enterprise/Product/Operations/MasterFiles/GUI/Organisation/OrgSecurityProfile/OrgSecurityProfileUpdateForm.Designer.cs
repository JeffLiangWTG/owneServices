namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSecurityProfileUpdateForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.Profiles = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShouldApplyGrantedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldApplyCustomerSelfManagementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldMaintainExistingContactsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldEraseExistingContactsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Profiles.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater);
			// 
			// Profiles
			// 
			this.Profiles.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Profiles, "ProfileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater)(null)).ProfileName)));
			this.Profiles.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityUpdateForm|Profile", "Profile");
			this.Profiles.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Profiles.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 21, true);
			this.Profiles.Name = "Profiles";
			this.Profiles.PreBoundMaxLength = 25;
			this.Profiles.ShouldResizeByMaxLength = true;
			this.Profiles.ShowDescriptionBox = false;
			this.Profiles.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.Profiles.TabIndex = 1;
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c4e1d671-bdc5-4ef3-91e5-d0bd6c79429a", "Update");
			this.UpdateButton.IsCaptionOverridden = false;
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 131, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 6;
			this.UpdateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UpdateButton.ToolTipCaption = null;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2d50cdc-9a21-4da8-99f6-eb3b75221e67", "Cancel");
			this.CancelFormButton.IsCaptionOverridden = false;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 131, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 7;
			this.CancelFormButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelFormButton.ToolTipCaption = null;
			this.CancelFormButton.UseVisualStyleBackColor = true;
			this.CancelFormButton.Click += new System.EventHandler(this.CancelFormButton_Click);
			// 
			// ShouldApplyGrantedCheckBox
			// 
			this.ShouldApplyGrantedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldApplyGrantedCheckBox, "ShouldApplyGranted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater)(null)).ShouldApplyGranted)));
			this.ShouldApplyGrantedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4e62478a-a792-4bad-bb36-1849c84c97fa", "Apply \'Granted\'");
			this.ShouldApplyGrantedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldApplyGrantedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldApplyGrantedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 51, true);
			this.ShouldApplyGrantedCheckBox.Name = "ShouldApplyGrantedCheckBox";
			this.ShouldApplyGrantedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShouldApplyGrantedCheckBox.TabIndex = 2;
			this.ShouldApplyGrantedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShouldApplyCustomerSelfManagementCheckBox
			// 
			this.ShouldApplyCustomerSelfManagementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldApplyCustomerSelfManagementCheckBox, "ShouldApplyIsCustomerManaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater)(null)).ShouldApplyIsCustomerManaged)));
			this.ShouldApplyCustomerSelfManagementCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aa7a5b09-2f1b-4e2e-af6d-2346dad761bc", "Apply \'Customer Managed\'");
			this.ShouldApplyCustomerSelfManagementCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldApplyCustomerSelfManagementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldApplyCustomerSelfManagementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 69, true);
			this.ShouldApplyCustomerSelfManagementCheckBox.Name = "ShouldApplyCustomerSelfManagementCheckBox";
			this.ShouldApplyCustomerSelfManagementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShouldApplyCustomerSelfManagementCheckBox.TabIndex = 3;
			this.ShouldApplyCustomerSelfManagementCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShouldMaintainExistingContactsCheckBox
			// 
			this.ShouldMaintainExistingContactsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldMaintainExistingContactsCheckBox, "ShouldMaintainExistingContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater)(null)).ShouldMaintainExistingContacts)));
			this.ShouldMaintainExistingContactsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("05a9e3b8-a67a-42cd-ae30-4b7d3a335162", "Maintain Contact Rights");
			this.ShouldMaintainExistingContactsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldMaintainExistingContactsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldMaintainExistingContactsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 87, true);
			this.ShouldMaintainExistingContactsCheckBox.Name = "ShouldMaintainExistingContactsCheckBox";
			this.ShouldMaintainExistingContactsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShouldMaintainExistingContactsCheckBox.TabIndex = 4;
			this.ShouldMaintainExistingContactsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShouldEraseExistingContactsCheckBox
			// 
			this.ShouldEraseExistingContactsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldEraseExistingContactsCheckBox, "ShouldEraseExistingContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater)(null)).ShouldEraseExistingContacts)));
			this.ShouldEraseExistingContactsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08bbe505-08a7-4832-a67e-95fbad6980da", "Erase Contact Rights");
			this.ShouldEraseExistingContactsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldEraseExistingContactsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldEraseExistingContactsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 107, true);
			this.ShouldEraseExistingContactsCheckBox.Name = "ShouldEraseExistingContactsCheckBox";
			this.ShouldEraseExistingContactsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShouldEraseExistingContactsCheckBox.TabIndex = 5;
			this.ShouldEraseExistingContactsCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgSecurityProfileUpdateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7a165a3b-7d81-48c9-b55e-f58f19c2c360", "Apply Security Profile");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 184, true);
			this.Controls.Add(this.ShouldEraseExistingContactsCheckBox);
			this.Controls.Add(this.ShouldMaintainExistingContactsCheckBox);
			this.Controls.Add(this.ShouldApplyCustomerSelfManagementCheckBox);
			this.Controls.Add(this.ShouldApplyGrantedCheckBox);
			this.Controls.Add(this.CancelFormButton);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.Profiles);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSecurityProfileUpdater);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 223, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 223, true);
			this.Name = "OrgSecurityProfileUpdateForm";
			this.Controls.SetChildIndex(this.Profiles, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.CancelFormButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ShouldApplyGrantedCheckBox, 0);
			this.Controls.SetChildIndex(this.ShouldApplyCustomerSelfManagementCheckBox, 0);
			this.Controls.SetChildIndex(this.ShouldMaintainExistingContactsCheckBox, 0);
			this.Controls.SetChildIndex(this.ShouldEraseExistingContactsCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Profiles.ResumeLayout(true);
			this.Profiles.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton UpdateButton;
		private ZArchitecture.GUI.ZButton CancelFormButton;
		protected ZArchitecture.GUI.ZDropEdit Profiles;
		private ZArchitecture.GUI.ZCheckBox ShouldApplyGrantedCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldApplyCustomerSelfManagementCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldMaintainExistingContactsCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldEraseExistingContactsCheckBox;
	}
}
