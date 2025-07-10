using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolUserControl
	{
		private System.ComponentModel.IContainer components = null;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DepartureArrivalTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.OrgsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JK_ConsolCutOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChargeableUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChargeableCheckCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentCountCheckCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendingForwarderAddressControl = new ZOrgAddressWithContactInfoControl();
			this.ReceivingForwarderAddressControl = new ZOrgAddressWithContactInfoControl();
			this.DangerousGoodsControl = new Enterprise.Freight.Forwarding.GUI.DangerousGoodsControl();
			this.Commodity = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConsolMaxDimsControl = new Enterprise.Freight.Forwarding.GUI.ConsolMaxDimsControl();
			this.JK_BookingReferenceBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_CoLoadMasterBillBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_AgentsReferenceBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_CarrierContractNumberBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierContractImportButton = new Enterprise.ZArchitecture.GUI.ZButton.Bare();
			this.JK_CarrierContractNumberFindBox = new Enterprise.Freight.Forwarding.GUI.ConsolContractAllocationCodeFindBox();
			this.JK_RCA_AllocationRouteCodeFindBox = new Enterprise.Freight.Forwarding.GUI.ContractAllocationGuidFindBox();
			this.JK_CoLoadBookingReferenceBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonSelectFromConsortium = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.JK_OA_CreditorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_ShippingLineAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DepartureTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepartureTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DepartureDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JK_DateFirstForeignPortDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_DateLastForeignPortDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LastForeignPortBoundCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CFSDepartureByTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CFSArrivalByTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.departurePartialEventsInfoControl = new Enterprise.Freight.Forwarding.GUI.PartialEventsInfoControl();
			this.arrivalPartialEventsInfoControl = new Enterprise.Freight.Forwarding.GUI.PartialEventsInfoControl();
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_OA_ContainerYardEmptyPickupAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_DepartureCTOAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_PackDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ArrivalTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ArrivalTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ArrivalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JK_DatePortOfFirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_DateLastForeignPortDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_OA_ContainerYardEmptyReturnAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_UnpackDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_ArrivalCTOAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreAllocationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AchievedQuantitiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsolChargeableUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CorrectedConsolVolumeUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CorrectedConsolWeightUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CorrectedConsolVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CorrectedConsolWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_NoCopyBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_NoOriginalBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeightUtilisationPercentageBar = new LabelledPercentageBar();
			this.VolumeUtilisationPercentageBar = new LabelledPercentageBar();
			this.CostFreePercentageBar = new LabelledPercentageBar();
			this.WeightUtilisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeUtilisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CostFreeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DensityFactorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DensityVisualisationControl = new DensityVisualisationControl();
			this.ExcessVolumeWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExcessVolumeWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_OverrideConsolChargeableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolChargeableRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConsolChargeableQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SecurityStatusDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.SpecialHandlingUserControl = new SpecialHandlingUserControl();
			this.JK_Calc_FreeSpaceEdit = new ZArchitecture.ZCalcEdit();
			this.JK_Calc_ActualVolumeWeightEdit = new ZArchitecture.ZCalcEdit();
			this.JK_Calc_FreeSpaceUnitTextBox = new ZArchitecture.ZTextBox();
			this.JK_Calc_ActualVolumeWeightUnitTextBox = new ZArchitecture.ZTextBox();
			this.MasterBillIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AWBDimsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackageGroupingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_PrintOptionForColoadsOnManifestDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomDatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsolCustomFields = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.JK_OA_DeparturePackCFSTransportAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JK_PrepaidCollectDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CRNPanelSea = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_MasterBillNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CRNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JK_CRNBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalShipmentWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalShipmentWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalShipmentVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalShipmentVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShowSubHouseBillsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.consolLeg1 = new Enterprise.Freight.GUI.ConsoLegDetailsControl();
			this.DetailsBottomPanel = new CargoWise.Windows.UI.KPanel();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JK_ScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.AirConsolPanel = new CargoWise.Windows.UI.KPanel();
			this.JK_AWBServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AirlinePrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBHyphenLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsNeutralCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CharterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MAWBTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBPendingAllocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MawbLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AircraftRegoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JK_AircraftRegoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsTopPanel = new CargoWise.Windows.UI.KPanel();
			this.JK_PhaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DomesticFreightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JK_AgentTypeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_RL_NKLoadPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_ConsolModeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_SendingForwarderHandlingTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JK_RL_NKDischargePortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_TotalChargeableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalChargeableUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalPrepaidChargeableAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalCollectChargeableAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalCollectChargeableAmountCurrencyCode = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentDetailsBottomPanel = new CargoWise.Windows.UI.KPanel();
			this.BottomInnerRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShipmentCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalShipmentLoadingMetersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TopPanel = new CargoWise.Windows.UI.KPanel();
			this.RightPanel = new CargoWise.Windows.UI.KPanel();
			this.ShipmentModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid();
			this.ColoadConsolModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ColoadConsolModuleButtonGrid();
			this.JK_RL_NKMasterBillIssuePlaceFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_Calc_ShipmentFreightCostChargeableTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargesApply = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemperatureControlBlock = new Enterprise.Freight.Forwarding.GUI.TemperatureControlBlock();
			this.referenceNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			this.JK_PackDepotReceiptRequestedControl = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_PackDepotDispatchRequestedControl = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_UnpackDepotReceiptRequestedControl = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_UnpackDepotDispatchRequestedControl = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureDetailsPanel = new ZArchitecture.GUI.ZPanel();
			this.ArrivalDetailsPanel = new ZArchitecture.GUI.ZPanel();
			this.LatestStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierBookingOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BillOfLadingBillTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BillOfLadingBillTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BillOfLadingBillDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BillOfLadingBillStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierBookingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CarrierBookingLatestStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoratingDateOverriddenDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalCO2eTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalCO2eUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsolDetailsBottomPanel = new CargoWise.Windows.UI.KPanel();
			this.ColoadConsolCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolWeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalColoadConsolVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolVolumeUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalColoadConsolChargeableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolChargeableUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode = new Enterprise.ZArchitecture.ZTextBox();
			this.ElectronicBillOfLadingReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewEBLButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DepartureArrivalTabControl.SuspendLayout();
			this.OrgsTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonSelectFromConsortium)).BeginInit();
			this.DepartureTabPage.SuspendLayout();
			this.DepartureTabControl.SuspendLayout();
			this.CFSDepartureByTransportModeDropEdit.SuspendLayout();
			this.CFSArrivalByTransportModeDropEdit.SuspendLayout();
			this.DepartureDetailsTabPage.SuspendLayout();
			this.ArrivalTabPage.SuspendLayout();
			this.ArrivalTabControl.SuspendLayout();
			this.ArrivalDetailsTabPage.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.PreAllocationTabPage.SuspendLayout();
			this.AchievedQuantitiesTabPage.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.CustomDatesTabPage.SuspendLayout();
			this.CRNPanelSea.SuspendLayout();
			this.ConsolDetailsGroupBox.SuspendLayout();
			this.DetailsBottomPanel.SuspendLayout();
			this.AirConsolPanel.SuspendLayout();
			this.CharterPanel.SuspendLayout();
			this.DetailsTopPanel.SuspendLayout();
			this.ConsolDetailsBottomPanel.SuspendLayout();
			this.ShipmentDetailsBottomPanel.SuspendLayout();
			this.BottomInnerRightPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.DepartureDetailsPanel.SuspendLayout();
			this.ArrivalDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColoadConsolModuleButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingConsol);
			//
			// AutoratingDateOverriddenDateEdit
			//
			this.AutoratingDateOverriddenDateEdit.AllowDrop = true;
			this.AutoratingDateOverriddenDateEdit.AutoCompleteMonthThreshold = 1;
			this.AutoratingDateOverriddenDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(AutoratingDateOverriddenDateEdit, "AutoratingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AutoratingDate)));
			this.AutoratingDateOverriddenDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9a85baad-716b-409f-a94c-7369e09e051d", "Autorating Date");
			this.AutoratingDateOverriddenDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.AutoratingDateOverriddenDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 8, true);
			this.AutoratingDateOverriddenDateEdit.Name = "AutoratingDateOverriddenDateEdit";
			this.AutoratingDateOverriddenDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 24, true);
			this.AutoratingDateOverriddenDateEdit.TabIndex = 0;
			//
			// referenceNumbersControl
			// 
			referenceNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(referenceNumbersControl, "Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.CusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Numbers)));
			referenceNumbersControl.DisplayDetailPanel = false;
			referenceNumbersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			referenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			referenceNumbersControl.Name = "referenceNumbersControl";
			referenceNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 199, true);
			referenceNumbersControl.TabIndex = 0;
			// 
			// JK_Calc_TotalShipmentQuantityBoundCalcEdit1
			// 
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1, "JK_TotalShipmentQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentQuantity)));
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|be6e2b30-d927-4e39-98d7-d78af81b8bd8", "Packs");
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.DecimalPlaces = 0;
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 15, true);
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.Name = "JK_Calc_TotalShipmentQuantityBoundCalcEdit1";
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.TabIndex = 2;
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.Text = "0";
			this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DepartureArrivalTabControl
			// 
			this.DepartureArrivalTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DepartureArrivalTabControl.Controls.Add(this.OrgsTab);
			this.DepartureArrivalTabControl.Controls.Add(this.DepartureTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.ArrivalTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.DocsTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.PreAllocationTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.AchievedQuantitiesTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.NumbersTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.RatesTabPage);
			this.DepartureArrivalTabControl.Controls.Add(this.CustomDatesTabPage);
			this.DepartureArrivalTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepartureArrivalTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.DepartureArrivalTabControl.Name = "DepartureArrivalTabControl";
			this.DepartureArrivalTabControl.SelectedIndex = 0;
			this.DepartureArrivalTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 252, true);
			this.DepartureArrivalTabControl.TabIndex = 0;
			this.DepartureArrivalTabControl.SelectedIndexChanging += new EventHandler(this.RefreshDocTabPage);
			// 
			// OrgsTab
			// 
			this.OrgsTab.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|cbb0c529-2655-4224-bca9-3db717464ef4", "Organizations");
			this.OrgsTab.Controls.Add(this.JK_ReceivingForwarderHandlingTypeDropDownEdit);
			this.OrgsTab.Controls.Add(this.JK_SendingForwarderHandlingTypeDropDownEdit);
			this.OrgsTab.Controls.Add(this.ReceivingForwarderAddressControl);
			this.OrgsTab.Controls.Add(this.SendingForwarderAddressControl);
			this.OrgsTab.Controls.Add(this.JK_BookingReferenceBoundTextEdit);
			this.OrgsTab.Controls.Add(this.JK_AgentsReferenceBoundTextEdit);
			this.OrgsTab.Controls.Add(this.ButtonSelectFromConsortium);
			this.OrgsTab.Controls.Add(this.JK_OA_CreditorAddressControl);
			this.OrgsTab.Controls.Add(this.JK_OA_ShippingLineAddressControl);
			this.OrgsTab.Controls.Add(this.JK_CoLoadMasterBillBoundTextEdit);
			this.OrgsTab.Controls.Add(this.JK_CoLoadBookingReferenceBoundTextEdit);
			this.OrgsTab.Controls.Add(this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox);
			this.OrgsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrgsTab.Name = "OrgsTab";
			this.OrgsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OrgsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.OrgsTab.TabIndex = 6;
			// 
			// CFSDepartureByTransportModeDropEdit
			// 
			this.CFSDepartureByTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CFSDepartureByTransportModeDropEdit, "CFSDepartureByTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonConsol)(null)).CFSDepartureByTransportMode)));
			this.CFSDepartureByTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|16138FA5-79C5-480D-903B-57179A2CD3E4", "", "CFS Departure By");
			this.CFSDepartureByTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104,52, true);
			this.CFSDepartureByTransportModeDropEdit.Name = "CFSDepartureByTransportModeDropEdit";
			this.CFSDepartureByTransportModeDropEdit.PreBoundMaxLength = 3;
			this.CFSDepartureByTransportModeDropEdit.ShowDescriptionBox = true;
			this.CFSDepartureByTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.CFSDepartureByTransportModeDropEdit.TabIndex = 3;
			// 
			// CFSArrivalByTransportModeDropEdit
			// 
			this.CFSArrivalByTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CFSArrivalByTransportModeDropEdit, "CFSArrivalByTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonConsol)(null)).CFSArrivalByTransportMode)));
			this.CFSArrivalByTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|4BF0CD80-853B-48F1-9DBF-B282AFE942B8", "", "CFS Arrival By");
			this.CFSArrivalByTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 52, true);
			this.CFSArrivalByTransportModeDropEdit.Name = "CFSArrivalByTransportModeDropEdit";
			this.CFSArrivalByTransportModeDropEdit.PreBoundMaxLength = 3;
			this.CFSArrivalByTransportModeDropEdit.ShowDescriptionBox = true;
			this.CFSArrivalByTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.CFSArrivalByTransportModeDropEdit.TabIndex = 3;
			//
			// JK_CarrierContractNumberBoundTextEdit
			//
			this.BindingSource.SetBindingMember(this.JK_CarrierContractNumberBoundTextEdit, "JK_CarrierContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CarrierContractNumber)));
			this.JK_CarrierContractNumberBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 15, true);
			this.JK_CarrierContractNumberBoundTextEdit.Name = "JK_CarrierContractNumberBoundTextEdit";
			this.JK_CarrierContractNumberBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JK_CarrierContractNumberBoundTextEdit.TabIndex = 0;
			// 
			// CarrierContractImportButton
			//
			this.CarrierContractImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 15, true);
			this.CarrierContractImportButton.Name = "CarrierContractImportButton";
			this.CarrierContractImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.CarrierContractImportButton.TabIndex = 1;
			// from ZFindBoxUserControl
			this.CarrierContractImportButton.Font = OFont.GetFontBold();
			this.CarrierContractImportButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarrierContractImportButton.Text = "...";
			this.CarrierContractImportButton.Click += new System.EventHandler(this.CarrierContractImportButton_Click);
			//
			// JK_CarrierContractNumberCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.JK_CarrierContractNumberFindBox, "JK_CarrierContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CarrierContractNumber)));
			this.JK_CarrierContractNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 15, true);
			this.JK_CarrierContractNumberFindBox.Name = "JK_CarrierContractNumberFindBox";
			this.JK_CarrierContractNumberFindBox.TabIndex = 0;
			this.JK_CarrierContractNumberFindBox.ShowDescriptionBox = false;
			this.JK_CarrierContractNumberFindBox.ShouldResize = false;
			this.JK_CarrierContractNumberFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.JK_CarrierContractNumberFindBox.PreBoundMaxLength = 15;
			// 
			// JK_RCA_AllocationRouteCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JK_RCA_AllocationRouteCodeFindBox, "JK_RCA_AllocationLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RCA_AllocationLine)));
			this.JK_RCA_AllocationRouteCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 15, true);
			this.JK_RCA_AllocationRouteCodeFindBox.Name = "JK_RCA_AllocationRouteCodeFindBox";
			this.JK_RCA_AllocationRouteCodeFindBox.ShowDescriptionBox = false;
			this.JK_RCA_AllocationRouteCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 21, true);
			this.JK_RCA_AllocationRouteCodeFindBox.TabIndex = 2;
			// 
			// JK_ConsolCutOffDateEdit
			// 
			this.JK_ConsolCutOffDateEdit.AllowDrop = true;
			this.JK_ConsolCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_ConsolCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_ConsolCutOffDateEdit, "JK_ConsolCutOffDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolCutOffDateLocal)));
			this.JK_ConsolCutOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_ConsolCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 169, true);
			this.JK_ConsolCutOffDateEdit.Name = "JK_ConsolCutOffDateEdit";
			this.JK_ConsolCutOffDateEdit.TabIndex = 10;
			// 
			// TotalCO2eTextBox
			// 
			this.TotalCO2eTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCO2eTextBox, "TotalCO2eForBinding");
			this.TotalCO2eTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7cfd3445-0149-4a93-b6ef-646b2b2f45ca", "CO2e");
			this.TotalCO2eTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 56, true);
			this.TotalCO2eTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eTextBox.Name = "TotalCO2eTextBox";
			this.TotalCO2eTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCO2eTextBox.TabIndex = 15;
			this.TotalCO2eTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCO2eTextBox.Visible = false;
			// 
			// TotalCO2eUnitLabel
			// 
			this.TotalCO2eUnitLabel.AutoSize = true;
			this.TotalCO2eUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalCO2eUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 60, true);
			this.TotalCO2eUnitLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eUnitLabel.Name = "TotalCO2eUnitLabel";
			this.TotalCO2eUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.TotalCO2eUnitLabel.TabIndex = 16;
			this.TotalCO2eUnitLabel.Text = Enterprise.Freight.Forwarding.GUI.Res.GetString("644e11a8-38dc-4d7e-9c00-1ac3c8b3dd6b", "KG");
			this.TotalCO2eUnitLabel.Visible = false;
			// 
			// ChargeableUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.ChargeableUnitLabel, "JK_TotalShipmentChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentChargeableUnit)));
			this.ChargeableUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|eceb03f7-5e92-45be-bd7d-748c1393b3d8", "{CBM}");
			this.ChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 65, true);
			this.ChargeableUnitLabel.Name = "ChargeableUnitLabel";
			this.ChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 21, true);
			this.ChargeableUnitLabel.TabIndex = 4;
			// 
			// ChargeableCheckCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeableCheckCalcEdit, "JK_TotalShipmentChargableCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentChargableCheck)));
			this.ChargeableCheckCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 65, true);
			this.ChargeableCheckCalcEdit.Name = "ChargeableCheckCalcEdit";
			this.ChargeableCheckCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ChargeableCheckCalcEdit.TabIndex = 6;
			this.ChargeableCheckCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentActVolumeCheck)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).VolumeVerificationUnit)));
			this.VolumeCalcDropEdit.BindToAmount = "JK_TotalShipmentActVolumeCheck";
			this.VolumeCalcDropEdit.BindToUnit = "VolumeVerificationUnit";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 40, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 5;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentActWeightCheck)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).WeightVerificationUnit)));
			this.WeightCalcDropEdit.BindToAmount = "JK_TotalShipmentActWeightCheck";
			this.WeightCalcDropEdit.BindToUnit = "WeightVerificationUnit";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 40, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.WeightCalcDropEdit.TabIndex = 4;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentCountCheckCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentCountCheckCalcEdit, "JK_TotalShipmentCountCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentCountCheck)));
			this.ShipmentCountCheckCalcEdit.DecimalPlaces = 0;
			this.ShipmentCountCheckCalcEdit.Decimals = 0;
			this.ShipmentCountCheckCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 40, true);
			this.ShipmentCountCheckCalcEdit.Name = "ShipmentCountCheckCalcEdit";
			this.ShipmentCountCheckCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.ShipmentCountCheckCalcEdit.TabIndex = 3;
			this.ShipmentCountCheckCalcEdit.Text = "0";
			this.ShipmentCountCheckCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TemperatureControlBlock
			//
			this.BindingSource.SetBindingMember(this.TemperatureControlBlock, ".");
			this.TemperatureControlBlock.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 94, true);
			this.TemperatureControlBlock.Configure(new ConsolTemperatureControlConfiguration());
			this.TemperatureControlBlock.TabIndex = 9;
			//
			// ReceivingForwarderAddressControl
			//
			this.ReceivingForwarderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivingForwarderAddressControl, "ReceivingForwarderWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ReceivingForwarderWithContact)));
			this.ReceivingForwarderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 0, true);
			this.ReceivingForwarderAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|ba787288-b0c8-6d9a-449f-79b6117f2e62", "Receiving Agent");
			this.ReceivingForwarderAddressControl.Name = "ReceivingForwarderWithContact";
			this.ReceivingForwarderAddressControl.PopupCaption = "";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceivingForwarderAddressControl, false);
			this.ReceivingForwarderAddressControl.TabIndex = 2;
			//
			// JK_ReceivingForwarderHandlingTypeDropDownEdit
			//
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_ReceivingForwarderHandlingTypeDropDownEdit, "JK_ReceivingForwarderHandlingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ReceivingForwarderHandlingType)));
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 16, true);
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.Name = "JK_ReceivingForwarderHandlingTypeDropDownEdit";
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.PreBoundMaxLength = 3;
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.ShowDescriptionBox = false;
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_ReceivingForwarderHandlingTypeDropDownEdit.TabIndex = 3;
			//
			// JK_SendingForwarderHandlingTypeDropDownEdit
			//
			this.JK_SendingForwarderHandlingTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_SendingForwarderHandlingTypeDropDownEdit, "JK_SendingForwarderHandlingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_SendingForwarderHandlingType)));
			this.JK_SendingForwarderHandlingTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 16, true);
			this.JK_SendingForwarderHandlingTypeDropDownEdit.Name = "JK_SendingForwarderHandlingTypeDropDownEdit";
			this.JK_SendingForwarderHandlingTypeDropDownEdit.PreBoundMaxLength = 3;
			this.JK_SendingForwarderHandlingTypeDropDownEdit.ShowDescriptionBox = false;
			this.JK_SendingForwarderHandlingTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_SendingForwarderHandlingTypeDropDownEdit.TabIndex = 1;
			//
			// SendingForwarderAddressControl
			//
			this.SendingForwarderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendingForwarderAddressControl, "SendingForwarderWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).SendingForwarderWithContact)));
			this.SendingForwarderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.SendingForwarderAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|7577e699-a00a-748c-4f8d-1824d92ff9fb", "Sending Agent");
			this.SendingForwarderAddressControl.Name = "SendingForwarderWithContact";
			this.SendingForwarderAddressControl.PopupCaption = "";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SendingForwarderAddressControl, false);
			this.SendingForwarderAddressControl.TabIndex = 0;
			// 
			// JK_BookingReferenceBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_BookingReferenceBoundTextEdit, "JK_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_BookingReference)));
			this.JK_BookingReferenceBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 167, true);
			this.JK_BookingReferenceBoundTextEdit.Name = "JK_BookingReferenceBoundTextEdit";
			this.JK_BookingReferenceBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.JK_BookingReferenceBoundTextEdit.TabIndex = 6;
			// 
			// JK_AgentsReferenceBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_AgentsReferenceBoundTextEdit, "JK_AgentsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_AgentsReference)));
			this.JK_AgentsReferenceBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 167, true);
			this.JK_AgentsReferenceBoundTextEdit.Name = "JK_AgentsReferenceBoundTextEdit";
			this.JK_AgentsReferenceBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.JK_AgentsReferenceBoundTextEdit.TabIndex = 7;
			// 
			// JK_CoLoadMasterBillBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_CoLoadMasterBillBoundTextEdit, "JK_CoLoadMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CoLoadMasterBill)));
			this.JK_CoLoadMasterBillBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 207, true);
			this.JK_CoLoadMasterBillBoundTextEdit.Name = "JK_CoLoadMasterBillBoundTextEdit";
			this.JK_CoLoadMasterBillBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.JK_CoLoadMasterBillBoundTextEdit.TabIndex = 9;
			// 
			// JK_CoLoadBookingReferenceBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_CoLoadBookingReferenceBoundTextEdit, "JK_CoLoadBookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CoLoadBookingReference)));
			this.JK_CoLoadBookingReferenceBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 207, true);
			this.JK_CoLoadBookingReferenceBoundTextEdit.Name = "JK_CoLoadBookingReferenceBoundTextEdit";
			this.JK_CoLoadBookingReferenceBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.JK_CoLoadBookingReferenceBoundTextEdit.TabIndex = 10;
			// 
			// ButtonSelectFromConsortium
			// 
			this.ButtonSelectFromConsortium.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 127, true);
			this.ButtonSelectFromConsortium.Name = "ButtonSelectFromConsortium";
			this.ButtonSelectFromConsortium.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 19, true);
			this.ButtonSelectFromConsortium.TabIndex = 3;
			this.ButtonSelectFromConsortium.TabStop = false;
			this.ButtonSelectFromConsortium.Click += new System.EventHandler(this.ButtonSelectFromConsortium_Click);
			// 
			// JK_OA_CreditorAddressControl
			// 
			this.JK_OA_CreditorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_CreditorAddressControl, "JK_OA_CreditorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_CreditorAddress)));
			this.JK_OA_CreditorAddressControl.BindToOrgList = "CreditorList";
			this.JK_OA_CreditorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 147, true);
			this.JK_OA_CreditorAddressControl.Name = "JK_OA_CreditorAddressControl";
			this.JK_OA_CreditorAddressControl.PopupCaption = "";
			this.JK_OA_CreditorAddressControl.ShowAddress = false;
			this.JK_OA_CreditorAddressControl.ShowOrganisationName = true;
			this.JK_OA_CreditorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_CreditorAddressControl.TabIndex = 5;
			// 
			// JK_OA_ShippingLineAddressControl
			// 
			this.JK_OA_ShippingLineAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ShippingLineAddressControl, "JK_OA_ShippingLineAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_ShippingLineAddress)));
			this.JK_OA_ShippingLineAddressControl.BindToOrgList = "ShippingProviderList";
			this.JK_OA_ShippingLineAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 127, true);
			this.JK_OA_ShippingLineAddressControl.Name = "JK_OA_ShippingLineAddressControl";
			this.JK_OA_ShippingLineAddressControl.PopupCaption = "Select Shipping Provider";
			this.JK_OA_ShippingLineAddressControl.ShowAddress = false;
			this.JK_OA_ShippingLineAddressControl.ShowOrganisationName = true;
			this.JK_OA_ShippingLineAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_ShippingLineAddressControl.TabIndex = 4;
			// 
			// JK_RS_NKGatewayServiceLevelBoundCodeFindBox
			// 
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox, "JK_RS_NKGatewayServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RS_NKGatewayServiceLevel)));
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 187, true);
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.Name = "JK_RS_NKGatewayServiceLevelBoundCodeFindBox";
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.ShowDescriptionBox = true;
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_RS_NKGatewayServiceLevelBoundCodeFindBox.TabIndex = 10;
			//
			// DepartureDetailsPanel
			//
			this.DepartureDetailsPanel.Controls.Add(this.JK_DateFirstForeignPortDateEdit);
			this.DepartureDetailsPanel.Controls.Add(this.JK_DateLastForeignPortDateEdit);
			this.DepartureDetailsPanel.Controls.Add(this.LastForeignPortBoundCodeFindBox1);
			this.DepartureDetailsPanel.Controls.Add(this.departurePartialEventsInfoControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_RL_NKFirstForeignPortBoundCodeFindBox);
			this.DepartureDetailsPanel.Controls.Add(this.JK_OA_ContainerYardEmptyPickupAddressControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_OA_DepartureCTOAddressControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_OA_PackDepotAddressControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_PackDepotReceiptRequestedControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_PackDepotDispatchRequestedControl);
			this.DepartureDetailsPanel.Controls.Add(this.JK_OA_DeparturePackCFSTransportAddressControl);
			this.DepartureDetailsPanel.Controls.Add(this.CFSDepartureByTransportModeDropEdit);
			this.DepartureDetailsPanel.AutoScroll = true;
			this.DepartureDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 209, true);
			this.DepartureDetailsPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 235, true);
			this.DepartureDetailsPanel.Name = "DepartureDetailsPanel";
			this.DepartureDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepartureDetailsPanel.TabIndex = 0;
			// 
			// DepartureTabPage
			//
			this.DepartureDetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|330D6809-FA98-4FF2-9617-703D57BEA0A3", "Details");
			this.DepartureDetailsTabPage.Controls.Add(this.DepartureDetailsPanel);
			this.DepartureDetailsTabPage.Name = "DepartureDetailsTabPage";

			this.DepartureTabControl.Name = "DepartureTabControl";
			this.DepartureTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 209, true);
			this.DepartureTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepartureTabControl.TabPages.Add(this.DepartureDetailsTabPage);

			this.DepartureTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|c7804379-0401-4b23-bf11-1d636eaee41c", "Departure");
			this.DepartureTabPage.Controls.Add(this.DepartureTabControl);
			this.DepartureTabPage.Name = "DepartureTabPage";
			// 
			// JK_DateFirstForeignPortDateEdit
			// 
			this.JK_DateFirstForeignPortDateEdit.AllowDrop = true;
			this.JK_DateFirstForeignPortDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_DateFirstForeignPortDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_DateFirstForeignPortDateEdit, "JK_DateFirstForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_DateFirstForeignPort)));
			this.JK_DateFirstForeignPortDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_DateFirstForeignPortDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 147, true);
			this.JK_DateFirstForeignPortDateEdit.Name = "JK_DateFirstForeignPortDateEdit";
			this.JK_DateFirstForeignPortDateEdit.TabIndex = 11;
			// 
			// JK_DateLastForeignPortDateEdit
			// 
			this.JK_DateLastForeignPortDateEdit.AllowDrop = true;
			this.JK_DateLastForeignPortDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_DateLastForeignPortDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_DateLastForeignPortDateEdit, "JK_DateLastForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_DateLastForeignPort)));
			this.JK_DateLastForeignPortDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_DateLastForeignPortDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 170, true);
			this.JK_DateLastForeignPortDateEdit.Name = "JK_DateLastForeignPortDateEdit";
			this.JK_DateLastForeignPortDateEdit.TabIndex = 15;
			// 
			// LastForeignPortBoundCodeFindBox1
			// 
			this.LastForeignPortBoundCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LastForeignPortBoundCodeFindBox1, "JK_RL_NKLastForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKLastForeignPort)));
			this.LastForeignPortBoundCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 170, true);
			this.LastForeignPortBoundCodeFindBox1.Name = "LastForeignPortBoundCodeFindBox1";
			this.LastForeignPortBoundCodeFindBox1.PreBoundMaxLength = 5;
			this.LastForeignPortBoundCodeFindBox1.ShowDescriptionBox = false;
			this.LastForeignPortBoundCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.LastForeignPortBoundCodeFindBox1.TabIndex = 13;
			// 
			// departurePartialEventsInfoControl
			// 
			this.departurePartialEventsInfoControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("fc989b6e-65aa-4120-a004-f11e8946bae6", "Partial Load");
			this.departurePartialEventsInfoControl.CaptionRenderingEnabled = true;
			this.LabelCaptionRenderProvider.SetLabelTop(this.departurePartialEventsInfoControl, 0);
			this.departurePartialEventsInfoControl.EventsToShow = PartialEventsInfoControl.EventTypes.Departure;
			this.departurePartialEventsInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 195, true);
			this.departurePartialEventsInfoControl.Name = "departurePartialEventsInfoControl";
			this.departurePartialEventsInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 58, true);
			this.departurePartialEventsInfoControl.TabIndex = 14;
			// 
			// arrivalPartialEventsInfoControl
			// 
			this.arrivalPartialEventsInfoControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("208b0c05-088a-4817-9d43-81a83647481e", "Partial Unload");
			this.arrivalPartialEventsInfoControl.CaptionRenderingEnabled = true;
			this.LabelCaptionRenderProvider.SetLabelTop(this.arrivalPartialEventsInfoControl, 0);
			this.arrivalPartialEventsInfoControl.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;
			this.arrivalPartialEventsInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 195, true);
			this.arrivalPartialEventsInfoControl.Name = "arrivalPartialEventsInfoControl";
			this.arrivalPartialEventsInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 58, true);
			this.arrivalPartialEventsInfoControl.TabIndex = 14;
			// 
			// JK_RL_NKFirstForeignPortBoundCodeFindBox
			// 
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKFirstForeignPortBoundCodeFindBox, "JK_RL_NKFirstForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKFirstForeignPort)));
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 147, true);
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.Name = "JK_RL_NKFirstForeignPortBoundCodeFindBox";
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKFirstForeignPortBoundCodeFindBox.TabIndex = 9;
			// 
			// JK_OA_ContainerYardEmptyPickupAddressControl
			// 
			this.JK_OA_ContainerYardEmptyPickupAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ContainerYardEmptyPickupAddressControl, "JK_OA_ContainerYardEmptyPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_ContainerYardEmptyPickupAddress)));
			this.JK_OA_ContainerYardEmptyPickupAddressControl.BindToOrgList = "OrgDepartureContainerYardList";
			this.JK_OA_ContainerYardEmptyPickupAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|b6582d8f-0533-4051-8cb0-69ab9b76fd90", "Container Yard");
			this.JK_OA_ContainerYardEmptyPickupAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 100, true);
			this.JK_OA_ContainerYardEmptyPickupAddressControl.Name = "JK_OA_ContainerYardEmptyPickupAddressControl";
			this.JK_OA_ContainerYardEmptyPickupAddressControl.PopupCaption = null;
			this.JK_OA_ContainerYardEmptyPickupAddressControl.ShowAddress = false;
			this.JK_OA_ContainerYardEmptyPickupAddressControl.ShowOrganisationName = true;
			this.JK_OA_ContainerYardEmptyPickupAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_ContainerYardEmptyPickupAddressControl.TabIndex = 6;
			// 
			// JK_OA_DepartureCTOAddressControl
			// 
			this.JK_OA_DepartureCTOAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_DepartureCTOAddressControl, "JK_OA_DepartureCTOAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_DepartureCTOAddress)));
			this.JK_OA_DepartureCTOAddressControl.BindToOrgList = "OrgDepartureCtoList";
			this.JK_OA_DepartureCTOAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 6, true);
			this.JK_OA_DepartureCTOAddressControl.Name = "JK_OA_DepartureCTOAddressControl";
			this.JK_OA_DepartureCTOAddressControl.PopupCaption = null;
			this.JK_OA_DepartureCTOAddressControl.ShowAddress = false;
			this.JK_OA_DepartureCTOAddressControl.ShowOrganisationName = true;
			this.JK_OA_DepartureCTOAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_DepartureCTOAddressControl.TabIndex = 1;
			// 
			// JK_OA_PackDepotAddressControl
			// 
			this.JK_OA_PackDepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_PackDepotAddressControl, "JK_OA_PackDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_PackDepotAddress)));
			this.JK_OA_PackDepotAddressControl.BindToOrgList = "OrgPackDepotList";
			this.JK_OA_PackDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 29, true);
			this.JK_OA_PackDepotAddressControl.Name = "JK_OA_PackDepotAddressControl";
			this.JK_OA_PackDepotAddressControl.PopupCaption = null;
			this.JK_OA_PackDepotAddressControl.ShowAddress = false;
			this.JK_OA_PackDepotAddressControl.ShowOrganisationName = true;
			this.JK_OA_PackDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_PackDepotAddressControl.TabIndex = 2;
			//
			// JK_PackDepotReceiptRequestedControl
			//
			this.JK_PackDepotReceiptRequestedControl.AllowDrop = true;
			this.JK_PackDepotReceiptRequestedControl.AutoCompleteMonthThreshold = 1;
			this.JK_PackDepotReceiptRequestedControl.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_PackDepotReceiptRequestedControl, "JK_PackDepotReceiptRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PackDepotReceiptRequested)));
			this.JK_PackDepotReceiptRequestedControl.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_PackDepotReceiptRequestedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 77, true);
			this.JK_PackDepotReceiptRequestedControl.Name = "JK_PackDepotReceiptRequestedControl";
			this.JK_PackDepotReceiptRequestedControl.TabIndex = 4;
			//
			// JK_PackDepotDispatchRequestedControl
			//
			this.JK_PackDepotDispatchRequestedControl.AllowDrop = true;
			this.JK_PackDepotDispatchRequestedControl.AutoCompleteMonthThreshold = 1;
			this.JK_PackDepotDispatchRequestedControl.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_PackDepotDispatchRequestedControl, "JK_PackDepotDispatchRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PackDepotDispatchRequested)));
			this.JK_PackDepotDispatchRequestedControl.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_PackDepotDispatchRequestedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 77, true);
			this.JK_PackDepotDispatchRequestedControl.Name = "JK_PackDepotReceiptRequestedControl";
			this.JK_PackDepotDispatchRequestedControl.TabIndex = 5;
			//
			// ArrivalDetailsPanel
			//
			this.ArrivalDetailsPanel.Controls.Add(this.JK_DatePortOfFirstArrivalDateEdit);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_DateLastForeignPortDateEdit2);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_RL_NKLastForeignPortBoundCodeFindBox2);
			this.ArrivalDetailsPanel.Controls.Add(this.arrivalPartialEventsInfoControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_OA_ContainerYardEmptyReturnAddressControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_OA_UnpackDepotAddressControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_UnpackDepotReceiptRequestedControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_UnpackDepotDispatchRequestedControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_OA_ArrivalCTOAddressControl);
			this.ArrivalDetailsPanel.Controls.Add(this.JK_OA_ArrivalUnpackCFSTransportAddressControl);
			this.ArrivalDetailsPanel.Controls.Add(this.CFSArrivalByTransportModeDropEdit);
			this.ArrivalDetailsPanel.AutoScroll = true;
			this.ArrivalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 209, true);
			this.ArrivalDetailsPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 235, true);
			this.ArrivalDetailsPanel.Name = "ArrivalDetailsPanel";
			this.ArrivalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalDetailsPanel.TabIndex = 0;
			// 
			// ArrivalTabPage
			//
			this.ArrivalDetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|0A53873C-078F-4986-A123-5C540B65933E", "Details");
			this.ArrivalDetailsTabPage.Controls.Add(this.ArrivalDetailsPanel);
			this.ArrivalDetailsTabPage.Name = "ArrivalDetailsTabPage";

			this.ArrivalTabControl.Name = "ArrivalTabControl";
			this.ArrivalTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 209, true);
			this.ArrivalTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalTabControl.TabPages.Add(this.ArrivalDetailsTabPage);

			this.ArrivalTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|67e7150a-6009-4afc-982b-5022a633a6ac", "Arrival");
			this.ArrivalTabPage.Controls.Add(this.ArrivalTabControl);
			this.ArrivalTabPage.Name = "ArrivalTabPage";
			// 
			// JK_DatePortOfFirstArrivalDateEdit
			// 
			this.JK_DatePortOfFirstArrivalDateEdit.AllowDrop = true;
			this.JK_DatePortOfFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_DatePortOfFirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_DatePortOfFirstArrivalDateEdit, "JK_DatePortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_DatePortOfFirstArrival)));
			this.JK_DatePortOfFirstArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_DatePortOfFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 147, true);
			this.JK_DatePortOfFirstArrivalDateEdit.Name = "JK_DatePortOfFirstArrivalDateEdit";
			this.JK_DatePortOfFirstArrivalDateEdit.TabIndex = 11;
			// 
			// JK_DateLastForeignPortDateEdit2
			// 
			this.JK_DateLastForeignPortDateEdit2.AllowDrop = true;
			this.JK_DateLastForeignPortDateEdit2.AutoCompleteMonthThreshold = 1;
			this.JK_DateLastForeignPortDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_DateLastForeignPortDateEdit2, "JK_DateLastForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_DateLastForeignPort)));
			this.JK_DateLastForeignPortDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_DateLastForeignPortDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 170, true);
			this.JK_DateLastForeignPortDateEdit2.Name = "JK_DateLastForeignPortDateEdit2";
			this.JK_DateLastForeignPortDateEdit2.TabIndex = 15;
			// 
			// JK_RL_NKLastForeignPortBoundCodeFindBox2
			// 
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKLastForeignPortBoundCodeFindBox2, "JK_RL_NKLastForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKLastForeignPort)));
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 170, true);
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.Name = "JK_RL_NKLastForeignPortBoundCodeFindBox2";
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.PreBoundMaxLength = 5;
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.ShowDescriptionBox = false;
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKLastForeignPortBoundCodeFindBox2.TabIndex = 13;
			// 
			// JK_RL_NKPortOfFirstArrivalBoundCodeFindBox
			// 
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox, "JK_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKPortOfFirstArrival)));
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 147, true);
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.Name = "JK_RL_NKPortOfFirstArrivalBoundCodeFindBox";
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKPortOfFirstArrivalBoundCodeFindBox.TabIndex = 9;
			// 
			// JK_OA_ContainerYardEmptyReturnAddressControl
			// 
			this.JK_OA_ContainerYardEmptyReturnAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ContainerYardEmptyReturnAddressControl, "JK_OA_ContainerYardEmptyReturnAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_ContainerYardEmptyReturnAddress)));
			this.JK_OA_ContainerYardEmptyReturnAddressControl.BindToOrgList = "OrgArrivalContainerYardList";
			this.JK_OA_ContainerYardEmptyReturnAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|5c1b5513-29ad-455e-98e6-704fcd9ab583", "Container Yard");
			this.JK_OA_ContainerYardEmptyReturnAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 100, true);
			this.JK_OA_ContainerYardEmptyReturnAddressControl.Name = "JK_OA_ContainerYardEmptyReturnAddressControl";
			this.JK_OA_ContainerYardEmptyReturnAddressControl.PopupCaption = null;
			this.JK_OA_ContainerYardEmptyReturnAddressControl.ShowAddress = false;
			this.JK_OA_ContainerYardEmptyReturnAddressControl.ShowOrganisationName = true;
			this.JK_OA_ContainerYardEmptyReturnAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_ContainerYardEmptyReturnAddressControl.TabIndex = 7;
			// 
			// JK_OA_UnpackDepotAddressControl
			// 
			this.JK_OA_UnpackDepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_UnpackDepotAddressControl, "JK_OA_UnpackDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_UnpackDepotAddress)));
			this.JK_OA_UnpackDepotAddressControl.BindToOrgList = "OrgUnpackDepotList";
			this.JK_OA_UnpackDepotAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|992a2ca4-d868-4ac5-94a4-1430b29d1d62", "CFS Address");
			this.JK_OA_UnpackDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 29, true);
			this.JK_OA_UnpackDepotAddressControl.Name = "JK_OA_UnpackDepotAddressControl";
			this.JK_OA_UnpackDepotAddressControl.PopupCaption = null;
			this.JK_OA_UnpackDepotAddressControl.ShowAddress = false;
			this.JK_OA_UnpackDepotAddressControl.ShowOrganisationName = true;
			this.JK_OA_UnpackDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_UnpackDepotAddressControl.TabIndex = 2;
			//
			// JK_UnpackDepotReceiptRequestedControl
			//
			this.JK_UnpackDepotReceiptRequestedControl.AllowDrop = true;
			this.JK_UnpackDepotReceiptRequestedControl.AutoCompleteMonthThreshold = 1;
			this.JK_UnpackDepotReceiptRequestedControl.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_UnpackDepotReceiptRequestedControl, "JK_UnpackDepotReceiptRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_UnpackDepotReceiptRequested)));
			this.JK_UnpackDepotReceiptRequestedControl.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_UnpackDepotReceiptRequestedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 77, true);
			this.JK_UnpackDepotReceiptRequestedControl.Name = "JK_UnpackDepotReceiptRequestedControl";
			this.JK_UnpackDepotReceiptRequestedControl.TabIndex = 5;
			//
			// JK_UnpackDepotDispatchRequestedControl
			//
			this.JK_UnpackDepotDispatchRequestedControl.AllowDrop = true;
			this.JK_UnpackDepotDispatchRequestedControl.AutoCompleteMonthThreshold = 1;
			this.JK_UnpackDepotDispatchRequestedControl.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JK_UnpackDepotDispatchRequestedControl, "JK_UnpackDepotDispatchRequested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_UnpackDepotDispatchRequested)));
			this.JK_UnpackDepotDispatchRequestedControl.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_UnpackDepotDispatchRequestedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 77, true);
			this.JK_UnpackDepotDispatchRequestedControl.Name = "JK_UnpackDepotReceiptRequestedControl";
			this.JK_UnpackDepotDispatchRequestedControl.TabIndex = 6;
			// 
			// JK_OA_ArrivalCTOAddressControl
			// 
			this.JK_OA_ArrivalCTOAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ArrivalCTOAddressControl, "JK_OA_ArrivalCTOAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_ArrivalCTOAddress)));
			this.JK_OA_ArrivalCTOAddressControl.BindToOrgList = "OrgArrivalCtoList";
			this.JK_OA_ArrivalCTOAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 6, true);
			this.JK_OA_ArrivalCTOAddressControl.Name = "JK_OA_ArrivalCTOAddressControl";
			this.JK_OA_ArrivalCTOAddressControl.PopupCaption = null;
			this.JK_OA_ArrivalCTOAddressControl.ShowAddress = false;
			this.JK_OA_ArrivalCTOAddressControl.ShowOrganisationName = true;
			this.JK_OA_ArrivalCTOAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_ArrivalCTOAddressControl.TabIndex = 1;
			// 
			// LatestStatusGroupBox
			//
			this.LatestStatusGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f6779494-9762-4754-90a7-6cffd4a2061a", "Latest Status");
			this.LatestStatusGroupBox.Controls.Add(this.ViewEBLButton);
			this.LatestStatusGroupBox.Controls.Add(this.ElectronicBillOfLadingReferenceTextBox);
			this.LatestStatusGroupBox.Controls.Add(this.BillOfLadingBillDateDateEdit);
			this.LatestStatusGroupBox.Controls.Add(this.BillOfLadingBillStatusDropEdit);
			this.LatestStatusGroupBox.Controls.Add(this.CarrierBookingDateDateEdit);
			this.LatestStatusGroupBox.Controls.Add(this.CarrierBookingLatestStatusDropEdit);
			this.LatestStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 124, true);
			this.LatestStatusGroupBox.Name = "LatestStatusGroupBox";
			this.LatestStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 80, true);
			this.LatestStatusGroupBox.TabIndex = 15;
			this.LatestStatusGroupBox.TabStop = false;
			// 
			// CarrierBookingOfficeCodeFindBox
			// 
			this.CarrierBookingOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierBookingOfficeCodeFindBox, "JK_RL_NKCarrierBookingOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKCarrierBookingOffice)));
			this.CarrierBookingOfficeCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c766b9b2-c840-4818-8ebd-e93b457de00a", "Carrier Booking Office");
			this.CarrierBookingOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 76, true);
			this.CarrierBookingOfficeCodeFindBox.Name = "CarrierBookingOfficeCodeFindBox";
			this.CarrierBookingOfficeCodeFindBox.ShowDescriptionBox = false;
			this.CarrierBookingOfficeCodeFindBox.PreBoundMaxLength = 5;
			this.CarrierBookingOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CarrierBookingOfficeCodeFindBox.TabIndex = 10;
			// 
			// BillOfLadingBillTypeDropEdit
			// 
			this.BillOfLadingBillTypeDropEdit.AllowDrop = true;
			this.BillOfLadingBillTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillTypeDropEdit, "JK_ElectronicBillOfLadingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ElectronicBillOfLadingType)));
			this.BillOfLadingBillTypeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("14f2e66f-8aa9-4741-87ba-8f144f2ecc8e", "Bill Type");
			this.BillOfLadingBillTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 100, true);
			this.BillOfLadingBillTypeDropEdit.Name = "BillOfLadingBillTypeDropEdit";
			this.BillOfLadingBillTypeDropEdit.PreBoundMaxLength = 3;
			this.BillOfLadingBillTypeDropEdit.ShouldResizeByMaxLength = true;
			this.BillOfLadingBillTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.BillOfLadingBillTypeDropEdit.TabIndex = 12;
			// 
			// BillingOfLadingBillTermsDropEdit
			// 
			this.BillOfLadingBillTermsDropEdit.AllowDrop = true;
			this.BillOfLadingBillTermsDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillTermsDropEdit, "JK_ElectronicBillOfLadingTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ElectronicBillOfLadingTerms)));
			this.BillOfLadingBillTermsDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("59ca740e-1de0-40fd-adac-2f68dd5b2c19", "Bill Terms");
			this.BillOfLadingBillTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 100, true);
			this.BillOfLadingBillTermsDropEdit.Name = "BillOfLadingBillTermsDropEdit";
			this.BillOfLadingBillTermsDropEdit.PreBoundMaxLength = 3;
			this.BillOfLadingBillTermsDropEdit.ShouldResizeByMaxLength = true;
			this.BillOfLadingBillTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.BillOfLadingBillTermsDropEdit.TabIndex = 14;
			// 
			// BillingOfLadingBillDateDateEdit
			// 
			this.BillOfLadingBillDateDateEdit.AllowDrop = true;
			this.BillOfLadingBillDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BillOfLadingBillDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillDateDateEdit, "JK_Calc_BillOfLadingBillDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_BillOfLadingBillDate)));
			this.BillOfLadingBillDateDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c7be02b5-2376-420f-88ac-c152c78ff9af", "Date");
			this.BillOfLadingBillDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.BillOfLadingBillDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 47, true);
			this.BillOfLadingBillDateDateEdit.Name = "BillOfLadingBillDateDateEdit";
			this.BillOfLadingBillDateDateEdit.TabIndex = 19;
			// 
			// BillingOfLadingBillStatusDropEdit
			// 
			this.BillOfLadingBillStatusDropEdit.AllowDrop = true;
			this.BillOfLadingBillStatusDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfLadingBillStatusDropEdit, "JK_Calc_BillOfLadingBillStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_BillOfLadingBillStatus)));
			this.BillOfLadingBillStatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("127ae7ec-b446-4d9f-83dc-8dae287343e0", "Bill Status");
			this.BillOfLadingBillStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 47, true);
			this.BillOfLadingBillStatusDropEdit.Name = "BillOfLadingBillStatusDropEdit";
			this.BillOfLadingBillStatusDropEdit.PreBoundMaxLength = 3;
			this.BillOfLadingBillStatusDropEdit.ShouldResizeByMaxLength = true;
			this.BillOfLadingBillStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.BillOfLadingBillStatusDropEdit.TabIndex = 18;
			// 
			// CarrierBookingDateDateEdit
			// 
			this.CarrierBookingDateDateEdit.AllowDrop = true;
			this.CarrierBookingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CarrierBookingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CarrierBookingDateDateEdit, "JK_Calc_CarrierBookingLatestDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_CarrierBookingLatestDate)));
			this.CarrierBookingDateDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c7be02b5-2376-420f-88ac-c152c78ff9af", "Date");
			this.CarrierBookingDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CarrierBookingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 23, true);
			this.CarrierBookingDateDateEdit.Name = "CarrierBookingDateDateEdit";
			this.CarrierBookingDateDateEdit.TabIndex = 17;
			// 
			// CarrierBookingLatestStatusDropEdit
			// 
			this.CarrierBookingLatestStatusDropEdit.AllowDrop = true;
			this.CarrierBookingLatestStatusDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CarrierBookingLatestStatusDropEdit, "JK_Calc_CarrierBookingLatestStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_CarrierBookingLatestStatus)));
			this.CarrierBookingLatestStatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2290ef9d-7084-48b6-8943-1dc2edf51510", "Booking Status");
			this.CarrierBookingLatestStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 23, true);
			this.CarrierBookingLatestStatusDropEdit.Name = "CarrierBookingLatestStatusDropEdit";
			this.CarrierBookingLatestStatusDropEdit.PreBoundMaxLength = 3;
			this.CarrierBookingLatestStatusDropEdit.ShouldResizeByMaxLength = true;
			this.CarrierBookingLatestStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CarrierBookingLatestStatusDropEdit.TabIndex = 16;
			// 
			// ElectronicBillOfLadingReferenceTextBox
			// 
			this.ElectronicBillOfLadingReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ElectronicBillOfLadingReferenceTextBox, "JK_ElectronicBillOfLadingReference");
			this.ElectronicBillOfLadingReferenceTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("51ED68DF-D6C9-426C-A53D-DA2A1221A6AD", "eBL Identifier");
			this.ElectronicBillOfLadingReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ElectronicBillOfLadingReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 71, true);
			this.ElectronicBillOfLadingReferenceTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ElectronicBillOfLadingReferenceTextBox.Name = "ElectronicBillOfLadingReferenceTextBox";
			this.ElectronicBillOfLadingReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.ElectronicBillOfLadingReferenceTextBox.TabIndex = 20;
			this.ElectronicBillOfLadingReferenceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.ElectronicBillOfLadingReferenceTextBox.ReadOnly = true;
			this.ElectronicBillOfLadingReferenceTextBox.MaxLength = 100;
			// 
			// ViewEBLButton
			// 
			this.ViewEBLButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("240EBE22-C69A-4337-9E75-BFF8CFAA049C", "View/Transact eBL");
			this.ViewEBLButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 70, true);
			this.ViewEBLButton.Name = "ViewEBLButton";
			this.ViewEBLButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.ViewEBLButton.TabIndex = 21;
			this.ViewEBLButton.ToolTipCaption = null;
			this.ViewEBLButton.Click += new System.EventHandler(this.ViewEBLButton_Click);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|9e9e7b4c-8358-443e-8930-c75bbfd20a65", "Docs");
			this.DocsTabPage.Controls.Add(this.JK_RL_NKMasterBillIssuePlaceFindBox);
			this.DocsTabPage.Controls.Add(this.JK_NoCopyBillsCalcEdit);
			this.DocsTabPage.Controls.Add(this.JK_NoOriginalBillsCalcEdit);
			this.DocsTabPage.Controls.Add(this.CarrierBookingOfficeCodeFindBox);
			this.DocsTabPage.Controls.Add(this.zDropEdit1);
			this.DocsTabPage.Controls.Add(this.MasterBillIssueDateEdit);
			this.DocsTabPage.Controls.Add(this.AWBDimsDropEdit);
			this.DocsTabPage.Controls.Add(this.PackageGroupingDropEdit);
			this.DocsTabPage.Controls.Add(this.JK_PrintOptionForColoadsOnOtherDocsDropEdit);
			this.DocsTabPage.Controls.Add(this.JK_PrintOptionForColoadsOnManifestDropEdit);
			this.DocsTabPage.Controls.Add(this.ConsolChargeableRateCalcEdit);
			this.DocsTabPage.Controls.Add(this.SecurityStatusDropEdit);
			this.DocsTabPage.Controls.Add(this.SpecialHandlingUserControl);
			this.DocsTabPage.Controls.Add(this.ChargesApply);
			this.DocsTabPage.Controls.Add(this.BillOfLadingBillTypeDropEdit);
			this.DocsTabPage.Controls.Add(this.BillOfLadingBillTermsDropEdit);
			this.DocsTabPage.Controls.Add(this.LatestStatusGroupBox);
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocsTabPage.Name = "DocsTabPage";
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.DocsTabPage.TabIndex = 4;
			// 
			// PreAllocationTabPage
			// 
			this.PreAllocationTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|c3134b4a-3c51-4106-93d7-a7b161b29833", "Pre-Allocation");
			this.PreAllocationTabPage.Controls.Add(this.JK_CarrierContractNumberFindBox);
			this.PreAllocationTabPage.Controls.Add(this.JK_CarrierContractNumberBoundTextEdit);
			this.PreAllocationTabPage.Controls.Add(this.CarrierContractImportButton);
			this.PreAllocationTabPage.Controls.Add(this.JK_RCA_AllocationRouteCodeFindBox);
			this.PreAllocationTabPage.Controls.Add(this.JK_ConsolCutOffDateEdit);
			this.PreAllocationTabPage.Controls.Add(this.ChargeableUnitLabel);
			this.PreAllocationTabPage.Controls.Add(this.ChargeableCheckCalcEdit);
			this.PreAllocationTabPage.Controls.Add(this.VolumeCalcDropEdit);
			this.PreAllocationTabPage.Controls.Add(this.WeightCalcDropEdit);
			this.PreAllocationTabPage.Controls.Add(this.ShipmentCountCheckCalcEdit);
			this.PreAllocationTabPage.Controls.Add(this.TemperatureControlBlock);
			this.PreAllocationTabPage.Controls.Add(this.DangerousGoodsControl);
			this.PreAllocationTabPage.Controls.Add(this.Commodity);
			this.PreAllocationTabPage.Controls.Add(this.ConsolMaxDimsControl);

			this.PreAllocationTabPage.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PreAllocationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 136, true);
			this.PreAllocationTabPage.Name = "PreAllocationTabPage";
			this.PreAllocationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 67, true);
			this.PreAllocationTabPage.TabIndex = 10;
			this.PreAllocationTabPage.TabStop = false;
			//
			// AchievedQuantitiesTabPage
			//
			this.AchievedQuantitiesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|dde3c3d5-2450-480a-bf84-c1d6e7876120", "Achieved Quantities");
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_OverrideConsolChargeableCheckBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CorrectedConsolVolumeUnitDropEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CorrectedConsolWeightUnitDropEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CorrectedConsolVolumeCalcEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CorrectedConsolWeightCalcEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.ConsolChargeableUnitTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.ConsolChargeableQuantityCalcEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ActualVolumeWeightEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ActualVolumeWeightUnitTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.ExcessVolumeWeightCalcEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.ExcessVolumeWeightUnitTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_FreeSpaceEdit);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_FreeSpaceUnitTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.WeightUtilisationPercentageBar);
			this.AchievedQuantitiesTabPage.Controls.Add(this.VolumeUtilisationPercentageBar);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CostFreePercentageBar);
			this.AchievedQuantitiesTabPage.Controls.Add(this.DensityVisualisationControl);
			this.AchievedQuantitiesTabPage.Controls.Add(this.DensityFactorLabel);
			this.AchievedQuantitiesTabPage.Controls.Add(this.WeightUtilisationLabel);
			this.AchievedQuantitiesTabPage.Controls.Add(this.VolumeUtilisationLabel);
			this.AchievedQuantitiesTabPage.Controls.Add(this.CostFreeLabel);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ConsolidatedFreightCostChargeableTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ShipmentFreightCostChargeableTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox);
			this.AchievedQuantitiesTabPage.Controls.Add(this.JK_Calc_ShipmentFreightCostChargeableDescTextBox);
			this.AchievedQuantitiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 205, true);
			this.AchievedQuantitiesTabPage.Name = "AchievedQuantitiesTabPage";
			this.AchievedQuantitiesTabPage.TabIndex = 5;
			// 
			// DangerousGoodsControl
			// 
			this.DangerousGoodsControl.AllowDrop = true;
			this.DangerousGoodsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 94, true);
			this.DangerousGoodsControl.Name = "DangerousGoodsControl";
			this.DangerousGoodsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 88, true);
			this.DangerousGoodsControl.TabIndex = 8;
			//
			// Commodity
			//
			this.Commodity.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Commodity, ForwardingConsol.Schema.JK_RH_NKConsolCommodity);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RH_NKConsolCommodity)));
			this.Commodity.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ee0b2bf6-ccef-4934-b691-083de35316e3", "Commodity");
			this.Commodity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 187, true); 
			this.Commodity.ShowDescriptionBox = false;
			this.Commodity.Name = "Commodity";
			this.Commodity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.Commodity.TabIndex = 9;

			//
			// ConsolMaxDimsControl
			//
			this.ConsolMaxDimsControl.AllowDrop = true;
			this.ConsolMaxDimsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 65, true);
			this.ConsolMaxDimsControl.Name = "ConsolMaxDimsControl";
			this.ConsolMaxDimsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 26, true);
			this.ConsolMaxDimsControl.TabIndex = 7;
			// 
			// JK_RL_NKMasterBillIssuePlaceFindBox
			// 
			this.JK_RL_NKMasterBillIssuePlaceFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKMasterBillIssuePlaceFindBox, "JK_RL_NKMasterBillIssuePlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKMasterBillIssuePlace)));
			this.JK_RL_NKMasterBillIssuePlaceFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|cc2bfd3f-2714-4c63-8622-39231cb1663b", "Place of Issue");
			this.JK_RL_NKMasterBillIssuePlaceFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 76, true);
			this.JK_RL_NKMasterBillIssuePlaceFindBox.Name = "JK_RL_NKMasterBillIssuePlaceFindBox";
			this.JK_RL_NKMasterBillIssuePlaceFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKMasterBillIssuePlaceFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JK_RL_NKMasterBillIssuePlaceFindBox.TabIndex = 8;
			// 
			// ConsolChargeableUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolChargeableUnitTextBox, "JK_ConsolChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolChargeableUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsolChargeableUnitTextBox, false);
			this.ConsolChargeableUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 81, true);
			this.ConsolChargeableUnitTextBox.Name = "ConsolChargeableUnitTextBox";
			this.ConsolChargeableUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.ConsolChargeableUnitTextBox.TabIndex = 13;
			// 
			// JK_NoCopyBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_NoCopyBillsCalcEdit, "JK_NoCopyBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_NoCopyBills)));
			this.JK_NoCopyBillsCalcEdit.DecimalPlaces = 2;
			this.JK_NoCopyBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(517, 52, true);
			this.JK_NoCopyBillsCalcEdit.Name = "JK_NoCopyBillsCalcEdit";
			this.JK_NoCopyBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.JK_NoCopyBillsCalcEdit.TabIndex = 6;
			this.JK_NoCopyBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_NoOriginalBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_NoOriginalBillsCalcEdit, "JK_NoOriginalBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_NoOriginalBills)));
			this.JK_NoOriginalBillsCalcEdit.DecimalPlaces = 2;
			this.JK_NoOriginalBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 52, true);
			this.JK_NoOriginalBillsCalcEdit.Name = "JK_NoOriginalBillsCalcEdit";
			this.JK_NoOriginalBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.JK_NoOriginalBillsCalcEdit.TabIndex = 5;
			this.JK_NoOriginalBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "JK_ReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ReleaseType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 52, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.zDropEdit1.TabIndex = 4;
			// 
			// JK_OverrideConsolChargeableCheckBox
			// 
			this.JK_OverrideConsolChargeableCheckBox.AutoSize = true;
			this.JK_OverrideConsolChargeableCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_OverrideConsolChargeableCheckBox, "JK_OverrideConsolChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OverrideConsolChargeable)));
			this.JK_OverrideConsolChargeableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JK_OverrideConsolChargeableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 12, true);
			this.JK_OverrideConsolChargeableCheckBox.Name = "JK_OverrideConsolChargeableCheckBox";
			this.JK_OverrideConsolChargeableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.JK_OverrideConsolChargeableCheckBox.TabIndex = 0;
			this.JK_OverrideConsolChargeableCheckBox.UseVisualStyleBackColor = false;
			////
			//// JK_Calc_VolumeWeightEdit
			////
			this.BindingSource.SetBindingMember(this.JK_Calc_ActualVolumeWeightEdit, "JK_Calc_ActualVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ActualVolumeWeight)));
			this.JK_Calc_ActualVolumeWeightEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 10, true);
			this.JK_Calc_ActualVolumeWeightEdit.Name = "JK_Calc_ActualVolumeWeightEdit";
			this.JK_Calc_ActualVolumeWeightEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.JK_Calc_ActualVolumeWeightEdit.TabIndex = 2;
			this.JK_Calc_ActualVolumeWeightEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JK_Calc_VolumeWeightUnitTextBox
			//
			this.BindingSource.SetBindingMember(this.JK_Calc_ActualVolumeWeightUnitTextBox, "JK_Calc_ActualVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the returjkn type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ActualVolumeWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_Calc_ActualVolumeWeightUnitTextBox, false);
			this.JK_Calc_ActualVolumeWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 10, true);
			this.JK_Calc_ActualVolumeWeightUnitTextBox.Name = "JK_Calc_ActualVolumeWeightUnitTextBox";
			this.JK_Calc_ActualVolumeWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.JK_Calc_ActualVolumeWeightUnitTextBox.TabIndex = 3;
			// 
			// CorrectedConsolWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CorrectedConsolWeightCalcEdit, "JK_CorrectedConsolWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CorrectedConsolWeight)));
			this.CorrectedConsolWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 35, true);
			this.CorrectedConsolWeightCalcEdit.Name = "CorrectedConsolWeightCalcEdit";
			this.CorrectedConsolWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.CorrectedConsolWeightCalcEdit.TabIndex = 4;
			this.CorrectedConsolWeightCalcEdit.Text = "0.000";
			this.CorrectedConsolWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JK_Calc_ConsolidatedFreightCostChargeableTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_Calc_ConsolidatedFreightCostChargeableTextBox, "JK_Calc_ConsolidatedFreightCostChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ConsolidatedFreightCostChargeable)));
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.DecimalPlaces = 2;
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 112, true);
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.Name = "JK_Calc_ConsolidatedFreightCostChargeableTextBox";
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.TabIndex = 16;
			this.JK_Calc_ConsolidatedFreightCostChargeableTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JK_Calc_ConsolidatedFreightCostChargeableDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox, "JK_Calc_ConsolidatedFreightCostChargeableDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ConsolidatedFreightCostChargeableDesc)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox, false);
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 112, true);
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.Name = "JK_Calc_ConsolidatedFreightCostChargeableDescTextBox";
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.JK_Calc_ConsolidatedFreightCostChargeableDescTextBox.TabIndex = 17;
			// 
			// JK_Calc_ShipmentFreightCostChargeable
			// 
			this.BindingSource.SetBindingMember(this.JK_Calc_ShipmentFreightCostChargeableTextBox, "JK_Calc_ShipmentFreightCostChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ShipmentFreightCostChargeable)));
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.DecimalPlaces = 2;
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 131, true);
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.Name = "JK_Calc_ShipmentFreightCostChargeableTextBox";
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.TabIndex = 20;
			this.JK_Calc_ShipmentFreightCostChargeableTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JK_Calc_ShipmentFreightCostChargeableDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_Calc_ShipmentFreightCostChargeableDescTextBox, "JK_Calc_ShipmentFreightCostChargeableDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_ShipmentFreightCostChargeableDesc)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_Calc_ShipmentFreightCostChargeableDescTextBox, false);
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 131, true);
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.Name = "JK_Calc_ShipmentFreightCostChargeableDescTextBox";
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.JK_Calc_ShipmentFreightCostChargeableDescTextBox.TabIndex = 21;
			// 
			// CorrectedConsolWeightUnitDropEdit
			// 
			this.CorrectedConsolWeightUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CorrectedConsolWeightUnitDropEdit, "JK_CorrectedConsolWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CorrectedConsolWeightUnit)));
			this.CorrectedConsolWeightUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 35, true);
			this.CorrectedConsolWeightUnitDropEdit.ShowDescriptionBox = false;
			this.CorrectedConsolWeightUnitDropEdit.Name = "CorrectedConsolWeightUnitDropEdit";
			this.CorrectedConsolWeightUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.CorrectedConsolWeightUnitDropEdit.TabIndex = 5;
			//
			// ExcessVolumeWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ExcessVolumeWeightCalcEdit, "Density+ExcessVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Density.ExcessVolumeWeight)));
			this.ExcessVolumeWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 35, true);
			this.ExcessVolumeWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|de48be4b-dfd0-4510-85fb-f2c6dea153f0", "Excess Weight/Volume", "Excess Weight or Volume is calculated as a difference between Consolidated Weight (or Volume) and its Volume Weight (or Weight Volume), whichever is greater.");
			this.ExcessVolumeWeightCalcEdit.Name = "ExcessVolumeWeightCalcEdit";
			this.ExcessVolumeWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ExcessVolumeWeightCalcEdit.TabIndex = 6;
			//
			// ExcessVolumeWeightUnitLabel
			//
			this.BindingSource.SetBindingMember(this.ExcessVolumeWeightUnitTextBox, "Density+ExcessVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Density.ExcessVolumeWeightUnit)));
			this.ExcessVolumeWeightUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|5a0eb014-7c4f-4600-a3f0-7fd4ff15ff85", "Excess Weight Unit");
			this.ExcessVolumeWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 35, true);
			this.ExcessVolumeWeightUnitTextBox.Name = "ExcessVolumeWeightUnitTextBox";
			this.ExcessVolumeWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.ExcessVolumeWeightUnitTextBox.TabIndex = 7;
			// 
			// ChargesApply
			// 
			this.ChargesApply.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargesApply, "JK_MBLAWBChargesDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MBLAWBChargesDisplay)));
			this.ChargesApply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 76, true);
			this.ChargesApply.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ChargesApply.Name = "ChargesApply";
			this.ChargesApply.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|788ece74-e9dc-b8a2-4b8a-f362d354b975", "Charges Apply");
			this.ChargesApply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.ChargesApply.TabIndex = 7;
			// 
			// CorrectedConsolVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CorrectedConsolVolumeCalcEdit, "JK_CorrectedConsolVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CorrectedConsolVolume)));
			this.CorrectedConsolVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 58, true);
			this.CorrectedConsolVolumeCalcEdit.Name = "CorrectedConsolVolumeCalcEdit";
			this.CorrectedConsolVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.CorrectedConsolVolumeCalcEdit.TabIndex = 8;
			this.CorrectedConsolVolumeCalcEdit.Text = "0.000";
			this.CorrectedConsolVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CorrectedConsolVolumeUnitDropEdit
			// 
			this.CorrectedConsolVolumeUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CorrectedConsolVolumeUnitDropEdit, "JK_CorrectedConsolVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CorrectedConsolVolumeUnit)));
			this.CorrectedConsolVolumeUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 58, true);
			this.CorrectedConsolVolumeUnitDropEdit.Name = "CorrectedConsolVolumeUnitDropEdit";
			this.CorrectedConsolVolumeUnitDropEdit.ShowDescriptionBox = false;
			this.CorrectedConsolVolumeUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.CorrectedConsolVolumeUnitDropEdit.TabIndex = 9;
			// 
			// ConsolChargeableRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsolChargeableRateCalcEdit, "JK_ConsolChargeableRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolChargeableRate)));
			this.ConsolChargeableRateCalcEdit.DecimalPlaces = 2;
			this.ConsolChargeableRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 98, true);
			this.ConsolChargeableRateCalcEdit.Name = "ConsolChargeableRateCalcEdit";
			this.ConsolChargeableRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ConsolChargeableRateCalcEdit.TabIndex = 11;
			this.ConsolChargeableRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsolChargeableQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsolChargeableQuantityCalcEdit, "JK_ConsolChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolChargeable)));
			this.ConsolChargeableQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 81, true);
			this.ConsolChargeableQuantityCalcEdit.Name = "ConsolChargeableQuantityCalcEdit";
			this.ConsolChargeableQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ConsolChargeableQuantityCalcEdit.TabIndex = 12;
			this.ConsolChargeableQuantityCalcEdit.Text = "0.000";
			this.ConsolChargeableQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// SpecialHandlingUserControl
			//
			this.BindingSource.SetBindingMember(this.SpecialHandlingUserControl, "AWBSpecialHandlingItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.JobConsolAWBSpecialHandling)(((System.Collections.IList)((Enterprise.Freight.Forwarding.Business.ForwardingConsol)null).AWBSpecialHandlingItems).SyncRoot)).JKH_Code);
			this.SpecialHandlingUserControl.Name = "SpecialHandlingUserControl";
			this.SpecialHandlingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 118, true);
			this.SpecialHandlingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 85, true);
			this.SpecialHandlingUserControl.TabIndex = 15;
			// 
			// SecurityStatusDropEdit
			// 
			this.SecurityStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityStatusDropEdit, "SecurityStatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SecurityStatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|fe09ac98-c5ac-497a-86a9-a77cecb55739", "Security Status");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).SecurityStatusCode)));
			this.SecurityStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 98, true);
			this.SecurityStatusDropEdit.Name = "SecurityStatusDropEdit";
			this.SecurityStatusDropEdit.ShowDescriptionBox = true;
			this.SecurityStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.SecurityStatusDropEdit.TabIndex = 13;
			//
			// WeightUtilisationPercentageBar
			//
			this.WeightUtilisationPercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 58, true);
			this.WeightUtilisationPercentageBar.Name = "WeightUtilisationPercentageBar";
			this.WeightUtilisationPercentageBar.TabIndex = 11;
			//
			// WeightUtilisationLabel
			//
			this.WeightUtilisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 58, true);
			this.WeightUtilisationLabel.Name = "WeightUtilisationLabel";
			this.WeightUtilisationLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|8783fbcf-a139-4fcf-aa52-0a51497e1395", "Weight Utilization");
			this.WeightUtilisationLabel.TabIndex = 10;
			this.WeightUtilisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			//
			// VolumeUtilisationPercentageBar
			//
			this.VolumeUtilisationPercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 81, true);
			this.VolumeUtilisationPercentageBar.Name = "VolumeUtilisationPercentageBar";
			this.VolumeUtilisationPercentageBar.TabIndex = 15;
			//
			// VolumeUtilisationLabel
			//	
			this.VolumeUtilisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 81, true);
			this.VolumeUtilisationLabel.Name = "VolumeUtilisationLabel";
			this.VolumeUtilisationLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|a232a755-f740-44f9-885e-9bd791f99239", "Volume Utilization");
			this.VolumeUtilisationLabel.TabIndex = 14;
			this.VolumeUtilisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			//
			// JK_Calc_FreeSpaceEdit
			//
			this.BindingSource.SetBindingMember(this.JK_Calc_FreeSpaceEdit, "JK_Calc_FreeSpace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_FreeSpace)));
			this.JK_Calc_FreeSpaceEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 131, true);
			this.JK_Calc_FreeSpaceEdit.Name = "JK_Calc_FreeSpaceEdit";
			this.JK_Calc_FreeSpaceEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.JK_Calc_FreeSpaceEdit.TabIndex = 22;
			this.JK_Calc_FreeSpaceEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JK_Calc_FreeSpaceUnitTextBox
			//
			this.BindingSource.SetBindingMember(this.JK_Calc_FreeSpaceUnitTextBox, "JK_ConsolChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolChargeableUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_Calc_FreeSpaceUnitTextBox, false);
			this.JK_Calc_FreeSpaceUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 131, true);
			this.JK_Calc_FreeSpaceUnitTextBox.Name = "JK_Calc_FreeSpaceUnitTextBox";
			this.JK_Calc_FreeSpaceUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.JK_Calc_FreeSpaceUnitTextBox.TabIndex = 23;
			//
			// CostFreePercentageBar
			//
			this.CostFreePercentageBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 112, true);
			this.CostFreePercentageBar.Name = "CostFreePercentageBar";
			this.CostFreePercentageBar.TabIndex = 19;
			//
			// CostFreeLabel
			//
			this.CostFreeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 112, true);
			this.CostFreeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|e8e0305c-31ee-47f0-8b06-6d4bfa860c22", "Cost Free");
			this.CostFreeLabel.TabIndex = 18;
			this.CostFreeLabel.Name = "CostFreeLabel";
			this.CostFreeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17);
			//
			// DensityFactorLabel
			//
			this.DensityFactorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 176, true);
			this.DensityFactorLabel.TabIndex = 24;
			this.DensityFactorLabel.Name = "DensityFactorLabel";
			this.DensityFactorLabel.AutoSize = true;
			this.DensityFactorLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|b254f7c6-2922-4fb6-9e10-3cd04656f2d9", "Density Factor");
			//
			// DensityVisualisationControl
			//
			this.BindingSource.SetBindingMember(this.DensityVisualisationControl, "Density");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.Density)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Density)));
			this.DensityVisualisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 155, true);
			this.DensityVisualisationControl.TabIndex = 25;
			this.DensityVisualisationControl.ConfigureWidth(347);
			// 
			// MasterBillIssueDateEdit
			// 
			this.MasterBillIssueDateEdit.AllowDrop = true;
			this.MasterBillIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.MasterBillIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MasterBillIssueDateEdit, "JK_MasterBillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MasterBillIssueDate)));
			this.MasterBillIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 76, true);
			this.MasterBillIssueDateEdit.Name = "MasterBillIssueDateEdit";
			this.MasterBillIssueDateEdit.TabIndex = 9;
			// 
			// AWBDimsDropEdit
			// 
			this.AWBDimsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBDimsDropEdit, "JK_PrintOptionForPackagesOnAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PrintOptionForPackagesOnAWB)));
			this.AWBDimsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 29, true);
			this.AWBDimsDropEdit.Name = "AWBDimsDropEdit";
			this.AWBDimsDropEdit.PreBoundMaxLength = 3;
			this.AWBDimsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.AWBDimsDropEdit.TabIndex = 2;
			// 
			// PackageGroupingDropEdit
			// 
			this.PackageGroupingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageGroupingDropEdit, "JK_PackageGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.PackageGroupingDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("6cf5598c-15b4-49af-888d-b910344bdf51", "Package Grouping");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PackageGrouping)));
			this.PackageGroupingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 29, true);
			this.PackageGroupingDropEdit.Name = "PackageGroupingDropEdit";
			this.PackageGroupingDropEdit.PreBoundMaxLength = 3;
			this.PackageGroupingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.PackageGroupingDropEdit.TabIndex = 3;
			// 
			// JK_PrintOptionForColoadsOnOtherDocsDropEdit
			// 
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_PrintOptionForColoadsOnOtherDocsDropEdit, "JK_PrintOptionForColoadsOnOtherDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PrintOptionForColoadsOnOtherDocs)));
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 6, true);
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.Name = "JK_PrintOptionForColoadsOnOtherDocsDropEdit";
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.PreBoundMaxLength = 3;
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.ShowDescriptionBox = false;
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_PrintOptionForColoadsOnOtherDocsDropEdit.TabIndex = 1;
			// 
			// JK_PrintOptionForColoadsOnManifestDropEdit
			// 
			this.JK_PrintOptionForColoadsOnManifestDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_PrintOptionForColoadsOnManifestDropEdit, "JK_PrintOptionForColoadsOnManifest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PrintOptionForColoadsOnManifest)));
			this.JK_PrintOptionForColoadsOnManifestDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 6, true);
			this.JK_PrintOptionForColoadsOnManifestDropEdit.Name = "JK_PrintOptionForColoadsOnManifestDropEdit";
			this.JK_PrintOptionForColoadsOnManifestDropEdit.PreBoundMaxLength = 3;
			this.JK_PrintOptionForColoadsOnManifestDropEdit.ShowDescriptionBox = false;
			this.JK_PrintOptionForColoadsOnManifestDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_PrintOptionForColoadsOnManifestDropEdit.TabIndex = 0;
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|9ddc2801-0ac3-40fc-8a85-b5beb7baa2a6", "Numbers");
			this.NumbersTabPage.Controls.Add(referenceNumbersControl);
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NumbersTabPage.Name = "NumbersTabPage";
			this.NumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.NumbersTabPage.TabIndex = 7;
			//
			// RatesTabPage
			//
			this.RatesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|8f1e3613-23b7-4877-91e7-428876aa63fb", "Rates");
			this.RatesTabPage.Controls.Add(AutoratingDateOverriddenDateEdit);
			this.RatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RatesTabPage.Name = "RatesTabPage";
			this.RatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.RatesTabPage.TabIndex = 8;
			// 
			// CustomDatesTabPage
			// 
			this.CustomDatesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|91866440-6734-4a0c-92b5-f935d559b30b", "Additional Detail");
			this.CustomDatesTabPage.Controls.Add(this.ConsolCustomFields);
			this.CustomDatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomDatesTabPage.Name = "CustomDatesTabPage";
			this.CustomDatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.CustomDatesTabPage.TabIndex = 5;
			// 
			// ConsolCustomFields
			// 
			this.ConsolCustomFields.AllowDrop = true;
			this.ConsolCustomFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCustomFields.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolCustomFields.Name = "ConsolCustomFields";
			this.ConsolCustomFields.NothingSetupMessageLabelText = "";
			this.ConsolCustomFields.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 205, true);
			this.ConsolCustomFields.TabIndex = 0;
			// 
			// JK_OA_DeparturePackCFSTransportAddressControl
			// 
			this.JK_OA_DeparturePackCFSTransportAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_DeparturePackCFSTransportAddressControl, "JK_OA_DeparturePackCFSTransportAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_DeparturePackCFSTransportAddress)));
			this.JK_OA_DeparturePackCFSTransportAddressControl.BindToOrgList = "LocalTransportList";
			this.JK_OA_DeparturePackCFSTransportAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|e86c84be-16a0-4492-949b-7b09caede8a3", "Port Transport", "Departure Port Transport", "");
			this.JK_OA_DeparturePackCFSTransportAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 123, true);
			this.JK_OA_DeparturePackCFSTransportAddressControl.Name = "JK_OA_DeparturePackCFSTransportAddressControl";
			this.JK_OA_DeparturePackCFSTransportAddressControl.PopupCaption = "";
			this.JK_OA_DeparturePackCFSTransportAddressControl.ShowAddress = false;
			this.JK_OA_DeparturePackCFSTransportAddressControl.ShowOrganisationName = true;
			this.JK_OA_DeparturePackCFSTransportAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_DeparturePackCFSTransportAddressControl.TabIndex = 8;
			// 
			// JK_OA_ArrivalUnpackCFSTransportAddressControl
			// 
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_OA_ArrivalUnpackCFSTransportAddressControl, "JK_OA_ArrivalUnpackCFSTransportAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_OA_ArrivalUnpackCFSTransportAddress)));
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.BindToOrgList = "LocalTransportList";
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|eeb2e8cc-fde5-49e5-8fd4-a0d31e8f47ae", "Port Transport", "Arrival Port Transport", "");
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 123, true);
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.Name = "JK_OA_ArrivalUnpackCFSTransportAddressControl";
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.PopupCaption = null;
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.ShowAddress = false;
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.ShowOrganisationName = true;
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			this.JK_OA_ArrivalUnpackCFSTransportAddressControl.TabIndex = 8;
			// 
			// JK_PrepaidCollectDropDownEdit
			// 
			this.JK_PrepaidCollectDropDownEdit.AllowDrop = true;
			this.JK_PrepaidCollectDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JK_PrepaidCollectDropDownEdit, "JK_PrepaidCollect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_PrepaidCollect)));
			this.JK_PrepaidCollectDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 32, true);
			this.JK_PrepaidCollectDropDownEdit.Name = "JK_PrepaidCollectDropDownEdit";
			this.JK_PrepaidCollectDropDownEdit.PreBoundMaxLength = 3;
			this.JK_PrepaidCollectDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JK_PrepaidCollectDropDownEdit.TabIndex = 2;
			// 
			// CRNPanelSea
			// 
			this.CRNPanelSea.Controls.Add(this.zDropEdit2);
			this.CRNPanelSea.Controls.Add(this.JK_MasterBillNumTextBox);
			this.CRNPanelSea.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CRNPanelSea.Name = "CRNPanelSea";
			this.CRNPanelSea.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 32, true);
			this.CRNPanelSea.TabIndex = 0;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "JK_AWBServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_AWBServiceLevel)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 6, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.zDropEdit2.TabIndex = 2;
			// 
			// JK_MasterBillNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_MasterBillNumTextBox, "JK_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MasterBillNum)));
			this.JK_MasterBillNumTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|3f010a99-cd72-45e1-86e9-b4bf72bacd3f", "BOL");
			this.JK_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 8, true);
			this.JK_MasterBillNumTextBox.Name = "JK_MasterBillNumTextBox";
			this.JK_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.JK_MasterBillNumTextBox.TabIndex = 1;
			// 
			// CRNLabel
			// 
			this.CRNLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CRNLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CRNLabel, "JK_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_EntryType)));
			this.CRNLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|854d5ee6-85ec-4d4d-ab34-e501c3a045ba", "CRN:");
			this.CRNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 35, true);
			this.CRNLabel.Name = "CRNLabel";
			this.CRNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.CRNLabel.TabIndex = 3;
			// 
			// JK_CRNBoundTextBox
			// 
			this.JK_CRNBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JK_CRNBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_CRNBoundTextBox, "JK_CRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_CRN)));
			this.JK_CRNBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|3ca22b47-0d39-407f-a01b-4068897dbe32", "CRN");
			this.JK_CRNBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 32, true);
			this.JK_CRNBoundTextBox.Name = "JK_CRNBoundTextBox";
			this.JK_CRNBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.JK_CRNBoundTextBox.TabIndex = 4;
			// 
			// JK_TotalShipmentWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentWeightCalcEdit, "JK_TotalShipmentWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentWeight)));
			this.JK_TotalShipmentWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|204ca6e9-6331-4a36-b86c-bc8c182a1cb7", "Weight");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalShipmentWeightCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalShipmentWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 15, true);
			this.JK_TotalShipmentWeightCalcEdit.Name = "JK_TotalShipmentWeightCalcEdit";
			this.JK_TotalShipmentWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JK_TotalShipmentWeightCalcEdit.TabIndex = 3;
			this.JK_TotalShipmentWeightCalcEdit.Text = "0.000";
			this.JK_TotalShipmentWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalShipmentWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentWeightUnitTextBox, "JK_TotalShipmentWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentWeightUnit)));
			this.JK_TotalShipmentWeightUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|3a6c6005-14fc-415e-b7b3-56e49d082a72", "Total Shipment Weight Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalShipmentWeightUnitTextBox, false);
			this.JK_TotalShipmentWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 15, true);
			this.JK_TotalShipmentWeightUnitTextBox.Name = "JK_TotalShipmentWeightUnitTextBox";
			this.JK_TotalShipmentWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.JK_TotalShipmentWeightUnitTextBox.TabIndex = 4;
			// 
			// JK_TotalShipmentVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentVolumeCalcEdit, "JK_TotalShipmentVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentVolume)));
			this.JK_TotalShipmentVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|09c76dc3-09bb-4054-9c75-28a37465e2cd", "Volume");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalShipmentVolumeCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalShipmentVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 15, true);
			this.JK_TotalShipmentVolumeCalcEdit.Name = "JK_TotalShipmentVolumeCalcEdit";
			this.JK_TotalShipmentVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JK_TotalShipmentVolumeCalcEdit.TabIndex = 5;
			this.JK_TotalShipmentVolumeCalcEdit.Text = "0.000";
			this.JK_TotalShipmentVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalShipmentVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentVolumeUnitTextBox, "JK_TotalShipmentVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentVolumeUnit)));
			this.JK_TotalShipmentVolumeUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|4cbc1f1e-c78b-4211-a215-0772f3bb0230", "Total Shipment Volume Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalShipmentVolumeUnitTextBox, false);
			this.JK_TotalShipmentVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 15, true);
			this.JK_TotalShipmentVolumeUnitTextBox.Name = "JK_TotalShipmentVolumeUnitTextBox";
			this.JK_TotalShipmentVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.JK_TotalShipmentVolumeUnitTextBox.TabIndex = 6;
			// 
			// ShowSubHouseBillsCheckBox
			// 
			this.ShowSubHouseBillsCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShowSubHouseBillsCheckBox, "ShowSubHouseBillShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ShowSubHouseBillShipments)));
			this.ShowSubHouseBillsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|08f172b6-3f81-4454-8a59-f1537a8fc739", "Show Sub\'s", "Show/Hide Sub Shipments on the grid linked to Master Shipments");
			this.ShowSubHouseBillsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowSubHouseBillsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.ShowSubHouseBillsCheckBox.Name = "ShowSubHouseBillsCheckBox";
			this.ShowSubHouseBillsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.ShowSubHouseBillsCheckBox.TabIndex = 0;
			this.ShowSubHouseBillsCheckBox.UseVisualStyleBackColor = false;
			// 
			// ConsolDetailsGroupBox
			// 
			this.ConsolDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|3a4a621c-56e8-422a-8601-2dfce388cd64", "Consol Details");
			this.ConsolDetailsGroupBox.Controls.Add(this.consolLeg1);
			this.ConsolDetailsGroupBox.Controls.Add(this.DetailsBottomPanel);
			this.ConsolDetailsGroupBox.Controls.Add(this.DetailsTopPanel);
			this.ConsolDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ConsolDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolDetailsGroupBox.Name = "ConsolDetailsGroupBox";
			this.ConsolDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 276, true);
			this.ConsolDetailsGroupBox.TabIndex = 0;
			this.ConsolDetailsGroupBox.TabStop = false;
			// 
			// consolLeg1
			// 
			this.consolLeg1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consolLeg1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonConsol)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)))));
			this.consolLeg1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consolLeg1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.consolLeg1.Name = "consolLeg1";
			this.consolLeg1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 110, true);
			this.consolLeg1.TabIndex = 1;
			// 
			// DetailsBottomPanel
			// 
			this.DetailsBottomPanel.Controls.Add(this.ScreenButton);
			this.DetailsBottomPanel.Controls.Add(this.JK_CRNBoundTextBox);
			this.DetailsBottomPanel.Controls.Add(this.JK_ScreeningStatusDropEdit);
			this.DetailsBottomPanel.Controls.Add(this.JK_PrepaidCollectDropDownEdit);
			this.DetailsBottomPanel.Controls.Add(this.CRNLabel);
			this.DetailsBottomPanel.Controls.Add(this.AirConsolPanel);
			this.DetailsBottomPanel.Controls.Add(this.CharterPanel);
			this.DetailsBottomPanel.Controls.Add(this.CRNPanelSea);
			this.DetailsBottomPanel.Controls.Add(this.TotalCO2eTextBox);
			this.DetailsBottomPanel.Controls.Add(this.TotalCO2eUnitLabel);
			this.DetailsBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DetailsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 173, true);
			this.DetailsBottomPanel.Name = "DetailsBottomPanel";
			this.DetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 80, true);
			this.DetailsBottomPanel.TabIndex = 3;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 33, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 12;
			this.ScreenButton.Text = "...";
			this.ScreenButton.UseVisualStyleBackColor = false;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// JK_ScreeningStatusDropEdit
			// 
			this.JK_ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_ScreeningStatusDropEdit, "JK_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ScreeningStatus)));
			this.JK_ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 33, true);
			this.JK_ScreeningStatusDropEdit.Name = "JK_ScreeningStatusDropEdit";
			this.JK_ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.JK_ScreeningStatusDropEdit.ShowDescriptionBox = false;
			this.JK_ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_ScreeningStatusDropEdit.TabIndex = 11;
			// 
			// AirConsolPanel
			// 
			this.AirConsolPanel.Controls.Add(this.MAWBPendingAllocationTextBox);
			this.AirConsolPanel.Controls.Add(this.JK_AWBServiceLevelDropEdit);
			this.AirConsolPanel.Controls.Add(this.AirlinePrefixTextBox);
			this.AirConsolPanel.Controls.Add(this.MAWBNumberTextBox);
			this.AirConsolPanel.Controls.Add(this.MAWBHyphenLabel);
			this.AirConsolPanel.Controls.Add(this.IsNeutralCheckBox);
			this.AirConsolPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AirConsolPanel.Name = "AirConsolPanel";
			this.AirConsolPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 32, true);
			this.AirConsolPanel.TabIndex = 0;
			// 
			// JK_AWBServiceLevelDropEdit
			// 
			this.JK_AWBServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_AWBServiceLevelDropEdit, "JK_AWBServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_AWBServiceLevel)));
			this.JK_AWBServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 8, true);
			this.JK_AWBServiceLevelDropEdit.Name = "JK_AWBServiceLevelDropEdit";
			this.JK_AWBServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.JK_AWBServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JK_AWBServiceLevelDropEdit.TabIndex = 1;
			// 
			// AirlinePrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirlinePrefixTextBox, "MasterBillAirlinePrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).MasterBillAirlinePrefix)));
			this.AirlinePrefixTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|0333355f-0817-49b3-ad26-e7807f03686f", "MAWB");
			this.AirlinePrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 8, true);
			this.AirlinePrefixTextBox.Name = "AirlinePrefixTextBox";
			this.AirlinePrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.AirlinePrefixTextBox.TabIndex = 3;
			// 
			// MAWBNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBNumberTextBox, "MasterBillMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).MasterBillMAWB)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MAWBNumberTextBox, false);
			this.MAWBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 8, true);
			this.MAWBNumberTextBox.Name = "MAWBNumberTextBox";
			this.MAWBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.MAWBNumberTextBox.TabIndex = 4;
			// 
			// MAWBHyphenLabel
			// 
			this.MAWBHyphenLabel.IsFontBold = true;
			this.MAWBHyphenLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			this.MAWBHyphenLabel.Name = "MAWBHyphenLabel";
			this.MAWBHyphenLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 20, true);
			this.MAWBHyphenLabel.TabIndex = 4;
			this.MAWBHyphenLabel.Text = "-";
			this.MAWBHyphenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MAWBPendingAllocationTextBox
			// 
			this.MAWBPendingAllocationTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MAWBPendingAllocationTextBox, "MasterBillNeutralMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).MasterBillNeutralMAWB)));
			this.MAWBPendingAllocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MAWBPendingAllocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 8, true);
			this.MAWBPendingAllocationTextBox.Name = "MAWBPendingAllocationTextBox";
			this.MAWBPendingAllocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.MAWBPendingAllocationTextBox.Visible = false;
			this.MAWBPendingAllocationTextBox.TabIndex = 4;
			// 
			// IsNeutralCheckBox
			// 
			this.IsNeutralCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsNeutralCheckBox, "JK_IsNeutralMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_IsNeutralMaster)));
			this.IsNeutralCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|56de8c76-398d-4dc7-8981-a27b37db5e37", "Neutral MAWB");
			this.IsNeutralCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsNeutralCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 8, true);
			this.IsNeutralCheckBox.Name = "IsNeutralCheckBox";
			this.IsNeutralCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.IsNeutralCheckBox.TabIndex = 5;
			this.IsNeutralCheckBox.CheckedChanged += IsNeutralCheckBox_CheckedChanged;
			// 
			// CharterPanel
			// 
			this.CharterPanel.Controls.Add(this.MAWBTextBox);
			this.CharterPanel.Controls.Add(this.MawbLabel);
			this.CharterPanel.Controls.Add(this.AircraftRegoLabel);
			this.CharterPanel.Controls.Add(this.JK_AircraftRegoBoundTextBox);
			this.CharterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CharterPanel.Name = "CharterPanel";
			this.CharterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 32, true);
			this.CharterPanel.TabIndex = 0;
			// 
			// MAWBTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBTextBox, "JK_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MasterBillNum)));
			this.MAWBTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|8d72b707-b16e-4f05-a158-d1ce53863bef", "MAWB");
			this.MAWBTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 8, true);
			this.MAWBTextBox.Name = "MAWBTextBox";
			this.MAWBTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.MAWBTextBox.TabIndex = 1;
			// 
			// MawbLabel
			// 
			this.MawbLabel.AutoSize = true;
			this.MawbLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|57985ddf-b5b7-47c4-abcc-c42338a236cf", "MAWB:");
			this.MawbLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 11, true);
			this.MawbLabel.Name = "MawbLabel";
			this.MawbLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.MawbLabel.TabIndex = 0;
			this.MawbLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// AircraftRegoLabel
			// 
			this.AircraftRegoLabel.AutoSize = true;
			this.AircraftRegoLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|ac67170c-7c4c-41c4-989d-a39785c03de2", "Rego.:", "Registration:.");
			this.AircraftRegoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 11, true);
			this.AircraftRegoLabel.Name = "AircraftRegoLabel";
			this.AircraftRegoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.AircraftRegoLabel.TabIndex = 0;
			// 
			// JK_AircraftRegoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_AircraftRegoBoundTextBox, "Transports.JW_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Transports)).SyncRoot)).JW_VoyageFlight)));
			this.JK_AircraftRegoBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|ea993052-ab76-4f25-8002-3126676c76dd", "Rego.", "Registration");
			this.JK_AircraftRegoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 8, true);
			this.JK_AircraftRegoBoundTextBox.Name = "JK_AircraftRegoBoundTextBox";
			this.JK_AircraftRegoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JK_AircraftRegoBoundTextBox.TabIndex = 1;
			// 
			// DetailsTopPanel
			// 
			this.DetailsTopPanel.Controls.Add(this.JK_PhaseDropEdit);
			this.DetailsTopPanel.Controls.Add(this.DomesticFreightCheckBox);
			this.DetailsTopPanel.Controls.Add(this.JK_AgentTypeBoundDropDownEdit);
			this.DetailsTopPanel.Controls.Add(this.JK_RL_NKLoadPortBoundCodeFindBox);
			this.DetailsTopPanel.Controls.Add(this.JK_TransportModeDropEdit);
			this.DetailsTopPanel.Controls.Add(this.JK_ConsolModeDropDownEdit);
			this.DetailsTopPanel.Controls.Add(this.JK_RL_NKDischargePortBoundCodeFindBox);
			this.DetailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DetailsTopPanel.Name = "DetailsTopPanel";
			this.DetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 47, true);
			this.DetailsTopPanel.TabIndex = 0;
			// 
			// JK_PhaseDropEdit
			// 
			this.JK_PhaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_PhaseDropEdit, "JK_Phase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Phase)));
			this.JK_PhaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 23, true);
			this.JK_PhaseDropEdit.Name = "JK_PhaseDropEdit";
			this.JK_PhaseDropEdit.PreBoundMaxLength = 3;
			this.JK_PhaseDropEdit.ShowDescriptionBox = false;
			this.JK_PhaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_PhaseDropEdit.TabIndex = 11;
			// 
			// DomesticFreightCheckBox
			// 
			this.DomesticFreightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DomesticFreightCheckBox, "IsDomesticFreight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).IsDomesticFreight)));
			this.DomesticFreightCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|f83ef1db-da4a-4fdb-a981-d103ba457a2b", "Dom", "Domestic", "Domestic Freight.");
			this.DomesticFreightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DomesticFreightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 26, true);
			this.DomesticFreightCheckBox.Name = "DomesticFreightCheckBox";
			this.DomesticFreightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.DomesticFreightCheckBox.TabIndex = 6;
			this.DomesticFreightCheckBox.UseVisualStyleBackColor = true;
			// 
			// JK_AgentTypeBoundDropDownEdit
			// 
			this.JK_AgentTypeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_AgentTypeBoundDropDownEdit, "JK_AgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_AgentType)));
			this.JK_AgentTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 0, true);
			this.JK_AgentTypeBoundDropDownEdit.Name = "JK_AgentTypeBoundDropDownEdit";
			this.JK_AgentTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JK_AgentTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JK_AgentTypeBoundDropDownEdit.TabIndex = 1;
			this.JK_AgentTypeBoundDropDownEdit.SelectedIndexChanged += new System.EventHandler(this.JK_AgentTypeBoundDropDownEdit_SelectedIndexChanged);
			// 
			// JK_RL_NKLoadPortBoundCodeFindBox
			//
			this.JK_RL_NKLoadPortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKLoadPortBoundCodeFindBox, "JK_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKLoadPort)));
			this.JK_RL_NKLoadPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 24, true);
			this.JK_RL_NKLoadPortBoundCodeFindBox.Name = "JK_RL_NKLoadPortBoundCodeFindBox";
			this.JK_RL_NKLoadPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RL_NKLoadPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKLoadPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKLoadPortBoundCodeFindBox.TabIndex = 8;
			// 
			// JK_TransportModeDropEdit
			// 
			this.JK_TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_TransportModeDropEdit, "JK_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TransportMode)));
			this.JK_TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 0, true);
			this.JK_TransportModeDropEdit.Name = "JK_TransportModeDropEdit";
			this.JK_TransportModeDropEdit.PreBoundMaxLength = 3;
			this.JK_TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.JK_TransportModeDropEdit.TabIndex = 3;
			// 
			// JK_ConsolModeDropDownEdit
			// 
			this.JK_ConsolModeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_ConsolModeDropDownEdit, "JK_ConsolMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_ConsolMode)));
			this.JK_ConsolModeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 0, true);
			this.JK_ConsolModeDropDownEdit.Name = "JK_ConsolModeDropDownEdit";
			this.JK_ConsolModeDropDownEdit.PreBoundMaxLength = 3;
			this.JK_ConsolModeDropDownEdit.ShowDescriptionBox = false;
			this.JK_ConsolModeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JK_ConsolModeDropDownEdit.TabIndex = 5;
			// 
			// JK_RL_NKDischargePortBoundCodeFindBox
			// 
			this.JK_RL_NKDischargePortBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JK_RL_NKDischargePortBoundCodeFindBox, "JK_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_RL_NKDischargePort)));
			this.JK_RL_NKDischargePortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 24, true);
			this.JK_RL_NKDischargePortBoundCodeFindBox.Name = "JK_RL_NKDischargePortBoundCodeFindBox";
			this.JK_RL_NKDischargePortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JK_RL_NKDischargePortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JK_RL_NKDischargePortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JK_RL_NKDischargePortBoundCodeFindBox.TabIndex = 10;
			// 
			// JK_TotalChargeableCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalChargeableCalcEdit, "JK_TotalShipmentChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentChargeable)));
			this.JK_TotalChargeableCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|c6c704ce-19dd-47d6-864f-984a78f68e02", "Chargeable");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalChargeableCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.JK_TotalChargeableCalcEdit.Name = "JK_TotalChargeableCalcEdit";
			this.JK_TotalChargeableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.JK_TotalChargeableCalcEdit.TabIndex = 7;
			this.JK_TotalChargeableCalcEdit.Text = "0.000";
			this.JK_TotalChargeableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalChargeableUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalChargeableUnitTextBox, "JK_Calc_TotalShipmentChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalShipmentChargeableUnit)));
			this.JK_TotalChargeableUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|c362d39f-bc2a-4f11-b8f3-d53face6aa22", "Total Chargeable Unit");
			this.JK_TotalChargeableUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 15, true);
			this.JK_TotalChargeableUnitTextBox.Name = "JK_TotalChargeableUnitTextBox";
			this.JK_TotalChargeableUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalChargeableUnitTextBox.TabIndex = 8;
			// 
			// JK_TotalPrepaidChargeableAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalPrepaidChargeableAmountCalcEdit, "JK_TotalPrepaidShipmentChargeableAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalPrepaidShipmentChargeableAmount)));
			this.JK_TotalPrepaidChargeableAmountCalcEdit.BindToDecimalPlaces = "JK_TotalPrepaidShipmentChargeableAmountDecimals";
			this.JK_TotalPrepaidChargeableAmountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|f6ef626b-77c6-4bdc-8918-a5f2d8ab1c70", "Prepaid");
			this.JK_TotalPrepaidChargeableAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalPrepaidChargeableAmountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalPrepaidChargeableAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 15, true);
			this.JK_TotalPrepaidChargeableAmountCalcEdit.Name = "JK_TotalPrepaidChargeableAmountCalcEdit";
			this.JK_TotalPrepaidChargeableAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JK_TotalPrepaidChargeableAmountCalcEdit.TabIndex = 9;
			this.JK_TotalPrepaidChargeableAmountCalcEdit.Text = "0.00";
			this.JK_TotalPrepaidChargeableAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox, "JK_TotalPrepaidShipmentChargeableAmountCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalPrepaidShipmentChargeableAmountCurrencyCode)));
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ca4b0cb3-d882-465c-9039-449a02794f6a", "Total Chargeable Prepaid Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox, false);
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 15, true);
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox.Name = "JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox";
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox.TabIndex = 10;
			// 
			// JK_TotalCollectChargeableAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalCollectChargeableAmountCalcEdit, "JK_TotalCollectShipmentChargeableAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalCollectShipmentChargeableAmount)));
			this.JK_TotalCollectChargeableAmountCalcEdit.BindToDecimalPlaces = "JK_TotalCollectShipmentChargeableAmountDecimals";
			this.JK_TotalCollectChargeableAmountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|5c1d9c3e-9fdc-4fd8-b071-66ccc8dc514a", "Collect");
			this.JK_TotalCollectChargeableAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalCollectChargeableAmountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalCollectChargeableAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 15, true);
			this.JK_TotalCollectChargeableAmountCalcEdit.Name = "JK_TotalCollectChargeableAmountCalcEdit";
			this.JK_TotalCollectChargeableAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JK_TotalCollectChargeableAmountCalcEdit.TabIndex = 11;
			this.JK_TotalCollectChargeableAmountCalcEdit.Text = "0.00";
			this.JK_TotalCollectChargeableAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalCollectChargeableAmountCurrencyCode
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalCollectChargeableAmountCurrencyCode, "JK_TotalCollectShipmentChargeableAmountCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalCollectShipmentChargeableAmountCurrencyCode)));
			this.JK_TotalCollectChargeableAmountCurrencyCode.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b140f292-6ef5-42cf-b86c-5438c9a06cfd", "Total Chargeable Collect Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalCollectChargeableAmountCurrencyCode, false);
			this.JK_TotalCollectChargeableAmountCurrencyCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 15, true);
			this.JK_TotalCollectChargeableAmountCurrencyCode.Name = "JK_TotalCollectChargeableAmountCurrencyCode";
			this.JK_TotalCollectChargeableAmountCurrencyCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalCollectChargeableAmountCurrencyCode.TabIndex = 12;
			// 
			// ShipmentDetailsBottomPanel
			// 
			this.ShipmentDetailsBottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentDetailsBottomPanel.Controls.Add(this.BottomInnerRightPanel);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.ShowSubHouseBillsCheckBox);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.ShipmentCountCalcEdit);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_Calc_TotalShipmentQuantityBoundCalcEdit1);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_TotalShipmentWeightCalcEdit);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_TotalShipmentWeightUnitTextBox);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_TotalShipmentVolumeCalcEdit);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_TotalShipmentVolumeUnitTextBox);
			this.ShipmentDetailsBottomPanel.Controls.Add(this.JK_TotalShipmentLoadingMetersCalcEdit);
			this.ShipmentDetailsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 546, true);
			this.ShipmentDetailsBottomPanel.Name = "ShipmentDetailsBottomPanel";
			this.ShipmentDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 35, true);
			this.ShipmentDetailsBottomPanel.TabIndex = 2;
			// 
			// BottomInnerRightPanel
			// 
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalChargeableCalcEdit);
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalChargeableUnitTextBox);
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalPrepaidChargeableAmountCalcEdit);
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox);
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalCollectChargeableAmountCalcEdit);
			this.BottomInnerRightPanel.Controls.Add(this.JK_TotalCollectChargeableAmountCurrencyCode);
			this.BottomInnerRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 0, true);
			this.BottomInnerRightPanel.Name = "BottomInnerRightPanel";
			this.BottomInnerRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 35, true);
			this.BottomInnerRightPanel.TabIndex = 14;
			// 
			// ShipmentCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentCountCalcEdit, "ShipmentCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ShipmentCount)));
			this.ShipmentCountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|01d9a313-e7b8-4915-a946-0704b5b04470", "Ship. Count", "Total amount of shipments attached to this consol.");
			this.ShipmentCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ShipmentCountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ShipmentCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 15, true);
			this.ShipmentCountCalcEdit.Name = "ShipmentCountCalcEdit";
			this.ShipmentCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.ShipmentCountCalcEdit.TabIndex = 1;
			this.ShipmentCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalShipmentLoadingMetersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalShipmentLoadingMetersCalcEdit, "JK_TotalShipmentLoadingMeters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalShipmentLoadingMeters)));
			this.JK_TotalShipmentLoadingMetersCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|416467ac-ec0b-4d85-967a-9a81153f12dc", "LDM", "Loading Meters.");
			this.JK_TotalShipmentLoadingMetersCalcEdit.DecimalPlaces = 3;
			this.JK_TotalShipmentLoadingMetersCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalShipmentLoadingMetersCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalShipmentLoadingMetersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 15, true);
			this.JK_TotalShipmentLoadingMetersCalcEdit.Name = "JK_TotalShipmentLoadingMetersCalcEdit";
			this.JK_TotalShipmentLoadingMetersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JK_TotalShipmentLoadingMetersCalcEdit.TabIndex = 13;
			this.JK_TotalShipmentLoadingMetersCalcEdit.Text = "0.000";
			this.JK_TotalShipmentLoadingMetersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.RightPanel);
			this.TopPanel.Controls.Add(this.ConsolDetailsGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 256, true);
			this.TopPanel.TabIndex = 0;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.DepartureArrivalTabControl);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(517, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 256, true);
			this.RightPanel.TabIndex = 1;
			// 
			// ShipmentModuleButtonGrid
			// 
			this.ShipmentModuleButtonGrid.AllowDrop = true;
			this.ShipmentModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.ShipmentModuleButtonGrid, "GridShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).GridShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).Shipments_List)));
			this.ShipmentModuleButtonGrid.BindToFindBoxList = "Shipments_List";
			this.ShipmentModuleButtonGrid.DetachMessage = Enterprise.Freight.Forwarding.GUI.Res.GetData("bfa9afaa-fd2d-495d-a595-497ea69303df", "Are you sure you want to detach the selected Shipment(s)?");
			this.ShipmentModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentModuleButtonGrid.GridId = "731b586f-fb33-41e9-b7eb-e193d5c53321";
			// 
			// 
			// 
			this.ShipmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ShipmentModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ShipmentModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.ShipmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ShipmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ShipmentModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 311, true);
			this.ShipmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ShipmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
			this.ShipmentModuleButtonGrid.Name = "ShipmentModuleButtonGrid";
			this.ShipmentModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("72120689-77F0-4D0E-B584-3E36F4CD5D89", "Shipment");
			this.ShipmentModuleButtonGrid.ReadOnly = false;
			this.ShipmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 329, true);
			this.ShipmentModuleButtonGrid.TabIndex = 1;
			// 
			// ColoadConsolModuleButtonGrid
			// 
			this.ColoadConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.ColoadConsolModuleButtonGrid, "ColoadConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ColoadConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ColoadConsols_List)));
			this.ColoadConsolModuleButtonGrid.BindToFindBoxList = "ColoadConsols_List";
			this.ColoadConsolModuleButtonGrid.DetachMessage = Enterprise.Freight.Forwarding.GUI.Res.GetData("8c239d24-3a9d-49e9-893e-85ca33a9daee", "Are you sure you want to detach the selected consol(s)?");
			this.ColoadConsolModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColoadConsolModuleButtonGrid.GridId = "aac231cb-a2ff-4302-ad74-9c35c7dbfedb";
			this.ColoadConsolModuleButtonGrid.Name = "ColoadConsolModuleButtonGrid";
			this.ColoadConsolModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("d49eafc2-ee33-49c7-a2ce-b4a10a644c47", "Consol");
			this.ColoadConsolModuleButtonGrid.ReadOnly = true;
			this.ColoadConsolModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 349, true);
			this.ColoadConsolModuleButtonGrid.TabIndex = 1;
			// InnerGrid
			this.ColoadConsolModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ColoadConsolModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ColoadConsolModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ColoadConsolModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = false;
			this.ColoadConsolModuleButtonGrid.InnerGrid.GridId = null;
			this.ColoadConsolModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ColoadConsolModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ColoadConsolModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ColoadConsolModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ColoadConsolModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ColoadConsolModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 311, true);
			this.ColoadConsolModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ColoadConsolModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			// 
			// ConsolDetailsBottomPanel
			// 
			this.ConsolDetailsBottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsolDetailsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 546, true);
			this.ConsolDetailsBottomPanel.Name = "ConsolDetailsBottomPanel";
			this.ConsolDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 35, true);
			this.ConsolDetailsBottomPanel.TabIndex = 3;
			this.ConsolDetailsBottomPanel.Controls.Add(this.ColoadConsolCountCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolWeightCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolWeightUnitTextBox);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolVolumeCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolVolumeUnitTextBox);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolChargeableCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolChargeableUnitTextBox);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit);
			this.ConsolDetailsBottomPanel.Controls.Add(this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode);
			// 
			// ColoadConsolCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColoadConsolCountCalcEdit, "ColoadConsolCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ColoadConsolCount)));
			this.ColoadConsolCountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|5cffd0ae-e1c9-40e1-a940-a40db2c3357d", "Count", "Total amount of consols attached to this consol.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ColoadConsolCountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ColoadConsolCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.ColoadConsolCountCalcEdit.Name = "ColoadConsolCountCalcEdit";
			this.ColoadConsolCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.ColoadConsolCountCalcEdit.TabIndex = 1;
			this.ColoadConsolCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_Calc_TotalColoadConsolQuantityBoundCalcEdit
			// 
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit, "JK_Calc_TotalColoadConsolQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolQuantity)));
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|fde5339a-b904-4979-a75c-81483869629d", "Packs");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 15, true);
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.Name = "JK_Calc_TotalColoadConsolQuantityBoundCalcEdit";
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.TabIndex = 2;
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.Text = "0";
			this.JK_Calc_TotalColoadConsolQuantityBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolWeightCalcEdit, "JK_Calc_TotalColoadConsolWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolWeight)));
			this.JK_TotalColoadConsolWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("31608e92-f5b5-4757-9005-c4df7c66926b", "Weight");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalColoadConsolWeightCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalColoadConsolWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.JK_TotalColoadConsolWeightCalcEdit.Name = "JK_TotalColoadConsolWeightCalcEdit";
			this.JK_TotalColoadConsolWeightCalcEdit.DecimalPlaces = 1;
			this.JK_TotalColoadConsolWeightCalcEdit.Decimals = 1;
			this.JK_TotalColoadConsolWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JK_TotalColoadConsolWeightCalcEdit.TabIndex = 3;
			this.JK_TotalColoadConsolWeightCalcEdit.Text = "0.000";
			this.JK_TotalColoadConsolWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolWeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolWeightUnitTextBox, "JK_Calc_TotalColoadConsolWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolWeightUnit)));
			this.JK_TotalColoadConsolWeightUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|489665d5-a945-4903-9a4c-88e9eb224753", "Total Coload Consol Weight Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalColoadConsolWeightUnitTextBox, false);
			this.JK_TotalColoadConsolWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 15, true);
			this.JK_TotalColoadConsolWeightUnitTextBox.Name = "JK_TotalColoadConsolWeightUnitTextBox";
			this.JK_TotalColoadConsolWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.JK_TotalColoadConsolWeightUnitTextBox.TabIndex = 4;
			// 
			// JK_TotalColoadConsolVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolVolumeCalcEdit, "JK_Calc_TotalColoadConsolVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolVolume)));
			this.JK_TotalColoadConsolVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|39a4cd89-e1f3-47b0-a931-a3db78dff6ae", "Volume");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalColoadConsolVolumeCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalColoadConsolVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 15, true);
			this.JK_TotalColoadConsolVolumeCalcEdit.Name = "JK_TotalColoadConsolVolumeCalcEdit";
			this.JK_TotalColoadConsolVolumeCalcEdit.DecimalPlaces = 3;
			this.JK_TotalColoadConsolVolumeCalcEdit.Decimals = 3;
			this.JK_TotalColoadConsolVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JK_TotalColoadConsolVolumeCalcEdit.TabIndex = 5;
			this.JK_TotalColoadConsolVolumeCalcEdit.Text = "0.000";
			this.JK_TotalColoadConsolVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolVolumeUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolVolumeUnitTextBox, "JK_Calc_TotalColoadConsolVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolVolumeUnit)));
			this.JK_TotalColoadConsolVolumeUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|4cbc1f1e-c78b-4211-a215-0772f3bb0230", "Total Coload Consol Volume Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalColoadConsolVolumeUnitTextBox, false);
			this.JK_TotalColoadConsolVolumeUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 15, true);
			this.JK_TotalColoadConsolVolumeUnitTextBox.Name = "JK_TotalColoadConsolVolumeUnitTextBox";
			this.JK_TotalColoadConsolVolumeUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.JK_TotalColoadConsolVolumeUnitTextBox.TabIndex = 6;
			// 
			// JK_TotalColoadConsolChargeableCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolChargeableCalcEdit, "JK_Calc_TotalColoadConsolChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolChargeable)));
			this.JK_TotalColoadConsolChargeableCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|8088d346-9bab-415b-b6f0-0d25a9a9a083", "Chargeable");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalColoadConsolChargeableCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalColoadConsolChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 15, true);
			this.JK_TotalColoadConsolChargeableCalcEdit.Name = "JK_TotalColoadConsolChargeableCalcEdit";
			this.JK_TotalColoadConsolChargeableCalcEdit.DecimalPlaces = 1;
			this.JK_TotalColoadConsolChargeableCalcEdit.Decimals = 1;
			this.JK_TotalColoadConsolChargeableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.JK_TotalColoadConsolChargeableCalcEdit.TabIndex = 7;
			this.JK_TotalColoadConsolChargeableCalcEdit.Text = "0.000";
			this.JK_TotalColoadConsolChargeableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolChargeableUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolChargeableUnitTextBox, "JK_Calc_TotalColoadConsolChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalColoadConsolChargeableUnit)));
			this.JK_TotalColoadConsolChargeableUnitTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|70ebee74-43cb-43f7-b6c1-39ff374aec22", "Total Chargeable Unit");
			this.JK_TotalColoadConsolChargeableUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 15, true);
			this.JK_TotalColoadConsolChargeableUnitTextBox.Name = "JK_TotalColoadConsolChargeableUnitTextBox";
			this.JK_TotalColoadConsolChargeableUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalColoadConsolChargeableUnitTextBox.TabIndex = 8;
			// 
			// JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit, "JK_Calc_TotalPrepaidColoadConsolChargeableAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalPrepaidColoadConsolChargeableAmount)));
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|7bcc951c-d422-4e28-bc83-ecca1520a5a6", "Prepaid");
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 15, true);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.Name = "JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit";
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.TabIndex = 9;
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.Text = "0.00";
			this.JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox, "JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCode)));
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("faa763e6-2e7b-4c04-9312-9ee1d84ece92", "Total Chargeable Prepaid Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox, false);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 15, true);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox.Name = "JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox";
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox.TabIndex = 10;
			// 
			// JK_TotalColoadConsolCollectChargeableAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit, "JK_Calc_TotalCollectColoadConsolChargeableAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_Calc_TotalCollectColoadConsolChargeableAmount)));
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolUserControl|3087dbec-9565-4774-8724-60b952b870e2", "Collect");
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 15, true);
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.Name = "JK_TotalColoadConsolCollectChargeableAmountCalcEdit";
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.TabIndex = 11;
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.Text = "0.00";
			this.JK_TotalColoadConsolCollectChargeableAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JK_TotalColoadConsolCollectChargeableAmountCurrencyCode
			// 
			this.BindingSource.SetBindingMember(this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode, "JK_TotalCollectColoadConsolChargeableAmountCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_TotalCollectColoadConsolChargeableAmountCurrencyCode)));
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e0587987-8bde-40a1-900b-e33da28d737e", "Total Chargeable Collect Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode, false);
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 15, true);
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode.Name = "JK_TotalColoadConsolCollectChargeableAmountCurrencyCode";
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 18, true);
			this.JK_TotalColoadConsolCollectChargeableAmountCurrencyCode.TabIndex = 12;
			// 
			// ConsolUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsolDetailsBottomPanel);
			this.Controls.Add(this.ShipmentDetailsBottomPanel);
			this.Controls.Add(this.ShipmentModuleButtonGrid);
			this.Controls.Add(this.ColoadConsolModuleButtonGrid);
			this.Controls.Add(this.TopPanel);
			this.Name = "ConsolUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 581, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DepartureArrivalTabControl.ResumeLayout(false);
			this.OrgsTab.ResumeLayout(false);
			this.OrgsTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonSelectFromConsortium)).EndInit();
			this.DepartureTabPage.ResumeLayout(false);
			this.DepartureTabPage.PerformLayout();
			this.DepartureTabControl.ResumeLayout(false);
			this.DepartureTabControl.PerformLayout();
			this.DepartureDetailsTabPage.ResumeLayout(false);
			this.DepartureDetailsTabPage.PerformLayout();
			this.ArrivalTabPage.ResumeLayout(false);
			this.ArrivalTabPage.PerformLayout();
			this.ArrivalTabControl.ResumeLayout(false);
			this.ArrivalTabControl.PerformLayout();
			this.ArrivalDetailsTabPage.ResumeLayout(false);
			this.ArrivalDetailsTabPage.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.PreAllocationTabPage.ResumeLayout(false);
			this.PreAllocationTabPage.PerformLayout();
			this.AchievedQuantitiesTabPage.ResumeLayout(false);
			this.AchievedQuantitiesTabPage.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.CustomDatesTabPage.ResumeLayout(false);
			this.CustomDatesTabPage.PerformLayout();
			this.CRNPanelSea.ResumeLayout(false);
			this.CRNPanelSea.PerformLayout();
			this.ConsolDetailsGroupBox.ResumeLayout(false);
			this.ConsolDetailsGroupBox.PerformLayout();
			this.DetailsBottomPanel.ResumeLayout(false);
			this.DetailsBottomPanel.PerformLayout();
			this.AirConsolPanel.ResumeLayout(false);
			this.AirConsolPanel.PerformLayout();
			this.CharterPanel.ResumeLayout(false);
			this.CharterPanel.PerformLayout();
			this.DetailsTopPanel.ResumeLayout(false);
			this.DetailsTopPanel.PerformLayout();
			this.ShipmentDetailsBottomPanel.ResumeLayout(false);
			this.ShipmentDetailsBottomPanel.PerformLayout();
			this.BottomInnerRightPanel.ResumeLayout(false);
			this.BottomInnerRightPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.DepartureDetailsPanel.ResumeLayout(false);
			this.DepartureDetailsPanel.PerformLayout();
			this.ArrivalDetailsPanel.ResumeLayout(false);
			this.ArrivalDetailsPanel.PerformLayout();
			this.ConsolDetailsBottomPanel.ResumeLayout(false);
			this.ConsolDetailsBottomPanel.PerformLayout();
			this.CFSDepartureByTransportModeDropEdit.ResumeLayout(true);
			this.CFSDepartureByTransportModeDropEdit.PerformLayout();
			this.CFSArrivalByTransportModeDropEdit.ResumeLayout(true);
			this.CFSArrivalByTransportModeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColoadConsolModuleButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}

		Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersControl;
		private Enterprise.ZArchitecture.ZTextBox JK_AgentsReferenceBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_CarrierContractNumberBoundTextEdit;
		private Enterprise.ZArchitecture.GUI.ZButton.Bare CarrierContractImportButton;
		private Enterprise.Freight.Forwarding.GUI.ConsolContractAllocationCodeFindBox JK_CarrierContractNumberFindBox;
		private Enterprise.Freight.Forwarding.GUI.ContractAllocationGuidFindBox JK_RCA_AllocationRouteCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox JK_CoLoadBookingReferenceBoundTextEdit;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_CreditorAddressControl;
		private Enterprise.ZArchitecture.ZTextBox JK_BookingReferenceBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_CoLoadMasterBillBoundTextEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_Calc_TotalShipmentQuantityBoundCalcEdit1;
		private Enterprise.ZArchitecture.GUI.ZTabPage DepartureTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabControl DepartureTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DepartureDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ArrivalTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabControl ArrivalTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ArrivalDetailsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl DepartureArrivalTabControl;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalShipmentWeightCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalShipmentWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalShipmentVolumeCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalShipmentVolumeUnitTextBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_PackDepotAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_UnpackDepotAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ContainerYardEmptyReturnAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ContainerYardEmptyPickupAddressControl;
		public Enterprise.ZArchitecture.GUI.ZCheckBox ShowSubHouseBillsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKFirstForeignPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKPortOfFirstArrivalBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKLastForeignPortBoundCodeFindBox2;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LastForeignPortBoundCodeFindBox1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CFSDepartureByTransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CFSArrivalByTransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RS_NKGatewayServiceLevelBoundCodeFindBox;
		private Enterprise.Freight.Forwarding.GUI.PartialEventsInfoControl departurePartialEventsInfoControl;
		private Enterprise.Freight.Forwarding.GUI.PartialEventsInfoControl arrivalPartialEventsInfoControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DocsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CustomDatesTabPage;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_DateFirstForeignPortDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_DateLastForeignPortDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_DatePortOfFirstArrivalDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_DateLastForeignPortDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_PrintOptionForColoadsOnOtherDocsDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_PrepaidCollectDropDownEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_MasterBillNumTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_CRNBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel CRNPanelSea;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_AgentTypeBoundDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_TransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_ConsolModeDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_SendingForwarderHandlingTypeDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_ReceivingForwarderHandlingTypeDropDownEdit;
		private CargoWise.Windows.UI.KPanel AirConsolPanel;
		private Enterprise.ZArchitecture.ZTextBox JK_AircraftRegoBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel AircraftRegoLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel CharterPanel;
		private Enterprise.ZArchitecture.ZTextBox MAWBTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsNeutralCheckBox;
		private Enterprise.ZArchitecture.ZTextBox AirlinePrefixTextBox;
		private Enterprise.ZArchitecture.ZTextBox MAWBNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_AWBServiceLevelDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly ButtonSelectFromConsortium;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AWBDimsDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PackageGroupingDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalChargeableCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalChargeableUnitTextBox;
		protected Enterprise.ZArchitecture.ZCalcEdit JK_TotalPrepaidChargeableAmountCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalPrepaidChargeableAmountCurrencyCodeTextBox;
		protected Enterprise.ZArchitecture.ZCalcEdit JK_TotalCollectChargeableAmountCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalCollectChargeableAmountCurrencyCode;
		private Enterprise.ZArchitecture.ZLabel CRNLabel;
		private CargoWise.Windows.UI.KPanel DetailsBottomPanel;
		private Enterprise.ZArchitecture.ZLabel MAWBHyphenLabel;
		private Enterprise.ZArchitecture.ZLabel MawbLabel;
		private CargoWise.Windows.UI.KPanel DetailsTopPanel;
		private CargoWise.Windows.UI.KPanel ShipmentDetailsBottomPanel;
		private CargoWise.Windows.UI.KPanel TopPanel;
		private CargoWise.Windows.UI.KPanel RightPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKLoadPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JK_RL_NKDischargePortBoundCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit MasterBillIssueDateEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage OrgsTab;
		private Enterprise.ZArchitecture.GUI.ZTabPage PreAllocationTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AchievedQuantitiesTabPage;
		private Enterprise.ZArchitecture.GUI.ZCheckBox DomesticFreightCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit ConsolChargeableRateCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit SecurityStatusDropEdit;
		private SpecialHandlingUserControl SpecialHandlingUserControl;
		private Enterprise.ZArchitecture.ZCalcEdit ShipmentCountCheckCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit ChargeableCheckCalcEdit;
		private Enterprise.ZArchitecture.ZLabel ChargeableUnitLabel;
		private Enterprise.ZArchitecture.GUI.ZTabPage NumbersTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage RatesTabPage;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ArrivalUnpackCFSTransportAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_DeparturePackCFSTransportAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		public Enterprise.ZArchitecture.GUI.ZCheckBox JK_OverrideConsolChargeableCheckBox;
		private ZOrgAddressWithContactInfoControl SendingForwarderAddressControl;
		private ZOrgAddressWithContactInfoControl ReceivingForwarderAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ShippingLineAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_DepartureCTOAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JK_OA_ArrivalCTOAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JK_PrintOptionForColoadsOnManifestDropEdit;
		private LabelledPercentageBar WeightUtilisationPercentageBar;
		private LabelledPercentageBar VolumeUtilisationPercentageBar;
		private LabelledPercentageBar CostFreePercentageBar;
		private Enterprise.ZArchitecture.ZCalcEdit ExcessVolumeWeightCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox ExcessVolumeWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZLabel WeightUtilisationLabel;
		private Enterprise.ZArchitecture.ZLabel VolumeUtilisationLabel;
		private Enterprise.ZArchitecture.ZLabel CostFreeLabel;
		private Enterprise.Freight.Forwarding.GUI.DangerousGoodsControl DangerousGoodsControl;
		private Enterprise.Freight.Forwarding.GUI.ConsolMaxDimsControl ConsolMaxDimsControl;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Commodity;
		private Enterprise.ZArchitecture.ZCalcEdit ConsolChargeableQuantityCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_NoOriginalBillsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_NoCopyBillsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ShipmentCountCalcEdit;
		private Enterprise.Freight.GUI.ConsoLegDetailsControl consolLeg1;
		internal Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit JK_ScreeningStatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		private ZArchitecture.GUI.ZDateEdit JK_ConsolCutOffDateEdit;
		private ZArchitecture.ZCalcEdit CorrectedConsolVolumeCalcEdit;
		private ZArchitecture.ZCalcEdit CorrectedConsolWeightCalcEdit;
		private ZArchitecture.GUI.ZDropEdit CorrectedConsolVolumeUnitDropEdit;
		private ZArchitecture.GUI.ZDropEdit CorrectedConsolWeightUnitDropEdit;
		private ZArchitecture.ZTextBox ConsolChargeableUnitTextBox;
		private ZArchitecture.GUI.ZDropEdit JK_PhaseDropEdit;
		private ZArchitecture.ZCalcEdit JK_TotalShipmentLoadingMetersCalcEdit;
		private ZArchitecture.GUI.ZPanel BottomInnerRightPanel;
		internal ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ConsolCustomFields;
		internal ConsolShipmentModuleButtonGrid ShipmentModuleButtonGrid;
		private ColoadConsolModuleButtonGrid ColoadConsolModuleButtonGrid;
		private ZArchitecture.ZTextBox MAWBPendingAllocationTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JK_RL_NKMasterBillIssuePlaceFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_Calc_FreeSpaceEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_FreeSpaceUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_Calc_ActualVolumeWeightEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_ActualVolumeWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_ConsolidatedFreightCostChargeableTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_ShipmentFreightCostChargeableTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_ConsolidatedFreightCostChargeableDescTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_Calc_ShipmentFreightCostChargeableDescTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargesApply;
		private Enterprise.Freight.Forwarding.GUI.TemperatureControlBlock TemperatureControlBlock;
		private Enterprise.ZArchitecture.ZLabel DensityFactorLabel;
		private Enterprise.Freight.Forwarding.GUI.DensityVisualisationControl DensityVisualisationControl;
		private ZArchitecture.GUI.ZDateEdit JK_PackDepotReceiptRequestedControl;
		private ZArchitecture.GUI.ZDateEdit JK_PackDepotDispatchRequestedControl;
		private ZArchitecture.GUI.ZDateEdit JK_UnpackDepotReceiptRequestedControl;
		private ZArchitecture.GUI.ZDateEdit JK_UnpackDepotDispatchRequestedControl;
		Enterprise.ZArchitecture.GUI.ZPanel DepartureDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZPanel ArrivalDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox LatestStatusGroupBox;
		private ZArchitecture.GUI.ZDropEdit BillOfLadingBillStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit BillOfLadingBillDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit CarrierBookingLatestStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit CarrierBookingDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox CarrierBookingOfficeCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit BillOfLadingBillTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit BillOfLadingBillTermsDropEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit AutoratingDateOverriddenDateEdit;
		Enterprise.ZArchitecture.ZTextBox TotalCO2eTextBox;
		Enterprise.ZArchitecture.ZLabel TotalCO2eUnitLabel;
		private CargoWise.Windows.UI.KPanel ConsolDetailsBottomPanel;
		private Enterprise.ZArchitecture.ZCalcEdit ColoadConsolCountCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_Calc_TotalColoadConsolQuantityBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalColoadConsolWeightCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalColoadConsolWeightUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalColoadConsolVolumeCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalColoadConsolVolumeUnitTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JK_TotalColoadConsolChargeableCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalColoadConsolChargeableUnitTextBox;
		protected Enterprise.ZArchitecture.ZCalcEdit JK_TotalColoadConsolPrepaidChargeableAmountCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalColoadConsolPrepaidChargeableAmountCurrencyCodeTextBox;
		protected Enterprise.ZArchitecture.ZCalcEdit JK_TotalColoadConsolCollectChargeableAmountCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_TotalColoadConsolCollectChargeableAmountCurrencyCode;
		Enterprise.ZArchitecture.ZTextBox ElectronicBillOfLadingReferenceTextBox;
		Enterprise.ZArchitecture.GUI.ZButton ViewEBLButton;
	}
}
