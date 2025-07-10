namespace Enterprise.MasterFiles.GUI
{
	partial class WhsProductPrefsUserControl
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
			UnHookEvents();

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
			this.UseSerialNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UsePackingDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseExpiryDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PartAttributeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartAttributeType3GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName3TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.PartAttributeType2GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName2TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.PartAttributeType1GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName1TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.OM_WhsDefaultWarehousePickModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PickModeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_WhsDefaultWarehouseRollUpCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ABCAnalysisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ABCAnalysisMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ABCAnalysisPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncludeClientInABCAnalysisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DefaultExpiryNotificationPeriodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefaultExpiryNotificationPeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartAttributeGroupBox.SuspendLayout();
			this.PartAttributeType3GuidFindBox.SuspendLayout();
			this.PartAttributeName3TextBox.SuspendLayout();
			this.PartAttributeType2GuidFindBox.SuspendLayout();
			this.PartAttributeName2TextBox.SuspendLayout();
			this.PartAttributeType1GuidFindBox.SuspendLayout();
			this.PartAttributeName1TextBox.SuspendLayout();
			this.OM_WhsDefaultWarehousePickModeDropEdit.SuspendLayout();
			this.PickModeGroupBox.SuspendLayout();
			this.ABCAnalysisGroupBox.SuspendLayout();
			this.ABCAnalysisMethodDropEdit.SuspendLayout();
			this.ABCAnalysisPeriodDropEdit.SuspendLayout();
			this.DefaultExpiryNotificationPeriodGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// UseSerialNumberCheckBox
			// 
			this.UseSerialNumberCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseSerialNumberCheckBox, "MiscServ.OM_IMUseSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMUseSerialNumber)));
			this.UseSerialNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseSerialNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 106, true);
			this.UseSerialNumberCheckBox.Name = "UseSerialNumberCheckBox";
			this.UseSerialNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.UseSerialNumberCheckBox.TabIndex = 13;
			// 
			// UsePackingDateCheckBox
			// 
			this.UsePackingDateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UsePackingDateCheckBox, "MiscServ+OM_IMUsePackingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMUsePackingDate)));
			this.UsePackingDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsePackingDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 106, true);
			this.UsePackingDateCheckBox.Name = "UsePackingDateCheckBox";
			this.UsePackingDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.UsePackingDateCheckBox.TabIndex = 15;
			// 
			// UseExpiryDateCheckBox
			// 
			this.UseExpiryDateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseExpiryDateCheckBox, "MiscServ+OM_IMUseExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMUseExpiryDate)));
			this.UseExpiryDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseExpiryDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 106, true);
			this.UseExpiryDateCheckBox.Name = "UseExpiryDateCheckBox";
			this.UseExpiryDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.UseExpiryDateCheckBox.TabIndex = 14;
			// 
			// PartAttributeGroupBox
			// 
			this.PartAttributeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsProductPrefsUserControl|f07503f5-9710-43d4-a49c-0484aff7eae6", "Part Attribute Rules");
			this.PartAttributeGroupBox.Controls.Add(this.UsePackingDateCheckBox);
			this.PartAttributeGroupBox.Controls.Add(this.UseSerialNumberCheckBox);
			this.PartAttributeGroupBox.Controls.Add(this.UseExpiryDateCheckBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType3GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName3TextBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType2GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName2TextBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType1GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName1TextBox);
			this.PartAttributeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.PartAttributeGroupBox.Name = "PartAttributeGroupBox";
			this.PartAttributeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 134, true);
			this.PartAttributeGroupBox.TabIndex = 0;
			this.PartAttributeGroupBox.TabStop = false;
			// 
			// PartAttributeType3GuidFindBox
			// 
			this.PartAttributeType3GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType3GuidFindBox, "MiscServ+OM_IMPartAttrib3Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib3Type)));
			this.PartAttributeType3GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 76, true);
			this.PartAttributeType3GuidFindBox.Name = "PartAttributeType3GuidFindBox";
			this.PartAttributeType3GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType3GuidFindBox.ShouldResizeByMaxLength = true;
			this.PartAttributeType3GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType3GuidFindBox.TabIndex = 11;
			// 
			// PartAttributeName3TextBox
			// 
			this.PartAttributeName3TextBox.AcceptsReturn = false;
			this.PartAttributeName3TextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeName3TextBox, "MiscServ+OM_IMPartAttrib3Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib3Name)));
			this.PartAttributeName3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName3TextBox.GridCurrent = null;
			this.PartAttributeName3TextBox.GridMember = null;
			this.PartAttributeName3TextBox.IsLanguageEditingEnabled = true;
			this.PartAttributeName3TextBox.IsMultiLine = false;
			this.PartAttributeName3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 76, true);
			this.PartAttributeName3TextBox.Name = "PartAttributeName3TextBox";
			this.PartAttributeName3TextBox.ReadOnly = false;
			this.PartAttributeName3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName3TextBox.TabIndex = 9;
			// 
			// PartAttributeType2GuidFindBox
			// 
			this.PartAttributeType2GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType2GuidFindBox, "MiscServ+OM_IMPartAttrib2Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib2Type)));
			this.PartAttributeType2GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 50, true);
			this.PartAttributeType2GuidFindBox.Name = "PartAttributeType2GuidFindBox";
			this.PartAttributeType2GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType2GuidFindBox.ShouldResizeByMaxLength = true;
			this.PartAttributeType2GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType2GuidFindBox.TabIndex = 7;
			// 
			// PartAttributeName2TextBox
			// 
			this.PartAttributeName2TextBox.AcceptsReturn = false;
			this.PartAttributeName2TextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeName2TextBox, "MiscServ+OM_IMPartAttrib2Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib2Name)));
			this.PartAttributeName2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName2TextBox.GridCurrent = null;
			this.PartAttributeName2TextBox.GridMember = null;
			this.PartAttributeName2TextBox.IsLanguageEditingEnabled = true;
			this.PartAttributeName2TextBox.IsMultiLine = false;
			this.PartAttributeName2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 50, true);
			this.PartAttributeName2TextBox.Name = "PartAttributeName2TextBox";
			this.PartAttributeName2TextBox.ReadOnly = false;
			this.PartAttributeName2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName2TextBox.TabIndex = 5;
			// 
			// PartAttributeType1GuidFindBox
			// 
			this.PartAttributeType1GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType1GuidFindBox, "MiscServ+OM_IMPartAttrib1Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib1Type)));
			this.PartAttributeType1GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 24, true);
			this.PartAttributeType1GuidFindBox.Name = "PartAttributeType1GuidFindBox";
			this.PartAttributeType1GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType1GuidFindBox.ShouldResizeByMaxLength = true;
			this.PartAttributeType1GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType1GuidFindBox.TabIndex = 3;
			// 
			// PartAttributeName1TextBox
			// 
			this.PartAttributeName1TextBox.AcceptsReturn = false;
			this.PartAttributeName1TextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeName1TextBox, "MiscServ+OM_IMPartAttrib1Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib1Name)));
			this.PartAttributeName1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName1TextBox.GridCurrent = null;
			this.PartAttributeName1TextBox.GridMember = null;
			this.PartAttributeName1TextBox.IsLanguageEditingEnabled = true;
			this.PartAttributeName1TextBox.IsMultiLine = false;
			this.PartAttributeName1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.PartAttributeName1TextBox.Name = "PartAttributeName1TextBox";
			this.PartAttributeName1TextBox.ReadOnly = false;
			this.PartAttributeName1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName1TextBox.TabIndex = 1;
			// 
			// OM_WhsDefaultWarehousePickModeDropEdit
			// 
			this.OM_WhsDefaultWarehousePickModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_WhsDefaultWarehousePickModeDropEdit, "MiscServ.OM_WhsDefaultWarehousePickMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsDefaultWarehousePickMode)));
			this.OM_WhsDefaultWarehousePickModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 19, true);
			this.OM_WhsDefaultWarehousePickModeDropEdit.Name = "OM_WhsDefaultWarehousePickModeDropEdit";
			this.OM_WhsDefaultWarehousePickModeDropEdit.PreBoundMaxLength = 3;
			this.OM_WhsDefaultWarehousePickModeDropEdit.ShouldResizeByMaxLength = true;
			this.OM_WhsDefaultWarehousePickModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.OM_WhsDefaultWarehousePickModeDropEdit.TabIndex = 2;
			// 
			// PickModeGroupBox
			// 
			this.PickModeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsProductPrefsUserControl|27c2ae18-7d7e-4c4d-97e6-cdd2545bdf44", "Picking Defaults");
			this.PickModeGroupBox.Controls.Add(this.OM_WhsDefaultWarehouseRollUpCheckBox);
			this.PickModeGroupBox.Controls.Add(this.OM_WhsDefaultWarehousePickModeDropEdit);
			this.PickModeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 148, true);
			this.PickModeGroupBox.Name = "PickModeGroupBox";
			this.PickModeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 75, true);
			this.PickModeGroupBox.TabIndex = 3;
			this.PickModeGroupBox.TabStop = false;
			// 
			// OM_WhsDefaultWarehouseRollUpCheckBox
			// 
			this.OM_WhsDefaultWarehouseRollUpCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_WhsDefaultWarehouseRollUpCheckBox, "MiscServ.OM_WhsDefaultWarehouseRollUp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsDefaultWarehouseRollUp)));
			this.OM_WhsDefaultWarehouseRollUpCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_WhsDefaultWarehouseRollUpCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_WhsDefaultWarehouseRollUpCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 50, true);
			this.OM_WhsDefaultWarehouseRollUpCheckBox.Name = "OM_WhsDefaultWarehouseRollUpCheckBox";
			this.OM_WhsDefaultWarehouseRollUpCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OM_WhsDefaultWarehouseRollUpCheckBox.TabIndex = 3;
			// 
			// ABCAnalysisGroupBox
			// 
			this.ABCAnalysisGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d030556a-8502-4457-a59a-f8e89fe961c1", "ABC Analysis");
			this.ABCAnalysisGroupBox.Controls.Add(this.ABCAnalysisMethodDropEdit);
			this.ABCAnalysisGroupBox.Controls.Add(this.ABCAnalysisPeriodDropEdit);
			this.ABCAnalysisGroupBox.Controls.Add(this.IncludeClientInABCAnalysisCheckBox);
			this.ABCAnalysisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 229, true);
			this.ABCAnalysisGroupBox.Name = "ABCAnalysisGroupBox";
			this.ABCAnalysisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 102, true);
			this.ABCAnalysisGroupBox.TabIndex = 4;
			this.ABCAnalysisGroupBox.TabStop = false;
			// 
			// ABCAnalysisMethodDropEdit
			// 
			this.ABCAnalysisMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ABCAnalysisMethodDropEdit, "MiscServ.OM_WhsABCAnalysisMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsABCAnalysisMethod)));
			this.ABCAnalysisMethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("efd8df9a-b799-423c-ba6d-fe09f59fff2a", "ABC Analysis Method");
			this.ABCAnalysisMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 68, true);
			this.ABCAnalysisMethodDropEdit.Name = "ABCAnalysisMethodDropEdit";
			this.ABCAnalysisMethodDropEdit.PreBoundMaxLength = 3;
			this.ABCAnalysisMethodDropEdit.ShouldResizeByMaxLength = true;
			this.ABCAnalysisMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ABCAnalysisMethodDropEdit.TabIndex = 5;
			// 
			// ABCAnalysisPeriodDropEdit
			// 
			this.ABCAnalysisPeriodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ABCAnalysisPeriodDropEdit, "MiscServ.OM_WhsABCAnalysisPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsABCAnalysisPeriod)));
			this.ABCAnalysisPeriodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("043669a1-b64d-4210-93d9-e6beefdb56c3", "ABC Analysis Period");
			this.ABCAnalysisPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 42, true);
			this.ABCAnalysisPeriodDropEdit.Name = "ABCAnalysisPeriodDropEdit";
			this.ABCAnalysisPeriodDropEdit.PreBoundMaxLength = 3;
			this.ABCAnalysisPeriodDropEdit.ShouldResizeByMaxLength = true;
			this.ABCAnalysisPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ABCAnalysisPeriodDropEdit.TabIndex = 4;
			// 
			// IncludeClientInABCAnalysisCheckBox
			// 
			this.IncludeClientInABCAnalysisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeClientInABCAnalysisCheckBox, "MiscServ.OM_WhsABCAnalysisEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsABCAnalysisEnabled)));
			this.IncludeClientInABCAnalysisCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("45fa3c17-d8e9-4583-82d2-2ed4fce4958f", "Include Client in ABC Analysis");
			this.IncludeClientInABCAnalysisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeClientInABCAnalysisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.IncludeClientInABCAnalysisCheckBox.Name = "IncludeClientInABCAnalysisCheckBox";
			this.IncludeClientInABCAnalysisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.IncludeClientInABCAnalysisCheckBox.TabIndex = 1;
			// 
			// DefaultExpiryNotificationPeriodGroupBox
			// 
			this.DefaultExpiryNotificationPeriodGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("998a15a4-d052-475a-9f06-c59d150bb674", "Expiry Notification Period");
			this.DefaultExpiryNotificationPeriodGroupBox.Controls.Add(this.DefaultExpiryNotificationPeriodCalcEdit);
			this.DefaultExpiryNotificationPeriodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 337, true);
			this.DefaultExpiryNotificationPeriodGroupBox.Name = "DefaultExpiryNotificationPeriodGroupBox";
			this.DefaultExpiryNotificationPeriodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 62, true);
			this.DefaultExpiryNotificationPeriodGroupBox.TabIndex = 3;
			this.DefaultExpiryNotificationPeriodGroupBox.TabStop = false;
			// 
			// DefaultExpiryNotificationPeriodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultExpiryNotificationPeriodCalcEdit, "MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays)));
			this.DefaultExpiryNotificationPeriodCalcEdit.CaptionResourceString = null;
			this.DefaultExpiryNotificationPeriodCalcEdit.DecimalPlaces = 0;
			this.DefaultExpiryNotificationPeriodCalcEdit.Decimals = 0;
			this.DefaultExpiryNotificationPeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 19, true);
			this.DefaultExpiryNotificationPeriodCalcEdit.Name = "DefaultExpiryNotificationPeriodCalcEdit";
			this.DefaultExpiryNotificationPeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.DefaultExpiryNotificationPeriodCalcEdit.TabIndex = 2;
			this.DefaultExpiryNotificationPeriodCalcEdit.Text = "0";
			this.DefaultExpiryNotificationPeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WhsProductPrefsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultExpiryNotificationPeriodGroupBox);
			this.Controls.Add(this.ABCAnalysisGroupBox);
			this.Controls.Add(this.PickModeGroupBox);
			this.Controls.Add(this.PartAttributeGroupBox);
			this.Name = "WhsProductPrefsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 455, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartAttributeGroupBox.ResumeLayout(false);
			this.PartAttributeGroupBox.PerformLayout();
			this.PartAttributeType3GuidFindBox.ResumeLayout(true);
			this.PartAttributeType3GuidFindBox.PerformLayout();
			this.PartAttributeName3TextBox.ResumeLayout(true);
			this.PartAttributeName3TextBox.PerformLayout();
			this.PartAttributeType2GuidFindBox.ResumeLayout(true);
			this.PartAttributeType2GuidFindBox.PerformLayout();
			this.PartAttributeName2TextBox.ResumeLayout(true);
			this.PartAttributeName2TextBox.PerformLayout();
			this.PartAttributeType1GuidFindBox.ResumeLayout(true);
			this.PartAttributeType1GuidFindBox.PerformLayout();
			this.PartAttributeName1TextBox.ResumeLayout(true);
			this.PartAttributeName1TextBox.PerformLayout();
			this.OM_WhsDefaultWarehousePickModeDropEdit.ResumeLayout(true);
			this.OM_WhsDefaultWarehousePickModeDropEdit.PerformLayout();
			this.PickModeGroupBox.ResumeLayout(false);
			this.PickModeGroupBox.PerformLayout();
			this.ABCAnalysisGroupBox.ResumeLayout(false);
			this.ABCAnalysisGroupBox.PerformLayout();
			this.ABCAnalysisMethodDropEdit.ResumeLayout(true);
			this.ABCAnalysisMethodDropEdit.PerformLayout();
			this.ABCAnalysisPeriodDropEdit.ResumeLayout(true);
			this.ABCAnalysisPeriodDropEdit.PerformLayout();
			this.DefaultExpiryNotificationPeriodGroupBox.ResumeLayout(false);
			this.DefaultExpiryNotificationPeriodGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		Enterprise.ZArchitecture.GUI.ZCheckBox UsePackingDateCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox UseExpiryDateCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox PartAttributeGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType3GuidFindBox;
		Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName3TextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType2GuidFindBox;
		Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName2TextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType1GuidFindBox;
		Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName1TextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit OM_WhsDefaultWarehousePickModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox PickModeGroupBox;
		ZArchitecture.GUI.ZCheckBox OM_WhsDefaultWarehouseRollUpCheckBox;
		ZArchitecture.GUI.ZGroupBox ABCAnalysisGroupBox;
		internal ZArchitecture.GUI.ZCheckBox IncludeClientInABCAnalysisCheckBox;
		internal ZArchitecture.GUI.ZDropEdit ABCAnalysisMethodDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ABCAnalysisPeriodDropEdit;
		internal ZArchitecture.GUI.ZCheckBox UseSerialNumberCheckBox;
		private ZArchitecture.GUI.ZGroupBox DefaultExpiryNotificationPeriodGroupBox;
		private ZArchitecture.ZCalcEdit DefaultExpiryNotificationPeriodCalcEdit;
	}
}
