using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ForwarderMainControl
	{
		private void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zPotentialCreditorFindBoxColumnStyleInfo = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoTransitTime = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoFrequency = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoFrequencyUnit = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.containersAndPackLineDynamicControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.RightTopStaticPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OneOffQuoteStatisticsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PotentialCarriersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PotentialCarriersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuotedBookingOrderManagementControl = new Enterprise.Freight.QuotedBookings.GUI.QuotedBookingOrderManagementControl();
			this.General3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RightTopRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.CustomsEntryNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperCODAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsEntryNumberTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipperCODTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryInvoiceLinesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntriesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ScreeningControlsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreightRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AviationSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApprovedShipperInspectionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EndDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OneOffQuoteKPIDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OneOffQuoteSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OneOffQuoteRevisionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CompanyTariffLevelOverrideForBookingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RateLocalCode = new Enterprise.ZArchitecture.ZTextBox();
			this.FMCTariffID = new Enterprise.ZArchitecture.ZTextBox();
			this.MainMiddlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MiddleTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.General2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MiddleTopRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.CarrierServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerModeOverrideDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OneOffQuoteHBLDeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OnBoardDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryRateFrequencyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DeliveryCloseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PickupCloseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryRateTransitTimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TotalCO2eTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TotalCO2eUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeliveryOpenDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PickupReadyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeliveryDueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ViaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LeftTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.General1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LeftTopRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.ChargesApplyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReleaseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalTermsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CompTariffLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsDomesticCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MarksAndNumbersNotePopupEdit = new Enterprise.Freight.GUI.ZStmNotePopupEditWithBindableText();
			this.JS_ShippingReferenceTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_GoodsDescriptionBoundTextBox = new Enterprise.Freight.GUI.GoodsDescriptionTextBox();
			this.PaymentTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MainBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MiddleBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GoodsDetails2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MiddleBottomRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.PickupEquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryEquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommodityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RightBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MonetaryValues = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RightBottomRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.FreightSpotRate = new Enterprise.Freight.GUI.FreightRateControl();
			this.ValueOfInsuranceCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfGoodsCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JP_InsuranceRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ValueOfGoodsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GoodsDetails1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LeftBottomRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.ChargeableUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChargeableDropEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ActualWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.MainTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ClientControl = new Enterprise.Freight.QuotedBookings.GUI.ClientControl();
			this.OrgRoleDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CarrierContractNumberBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierContractNumberFindBox = new Enterprise.Freight.QuotedBookings.GUI.QuotedBookingContractAllocationCodeFindBox();
			this.AllocationRouteCodeFindBox = new Enterprise.Freight.QuotedBookings.GUI.QuotedBookingContractAllocationGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.MiddleTopRowPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.RightTopStaticPanel.SuspendLayout();
			this.OneOffQuoteStatisticsPanel.SuspendLayout();
			this.CompanyTariffLevelOverrideForBookingDropEdit.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.PotentialCarriersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PotentialCarriersGrid)).BeginInit();
			this.PotentialCarriersGrid.SuspendLayout();
			this.RightTopRowPanel.SuspendLayout();
			this.AviationSecurityPanel.SuspendLayout();
			this.MainMiddlePanel.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.LeftTopRowPanel.SuspendLayout();
			this.MainBottomPanel.SuspendLayout();
			this.MiddleBottomPanel.SuspendLayout();
			this.MiddleBottomRowPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.RightBottomRowPanel.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.LeftBottomRowPanel.SuspendLayout();
			this.MainTopPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.QuotedBookingOrderManagementControl.SuspendLayout();
			this.General3.SuspendLayout();
			this.CustomsEntryNumberTypeBoundDropEdit.SuspendLayout();
			this.ShipperCODTypeDropEdit.SuspendLayout();
			this.ScreeningControlsGroupBox.SuspendLayout();
			this.FreightRatesGroupBox.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.ApprovedShipperInspectionTypeDropEdit.SuspendLayout();
			this.EndDateDateEdit.SuspendLayout();
			this.StartDateDateEdit.SuspendLayout();
			this.OneOffQuoteKPIDropEdit.SuspendLayout();
			this.OneOffQuoteSourceDropEdit.SuspendLayout();
			this.OneOffQuoteRevisionReasonDropEdit.SuspendLayout();
			this.General2.SuspendLayout();
			this.CarrierServiceLevelDropEdit.SuspendLayout();
			this.ContainerModeOverrideDropEdit.SuspendLayout();
			this.OneOffQuoteHBLDeliveryModeDropEdit.SuspendLayout();
			this.OnBoardDropEdit.SuspendLayout();
			this.DeliveryDueDateDateEdit.SuspendLayout();
			this.EntryRateFrequencyCalcDropEdit.SuspendLayout();
			this.DeliveryCloseDateEdit.SuspendLayout();
			this.PickupCloseDateEdit.SuspendLayout();
			this.EntryRateTransitTimeDropEdit.SuspendLayout();
			this.TotalCO2eTextBox.SuspendLayout();
			this.CreditorGuidFindBox.SuspendLayout();
			this.TotalCO2eUnitLabel.SuspendLayout();
			this.OriginCodeFindBox.SuspendLayout();
			this.DeliveryOpenDateEdit.SuspendLayout();
			this.PickupReadyDateEdit.SuspendLayout();
			this.DestinationCodeFindBox.SuspendLayout();
			this.ViaCodeFindBox.SuspendLayout();
			this.CarrierGuidFindBox.SuspendLayout();
			this.General1.SuspendLayout();
			this.ChargesApplyDropEdit.SuspendLayout();
			this.ReleaseTypeDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.ContainerModeDropEdit.SuspendLayout();
			this.CompTariffLevelDropEdit.SuspendLayout();
			this.MarksAndNumbersNotePopupEdit.SuspendLayout();
			this.JS_GoodsDescriptionBoundTextBox.SuspendLayout();
			this.PaymentTermDropEdit.SuspendLayout();
			this.ServiceLevelFindBox.SuspendLayout();
			this.GoodsDetails2.SuspendLayout();
			this.PickupEquipmentDropEdit.SuspendLayout();
			this.DeliveryEquipmentDropEdit.SuspendLayout();
			this.CommodityFindBox.SuspendLayout();
			this.MonetaryValues.SuspendLayout();
			this.FreightSpotRate.SuspendLayout();
			this.ValueOfInsuranceCurrencyFindbox.SuspendLayout();
			this.ValueOfGoodsCurrencyFindbox.SuspendLayout();
			this.GoodsDetails1.SuspendLayout();
			this.PackagesCalcDropEdit.SuspendLayout();
			this.ActualWeightDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.DeliveryDocAddressControl.SuspendLayout();
			this.PickupDocAddressControl.SuspendLayout();
			this.ClientControl.SuspendLayout();
			this.OrgRoleDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.containersAndPackLineDynamicControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 103, true);
			this.BottomPanel.TabIndex = 1;
			//
			// containersAndPackLineDynamicControl
			//
			this.containersAndPackLineDynamicControl.AllowDrop = true;
			this.containersAndPackLineDynamicControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersAndPackLineDynamicControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containersAndPackLineDynamicControl.Name = "containersAndPackLineDynamicControl";
			this.containersAndPackLineDynamicControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 128, true);
			this.containersAndPackLineDynamicControl.TabIndex = 41;
			//
			// RightTopStaticPanel
			//
			this.RightTopStaticPanel.Controls.Add(this.RightTopPanel);
			this.RightTopStaticPanel.Controls.Add(this.EndDateDateEdit);
			this.RightTopStaticPanel.Controls.Add(this.StartDateDateEdit);
			this.RightTopStaticPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTopStaticPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 0, true);
			this.RightTopStaticPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 565, true);
			this.RightTopStaticPanel.Name = "RightTopStaticPanel";
			this.RightTopStaticPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 565, true);
			this.RightTopStaticPanel.TabIndex = 1;
			//
			// OneOffQuoteStatisticsPanel
			//
			this.OneOffQuoteStatisticsPanel.Controls.Add(this.OneOffQuoteKPIDropEdit);
			this.OneOffQuoteStatisticsPanel.Controls.Add(this.OneOffQuoteSourceDropEdit);
			this.OneOffQuoteStatisticsPanel.Controls.Add(this.OneOffQuoteRevisionReasonDropEdit);
			this.OneOffQuoteStatisticsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.OneOffQuoteStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 0, true);
			this.OneOffQuoteStatisticsPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 525, true);
			this.OneOffQuoteStatisticsPanel.Name = "OneOffQuoteStatisticsPanel";
			this.OneOffQuoteStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 525, true);
			this.OneOffQuoteStatisticsPanel.TabIndex = 2;

			//
			// RightTopPanel
			//
			this.RightTopPanel.Controls.Add(this.PotentialCarriersGroupBox);
			this.RightTopPanel.Controls.Add(this.QuotedBookingOrderManagementControl);
			this.RightTopPanel.Controls.Add(this.General3);
			this.RightTopPanel.Controls.Add(this.ScreeningControlsGroupBox);
			this.RightTopPanel.Controls.Add(this.AviationSecurityPanel);
			this.RightTopPanel.Controls.Add(this.FreightRatesGroupBox);
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopPanel, true);
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 500, true);
			this.RightTopPanel.TabIndex = 2;
			//
			// CompanyTariffLevelOverrideForBookingDropEdit
			//
			this.CompanyTariffLevelOverrideForBookingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyTariffLevelOverrideForBookingDropEdit, "CompanyTariffLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).CompanyTariffLevel)));
			this.CompanyTariffLevelOverrideForBookingDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|8a12741e-ac28-474b-95ef-658c9b199cf6", "CT Level", "Company Tariff Level Override", "Company Tariff Level Override - Override the Company Tariff Level to be used for Autorating Revenue");
			this.CompanyTariffLevelOverrideForBookingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 16, true);
			this.CompanyTariffLevelOverrideForBookingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CompanyTariffLevelOverrideForBookingDropEdit.Name = "CompanyTariffLevel";
			this.CompanyTariffLevelOverrideForBookingDropEdit.TabIndex = 0;
			this.CompanyTariffLevelOverrideForBookingDropEdit.CodeBox.MaxLength = 2;
			//
			// PotentialCarriersGroupBox
			//
			this.PotentialCarriersGroupBox.CaptionResourceString = Res.GetData("e87886de-e7cc-4c6e-a208-708fc185b77b", "Potential Carriers");
			this.PotentialCarriersGroupBox.Controls.Add(this.PotentialCarriersGrid);
			this.PotentialCarriersGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PotentialCarriersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 55, true);
			this.PotentialCarriersGroupBox.Name = "PotentialCarriersGroupBox";
			this.PotentialCarriersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 177, true);
			this.PotentialCarriersGroupBox.TabIndex = 1;
			this.PotentialCarriersGroupBox.TabStop = false;
			//
			// PotentialCarriersGrid
			//
			this.PotentialCarriersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PotentialCarriersGrid, "Quote.CurrentOneOffQuote.PossibleCarriers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).TTC_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).CarrierName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).TTC_TransitTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).TTC_Frequency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).TTC_FrequencyUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.RateOneOffCarrier)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PossibleCarriers)).SyncRoot)).TTC_OH_Creditor)));
			this.PotentialCarriersGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("2904f1c0-983e-4211-b272-1be72f23e398", "Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "TTC_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zPotentialCreditorFindBoxColumnStyleInfo.CaptionResourceString = Res.GetData("ef0fad8c-81b0-43aa-b36f-94ca46ce57f8", "Creditor");
			zPotentialCreditorFindBoxColumnStyleInfo.ColumnName = "TTC_OH_Creditor";
			zPotentialCreditorFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zPotentialCreditorFindBoxColumnStyleInfo.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("296fafaa-7c1a-48b1-adca-5b50667f845e", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CarrierName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zDropEditColumnStyleInfoTransitTime.ColumnName = "TTC_TransitTime";
			zDropEditColumnStyleInfoTransitTime.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfoFrequency.ColumnName = "TTC_Frequency";
			zTextBoxColumnStyleInfoFrequency.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfoFrequencyUnit.ColumnName = "TTC_FrequencyUnit";
			zDropEditColumnStyleInfoFrequencyUnit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.PotentialCarriersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PotentialCarriersGrid.ColumnStyles.Add(zPotentialCreditorFindBoxColumnStyleInfo);
			this.PotentialCarriersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PotentialCarriersGrid.ColumnStyles.Add(zDropEditColumnStyleInfoTransitTime);
			this.PotentialCarriersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoFrequency);
			this.PotentialCarriersGrid.ColumnStyles.Add(zDropEditColumnStyleInfoFrequencyUnit);
			this.PotentialCarriersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PotentialCarriersGrid.GridId = "4483d6a4-baa9-4224-8646-b956ad2ae040";
			this.PotentialCarriersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PotentialCarriersGrid.LayoutKey = "PotentialCarriersGrid";
			this.PotentialCarriersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.PotentialCarriersGrid.Name = "PotentialCarriersGrid";
			this.PotentialCarriersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 160, true);
			this.PotentialCarriersGrid.TabIndex = 0;
			//
			// QuotedBookingOrderManagementControl
			//
			this.QuotedBookingOrderManagementControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotedBookingOrderManagementControl, ".");
			this.QuotedBookingOrderManagementControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuotedBookingOrderManagementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuotedBookingOrderManagementControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 232, true);
			this.QuotedBookingOrderManagementControl.Name = "QuotedBookingOrderManagementControl";
			this.QuotedBookingOrderManagementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 232, true);
			this.QuotedBookingOrderManagementControl.TabIndex = 0;
			//
			// General3
			//
			this.General3.CaptionResourceString = Res.GetData("ForwarderMainControl|8a0ea670-076a-40ec-ae09-aa856b13efcf", "Brokerage Details");
			this.General3.Controls.Add(this.RightTopRowPanel);
			this.General3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.General3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.General3.Name = "General3";
			this.General3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 135, true);
			this.General3.TabIndex = 0;
			this.General3.TabStop = false;
			//
			// RightTopRowPanel
			//
			this.RightTopRowPanel.Controls.Add(this.CustomsEntryNumberBoundTextBox);
			this.RightTopRowPanel.Controls.Add(this.ShipperCODAmountCalcEdit);
			this.RightTopRowPanel.Controls.Add(this.CustomsEntryNumberTypeBoundDropEdit);
			this.RightTopRowPanel.Controls.Add(this.ShipperCODTypeDropEdit);
			this.RightTopRowPanel.Controls.Add(this.EntryInvoiceLinesCalcEdit);
			this.RightTopRowPanel.Controls.Add(this.EntriesCalcEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopRowPanel, true);
			this.RightTopRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 12, true);
			this.RightTopRowPanel.Name = "RightTopRowPanel";
			this.RightTopRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.RightTopRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 112, true);
			this.RightTopRowPanel.TabIndex = 0;
			//
			// CustomsEntryNumberBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberBoundTextBox, "Booking+CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.CustomsEntryNumber)));
			this.CustomsEntryNumberBoundTextBox.CaptionResourceString = Res.GetData("ForwarderMainControl|1db9cf13-08b5-4bc3-944c-5458e2ffa447", "Entry Number", "Customs Entry Number", "The Customs Entry Number for this movement.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberBoundTextBox, false);
			this.CustomsEntryNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 92, true);
			this.CustomsEntryNumberBoundTextBox.Name = "CustomsEntryNumberBoundTextBox";
			this.RightTopRowPanel.SetRow(this.CustomsEntryNumberBoundTextBox, 4);
			this.CustomsEntryNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.CustomsEntryNumberBoundTextBox.TabIndex = 6;
			//
			// ShipperCODAmountCalcEdit
			//
			this.ShipperCODAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ShipperCODAmountCalcEdit, "Booking+JS_ShipperCODAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ShipperCODAmount)));
			this.ShipperCODAmountCalcEdit.CaptionResourceString = null;
			this.ShipperCODAmountCalcEdit.DecimalPlaces = 2;
			this.ShipperCODAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipperCODAmountCalcEdit.Name = "ShipperCODAmountCalcEdit";
			this.RightTopRowPanel.SetRow(this.ShipperCODAmountCalcEdit, 0);
			this.ShipperCODAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.ShipperCODAmountCalcEdit.TabIndex = 1;
			this.ShipperCODAmountCalcEdit.Text = "0.00";
			this.ShipperCODAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CustomsEntryNumberTypeBoundDropEdit
			//
			this.CustomsEntryNumberTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTypeBoundDropEdit, "Booking+CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.CustomsEntryNumberType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberTypeBoundDropEdit, false);
			this.CustomsEntryNumberTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Name = "CustomsEntryNumberTypeBoundDropEdit";
			this.CustomsEntryNumberTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.RightTopRowPanel.SetRow(this.CustomsEntryNumberTypeBoundDropEdit, 4);
			this.CustomsEntryNumberTypeBoundDropEdit.ShowDescriptionBox = false;
			this.CustomsEntryNumberTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CustomsEntryNumberTypeBoundDropEdit.TabIndex = 5;
			//
			// ShipperCODTypeDropEdit
			//
			this.ShipperCODTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperCODTypeDropEdit, "Booking+JS_ShipperCODPayMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ShipperCODPayMethod)));
			this.ShipperCODTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ShipperCODTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ShipperCODTypeDropEdit.Name = "ShipperCODTypeDropEdit";
			this.ShipperCODTypeDropEdit.PreBoundMaxLength = 3;
			this.RightTopRowPanel.SetRow(this.ShipperCODTypeDropEdit, 1);
			this.ShipperCODTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.ShipperCODTypeDropEdit.TabIndex = 2;
			//
			// EntryInvoiceLinesCalcEdit
			//
			this.BindingSource.SetBindingMember(this.EntryInvoiceLinesCalcEdit, "QuoteNumberOfEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).QuoteNumberOfEntryLines)));
			this.EntryInvoiceLinesCalcEdit.CaptionResourceString = null;
			this.EntryInvoiceLinesCalcEdit.DecimalPlaces = 0;
			this.EntryInvoiceLinesCalcEdit.Decimals = 0;
			this.EntryInvoiceLinesCalcEdit.AllowNegative = false;
			this.EntryInvoiceLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.EntryInvoiceLinesCalcEdit.Name = "EntryInvoiceLinesCalcEdit";
			this.RightTopRowPanel.SetRow(this.EntryInvoiceLinesCalcEdit, 3);
			this.EntryInvoiceLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.EntryInvoiceLinesCalcEdit.TabIndex = 4;
			this.EntryInvoiceLinesCalcEdit.Text = "0";
			this.EntryInvoiceLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// EntriesCalcEdit
			//
			this.BindingSource.SetBindingMember(this.EntriesCalcEdit, "QuoteNumberOfEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).QuoteNumberOfEntries)));
			this.EntriesCalcEdit.CaptionResourceString = null;
			this.EntriesCalcEdit.DecimalPlaces = 0;
			this.EntriesCalcEdit.AllowNegative = false;
			this.EntriesCalcEdit.Decimals = 0;
			this.EntriesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.EntriesCalcEdit.Name = "EntriesCalcEdit";
			this.RightTopRowPanel.SetRow(this.EntriesCalcEdit, 2);
			this.EntriesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.EntriesCalcEdit.TabIndex = 3;
			this.EntriesCalcEdit.Text = "0";
			this.EntriesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ScreeningControlsGroupBox
			//
			this.ScreeningControlsGroupBox.CaptionResourceString = Res.GetData("ForwarderMainControl|B7D33844-374A-460A-9FAC-04A929C4FF8E", "Security");
			this.ScreeningControlsGroupBox.Controls.Add(this.ScreeningStatusDropEdit);
			this.ScreeningControlsGroupBox.Controls.Add(this.ScreenButton);
			this.ScreeningControlsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ScreeningControlsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 367, true);
			this.ScreeningControlsGroupBox.Name = "ScreeningControlsGroupBox";
			this.ScreeningControlsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 42, true);
			this.ScreeningControlsGroupBox.TabIndex = 0;
			this.ScreeningControlsGroupBox.TabStop = false;
			//
			// FreightRatesGroupBox
			//
			this.FreightRatesGroupBox.CaptionResourceString = Res.GetData("ForwarderMainControl|c631f7c8-e7dc-46a1-81bd-5270abc17085", "Freight Rates");
			this.FreightRatesGroupBox.Controls.Add(this.CompanyTariffLevelOverrideForBookingDropEdit);
			this.FreightRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FreightRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.FreightRatesGroupBox.Name = "FreightRatesGroupBox";
			this.FreightRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 50, true);
			this.FreightRatesGroupBox.TabIndex = 0;
			this.FreightRatesGroupBox.TabStop = false;
			//
			// ScreeningStatusDropEdit
			//
			this.ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatusDropEdit, "Booking.JS_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ScreeningStatus)));
			this.ScreeningStatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 16, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ScreeningStatusDropEdit.TabIndex = 0;
			//
			// ScreenButton
			//
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 16, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 10;
			this.ScreenButton.Text = "...";
			this.ScreenButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			//
			// AviationSecurityPanel
			//
			this.AviationSecurityPanel.Controls.Add(this.ApprovedShipperInspectionTypeDropEdit);
			this.AviationSecurityPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AviationSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 409, true);
			this.AviationSecurityPanel.Name = "AviationSecurityPanel";
			this.AviationSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 31, true);
			this.AviationSecurityPanel.TabIndex = 2;
			//
			// ApprovedShipperInspectionTypeDropEdit
			//
			this.ApprovedShipperInspectionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApprovedShipperInspectionTypeDropEdit, "Booking.JS_InspectionTypeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_InspectionTypeCode)));
			this.ApprovedShipperInspectionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 4, true);
			this.ApprovedShipperInspectionTypeDropEdit.Name = "ApprovedShipperInspectionTypeDropEdit";
			this.ApprovedShipperInspectionTypeDropEdit.PreBoundMaxLength = 3;
			this.ApprovedShipperInspectionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ApprovedShipperInspectionTypeDropEdit.TabIndex = 0;
			//
			// EndDateDateEdit
			//
			this.EndDateDateEdit.AllowDrop = true;
			this.EndDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EndDateDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).EndDate)));
			this.EndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 37, true);
			this.EndDateDateEdit.Name = "EndDateDateEdit";
			this.EndDateDateEdit.TabIndex = 1;
			//
			// StartDateDateEdit
			//
			this.StartDateDateEdit.AllowDrop = true;
			this.StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.StartDateDateEdit, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).StartDate)));
			this.StartDateDateEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|cfe76695-5ff4-4455-98ff-6acf7b466a95", "Start", "Start Date", "The starting date for the validity of the quotation.");
			this.StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 11, true);
			this.StartDateDateEdit.Name = "StartDateDateEdit";
			this.StartDateDateEdit.TabIndex = 0;
			//
			// OneOffQuoteKPIDropEdit
			//
			this.OneOffQuoteKPIDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OneOffQuoteKPIDropEdit, "OneOffQuoteStatistics.OneOffQuoteKPI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OneOffQuoteStatistics.OneOffQuoteKPI)));
			this.OneOffQuoteKPIDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|8a12741e-ac08-474b-95ef-658c9b199cf6", "KPI", "Quote KPI", "The KPI of One Off Quote.");
			this.OneOffQuoteKPIDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 11, true);
			this.OneOffQuoteKPIDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 22, true);
			this.OneOffQuoteKPIDropEdit.Name = "OneOffQuoteKPI";
			this.OneOffQuoteKPIDropEdit.TabIndex = 0;
			this.OneOffQuoteKPIDropEdit.CodeBox.MaxLength = 3;
			//
			// OneOffQuoteSourceDropEdit
			//
			this.OneOffQuoteSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OneOffQuoteSourceDropEdit, "OneOffQuoteStatistics.OneOffQuoteSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OneOffQuoteStatistics.OneOffQuoteSource)));
			this.OneOffQuoteSourceDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|8456bf22-0dea-4e04-87db-11ec0e24acf3", "Source", "Quote Source", "The Source of One Off Quote.");
			this.OneOffQuoteSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 37, true);
			this.OneOffQuoteSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 22, true);
			this.OneOffQuoteSourceDropEdit.Name = "OneOffQuoteSource";
			this.OneOffQuoteSourceDropEdit.TabIndex = 0;
			//
			// OneOffQuoteRevisionReasonDropEdit
			//
			this.OneOffQuoteRevisionReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OneOffQuoteRevisionReasonDropEdit, "OneOffQuoteStatistics.OneOffQuoteRevisionReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OneOffQuoteStatistics.OneOffQuoteRevisionReason)));
			this.OneOffQuoteRevisionReasonDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|18b38ee9-337b-4ce6-93ff-36855f686c8f", "Revision", "Revision Reason", "The Revision Reason of One Off Quote.");
			this.OneOffQuoteRevisionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 63, true);
			this.OneOffQuoteRevisionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 22, true);
			this.OneOffQuoteRevisionReasonDropEdit.Name = "OneOffQuoteRevisionReason";
			this.OneOffQuoteRevisionReasonDropEdit.TabIndex = 0;
			//
			// MainPanel
			//
			this.MainPanel.Controls.Add(this.MainMiddlePanel);
			this.MainPanel.Controls.Add(this.MainBottomPanel);
			this.MainPanel.Controls.Add(this.MainTopPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 613, true);
			this.MainPanel.TabIndex = 0;
			//
			// MainMiddlePanel
			//
			this.MainMiddlePanel.Controls.Add(this.MiddleTopPanel);
			this.MainMiddlePanel.Controls.Add(this.LeftTopPanel);
			this.MainMiddlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainMiddlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 212, true);
			this.MainMiddlePanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 274, true);
			this.MainMiddlePanel.Name = "MainMiddlePanel";
			this.MainMiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 323, true);
			this.MainMiddlePanel.TabIndex = 1;
			//
			// MiddleTopPanel
			//
			this.MiddleTopPanel.Controls.Add(this.General2);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleTopPanel, true);
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 0, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 273, true);
			this.MiddleTopPanel.TabIndex = 1;
			//
			// General2
			//
			this.General2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.General2.Controls.Add(this.MiddleTopRowPanel);
			this.General2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.General2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.General2.Name = "General2";
			this.General2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 250, true);
			this.General2.TabIndex = 0;
			this.General2.TabStop = false;
			//
			// MiddleTopRowPanel
			//
			this.MiddleTopRowPanel.Controls.Add(this.ContainerModeOverrideDropEdit);
			this.MiddleTopRowPanel.Controls.Add(this.OnBoardDropEdit);
			this.MiddleTopRowPanel.Controls.Add(this.DeliveryOpenDateEdit);
			this.MiddleTopRowPanel.Controls.Add(this.EntryRateFrequencyCalcDropEdit);
			this.MiddleTopRowPanel.Controls.Add(this.DeliveryCloseDateEdit);
			this.MiddleTopRowPanel.Controls.Add(this.PickupCloseDateEdit);
			this.MiddleTopRowPanel.Controls.Add(this.EntryRateTransitTimeDropEdit);
			this.MiddleTopRowPanel.Controls.Add(this.TotalCO2eTextBox);
			this.MiddleTopRowPanel.Controls.Add(this.CreditorGuidFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.TotalCO2eUnitLabel);
			this.MiddleTopRowPanel.Controls.Add(this.OriginCodeFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.PickupReadyDateEdit);
			this.MiddleTopRowPanel.Controls.Add(this.DeliveryDueDateDateEdit);
			this.MiddleTopRowPanel.Controls.Add(this.DestinationCodeFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.ViaCodeFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.CarrierGuidFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.CarrierServiceLevelDropEdit);
			this.MiddleTopRowPanel.Controls.Add(this.CarrierContractNumberBoundTextEdit);
			this.MiddleTopRowPanel.Controls.Add(this.CarrierContractNumberFindBox);
			this.MiddleTopRowPanel.Controls.Add(this.AllocationRouteCodeFindBox);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleTopRowPanel, true);
			this.MiddleTopRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 15, true);
			this.MiddleTopRowPanel.Name = "MiddleTopRowPanel";
			this.MiddleTopRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.MiddleTopRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 256, true);
			this.MiddleTopRowPanel.TabIndex = 0;
			//
			// ContainerModeOverrideDropEdit
			//
			this.ContainerModeOverrideDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeOverrideDropEdit, "ContainerPackModeOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ContainerPackModeOverride)));
			this.ContainerModeOverrideDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.ContainerModeOverrideDropEdit.Name = "ContainerModeOverrideDropEdit";
			this.ContainerModeOverrideDropEdit.PreBoundMaxLength = 3;
			this.MiddleTopRowPanel.SetRow(this.ContainerModeOverrideDropEdit, 12);
			this.ContainerModeOverrideDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.ContainerModeOverrideDropEdit.TabIndex = 14;
			//
			// OnBoardDropEdit
			//
			this.OnBoardDropEdit.AllowDrop = true;
			this.OnBoardDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OnBoardDropEdit, "Booking+JS_ShippedOnBoard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ShippedOnBoard)));
			this.OnBoardDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 209, true);
			this.OnBoardDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OnBoardDropEdit.Name = "OnBoardDropEdit";
			this.OnBoardDropEdit.PreBoundMaxLength = 3;
			this.MiddleTopRowPanel.SetRow(this.OnBoardDropEdit, 11);
			this.OnBoardDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.OnBoardDropEdit.TabIndex = 13;
			//
			// DeliveryDueDateDateEdit
			//
			this.DeliveryDueDateDateEdit.AllowDrop = true;
			this.DeliveryDueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeliveryDueDateDateEdit, "DeliveryDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).DeliveryDueDate)));
			this.DeliveryDueDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveryDueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 230, true);
			this.DeliveryDueDateDateEdit.Name = "DeliveryDueDateDateEdit";
			this.MiddleTopRowPanel.SetRow(this.DeliveryDueDateDateEdit, 10);
			this.DeliveryDueDateDateEdit.TabIndex = 12;
			this.DeliveryDueDateDateEdit.Visible = false;
			//
			// EntryRateFrequencyCalcDropEdit
			//
			this.EntryRateFrequencyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryRateFrequencyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Frequency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).FrequencyUnit)));
			this.EntryRateFrequencyCalcDropEdit.BindToAmount = "Frequency";
			this.EntryRateFrequencyCalcDropEdit.BindToUnit = "FrequencyUnit";
			this.EntryRateFrequencyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 162, true);
			this.EntryRateFrequencyCalcDropEdit.Name = "EntryRateFrequencyCalcDropEdit";
			this.MiddleTopRowPanel.SetRow(this.EntryRateFrequencyCalcDropEdit, 9);
			this.EntryRateFrequencyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.EntryRateFrequencyCalcDropEdit.TabIndex = 11;
			this.EntryRateFrequencyCalcDropEdit.UnitPreBoundMaxLength = 7;
			//
			// DeliveryCloseDateEdit
			//
			this.DeliveryCloseDateEdit.AllowDrop = true;
			this.DeliveryCloseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeliveryCloseDateEdit, "DeliveryClose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).DeliveryClose)));
			this.DeliveryCloseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveryCloseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 139, true);
			this.DeliveryCloseDateEdit.Name = "DeliveryCloseDateEdit";
			this.MiddleTopRowPanel.SetRow(this.DeliveryCloseDateEdit, 8);
			this.DeliveryCloseDateEdit.TabIndex = 9;
			//
			// PickupCloseDateEdit
			//
			this.PickupCloseDateEdit.AllowDrop = true;
			this.PickupCloseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PickupCloseDateEdit, "PickupClose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).PickupClose)));
			this.PickupCloseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PickupCloseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 116, true);
			this.PickupCloseDateEdit.Name = "PickupCloseDateEdit";
			this.MiddleTopRowPanel.SetRow(this.PickupCloseDateEdit, 7);
			this.PickupCloseDateEdit.TabIndex = 7;
			//
			// EntryRateTransitTimeDropEdit
			//
			this.EntryRateTransitTimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryRateTransitTimeDropEdit, "TransitTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).TransitTime)));
			this.EntryRateTransitTimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 162, true);
			this.EntryRateTransitTimeDropEdit.Name = "EntryRateTransitTimeDropEdit";
			this.EntryRateTransitTimeDropEdit.PreBoundMaxLength = 2;
			this.MiddleTopRowPanel.SetRow(this.EntryRateTransitTimeDropEdit, 9);
			this.EntryRateTransitTimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.EntryRateTransitTimeDropEdit.TabIndex = 10;

			//
			// TotalCO2eTextBox
			//
			this.TotalCO2eTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCO2eTextBox, "TotalCO2eForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalCO2eForBinding)));
			this.TotalCO2eTextBox.CaptionResourceString = Res.GetData("e67bc285-fb06-40db-b219-624d63c52787", "CO2e");
			this.TotalCO2eTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.TotalCO2eTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eTextBox.Name = "TotalCO2eTextBox";
			this.MiddleTopRowPanel.SetRow(this.TotalCO2eTextBox, 10);
			this.TotalCO2eTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.TotalCO2eTextBox.TabIndex = 11;
			this.TotalCO2eTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCO2eTextBox.Visible = false;

			//
			// TotalCO2eUnitLabel
			//
			this.TotalCO2eUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 88, true);
			this.TotalCO2eUnitLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eUnitLabel.Name = "TotalCO2eUnitLabel";
			this.TotalCO2eUnitLabel.Text = Res.GetString("b36d6ff2-8d73-4191-a899-26b6295bb969", "KG");
			this.MiddleTopRowPanel.SetRow(this.TotalCO2eUnitLabel, 10);
			this.TotalCO2eUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.TotalCO2eUnitLabel.TabIndex = 12;
			this.TotalCO2eUnitLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// OriginCodeFindBox
			//
			this.OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Origin)));
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.MiddleTopRowPanel.SetRow(this.OriginCodeFindBox, 0);
			this.OriginCodeFindBox.ShouldResize = true;
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.OriginCodeFindBox.TabIndex = 1;
			//
			// DeliveryOpenDateEdit
			//
			this.DeliveryOpenDateEdit.AllowDrop = true;
			this.DeliveryOpenDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeliveryOpenDateEdit, "DeliveryOpen");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).DeliveryOpen)));
			this.DeliveryOpenDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveryOpenDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 139, true);
			this.DeliveryOpenDateEdit.Name = "DeliveryOpenDateEdit";
			this.MiddleTopRowPanel.SetRow(this.DeliveryOpenDateEdit, 8);
			this.DeliveryOpenDateEdit.TabIndex = 8;
			//
			// PickupReadyDateEdit
			//
			this.PickupReadyDateEdit.AllowDrop = true;
			this.PickupReadyDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PickupReadyDateEdit, "PickupReady");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).PickupReady)));
			this.PickupReadyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PickupReadyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 116, true);
			this.PickupReadyDateEdit.Name = "PickupReadyDateEdit";
			this.MiddleTopRowPanel.SetRow(this.PickupReadyDateEdit, 7);
			this.PickupReadyDateEdit.TabIndex = 6;
			//
			// DestinationCodeFindBox
			//
			this.DestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCodeFindBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Destination)));
			this.DestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.DestinationCodeFindBox.Name = "DestinationCodeFindBox";
			this.MiddleTopRowPanel.SetRow(this.DestinationCodeFindBox, 1);
			this.DestinationCodeFindBox.ShouldResize = true;
			this.DestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.DestinationCodeFindBox.TabIndex = 2;
			//
			// ViaCodeFindBox
			//
			this.ViaCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ViaCodeFindBox, "Via");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Via)));
			this.ViaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.ViaCodeFindBox.Name = "ViaCodeFindBox";
			this.MiddleTopRowPanel.SetRow(this.ViaCodeFindBox, 2);
			this.ViaCodeFindBox.ShouldResize = true;
			this.ViaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ViaCodeFindBox.TabIndex = 3;
			//
			// CarrierGuidFindBox
			//
			this.CarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OH_Carrier)));
			this.CarrierGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
			this.MiddleTopRowPanel.SetRow(this.CarrierGuidFindBox, 3);
			this.CarrierGuidFindBox.ShouldResize = true;
			this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.CarrierGuidFindBox.TabIndex = 4;
			//
			// CreditorGuidFindBox
			//
			this.CreditorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorGuidFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_OH_Creditor)));
			this.CreditorGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CreditorGuidFindBox.CaptionResourceString = Res.GetData("6b55116c-a16f-41e8-ad0e-09a935c9d3a9", "Creditor");
			this.CreditorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.CreditorGuidFindBox.Name = "CreditorGuidFindBox";
			this.MiddleTopRowPanel.SetRow(this.CreditorGuidFindBox, 4);
			this.CreditorGuidFindBox.ShouldResize = true;
			this.CreditorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.CreditorGuidFindBox.TabIndex = 4;
			//
			// CarrierServiceLevelDropEdit
			//
			this.CarrierServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceLevelDropEdit, "CarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).CarrierServiceLevel)));
			this.CarrierServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 93, true);
			this.CarrierServiceLevelDropEdit.Name = "CarrierServiceLevelDropEdit";
			this.CarrierServiceLevelDropEdit.PreBoundMaxLength = 2;
			this.MiddleTopRowPanel.SetRow(this.CarrierServiceLevelDropEdit, 6);
			this.CarrierServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 18, true);
			this.CarrierServiceLevelDropEdit.TabIndex = 5;
			//
			// CarrierContractNumberBoundTextEdit
			//
			this.BindingSource.SetBindingMember(this.CarrierContractNumberBoundTextEdit, "CarrierContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).CarrierContractNumber)));
			this.CarrierContractNumberBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.CarrierContractNumberBoundTextEdit.Name = "CarrierContractNumberBoundTextEdit";
			this.CarrierContractNumberBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.CarrierContractNumberBoundTextEdit.TabIndex = 0;
			this.MiddleTopRowPanel.SetRow(this.CarrierContractNumberBoundTextEdit, 5);
			//
			// CarrierContractNumberCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.CarrierContractNumberFindBox, "CarrierContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).CarrierContractNumber)));
			this.CarrierContractNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 93, true);
			this.CarrierContractNumberFindBox.Name = "CarrierContractNumberFindBox";
			this.CarrierContractNumberFindBox.TabIndex = 0;
			this.CarrierContractNumberFindBox.ShowDescriptionBox = false;
			this.CarrierContractNumberFindBox.ShouldResize = false;
			this.CarrierContractNumberFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CarrierContractNumberFindBox.PreBoundMaxLength = 15;
			this.MiddleTopRowPanel.SetRow(this.CarrierContractNumberFindBox, 5);
			//
			// AllocationRouteCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.AllocationRouteCodeFindBox, "AllocationLinePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).AllocationLinePK)));
			this.AllocationRouteCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 450, true);
			this.AllocationRouteCodeFindBox.Name = "AllocationRouteCodeFindBox";
			this.AllocationRouteCodeFindBox.ShowDescriptionBox = false;
			this.AllocationRouteCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.AllocationRouteCodeFindBox.TabIndex = 5;
			this.MiddleTopRowPanel.SetRow(this.AllocationRouteCodeFindBox, 5);
			//
			// LeftTopPanel
			//
			this.LeftTopPanel.Controls.Add(this.General1);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftTopPanel, true);
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 273, true);
			this.LeftTopPanel.TabIndex = 0;
			//
			// General1
			//
			this.General1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.General1.Controls.Add(this.LeftTopRowPanel);
			this.General1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.General1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.General1.Name = "General1";
			this.General1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 250, true);
			this.General1.TabIndex = 0;
			this.General1.TabStop = false;
			//
			// LeftTopRowPanel
			//
			this.LeftTopRowPanel.Controls.Add(this.ChargesApplyDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.ReleaseTypeDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.AdditionalTermsTextBox);
			this.LeftTopRowPanel.Controls.Add(this.TransportModeDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.ContainerModeDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.CompTariffLevelDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.OneOffQuoteHBLDeliveryModeDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.IsDomesticCheckBox);
			this.LeftTopRowPanel.Controls.Add(this.MarksAndNumbersNotePopupEdit);
			this.LeftTopRowPanel.Controls.Add(this.JS_ShippingReferenceTextEdit);
			this.LeftTopRowPanel.Controls.Add(this.JS_GoodsDescriptionBoundTextBox);
			this.LeftTopRowPanel.Controls.Add(this.PaymentTermDropEdit);
			this.LeftTopRowPanel.Controls.Add(this.ServiceLevelFindBox);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftTopRowPanel, true);
			this.LeftTopRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 15, true);
			this.LeftTopRowPanel.Name = "LeftTopRowPanel";
			this.LeftTopRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.LeftTopRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 225, true);
			this.LeftTopRowPanel.TabIndex = 0;
			//
			// ChargesApplyDropEdit
			//
			this.ChargesApplyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargesApplyDropEdit, "HBLAWBChargesDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).HBLAWBChargesDisplay)));
			this.ChargesApplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.ChargesApplyDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ChargesApplyDropEdit.Name = "ChargesApplyDropEdit";
			this.ChargesApplyDropEdit.PreBoundMaxLength = 3;
			this.LeftTopRowPanel.SetRow(this.ChargesApplyDropEdit, 10);
			this.ChargesApplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ChargesApplyDropEdit.TabIndex = 11;
			//
			// ReleaseTypeDropEdit
			//
			this.ReleaseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseTypeDropEdit, "Booking+JS_ReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ReleaseType)));
			this.ReleaseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 207, true);
			this.ReleaseTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ReleaseTypeDropEdit.Name = "ReleaseTypeDropEdit";
			this.ReleaseTypeDropEdit.PreBoundMaxLength = 3;
			this.LeftTopRowPanel.SetRow(this.ReleaseTypeDropEdit, 9);
			this.ReleaseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ReleaseTypeDropEdit.TabIndex = 10;
			//
			// AdditionalTermsTextBox
			//
			this.BindingSource.SetBindingMember(this.AdditionalTermsTextBox, "AdditionalTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).AdditionalTerms)));
			this.AdditionalTermsTextBox.CaptionResourceString = null;
			this.AdditionalTermsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalTermsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.AdditionalTermsTextBox.Name = "AdditionalTermsTextBox";
			this.LeftTopRowPanel.SetRow(this.AdditionalTermsTextBox, 3);
			this.AdditionalTermsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.AdditionalTermsTextBox.TabIndex = 4;
			//
			// TransportModeDropEdit
			//
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|270cd01e-a93c-44d1-ab34-8bf3bad48fe5", "Transport", "The Transport Mode of this quotation / booking.");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.LeftTopRowPanel.SetRow(this.TransportModeDropEdit, 0);
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.TransportModeDropEdit.TabIndex = 1;
			//
			// ContainerModeDropEdit
			//
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ContainerMode)));
			this.ContainerModeDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|cb179b2e-e3b7-46c5-96d5-cfc98052c725", "Container", "The Container Mode of this quotation / booking.");
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.LeftTopRowPanel.SetRow(this.ContainerModeDropEdit, 1);
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.ContainerModeDropEdit.TabIndex = 1;
			//
			// CompTariffLevelDropEdit
			//
			this.CompTariffLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompTariffLevelDropEdit, "CompanyTariffLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).CompanyTariffLevel)));
			this.CompTariffLevelDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|eaf2fa30-7803-499b-b609-f7159b765d8e", "Tariff", "CT Level", "Company Tariff Level Override", "Company Tariff Level Override - Override the Company Tariff Level to be used for Autorating Revenue");
			this.CompTariffLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 184, true);
			this.CompTariffLevelDropEdit.Name = "CompTariffLevelDropEdit";
			this.CompTariffLevelDropEdit.PreBoundMaxLength = 2;
			this.LeftTopRowPanel.SetRow(this.CompTariffLevelDropEdit, 8);
			this.CompTariffLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.CompTariffLevelDropEdit.TabIndex = 9;
			//
			// IsDomesticCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsDomesticCheckBox, "IsDomesticFreight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).IsDomesticFreight)));
			this.IsDomesticCheckBox.CaptionResourceString = Res.GetData("ForwarderMainControl|8179b269-d8e4-45c2-beb6-03d2889b3fa5", "Domestic", "Is Domestic", "Specifies whether this registration is for a domestic freight movement.");
			this.IsDomesticCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsDomesticCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDomesticCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 23, true);
			this.IsDomesticCheckBox.Name = "IsDomesticCheckBox";
			this.LeftTopRowPanel.SetRow(this.IsDomesticCheckBox, 1);
			this.IsDomesticCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.IsDomesticCheckBox.TabIndex = 2;
			this.IsDomesticCheckBox.UseVisualStyleBackColor = true;
			//
			// MarksAndNumbersNotePopupEdit
			//
			this.MarksAndNumbersNotePopupEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MarksAndNumbersNotePopupEdit, "Booking+JS_MarksAndNumbersShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_MarksAndNumbersShort)));
			this.MarksAndNumbersNotePopupEdit.ButtonText = "More...";
			this.MarksAndNumbersNotePopupEdit.ButtonWidth = 52;
			this.MarksAndNumbersNotePopupEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|003902cf-7ecd-43c1-8155-e110faa146be", "Marks & Nums.", "Marks and Numbers", "");
			this.MarksAndNumbersNotePopupEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
			this.MarksAndNumbersNotePopupEdit.MaximumNoteLength = null;
			this.MarksAndNumbersNotePopupEdit.Name = "MarksAndNumbersNotePopupEdit";
			this.MarksAndNumbersNotePopupEdit.NoteTypeDescription = "Marks & Numbers";
			this.MarksAndNumbersNotePopupEdit.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.LeftTopRowPanel.SetRow(this.MarksAndNumbersNotePopupEdit, 7);
			this.MarksAndNumbersNotePopupEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 23, true);
			this.MarksAndNumbersNotePopupEdit.TabIndex = 8;
			//
			// JS_ShippingReferenceTextEdit
			//
			this.BindingSource.SetBindingMember(this.JS_ShippingReferenceTextEdit, "Booking+JS_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_BookingReference)));
			this.JS_ShippingReferenceTextEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|662ce05b-1a8d-4255-8766-c36022d64e5f", "Shipper\'s Ref");
			this.JS_ShippingReferenceTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.JS_ShippingReferenceTextEdit.Name = "JS_ShippingReferenceTextEdit";
			this.LeftTopRowPanel.SetRow(this.JS_ShippingReferenceTextEdit, 5);
			this.JS_ShippingReferenceTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.JS_ShippingReferenceTextEdit.TabIndex = 6;
			//
			// JS_GoodsDescriptionBoundTextBox
			//
			this.JS_GoodsDescriptionBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_GoodsDescriptionBoundTextBox, "Booking+JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_GoodsDescription)));
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = "Details";
			this.JS_GoodsDescriptionBoundTextBox.ButtonWidth = 52;
			this.JS_GoodsDescriptionBoundTextBox.CaptionResourceString = Res.GetData("ForwarderMainControl|c7f0ac96-5788-4fd1-8652-26eafcafe313", "Description");
			this.JS_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.JS_GoodsDescriptionBoundTextBox.MaximumNoteLength = null;
			this.JS_GoodsDescriptionBoundTextBox.Name = "JS_GoodsDescriptionBoundTextBox";
			this.JS_GoodsDescriptionBoundTextBox.NoteTypeDescription = "Detailed Goods Description";
			this.JS_GoodsDescriptionBoundTextBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.LeftTopRowPanel.SetRow(this.JS_GoodsDescriptionBoundTextBox, 6);
			this.JS_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 23, true);
			this.JS_GoodsDescriptionBoundTextBox.TabIndex = 7;
			//
			// PaymentTermDropEdit
			//
			this.PaymentTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTermDropEdit, "PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).PaymentTerms)));
			this.PaymentTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.PaymentTermDropEdit.Name = "PaymentTermDropEdit";
			this.PaymentTermDropEdit.PreBoundMaxLength = 3;
			this.LeftTopRowPanel.SetRow(this.PaymentTermDropEdit, 2);
			this.PaymentTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.PaymentTermDropEdit.TabIndex = 3;
			//
			// ServiceLevelFindBox
			//
			this.ServiceLevelFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelFindBox, "ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ServiceLevel)));
			this.ServiceLevelFindBox.CaptionResourceString = Res.GetData("ForwarderMainControl|ce240c89-4d7a-4dc9-9796-a9212b9160c8", "Svc. Lvl.", "Service Level", "The Service Level of this registration.");
			this.ServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.ServiceLevelFindBox.Name = "ServiceLevelFindBox";
			this.LeftTopRowPanel.SetRow(this.ServiceLevelFindBox, 4);
			this.ServiceLevelFindBox.ShouldResize = true;
			this.ServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ServiceLevelFindBox.TabIndex = 5;
			//
			// HBLDeliveryModeDropEdit
			//
			this.OneOffQuoteHBLDeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OneOffQuoteHBLDeliveryModeDropEdit, "ContainerPackModeOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ContainerPackModeOverride)));
			this.OneOffQuoteHBLDeliveryModeDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|aac98ab1-2179-4c74-ba81-582af41f0d7b", "HBL Dlv. Mode", "HBL Delivery Mode");
			this.OneOffQuoteHBLDeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.OneOffQuoteHBLDeliveryModeDropEdit.Name = "OneOffQuoteHBLDeliveryModeDropEdit";
			this.OneOffQuoteHBLDeliveryModeDropEdit.PreBoundMaxLength = 2;
			this.LeftTopRowPanel.SetRow(this.OneOffQuoteHBLDeliveryModeDropEdit, 5);
			this.OneOffQuoteHBLDeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.OneOffQuoteHBLDeliveryModeDropEdit.TabIndex = 6;
			//
			// MainBottomPanel
			//
			this.MainBottomPanel.Controls.Add(this.MiddleBottomPanel);
			this.MainBottomPanel.Controls.Add(this.RightBottomPanel);
			this.MainBottomPanel.Controls.Add(this.LeftBottomPanel);
			this.MainBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MainBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 455, true);
			this.MainBottomPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 132, true);
			this.MainBottomPanel.Name = "MainBottomPanel";
			this.MainBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 132, true);
			this.MainBottomPanel.TabIndex = 2;
			//
			// MiddleBottomPanel
			//
			this.MiddleBottomPanel.AutoSize = true;
			this.MiddleBottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MiddleBottomPanel.Controls.Add(this.GoodsDetails2);
			this.MiddleBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleBottomPanel, true);
			this.MiddleBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 0, true);
			this.MiddleBottomPanel.Name = "MiddleBottomPanel";
			this.MiddleBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 132, true);
			this.MiddleBottomPanel.TabIndex = 1;
			//
			// GoodsDetails2
			//
			this.GoodsDetails2.AutoSize = true;
			this.GoodsDetails2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GoodsDetails2.CaptionResourceString = Res.GetData("ForwarderMainControl|cbe500e4-4aea-4aaf-ab12-d222ea2ecea8", "Goods Details");
			this.GoodsDetails2.Controls.Add(this.MiddleBottomRowPanel);
			this.GoodsDetails2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsDetails2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsDetails2.Name = "GoodsDetails2";
			this.GoodsDetails2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 132, true);
			this.GoodsDetails2.TabIndex = 0;
			this.GoodsDetails2.TabStop = false;
			//
			// MiddleBottomRowPanel
			//
			this.MiddleBottomRowPanel.Controls.Add(this.PickupEquipmentDropEdit);
			this.MiddleBottomRowPanel.Controls.Add(this.DeliveryEquipmentDropEdit);
			this.MiddleBottomRowPanel.Controls.Add(this.CommodityFindBox);
			this.MiddleBottomRowPanel.Controls.Add(this.RateLocalCode);
			this.MiddleBottomRowPanel.Controls.Add(this.FMCTariffID);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleBottomRowPanel, true);
			this.MiddleBottomRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 14, true);
			this.MiddleBottomRowPanel.Name = "MiddleBottomRowPanel";
			this.MiddleBottomRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.MiddleBottomRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 118, true);
			this.MiddleBottomRowPanel.TabIndex = 0;
			//
			// PickupEquipmentDropEdit
			//
			this.PickupEquipmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupEquipmentDropEdit, "PickupEquipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).PickupEquipment)));
			this.PickupEquipmentDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|d2b87a8c-2630-4a95-86d6-74d34fdb169f", "Pic. Drop", "Pickup Drop Mode", "The drop mode / equipment to use for pickup purposes.");
			this.PickupEquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickupEquipmentDropEdit.Name = "PickupEquipmentDropEdit";
			this.MiddleBottomRowPanel.SetRow(this.PickupEquipmentDropEdit, 0);
			this.PickupEquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.PickupEquipmentDropEdit.TabIndex = 1;
			//
			// DeliveryEquipmentDropEdit
			//
			this.DeliveryEquipmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryEquipmentDropEdit, "DeliveryEquipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).DeliveryEquipment)));
			this.DeliveryEquipmentDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|ff634db9-94b1-4ccd-b3b0-a21244cb6edc", "Dlv. Drop", "Delivery Drop Mode", "The drop mode / equipment to use for delivery purposes.");
			this.DeliveryEquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.DeliveryEquipmentDropEdit.Name = "DeliveryEquipmentDropEdit";
			this.MiddleBottomRowPanel.SetRow(this.DeliveryEquipmentDropEdit, 1);
			this.DeliveryEquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.DeliveryEquipmentDropEdit.TabIndex = 2;
			//
			// CommodityFindBox
			//
			this.CommodityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityFindBox, "Commodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Commodity)));
			this.CommodityFindBox.CaptionResourceString = Res.GetData("ForwarderMainControl|12b8e069-4faf-47f9-a632-51cfbab47193", "Commodity", "The commodity that is being quoted/moved.");
			this.CommodityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.CommodityFindBox.Name = "CommodityFindBox";
			this.MiddleBottomRowPanel.SetRow(this.CommodityFindBox, 2);
			this.CommodityFindBox.ShouldResize = true;
			this.CommodityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.CommodityFindBox.TabIndex = 3;
			//
			// RateLocalCode
			//
			this.RateLocalCode.AllowDrop = false;
			this.RateLocalCode.ReadOnly = true;
			this.BindingSource.SetBindingMember(this.RateLocalCode, "RateLocalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).RateLocalCode)));
			this.RateLocalCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.RateLocalCode.CaptionResourceString = Res.GetData("FreightRatesControl|f26d35aa-847b-4a2a-a8b8-b67a4384ddd7", "Comm. LC", "Commodity Local Code", "The Rating Local Code that represents the Commodity");
			this.RateLocalCode.Name = "RateLocalCode";
			this.MiddleBottomRowPanel.SetRow(this.RateLocalCode, 3);
			this.RateLocalCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.RateLocalCode.TabIndex = 4;
			//
			// FMCTariffID
			//
			this.FMCTariffID.AllowDrop = false;
			this.FMCTariffID.ReadOnly = true;
			this.BindingSource.SetBindingMember(this.FMCTariffID, "FMCTariffID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).FMCTariffID)));
			this.FMCTariffID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.FMCTariffID.CaptionResourceString = Res.GetData("FreightRatesControl|00962a1f-a9f2-4fe3-b8fa-cc32e95bb627", "FMC TID", "FMC Tariff ID", "Federal Maritime Commission Tariff ID");
			this.FMCTariffID.Name = "FMCTariffID";
			this.MiddleBottomRowPanel.SetRow(this.FMCTariffID, 4);
			this.FMCTariffID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.FMCTariffID.TabIndex = 5;

			//
			// RightBottomPanel
			//
			this.RightBottomPanel.Controls.Add(this.MonetaryValues);
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightBottomPanel, true);
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 0, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 110, true);
			this.RightBottomPanel.TabIndex = 2;
			//
			// MonetaryValues
			//
			this.MonetaryValues.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MonetaryValues.CaptionResourceString = Res.GetData("ForwarderMainControl|b7d3e7d9-229b-4905-a572-29e7fac23833", "Monetary Values");
			this.MonetaryValues.Controls.Add(this.RightBottomRowPanel);
			this.MonetaryValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MonetaryValues.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MonetaryValues.Name = "MonetaryValues";
			this.MonetaryValues.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 110, true);
			this.MonetaryValues.TabIndex = 0;
			this.MonetaryValues.TabStop = false;
			//
			// RightBottomRowPanel
			//
			this.RightBottomRowPanel.Controls.Add(this.FreightSpotRate);
			this.RightBottomRowPanel.Controls.Add(this.ValueOfInsuranceCurrencyFindbox);
			this.RightBottomRowPanel.Controls.Add(this.ValueOfGoodsCurrencyFindbox);
			this.RightBottomRowPanel.Controls.Add(this.ValueOfInsuranceCalcEdit);
			this.RightBottomRowPanel.Controls.Add(this.JP_InsuranceRequiredCheckBox);
			this.RightBottomRowPanel.Controls.Add(this.ValueOfGoodsCalcEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightBottomRowPanel, true);
			this.RightBottomRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 14, true);
			this.RightBottomRowPanel.Name = "RightBottomRowPanel";
			this.RightBottomRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.RightBottomRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 94, true);
			this.RightBottomRowPanel.TabIndex = 0;
			//
			// FreightSpotRate
			//
			this.FreightSpotRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightSpotRate, "Booking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking)));
			this.FreightSpotRate.CaptionResourceString = Res.GetData("f0536918-c7d0-4d4a-a8f2-ae748a9d87c1", "Spot Rate");
			this.FreightSpotRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.FreightSpotRate.Name = "FreightSpotRate";
			this.RightBottomRowPanel.SetRow(this.FreightSpotRate, 3);
			this.FreightSpotRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 21, true);
			this.FreightSpotRate.TabIndex = 6;
			//
			// ValueOfInsuranceCurrencyFindbox
			//
			this.ValueOfInsuranceCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCurrencyFindbox, "InsuranceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceCurrency)));
			this.ValueOfInsuranceCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 23, true);
			this.ValueOfInsuranceCurrencyFindbox.Name = "ValueOfInsuranceCurrencyFindbox";
			this.ValueOfInsuranceCurrencyFindbox.PreBoundMaxLength = 3;
			this.RightBottomRowPanel.SetRow(this.ValueOfInsuranceCurrencyFindbox, 1);
			this.ValueOfInsuranceCurrencyFindbox.ShouldResize = true;
			this.ValueOfInsuranceCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfInsuranceCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.ValueOfInsuranceCurrencyFindbox.TabIndex = 4;
			//
			// ValueOfGoodsCurrencyFindbox
			//
			this.ValueOfGoodsCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCurrencyFindbox, "GoodsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsCurrency)));
			this.ValueOfGoodsCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.ValueOfGoodsCurrencyFindbox.Name = "ValueOfGoodsCurrencyFindbox";
			this.ValueOfGoodsCurrencyFindbox.PreBoundMaxLength = 3;
			this.RightBottomRowPanel.SetRow(this.ValueOfGoodsCurrencyFindbox, 0);
			this.ValueOfGoodsCurrencyFindbox.ShouldResize = true;
			this.ValueOfGoodsCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfGoodsCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.ValueOfGoodsCurrencyFindbox.TabIndex = 2;
			//
			// ValueOfInsuranceCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCalcEdit, "InsuranceValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceValue)));
			this.ValueOfInsuranceCalcEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|17dcd5fb-71de-4260-93ac-a9cd66a3f338", "Ins Value", "Insurance Value", "The value of goods for insurance purposes.");
			this.ValueOfInsuranceCalcEdit.DecimalPlaces = 2;
			this.ValueOfInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ValueOfInsuranceCalcEdit.Name = "ValueOfInsuranceCalcEdit";
			this.RightBottomRowPanel.SetRow(this.ValueOfInsuranceCalcEdit, 1);
			this.ValueOfInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
			this.ValueOfInsuranceCalcEdit.TabIndex = 3;
			this.ValueOfInsuranceCalcEdit.Text = "0.00";
			this.ValueOfInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JP_InsuranceRequiredCheckBox
			//
			this.BindingSource.SetBindingMember(this.JP_InsuranceRequiredCheckBox, "Booking+DocsAndCartage+JP_InsuranceRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.JP_InsuranceRequired)));
			this.JP_InsuranceRequiredCheckBox.CaptionResourceString = Res.GetData("ForwarderMainControl|eeb4394a-4270-483c-ad0a-2ab1b31e0b16", "Insurance Required");
			this.JP_InsuranceRequiredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JP_InsuranceRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.JP_InsuranceRequiredCheckBox.Name = "JP_InsuranceRequiredCheckBox";
			this.RightBottomRowPanel.SetRow(this.JP_InsuranceRequiredCheckBox, 2);
			this.JP_InsuranceRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.JP_InsuranceRequiredCheckBox.TabIndex = 5;
			//
			// ValueOfGoodsCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCalcEdit, "GoodsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsValue)));
			this.ValueOfGoodsCalcEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|807a6e6f-5916-47ca-a94a-6003ae206af9", "Goods Val", "Goods Value", "The value of goods for this quotation/movement.");
			this.ValueOfGoodsCalcEdit.DecimalPlaces = 2;
			this.ValueOfGoodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValueOfGoodsCalcEdit.Name = "ValueOfGoodsCalcEdit";
			this.RightBottomRowPanel.SetRow(this.ValueOfGoodsCalcEdit, 0);
			this.ValueOfGoodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
			this.ValueOfGoodsCalcEdit.TabIndex = 1;
			this.ValueOfGoodsCalcEdit.Text = "0.00";
			this.ValueOfGoodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// LeftBottomPanel
			//
			this.LeftBottomPanel.AutoSize = true;
			this.LeftBottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.LeftBottomPanel.Controls.Add(this.GoodsDetails1);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftBottomPanel, true);
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 110, true);
			this.LeftBottomPanel.TabIndex = 0;
			//
			// GoodsDetails1
			//
			this.GoodsDetails1.AutoSize = true;
			this.GoodsDetails1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GoodsDetails1.CaptionResourceString = Res.GetData("ForwarderMainControl|fc828d96-b135-4a50-a86c-79c008b3c65f", "Goods Details");
			this.GoodsDetails1.Controls.Add(this.LeftBottomRowPanel);
			this.GoodsDetails1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsDetails1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsDetails1.Name = "GoodsDetails1";
			this.GoodsDetails1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 110, true);
			this.GoodsDetails1.TabIndex = 0;
			this.GoodsDetails1.TabStop = false;
			//
			// LeftBottomRowPanel
			//
			this.LeftBottomRowPanel.Controls.Add(this.ChargeableUnitLabel);
			this.LeftBottomRowPanel.Controls.Add(this.ChargeableDropEdit);
			this.LeftBottomRowPanel.Controls.Add(this.PackagesCalcDropEdit);
			this.LeftBottomRowPanel.Controls.Add(this.ActualWeightDropEdit);
			this.LeftBottomRowPanel.Controls.Add(this.VolumeCalcDropEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftBottomRowPanel, true);
			this.LeftBottomRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 14, true);
			this.LeftBottomRowPanel.Name = "LeftBottomRowPanel";
			this.LeftBottomRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.LeftBottomRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 94, true);
			this.LeftBottomRowPanel.TabIndex = 0;
			//
			// ChargeableUnitLabel
			//
			this.BindingSource.SetBindingMember(this.ChargeableUnitLabel, "ChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ChargeableUnit)));
			this.ChargeableUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableUnitLabel, false);
			this.ChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 69, true);
			this.ChargeableUnitLabel.Name = "ChargeableUnitLabel";
			this.LeftBottomRowPanel.SetRow(this.ChargeableUnitLabel, 3);
			this.ChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 20, true);
			this.ChargeableUnitLabel.TabIndex = 5;
			this.ChargeableUnitLabel.Text = "M3";
			//
			// ChargeableDropEdit
			//
			this.BindingSource.SetBindingMember(this.ChargeableDropEdit, "Chargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Chargeable)));
			this.ChargeableDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|acce6d49-2003-4bed-8e0a-204472739c92", "Chargeable", "The Chargeable Weight/Volume for this movement.");
			this.ChargeableDropEdit.DecimalPlaces = 2;
			this.ChargeableDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.ChargeableDropEdit.Name = "ChargeableDropEdit";
			this.LeftBottomRowPanel.SetRow(this.ChargeableDropEdit, 3);
			this.ChargeableDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.ChargeableDropEdit.TabIndex = 4;
			this.ChargeableDropEdit.Text = "0.000";
			this.ChargeableDropEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// PackagesCalcDropEdit
			//
			this.PackagesCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_F3_NKPackType)));
			this.PackagesCalcDropEdit.BindToAmount = "Booking+JS_OuterPacks";
			this.PackagesCalcDropEdit.BindToUnit = "Booking+JS_F3_NKPackType";
			this.PackagesCalcDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|26600a91-e184-415c-a698-02099b9b96dc", "Outers");
			this.PackagesCalcDropEdit.Decimals = 0;
			this.PackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesCalcDropEdit.Name = "PackagesCalcDropEdit";
			this.LeftBottomRowPanel.SetRow(this.PackagesCalcDropEdit, 0);
			this.PackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.PackagesCalcDropEdit.TabIndex = 1;
			this.PackagesCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// ActualWeightDropEdit
			//
			this.ActualWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).WeightUnit)));
			this.ActualWeightDropEdit.BindToAmount = "Weight";
			this.ActualWeightDropEdit.BindToUnit = "WeightUnit";
			this.ActualWeightDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|da202406-60d7-419d-a2f6-c8c65b338bff", "Weight", "The weight of this movement.");
			this.ActualWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ActualWeightDropEdit.Name = "ActualWeightDropEdit";
			this.LeftBottomRowPanel.SetRow(this.ActualWeightDropEdit, 1);
			this.ActualWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ActualWeightDropEdit.TabIndex = 2;
			this.ActualWeightDropEdit.UnitPreBoundMaxLength = 2;
			//
			// VolumeCalcDropEdit
			//
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).VolumeUnit)));
			this.VolumeCalcDropEdit.BindToAmount = "Volume";
			this.VolumeCalcDropEdit.BindToUnit = "VolumeUnit";
			this.VolumeCalcDropEdit.CaptionResourceString = Res.GetData("ForwarderMainControl|912db1fe-337f-444f-9afe-cd15053837c8", "Volume", "The volume of this movement.");
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.LeftBottomRowPanel.SetRow(this.VolumeCalcDropEdit, 2);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 3;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// MainTopPanel
			//
			this.MainTopPanel.Controls.Add(this.DeliveryDocAddressControl);
			this.MainTopPanel.Controls.Add(this.PickupDocAddressControl);
			this.MainTopPanel.Controls.Add(this.ClientControl);
			this.MainTopPanel.Controls.Add(this.OrgRoleDropEdit);
			this.MainTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTopPanel.Name = "MainTopPanel";
			this.MainTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 202, true);
			this.MainTopPanel.TabIndex = 0;
			//
			// DeliveryDocAddressControl
			//
			this.DeliveryDocAddressControl.AddressValidationProcessCmdKey = null;
			this.DeliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ConsigneeDocumentaryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "Consignee_List";
			this.DeliveryDocAddressControl.CaptionResourceString = Res.GetData("ForwarderMainControl|2BD8D65E-684B-4e5a-81D9-07496D60B502", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryDocAddressControl, false);
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 30, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.ReadOnly = false;
			this.DeliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 3;
			this.DeliveryDocAddressControl.ValidationJustForced = false;
			//
			// PickupDocAddressControl
			//
			this.PickupDocAddressControl.AddressValidationProcessCmdKey = null;
			this.PickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupDocAddressControl, "ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ConsignorDocumentaryAddress)));
			this.PickupDocAddressControl.BindToOrganisations = "Consignor_List";
			this.PickupDocAddressControl.CaptionResourceString = Res.GetData("ForwarderMainControl|030bd527-c1da-457e-92cb-69570707e054", "Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupDocAddressControl, false);
			this.PickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 30, true);
			this.PickupDocAddressControl.Name = "PickupDocAddressControl";
			this.PickupDocAddressControl.ReadOnly = false;
			this.PickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.PickupDocAddressControl.TabIndex = 2;
			this.PickupDocAddressControl.ValidationJustForced = false;
			//
			// ClientControl
			//
			this.ClientControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientControl, ".");
			this.ClientControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.ClientControl.Name = "ClientControl";
			this.ClientControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 174, true);
			this.ClientControl.TabIndex = 1;
			// 
			// OrgRoleDropEdit
			// 
			this.OrgRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgRoleDropEdit, "OrgRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OrgRole)));
			this.OrgRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 2, true);
			this.OrgRoleDropEdit.Name = "OrgRoleDropEdit";
			this.OrgRoleDropEdit.PreBoundMaxLength = 3;
			this.OrgRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.OrgRoleDropEdit.TabIndex = 0;
			//
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.RightTopStaticPanel);
			this.TopPanel.Controls.Add(this.MainPanel);
			this.TopPanel.Controls.Add(this.OneOffQuoteStatisticsPanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1398, 615, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 637, true);
			this.TopPanel.TabIndex = 0;
			//
			// ForwarderMainControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "ForwarderMainControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 690, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierServiceLevelDropEdit.ResumeLayout(true);
			this.CarrierServiceLevelDropEdit.PerformLayout();
			this.PotentialCarriersGroupBox.ResumeLayout(false);
			this.PotentialCarriersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PotentialCarriersGrid)).EndInit();
			this.PotentialCarriersGrid.ResumeLayout(false);
			this.PotentialCarriersGrid.PerformLayout();
			this.QuotedBookingOrderManagementControl.ResumeLayout(true);
			this.QuotedBookingOrderManagementControl.PerformLayout();
			this.General3.ResumeLayout(false);
			this.General3.PerformLayout();
			this.CustomsEntryNumberTypeBoundDropEdit.ResumeLayout(true);
			this.CustomsEntryNumberTypeBoundDropEdit.PerformLayout();
			this.ShipperCODTypeDropEdit.ResumeLayout(true);
			this.ShipperCODTypeDropEdit.PerformLayout();
			this.ScreeningControlsGroupBox.ResumeLayout(false);
			this.ScreeningControlsGroupBox.PerformLayout();
			this.FreightRatesGroupBox.ResumeLayout(false);
			this.FreightRatesGroupBox.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.ApprovedShipperInspectionTypeDropEdit.ResumeLayout(true);
			this.ApprovedShipperInspectionTypeDropEdit.PerformLayout();
			this.EndDateDateEdit.ResumeLayout(true);
			this.EndDateDateEdit.PerformLayout();
			this.StartDateDateEdit.ResumeLayout(true);
			this.StartDateDateEdit.PerformLayout();
			this.OneOffQuoteKPIDropEdit.ResumeLayout(true);
			this.OneOffQuoteKPIDropEdit.PerformLayout();
			this.OneOffQuoteSourceDropEdit.ResumeLayout(true);
			this.OneOffQuoteSourceDropEdit.PerformLayout();
			this.OneOffQuoteRevisionReasonDropEdit.ResumeLayout(true);
			this.OneOffQuoteRevisionReasonDropEdit.PerformLayout();
			this.General2.ResumeLayout(false);
			this.General2.PerformLayout();
			this.ContainerModeOverrideDropEdit.ResumeLayout(true);
			this.ContainerModeOverrideDropEdit.PerformLayout();
			this.OneOffQuoteHBLDeliveryModeDropEdit.ResumeLayout(true);
			this.OneOffQuoteHBLDeliveryModeDropEdit.PerformLayout();
			this.OnBoardDropEdit.ResumeLayout(true);
			this.OnBoardDropEdit.PerformLayout();
			this.DeliveryDueDateDateEdit.ResumeLayout(true);
			this.DeliveryDueDateDateEdit.PerformLayout();
			this.EntryRateFrequencyCalcDropEdit.ResumeLayout(true);
			this.EntryRateFrequencyCalcDropEdit.PerformLayout();
			this.DeliveryCloseDateEdit.ResumeLayout(true);
			this.DeliveryCloseDateEdit.PerformLayout();
			this.PickupCloseDateEdit.ResumeLayout(true);
			this.PickupCloseDateEdit.PerformLayout();
			this.EntryRateTransitTimeDropEdit.ResumeLayout(true);
			this.EntryRateTransitTimeDropEdit.PerformLayout();
			this.TotalCO2eTextBox.ResumeLayout(true);
			this.TotalCO2eTextBox.PerformLayout();
			this.CreditorGuidFindBox.ResumeLayout(true);
			this.CreditorGuidFindBox.PerformLayout();
			this.TotalCO2eUnitLabel.ResumeLayout(true);
			this.TotalCO2eUnitLabel.PerformLayout();
			this.OriginCodeFindBox.ResumeLayout(true);
			this.OriginCodeFindBox.PerformLayout();
			this.DeliveryOpenDateEdit.ResumeLayout(true);
			this.DeliveryOpenDateEdit.PerformLayout();
			this.PickupReadyDateEdit.ResumeLayout(true);
			this.PickupReadyDateEdit.PerformLayout();
			this.DestinationCodeFindBox.ResumeLayout(true);
			this.DestinationCodeFindBox.PerformLayout();
			this.ViaCodeFindBox.ResumeLayout(true);
			this.ViaCodeFindBox.PerformLayout();
			this.CarrierGuidFindBox.ResumeLayout(true);
			this.CarrierGuidFindBox.PerformLayout();
			this.General1.ResumeLayout(false);
			this.General1.PerformLayout();
			this.ChargesApplyDropEdit.ResumeLayout(true);
			this.ChargesApplyDropEdit.PerformLayout();
			this.ReleaseTypeDropEdit.ResumeLayout(true);
			this.ReleaseTypeDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.ContainerModeDropEdit.ResumeLayout(true);
			this.ContainerModeDropEdit.PerformLayout();
			this.CompTariffLevelDropEdit.ResumeLayout(true);
			this.CompTariffLevelDropEdit.PerformLayout();
			this.MarksAndNumbersNotePopupEdit.ResumeLayout(true);
			this.MarksAndNumbersNotePopupEdit.PerformLayout();
			this.JS_GoodsDescriptionBoundTextBox.ResumeLayout(true);
			this.JS_GoodsDescriptionBoundTextBox.PerformLayout();
			this.CompanyTariffLevelOverrideForBookingDropEdit.ResumeLayout(true);
			this.CompanyTariffLevelOverrideForBookingDropEdit.PerformLayout();
			this.PaymentTermDropEdit.ResumeLayout(true);
			this.PaymentTermDropEdit.PerformLayout();
			this.ServiceLevelFindBox.ResumeLayout(true);
			this.ServiceLevelFindBox.PerformLayout();
			this.GoodsDetails2.ResumeLayout(false);
			this.GoodsDetails2.PerformLayout();
			this.PickupEquipmentDropEdit.ResumeLayout(true);
			this.PickupEquipmentDropEdit.PerformLayout();
			this.DeliveryEquipmentDropEdit.ResumeLayout(true);
			this.DeliveryEquipmentDropEdit.PerformLayout();
			this.CommodityFindBox.ResumeLayout(true);
			this.CommodityFindBox.PerformLayout();
			this.MonetaryValues.ResumeLayout(false);
			this.MonetaryValues.PerformLayout();
			this.FreightSpotRate.ResumeLayout(true);
			this.FreightSpotRate.PerformLayout();
			this.ValueOfInsuranceCurrencyFindbox.ResumeLayout(true);
			this.ValueOfInsuranceCurrencyFindbox.PerformLayout();
			this.ValueOfGoodsCurrencyFindbox.ResumeLayout(true);
			this.ValueOfGoodsCurrencyFindbox.PerformLayout();
			this.GoodsDetails1.ResumeLayout(false);
			this.GoodsDetails1.PerformLayout();
			this.PackagesCalcDropEdit.ResumeLayout(true);
			this.PackagesCalcDropEdit.PerformLayout();
			this.ActualWeightDropEdit.ResumeLayout(true);
			this.ActualWeightDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.DeliveryDocAddressControl.ResumeLayout(true);
			this.DeliveryDocAddressControl.PerformLayout();
			this.PickupDocAddressControl.ResumeLayout(true);
			this.PickupDocAddressControl.PerformLayout();
			this.ClientControl.ResumeLayout(true);
			this.ClientControl.PerformLayout();
			this.OrgRoleDropEdit.ResumeLayout(true);
			this.OrgRoleDropEdit.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.MiddleTopRowPanel.ResumeLayout(false);
			this.MiddleTopRowPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.RightTopStaticPanel.ResumeLayout(false);
			this.RightTopStaticPanel.PerformLayout();
			this.OneOffQuoteStatisticsPanel.ResumeLayout(false);
			this.OneOffQuoteStatisticsPanel.PerformLayout();
			this.RightTopPanel.ResumeLayout(false);
			this.RightTopPanel.PerformLayout();
			this.RightTopRowPanel.ResumeLayout(false);
			this.RightTopRowPanel.PerformLayout();
			this.AviationSecurityPanel.ResumeLayout(false);
			this.AviationSecurityPanel.PerformLayout();
			this.MainMiddlePanel.ResumeLayout(false);
			this.MainMiddlePanel.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.LeftTopRowPanel.ResumeLayout(false);
			this.LeftTopRowPanel.PerformLayout();
			this.MainBottomPanel.ResumeLayout(false);
			this.MainBottomPanel.PerformLayout();
			this.MiddleBottomPanel.ResumeLayout(false);
			this.MiddleBottomPanel.PerformLayout();
			this.MiddleBottomRowPanel.ResumeLayout(false);
			this.MiddleBottomRowPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.RightBottomRowPanel.ResumeLayout(false);
			this.RightBottomRowPanel.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.LeftBottomRowPanel.ResumeLayout(false);
			this.LeftBottomRowPanel.PerformLayout();
			this.MainTopPanel.ResumeLayout(false);
			this.MainTopPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZPanel RightTopStaticPanel;
		private ZArchitecture.GUI.ZPanel OneOffQuoteStatisticsPanel;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl containersAndPackLineDynamicControl;
		private QuotedBookingOrderManagementControl QuotedBookingOrderManagementControl;
		private ZArchitecture.GUI.ZDateEdit EndDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit StartDateDateEdit;
		private ZArchitecture.ZCalcEdit ShipperCODAmountCalcEdit;
		private ZArchitecture.GUI.ZDropEdit ShipperCODTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit OneOffQuoteKPIDropEdit;
		private ZArchitecture.GUI.ZDropEdit OneOffQuoteSourceDropEdit;
		private ZArchitecture.GUI.ZDropEdit OneOffQuoteRevisionReasonDropEdit;
		private ZArchitecture.GUI.ZDropEdit CompanyTariffLevelOverrideForBookingDropEdit;
		private ZArchitecture.GUI.ZPanel MainTopPanel;
		private ClientControl ClientControl;
		private ZArchitecture.GUI.ZDropEdit OrgRoleDropEdit;
		private MasterFiles.GUI.ZDocAddressControl PickupDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
		private ZArchitecture.GUI.ZPanel MainMiddlePanel;
		private ZArchitecture.GUI.ZPanel MainBottomPanel;
		private ZArchitecture.GUI.ZPanel MiddleTopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel LeftTopPanel;
		private ZArchitecture.GUI.ZPanel RightBottomPanel;
		private ZArchitecture.GUI.ZPanel MiddleBottomPanel;
		private ZArchitecture.GUI.ZPanel LeftBottomPanel;
		private ZArchitecture.GUI.ZDropEdit CompTariffLevelDropEdit;
		private ZArchitecture.ZTextBox JS_ShippingReferenceTextEdit;
		internal Freight.GUI.ZStmNotePopupEditWithBindableText MarksAndNumbersNotePopupEdit;
		private Freight.GUI.GoodsDescriptionTextBox JS_GoodsDescriptionBoundTextBox;
		private ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ContainerModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PaymentTermDropEdit;
		private ZArchitecture.GUI.ZCheckBox IsDomesticCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox ServiceLevelFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit PackagesCalcDropEdit;
		private ZArchitecture.ZCalcEdit ChargeableDropEdit;
		private ZArchitecture.ZLabel ChargeableUnitLabel;
		private ZArchitecture.GUI.ZCalcDropEdit ActualWeightDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit DeliveryEquipmentDropEdit;
		private ZArchitecture.GUI.ZDropEdit PickupEquipmentDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CommodityFindBox;
		private ZArchitecture.GUI.ZCheckBox JP_InsuranceRequiredCheckBox;
		private ZTextBox RateLocalCode;
		private ZTextBox FMCTariffID;
		private ZArchitecture.GUI.ZCodeFindBox ValueOfInsuranceCurrencyFindbox;
		private ZArchitecture.ZCalcEdit ValueOfInsuranceCalcEdit;
		protected ZArchitecture.GUI.ZDropEdit ApprovedShipperInspectionTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ScreeningControlsGroupBox;
		private ZArchitecture.GUI.ZGroupBox FreightRatesGroupBox;
		private DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
		private ZArchitecture.GUI.ZButton ScreenButton;
		private ZArchitecture.GUI.ZCodeFindBox ValueOfGoodsCurrencyFindbox;
		private ZArchitecture.ZCalcEdit ValueOfGoodsCalcEdit;
		private ZArchitecture.ZCalcEdit EntriesCalcEdit;
		private ZArchitecture.ZCalcEdit EntryInvoiceLinesCalcEdit;
		private ZArchitecture.ZTextBox CustomsEntryNumberBoundTextBox;
		private ZArchitecture.GUI.ZDropEdit CustomsEntryNumberTypeBoundDropEdit;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel LeftTopRowPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel LeftBottomRowPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel MiddleBottomRowPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel RightBottomRowPanel;
		private ZArchitecture.GUI.ZGroupBox General1;
		private ZArchitecture.GUI.ZGroupBox GoodsDetails1;
		private ZArchitecture.GUI.ZGroupBox GoodsDetails2;
		private ZArchitecture.GUI.ZGroupBox MonetaryValues;
		private ZArchitecture.GUI.ZPanel RightTopPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel RightTopRowPanel;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZPanel AviationSecurityPanel;
		private ZArchitecture.GUI.ZGroupBox General3;
		private ZArchitecture.ZTextBox AdditionalTermsTextBox;
		private ZArchitecture.GUI.ZDropEdit ChargesApplyDropEdit;
		private ZArchitecture.GUI.ZDropEdit ReleaseTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox General2;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel MiddleTopRowPanel;
		private ZArchitecture.GUI.ZCalcDropEdit EntryRateFrequencyCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit EntryRateTransitTimeDropEdit;
		private ZArchitecture.GUI.ZDateEdit DeliveryCloseDateEdit;
		private ZArchitecture.GUI.ZDateEdit DeliveryOpenDateEdit;
		private ZArchitecture.GUI.ZDateEdit PickupCloseDateEdit;
		private ZArchitecture.GUI.ZDateEdit PickupReadyDateEdit;
		private ZArchitecture.GUI.ZDateEdit DeliveryDueDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox DestinationCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ViaCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CreditorGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit OnBoardDropEdit;
		private ZArchitecture.GUI.ZDropEdit ContainerModeOverrideDropEdit;
		private ZArchitecture.GUI.ZDropEdit OneOffQuoteHBLDeliveryModeDropEdit;
		private Freight.GUI.FreightRateControl FreightSpotRate;
		private ZArchitecture.GUI.ZDropEdit CarrierServiceLevelDropEdit;
		private ZArchitecture.GUI.ZGroupBox PotentialCarriersGroupBox;
		private ZArchitecture.ZGrid PotentialCarriersGrid;
		private ZTextBox TotalCO2eTextBox;
		private ZLabel TotalCO2eUnitLabel;
		private Enterprise.ZArchitecture.ZTextBox CarrierContractNumberBoundTextEdit;
		private Enterprise.Freight.QuotedBookings.GUI.QuotedBookingContractAllocationGuidFindBox AllocationRouteCodeFindBox;
		private Enterprise.Freight.QuotedBookings.GUI.QuotedBookingContractAllocationCodeFindBox CarrierContractNumberFindBox;
	}
}
