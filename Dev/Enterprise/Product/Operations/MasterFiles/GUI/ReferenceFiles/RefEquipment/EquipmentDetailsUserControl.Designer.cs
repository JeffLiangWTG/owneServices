namespace Enterprise.MasterFiles.GUI
{
	partial class EquipmentDetailsUserControl
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
			this.EquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HeaderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CapacitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CapacityInTEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RegistrationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RQ_RN_NKRegoCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RegExpiryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StateOrProvinceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RoadContainerTypeGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EquipmentGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShortCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.RegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageCountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TareWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CubicCapacityBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCapacityBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EquipmentGroupBox.SuspendLayout();
			this.CapacitiesGroupBox.SuspendLayout();
			this.RegistrationsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefEquipment);
			// 
			// EquipmentGroupBox
			// 
			this.EquipmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6d206ffc-c9c9-49d1-b5f7-5b677def890f", "Equipment Details");
			this.EquipmentGroupBox.Controls.Add(this.HeaderGuidFindBox);
			this.EquipmentGroupBox.Controls.Add(this.CapacitiesGroupBox);
			this.EquipmentGroupBox.Controls.Add(this.RegistrationsGroupBox);
			this.EquipmentGroupBox.Controls.Add(this.RoadContainerTypeGuidDropEdit);
			this.EquipmentGroupBox.Controls.Add(this.EquipmentGroupDropEdit);
			this.EquipmentGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.EquipmentGroupBox.Controls.Add(this.ShortCodeBoundTextBox);
			this.EquipmentGroupBox.Controls.Add(this.DescriptionBoundTextBox);
			this.EquipmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentGroupBox.Name = "EquipmentGroupBox";
			this.EquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 416, true);
			this.EquipmentGroupBox.TabIndex = 0;
			this.EquipmentGroupBox.TabStop = false;
			// 
			// HeaderGuidFindBox
			// 
			this.HeaderGuidFindBox.AllowDrop = true;
			this.HeaderGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.HeaderGuidFindBox, "RQ_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_OH_Owner)));
			this.HeaderGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5a3b6f36-9095-48bf-a99c-1b8fa20e92ed", "Owner");
			this.HeaderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 120, true);
			this.HeaderGuidFindBox.Name = "HeaderGuidFindBox";
			this.HeaderGuidFindBox.PreBoundMaxLength = 10;
			this.HeaderGuidFindBox.ShowDescriptionBox = false;
			this.HeaderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 21, true);
			this.HeaderGuidFindBox.TabIndex = 5;
			// 
			// CapacitiesGroupBox
			// 
			this.CapacitiesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CapacitiesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4cb52541-e9a7-4504-8ae0-fd6c7480c86a", "Capacities");
			this.CapacitiesGroupBox.Controls.Add(this.PackageCountCalcDropEdit);
			this.CapacitiesGroupBox.Controls.Add(this.TareWeightCalcDropEdit);
			this.CapacitiesGroupBox.Controls.Add(this.CapacityInTEUCalcEdit);
			this.CapacitiesGroupBox.Controls.Add(this.CubicCapacityBoundCalcDropEdit);
			this.CapacitiesGroupBox.Controls.Add(this.WeightCapacityBoundCalcDropEdit);
			this.CapacitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 266, true);
			this.CapacitiesGroupBox.Name = "CapacitiesGroupBox";
			this.CapacitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 147, true);
			this.CapacitiesGroupBox.TabIndex = 6;
			this.CapacitiesGroupBox.TabStop = false;
			// 
			// CapacityInTEUCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CapacityInTEUCalcEdit, "RQ_CapacityInTEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_CapacityInTEU)));
			this.CapacityInTEUCalcEdit.DecimalPlaces = 0;
			this.CapacityInTEUCalcEdit.Decimals = 0;
			this.CapacityInTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 119, true);
			this.CapacityInTEUCalcEdit.Name = "CapacityInTEUCalcEdit";
			this.CapacityInTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.CapacityInTEUCalcEdit.TabIndex = 4;
			this.CapacityInTEUCalcEdit.Text = "0";
			this.CapacityInTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RegistrationsGroupBox
			// 
			this.RegistrationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RegistrationsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("549d9c99-fa02-4ab8-aac6-a6ef91d4e904", "Registration");
			this.RegistrationsGroupBox.Controls.Add(this.RegistrationTextBox);
			this.RegistrationsGroupBox.Controls.Add(this.RQ_RN_NKRegoCountryCodeFindBox);
			this.RegistrationsGroupBox.Controls.Add(this.RegExpiryDateDateEdit);
			this.RegistrationsGroupBox.Controls.Add(this.StateOrProvinceDropEdit);
			this.RegistrationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 142, true);
			this.RegistrationsGroupBox.Name = "RegistrationsGroupBox";
			this.RegistrationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 118, true);
			this.RegistrationsGroupBox.TabIndex = 5;
			this.RegistrationsGroupBox.TabStop = false;
			// 
			// RQ_RN_NKRegoCountryCodeFindBox
			// 
			this.RQ_RN_NKRegoCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RQ_RN_NKRegoCountryCodeFindBox, "RQ_RN_NKRegistrationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_RN_NKRegistrationCountry)));
			this.RQ_RN_NKRegoCountryCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dcb24f7f-0f96-4480-8364-03d91516d618", "Reg.Ctry/Rgn.", "Reg. Country/Region");
			this.RQ_RN_NKRegoCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.RQ_RN_NKRegoCountryCodeFindBox.Name = "RQ_RN_NKRegoCountryCodeFindBox";
			this.RQ_RN_NKRegoCountryCodeFindBox.PreBoundMaxLength = 2;
			this.RQ_RN_NKRegoCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.RQ_RN_NKRegoCountryCodeFindBox.TabIndex = 1;
			// 
			// RegExpiryDateDateEdit
			// 
			this.RegExpiryDateDateEdit.AllowDrop = true;
			this.RegExpiryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.RegExpiryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RegExpiryDateDateEdit, "RQ_RegExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_RegExpiry)));
			this.RegExpiryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 90, true);
			this.RegExpiryDateDateEdit.Name = "RegExpiryDateDateEdit";
			this.RegExpiryDateDateEdit.TabIndex = 3;
			// 
			// StateOrProvinceDropEdit
			// 
			this.StateOrProvinceDropEdit.AllowDrop = true;
			this.StateOrProvinceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StateOrProvinceDropEdit, "RQ_RegState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_RegState)));
			this.StateOrProvinceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 64, true);
			this.StateOrProvinceDropEdit.Name = "StateOrProvinceDropEdit";
			this.StateOrProvinceDropEdit.PreBoundMaxLength = 3;
			this.StateOrProvinceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.StateOrProvinceDropEdit.TabIndex = 2;
			// 
			// RoadContainerTypeGuidDropEdit
			// 
			this.RoadContainerTypeGuidDropEdit.AllowDrop = true;
			this.RoadContainerTypeGuidDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RoadContainerTypeGuidDropEdit, "RQ_RC_RoadContainerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_RC_RoadContainerType)));
			this.RoadContainerTypeGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 40, true);
			this.RoadContainerTypeGuidDropEdit.Name = "RoadContainerTypeGuidDropEdit";
			this.RoadContainerTypeGuidDropEdit.PreBoundMaxLength = 4;
			this.RoadContainerTypeGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.RoadContainerTypeGuidDropEdit.TabIndex = 2;
			// 
			// EquipmentGroupDropEdit
			// 
			this.EquipmentGroupDropEdit.AllowDrop = true;
			this.EquipmentGroupDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EquipmentGroupDropEdit, "RQ_EquipmentGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_EquipmentGroup)));
			this.EquipmentGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 66, true);
			this.EquipmentGroupDropEdit.Name = "EquipmentGroupDropEdit";
			this.EquipmentGroupDropEdit.PreBoundMaxLength = 3;
			this.EquipmentGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.EquipmentGroupDropEdit.TabIndex = 3;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RQ_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 16, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.IsActiveCheckBox.TabIndex = 1;
			// 
			// ShortCodeBoundTextBox
			// 
			this.ShortCodeBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShortCodeBoundTextBox, "RQ_ShortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_ShortCode)));
			this.ShortCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 14, true);
			this.ShortCodeBoundTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.ShortCodeBoundTextBox.Name = "ShortCodeBoundTextBox";
			this.ShortCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.ShortCodeBoundTextBox.TabIndex = 0;
			// 
			// DescriptionBoundTextBox
			// 
			this.DescriptionBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionBoundTextBox, "RQ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_Description)));
			this.DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionBoundTextBox.GridCurrent = null;
			this.DescriptionBoundTextBox.GridMember = null;
			this.DescriptionBoundTextBox.IsMultiLine = false;
			this.DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 92, true);
			this.DescriptionBoundTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.DescriptionBoundTextBox.Name = "DescriptionBoundTextBox";
			this.DescriptionBoundTextBox.ReadOnly = false;
			this.DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.DescriptionBoundTextBox.TabIndex = 4;
			// 
			// RegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationTextBox, "RQ_Registration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_Registration)));
			this.RegistrationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
			this.RegistrationTextBox.Name = "RegistrationTextBox";
			this.RegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.RegistrationTextBox.TabIndex = 0;
			// 
			// PackageCountCalcDropEdit
			// 
			this.PackageCountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageCountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_PackCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_F3_NKPackType)));
			this.PackageCountCalcDropEdit.BindToAmount = "RQ_PackCapacity";
			this.PackageCountCalcDropEdit.BindToUnit = "RQ_F3_NKPackType";
			this.PackageCountCalcDropEdit.Decimals = 0;
			this.PackageCountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 93, true);
			this.PackageCountCalcDropEdit.Name = "PackageCountCalcDropEdit";
			this.PackageCountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.PackageCountCalcDropEdit.TabIndex = 3;
			this.PackageCountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TareWeightCalcDropEdit
			// 
			this.TareWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TareWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_TareWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_WeightUnit)));
			this.TareWeightCalcDropEdit.BindToAmount = "RQ_TareWeight";
			this.TareWeightCalcDropEdit.BindToUnit = "RQ_WeightUnit";
			this.TareWeightCalcDropEdit.Decimals = 3;
			this.TareWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.TareWeightCalcDropEdit.Name = "TareWeightCalcDropEdit";
			this.TareWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.TareWeightCalcDropEdit.TabIndex = 0;
			this.TareWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CubicCapacityBoundCalcDropEdit
			// 
			this.CubicCapacityBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CubicCapacityBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_CubicCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_CubicUnit)));
			this.CubicCapacityBoundCalcDropEdit.BindToAmount = "RQ_CubicCapacity";
			this.CubicCapacityBoundCalcDropEdit.BindToUnit = "RQ_CubicUnit";
			this.CubicCapacityBoundCalcDropEdit.Decimals = 3;
			this.CubicCapacityBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 67, true);
			this.CubicCapacityBoundCalcDropEdit.Name = "CubicCapacityBoundCalcDropEdit";
			this.CubicCapacityBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CubicCapacityBoundCalcDropEdit.TabIndex = 2;
			this.CubicCapacityBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// WeightCapacityBoundCalcDropEdit
			// 
			this.WeightCapacityBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCapacityBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_WeightCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefEquipment)(null)).RQ_WeightUnit)));
			this.WeightCapacityBoundCalcDropEdit.BindToAmount = "RQ_WeightCapacity";
			this.WeightCapacityBoundCalcDropEdit.BindToUnit = "RQ_WeightUnit";
			this.WeightCapacityBoundCalcDropEdit.Decimals = 3;
			this.WeightCapacityBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 41, true);
			this.WeightCapacityBoundCalcDropEdit.Name = "WeightCapacityBoundCalcDropEdit";
			this.WeightCapacityBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.WeightCapacityBoundCalcDropEdit.TabIndex = 1;
			this.WeightCapacityBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// EquipmentDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EquipmentGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 380, true);
			this.Name = "EquipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EquipmentGroupBox.ResumeLayout(false);
			this.EquipmentGroupBox.PerformLayout();
			this.CapacitiesGroupBox.ResumeLayout(false);
			this.CapacitiesGroupBox.PerformLayout();
			this.RegistrationsGroupBox.ResumeLayout(false);
			this.RegistrationsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox EquipmentGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox RoadContainerTypeGuidDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit PackageCountCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit EquipmentGroupDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit TareWeightCalcDropEdit;
		private ZArchitecture.ZCalcEdit CapacityInTEUCalcEdit;
		private ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private ZArchitecture.GUI.ZCalcDropEdit CubicCapacityBoundCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCapacityBoundCalcDropEdit;
		private ZArchitecture.ZTextBox ShortCodeBoundTextBox;
		private ZArchitecture.ZTranslatableTextControl DescriptionBoundTextBox;
		private ZArchitecture.GUI.ZGroupBox RegistrationsGroupBox;
		private ZArchitecture.GUI.ZDropEdit StateOrProvinceDropEdit;
		private ZArchitecture.GUI.ZDateEdit RegExpiryDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox CapacitiesGroupBox;
#if DEBUG
		internal
#endif 
		ZArchitecture.GUI.ZGuidFindBox HeaderGuidFindBox;
		private ZArchitecture.GUI.ZCodeFindBox RQ_RN_NKRegoCountryCodeFindBox;
		private ZArchitecture.ZTextBox RegistrationTextBox;

	}
}
