namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class NOBillPartiesUserControl
	{
		private void InitializeComponent()
		{
            this.RepresentativeSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
            this.RepresentativeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.RepresentativeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.RepresentativeStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RepresentativePostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativePhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ShipperEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ConsigneeEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RepresentativeSeparatorUserControl.SuspendLayout();
            this.RepresentativeAddressControl.SuspendLayout();
            this.RepresentativeCountryCodeFindBox.SuspendLayout();
            this.RepresentativeStateDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Manifest.Business.AsycudaBill);
            // 
            // RepresentativeSeparatorUserControl
            // 
            this.RepresentativeSeparatorUserControl.AllowDrop = true;
            this.RepresentativeSeparatorUserControl.CaptionResourceString = Enterprise.Customs.NO.Manifest.GUI.Res.GetData("CF039BF5-972E-4BF7-8CF4-03F4C952050C", "Representative Details");
            this.RepresentativeSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 4, true);
            this.RepresentativeSeparatorUserControl.Name = "RepresentativeSeparatorUserControl";
            this.RepresentativeSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.RepresentativeSeparatorUserControl.TabIndex = 0;
            // 
            // RepresentativeAddressControl
            // 
            this.RepresentativeAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "ABL_OA_Forwarder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_OA_Forwarder)));
            this.RepresentativeAddressControl.BindToOrgList = "Lookups.Organisations";
            this.RepresentativeAddressControl.CaptionResourceString = Enterprise.Customs.NO.Manifest.GUI.Res.GetData("B8E94C75-0F32-4E24-8898-D831EBDA6A5D", "Party");
            this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 20, true);
            this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
            this.RepresentativeAddressControl.PopupCaption = "";
            this.RepresentativeAddressControl.ShowAddress = false;
            this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 15, true);
            this.RepresentativeAddressControl.TabIndex = 1;
            // 
            // RepresentativeNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeNameTextBox, "ABL_ForwarderCompanyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderCompanyName)));
            this.RepresentativeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 58, true);
            this.RepresentativeNameTextBox.Name = "RepresentativeNameTextBox";
            this.RepresentativeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.RepresentativeNameTextBox.TabIndex = 3;
            // 
            // RepresentativeStreet1TextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeStreet1TextBox, "ABL_ForwarderStreet1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderStreet1)));
            this.RepresentativeStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 83, true);
            this.RepresentativeStreet1TextBox.Name = "RepresentativeStreet1TextBox";
            this.RepresentativeStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.RepresentativeStreet1TextBox.TabIndex = 4;
            // 
            // RepresentativeStreet2TextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeStreet2TextBox, "ABL_ForwarderStreet2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderStreet2)));
            this.RepresentativeStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 107, true);
            this.RepresentativeStreet2TextBox.Name = "RepresentativeStreet2TextBox";
            this.RepresentativeStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.RepresentativeStreet2TextBox.TabIndex = 5;
            // 
            // RepresentativeCityTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeCityTextBox, "ABL_ForwarderCity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderCity)));
            this.RepresentativeCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 128, true);
            this.RepresentativeCityTextBox.Name = "RepresentativeCityTextBox";
            this.RepresentativeCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.RepresentativeCityTextBox.TabIndex = 6;
            // 
            // RepresentativeCountryCodeFindBox
            // 
            this.RepresentativeCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RepresentativeCountryCodeFindBox, "ABL_Forwarder_RN_NKCountryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_Forwarder_RN_NKCountryCode)));
            this.RepresentativeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 149, true);
            this.RepresentativeCountryCodeFindBox.Name = "RepresentativeCountryCodeFindBox";
            this.RepresentativeCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.RepresentativeCountryCodeFindBox.ParentType = null;
            this.RepresentativeCountryCodeFindBox.PreBoundMaxLength = 2;
            this.RepresentativeCountryCodeFindBox.ShowDescriptionBox = false;
            this.RepresentativeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 15, true);
            this.RepresentativeCountryCodeFindBox.TabIndex = 7;
            // 
            // RepresentativeStateDropEdit
            // 
            this.RepresentativeStateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RepresentativeStateDropEdit, "ABL_ForwarderState");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderState)));
            this.RepresentativeStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 170, true);
            this.RepresentativeStateDropEdit.Name = "RepresentativeStateDropEdit";
            this.RepresentativeStateDropEdit.PreBoundMaxLength = 25;
            this.RepresentativeStateDropEdit.ShowDescriptionBox = false;
            this.RepresentativeStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 15, true);
            this.RepresentativeStateDropEdit.TabIndex = 8;
            // 
            // RepresentativePostCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativePostCodeTextBox, "ABL_ForwarderPostCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderPostCode)));
            this.RepresentativePostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 191, true);
            this.RepresentativePostCodeTextBox.Name = "RepresentativePostCodeTextBox";
            this.RepresentativePostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.RepresentativePostCodeTextBox.TabIndex = 9;
            // 
            // RepresentativePhoneTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativePhoneTextBox, "ABL_ForwarderPhone");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderPhone)));
            this.RepresentativePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 212, true);
            this.RepresentativePhoneTextBox.Name = "RepresentativePhoneTextBox";
            this.RepresentativePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
            this.RepresentativePhoneTextBox.TabIndex = 10;
            // 
            // RepresentativeRegNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeRegNoTextBox, "ABL_ForwarderRegNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderRegNumber)));
            this.RepresentativeRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 233, true);
            this.RepresentativeRegNoTextBox.Name = "RepresentativeRegNoTextBox";
            this.RepresentativeRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 15, true);
            this.RepresentativeRegNoTextBox.TabIndex = 12;
            // 
            // RepresentativeEmailTextBox
            // 
            this.BindingSource.SetBindingMember(this.RepresentativeEmailTextBox, "ABL_ForwarderEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ForwarderEmail)));
            this.RepresentativeEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.RepresentativeEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 257, true);
            this.RepresentativeEmailTextBox.Name = "RepresentativeEmailTextBox";
            this.RepresentativeEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.RepresentativeEmailTextBox.TabIndex = 13;
            // 
            // ShipperEmailTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShipperEmailTextBox, "ABL_ShipperEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ShipperEmail)));
            this.ShipperEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ShipperEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 20, true);
            this.ShipperEmailTextBox.Name = "ShipperEmailTextBox";
            this.ShipperEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.ShipperEmailTextBox.TabIndex = 14;
            // 
            // ConsigneeEmailTextBox
            // 
            this.BindingSource.SetBindingMember(this.ConsigneeEmailTextBox, "ABL_ConsigneeEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeEmail)));
            this.ConsigneeEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ConsigneeEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 48, true);
            this.ConsigneeEmailTextBox.Name = "ConsigneeEmailTextBox";
            this.ConsigneeEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.ConsigneeEmailTextBox.TabIndex = 15;
            // 
            // NOBillPartiesUserControl
            // 
            this.AutoScroll = true;
            this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 0, true);
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.RepresentativeSeparatorUserControl);
            this.Controls.Add(this.RepresentativeAddressControl);
            this.Controls.Add(this.RepresentativeNameTextBox);
            this.Controls.Add(this.RepresentativeStreet1TextBox);
            this.Controls.Add(this.RepresentativeStreet2TextBox);
            this.Controls.Add(this.RepresentativeCityTextBox);
            this.Controls.Add(this.RepresentativeCountryCodeFindBox);
            this.Controls.Add(this.RepresentativeStateDropEdit);
            this.Controls.Add(this.RepresentativePostCodeTextBox);
            this.Controls.Add(this.RepresentativePhoneTextBox);
            this.Controls.Add(this.RepresentativeRegNoTextBox);
            this.Controls.Add(this.RepresentativeEmailTextBox);
            this.Controls.Add(this.ShipperEmailTextBox);
            this.Controls.Add(this.ConsigneeEmailTextBox);
            this.Name = "NOBillPartiesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 341, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RepresentativeSeparatorUserControl.ResumeLayout(true);
            this.RepresentativeSeparatorUserControl.PerformLayout();
            this.RepresentativeAddressControl.ResumeLayout(true);
            this.RepresentativeAddressControl.PerformLayout();
            this.RepresentativeCountryCodeFindBox.ResumeLayout(true);
            this.RepresentativeCountryCodeFindBox.PerformLayout();
            this.RepresentativeStateDropEdit.ResumeLayout(true);
            this.RepresentativeStateDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl RepresentativeSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeStreet1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeStreet2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeCityTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox RepresentativeCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit RepresentativeStateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativePostCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativePhoneTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeRegNoTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RepresentativeEmailTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ShipperEmailTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeEmailTextBox;
	}
}
