using System;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGlobalChargeCodeForm
	{
		new void InitializeComponent()
		{
			this.LocalChargeCodesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeCodeTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ChargeCodeTabControl
			// 
			this.ChargeCodeTabControl.Controls.Add(this.LocalChargeCodesTab);
			this.ChargeCodeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 473, true);
			this.ChargeCodeTabControl.Controls.SetChildIndex(this.LocalChargeCodesTab, 0);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 660, true);
			// 
			// LocalChargeCodesTab
			// 
			this.LocalChargeCodesTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b85aa590-074f-4450-b06f-623e1bf6fc34", "Local Charge Codes");
			this.LocalChargeCodesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LocalChargeCodesTab.Name = "LocalChargeCodesTab";
			this.LocalChargeCodesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 450, true);
			this.LocalChargeCodesTab.TabIndex = 8;
			this.LocalChargeCodesTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.LocalChargeCodesTab_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).Company.GC_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).Company.GC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AG_AccrualAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AG_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AG_RevenueAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AG_WIPAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AllowDescriptionOvertype)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AR_ExpenseGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AR_SalesGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AT_GSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AW_WithholdingTaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_AX_TaxOverrideGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ChargeOtherGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ChargeSubGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_DepartmentFilterList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_DescMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ENettChargeCodeMap)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_GoodsServiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_IATA_ChargeCodeMap)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_IsCommissionable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_IsGroupageCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_LocalLanguageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_MarginPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_PrintSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_RateCalculator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_ShowOnQuotation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).AC_SuppressOnQuoteIfZero)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).ChildChargeCodes)).SyncRoot)).IsDifferentToGlobalChargeCode)));
			// 
			// AccGlobalChargeCodeForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccChargeCodeForm|c1c5364c-3497-43c7-8133-c064572bc30d", "Global Charge Code");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 684, true);
			this.Name = "AccGlobalChargeCodeForm";
			this.ChargeCodeTabControl.ResumeLayout(false);
			this.ChargeCodeTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void LocalChargeCodesTab_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.LocalChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LocalChargeCodesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalChargeCodesGrid)).BeginInit();
			this.LocalChargeCodesGrid.SuspendLayout();
			this.LocalChargeCodesTab.Controls.Add(this.LocalChargeCodesGrid);
			// 
			// LocalChargeCodesGrid
			// 
			this.LocalChargeCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocalChargeCodesGrid, "ChildChargeCodes");
			this.LocalChargeCodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Company+GC_Name";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGlobalChargeCodeForm|d9762796-6eaa-4699-bd62-fce9a4e93e7e", "Company");
			zTextBoxColumnStyleInfo2.ColumnName = "Company+GC_Code";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "AC_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AC_AG_AccrualAccount";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AC_AG_CostAccount";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AC_AG_RevenueAccount";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AC_AG_WIPAccount";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "AC_AllowDescriptionOvertype";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AC_AR_ExpenseGroup";
			zGuidFindBoxColumnStyleInfo5.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AC_AR_SalesGroup";
			zGuidFindBoxColumnStyleInfo6.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "AC_AT_GSTRate";
			zGuidFindBoxColumnStyleInfo7.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "AC_AW_WithholdingTaxRate";
			zGuidFindBoxColumnStyleInfo8.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo8.IsVisible = false;
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "AC_AX_TaxOverrideGroup";
			zGuidFindBoxColumnStyleInfo9.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo9.IsVisible = false;
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "AC_ChargeGroup";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AC_ChargeOtherGroups";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "AC_ChargeSubGroup";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "AC_ChargeType";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "AC_DepartmentFilterList";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("27e569ba-f10a-4bfe-92c8-ea6dc808b791", "Description");
			zTextBoxColumnStyleInfo9.ColumnName = "AC_DescMultilingual";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "AC_ENettChargeCodeMap";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "AC_GoodsServiceType";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "AC_IATA_ChargeCodeMap";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "AC_IsActive";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "AC_IsCommissionable";
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "AC_IsGroupageCharge";
			zCheckBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "AC_LocalLanguageDescription";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AC_MarginPercentage";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AC_PrintSequence";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "AC_RateCalculator";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.ColumnName = "AC_ShowOnQuotation";
			zCheckBoxColumnStyleInfo5.IsReadOnly = true;
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "AC_SuppressOnQuoteIfZero";
			zCheckBoxColumnStyleInfo6.IsReadOnly = true;
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGlobalChargeCodeForm|e12c9bd6-ee77-45ca-85dc-3e5b4d99f717", "Different To Global?");
			zCheckBoxColumnStyleInfo7.ColumnName = "IsDifferentToGlobalChargeCode";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.LocalChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.LocalChargeCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalChargeCodesGrid.GridId = "51a56179-d0c0-49b0-ab53-4ae23312e932";
			this.LocalChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocalChargeCodesGrid.LayoutKey = "LocalChargeCodesGrid";
			this.LocalChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocalChargeCodesGrid.Name = "LocalChargeCodesGrid";
			this.LocalChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 450, true);
			this.LocalChargeCodesGrid.TabIndex = 0;
			this.LocalChargeCodesGrid.Initialized += new System.EventHandler(this.LocalChargeCodesGrid_Initialized);
			this.LocalChargeCodesTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalChargeCodesGrid)).EndInit();
			this.LocalChargeCodesGrid.ResumeLayout(false);
			this.LocalChargeCodesGrid.PerformLayout();
			this.LocalChargeCodesTab.ResumeLayout(true);
		}

	}
}
