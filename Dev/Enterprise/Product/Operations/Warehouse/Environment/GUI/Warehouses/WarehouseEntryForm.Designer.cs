using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class WarehouseEntryForm
	{
		ZGroupBox DetailsGroupBox;
		ZTranslatableTextControl WarehouseNameTextBox;
		ZAddressControl AddressControl;
		ZGuidFindBox RelatedCompanyBranchGuidFindBox;
		ZTabPage ParametersTab;
		ZGroupBox LocationConfigurationGroupBox;
		ZTextBox LocationComponentDelimiterTextBox;
		ZCheckBox LocationColumnsAlphaCheckBox;
		ZCheckBox LocationLevelsAlphaCheckBox;
		ZCheckBox LocationTraysAlphaCheckBox;
		ZGroupBox AutoPrintGroupBox;
		ZCheckBox AutoPrintPackingSlipCheckBox;
		ZCheckBox AutoPrintPickingSlipCheckBox;
		ZCheckBox AutoPrintPickingShortfallItemsCheckBox;
		ZCheckBox AutoPrintPickingNonPickedItemsCheckBox;
		ZCheckBox AutoPrintOrderCopyForMOPOnPickCheckBox;
		ZCheckBox AutoPrintOrderSummaryOnPickCheckBox;
		ZCheckBox LeadingZerosCheckBox;
		ZCheckBox UseArrivalDateForInwardsFinalisedDateCheckBox;
		ZCheckBox UseRequiredDateForOutwardsFinalisedDateCheckBox;
		ZGroupBox FinalisedDateGroupBox;
		ZTextBox WarehouseCodeTextBox;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox IsVirtualWarehouseCheckBox;
		ZLabel LabelTips;
		ZGroupBox WarehouseTypeGroupBox;
		ZModuleButtonGrid AreasModuleButtonGrid;
		ZGroupBox AreasGroupBox;
		ZTextBox CountryTextBox;
		ZPanel AreasPanel;
		ZCheckBox LocationColumnsZeroBasedCheckBox;
		ZCheckBox LocationTraysZeroBasedCheckBox;
		ZCheckBox LocationLevelsZeroBasedCheckBox;
		ZGroupBox PackageIDsGroupBox;
		ZCheckBox UseWarehouseGS1PrefixCheckBox;
		ZGroupBox ABCAnalysisGroupBox;
		ZTabPage WorkingHoursTabPage;
		GlbWorkTimeControl WorkTimeControl;
		ZCheckBox IncludeInABCAnalysisCheckBox;
		ZGroupBox VerifyEmptyLocationsGroupBox;
		ZDropEdit WarehouseTypeDropEdit;
		ZLabel TransactionsEnabledLabel;
		ZLabel FreeStoreLabel;
		ZLabel ExciseLabel;
		ZLabel InwardProcessingLabel;
		ZLabel VATFiscalLabel;
		ZPanel ParametersPanel;
		ZLabel BondedLabel;
		ZGroupBox TransitSecurityGroupBox;
		ZCheckBox TransitSecurityProcessingRequiredCheckBox;
		ZGroupBox TransitLoadingGroupBox;
		ZCheckBox TransitAllowPartialLoadingDefaultCheckBox;
		ZGroupBox PickPackGroupBox;
		ZGuidDropEdit WarehousePickPackPrinterGuidDropEdit;
		ZGroupBox IsPickByUOMGroupBox;
		ZCheckBox IsPickByUOMCheckBox;
		ZGroupBox DGContactGroupBox;
		ZTextBox DGPhoneNumber;
		ZDropEdit DGPhoneTypeDropEdit;
		ZGuidFindBox DGContactGuidFindBox;
		ZGroupBox DockDoorGroupBox;
		ZGuidFindBox DefaultInboundDoorFindBox;
		ZGuidFindBox DefaultOutboundDoorFindBox;
		ZGroupBox DefaultLocationTypeGroupBox;
		ZGuidFindBox DefaultLocationTypeFindBox;
		ZGroupBox DetailedTrackingGroupBox;
		ZCalcEdit DetailedTrackingWarningPercentage;
		ZDropEdit DetailedTrackingMethodDropEdit;
		ZCheckBox DetailedTrackingEnabledCheckBox;
		ZGroupBox CycleCountGroupBox;
		ZCalcEdit NumberOfCycleCountLocationToAutoAssign;
		ZGroupBox RFGroupBox;
		ZCheckBox DefaultScanAllModeCheckBox;
		ZGroupBox ReleasingGroupBox;
		ZCheckBox PreventReleaseOfPackageIfNotPickedCheckBox;
		ZCheckBox VerifyEmptyLocationsCheckBox;
		ZGroupBox TransitCustomsGroupBox;
		ZCheckBox IsPortAuthorityControlledCheckBox;
		ZCheckBox IsCustomsControlledCheckBox;
		ZGroupBox FixedWidthLocationGroupBox;
		ZCalcEdit LocationTraysFixedWidthCalcEdit;
		ZCalcEdit LocationLevelsFixedWidthCalcEdit;
		ZCalcEdit LocationColumnsFixedWidthCalcEdit;
		ZCheckBox IsFixedWidthLocationCheckBox;
		ZGroupBox VehicleBookingIntegrationGroupBox;
		ZCheckBox VehicleBookingIntegrationCheckBox;

		new void InitializeComponent()
		{
			this.ParametersTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkingHoursTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UNDGTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ParametersTab);
			this.MainTabControl.Controls.Add(this.WorkingHoursTabPage);
			this.MainTabControl.Controls.Add(this.UNDGTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.UNDGTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkingHoursTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ParametersTab, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("WarehouseEntryForm|840eaaf9-4fdc-41aa-b174-1c78de56a214", "Entry");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 630, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1227);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsWarehouse);
			// 
			// ParametersTab
			// 
			this.ParametersTab.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("WarehouseEntryForm|a608de93-7f52-4d85-b654-68a710dbbda2", "Parameters", "Here you can change the default name of your Packing Slip");
			this.ParametersTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.ParametersTab.Name = "ParametersTab";
			this.ParametersTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			this.ParametersTab.TabIndex = 4;
			this.ParametersTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.ParametersTab_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).RFPickPackPrinterPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_TransitSecurityProcessingRequired)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AllowPartialLoadingDefault)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_VerifyEmptyLocations)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_ABCAnalysisEnabled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_UseGS1PrefixFallback)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_UseArrivalDateForInwardsFinalisedDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_UseRequiredDateForOutwardsFinalisedDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationTraysZeroBased)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationLevelsZeroBased)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationColumnsZeroBased)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationsHaveLeadingZeros)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationComponentDelimiter)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationTraysAlpha)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationLevelsAlpha)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationColumnsAlpha)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintPackingSlip)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintPickingSlip)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintOrderCopyForMOPOnPick)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintOrderSummaryOnPick)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintPickingNonPickedItems)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_AutoPrintPickingShortfallItems)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsPickByUOMEnabled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_DefaultInboundDockDoor)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_WLT_DefaultLocationType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_FTZIsDetailedTrackingEnabled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_FTZDetailedTrackingMethod)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_FTZDetailedTrackingWarningPercentage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_NumberOfCycleCountLocationsToAutoAssign)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_ScanAll)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_DefaultOutboundDockDoor)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_PreventReleaseOfPackageIfNotPicked)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsCustomsControlled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsPortAuthorityControlled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationTraysFixedWidth)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationLevelsFixedWidth)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_LocationColumnsFixedWidth)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).IsFixedWidthLocation)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_UsesVehicleBookingIntegration)));
			// 
			// WorkingHoursTabPage
			// 
			this.WorkingHoursTabPage.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("WarehouseEntryForm|4036b71c-24bd-4aef-9318-1eb27615b60c", "Working Hours");
			this.WorkingHoursTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.WorkingHoursTabPage.Name = "WorkingHoursTabPage";
			this.WorkingHoursTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkingHoursTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			this.WorkingHoursTabPage.TabIndex = 5;
			this.WorkingHoursTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkingHoursTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbWorkTimeViewModel)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WorkTimeViewModel)));
			// 
			// UNDGTabPage
			// 
			this.UNDGTabPage.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("WarehouseEntryForm|d1c3d59a-43b5-42d3-96b3-8c2ef052e8fe", "UNDG Thresholds");
			this.UNDGTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 46, true);
			this.UNDGTabPage.Name = "UNDGTabPage";
			this.UNDGTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UNDGTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 580, true);
			this.UNDGTabPage.TabIndex = 6;
			this.UNDGTabPage.UseVisualStyleBackColor = true;
			this.UNDGTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UNDGTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_DGThresholdPercentage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsDangerousGoodsManagementEnabled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).Areas)));
			// 
			// 
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsVirtualWarehouse)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).CountryCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_OA_WarehouseAddress)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_WarehouseCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_GB_RelatedCompanyBranch)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_WarehouseName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_WarehouseType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).FreeStoreText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).BondedText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).ExciseText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).InwardProcessingText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_OC_DGContact)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_DGContactPhoneType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).DGPhoneNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).WW_GG_ReleaseGroup)));
			// 
			// WarehouseEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("WarehouseEntryForm|b8918dae-d63d-4d74-9a89-06b4dcdf5c13", "Warehouse");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 686, true);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Environment.Business";
			this.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsWarehouse);
			this.DataSourceTypeName = "Enterprise.Warehouse.Environment.Business.WhsWarehouse";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 725, true);
			this.Name = "WarehouseEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZTabPage UNDGTabPage;
		private ZPanel UNDGPanel;
		private ZGroupBox UNDGLimitsGroupBox;
		private ZCalcEdit DGThresholdPercentage;
		private ZLabel DGPercentLabel;
		private ZCheckBox IsDGManagementEnabledCheckBox;
		private ZGroupBox LimitsGroupBox;
		private WhsUNDGLimitControl UNDGLimitControl;
		private ZGroupBox TaskManagementGroupBox;
		private ZGuidFindBox ReleaseGroupFindBox;
	}
}
