using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	partial class ISF10UserControl
	{
		void InitializeComponent()
		{
			this.rightPanel = new ZPanel();
			this.consolidatorDocAddressControl = new ISFDocAddressControl();
			this.stuffingLocationDocAddressControl = new ISFDocAddressControl();
			this.middlePanel = new ZPanel();
			this.sellingPartyDocAddressControl = new ISFDocAddressControl();
			this.buyingPartyDocAddressControl = new ISFDocAddressControl();
			this.leftPanel = new ZPanel();
			this.customizedFieldsGroupBox = new ZGroupBox();
			this.customAttribute2TextBox = new ZArchitecture.ZTextBox();
			this.customAttribute1TextBox = new ZArchitecture.ZTextBox();
			this.bondDetailsGroupBox = new ZGroupBox();
			this.bondNumberOrHolderCodeFindBox = new ZCodeFindBox();
			this.bF_BondReferenceNumberTextBox = new ZArchitecture.ZTextBox();
			this.bF_BondTypeDropEdit = new ZDropEdit();
			this.bF_BondActivityCodeDropEdit = new ZDropEdit();
			this.bF_EntryNumberTextBox2 = new ZArchitecture.ZTextBox();
			this.bF_SuretyCodeTextBox2 = new ZArchitecture.ZTextBox();
			this.consigneeDetailsGroupBox = new ZGroupBox();
			this.consigneeDropEditCodeFindBox = new MasterFiles.GUI.ZDropEditCodeFindBox();
			this.bF_ConsigneeFullNameTextBox = new ZArchitecture.ZTextBox();
			this.bF_ConsigneeCountryOfIssueCodeFindBox = new ZCodeFindBox();
			this.bF_ConsigneeDateOfBirthDateEdit = new ZDateEdit();
			this.importerDetailsGroupBox = new ZGroupBox();
			this.importerDropEditCodeFindBox = new MasterFiles.GUI.ZDropEditCodeFindBox();
			this.bF_ImporterFullNameTextBox = new ZArchitecture.ZTextBox();
			this.bF_DateOfBirthDateEdit = new ZDateEdit();
			this.bF_CountryOfIssueCodeFindBox = new ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.rightPanel.SuspendLayout();
			this.middlePanel.SuspendLayout();
			this.leftPanel.SuspendLayout();
			this.customizedFieldsGroupBox.SuspendLayout();
			this.bondDetailsGroupBox.SuspendLayout();
			this.consigneeDetailsGroupBox.SuspendLayout();
			this.importerDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusISFHeader);
			// 
			// RightPanel
			// 
			this.rightPanel.Controls.Add(this.consolidatorDocAddressControl);
			this.rightPanel.Controls.Add(this.stuffingLocationDocAddressControl);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 16, true);
			this.rightPanel.Name = "RightPanel";
			this.rightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 679, true);
			this.rightPanel.TabIndex = 2;
			// 
			// ConsolidatorDocAddressControl
			// 
			this.consolidatorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consolidatorDocAddressControl, "Consolidator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((CusISFHeader)(null)).Consolidator)));
			this.consolidatorDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.consolidatorDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("9de6063a-4b11-474a-bbc1-f4c7b3de2dd4", "Consolidator");
			this.consolidatorDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.consolidatorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.consolidatorDocAddressControl.Name = "ConsolidatorDocAddressControl";
			this.consolidatorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.consolidatorDocAddressControl.TabIndex = 1;
			// 
			// StuffingLocationDocAddressControl
			// 
			this.stuffingLocationDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.stuffingLocationDocAddressControl, "StuffingLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((CusISFHeader)(null)).StuffingLocation)));
			this.stuffingLocationDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.stuffingLocationDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("db820081-8f6d-4f39-8f4c-a2fe677b6d6c", "Container Stuffing Location");
			this.stuffingLocationDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.stuffingLocationDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.stuffingLocationDocAddressControl.Name = "StuffingLocationDocAddressControl";
			this.stuffingLocationDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.stuffingLocationDocAddressControl.TabIndex = 0;
			// 
			// MiddlePanel
			// 
			this.middlePanel.Controls.Add(this.sellingPartyDocAddressControl);
			this.middlePanel.Controls.Add(this.buyingPartyDocAddressControl);
			this.middlePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.middlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 16, true);
			this.middlePanel.Name = "MiddlePanel";
			this.middlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 679, true);
			this.middlePanel.TabIndex = 1;
			// 
			// SellingPartyDocAddressControl
			// 
			this.sellingPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sellingPartyDocAddressControl, "SellingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((CusISFHeader)(null)).SellingParty)));
			this.sellingPartyDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.sellingPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("c0e14976-fed5-4a90-8d1d-23d43f6a6682", "Selling Party");
			this.sellingPartyDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.sellingPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.sellingPartyDocAddressControl.Name = "SellingPartyDocAddressControl";
			this.sellingPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.sellingPartyDocAddressControl.TabIndex = 1;
			// 
			// BuyingPartyDocAddressControl
			// 
			this.buyingPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.buyingPartyDocAddressControl, "BuyingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((CusISFHeader)(null)).BuyingParty)));
			this.buyingPartyDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.buyingPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("54208f30-da62-4472-b82a-a71d99f4ecd0", "Buying Party");
			this.buyingPartyDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.buyingPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.buyingPartyDocAddressControl.Name = "BuyingPartyDocAddressControl";
			this.buyingPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.buyingPartyDocAddressControl.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.leftPanel.Controls.Add(this.customizedFieldsGroupBox);
			this.leftPanel.Controls.Add(this.bondDetailsGroupBox);
			this.leftPanel.Controls.Add(this.consigneeDetailsGroupBox);
			this.leftPanel.Controls.Add(this.importerDetailsGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.leftPanel.Name = "LeftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 679, true);
			this.leftPanel.TabIndex = 0;
			// 
			// CustomizedFieldsGroupBox
			// 
			this.customizedFieldsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("232f24a5-bdc3-4e5d-b500-e41562ba3529", "Customized Fields");
			this.customizedFieldsGroupBox.Controls.Add(this.customAttribute2TextBox);
			this.customizedFieldsGroupBox.Controls.Add(this.customAttribute1TextBox);
			this.customizedFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.customizedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 305, true);
			this.customizedFieldsGroupBox.Name = "CustomizedFieldsGroupBox";
			this.customizedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 59, true);
			this.customizedFieldsGroupBox.TabIndex = 3;
			this.customizedFieldsGroupBox.TabStop = false;
			// 
			// CustomAttribute2TextBox
			// 
			this.BindingSource.SetBindingMember(this.customAttribute2TextBox, "CustomAttribute2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).CustomAttribute2)));
			this.customAttribute2TextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("bf8329f4-7730-4e37-894b-6eba90c5e1bd", "Custom Attrib. 2");
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
			this.customAttribute1TextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("fccbe2a0-9af0-491c-8434-ae35f459a9fc", "Custom Attrib. 1");
			this.customAttribute1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 14, true);
			this.customAttribute1TextBox.Name = "CustomAttribute1TextBox";
			this.customAttribute1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.customAttribute1TextBox.TabIndex = 0;
			// 
			// BondDetailsGroupBox
			// 
			this.bondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("710cc68f-0ff7-4b2c-8df0-310471aaece4", "Bond Details");
			this.bondDetailsGroupBox.Controls.Add(this.bondNumberOrHolderCodeFindBox);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondReferenceNumberTextBox);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondTypeDropEdit);
			this.bondDetailsGroupBox.Controls.Add(this.bF_BondActivityCodeDropEdit);
			this.bondDetailsGroupBox.Controls.Add(this.bF_EntryNumberTextBox2);
			this.bondDetailsGroupBox.Controls.Add(this.bF_SuretyCodeTextBox2);
			this.bondDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.bondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.bondDetailsGroupBox.Name = "BondDetailsGroupBox";
			this.bondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 131, true);
			this.bondDetailsGroupBox.TabIndex = 2;
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
			this.bondNumberOrHolderCodeFindBox.ShowDescriptionBox = false;
			this.bondNumberOrHolderCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.bondNumberOrHolderCodeFindBox.TabIndex = 0;
			// 
			// BF_BondReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.bF_BondReferenceNumberTextBox, "BF_BondReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_BondReferenceNumber)));
			this.bF_BondReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("dfefd448-d799-40d4-bf3e-be7228d3506d", "Bond Ref. No.");
			this.bF_BondReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 85, true);
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
			this.bF_BondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 63, true);
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
			this.bF_BondActivityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 41, true);
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
			this.bF_EntryNumberTextBox2.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("28c493a4-4e3f-4057-98bd-20e8cf590392", "Entry Number");
			this.bF_EntryNumberTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 107, true);
			this.bF_EntryNumberTextBox2.Name = "BF_EntryNumberTextBox2";
			this.bF_EntryNumberTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_EntryNumberTextBox2.TabIndex = 5;
			// 
			// BF_SuretyCodeTextBox2
			// 
			this.BindingSource.SetBindingMember(this.bF_SuretyCodeTextBox2, "BF_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_SuretyCode)));
			this.bF_SuretyCodeTextBox2.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("c1eefe04-3cf1-4424-98d0-9364359540cb", "Surety Code");
			this.bF_SuretyCodeTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 63, true);
			this.bF_SuretyCodeTextBox2.Name = "BF_SuretyCodeTextBox2";
			this.bF_SuretyCodeTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.bF_SuretyCodeTextBox2.TabIndex = 3;
			// 
			// ConsigneeDetailsGroupBox
			// 
			this.consigneeDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("73dbe061-8c18-41c5-8039-23530285ccac", "Consignee Details");
			this.consigneeDetailsGroupBox.Controls.Add(this.consigneeDropEditCodeFindBox);
			this.consigneeDetailsGroupBox.Controls.Add(this.bF_ConsigneeFullNameTextBox);
			this.consigneeDetailsGroupBox.Controls.Add(this.bF_ConsigneeCountryOfIssueCodeFindBox);
			this.consigneeDetailsGroupBox.Controls.Add(this.bF_ConsigneeDateOfBirthDateEdit);
			this.consigneeDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.consigneeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
			this.consigneeDetailsGroupBox.Name = "ConsigneeDetailsGroupBox";
			this.consigneeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 87, true);
			this.consigneeDetailsGroupBox.TabIndex = 1;
			this.consigneeDetailsGroupBox.TabStop = false;
			// 
			// ConsigneeDropEditCodeFindBox
			// 
			this.consigneeDropEditCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consigneeDropEditCodeFindBox, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).ConsigneeCodeForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_ConsigneeCodeType)));
			this.consigneeDropEditCodeFindBox.BindToCode = "ConsigneeCodeForDisplay";
			this.consigneeDropEditCodeFindBox.BindToCodeType = "BF_ConsigneeCodeType";
			this.consigneeDropEditCodeFindBox.CodeModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.consigneeDropEditCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 11, true);
			this.consigneeDropEditCodeFindBox.Name = "ConsigneeDropEditCodeFindBox";
			this.consigneeDropEditCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.consigneeDropEditCodeFindBox.TabIndex = 1;
			// 
			// BF_ConsigneeFullNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.bF_ConsigneeFullNameTextBox, "BF_ConsigneeFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_ConsigneeFullName)));
			this.bF_ConsigneeFullNameTextBox.CaptionResourceString = null;
			this.bF_ConsigneeFullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 35, true);
			this.bF_ConsigneeFullNameTextBox.Name = "BF_ConsigneeFullNameTextBox";
			this.bF_ConsigneeFullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.bF_ConsigneeFullNameTextBox.TabIndex = 2;
			// 
			// BF_ConsigneeCountryOfIssueCodeFindBox
			// 
			this.bF_ConsigneeCountryOfIssueCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bF_ConsigneeCountryOfIssueCodeFindBox, "BF_ConsigneeCountryOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusISFHeader)(null)).BF_ConsigneeCountryOfIssue)));
			this.bF_ConsigneeCountryOfIssueCodeFindBox.CaptionResourceString = null;
			this.bF_ConsigneeCountryOfIssueCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 56, true);
			this.bF_ConsigneeCountryOfIssueCodeFindBox.Name = "BF_ConsigneeCountryOfIssueCodeFindBox";
			this.bF_ConsigneeCountryOfIssueCodeFindBox.PreBoundMaxLength = 2;
			this.bF_ConsigneeCountryOfIssueCodeFindBox.ShowDescriptionBox = false;
			this.bF_ConsigneeCountryOfIssueCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.bF_ConsigneeCountryOfIssueCodeFindBox.TabIndex = 4;
			// 
			// BF_ConsigneeDateOfBirthDateEdit
			// 
			this.bF_ConsigneeDateOfBirthDateEdit.AllowDrop = true;
			this.bF_ConsigneeDateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.bF_ConsigneeDateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.bF_ConsigneeDateOfBirthDateEdit, "BF_ConsigneeDateOfBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusISFHeader)(null)).BF_ConsigneeDateOfBirth)));
			this.bF_ConsigneeDateOfBirthDateEdit.CaptionResourceString = null;
			this.bF_ConsigneeDateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 56, true);
			this.bF_ConsigneeDateOfBirthDateEdit.Name = "BF_ConsigneeDateOfBirthDateEdit";
			this.bF_ConsigneeDateOfBirthDateEdit.TabIndex = 3;
			// 
			// ImporterDetailsGroupBox
			// 
			this.importerDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("43ef5264-d228-423f-90c9-c42401d8e477", "Importer Details");
			this.importerDetailsGroupBox.Controls.Add(this.importerDropEditCodeFindBox);
			this.importerDetailsGroupBox.Controls.Add(this.bF_ImporterFullNameTextBox);
			this.importerDetailsGroupBox.Controls.Add(this.bF_DateOfBirthDateEdit);
			this.importerDetailsGroupBox.Controls.Add(this.bF_CountryOfIssueCodeFindBox);
			this.importerDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.importerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importerDetailsGroupBox.Name = "ImporterDetailsGroupBox";
			this.importerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 87, true);
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
			this.importerDropEditCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 11, true);
			this.importerDropEditCodeFindBox.Name = "ImporterDropEditCodeFindBox";
			this.importerDropEditCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.importerDropEditCodeFindBox.TabIndex = 1;
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
			// ISF10UserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.rightPanel);
			this.Controls.Add(this.middlePanel);
			this.Controls.Add(this.leftPanel);
			this.Name = "ISF10UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 698, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.rightPanel.ResumeLayout(false);
			this.middlePanel.ResumeLayout(false);
			this.leftPanel.ResumeLayout(false);
			this.customizedFieldsGroupBox.ResumeLayout(false);
			this.customizedFieldsGroupBox.PerformLayout();
			this.bondDetailsGroupBox.ResumeLayout(false);
			this.bondDetailsGroupBox.PerformLayout();
			this.consigneeDetailsGroupBox.ResumeLayout(false);
			this.consigneeDetailsGroupBox.PerformLayout();
			this.importerDetailsGroupBox.ResumeLayout(false);
			this.importerDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		ZPanel middlePanel;
		ISFDocAddressControl sellingPartyDocAddressControl;
		ISFDocAddressControl buyingPartyDocAddressControl;
		ZPanel leftPanel;
		ZPanel rightPanel;
		ISFDocAddressControl consolidatorDocAddressControl;
		ISFDocAddressControl stuffingLocationDocAddressControl;
		ZGroupBox customizedFieldsGroupBox;
		ZArchitecture.ZTextBox customAttribute2TextBox;
		ZArchitecture.ZTextBox customAttribute1TextBox;
		ZGroupBox bondDetailsGroupBox;
		ZArchitecture.ZTextBox bF_BondReferenceNumberTextBox;
		ZDropEdit bF_BondTypeDropEdit;
		ZDropEdit bF_BondActivityCodeDropEdit;
		ZArchitecture.ZTextBox bF_EntryNumberTextBox2;
		ZArchitecture.ZTextBox bF_SuretyCodeTextBox2;
		ZGroupBox consigneeDetailsGroupBox;
		ZArchitecture.ZTextBox bF_ConsigneeFullNameTextBox;
		ZCodeFindBox bF_ConsigneeCountryOfIssueCodeFindBox;
		ZDateEdit bF_ConsigneeDateOfBirthDateEdit;
		ZGroupBox importerDetailsGroupBox;
		ZArchitecture.ZTextBox bF_ImporterFullNameTextBox;
		ZDateEdit bF_DateOfBirthDateEdit;
		ZCodeFindBox bF_CountryOfIssueCodeFindBox;
		MasterFiles.GUI.ZDropEditCodeFindBox importerDropEditCodeFindBox;
		MasterFiles.GUI.ZDropEditCodeFindBox consigneeDropEditCodeFindBox;
		ZCodeFindBox bondNumberOrHolderCodeFindBox;

	}
}
