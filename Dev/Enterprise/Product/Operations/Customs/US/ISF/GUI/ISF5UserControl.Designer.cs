using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	partial class ISF5UserControl
	{
		void InitializeComponent()
		{
			this.bookingPartyDocAddressControl = new ISFDocAddressControl();
			this.leftPanel = new ZPanel();
			this.customizedFieldsGroupBox = new ZGroupBox();
			this.customAttribute2TextBox = new ZArchitecture.ZTextBox();
			this.customAttribute1TextBox = new ZArchitecture.ZTextBox();
			this.locationGroupBox = new ZGroupBox();
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox = new ZCodeFindBox();
			this.bF_RL_NKPortOfUnloadCodeFindBox = new ZCodeFindBox();
			this.bondDetailsGroupBox = new ZGroupBox();
			this.bondNumberOrHolderCodeFindBox = new ZCodeFindBox();
			this.bF_BondReferenceNumberTextBox = new ZArchitecture.ZTextBox();
			this.bF_BondTypeDropEdit = new ZDropEdit();
			this.bF_BondActivityCodeDropEdit = new ZDropEdit();
			this.bF_EntryNumberTextBox2 = new ZArchitecture.ZTextBox();
			this.bF_SuretyCodeTextBox2 = new ZArchitecture.ZTextBox();
			this.importerDetailsGroupBox = new ZGroupBox();
			this.importerDropEditCodeFindBox = new MasterFiles.GUI.ZDropEditCodeFindBox();
			this.bF_DateOfBirthDateEdit = new ZDateEdit();
			this.bF_CountryOfIssueCodeFindBox = new ZCodeFindBox();
			this.bF_ImporterFullNameTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.leftPanel.SuspendLayout();
			this.customizedFieldsGroupBox.SuspendLayout();
			this.locationGroupBox.SuspendLayout();
			this.bondDetailsGroupBox.SuspendLayout();
			this.importerDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusISFHeader);
			// 
			// BookingPartyDocAddressControl
			// 
			this.bookingPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bookingPartyDocAddressControl, "BookingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((CusISFHeader)(null)).BookingParty)));
			this.bookingPartyDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.bookingPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|330bd527-c1da-457e-92cb-69570707e054", "Booking Party");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.bookingPartyDocAddressControl, false);
			this.bookingPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 14, true);
			this.bookingPartyDocAddressControl.Name = "BookingPartyDocAddressControl";
			this.bookingPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.bookingPartyDocAddressControl.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.leftPanel.Controls.Add(this.customizedFieldsGroupBox);
			this.leftPanel.Controls.Add(this.locationGroupBox);
			this.leftPanel.Controls.Add(this.bondDetailsGroupBox);
			this.leftPanel.Controls.Add(this.importerDetailsGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.leftPanel.Name = "LeftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 486, true);
			this.leftPanel.TabIndex = 0;
			// 
			// CustomizedFieldsGroupBox
			// 
			this.customizedFieldsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|c2b0b842-6c54-4285-9f1a-f2ce61926e90", "Customized Fields");
			this.customizedFieldsGroupBox.Controls.Add(this.customAttribute2TextBox);
			this.customizedFieldsGroupBox.Controls.Add(this.customAttribute1TextBox);
			this.customizedFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.customizedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 278, true);
			this.customizedFieldsGroupBox.Name = "CustomizedFieldsGroupBox";
			this.customizedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 91, true);
			this.customizedFieldsGroupBox.TabIndex = 3;
			this.customizedFieldsGroupBox.TabStop = false;
			// 
			// CustomAttribute2TextBox
			// 
			this.BindingSource.SetBindingMember(this.customAttribute2TextBox, "CustomAttribute2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).CustomAttribute2)));
			this.customAttribute2TextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|5b0bd9f3-26a1-4c70-a084-3b959cbe1d93", "Cus. Att. 2", "Cus. Attrib. 2", "Custom Attrib. 2", "");
			this.customAttribute2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 35, true);
			this.customAttribute2TextBox.Name = "CustomAttribute2TextBox";
			this.customAttribute2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.customAttribute2TextBox.TabIndex = 1;
			// 
			// CustomAttribute1TextBox
			// 
			this.BindingSource.SetBindingMember(this.customAttribute1TextBox, "CustomAttribute1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).CustomAttribute1)));
			this.customAttribute1TextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|eda2d63d-c710-4df1-8600-4937e316c49f", "Cus. Att. 1", "Cus. Attrib. 1", "Custom Attrib. 1", "");
			this.customAttribute1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 14, true);
			this.customAttribute1TextBox.Name = "CustomAttribute1TextBox";
			this.customAttribute1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.customAttribute1TextBox.TabIndex = 0;
			// 
			// LocationGroupBox
			// 
			this.locationGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|0d9658a5-4109-42d2-b9c4-4f4aad77476c", "Locations");
			this.locationGroupBox.Controls.Add(this.bF_RL_NKPlaceOfDeliveryCodeFindBox);
			this.locationGroupBox.Controls.Add(this.bF_RL_NKPortOfUnloadCodeFindBox);
			this.locationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.locationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
			this.locationGroupBox.Name = "LocationGroupBox";
			this.locationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 62, true);
			this.locationGroupBox.TabIndex = 2;
			this.locationGroupBox.TabStop = false;
			// 
			// BF_RL_NKPlaceOfDeliveryCodeFindBox
			// 
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_RL_NKPlaceOfDeliveryCodeFindBox, "BF_RL_NKPlaceOfDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_RL_NKPlaceOfDelivery)));
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.CaptionResourceString = null;
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 35, true);
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.Name = "BF_RL_NKPlaceOfDeliveryCodeFindBox";
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.PreBoundMaxLength = 5;
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_RL_NKPlaceOfDeliveryCodeFindBox.TabIndex = 1;
			// 
			// BF_RL_NKPortOfUnloadCodeFindBox
			// 
			this.bF_RL_NKPortOfUnloadCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_RL_NKPortOfUnloadCodeFindBox, "BF_RL_NKPortOfUnload");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_RL_NKPortOfUnload)));
			this.bF_RL_NKPortOfUnloadCodeFindBox.CaptionResourceString = null;
			this.bF_RL_NKPortOfUnloadCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 14, true);
			this.bF_RL_NKPortOfUnloadCodeFindBox.Name = "BF_RL_NKPortOfUnloadCodeFindBox";
			this.bF_RL_NKPortOfUnloadCodeFindBox.PreBoundMaxLength = 5;
			this.bF_RL_NKPortOfUnloadCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_RL_NKPortOfUnloadCodeFindBox.TabIndex = 0;
			// 
			// BondDetailsGroupBox
			// 
			this.bondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|86b392f2-ec80-45c9-a570-3d162d5fdb4e", "Bond Details");
			this.bondDetailsGroupBox.Controls.Add(this.bondNumberOrHolderCodeFindBox);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondReferenceNumberTextBox);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondTypeDropEdit);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondActivityCodeDropEdit);
			this.bondDetailsGroupBox.Controls.Add(this.bF_EntryNumberTextBox2);
			this.bondDetailsGroupBox.Controls.Add(this.bF_SuretyCodeTextBox2);
			this.bondDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.bondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.bondDetailsGroupBox.Name = "BondDetailsGroupBox";
			this.bondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 132, true);
			this.bondDetailsGroupBox.TabIndex = 1;
			this.bondDetailsGroupBox.TabStop = false;
			// 
			// BondNumberOrHolderCodeFindBox
			// 
			this.bondNumberOrHolderCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bondNumberOrHolderCodeFindBox, "BondNumberOrHolderForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BondNumberOrHolderForDisplay)));
			this.bondNumberOrHolderCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 19, true);
			this.bondNumberOrHolderCodeFindBox.Name = "BondNumberOrHolderCodeFindBox";
			this.bondNumberOrHolderCodeFindBox.PreBoundMaxLength = 4;
			this.bondNumberOrHolderCodeFindBox.ShowDescriptionBox = false;
			this.bondNumberOrHolderCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.bondNumberOrHolderCodeFindBox.TabIndex = 0;
			// 
			// BF_BondReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.bF_BondReferenceNumberTextBox, "BF_BondReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_BondReferenceNumber)));
			this.bF_BondReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|5627a1bb-3f89-4e50-85d2-36fecfa4818e", "Bond Ref. No.");
			this.bF_BondReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 82, true);
			this.bF_BondReferenceNumberTextBox.Name = "BF_BondReferenceNumberTextBox";
			this.bF_BondReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_BondReferenceNumberTextBox.TabIndex = 4;
			// 
			// BF_BondTypeDropEdit
			// 
			this.bF_BondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_BondTypeDropEdit, "BF_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusISFHeader)(null)).BF_BondType)));
			this.bF_BondTypeDropEdit.CaptionResourceString = null;
			this.bF_BondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 61, true);
			this.bF_BondTypeDropEdit.Name = "BF_BondTypeDropEdit";
			this.bF_BondTypeDropEdit.PreBoundMaxLength = 1;
			this.bF_BondTypeDropEdit.ShowDescriptionBox = false;
			this.bF_BondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.bF_BondTypeDropEdit.TabIndex = 2;
			// 
			// BF_BondActivityCodeDropEdit
			// 
			this.bF_BondActivityCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_BondActivityCodeDropEdit, "BF_BondActivityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusISFHeader)(null)).BF_BondActivityCode)));
			this.bF_BondActivityCodeDropEdit.CaptionResourceString = null;
			this.bF_BondActivityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 40, true);
			this.bF_BondActivityCodeDropEdit.Name = "BF_BondActivityCodeDropEdit";
			this.bF_BondActivityCodeDropEdit.PreBoundMaxLength = 2;
			this.bF_BondActivityCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_BondActivityCodeDropEdit.TabIndex = 1;
			// 
			// BF_EntryNumberTextBox2
			// 
			this.BindingSource.SetBindingMember(this.bF_EntryNumberTextBox2, "BF_EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_EntryNumber)));
			this.bF_EntryNumberTextBox2.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|a660a29b-d723-4a3c-a60b-8e607b3410df", "Entry Number");
			this.bF_EntryNumberTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 105, true);
			this.bF_EntryNumberTextBox2.Name = "BF_EntryNumberTextBox2";
			this.bF_EntryNumberTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_EntryNumberTextBox2.TabIndex = 5;
			// 
			// BF_SuretyCodeTextBox2
			// 
			this.BindingSource.SetBindingMember(this.bF_SuretyCodeTextBox2, "BF_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_SuretyCode)));
			this.bF_SuretyCodeTextBox2.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|39cbbf98-cf3d-425d-923c-d986ad6167fb", "Surety Code");
			this.bF_SuretyCodeTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 61, true);
			this.bF_SuretyCodeTextBox2.Name = "BF_SuretyCodeTextBox2";
			this.bF_SuretyCodeTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.bF_SuretyCodeTextBox2.TabIndex = 3;
			// 
			// ImporterDetailsGroupBox
			// 
			this.importerDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISF5UserControl|3d4bb209-35c1-44ef-931b-a18b15a943d7", "Importer Details");
			this.importerDetailsGroupBox.Controls.Add(this.importerDropEditCodeFindBox);
			this.importerDetailsGroupBox.Controls.Add(this.bF_DateOfBirthDateEdit);
			this.importerDetailsGroupBox.Controls.Add(this.bF_CountryOfIssueCodeFindBox);
			this.importerDetailsGroupBox.Controls.Add(this.bF_ImporterFullNameTextBox);
			this.importerDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.importerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importerDetailsGroupBox.Name = "ImporterDetailsGroupBox";
			this.importerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 84, true);
			this.importerDetailsGroupBox.TabIndex = 0;
			this.importerDetailsGroupBox.TabStop = false;
			// 
			// ImporterDropEditCodeFindBox
			// 
			this.importerDropEditCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importerDropEditCodeFindBox, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).ImporterCodeForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_ImporterCodeType)));
			this.importerDropEditCodeFindBox.BindToCode = "ImporterCodeForDisplay";
			this.importerDropEditCodeFindBox.BindToCodeType = "BF_ImporterCodeType";
			this.importerDropEditCodeFindBox.CodeModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.importerDropEditCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 13, true);
			this.importerDropEditCodeFindBox.Name = "ImporterDropEditCodeFindBox";
			this.importerDropEditCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.importerDropEditCodeFindBox.TabIndex = 1;
			// 
			// BF_DateOfBirthDateEdit
			// 
			this.bF_DateOfBirthDateEdit.AllowDrop = true;
			this.bF_DateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.bF_DateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.bF_DateOfBirthDateEdit, "BF_DateOfBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusISFHeader)(null)).BF_DateOfBirth)));
			this.bF_DateOfBirthDateEdit.CaptionResourceString = null;
			this.bF_DateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 56, true);
			this.bF_DateOfBirthDateEdit.Name = "BF_DateOfBirthDateEdit";
			this.bF_DateOfBirthDateEdit.TabIndex = 3;
			// 
			// BF_CountryOfIssueCodeFindBox
			// 
			this.bF_CountryOfIssueCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_CountryOfIssueCodeFindBox, "BF_CountryOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_CountryOfIssue)));
			this.bF_CountryOfIssueCodeFindBox.CaptionResourceString = null;
			this.bF_CountryOfIssueCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 56, true);
			this.bF_CountryOfIssueCodeFindBox.Name = "BF_CountryOfIssueCodeFindBox";
			this.bF_CountryOfIssueCodeFindBox.PreBoundMaxLength = 2;
			this.bF_CountryOfIssueCodeFindBox.ShowDescriptionBox = false;
			this.bF_CountryOfIssueCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.bF_CountryOfIssueCodeFindBox.TabIndex = 4;
			// 
			// BF_ImporterFullNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.bF_ImporterFullNameTextBox, "BF_ImporterFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_ImporterFullName)));
			this.bF_ImporterFullNameTextBox.CaptionResourceString = null;
			this.bF_ImporterFullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 35, true);
			this.bF_ImporterFullNameTextBox.Name = "BF_ImporterFullNameTextBox";
			this.bF_ImporterFullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_ImporterFullNameTextBox.TabIndex = 2;
			// 
			// ISF5UserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.leftPanel);
			this.Controls.Add(this.bookingPartyDocAddressControl);
			this.Name = "ISF5UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 505, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.leftPanel.ResumeLayout(false);
			this.customizedFieldsGroupBox.ResumeLayout(false);
			this.customizedFieldsGroupBox.PerformLayout();
			this.locationGroupBox.ResumeLayout(false);
			this.bondDetailsGroupBox.ResumeLayout(false);
			this.bondDetailsGroupBox.PerformLayout();
			this.importerDetailsGroupBox.ResumeLayout(false);
			this.importerDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		ISFDocAddressControl bookingPartyDocAddressControl;
		ZPanel leftPanel;
		ZGroupBox customizedFieldsGroupBox;
		ZArchitecture.ZTextBox customAttribute2TextBox;
		ZArchitecture.ZTextBox customAttribute1TextBox;
		ZGroupBox locationGroupBox;
		ZCodeFindBox bF_RL_NKPlaceOfDeliveryCodeFindBox;
		ZCodeFindBox bF_RL_NKPortOfUnloadCodeFindBox;
		ZGroupBox bondDetailsGroupBox;
		ZArchitecture.ZTextBox bF_BondReferenceNumberTextBox;
		ZDropEdit bF_BondTypeDropEdit;
		ZDropEdit bF_BondActivityCodeDropEdit;
		ZArchitecture.ZTextBox bF_EntryNumberTextBox2;
		ZArchitecture.ZTextBox bF_SuretyCodeTextBox2;
		ZGroupBox importerDetailsGroupBox;
		ZDateEdit bF_DateOfBirthDateEdit;
		ZCodeFindBox bF_CountryOfIssueCodeFindBox;
		ZCodeFindBox bondNumberOrHolderCodeFindBox;
		MasterFiles.GUI.ZDropEditCodeFindBox importerDropEditCodeFindBox;
		ZArchitecture.ZTextBox bF_ImporterFullNameTextBox;
	}
}
