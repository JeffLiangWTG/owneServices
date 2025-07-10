namespace Enterprise.MasterFiles.GUI
{
	public partial class AccTaxRateForm
	{
		#region Windows Form Designer generated code

		Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		Enterprise.ZArchitecture.ZTextBox AT_CodeBoundTextEdit;
		Enterprise.ZArchitecture.ZTextBox AT_DescriptionBoundTextEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		internal Enterprise.ZArchitecture.ZCalcEdit AT_RateBoundCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit AT_ExtraRateBoundCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit TaxTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AuxiliaryTaxTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox AT_A9_DefaultVatClassBoundGuidFindBox;
		internal Enterprise.ZArchitecture.ZCalcEdit PostingGroupEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox RefTaxRateGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox RefExtraTaxRateGroupBox;
		ZArchitecture.ZGrid TaxRateGrid;
		ZArchitecture.ZGrid ExtraTaxRateGrid;
		internal Enterprise.ZArchitecture.ZLabel RefDataEmptyLabel;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AT_CodeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AT_DescriptionBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AT_RateBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AT_ExtraRateBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuxiliaryTaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AT_A9_DefaultVatClassBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PostingGroupEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExtraTaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RefTaxRateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefExtraTaxRateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefDataEmptyLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.TaxTypeDropEdit.SuspendLayout();
			this.AuxiliaryTaxTypeDropEdit.SuspendLayout();
			this.AT_A9_DefaultVatClassBoundGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxRateGrid)).BeginInit();
			this.TaxRateGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraTaxRateGrid)).BeginInit();
			this.ExtraTaxRateGrid.SuspendLayout();
			this.RefTaxRateGroupBox.SuspendLayout();
			this.RefExtraTaxRateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 374, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 20, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(389);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccTaxRate);
			// 
			// AT_CodeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AT_CodeBoundTextEdit, "AT_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_Code)));
			this.AT_CodeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 7, true);
			this.AT_CodeBoundTextEdit.Name = "AT_CodeBoundTextEdit";
			this.AT_CodeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.AT_CodeBoundTextEdit.TabIndex = 0;
			// 
			// AT_DescriptionBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AT_DescriptionBoundTextEdit, "AT_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_Description)));
			this.AT_DescriptionBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AT_DescriptionBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 30, true);
			this.AT_DescriptionBoundTextEdit.Name = "AT_DescriptionBoundTextEdit";
			this.AT_DescriptionBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 17, true);
			this.AT_DescriptionBoundTextEdit.TabIndex = 4;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 349, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 25, true);
			this.ButtonsUserControl.TabIndex = 12;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "AT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 7, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.IsActiveCheckBox.TabIndex = 2;
			// 
			// AT_RateBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AT_RateBoundCalcEdit, "RateForTodayForUIBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RateForTodayForUIBinding)));
			this.AT_RateBoundCalcEdit.DecimalPlaces = 2;
			this.AT_RateBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 52, true);
			this.AT_RateBoundCalcEdit.Name = "AT_RateBoundCalcEdit";
			this.AT_RateBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.AT_RateBoundCalcEdit.TabIndex = 5;
			this.AT_RateBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AT_ExtraRateBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AT_ExtraRateBoundCalcEdit, "ExtraRateForTodayForUIBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).ExtraRateForTodayForUIBinding)));
			this.AT_ExtraRateBoundCalcEdit.DecimalPlaces = 3;
			this.AT_ExtraRateBoundCalcEdit.Decimals = 3;
			this.AT_ExtraRateBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 74, true);
			this.AT_ExtraRateBoundCalcEdit.Name = "AT_ExtraRateBoundCalcEdit";
			this.AT_ExtraRateBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.AT_ExtraRateBoundCalcEdit.TabIndex = 7;
			this.AT_ExtraRateBoundCalcEdit.Text = "0.000";
			this.AT_ExtraRateBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxTypeDropEdit
			// 
			this.TaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxTypeDropEdit, "AT_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_Type)));
			this.TaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 52, true);
			this.TaxTypeDropEdit.Name = "TaxTypeDropEdit";
			this.TaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.TaxTypeDropEdit.TabIndex = 6;
			// 
			// AuxiliaryTaxTypeDropEdit
			// 
			this.AuxiliaryTaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuxiliaryTaxTypeDropEdit, "AT_ExtraTaxRateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_ExtraTaxRateType)));
			this.AuxiliaryTaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 74, true);
			this.AuxiliaryTaxTypeDropEdit.Name = "AuxiliaryTaxTypeDropEdit";
			this.AuxiliaryTaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.AuxiliaryTaxTypeDropEdit.TabIndex = 8;
			// 
			// AT_A9_DefaultVatClassBoundGuidFindBox
			// 
			this.AT_A9_DefaultVatClassBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AT_A9_DefaultVatClassBoundGuidFindBox, "AT_A9_DefaultVatClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_A9_DefaultVatClass)));
			this.AT_A9_DefaultVatClassBoundGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AT_A9_DefaultVatClassBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 96, true);
			this.AT_A9_DefaultVatClassBoundGuidFindBox.Name = "AT_A9_DefaultVatClassBoundGuidFindBox";
			this.AT_A9_DefaultVatClassBoundGuidFindBox.PopupCaption = null;
			this.AT_A9_DefaultVatClassBoundGuidFindBox.ShouldResize = true;
			this.AT_A9_DefaultVatClassBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 17, true);
			this.AT_A9_DefaultVatClassBoundGuidFindBox.TabIndex = 9;
			// 
			// PostingGroupEdit
			// 
			this.PostingGroupEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PostingGroupEdit, "AT_PostingGroupId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).AT_PostingGroupId)));
			this.PostingGroupEdit.DecimalPlaces = 0;
			this.PostingGroupEdit.Decimals = 0;
			this.PostingGroupEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 7, true);
			this.PostingGroupEdit.MaxValue = new decimal(new int[] {
			9999,
			0,
			0,
			0 });
			this.PostingGroupEdit.Name = "PostingGroupEdit";
			this.PostingGroupEdit.ShowGroupSeparators = false;
			this.PostingGroupEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.PostingGroupEdit.TabIndex = 2;
			this.PostingGroupEdit.Text = "0";
			this.PostingGroupEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PostingGroupEdit.WordWrap = false;
			// 
			// TaxRateGrid
			// 
			this.TaxRateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxRateGrid, "RefTaxRateDataForUIBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefTaxRateDataForUIBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefTaxRateDataForUIBinding)).SyncRoot)).ZAT_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefTaxRateDataForUIBinding)).SyncRoot)).ZAT_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefTaxRateDataForUIBinding)).SyncRoot)).ZAT_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefTaxRateDataForUIBinding)).SyncRoot)).ZAT_ReferenceRateType)));
			this.TaxRateGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a14e4901-da03-400a-8078-14d9fe6fb03f", "Rate (%)");
			zCalcEditColumnStyleInfo1.ColumnName = "ZAT_Rate";
			zCalcEditColumnStyleInfo1.Decimals = 9;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("71b47ef1-ccfb-4500-baac-a3aaea759770", "Start Date");
			zTextBoxColumnStyleInfo1.ColumnName = "ZAT_StartDate";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f629450d-ef92-493d-8601-a370e0fe87e6", "End Date");
			zTextBoxColumnStyleInfo2.ColumnName = "ZAT_EndDate";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("59667040-a153-48d7-bbac-4cf37995a27b", "Reference Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ZAT_ReferenceRateType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TaxRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxRateGrid.GridId = "aed8c748-9e79-47c0-b946-f95ec478bd18";
			this.TaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxRateGrid.LayoutKey = "TaxRateGrid";
			this.TaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.TaxRateGrid.Name = "TaxRateGrid";
			this.TaxRateGrid.ReadOnly = true;
			this.TaxRateGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 83, true);
			this.TaxRateGrid.TabIndex = 11;
			// 
			// ExtraTaxRateGrid
			// 
			this.ExtraTaxRateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExtraTaxRateGrid, "RefExtraTaxRateDataForUIBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefExtraTaxRateDataForUIBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefExtraTaxRateDataForUIBinding)).SyncRoot)).ZAT_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefExtraTaxRateDataForUIBinding)).SyncRoot)).ZAT_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefExtraTaxRateDataForUIBinding)).SyncRoot)).ZAT_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAccTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccTaxRate)(null)).RefExtraTaxRateDataForUIBinding)).SyncRoot)).ZAT_ReferenceRateType)));
			this.ExtraTaxRateGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a56988db-877d-4162-a4db-8f69d2da0f22", "Rate (%)");
			zCalcEditColumnStyleInfo2.ColumnName = "ZAT_Rate";
			zCalcEditColumnStyleInfo2.Decimals = 9;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dabdf433-a2bc-4b80-9c9b-1e8174bac641", "Start Date");
			zTextBoxColumnStyleInfo4.ColumnName = "ZAT_StartDate";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("01e91733-f4b8-4108-b181-09d5df46d4d2", "End Date");
			zTextBoxColumnStyleInfo5.ColumnName = "ZAT_EndDate";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("155b7ca0-f9c8-4059-befb-cffa39db79be", "Extra Reference Type");
			zTextBoxColumnStyleInfo6.ColumnName = "ZAT_ReferenceRateType";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ExtraTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ExtraTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ExtraTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ExtraTaxRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ExtraTaxRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtraTaxRateGrid.GridId = "78c95f92-8da2-4b1a-920c-7b46194ebc3f";
			this.ExtraTaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExtraTaxRateGrid.LayoutKey = "ExtraTaxRateGrid";
			this.ExtraTaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ExtraTaxRateGrid.Name = "ExtraTaxRateGrid";
			this.ExtraTaxRateGrid.ReadOnly = true;
			this.ExtraTaxRateGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ExtraTaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 83, true);
			this.ExtraTaxRateGrid.TabIndex = 12;
			// 
			// RefTaxRateGroupBox
			// 
			this.RefTaxRateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccTaxRateForm|97bf4925-3a0d-4760-b3e7-fca55f24770e", "Tax Rate Grid");
			this.RefTaxRateGroupBox.Controls.Add(this.TaxRateGrid);
			this.RefTaxRateGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.RefTaxRateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.RefTaxRateGroupBox.Name = "RefTaxRateGroupBox";
			this.RefTaxRateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 100, true);
			this.RefTaxRateGroupBox.TabIndex = 11;
			this.RefTaxRateGroupBox.TabStop = false;
			// 
			// RefExtraTaxRateGroupBox
			// 
			this.RefExtraTaxRateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccTaxRateForm|c96ba59d-101c-42b8-b0e9-31d0dac7a847", "Extra Tax Rate Grid");
			this.RefExtraTaxRateGroupBox.Controls.Add(this.ExtraTaxRateGrid);
			this.RefExtraTaxRateGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.RefExtraTaxRateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
			this.RefExtraTaxRateGroupBox.Name = "RefExtraTaxRateGroupBox";
			this.RefExtraTaxRateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 100, true);
			this.RefExtraTaxRateGroupBox.TabIndex = 12;
			this.RefExtraTaxRateGroupBox.TabStop = false;
			// 
			// RefDataEmptyLabel
			// 
			this.RefDataEmptyLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccTaxRateForm|d129a3c5-335b-4271-8d6a-1166231414f9", "Single reference database is empty. Please contact administrator to check whether RDU service task is running.");
			this.RefDataEmptyLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.RefDataEmptyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RefDataEmptyLabel.ForeColor = System.Drawing.Color.Black;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefDataEmptyLabel, false);
			this.RefDataEmptyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 325, true);
			this.RefDataEmptyLabel.Name = "RefDataEmptyLabel";
			this.RefDataEmptyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 25, true);
			this.RefDataEmptyLabel.TabIndex = 11;
			this.RefDataEmptyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.RefDataEmptyLabel.Visible = false;
			// 
			// AccTaxRateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 395, true);
			this.Controls.Add(this.RefTaxRateGroupBox);
			this.Controls.Add(this.RefExtraTaxRateGroupBox);
			this.Controls.Add(this.RefDataEmptyLabel);
			this.Controls.Add(this.AT_A9_DefaultVatClassBoundGuidFindBox);
			this.Controls.Add(this.AuxiliaryTaxTypeDropEdit);
			this.Controls.Add(this.PostingGroupEdit);
			this.Controls.Add(this.TaxTypeDropEdit);
			this.Controls.Add(this.AT_ExtraRateBoundCalcEdit);
			this.Controls.Add(this.AT_RateBoundCalcEdit);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.ButtonsUserControl);
			this.Controls.Add(this.AT_DescriptionBoundTextEdit);
			this.Controls.Add(this.AT_CodeBoundTextEdit);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccTaxRate);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AccTaxRateForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AT_CodeBoundTextEdit, 0);
			this.Controls.SetChildIndex(this.AT_DescriptionBoundTextEdit, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.AT_RateBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.AT_ExtraRateBoundCalcEdit, 0);
			this.Controls.SetChildIndex(this.TaxTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.PostingGroupEdit, 0);
			this.Controls.SetChildIndex(this.AuxiliaryTaxTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.AT_A9_DefaultVatClassBoundGuidFindBox, 0);
			this.Controls.SetChildIndex(this.RefDataEmptyLabel, 0);
			this.Controls.SetChildIndex(this.RefExtraTaxRateGroupBox, 0);
			this.Controls.SetChildIndex(this.RefTaxRateGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.TaxTypeDropEdit.ResumeLayout(true);
			this.TaxTypeDropEdit.PerformLayout();
			this.AuxiliaryTaxTypeDropEdit.ResumeLayout(true);
			this.AuxiliaryTaxTypeDropEdit.PerformLayout();
			this.AT_A9_DefaultVatClassBoundGuidFindBox.ResumeLayout(true);
			this.AT_A9_DefaultVatClassBoundGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxRateGrid)).EndInit();
			this.TaxRateGrid.ResumeLayout(false);
			this.TaxRateGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraTaxRateGrid)).EndInit();
			this.ExtraTaxRateGrid.ResumeLayout(false);
			this.ExtraTaxRateGrid.PerformLayout();
			this.RefTaxRateGroupBox.ResumeLayout(false);
			this.RefTaxRateGroupBox.PerformLayout();
			this.RefExtraTaxRateGroupBox.ResumeLayout(false);
			this.RefExtraTaxRateGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
