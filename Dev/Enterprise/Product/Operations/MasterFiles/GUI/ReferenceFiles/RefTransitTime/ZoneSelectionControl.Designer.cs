namespace Enterprise.MasterFiles.GUI
{
	partial class ZoneSelectionControl
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
			this.isDomesticCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zoneFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.domesticZoneOwnerFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.domesticZoneCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.internationalZoneCarrierFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zoneFindBox.SuspendLayout();
			this.domesticZoneOwnerFindBox.SuspendLayout();
			this.domesticZoneCountryFindBox.SuspendLayout();
			this.internationalZoneCarrierFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ZoneSelection);
			// 
			// isDomesticCheckBox
			// 
			this.isDomesticCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isDomesticCheckBox, "IsDomestic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ZoneSelection)(null)).IsDomestic)));
			this.isDomesticCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isDomesticCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 2, true);
			this.isDomesticCheckBox.Name = "isDomesticCheckBox";
			this.isDomesticCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.isDomesticCheckBox.TabIndex = 0;
			this.isDomesticCheckBox.UseVisualStyleBackColor = true;
			// 
			// zoneFindBox
			// 
			this.zoneFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zoneFindBox, "ZonePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ZoneSelection)(null)).ZonePK)));
			this.zoneFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 22, true);
			this.zoneFindBox.Name = "zoneFindBox";
			this.zoneFindBox.ShowDescriptionBox = false;
			this.zoneFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.zoneFindBox.TabIndex = 8;
			// 
			// domesticZoneOwnerFindBox
			// 
			this.domesticZoneOwnerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.domesticZoneOwnerFindBox, "TransportZoneOwnerPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ZoneSelection)(null)).TransportZoneOwnerPK)));
			this.domesticZoneOwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 45, true);
			this.domesticZoneOwnerFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.domesticZoneOwnerFindBox.Name = "domesticZoneOwnerFindBox";
			this.domesticZoneOwnerFindBox.ShowDescriptionBox = false;
			this.domesticZoneOwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.domesticZoneOwnerFindBox.TabIndex = 11;
			this.domesticZoneOwnerFindBox.Visible = false;
			// 
			// domesticZoneCountryFindBox
			// 
			this.domesticZoneCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.domesticZoneCountryFindBox, "TransportZoneCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZoneSelection)(null)).TransportZoneCountry)));
			this.domesticZoneCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 67, true);
			this.domesticZoneCountryFindBox.Name = "domesticZoneCountryFindBox";
			this.domesticZoneCountryFindBox.ShowDescriptionBox = false;
			this.domesticZoneCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.domesticZoneCountryFindBox.TabIndex = 12;
			this.domesticZoneCountryFindBox.Visible = false;
			// 
			// internationalZoneCarrierFindBox
			// 
			this.internationalZoneCarrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.internationalZoneCarrierFindBox, "InternationalZoneCarrierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ZoneSelection)(null)).InternationalZoneCarrierPK)));
			this.internationalZoneCarrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 45, true);
			this.internationalZoneCarrierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.internationalZoneCarrierFindBox.Name = "internationalZoneCarrierFindBox";
			this.internationalZoneCarrierFindBox.ShowDescriptionBox = false;
			this.internationalZoneCarrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.internationalZoneCarrierFindBox.TabIndex = 13;
			// 
			// ZoneSelectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.internationalZoneCarrierFindBox);
			this.Controls.Add(this.domesticZoneCountryFindBox);
			this.Controls.Add(this.domesticZoneOwnerFindBox);
			this.Controls.Add(this.zoneFindBox);
			this.Controls.Add(this.isDomesticCheckBox);
			this.Name = "ZoneSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zoneFindBox.ResumeLayout(true);
			this.zoneFindBox.PerformLayout();
			this.domesticZoneOwnerFindBox.ResumeLayout(true);
			this.domesticZoneOwnerFindBox.PerformLayout();
			this.domesticZoneCountryFindBox.ResumeLayout(true);
			this.domesticZoneCountryFindBox.PerformLayout();
			this.internationalZoneCarrierFindBox.ResumeLayout(true);
			this.internationalZoneCarrierFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox isDomesticCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox zoneFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox domesticZoneOwnerFindBox;
		private ZArchitecture.GUI.ZCodeFindBox domesticZoneCountryFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox internationalZoneCarrierFindBox;
	}
}
