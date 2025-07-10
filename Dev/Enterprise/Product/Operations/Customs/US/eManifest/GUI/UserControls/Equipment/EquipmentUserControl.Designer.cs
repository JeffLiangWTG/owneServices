namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class EquipmentUserControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.InsuranceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InsuranceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InsuranceAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InsuranceYearPolicyIssueYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.InsurancePolicyNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InsuranceNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RefEquipmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EqupmentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SealNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SealNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EmptyIITsCoveredByCarrierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RoadContainerTypeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RegistrationStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EquipmentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SealNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IITTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IITGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MerchandiseAndIITsCoveredByImporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MerchandiseAndIITsCoveredByCarrierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EmptyIITsCoveredByImporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EqupmentVINTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InsuranceTabPage.SuspendLayout();
			this.InsuranceGroupBox.SuspendLayout();
			this.InsuranceAmountCalcEdit.SuspendLayout();
			this.RefEquipmentGuidFindBox.SuspendLayout();
			this.SealNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).BeginInit();
			this.SealNumbersGrid.SuspendLayout();
			this.EquipmentGroupBox.SuspendLayout();
			this.RoadContainerTypeGuidFindBox.SuspendLayout();
			this.RegistrationStateDropEdit.SuspendLayout();
			this.RegistrationCountryFindBox.SuspendLayout();
			this.EquipmentTabControl.SuspendLayout();
			this.SealNumbersTabPage.SuspendLayout();
			this.IITTabPage.SuspendLayout();
			this.IITGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Equipment);
			// 
			// InsuranceTabPage
			// 
			this.InsuranceTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|4ec962c9-e361-4764-ad7d-f375089a40fe", "Insurance");
			this.InsuranceTabPage.Controls.Add(this.InsuranceGroupBox);
			this.InsuranceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InsuranceTabPage.Name = "InsuranceTabPage";
			this.InsuranceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InsuranceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 114, true);
			this.InsuranceTabPage.TabIndex = 2;
			// 
			// InsuranceGroupBox
			// 
			this.InsuranceGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|63464cec-6fea-4573-b4d8-85b45ef3f184", "Insurance Details");
			this.InsuranceGroupBox.Controls.Add(this.InsuranceAmountCalcEdit);
			this.InsuranceGroupBox.Controls.Add(this.InsuranceYearPolicyIssueYearEdit);
			this.InsuranceGroupBox.Controls.Add(this.InsurancePolicyNumberTextBox);
			this.InsuranceGroupBox.Controls.Add(this.InsuranceNameTextBox);
			this.InsuranceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InsuranceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InsuranceGroupBox.Name = "InsuranceGroupBox";
			this.InsuranceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 108, true);
			this.InsuranceGroupBox.TabIndex = 0;
			this.InsuranceGroupBox.TabStop = false;
			// 
			// InsuranceAmountCalcEdit
			// 
			this.InsuranceAmountCalcEdit.AllowDrop = true;
			this.InsuranceAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InsuranceAmountCalcEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_InsuranceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).EQ_InsuranceCurrency)));
			this.InsuranceAmountCalcEdit.BindToAmount = "BJ_InsuranceAmount";
			this.InsuranceAmountCalcEdit.BindToUnit = "EQ_InsuranceCurrency";
			this.InsuranceAmountCalcEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|d6bf08d7-fbe2-4720-9d5b-f31160da1942", "Amount", "Insurance amount in USD for hazardous shipment.");
			this.InsuranceAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 60, true);
			this.InsuranceAmountCalcEdit.Name = "InsuranceAmountCalcEdit";
			this.InsuranceAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 20, true);
			this.InsuranceAmountCalcEdit.TabIndex = 2;
			this.InsuranceAmountCalcEdit.UnitPreBoundMaxLength = 3;
			// 
			// InsuranceYearPolicyIssueYearEdit
			// 
			this.BindingSource.SetBindingMember(this.InsuranceYearPolicyIssueYearEdit, "BJ_InsuranceYearPolicyIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_InsuranceYearPolicyIssue)));
			this.InsuranceYearPolicyIssueYearEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|7090a6fa-4c86-49bf-b64a-0cccd1287136", "Year Policy Issue", "Insurance year policy issue for hazardous shipment.");
			this.InsuranceYearPolicyIssueYearEdit.DecimalPlaces = 0;
			this.InsuranceYearPolicyIssueYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 83, true);
			this.InsuranceYearPolicyIssueYearEdit.Name = "InsuranceYearPolicyIssueYearEdit";
			this.InsuranceYearPolicyIssueYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.InsuranceYearPolicyIssueYearEdit.TabIndex = 3;
			this.InsuranceYearPolicyIssueYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InsurancePolicyNumberTextBox
			// 
			this.InsurancePolicyNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InsurancePolicyNumberTextBox, "BJ_InsurancePolicyNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_InsurancePolicyNumber)));
			this.InsurancePolicyNumberTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|5b513565-4754-46a1-b5dc-175611c469d1", "Policy Number", "Insurance policy number for hazardous shipment.");
			this.InsurancePolicyNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 37, true);
			this.InsurancePolicyNumberTextBox.Name = "InsurancePolicyNumberTextBox";
			this.InsurancePolicyNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 20, true);
			this.InsurancePolicyNumberTextBox.TabIndex = 1;
			// 
			// InsuranceNameTextBox
			// 
			this.InsuranceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InsuranceNameTextBox, "BJ_InsuranceName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_InsuranceName)));
			this.InsuranceNameTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ConveyanceUserControl|79ad9a85-b0ea-4372-a349-f166f551812e", "Insurance Name", "Insurance name for hazardous shipment.");
			this.InsuranceNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 14, true);
			this.InsuranceNameTextBox.Name = "InsuranceNameTextBox";
			this.InsuranceNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 20, true);
			this.InsuranceNameTextBox.TabIndex = 0;
			// 
			// RefEquipmentGuidFindBox
			// 
			this.RefEquipmentGuidFindBox.AllowDrop = true;
			this.RefEquipmentGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RefEquipmentGuidFindBox, "BJ_RQ_Equipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_RQ_Equipment)));
			this.RefEquipmentGuidFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("28a96abc-74ce-4f9d-8377-f5381f61a2f0", "Equipment ID");
			this.RefEquipmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 16, true);
			this.RefEquipmentGuidFindBox.Name = "RefEquipmentGuidFindBox";
			this.RefEquipmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.RefEquipmentGuidFindBox.TabIndex = 0;
			// 
			// EqupmentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EqupmentNumberTextBox, "BJ_RegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_RegistrationNumber)));
			this.EqupmentNumberTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|e0597cd8-76ca-4df8-a590-1eeff0f05f17", "Equipment No.", "Number shown on the equipment - Mark/Initial (if applicable) + Number.");
			this.EqupmentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 40, true);
			this.EqupmentNumberTextBox.Name = "EqupmentNumberTextBox";
			this.EqupmentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.EqupmentNumberTextBox.TabIndex = 1;
			// 
			// SealNumbersGroupBox
			// 
			this.SealNumbersGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|349db67f-94c0-4bea-b2e3-ea3b59e665a1", "Seal Numbers");
			this.SealNumbersGroupBox.Controls.Add(this.SealNumbersGrid);
			this.SealNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SealNumbersGroupBox.Name = "SealNumbersGroupBox";
			this.SealNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 108, true);
			this.SealNumbersGroupBox.TabIndex = 0;
			this.SealNumbersGroupBox.TabStop = false;
			// 
			// SealNumbersGrid
			// 
			this.SealNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SealNumbersGrid, "SealNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).SealNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.SealNumber)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).SealNumbers)).SyncRoot)).CY_Data)));
			this.SealNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|c4a0b5e1-adee-46b6-8c1a-10ff6304c4ba", "Seal Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.SealNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SealNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumbersGrid.GridId = "766c6be0-268d-4083-939d-541523040b65";
			this.SealNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealNumbersGrid.LayoutKey = "SealNumbersGrid";
			this.SealNumbersGrid.LimitedColumns = null;
			this.SealNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SealNumbersGrid.Name = "SealNumbersGrid";
			this.SealNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 89, true);
			this.SealNumbersGrid.TabIndex = 0;
			// 
			// EmptyIITsCoveredByCarrierCheckBox
			// 
			this.EmptyIITsCoveredByCarrierCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EmptyIITsCoveredByCarrierCheckBox, "BJ_EmptyIITsCoveredByCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_EmptyIITsCoveredByCarrier)));
			this.EmptyIITsCoveredByCarrierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EmptyIITsCoveredByCarrierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
			this.EmptyIITsCoveredByCarrierCheckBox.Name = "EmptyIITsCoveredByCarrierCheckBox";
			this.EmptyIITsCoveredByCarrierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 17, true);
			this.EmptyIITsCoveredByCarrierCheckBox.TabIndex = 0;
			this.EmptyIITsCoveredByCarrierCheckBox.UseVisualStyleBackColor = true;
			// 
			// EquipmentGroupBox
			// 
			this.EquipmentGroupBox.Controls.Add(this.RoadContainerTypeGuidFindBox);
			this.EquipmentGroupBox.Controls.Add(this.RegistrationStateDropEdit);
			this.EquipmentGroupBox.Controls.Add(this.RegistrationCountryFindBox);
			this.EquipmentGroupBox.Controls.Add(this.EquipmentTabControl);
			this.EquipmentGroupBox.Controls.Add(this.EqupmentNumberTextBox);
			this.EquipmentGroupBox.Controls.Add(this.RefEquipmentGuidFindBox);
			this.EquipmentGroupBox.Controls.Add(this.EqupmentVINTextBox);
			this.EquipmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentGroupBox.Name = "EquipmentGroupBox";
			this.EquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 232, true);
			this.EquipmentGroupBox.TabIndex = 0;
			// 
			// RoadContainerTypeGuidFindBox
			// 
			this.RoadContainerTypeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoadContainerTypeGuidFindBox, "BJ_RC_RoadContainerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_RC_RoadContainerType)));
			this.RoadContainerTypeGuidFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ace4a56e-3bd8-4ed6-9c50-f5012d04aed8", "Equipment Type", "Road Container Type");
			this.RoadContainerTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 62, true);
			this.RoadContainerTypeGuidFindBox.Name = "RoadContainerTypeGuidFindBox";
			this.RoadContainerTypeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.RoadContainerTypeGuidFindBox.TabIndex = 4;
			// 
			// RegistrationStateDropEdit
			// 
			this.RegistrationStateDropEdit.AllowDrop = true;
			this.RegistrationStateDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RegistrationStateDropEdit, "BJ_RW_NKRegistrationState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_RW_NKRegistrationState)));
			this.RegistrationStateDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|af4f7cb8-2b16-40ec-92c4-7bbcb4439b82", "State");
			this.RegistrationStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 40, true);
			this.RegistrationStateDropEdit.Name = "RegistrationStateDropEdit";
			this.RegistrationStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.RegistrationStateDropEdit.TabIndex = 3;
			// 
			// RegistrationCountryFindBox
			// 
			this.RegistrationCountryFindBox.AllowDrop = true;
			this.RegistrationCountryFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RegistrationCountryFindBox, "BJ_RN_NKRegistrationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_RN_NKRegistrationCountry)));
			this.RegistrationCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 40, true);
			this.RegistrationCountryFindBox.Name = "RegistrationCountryFindBox";
			this.RegistrationCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.RegistrationCountryFindBox.TabIndex = 2;
			// 
			// EquipmentTabControl
			// 
			this.EquipmentTabControl.Controls.Add(this.InsuranceTabPage);
			this.EquipmentTabControl.Controls.Add(this.SealNumbersTabPage);
			this.EquipmentTabControl.Controls.Add(this.IITTabPage);
			this.EquipmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 88, true);
			this.EquipmentTabControl.Name = "EquipmentTabControl";
			this.EquipmentTabControl.SelectedIndex = 0;
			this.EquipmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 141, true);
			this.EquipmentTabControl.TabIndex = 6;
			// 
			// SealNumbersTabPage
			// 
			this.SealNumbersTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|a4e5533e-d455-4cff-a5c2-3b19605546ec", "Seal Numbers");
			this.SealNumbersTabPage.Controls.Add(this.SealNumbersGroupBox);
			this.SealNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SealNumbersTabPage.Name = "SealNumbersTabPage";
			this.SealNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SealNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 114, true);
			this.SealNumbersTabPage.TabIndex = 0;
			// 
			// IITTabPage
			// 
			this.IITTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|14cf9f79-f036-4ad7-857b-8af052a2e2a8", "Instruments of International Traffic");
			this.IITTabPage.Controls.Add(this.IITGroupBox);
			this.IITTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IITTabPage.Name = "IITTabPage";
			this.IITTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IITTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 114, true);
			this.IITTabPage.TabIndex = 1;
			// 
			// IITGroupBox
			// 
			this.IITGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("EquipmentUserControl|6076c017-333b-42a3-b156-52b74ede2d32", "Instruments of International Traffic");
			this.IITGroupBox.Controls.Add(this.MerchandiseAndIITsCoveredByImporterCheckBox);
			this.IITGroupBox.Controls.Add(this.MerchandiseAndIITsCoveredByCarrierCheckBox);
			this.IITGroupBox.Controls.Add(this.EmptyIITsCoveredByImporterCheckBox);
			this.IITGroupBox.Controls.Add(this.EmptyIITsCoveredByCarrierCheckBox);
			this.IITGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IITGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IITGroupBox.Name = "IITGroupBox";
			this.IITGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 108, true);
			this.IITGroupBox.TabIndex = 0;
			this.IITGroupBox.TabStop = false;
			// 
			// MerchandiseAndIITsCoveredByImporterCheckBox
			// 
			this.MerchandiseAndIITsCoveredByImporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MerchandiseAndIITsCoveredByImporterCheckBox, "BJ_MerchandiseAndIITsCoveredByImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_MerchandiseAndIITsCoveredByImporter)));
			this.MerchandiseAndIITsCoveredByImporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MerchandiseAndIITsCoveredByImporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 77, true);
			this.MerchandiseAndIITsCoveredByImporterCheckBox.Name = "MerchandiseAndIITsCoveredByImporterCheckBox";
			this.MerchandiseAndIITsCoveredByImporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 17, true);
			this.MerchandiseAndIITsCoveredByImporterCheckBox.TabIndex = 3;
			this.MerchandiseAndIITsCoveredByImporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// MerchandiseAndIITsCoveredByCarrierCheckBox
			// 
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MerchandiseAndIITsCoveredByCarrierCheckBox, "BJ_MerchandiseAndIITsCoveredByCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_MerchandiseAndIITsCoveredByCarrier)));
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 57, true);
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.Name = "MerchandiseAndIITsCoveredByCarrierCheckBox";
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 17, true);
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.TabIndex = 2;
			this.MerchandiseAndIITsCoveredByCarrierCheckBox.UseVisualStyleBackColor = true;
			// 
			// EmptyIITsCoveredByImporterCheckBox
			// 
			this.EmptyIITsCoveredByImporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EmptyIITsCoveredByImporterCheckBox, "BJ_EmptyIITsCoveredByImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_EmptyIITsCoveredByImporter)));
			this.EmptyIITsCoveredByImporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EmptyIITsCoveredByImporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 37, true);
			this.EmptyIITsCoveredByImporterCheckBox.Name = "EmptyIITsCoveredByImporterCheckBox";
			this.EmptyIITsCoveredByImporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.EmptyIITsCoveredByImporterCheckBox.TabIndex = 1;
			this.EmptyIITsCoveredByImporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// EqupmentVINTextBox
			// 
			this.EqupmentVINTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EqupmentVINTextBox, "BJ_VIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(null)).BJ_VIN)));
			this.EqupmentVINTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("d239c390-d8a3-463a-8537-0bedefd224ab", "VIN");
			this.EqupmentVINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 62, true);
			this.EqupmentVINTextBox.Name = "EqupmentVINTextBox";
			this.EqupmentVINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.EqupmentVINTextBox.TabIndex = 5;
			// 
			// EquipmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EquipmentGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 222, true);
			this.Name = "EquipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 232, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InsuranceTabPage.ResumeLayout(false);
			this.InsuranceTabPage.PerformLayout();
			this.InsuranceGroupBox.ResumeLayout(false);
			this.InsuranceGroupBox.PerformLayout();
			this.InsuranceAmountCalcEdit.ResumeLayout(true);
			this.InsuranceAmountCalcEdit.PerformLayout();
			this.RefEquipmentGuidFindBox.ResumeLayout(true);
			this.RefEquipmentGuidFindBox.PerformLayout();
			this.SealNumbersGroupBox.ResumeLayout(false);
			this.SealNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealNumbersGrid)).EndInit();
			this.SealNumbersGrid.ResumeLayout(false);
			this.SealNumbersGrid.PerformLayout();
			this.EquipmentGroupBox.ResumeLayout(false);
			this.EquipmentGroupBox.PerformLayout();
			this.RoadContainerTypeGuidFindBox.ResumeLayout(true);
			this.RoadContainerTypeGuidFindBox.PerformLayout();
			this.RegistrationStateDropEdit.ResumeLayout(true);
			this.RegistrationStateDropEdit.PerformLayout();
			this.RegistrationCountryFindBox.ResumeLayout(true);
			this.RegistrationCountryFindBox.PerformLayout();
			this.EquipmentTabControl.ResumeLayout(false);
			this.EquipmentTabControl.PerformLayout();
			this.SealNumbersTabPage.ResumeLayout(false);
			this.SealNumbersTabPage.PerformLayout();
			this.IITTabPage.ResumeLayout(false);
			this.IITTabPage.PerformLayout();
			this.IITGroupBox.ResumeLayout(false);
			this.IITGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		private ZArchitecture.GUI.ZTabPage InsuranceTabPage;
		private ZArchitecture.GUI.ZGroupBox InsuranceGroupBox;
		private ZArchitecture.ZTextBox InsuranceNameTextBox;
		private ZArchitecture.ZTextBox InsurancePolicyNumberTextBox;
		private ZArchitecture.GUI.ZYearEdit InsuranceYearPolicyIssueYearEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InsuranceAmountCalcEdit;
		protected ZArchitecture.GUI.ZGuidFindBox RefEquipmentGuidFindBox;
		protected ZArchitecture.ZTextBox EqupmentNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox SealNumbersGroupBox;
		private ZArchitecture.GUI.ZCheckBox EmptyIITsCoveredByCarrierCheckBox;
		private ZArchitecture.GUI.ZGroupBox IITGroupBox;
		private ZArchitecture.GUI.ZCheckBox MerchandiseAndIITsCoveredByImporterCheckBox;
		private ZArchitecture.GUI.ZCheckBox MerchandiseAndIITsCoveredByCarrierCheckBox;
		private ZArchitecture.GUI.ZCheckBox EmptyIITsCoveredByImporterCheckBox;
		private ZArchitecture.ZGrid SealNumbersGrid;
		private ZArchitecture.GUI.ZTabPage SealNumbersTabPage;
		private ZArchitecture.GUI.ZTabPage IITTabPage;
		protected ZArchitecture.GUI.ZPanel EquipmentGroupBox;
		protected ZArchitecture.GUI.ZTabControl EquipmentTabControl;
		protected ZArchitecture.GUI.ZCodeFindBox RegistrationCountryFindBox;
		protected ZArchitecture.GUI.ZGuidFindBox RoadContainerTypeGuidFindBox;
		protected ZArchitecture.GUI.ZDropEdit RegistrationStateDropEdit;
		protected ZArchitecture.ZTextBox EqupmentVINTextBox;
	}
}
