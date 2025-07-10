namespace Enterprise.Customs.US.GUI
{
	partial class OMCForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.US_ElectronicImageSubmittedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.US_OA_ExporterControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_FDAQty1CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GovOfficialCertificationDateDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.US_OA_ResponsibleGovernmentOfficialAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.US_ExporterCertificationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.US_DeclarationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AquacultureFacilityAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.US_DepartureDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.US_SourceCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalAquacultureFacilitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CigarsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.US_OA_ExporterControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.Panel2.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.HeaderGroupBox.SuspendLayout();
			this.US_FDAQty1CalcDropEdit.SuspendLayout();
			this.GovOfficialCertificationDateDateEdit1.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.SuspendLayout();
			this.US_ExporterCertificationDateDateEdit.SuspendLayout();
			this.US_DeclarationCodeDropEdit.SuspendLayout();
			this.AquacultureFacilityAddressControl.SuspendLayout();
			this.US_DepartureDateDateEdit.SuspendLayout();
			this.US_SourceCountryCodeFindBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.AdditionalAquacultureFacilitiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CigarsGrid)).BeginInit();
			this.CigarsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.OMCHeader);
			// 
			// US_ElectronicImageSubmittedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_ElectronicImageSubmittedCheckBox, "US_ElectronicImageSubmitted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_ElectronicImageSubmitted)));
			this.US_ElectronicImageSubmittedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_ElectronicImageSubmittedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_ElectronicImageSubmittedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 19, true);
			this.US_ElectronicImageSubmittedCheckBox.Name = "US_ElectronicImageSubmittedCheckBox";
			this.US_ElectronicImageSubmittedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 19, true);
			this.US_ElectronicImageSubmittedCheckBox.TabIndex = 0;
			this.US_ElectronicImageSubmittedCheckBox.Text = "Elec. Image Submitted";
			this.US_ElectronicImageSubmittedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.US_ElectronicImageSubmittedCheckBox.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 16;
			this.OKButton.Text = "Close";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// US_OA_ExporterControl
			// 
			this.US_OA_ExporterControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_ExporterControl, "US_OA_Exporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OA_Exporter)));
			this.US_OA_ExporterControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.US_OA_ExporterControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6505D9B4-5B8C-483C-B84F-AE277524EBEE", "Exporter");
			this.US_OA_ExporterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 13, true);
			this.US_OA_ExporterControl.Name = "US_OA_ExporterControl";
			this.US_OA_ExporterControl.PopupCaption = "";
			this.US_OA_ExporterControl.ReadOnly = false;
			this.US_OA_ExporterControl.ShowAddress = false;
			this.US_OA_ExporterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.US_OA_ExporterControl.TabIndex = 0;
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			this.DetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HeaderGroupBox);
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 446, true);
			this.DetailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(250);
			// 
			// DetailsSplitContainer.Panel2
			// 
			this.DetailsSplitContainer.Panel2.Controls.Add(this.AdditionalAquacultureFacilitiesGroupBox);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(250);
			this.DetailsSplitContainer.TabIndex = 18;
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.Controls.Add(this.US_FDAQty1CalcDropEdit);
			this.HeaderGroupBox.Controls.Add(this.GovOfficialCertificationDateDateEdit1);
			this.HeaderGroupBox.Controls.Add(this.zGroupBox3);
			this.HeaderGroupBox.Controls.Add(this.US_ExporterCertificationDateDateEdit);
			this.HeaderGroupBox.Controls.Add(this.US_DeclarationCodeDropEdit);
			this.HeaderGroupBox.Controls.Add(this.AquacultureFacilityAddressControl);
			this.HeaderGroupBox.Controls.Add(this.US_DepartureDateDateEdit);
			this.HeaderGroupBox.Controls.Add(this.US_SourceCountryCodeFindBox);
			this.HeaderGroupBox.Controls.Add(this.zGroupBox1);
			this.HeaderGroupBox.Controls.Add(this.US_ElectronicImageSubmittedCheckBox);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 250, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			this.HeaderGroupBox.Text = "Header";
			// 
			// US_FDAQty1CalcDropEdit
			// 
			this.US_FDAQty1CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty1CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.OMCHeader)(null)).AddInfoLookups.UnitOfMeasureList)));
			this.US_FDAQty1CalcDropEdit.BindToAmount = "US_NetWeight";
			this.US_FDAQty1CalcDropEdit.BindToList = "AddInfoLookups+UnitOfMeasureList";
			this.US_FDAQty1CalcDropEdit.BindToUnit = "US_NetWeightUQ";
			this.US_FDAQty1CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cdc78089-c247-4140-aff6-dd920e5f7f02", "Net Weight");
			this.US_FDAQty1CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 71, true);
			this.US_FDAQty1CalcDropEdit.Name = "US_FDAQty1CalcDropEdit";
			this.US_FDAQty1CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 18, true);
			this.US_FDAQty1CalcDropEdit.TabIndex = 7;
			this.US_FDAQty1CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// GovOfficialCertificationDateDateEdit1
			// 
			this.GovOfficialCertificationDateDateEdit1.AllowDrop = true;
			this.GovOfficialCertificationDateDateEdit1.AutoCompleteMonthThreshold = 1;
			this.GovOfficialCertificationDateDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.GovOfficialCertificationDateDateEdit1, "US_OfficialCertificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OfficialCertificationDate)));
			this.GovOfficialCertificationDateDateEdit1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("359ae46d-b711-4c81-800b-8bff3b3493bc", "Gov. Official Certification Date ");
			this.GovOfficialCertificationDateDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 219, true);
			this.GovOfficialCertificationDateDateEdit1.Name = "GovOfficialCertificationDateDateEdit1";
			this.GovOfficialCertificationDateDateEdit1.TabIndex = 9;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.zTextBox4);
			this.zGroupBox3.Controls.Add(this.zTextBox5);
			this.zGroupBox3.Controls.Add(this.zTextBox6);
			this.zGroupBox3.Controls.Add(this.US_OA_ResponsibleGovernmentOfficialAddressControl);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 97, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 116, true);
			this.zGroupBox3.TabIndex = 8;
			this.zGroupBox3.TabStop = false;
			this.zGroupBox3.Text = "Box 8 Gov’t Official";
			// 
			// zTextBox4
			// 
			this.zTextBox4.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox4, "US_OfficialPGAContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OfficialPGAContactEmail)));
			this.zTextBox4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a168ac35-2dd0-422b-8227-2747ab9cc631", "PGA Contact Email");
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 90, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.zTextBox4.TabIndex = 3;
			// 
			// zTextBox5
			// 
			this.zTextBox5.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox5, "US_OfficialPGAContactPhoneNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OfficialPGAContactPhoneNo)));
			this.zTextBox5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("90319214-2488-4977-ae3f-df6e1e5d4843", "PGA Contact Phone");
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 64, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.zTextBox5.TabIndex = 2;
			// 
			// zTextBox6
			// 
			this.zTextBox6.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox6, "US_OfficialPGAContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OfficialPGAContactName)));
			this.zTextBox6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("93328464-5f8f-44bc-85c2-e0c5aeb37577", "PGA Contact Name");
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 38, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.zTextBox6.TabIndex = 1;
			// 
			// US_OA_ResponsibleGovernmentOfficialAddressControl
			// 
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_ResponsibleGovernmentOfficialAddressControl, "US_OA_ResponsibleGovernmentOfficial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OA_ResponsibleGovernmentOfficial)));
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("21205708-318a-425c-9ee2-9f46d6ea6cbb", "Responsible Gov. Official");
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 12, true);
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.Name = "US_OA_ResponsibleGovernmentOfficialAddressControl";
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.PopupCaption = "";
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.ReadOnly = false;
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.ShowAddress = false;
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.TabIndex = 0;
			// 
			// US_ExporterCertificationDateDateEdit
			// 
			this.US_ExporterCertificationDateDateEdit.AllowDrop = true;
			this.US_ExporterCertificationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.US_ExporterCertificationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.US_ExporterCertificationDateDateEdit, "US_ExporterCertificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_ExporterCertificationDate)));
			this.US_ExporterCertificationDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fd2c4c68-9c8f-49cd-baff-05c535b9d40f", "Exporter Certification Date");
			this.US_ExporterCertificationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 219, true);
			this.US_ExporterCertificationDateDateEdit.Name = "US_ExporterCertificationDateDateEdit";
			this.US_ExporterCertificationDateDateEdit.TabIndex = 4;
			// 
			// US_DeclarationCodeDropEdit
			// 
			this.US_DeclarationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_DeclarationCodeDropEdit, "US_DeclarationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_DeclarationCode)));
			this.US_DeclarationCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9491a79b-de0b-445b-ab41-24d922b1a166", "Box 7 Declaration Code");
			this.US_DeclarationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 45, true);
			this.US_DeclarationCodeDropEdit.Name = "US_DeclarationCodeDropEdit";
			this.US_DeclarationCodeDropEdit.PreBoundMaxLength = 2;
			this.US_DeclarationCodeDropEdit.ShowDescriptionBox = false;
			this.US_DeclarationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
			this.US_DeclarationCodeDropEdit.TabIndex = 6;
			// 
			// AquacultureFacilityAddressControl
			// 
			this.AquacultureFacilityAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AquacultureFacilityAddressControl, "US_OA_AquacultureFacility");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_OA_AquacultureFacility)));
			this.AquacultureFacilityAddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.AquacultureFacilityAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("11ecc133-53d0-4ef0-8169-103c4c798ba6", "Aquaculture Facility");
			this.AquacultureFacilityAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 71, true);
			this.AquacultureFacilityAddressControl.Name = "AquacultureFacilityAddressControl";
			this.AquacultureFacilityAddressControl.PopupCaption = "";
			this.AquacultureFacilityAddressControl.ReadOnly = false;
			this.AquacultureFacilityAddressControl.ShowAddress = false;
			this.AquacultureFacilityAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.AquacultureFacilityAddressControl.TabIndex = 2;
			// 
			// US_DepartureDateDateEdit
			// 
			this.US_DepartureDateDateEdit.AllowDrop = true;
			this.US_DepartureDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.US_DepartureDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.US_DepartureDateDateEdit, "US_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_DepartureDate)));
			this.US_DepartureDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e9eeb685-4a73-493b-bfff-3bdebd4475b8", "Departure Date");
			this.US_DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 45, true);
			this.US_DepartureDateDateEdit.Name = "US_DepartureDateDateEdit";
			this.US_DepartureDateDateEdit.TabIndex = 1;
			// 
			// US_SourceCountryCodeFindBox
			// 
			this.US_SourceCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_SourceCountryCodeFindBox, "US_SourceCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_SourceCountry)));
			this.US_SourceCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4e0685b8-663c-4ad5-b2f6-ce13f945c251", "Source Country/Region");
			this.US_SourceCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 19, true);
			this.US_SourceCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.US_SourceCountryCodeFindBox.Name = "US_SourceCountryCodeFindBox";
			this.US_SourceCountryCodeFindBox.PopupCaption = null;
			this.US_SourceCountryCodeFindBox.PreBoundMaxLength = 2;
			this.US_SourceCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 18, true);
			this.US_SourceCountryCodeFindBox.TabIndex = 5;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c9489b90-3e5a-4ab4-ae4f-2abd56b7f9d1", "Exporter");
			this.zGroupBox1.Controls.Add(this.zTextBox3);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.US_OA_ExporterControl);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 97, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 116, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Exporter";
			// 
			// zTextBox3
			// 
			this.zTextBox3.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox3, "US_ExporterPGAContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_ExporterPGAContactEmail)));
			this.zTextBox3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f6e7be9a-dc31-4c7b-8a9c-27de4b4a6fa4", "PGA Contact Email");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 90, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 18, true);
			this.zTextBox3.TabIndex = 3;
			// 
			// zTextBox2
			// 
			this.zTextBox2.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox2, "US_ExporterPGAContactPhoneNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_ExporterPGAContactPhoneNo)));
			this.zTextBox2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eb6b4082-7901-423c-9cf6-ffce578921c9", "PGA Contact Phone");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 18, true);
			this.zTextBox2.TabIndex = 2;
			// 
			// zTextBox1
			// 
			this.zTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox1, "US_ExporterPGAContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OMCHeader)(null)).US_ExporterPGAContactName)));
			this.zTextBox1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b5c65d25-1132-428e-b93d-ed56a4d9beb8", "PGA Contact Name");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 38, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 18, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// AdditionalAquacultureFacilitiesGroupBox
			// 
			this.AdditionalAquacultureFacilitiesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8fce4cbc-4e95-4b73-abe6-7a95c2310bff", "Additional Aquaculture Facilities");
			this.AdditionalAquacultureFacilitiesGroupBox.Controls.Add(this.CigarsGrid);
			this.AdditionalAquacultureFacilitiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAquacultureFacilitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalAquacultureFacilitiesGroupBox.Name = "AdditionalAquacultureFacilitiesGroupBox";
			this.AdditionalAquacultureFacilitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 194, true);
			this.AdditionalAquacultureFacilitiesGroupBox.TabIndex = 10;
			this.AdditionalAquacultureFacilitiesGroupBox.TabStop = false;
			this.AdditionalAquacultureFacilitiesGroupBox.Text = "Additional Aquaculture Facilities";
			// 
			// CigarsGrid
			// 
			this.CigarsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CigarsGrid, "AquacultureFacilities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.OMCHeader)(null)).AquacultureFacilities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.USOMCAquacultureFacility)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OMCHeader)(null)).AquacultureFacilities)).SyncRoot)).AquacultureFacilityOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.USOMCAquacultureFacility)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OMCHeader)(null)).AquacultureFacilities)).SyncRoot)).US_OA_AquacultureFacility)));
			this.CigarsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3a33bc60-6225-435b-bf66-bc98547ce5e1", "Aquaculture Facility");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AquacultureFacilityOrgPK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0bfa06e9-4c15-4f83-8b1e-df9dedd4a3b1", "Address");
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_AquacultureFacility";
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CigarsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CigarsGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.CigarsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CigarsGrid.GridId = "c3b0c525-e5c1-4250-8934-494b2cf2c4dc";
			this.CigarsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CigarsGrid.LayoutKey = "CigarsGrid";
			this.CigarsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CigarsGrid.Name = "CigarsGrid";
			this.CigarsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 177, true);
			this.CigarsGrid.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 446, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 30, true);
			this.BottomPanel.TabIndex = 19;
			// 
			// OMCForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 500, true);
			this.Controls.Add(this.DetailsSplitContainer);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.OMCHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "OMCForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OMC Edit";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.DetailsSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.US_OA_ExporterControl.ResumeLayout(true);
			this.US_OA_ExporterControl.PerformLayout();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			this.DetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).EndInit();
			this.DetailsSplitContainer.ResumeLayout(false);
			this.DetailsSplitContainer.PerformLayout();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.US_FDAQty1CalcDropEdit.ResumeLayout(true);
			this.US_FDAQty1CalcDropEdit.PerformLayout();
			this.GovOfficialCertificationDateDateEdit1.ResumeLayout(true);
			this.GovOfficialCertificationDateDateEdit1.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.ResumeLayout(true);
			this.US_OA_ResponsibleGovernmentOfficialAddressControl.PerformLayout();
			this.US_ExporterCertificationDateDateEdit.ResumeLayout(true);
			this.US_ExporterCertificationDateDateEdit.PerformLayout();
			this.US_DeclarationCodeDropEdit.ResumeLayout(true);
			this.US_DeclarationCodeDropEdit.PerformLayout();
			this.AquacultureFacilityAddressControl.ResumeLayout(true);
			this.AquacultureFacilityAddressControl.PerformLayout();
			this.US_DepartureDateDateEdit.ResumeLayout(true);
			this.US_DepartureDateDateEdit.PerformLayout();
			this.US_SourceCountryCodeFindBox.ResumeLayout(true);
			this.US_SourceCountryCodeFindBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.AdditionalAquacultureFacilitiesGroupBox.ResumeLayout(false);
			this.AdditionalAquacultureFacilitiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CigarsGrid)).EndInit();
			this.CigarsGrid.ResumeLayout(false);
			this.CigarsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCheckBox US_ElectronicImageSubmittedCheckBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZAddressControl US_OA_ExporterControl;
		private CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZGroupBox AdditionalAquacultureFacilitiesGroupBox;
		internal ZArchitecture.ZGrid CigarsGrid;
		private ZArchitecture.GUI.ZCodeFindBox US_SourceCountryCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit US_DepartureDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit US_DeclarationCodeDropEdit;
		private ZArchitecture.GUI.ZAddressControl AquacultureFacilityAddressControl;
		private ZArchitecture.GUI.ZDateEdit US_ExporterCertificationDateDateEdit;
		internal ZArchitecture.ZTextBox zTextBox3;
		internal ZArchitecture.ZTextBox zTextBox2;
		internal ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		internal ZArchitecture.ZTextBox zTextBox4;
		internal ZArchitecture.ZTextBox zTextBox5;
		internal ZArchitecture.ZTextBox zTextBox6;
		private ZArchitecture.GUI.ZAddressControl US_OA_ResponsibleGovernmentOfficialAddressControl;
		private ZArchitecture.GUI.ZDateEdit GovOfficialCertificationDateDateEdit1;
		private ZArchitecture.GUI.ZCalcDropEdit US_FDAQty1CalcDropEdit;
	}
}
