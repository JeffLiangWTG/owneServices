using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USJobDeclarationUserControl
	{
		#region InitializeComponent

		void InitializeComponent()
		{
			this.ImportShipmentTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.US_ImmediateDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_PGAExpeditedReleaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CargoReleaseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_EnableSPNCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LiveEntryIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceByRequestCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnableENSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_CertifyCargoReleaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_EnableAIICheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_EnableCRLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_MonthlyFilingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SEDGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_RN_NKCountryOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignTradeZoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LicenseNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ECCNCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ECCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InbondTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportEntryNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LicenseTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfExportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PortOfExportSchDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfExportSchDCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DateOfExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingSchDFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingSchDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PortOfDischargeSchDFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfDischargeSchDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeOrganisationControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FPPIGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PortOfImportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HMFPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HMFApplicableZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_DateOfFirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfEntryScheduleCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DDTCDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DDTCUSMLCategoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DDTCPartyCertificationIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DDTCMilitaryEquipmentIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DDTCRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DDTCCategoryXXIDeterminationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DDTCITARExemptionNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SEReleaseAndStandAlonePriorNoticePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SPNIDTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SplitShipmentReleaseCodeForSPNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FTZPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FTZSPNIDTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AllocateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ZoneIDDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FTZControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTZYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SEReleasePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SplitShipmentReleaseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AllocateImportEntryNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AccLiqCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntryNumberDividerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PSCCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WarehouseWithdrawalPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarehouseWithdrawalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsFinalWHSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WHSEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WHSDistrictPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WHSEntryFilerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QtyInWhBeforeWithdrawalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QtyBeingWithdrawnCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QtyInWHAfterWithdrawalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TIBPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TIBPurposeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TIBMotorVehiclesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TIBMVNonConformingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IORAuthAgentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AuthorizedAgentRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IORRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PurchasedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManualEntryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Box29Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LocationOfGoodsCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CentralizedExamSiteFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ImportEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryFilerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NonAMSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FTZAdmissionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectDeliveryIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MasterBillIssuerSCACFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HouseBillIssuerSCACFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CarrierSCACCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MasterBillForFTZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportDateOfFirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ITDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PrimaryITNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EstimatedEntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransactionsRelatedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HazardousMaterialDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MergeByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RoutedTransactionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportShipmentTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExportAESTIRShipmentTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.US_FirstPortOfCallCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_SoldEnRouteIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.US_CommodityFilingOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AESTIRRoutedTransactionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AESTIRTariffTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AESTIRHazardousMaterialDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AESTIRTransactionsRelatedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SEDMessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginalITNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_DateOfExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.US_UC_NKCountryOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JourneyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTZSPNCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludePTTCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PTTWithoutExceptionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CalcTransportationModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PipelineNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchTicketTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportOrgsControl = new Enterprise.Customs.US.GUI.JobTransportOrgsControl();
			this.USOrganisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JobMainOrgsControl = new Enterprise.Customs.US.GUI.JobMainOrgsControl();
			this.JobMiscOrgsControl = new Enterprise.Customs.US.GUI.JobMiscOrgsControl();
			this.ConsolidatedSummaryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExpressTrackingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExpressTrackingNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.InsuranceValueCalcFindBox.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.FinalDestinationFindBox.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit2.SuspendLayout();
			this.JE_ExportDateBoundDateEdit2.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ShipmentCustomFieldsPage.SuspendLayout();
			this.shipmentCustomFieldsControl1.SuspendLayout();
			this.OrdersPanel.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.OrdersAttachUserControl.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.FPPIGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImportShipmentTypePanel.SuspendLayout();
			this.CargoReleaseTypeDropEdit.SuspendLayout();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.LiveEntryIndicatorDropEdit.SuspendLayout();
			this.EntryModeDropEdit.SuspendLayout();
			this.EntryTypeDropEdit.SuspendLayout();
			this.SEDGroupBox.SuspendLayout();
			this.US_RN_NKCountryOfDestinationCodeFindBox.SuspendLayout();
			this.StateOfOriginDropEdit.SuspendLayout();
			this.ExportCodeDropEdit.SuspendLayout();
			this.InbondTypeDropEdit.SuspendLayout();
			this.LicenseTypeCodeFindBox.SuspendLayout();
			this.PortOfExportPanel.SuspendLayout();
			this.PortOfExportSchDCodeFindBox.SuspendLayout();
			this.PortOfExportSchDCodeDropEdit.SuspendLayout();
			this.DateOfExportDateEdit.SuspendLayout();
			this.PortOfExportCodeFindBox.SuspendLayout();
			this.PortOfLoadingSchDFindBox.SuspendLayout();
			this.PortOfLoadingSchDDropEdit.SuspendLayout();
			this.PortOfDischargeSchDFindBox.SuspendLayout();
			this.PortOfDischargeSchDDropEdit.SuspendLayout();
			this.ConsigneeOrganisationControl.SuspendLayout();
			this.PortOfImportPanel.SuspendLayout();
			this.HMFPanel.SuspendLayout();
			this.HMFApplicableZDropEdit.SuspendLayout();
			this.JE_DateOfFirstArrivalDateEdit.SuspendLayout();
			this.PortOfEntryScheduleCodeFindBox.SuspendLayout();
			this.DDTCDetailsTabPage.SuspendLayout();
			this.DDTCUSMLCategoryCodeDropEdit.SuspendLayout();
			this.DDTCPartyCertificationIndicatorDropEdit.SuspendLayout();
			this.DDTCMilitaryEquipmentIndicatorDropEdit.SuspendLayout();
			this.DDTCITARExemptionNumberDropEdit.SuspendLayout();
			this.ImportStatusGroupBox.SuspendLayout();
			this.SEReleaseAndStandAlonePriorNoticePanel.SuspendLayout();
			this.SPNIDTypeDropEdit.SuspendLayout();
			this.SplitShipmentReleaseCodeForSPNDropEdit.SuspendLayout();
			this.FTZPanel.SuspendLayout();
			this.FTZSPNIDTypeDropEdit.SuspendLayout();
			this.ZoneIDDropDownEdit.SuspendLayout();
			this.DeliveryDropEdit.SuspendLayout();
			this.SEReleasePanel.SuspendLayout();
			this.SplitShipmentReleaseCodeDropEdit.SuspendLayout();
			this.WarehouseWithdrawalPanel.SuspendLayout();
			this.WHSDistrictPortCodeFindBox.SuspendLayout();
			this.TIBPanel.SuspendLayout();
			this.IORAuthAgentPanel.SuspendLayout();
			this.PurchasedDropEdit.SuspendLayout();
			this.LocationOfGoodsCodeFindBox.SuspendLayout();
			this.CentralizedExamSiteFindBox.SuspendLayout();
			this.FTZAdmissionTypeDropEdit.SuspendLayout();
			this.MasterBillIssuerSCACFindBox.SuspendLayout();
			this.HouseBillIssuerSCACFindBox.SuspendLayout();
			this.CarrierSCACCodeFindBox.SuspendLayout();
			this.DestinationStateDropEdit.SuspendLayout();
			this.ImportDateOfFirstArrivalDateEdit.SuspendLayout();
			this.ITDateDateEdit.SuspendLayout();
			this.EstimatedEntryDateDateEdit.SuspendLayout();
			this.TransactionsRelatedDropEdit.SuspendLayout();
			this.HazardousMaterialDropEdit.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.TariffTypeDropEdit.SuspendLayout();
			this.RoutedTransactionDropEdit.SuspendLayout();
			this.ExportShipmentTypePanel.SuspendLayout();
			this.ExportAESTIRShipmentTypePanel.SuspendLayout();
			this.US_SoldEnRouteIndicatorDropEdit.SuspendLayout();
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.SuspendLayout();
			this.US_CommodityFilingOptionDropEdit.SuspendLayout();
			this.AESTIRRoutedTransactionDropEdit.SuspendLayout();
			this.AESTIRTariffTypeDropEdit.SuspendLayout();
			this.AESTIRHazardousMaterialDropEdit.SuspendLayout();
			this.AESTIRTransactionsRelatedDropEdit.SuspendLayout();
			this.SEDMessageStatusDropEdit.SuspendLayout();
			this.US_DateOfExportDateEdit.SuspendLayout();
			this.US_UC_NKCountryOfExportCodeFindBox.SuspendLayout();
			this.TransportOrgsControl.SuspendLayout();
			this.USOrganisationsTabPage.SuspendLayout();
			this.JobMainOrgsControl.SuspendLayout();
			this.JobMiscOrgsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 17, true);
			this.JE_MessageTypeBoundDropDownEdit.TabIndex = 0;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 41, true);
			this.JE_TransportModeBoundDropDownEdit.MaxItemsToShowInDropDown = 15;
			this.JE_TransportModeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 1;
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 65, true);
			this.JE_ContainerModeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 2;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 41, true);
			this.JE_MessageSubTypeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 154, true);
			this.WeightzCalcDropEdit.TabIndex = 14;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 131, true);
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 11;
			//
			// InsuranceValueCalcFindBox
			//
			this.InsuranceValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 177, true);
			this.InsuranceValueCalcFindBox.TabIndex = 16;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 200, true);
			this.ScreeningStatusDropEdit.TabIndex = 17;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 200, true);
			this.ScreenButton.TabIndex = 18;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 177, true);
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 18;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 108, true);
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 10;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.AllowAlphaInMAWP = true;
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 19, true);
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 67, true);
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.FolioNumberTextBox.TabIndex = 7;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.BindToList = "Lookups+Vessels";
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 43, true);
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselFindBox.TabIndex = 4;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 19, true);
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 67, true);
			this.JE_VoyageFlightNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 6;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 91, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 10;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 115, true);
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 13;
			// 
			// PortOfDischargeFindBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PortOfDischargeFindBox, false);
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 115, true);
			this.PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			// 
			// PortOfLoadingFindBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PortOfLoadingFindBox, false);
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 91, true);
			this.PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 11;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Controls.Add(this.US_UC_NKCountryOfExportCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.US_DateOfExportDateEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.DestinationStateDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.HouseBillIssuerSCACFindBox);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 194, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 224, true);
			this.ShipmentDetailsGroupBox.TabIndex = 4;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillIssuerSCACFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.DestinationStateDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.InsuranceValueCalcFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.US_DateOfExportDateEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.US_UC_NKCountryOfExportCodeFindBox, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.CalcTransportationModeTextBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.PTTWithoutExceptionCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.IncludePTTCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.ConsolidatedSummaryCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.US_MonthlyFilingCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.FTZSPNCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.FTZAdmissionTypeDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.DirectDeliveryIndicatorCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.ImportShipmentTypePanel);
			this.ShipmentTypeGroupBox.Controls.Add(this.ExportShipmentTypePanel);
			this.ShipmentTypeGroupBox.Controls.Add(this.ExportAESTIRShipmentTypePanel);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 286, true);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ExportAESTIRShipmentTypePanel, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ExportShipmentTypePanel, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ImportShipmentTypePanel, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.DirectDeliveryIndicatorCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.FTZAdmissionTypeDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.FTZSPNCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.US_MonthlyFilingCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ConsolidatedSummaryCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.IncludePTTCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.PTTWithoutExceptionCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.CalcTransportationModeTextBox, 0);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 159, true);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 62, true);
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.FinalDestinationFindBox.TabIndex = 5;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 62, true);
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 6;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 39, true);
			this.JE_ExportDateBoundDateEdit2.TabIndex = 4;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 39, true);
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 16, true);
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 2;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 131, true);
			this.IncoTermDropEdit.ShowDescriptionBox = false;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IncoTermDropEdit.TabIndex = 12;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Controls.Add(this.USOrganisationsTabPage);
			this.RightTabControl.Controls.Add(this.DDTCDetailsTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 2, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 645, true);
			this.RightTabControl.Controls.SetChildIndex(this.DDTCDetailsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.NumbersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.ShipmentCustomFieldsPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrdersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.USOrganisationsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.DocsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrganisationsTabPage, 0);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Controls.Add(this.ConsigneeOrganisationControl);
			this.OrganisationsTabPage.Controls.Add(this.FPPIGuidFindBox);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.ConsigneeOrganisationControl, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.FPPIGuidFindBox, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationsTopPanel.Dock = System.Windows.Forms.DockStyle.None;
			this.OrganisationsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 198, true);
			// 
			// ShippingOrAirLineOrganisationControl
			// 
			this.ShippingOrAirLineOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
			// 
			// ForwarderOrganisationControl
			// 
			this.ForwarderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 22, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 177, true);
			this.JE_ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 14;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 177, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 23;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 130, true);
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 21, true);
			this.IncoTermExplainButton.TabIndex = 13;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 0, true);
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 154, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 15;
			// 
			// ContainerTerminalOperatorAddressControl
			// 
			this.ContainerTerminalOperatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 44, true);
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// DepotAddressControl
			// 
			this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 66, true);
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BondedWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 110, true);
			// 
			// ContainerYardAddressControl
			// 
			this.ContainerYardAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 88, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 618, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 418, true);
			// 
			// ExternalBrokerGuidFindBox
			// 
			this.ExternalBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 176, true);
			// 
			// ControllingCustomerGuidFindBox
			// 
			this.ControllingCustomerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 154, true);
			// 
			// ControllingAgentGuidFindBox
			// 
			this.ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 132, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.AccessibleDescription = "1";
			this.TransportDetailsGroupBox.Controls.Add(this.FTZTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ExpressTrackingNumberTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ExpressTrackingCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.BatchTicketTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.NonAMSCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.PipelineNameTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CarrierNameTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JourneyTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.EstimatedEntryDateDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PrimaryITNumberTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ITDateDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.MasterBillForFTZTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CarrierSCACCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfDischargeSchDFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfDischargeSchDDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfLoadingSchDFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfLoadingSchDDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.MasterBillIssuerSCACFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ImportDateOfFirstArrivalDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfImportPanel);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfExportPanel);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 5, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 188, true);
			this.TransportDetailsGroupBox.TabIndex = 3;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfExportPanel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfImportPanel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ImportDateOfFirstArrivalDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MasterBillIssuerSCACFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeSchDDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingSchDDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingSchDFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeSchDFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CarrierSCACCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MasterBillForFTZTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ITDateDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PrimaryITNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.EstimatedEntryDateDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JourneyTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CarrierNameTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PipelineNameTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.NonAMSCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.BatchTicketTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ExpressTrackingCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ExpressTrackingNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FTZTextBox, 0);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 13, true);
			this.ExportDeclarationNumberBoundTextBox.TabIndex = 0;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.Controls.Add(this.OriginalITNTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.SEDMessageStatusDropEdit);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 532, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 50, true);
			this.DeclarationDetailsGroupBox.TabIndex = 7;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.SEDMessageStatusDropEdit, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.OriginalITNTextBox, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 13, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.StatusTextBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// ImportShipmentTypePanel
			// 
			this.ImportShipmentTypePanel.Controls.Add(this.US_ImmediateDeliveryCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.US_PGAExpeditedReleaseCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.CargoReleaseTypeDropEdit);
			this.ImportShipmentTypePanel.Controls.Add(this.ApplicationCodeDropEdit);
			this.ImportShipmentTypePanel.Controls.Add(this.US_EnableSPNCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.LiveEntryIndicatorDropEdit);
			this.ImportShipmentTypePanel.Controls.Add(this.InvoiceByRequestCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.EnableENSCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.EntryModeDropEdit);
			this.ImportShipmentTypePanel.Controls.Add(this.US_CertifyCargoReleaseCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.US_EnableAIICheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.US_EnableCRLCheckBox);
			this.ImportShipmentTypePanel.Controls.Add(this.EntryTypeDropEdit);
			this.ImportShipmentTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 87, true);
			this.ImportShipmentTypePanel.Name = "ImportShipmentTypePanel";
			this.ImportShipmentTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 178, true);
			this.ImportShipmentTypePanel.TabIndex = 3;
			// 
			// US_ImmediateDeliveryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_ImmediateDeliveryCheckBox, "US_ImmediateDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ImmediateDelivery)));
			this.US_ImmediateDeliveryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_ImmediateDeliveryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_ImmediateDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 159, true);
			this.US_ImmediateDeliveryCheckBox.Name = "US_ImmediateDeliveryCheckBox";
			this.US_ImmediateDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.US_ImmediateDeliveryCheckBox.TabIndex = 14;
			this.US_ImmediateDeliveryCheckBox.Text = "Immediate Delivery";
			this.US_ImmediateDeliveryCheckBox.UseVisualStyleBackColor = true;
			// 
			// US_PGAExpeditedReleaseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_PGAExpeditedReleaseCheckBox, "US_PGAExpeditedRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PGAExpeditedRelease)));
			this.US_PGAExpeditedReleaseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_PGAExpeditedReleaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_PGAExpeditedReleaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 124, true);
			this.US_PGAExpeditedReleaseCheckBox.Name = "US_PGAExpeditedReleaseCheckBox";
			this.US_PGAExpeditedReleaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.US_PGAExpeditedReleaseCheckBox.TabIndex = 11;
			this.US_PGAExpeditedReleaseCheckBox.Text = "PGA Expedited Release";
			this.US_PGAExpeditedReleaseCheckBox.UseVisualStyleBackColor = true;
			// 
			// CargoReleaseTypeDropEdit
			// 
			this.CargoReleaseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoReleaseTypeDropEdit, "US_CargoReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_CargoReleaseType)));
			this.CargoReleaseTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9ba769fa-a50b-4435-8b42-f33ea0bdd78c", "Type");
			this.CargoReleaseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 72, true);
			this.CargoReleaseTypeDropEdit.Name = "CargoReleaseTypeDropEdit";
			this.CargoReleaseTypeDropEdit.PreBoundMaxLength = 3;
			this.CargoReleaseTypeDropEdit.ShouldResizeByMaxLength = true;
			this.CargoReleaseTypeDropEdit.ShowDescriptionBox = false;
			this.CargoReleaseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CargoReleaseTypeDropEdit.TabIndex = 8;
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_ApplicationCode)));
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 30, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ApplicationCodeDropEdit.ShowDescriptionBox = false;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ApplicationCodeDropEdit.TabIndex = 2;
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 89, true);
			this.JE_ApplicationCodeBoundDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2267AF83-F93F-4C12-867B-82A879F31F57", "Message");
			this.JE_ApplicationCodeBoundDropEdit.ShowDescriptionBox = false;
			this.JE_ApplicationCodeBoundDropEdit.ShouldResizeByMaxLength = true;
			this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 2;
			// 
			// US_EnableSPNCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_EnableSPNCheckBox, "US_EnableSPN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EnableSPN)));
			this.US_EnableSPNCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_EnableSPNCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_EnableSPNCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 141, true);
			this.US_EnableSPNCheckBox.Name = "US_EnableSPNCheckBox";
			this.US_EnableSPNCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.US_EnableSPNCheckBox.TabIndex = 12;
			this.US_EnableSPNCheckBox.Text = "Stand Alone Prior Notice";
			this.US_EnableSPNCheckBox.UseVisualStyleBackColor = true;
			// 
			// LiveEntryIndicatorDropEdit
			// 
			this.LiveEntryIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LiveEntryIndicatorDropEdit, "US_LiveEntryIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_LiveEntryIndicator)));
			this.LiveEntryIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c6220f95-1db1-4dc7-95cc-0e4d56930a40", "Live", "Live", "");
			this.LiveEntryIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 30, true);
			this.LiveEntryIndicatorDropEdit.Name = "LiveEntryIndicatorDropEdit";
			this.LiveEntryIndicatorDropEdit.PreBoundMaxLength = 1;
			this.LiveEntryIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.LiveEntryIndicatorDropEdit.ShowDescriptionBox = false;
			this.LiveEntryIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.LiveEntryIndicatorDropEdit.TabIndex = 3;
			// 
			// InvoiceByRequestCheckBox
			// 
			this.InvoiceByRequestCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.InvoiceByRequestCheckBox, "US_IsInvoiceByRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_IsInvoiceByRequest)));
			this.InvoiceByRequestCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.InvoiceByRequestCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InvoiceByRequestCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 54, true);
			this.InvoiceByRequestCheckBox.Name = "InvoiceByRequestCheckBox";
			this.InvoiceByRequestCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.InvoiceByRequestCheckBox.TabIndex = 6;
			this.InvoiceByRequestCheckBox.Text = "Inv By Req";
			this.InvoiceByRequestCheckBox.UseVisualStyleBackColor = true;
			// 
			// EnableENSCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableENSCheckBox, "US_EnableENS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EnableENS)));
			this.EnableENSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableENSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableENSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 54, true);
			this.EnableENSCheckBox.Name = "EnableENSCheckBox";
			this.EnableENSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.EnableENSCheckBox.TabIndex = 5;
			this.EnableENSCheckBox.Text = "Enable 7501";
			// 
			// EntryModeDropEdit
			// 
			this.EntryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryModeDropEdit, "US_EntryMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryMode)));
			this.EntryModeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("da0954ae-8a6e-4173-a24f-80796e240cca", "RLF", "Remote Location Filing");
			this.EntryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 4, true);
			this.EntryModeDropEdit.Name = "EntryModeDropEdit";
			this.EntryModeDropEdit.PreBoundMaxLength = 3;
			this.EntryModeDropEdit.ShouldResizeByMaxLength = true;
			this.EntryModeDropEdit.ShowDescriptionBox = false;
			this.EntryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.EntryModeDropEdit.TabIndex = 1;
			// 
			// US_CertifyCargoReleaseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_CertifyCargoReleaseCheckBox, "US_CertifyCargoRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_CertifyCargoRelease)));
			this.US_CertifyCargoReleaseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_CertifyCargoReleaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_CertifyCargoReleaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 107, true);
			this.US_CertifyCargoReleaseCheckBox.Name = "US_CertifyCargoReleaseCheckBox";
			this.US_CertifyCargoReleaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.US_CertifyCargoReleaseCheckBox.TabIndex = 10;
			this.US_CertifyCargoReleaseCheckBox.Text = "Certify Cargo Rel from Sum";
			this.US_CertifyCargoReleaseCheckBox.UseVisualStyleBackColor = true;
			// 
			// US_EnableAIICheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_EnableAIICheckBox, "US_EnableAII");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EnableAII)));
			this.US_EnableAIICheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_EnableAIICheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_EnableAIICheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 90, true);
			this.US_EnableAIICheckBox.Name = "US_EnableAIICheckBox";
			this.US_EnableAIICheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.US_EnableAIICheckBox.TabIndex = 9;
			this.US_EnableAIICheckBox.Text = "Enable AII";
			// 
			// US_EnableCRLCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_EnableCRLCheckBox, "US_EnableCRL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EnableCRL)));
			this.US_EnableCRLCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_EnableCRLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_EnableCRLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 72, true);
			this.US_EnableCRLCheckBox.Name = "US_EnableCRLCheckBox";
			this.US_EnableCRLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.US_EnableCRLCheckBox.TabIndex = 7;
			this.US_EnableCRLCheckBox.Text = "Enable 3461";
			// 
			// EntryTypeDropEdit
			// 
			this.EntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "US_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_EntryTypeList)));
			this.EntryTypeDropEdit.BindToList = "AddInfoLookups+US_EntryTypeList";
			this.EntryTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("de477514-f21d-4503-9af6-66dc414de498", "Entry", "Entry Type", "");
			this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 4, true);
			this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.EntryTypeDropEdit.PreBoundMaxLength = 2;
			this.EntryTypeDropEdit.ShouldResizeByMaxLength = true;
			this.EntryTypeDropEdit.ShowDescriptionBox = false;
			this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.EntryTypeDropEdit.TabIndex = 0;
			// 
			// US_MonthlyFilingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_MonthlyFilingCheckBox, "US_MonthlyFiling");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_MonthlyFiling)));
			this.US_MonthlyFilingCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("937928c8-1b4e-4f42-bbca-83f5029e455c", "Monthly Filing");
			this.US_MonthlyFilingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_MonthlyFilingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_MonthlyFilingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 44, true);
			this.US_MonthlyFilingCheckBox.Name = "US_MonthlyFilingCheckBox";
			this.US_MonthlyFilingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.US_MonthlyFilingCheckBox.TabIndex = 2;
			this.US_MonthlyFilingCheckBox.Text = "Monthly Filing";
			this.US_MonthlyFilingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.US_MonthlyFilingCheckBox.UseVisualStyleBackColor = true;
			// 
			// SEDGroupBox
			// 
			this.SEDGroupBox.Controls.Add(this.US_RN_NKCountryOfDestinationCodeFindBox);
			this.SEDGroupBox.Controls.Add(this.TransportReferenceTextBox);
			this.SEDGroupBox.Controls.Add(this.ForeignTradeZoneTextBox);
			this.SEDGroupBox.Controls.Add(this.StateOfOriginDropEdit);
			this.SEDGroupBox.Controls.Add(this.ExportCodeDropEdit);
			this.SEDGroupBox.Controls.Add(this.LicenseNoTextBox);
			this.SEDGroupBox.Controls.Add(this.ECCNCodeFindBox);
			this.SEDGroupBox.Controls.Add(this.ECCNTextBox);
			this.SEDGroupBox.Controls.Add(this.InbondTypeDropEdit);
			this.SEDGroupBox.Controls.Add(this.ImportEntryNoTextBox);
			this.SEDGroupBox.Controls.Add(this.LicenseTypeCodeFindBox);
			this.SEDGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 401, true);
			this.SEDGroupBox.Name = "SEDGroupBox";
			this.SEDGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 127, true);
			this.SEDGroupBox.TabIndex = 5;
			this.SEDGroupBox.TabStop = false;
			this.SEDGroupBox.Text = "SED Details";
			// 
			// US_RN_NKCountryOfDestinationCodeFindBox
			// 
			this.US_RN_NKCountryOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_RN_NKCountryOfDestinationCodeFindBox, "US_RN_NKCountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_RN_NKCountryOfDestination)));
			this.US_RN_NKCountryOfDestinationCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eea73a16-49ac-49c8-ae8e-597ccde7bc5d", "Ctry/Rgn. Dest.", "Ctry/Rgn. Dest.", "Country/Region of Dest.", "The ultimate country or region of destination of the goods.");
			this.US_RN_NKCountryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 13, true);
			this.US_RN_NKCountryOfDestinationCodeFindBox.Name = "US_RN_NKCountryOfDestinationCodeFindBox";
			this.US_RN_NKCountryOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.US_RN_NKCountryOfDestinationCodeFindBox.ParentType = null;
			this.US_RN_NKCountryOfDestinationCodeFindBox.PreBoundMaxLength = 2;
			this.US_RN_NKCountryOfDestinationCodeFindBox.ShowDescriptionBox = false;
			this.US_RN_NKCountryOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.US_RN_NKCountryOfDestinationCodeFindBox.TabIndex = 1;
			// 
			// TransportReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportReferenceTextBox, "US_TransportReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TransportReference)));
			this.TransportReferenceTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("63652326-1abc-41ac-8732-f3e2cfeb4507", "Transport Ref.");
			this.TransportReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 13, true);
			this.TransportReferenceTextBox.Name = "TransportReferenceTextBox";
			this.TransportReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TransportReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.TransportReferenceTextBox.TabIndex = 0;
			// 
			// ForeignTradeZoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignTradeZoneTextBox, "US_ForeignTradeZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ForeignTradeZone)));
			this.ForeignTradeZoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b48ed19c-f061-42b0-9283-5326b9b1a043", "FT Zone");
			this.ForeignTradeZoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 35, true);
			this.ForeignTradeZoneTextBox.Name = "ForeignTradeZoneTextBox";
			this.ForeignTradeZoneTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ForeignTradeZoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.ForeignTradeZoneTextBox.TabIndex = 2;
			// 
			// StateOfOriginDropEdit
			// 
			this.StateOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateOfOriginDropEdit, "US_StateOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_StateOfOrigin)));
			this.StateOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("094ddc1f-9cb4-4ec1-9d41-1f499d3d5c4f", "State Of Origin");
			this.StateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 35, true);
			this.StateOfOriginDropEdit.Name = "StateOfOriginDropEdit";
			this.StateOfOriginDropEdit.PreBoundMaxLength = 2;
			this.StateOfOriginDropEdit.ShouldResizeByMaxLength = true;
			this.StateOfOriginDropEdit.ShowDescriptionBox = false;
			this.StateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.StateOfOriginDropEdit.TabIndex = 3;
			// 
			// ExportCodeDropEdit
			// 
			this.ExportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportCodeDropEdit, "US_ExportCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ExportCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_ExportCode_List)));
			this.ExportCodeDropEdit.BindToList = "AddInfoLookups+US_ExportCode_List";
			this.ExportCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8efc4ac5-7c27-4941-8b37-ce729bf9bd55", "Export Code");
			this.ExportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 101, true);
			this.ExportCodeDropEdit.Name = "ExportCodeDropEdit";
			this.ExportCodeDropEdit.PreBoundMaxLength = 3;
			this.ExportCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ExportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.ExportCodeDropEdit.TabIndex = 8;
			// 
			// LicenseNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LicenseNoTextBox, "US_LicenseNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_LicenseNo)));
			this.LicenseNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8ec0bcca-54f9-43c7-926e-a672fcaf6faf", "License No.");
			this.LicenseNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 79, true);
			this.LicenseNoTextBox.Name = "LicenseNoTextBox";
			this.LicenseNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LicenseNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.LicenseNoTextBox.TabIndex = 7;
			// 
			// ECCNCodeFindBox
			//
			this.ECCNCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ECCNCodeFindBox, "US_ECCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ECCN)));
			this.ECCNCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("96E9085F-F099-44CD-B11A-792262F88F30", "ECCN");
			this.ECCNCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 101, true);
			this.ECCNCodeFindBox.Name = "ECCNCodeFindBox";
			this.ECCNCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ECCNCodeFindBox.ParentType = null;
			this.ECCNCodeFindBox.PopupCaption = null;
			this.ECCNCodeFindBox.PreBoundMaxLength = 5;
			this.ECCNCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ECCNCodeFindBox.TabIndex = 9;
			// 
			// ECCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.ECCNTextBox, "US_ECCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ECCN)));
			this.ECCNTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a54b537e-296d-4b42-94ae-09fb952e45fd", "ECCN");
			this.ECCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 101, true);
			this.ECCNTextBox.Name = "ECCNTextBox";
			this.ECCNTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ECCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ECCNTextBox.TabIndex = 9;
			// 
			// InbondTypeDropEdit
			// 
			this.InbondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InbondTypeDropEdit, "US_InbondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_InbondType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_InbondType_List)));
			this.InbondTypeDropEdit.BindToList = "AddInfoLookups+US_InbondType_List";
			this.InbondTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9eebe5c1-a39e-4bf5-80ac-c747fd260de0", "In Bond Type");
			this.InbondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 57, true);
			this.InbondTypeDropEdit.Name = "InbondTypeDropEdit";
			this.InbondTypeDropEdit.PreBoundMaxLength = 2;
			this.InbondTypeDropEdit.ShouldResizeByMaxLength = true;
			this.InbondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.InbondTypeDropEdit.TabIndex = 4;
			// 
			// ImportEntryNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportEntryNoTextBox, "US_ImportEntryNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ImportEntryNo)));
			this.ImportEntryNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("50835d6d-2b78-4714-8547-2565800103bf", "Imp. Entry No.");
			this.ImportEntryNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 57, true);
			this.ImportEntryNoTextBox.Name = "ImportEntryNoTextBox";
			this.ImportEntryNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ImportEntryNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ImportEntryNoTextBox.TabIndex = 5;
			// 
			// LicenseTypeCodeFindBox
			// 
			this.LicenseTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenseTypeCodeFindBox, "US_LicenseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_LicenseType)));
			this.LicenseTypeCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("57da8bd9-149e-4b78-819d-4c17fcfa8c83", "License Type");
			this.LicenseTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 79, true);
			this.LicenseTypeCodeFindBox.Name = "LicenseTypeCodeFindBox";
			this.LicenseTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LicenseTypeCodeFindBox.ParentType = null;
			this.LicenseTypeCodeFindBox.PreBoundMaxLength = 3;
			this.LicenseTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.LicenseTypeCodeFindBox.TabIndex = 6;
			// 
			// PortOfExportPanel
			// 
			this.PortOfExportPanel.Controls.Add(this.PortOfExportSchDCodeFindBox);
			this.PortOfExportPanel.Controls.Add(this.PortOfExportSchDCodeDropEdit);
			this.PortOfExportPanel.Controls.Add(this.DateOfExportDateEdit);
			this.PortOfExportPanel.Controls.Add(this.PortOfExportCodeFindBox);
			this.PortOfExportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 136, true);
			this.PortOfExportPanel.Name = "PortOfExportPanel";
			this.PortOfExportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 25, true);
			this.PortOfExportPanel.TabIndex = 15;
			// 
			// PortOfExportSchDCodeFindBox
			// 
			this.PortOfExportSchDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfExportSchDCodeFindBox, "US_SchDExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.SchDExportList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDExportDescription)));
			this.PortOfExportSchDCodeFindBox.BindToForDescription = "US_SchDExportDescription";
			this.PortOfExportSchDCodeFindBox.BindToList = "AddInfoLookups+SchDExportList";
			this.PortOfExportSchDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c2fb1269-7b55-400b-b1d9-8058189cb655", "Export");
			this.PortOfExportSchDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 1, true);
			this.PortOfExportSchDCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PortOfExportSchDCodeFindBox.Name = "PortOfExportSchDCodeFindBox";
			this.PortOfExportSchDCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfExportSchDCodeFindBox.ParentType = null;
			this.PortOfExportSchDCodeFindBox.PopupCaption = null;
			this.PortOfExportSchDCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfExportSchDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfExportSchDCodeFindBox.TabIndex = 0;
			// 
			// PortOfExportSchDCodeDropEdit
			// 
			this.PortOfExportSchDCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfExportSchDCodeDropEdit, "US_SchDExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.SchDExportList)));
			this.PortOfExportSchDCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c2fb1269-7b55-400b-b1d9-8058189cb655", "Export");
			this.PortOfExportSchDCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 1, true);
			this.PortOfExportSchDCodeDropEdit.Name = "PortOfExportSchDCodeDropEdit";
			this.PortOfExportSchDCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfExportSchDCodeDropEdit.TabIndex = 0;
			// 
			// DateOfExportDateEdit
			// 
			this.DateOfExportDateEdit.AllowDrop = true;
			this.DateOfExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfExportDateEdit, "US_DateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DateOfExport)));
			this.DateOfExportDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1083cf74-d876-435e-b375-4baeb671f110", "Exp.", "Date Of Export", "Date of export is the date the merchandise is scheduled to leave the United States for all methods of transportation.");
			this.DateOfExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 1, true);
			this.DateOfExportDateEdit.Name = "DateOfExportDateEdit";
			this.DateOfExportDateEdit.TabIndex = 1;
			// 
			// PortOfExportCodeFindBox
			// 
			this.PortOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfExportCodeFindBox, "US_RL_NKPortOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_RL_NKPortOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.PortOfLoadings)));
			this.PortOfExportCodeFindBox.BindToList = "Lookups+PortOfLoadings";
			this.PortOfExportCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2f7bc386-4c05-4fef-970d-abe3d7f72e66", "Port of Export", "The port of export is the U.S. Customs port at which the merchandise is loaded on the carrier that is taking the merchandise out of the United States.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PortOfExportCodeFindBox, false);
			this.PortOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 1, true);
			this.PortOfExportCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfExportCodeFindBox.Name = "PortOfExportCodeFindBox";
			this.PortOfExportCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfExportCodeFindBox.ParentType = null;
			this.PortOfExportCodeFindBox.PopupCaption = null;
			this.PortOfExportCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfExportCodeFindBox.ShowDescriptionBox = false;
			this.PortOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfExportCodeFindBox.TabIndex = 2;
			// 
			// PortOfLoadingSchDFindBox
			// 
			this.PortOfLoadingSchDFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingSchDFindBox, "US_SchDLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.LoadingSchDList)));
			this.PortOfLoadingSchDFindBox.BindToList = "AddInfoLookups+LoadingSchDList";
			this.PortOfLoadingSchDFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("720e9e45-f6b6-4256-b405-db35432b71b4", "Loading");
			this.PortOfLoadingSchDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 91, true);
			this.PortOfLoadingSchDFindBox.Name = "PortOfLoadingSchDFindBox";
			this.PortOfLoadingSchDFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingSchDFindBox.ParentType = null;
			this.PortOfLoadingSchDFindBox.PopupCaption = null;
			this.PortOfLoadingSchDFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingSchDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfLoadingSchDFindBox.TabIndex = 10;
			// 
			// PortOfLoadingSchDDropEdit
			// 
			this.PortOfLoadingSchDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingSchDDropEdit, "US_SchDLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.LoadingSchDList)));
			this.PortOfLoadingSchDDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("720e9e45-f6b6-4256-b405-db35432b71b4", "Loading");
			this.PortOfLoadingSchDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 91, true);
			this.PortOfLoadingSchDDropEdit.Name = "PortOfLoadingSchDDropEdit";
			this.PortOfLoadingSchDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfLoadingSchDDropEdit.TabIndex = 10;
			// 
			// PortOfDischargeSchDFindBox
			// 
			this.PortOfDischargeSchDFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeSchDFindBox, "US_SchDArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDArrival)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.DischargeSchDList)));
			this.PortOfDischargeSchDFindBox.BindToList = "AddInfoLookups+DischargeSchDList";
			this.PortOfDischargeSchDFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5c29069f-38f2-4cab-9a69-ab42bec3f36d", "Discharge");
			this.PortOfDischargeSchDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 115, true);
			this.PortOfDischargeSchDFindBox.Name = "PortOfDischargeSchDFindBox";
			this.PortOfDischargeSchDFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeSchDFindBox.ParentType = null;
			this.PortOfDischargeSchDFindBox.PopupCaption = null;
			this.PortOfDischargeSchDFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeSchDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfDischargeSchDFindBox.TabIndex = 12;
			// 
			// PortOfDischargeSchDDropEdit
			// 
			this.PortOfDischargeSchDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeSchDDropEdit, "US_SchDArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDArrival)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.DischargeSchDList)));
			this.PortOfDischargeSchDDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5c29069f-38f2-4cab-9a69-ab42bec3f36d", "Discharge");
			this.PortOfDischargeSchDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 115, true);
			this.PortOfDischargeSchDDropEdit.Name = "PortOfDischargeSchDDropEdit";
			this.PortOfDischargeSchDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfDischargeSchDDropEdit.TabIndex = 12;
			// 
			// ConsigneeOrganisationControl
			// 
			this.ConsigneeOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeOrganisationControl, "JE_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.ImportersList)));
			this.ConsigneeOrganisationControl.BindToList = "Lookups+ImportersList";
			this.ConsigneeOrganisationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("09c5887a-a9ed-4b5e-b085-7719acb78293", "Intermediate Consignee");
			this.ConsigneeOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 2, true);
			this.ConsigneeOrganisationControl.Name = "ConsigneeOrganisationControl";
			this.ConsigneeOrganisationControl.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsigneeOrganisationControl.ParentType = null;
			this.ConsigneeOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ConsigneeOrganisationControl.TabIndex = 0;
			// 
			// FPPIGuidFindBox
			// 
			this.FPPIGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FPPIGuidFindBox, "USD_OH_ForeignPrincipalParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).USD_OH_ForeignPrincipalParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).USDeclaration.Lookups.ForeignPrincipalParties)));
			this.FPPIGuidFindBox.BindToList = "USDeclaration+Lookups+ForeignPrincipalParties";
			this.FPPIGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8DEC3AEF-45BB-4CDD-BDAE-683428C762D6", "FPPI");
			this.FPPIGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.FPPIGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 222, true);
			this.FPPIGuidFindBox.Name = "FPPIGuidFindBox";
			this.FPPIGuidFindBox.ShouldResize = true;
			this.FPPIGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.FPPIGuidFindBox.TabIndex = 1;
			// 
			// PortOfImportPanel
			// 
			this.PortOfImportPanel.Controls.Add(this.HMFPanel);
			this.PortOfImportPanel.Controls.Add(this.JE_DateOfFirstArrivalDateEdit);
			this.PortOfImportPanel.Controls.Add(this.PortOfEntryScheduleCodeFindBox);
			this.PortOfImportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 136, true);
			this.PortOfImportPanel.Name = "PortOfImportPanel";
			this.PortOfImportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 25, true);
			this.PortOfImportPanel.TabIndex = 15;
			// 
			// HMFPanel
			// 
			this.HMFPanel.Controls.Add(this.HMFApplicableZDropEdit);
			this.HMFPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 0, true);
			this.HMFPanel.Name = "HMFPanel";
			this.HMFPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.HMFPanel.TabIndex = 2;
			// 
			// HMFApplicableZDropEdit
			// 
			this.HMFApplicableZDropEdit.AllowDrop = true;
			this.HMFApplicableZDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HMFApplicableZDropEdit, "US_IsHMFApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_IsHMFApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_YesNoList)));
			this.HMFApplicableZDropEdit.BindToList = "AddInfoLookups.US_YesNoList";
			this.HMFApplicableZDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6a51d353-e24c-4619-a0a1-97c8de936b3a", "HMF?");
			this.HMFApplicableZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 1, true);
			this.HMFApplicableZDropEdit.Name = "HMFApplicableZDropEdit";
			this.HMFApplicableZDropEdit.PreBoundMaxLength = 1;
			this.HMFApplicableZDropEdit.ShouldResizeByMaxLength = true;
			this.HMFApplicableZDropEdit.ShowDescriptionBox = false;
			this.HMFApplicableZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.HMFApplicableZDropEdit.TabIndex = 0;
			// 
			// JE_DateOfFirstArrivalDateEdit
			// 
			this.JE_DateOfFirstArrivalDateEdit.AllowDrop = true;
			this.JE_DateOfFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_DateOfFirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_DateOfFirstArrivalDateEdit, "US_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryDate)));
			this.JE_DateOfFirstArrivalDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7e8b973c-37aa-448d-9b9e-05c14ca36a1b", "Arr.");
			this.JE_DateOfFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 3, true);
			this.JE_DateOfFirstArrivalDateEdit.Name = "JE_DateOfFirstArrivalDateEdit";
			this.JE_DateOfFirstArrivalDateEdit.TabIndex = 1;
			// 
			// PortOfEntryScheduleCodeFindBox
			// 
			this.PortOfEntryScheduleCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfEntryScheduleCodeFindBox, "US_SchDEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SchDEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.RegionDistrictPorts)));
			this.PortOfEntryScheduleCodeFindBox.BindToList = "AddInfoLookups+RegionDistrictPorts";
			this.PortOfEntryScheduleCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8e6f7d4a-8688-41bd-9f2d-4f1a61dea471", "Entry Port");
			this.PortOfEntryScheduleCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 3, true);
			this.PortOfEntryScheduleCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PortOfEntryScheduleCodeFindBox.Name = "PortOfEntryScheduleCodeFindBox";
			this.PortOfEntryScheduleCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfEntryScheduleCodeFindBox.ParentType = null;
			this.PortOfEntryScheduleCodeFindBox.PopupCaption = null;
			this.PortOfEntryScheduleCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfEntryScheduleCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PortOfEntryScheduleCodeFindBox.TabIndex = 0;
			// 
			// DDTCDetailsTabPage
			// 
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCUSMLCategoryCodeDropEdit);
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCPartyCertificationIndicatorDropEdit);
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCMilitaryEquipmentIndicatorDropEdit);
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCRegistrationNumberTextBox);
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCITARExemptionNumberDropEdit);
			this.DDTCDetailsTabPage.Controls.Add(this.DDTCCategoryXXIDeterminationNumberTextBox);
			this.DDTCDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DDTCDetailsTabPage.Name = "DDTCDetailsTabPage";
			this.DDTCDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.DDTCDetailsTabPage.TabIndex = 8;
			this.DDTCDetailsTabPage.Text = "DDTC Details";
			// 
			// DDTCCategoryXXIDeterminationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DDTCCategoryXXIDeterminationNumberTextBox, "US_JurisdictionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_JurisdictionNumber)));
			this.DDTCCategoryXXIDeterminationNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2230D2BC-4F80-41C1-9477-AB0C2CDE8A0F", "Cat. XXI Determ. No.", "Cat. XXI Determination No.", "Category XXI Determination Number", "");
			this.DDTCCategoryXXIDeterminationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 142, true);
			this.DDTCCategoryXXIDeterminationNumberTextBox.Name = "DDTCCategoryXXIDeterminationNumberTextBox";
			this.DDTCCategoryXXIDeterminationNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DDTCCategoryXXIDeterminationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.DDTCCategoryXXIDeterminationNumberTextBox.TabIndex = 10;
			// 
			// DDTCUSMLCategoryCodeDropEdit
			// 
			this.DDTCUSMLCategoryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTCUSMLCategoryCodeDropEdit, "US_DDTCUSMLCategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DDTCUSMLCategoryCode)));
			this.DDTCUSMLCategoryCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d8bc861f-4ecb-4a45-9801-9b3ff5500b59", "USML Category Code");
			this.DDTCUSMLCategoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 114, true);
			this.DDTCUSMLCategoryCodeDropEdit.Name = "DDTCUSMLCategoryCodeDropEdit";
			this.DDTCUSMLCategoryCodeDropEdit.PreBoundMaxLength = 2;
			this.DDTCUSMLCategoryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.DDTCUSMLCategoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.DDTCUSMLCategoryCodeDropEdit.TabIndex = 9;
			// 
			// DDTCPartyCertificationIndicatorDropEdit
			// 
			this.DDTCPartyCertificationIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTCPartyCertificationIndicatorDropEdit, "US_DDTCPartyCertificationIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DDTCPartyCertificationIndicator)));
			this.DDTCPartyCertificationIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f1773b4f-330a-40f5-abc1-470f1ee45772", "Party Certification Ind.");
			this.DDTCPartyCertificationIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 86, true);
			this.DDTCPartyCertificationIndicatorDropEdit.Name = "DDTCPartyCertificationIndicatorDropEdit";
			this.DDTCPartyCertificationIndicatorDropEdit.PreBoundMaxLength = 1;
			this.DDTCPartyCertificationIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.DDTCPartyCertificationIndicatorDropEdit.ShowDescriptionBox = false;
			this.DDTCPartyCertificationIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.DDTCPartyCertificationIndicatorDropEdit.TabIndex = 7;
			// 
			// DDTCMilitaryEquipmentIndicatorDropEdit
			// 
			this.DDTCMilitaryEquipmentIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTCMilitaryEquipmentIndicatorDropEdit, "US_DDTCMilitaryEquipmentIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DDTCMilitaryEquipmentIndicator)));
			this.DDTCMilitaryEquipmentIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e291224d-6526-40ef-ad19-c718705d3348", "Military Equip. Ind.");
			this.DDTCMilitaryEquipmentIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 60, true);
			this.DDTCMilitaryEquipmentIndicatorDropEdit.Name = "DDTCMilitaryEquipmentIndicatorDropEdit";
			this.DDTCMilitaryEquipmentIndicatorDropEdit.PreBoundMaxLength = 1;
			this.DDTCMilitaryEquipmentIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.DDTCMilitaryEquipmentIndicatorDropEdit.ShowDescriptionBox = false;
			this.DDTCMilitaryEquipmentIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.DDTCMilitaryEquipmentIndicatorDropEdit.TabIndex = 5;
			// 
			// DDTCRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DDTCRegistrationNumberTextBox, "US_DDTCRegistrationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DDTCRegistrationNo)));
			this.DDTCRegistrationNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("39afdef1-901a-4d65-b2e6-2b9f6b925703", "Registration Number");
			this.DDTCRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 34, true);
			this.DDTCRegistrationNumberTextBox.Name = "DDTCRegistrationNumberTextBox";
			this.DDTCRegistrationNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DDTCRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.DDTCRegistrationNumberTextBox.TabIndex = 3;
			// 
			// DDTCITARExemptionNumberDropEdit
			// 
			this.DDTCITARExemptionNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTCITARExemptionNumberDropEdit, "US_DDTCITARExemptionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DDTCITARExemptionNo)));
			this.DDTCITARExemptionNumberDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("80fbc2f6-b5b8-498a-a80e-869632fdb714", "ITAR Exemption Number");
			this.DDTCITARExemptionNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 8, true);
			this.DDTCITARExemptionNumberDropEdit.Name = "DDTCITARExemptionNumberDropEdit";
			this.DDTCITARExemptionNumberDropEdit.PreBoundMaxLength = 12;
			this.DDTCITARExemptionNumberDropEdit.ShouldResizeByMaxLength = true;
			this.DDTCITARExemptionNumberDropEdit.ShowDescriptionBox = false;
			this.DDTCITARExemptionNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.DDTCITARExemptionNumberDropEdit.TabIndex = 1;
			// 
			// ImportStatusGroupBox
			// 
			this.ImportStatusGroupBox.Controls.Add(this.SEReleaseAndStandAlonePriorNoticePanel);
			this.ImportStatusGroupBox.Controls.Add(this.FTZPanel);
			this.ImportStatusGroupBox.Controls.Add(this.SEReleasePanel);
			this.ImportStatusGroupBox.Controls.Add(this.AllocateImportEntryNumberButton);
			this.ImportStatusGroupBox.Controls.Add(this.AccLiqCheckBox);
			this.ImportStatusGroupBox.Controls.Add(this.EntryNumberDividerLabel);
			this.ImportStatusGroupBox.Controls.Add(this.PSCCheckBox);
			this.ImportStatusGroupBox.Controls.Add(this.WarehouseWithdrawalPanel);
			this.ImportStatusGroupBox.Controls.Add(this.TIBPanel);
			this.ImportStatusGroupBox.Controls.Add(this.IORAuthAgentPanel);
			this.ImportStatusGroupBox.Controls.Add(this.PurchasedDropEdit);
			this.ImportStatusGroupBox.Controls.Add(this.ManualEntryCheckBox);
			this.ImportStatusGroupBox.Controls.Add(this.Box29Button);
			this.ImportStatusGroupBox.Controls.Add(this.LocationOfGoodsCodeFindBox);
			this.ImportStatusGroupBox.Controls.Add(this.CentralizedExamSiteFindBox);
			this.ImportStatusGroupBox.Controls.Add(this.ImportEntryNumberTextBox);
			this.ImportStatusGroupBox.Controls.Add(this.EntryFilerCodeTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImportStatusGroupBox, false);
			this.ImportStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 418, true);
			this.ImportStatusGroupBox.Name = "ImportStatusGroupBox";
			this.ImportStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 172, true);
			this.ImportStatusGroupBox.TabIndex = 5;
			this.ImportStatusGroupBox.TabStop = false;
			// 
			// SEReleaseAndStandAlonePriorNoticePanel
			// 
			this.SEReleaseAndStandAlonePriorNoticePanel.Controls.Add(this.SPNIDTypeDropEdit);
			this.SEReleaseAndStandAlonePriorNoticePanel.Controls.Add(this.SplitShipmentReleaseCodeForSPNDropEdit);
			this.SEReleaseAndStandAlonePriorNoticePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 108, true);
			this.SEReleaseAndStandAlonePriorNoticePanel.Name = "SEReleaseAndStandAlonePriorNoticePanel";
			this.SEReleaseAndStandAlonePriorNoticePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 20, true);
			this.SEReleaseAndStandAlonePriorNoticePanel.TabIndex = 0;
			// 
			// SPNIDTypeDropEdit
			// 
			this.SPNIDTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SPNIDTypeDropEdit, "US_SPNIDType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SPNIDType)));
			this.SPNIDTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("684a386d-0dec-4cfd-b92d-02cdc15fc9b1", "Stand Alone Prior Notice ID Type");
			this.SPNIDTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 0, true);
			this.SPNIDTypeDropEdit.Name = "SPNIDTypeDropEdit";
			this.SPNIDTypeDropEdit.ShouldResizeByMaxLength = true;
			this.SPNIDTypeDropEdit.ShowDescriptionBox = false;
			this.SPNIDTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SPNIDTypeDropEdit.TabIndex = 0;
			// 
			// SplitShipmentReleaseCodeForSPNDropEdit
			// 
			this.SplitShipmentReleaseCodeForSPNDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SplitShipmentReleaseCodeForSPNDropEdit, "US_SESplitRel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SESplitRel)));
			this.SplitShipmentReleaseCodeForSPNDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e05eb523-2216-4cbe-b94b-f53826a1da19", "Split Shpt Release");
			this.SplitShipmentReleaseCodeForSPNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 0, true);
			this.SplitShipmentReleaseCodeForSPNDropEdit.Name = "SplitShipmentReleaseCodeForSPNDropEdit";
			this.SplitShipmentReleaseCodeForSPNDropEdit.ShouldResizeByMaxLength = true;
			this.SplitShipmentReleaseCodeForSPNDropEdit.ShowDescriptionBox = false;
			this.SplitShipmentReleaseCodeForSPNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SplitShipmentReleaseCodeForSPNDropEdit.TabIndex = 1;
			// 
			// FTZPanel
			// 
			this.FTZPanel.Controls.Add(this.FTZSPNIDTypeDropEdit);
			this.FTZPanel.Controls.Add(this.AllocateButton);
			this.FTZPanel.Controls.Add(this.ZoneIDDropDownEdit);
			this.FTZPanel.Controls.Add(this.FTZControlNumberTextBox);
			this.FTZPanel.Controls.Add(this.FTZYearTextBox);
			this.FTZPanel.Controls.Add(this.DeliveryDropEdit);
			this.FTZPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 30, true);
			this.FTZPanel.Name = "FTZPanel";
			this.FTZPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 95, true);
			this.FTZPanel.TabIndex = 1;
			// 
			// FTZSPNIDTypeDropEdit
			// 
			this.FTZSPNIDTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FTZSPNIDTypeDropEdit, "US_SPNIDType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SPNIDType)));
			this.FTZSPNIDTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("885ba73e-479e-410c-8fc2-00566b692a61", "Stand Alone Prior Notice ID Type");
			this.FTZSPNIDTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 42, true);
			this.FTZSPNIDTypeDropEdit.Name = "FTZSPNIDTypeDropEdit";
			this.FTZSPNIDTypeDropEdit.ShouldResizeByMaxLength = true;
			this.FTZSPNIDTypeDropEdit.ShowDescriptionBox = false;
			this.FTZSPNIDTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.FTZSPNIDTypeDropEdit.TabIndex = 11;
			// 
			// AllocateButton
			// 
			this.AllocateButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d107d342-5a17-40e2-9ac5-84167cbf3275", "Allocate");
			this.AllocateButton.IsCaptionOverridden = false;
			this.AllocateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 10, true);
			this.AllocateButton.Name = "AllocateButton";
			this.AllocateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 21, true);
			this.AllocateButton.TabIndex = 12;
			this.AllocateButton.Tag = "10";
			this.AllocateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AllocateButton.ToolTipCaption = null;
			this.AllocateButton.UseVisualStyleBackColor = true;
			this.AllocateButton.Click += new System.EventHandler(this.AllocateButton_Click);
			// 
			// ZoneIDDropDownEdit
			// 
			this.ZoneIDDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZoneIDDropDownEdit, "FTZZoneID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FTZZoneID)));
			this.ZoneIDDropDownEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("834e0a14-d473-479b-89cc-34cfdc95a04d", "Zone ID");
			this.ZoneIDDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 11, true);
			this.ZoneIDDropDownEdit.Name = "ZoneIDDropDownEdit";
			this.ZoneIDDropDownEdit.PreBoundMaxLength = 7;
			this.ZoneIDDropDownEdit.ShouldResizeByMaxLength = true;
			this.ZoneIDDropDownEdit.ShowDescriptionBox = false;
			this.ZoneIDDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ZoneIDDropDownEdit.TabIndex = 7;
			// 
			// FTZControlNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTZControlNumberTextBox, "FTZControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FTZControlNumber)));
			this.FTZControlNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b955bb77-1b99-47f4-b8d7-ee1c2454bd21", "No.");
			this.FTZControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 11, true);
			this.FTZControlNumberTextBox.Name = "FTZControlNumberTextBox";
			this.FTZControlNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.FTZControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.FTZControlNumberTextBox.TabIndex = 9;
			// 
			// FTZYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTZYearTextBox, "FTZYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FTZYear)));
			this.FTZYearTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5131915f-cf3e-4dce-8666-79f43456a24a", "Year");
			this.FTZYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 11, true);
			this.FTZYearTextBox.Name = "FTZYearTextBox";
			this.FTZYearTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.FTZYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.FTZYearTextBox.TabIndex = 8;
			// 
			// DeliveryDropEdit
			// 
			this.DeliveryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDropEdit, "US_F_DeliveryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_F_DeliveryCode)));
			this.DeliveryDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a2e8f762-cbfd-4dff-a17d-b596ffdccfb1", "Delivery Code");
			this.DeliveryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 42, true);
			this.DeliveryDropEdit.Name = "DeliveryDropEdit";
			this.DeliveryDropEdit.PreBoundMaxLength = 1;
			this.DeliveryDropEdit.ShouldResizeByMaxLength = true;
			this.DeliveryDropEdit.ShowDescriptionBox = false;
			this.DeliveryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.DeliveryDropEdit.TabIndex = 10;
			this.DeliveryDropEdit.Tag = "11";
			// 
			// SEReleasePanel
			// 
			this.SEReleasePanel.Controls.Add(this.SplitShipmentReleaseCodeDropEdit);
			this.SEReleasePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 108, true);
			this.SEReleasePanel.Name = "SEReleasePanel";
			this.SEReleasePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 20, true);
			this.SEReleasePanel.TabIndex = 16;
			// 
			// SplitShipmentReleaseCodeDropEdit
			// 
			this.SplitShipmentReleaseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SplitShipmentReleaseCodeDropEdit, "US_SESplitRel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SESplitRel)));
			this.SplitShipmentReleaseCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4f603fb9-2a2b-41e5-bb74-df1ceac093d8", "Split Shpt Release");
			this.SplitShipmentReleaseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 0, true);
			this.SplitShipmentReleaseCodeDropEdit.Name = "SplitShipmentReleaseCodeDropEdit";
			this.SplitShipmentReleaseCodeDropEdit.ShouldResizeByMaxLength = true;
			this.SplitShipmentReleaseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.SplitShipmentReleaseCodeDropEdit.TabIndex = 11;
			// 
			// AllocateImportEntryNumberButton
			// 
			this.AllocateImportEntryNumberButton.IsCaptionOverridden = true;
			this.AllocateImportEntryNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 55, true);
			this.AllocateImportEntryNumberButton.Name = "AllocateImportEntryNumberButton";
			this.AllocateImportEntryNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateImportEntryNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AllocateImportEntryNumberButton.TabIndex = 6;
			this.AllocateImportEntryNumberButton.Text = "Allocate";
			this.AllocateImportEntryNumberButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AllocateImportEntryNumberButton.ToolTipCaption = null;
			this.AllocateImportEntryNumberButton.UseVisualStyleBackColor = true;
			this.AllocateImportEntryNumberButton.Click += new System.EventHandler(this.AllocateImportEntryNumberButton_Click);
			// 
			// AccLiqCheckBox
			// 
			this.AccLiqCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AccLiqCheckBox, "US_AccLiqReq");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_AccLiqReq)));
			this.AccLiqCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ed3c936b-8a47-490d-87b6-92dc6528be62", "Accelerated. Liq.:");
			this.AccLiqCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AccLiqCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AccLiqCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 59, true);
			this.AccLiqCheckBox.Name = "AccLiqCheckBox";
			this.AccLiqCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AccLiqCheckBox.TabIndex = 6;
			this.AccLiqCheckBox.UseVisualStyleBackColor = true;
			// 
			// EntryNumberDividerLabel
			// 
			this.EntryNumberDividerLabel.AutoSize = true;
			this.EntryNumberDividerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EntryNumberDividerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 60, true);
			this.EntryNumberDividerLabel.Name = "EntryNumberDividerLabel";
			this.EntryNumberDividerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.EntryNumberDividerLabel.TabIndex = 11;
			this.EntryNumberDividerLabel.Text = "-";
			// 
			// PSCCheckBox
			// 
			this.PSCCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PSCCheckBox, "US_PSC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PSC)));
			this.PSCCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8aec3f4a-db51-4544-8b55-c10709353cc5", "PSC:");
			this.PSCCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PSCCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PSCCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 59, true);
			this.PSCCheckBox.Name = "PSCCheckBox";
			this.PSCCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PSCCheckBox.TabIndex = 5;
			this.PSCCheckBox.UseVisualStyleBackColor = true;
			// 
			// WarehouseWithdrawalPanel
			// 
			this.WarehouseWithdrawalPanel.Controls.Add(this.WarehouseWithdrawalLabel);
			this.WarehouseWithdrawalPanel.Controls.Add(this.DashLabel);
			this.WarehouseWithdrawalPanel.Controls.Add(this.IsFinalWHSCheckBox);
			this.WarehouseWithdrawalPanel.Controls.Add(this.WHSEntryNumberTextBox);
			this.WarehouseWithdrawalPanel.Controls.Add(this.WHSDistrictPortCodeFindBox);
			this.WarehouseWithdrawalPanel.Controls.Add(this.WHSEntryFilerCodeTextBox);
			this.WarehouseWithdrawalPanel.Controls.Add(this.QtyInWhBeforeWithdrawalCalcEdit);
			this.WarehouseWithdrawalPanel.Controls.Add(this.QtyBeingWithdrawnCalcEdit);
			this.WarehouseWithdrawalPanel.Controls.Add(this.QtyInWHAfterWithdrawalCalcEdit);
			this.WarehouseWithdrawalPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.WarehouseWithdrawalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 111, true);
			this.WarehouseWithdrawalPanel.Name = "WarehouseWithdrawalPanel";
			this.WarehouseWithdrawalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 71, true);
			this.WarehouseWithdrawalPanel.TabIndex = 10;
			// 
			// WarehouseWithdrawalLabel
			// 
			this.WarehouseWithdrawalLabel.AutoSize = true;
			this.WarehouseWithdrawalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.WarehouseWithdrawalLabel.IsFontBold = true;
			this.WarehouseWithdrawalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.WarehouseWithdrawalLabel.Name = "WarehouseWithdrawalLabel";
			this.WarehouseWithdrawalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 13, true);
			this.WarehouseWithdrawalLabel.TabIndex = 0;
			this.WarehouseWithdrawalLabel.Text = "Warehouse Withdrawal";
			// 
			// DashLabel
			// 
			this.DashLabel.AutoSize = true;
			this.DashLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 27, true);
			this.DashLabel.Name = "DashLabel";
			this.DashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.DashLabel.TabIndex = 2;
			this.DashLabel.Text = "-";
			// 
			// IsFinalWHSCheckBox
			// 
			this.IsFinalWHSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsFinalWHSCheckBox, "US_IsFinalWHS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_IsFinalWHS)));
			this.IsFinalWHSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsFinalWHSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsFinalWHSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 23, true);
			this.IsFinalWHSCheckBox.Name = "IsFinalWHSCheckBox";
			this.IsFinalWHSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.IsFinalWHSCheckBox.TabIndex = 5;
			this.IsFinalWHSCheckBox.Text = "Final Withdrawal\r\n";
			this.IsFinalWHSCheckBox.UseVisualStyleBackColor = true;
			// 
			// WHSEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WHSEntryNumberTextBox, "US_WHSEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_WHSEntryNumber)));
			this.WHSEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("241396a8-afdd-457a-8ffc-6cf692fd97fb", "-", "-", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WHSEntryNumberTextBox, false);
			this.WHSEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 22, true);
			this.WHSEntryNumberTextBox.Name = "WHSEntryNumberTextBox";
			this.WHSEntryNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.WHSEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.WHSEntryNumberTextBox.TabIndex = 3;
			// 
			// WHSDistrictPortCodeFindBox
			// 
			this.WHSDistrictPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WHSDistrictPortCodeFindBox, "US_WHSDistrictPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_WHSDistrictPortCode)));
			this.WHSDistrictPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d5bd4770-c12d-440f-94d9-b2a1de8be320", "Port");
			this.WHSDistrictPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 22, true);
			this.WHSDistrictPortCodeFindBox.Name = "WHSDistrictPortCodeFindBox";
			this.WHSDistrictPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WHSDistrictPortCodeFindBox.ParentType = null;
			this.WHSDistrictPortCodeFindBox.PreBoundMaxLength = 4;
			this.WHSDistrictPortCodeFindBox.ShowDescriptionBox = false;
			this.WHSDistrictPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.WHSDistrictPortCodeFindBox.TabIndex = 4;
			// 
			// WHSEntryFilerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.WHSEntryFilerCodeTextBox, "US_WHSEntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_WHSEntryFilerCode)));
			this.WHSEntryFilerCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("521137cd-8521-49a4-923f-117fdd0e37aa", "Entry No.", "Entry No.", "");
			this.WHSEntryFilerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 22, true);
			this.WHSEntryFilerCodeTextBox.Name = "WHSEntryFilerCodeTextBox";
			this.WHSEntryFilerCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.WHSEntryFilerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.WHSEntryFilerCodeTextBox.TabIndex = 1;
			// 
			// QtyInWhBeforeWithdrawalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QtyInWhBeforeWithdrawalCalcEdit, "US_QtyInWHBeforeWithdrawal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_QtyInWHBeforeWithdrawal)));
			this.QtyInWhBeforeWithdrawalCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e7c2453e-0111-47cb-b079-b1593764fe86", "Qty.", "Qty in W/H.", "Qty before withdrawal", "Bonded Amount: quantity in the warehouse account before the withdrawal.");
			this.QtyInWhBeforeWithdrawalCalcEdit.DecimalPlaces = 2;
			this.QtyInWhBeforeWithdrawalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 47, true);
			this.QtyInWhBeforeWithdrawalCalcEdit.Name = "QtyInWhBeforeWithdrawalCalcEdit";
			this.QtyInWhBeforeWithdrawalCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.QtyInWhBeforeWithdrawalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.QtyInWhBeforeWithdrawalCalcEdit.TabIndex = 7;
			this.QtyInWhBeforeWithdrawalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QtyBeingWithdrawnCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QtyBeingWithdrawnCalcEdit, "US_QtyBeingWithdrawn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_QtyBeingWithdrawn)));
			this.QtyBeingWithdrawnCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("76646a81-c40a-4e49-a487-dc46bfe74496", "Qty.", "W/draw Qty.", "Qty being Withdrawn", "Withdrawal: quantity being withdrawn in this entry.");
			this.QtyBeingWithdrawnCalcEdit.DecimalPlaces = 2;
			this.QtyBeingWithdrawnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 47, true);
			this.QtyBeingWithdrawnCalcEdit.Name = "QtyBeingWithdrawnCalcEdit";
			this.QtyBeingWithdrawnCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.QtyBeingWithdrawnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.QtyBeingWithdrawnCalcEdit.TabIndex = 8;
			this.QtyBeingWithdrawnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QtyInWHAfterWithdrawalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QtyInWHAfterWithdrawalCalcEdit, "US_QtyInWHAfterWithdrawal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_QtyInWHAfterWithdrawal)));
			this.QtyInWHAfterWithdrawalCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4eabd662-80d2-4195-b4bf-1ea96f24d808", "Qty.", "Balance", "Qty Remaining in W/H", "Balance: quantity remaining in the warehouse after withdrawal entry.");
			this.QtyInWHAfterWithdrawalCalcEdit.DecimalPlaces = 2;
			this.QtyInWHAfterWithdrawalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 47, true);
			this.QtyInWHAfterWithdrawalCalcEdit.Name = "QtyInWHAfterWithdrawalCalcEdit";
			this.QtyInWHAfterWithdrawalCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.QtyInWHAfterWithdrawalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.QtyInWHAfterWithdrawalCalcEdit.TabIndex = 9;
			this.QtyInWHAfterWithdrawalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TIBPanel
			// 
			this.TIBPanel.Controls.Add(this.TIBPurposeTextBox);
			this.TIBPanel.Controls.Add(this.TIBMotorVehiclesDropEdit);
			this.TIBPanel.Controls.Add(this.TIBMVNonConformingCheckBox);
			this.TIBPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.TIBPanel.Name = "TIBPanel";
			this.TIBPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 58, true);
			this.TIBPanel.TabIndex = 12;
			this.TIBPanel.Visible = false;
			// 
			// TIBPurposeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TIBPurposeTextBox, "US_TIBPurpose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TIBPurpose)));
			this.TIBPurposeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("782a20ba-11a2-4dbe-9e18-80fd7dded807", "TIB Purpose");
			this.TIBPurposeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 11, true);
			this.TIBPurposeTextBox.Name = "TIBPurposeTextBox";
			this.TIBPurposeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TIBPurposeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.TIBPurposeTextBox.TabIndex = 3;
			// 
			// TIBMotorVehiclesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TIBMotorVehiclesDropEdit, "US_TIBMotorVehicles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TIBMotorVehicles)));
			this.TIBMotorVehiclesDropEdit.AllowDrop = true;
			this.TIBMotorVehiclesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 34, true);
			this.TIBMotorVehiclesDropEdit.Name = "TIBMotorVehiclesDropEdit";
			this.TIBMotorVehiclesDropEdit.PreBoundMaxLength = 1;
			this.TIBMotorVehiclesDropEdit.ShouldResizeByMaxLength = true;
			this.TIBMotorVehiclesDropEdit.ShowDescriptionBox = false;
			this.TIBMotorVehiclesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TIBMotorVehiclesDropEdit.TabIndex = 4;
			// 
			// TIBMVNonConformingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TIBMVNonConformingCheckBox, "US_TIBMVNonConforming");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TIBMVNonConforming)));
			this.TIBMVNonConformingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 34, true);
			this.TIBMVNonConformingCheckBox.Name = "TIBMVNonConformingCheckBox";
			this.TIBMVNonConformingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.TIBMVNonConformingCheckBox.TabIndex = 5;
			// 
			// IORAuthAgentPanel
			// 
			this.IORAuthAgentPanel.Controls.Add(this.AuthorizedAgentRadioButton);
			this.IORAuthAgentPanel.Controls.Add(this.IORRadioButton);
			this.IORAuthAgentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 83, true);
			this.IORAuthAgentPanel.Name = "IORAuthAgentPanel";
			this.IORAuthAgentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 22, true);
			this.IORAuthAgentPanel.TabIndex = 9;
			// 
			// AuthorizedAgentRadioButton
			// 
			this.AuthorizedAgentRadioButton.AutoCheck = false;
			this.AuthorizedAgentRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AuthorizedAgentRadioButton, "US_7501Agent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_7501Agent)));
			this.AuthorizedAgentRadioButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AuthorizedAgentRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AuthorizedAgentRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 0, true);
			this.AuthorizedAgentRadioButton.Name = "AuthorizedAgentRadioButton";
			this.AuthorizedAgentRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 22, true);
			this.AuthorizedAgentRadioButton.TabIndex = 9;
			this.AuthorizedAgentRadioButton.Text = "Auth. Agent";
			this.AuthorizedAgentRadioButton.UseVisualStyleBackColor = true;
			// 
			// IORRadioButton
			// 
			this.IORRadioButton.AutoCheck = false;
			this.IORRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IORRadioButton, "US_7501IOR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_7501IOR)));
			this.IORRadioButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.IORRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IORRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IORRadioButton.Name = "IORRadioButton";
			this.IORRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 22, true);
			this.IORRadioButton.TabIndex = 8;
			this.IORRadioButton.TabStop = true;
			this.IORRadioButton.Text = "Importer of Record";
			this.IORRadioButton.UseVisualStyleBackColor = true;
			// 
			// PurchasedDropEdit
			// 
			this.PurchasedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PurchasedDropEdit, "US_7501Purchased");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_7501Purchased)));
			this.PurchasedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ea13907a-e79f-4eef-a5c8-f5f40ddca657", "Purchased", "Purchased", "");
			this.PurchasedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 85, true);
			this.PurchasedDropEdit.Name = "PurchasedDropEdit";
			this.PurchasedDropEdit.PreBoundMaxLength = 1;
			this.PurchasedDropEdit.ShouldResizeByMaxLength = true;
			this.PurchasedDropEdit.ShowDescriptionBox = false;
			this.PurchasedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PurchasedDropEdit.TabIndex = 7;
			// 
			// ManualEntryCheckBox
			// 
			this.ManualEntryCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManualEntryCheckBox, "US_ManEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ManEntry)));
			this.ManualEntryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ManualEntryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ManualEntryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 85, true);
			this.ManualEntryCheckBox.Name = "ManualEntryCheckBox";
			this.ManualEntryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.ManualEntryCheckBox.TabIndex = 8;
			this.ManualEntryCheckBox.Text = "Manual Entry";
			this.ManualEntryCheckBox.UseVisualStyleBackColor = true;
			// 
			// Box29Button
			// 
			this.Box29Button.IsCaptionOverridden = true;
			this.Box29Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 30, true);
			this.Box29Button.Name = "Box29Button";
			this.Box29Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Box29Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Box29Button.TabIndex = 2;
			this.Box29Button.Text = "3461 Box 29";
			this.Box29Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Box29Button.ToolTipCaption = null;
			this.Box29Button.UseVisualStyleBackColor = true;
			this.Box29Button.Click += new System.EventHandler(this.Box29Button_Click);
			// 
			// LocationOfGoodsCodeFindBox
			// 
			this.LocationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsCodeFindBox, "US_US_NKLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_US_NKLocationOfGoods)));
			this.LocationOfGoodsCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0f846519-2233-4f14-97e9-258f780ada9e", "FIRMS Code");
			this.LocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 8, true);
			this.LocationOfGoodsCodeFindBox.Name = "LocationOfGoodsCodeFindBox";
			this.LocationOfGoodsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocationOfGoodsCodeFindBox.ParentType = null;
			this.LocationOfGoodsCodeFindBox.PopupCaption = null;
			this.LocationOfGoodsCodeFindBox.PreBoundMaxLength = 4;
			this.LocationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.LocationOfGoodsCodeFindBox.TabIndex = 0;
			// 
			// CentralizedExamSiteFindBox
			// 
			this.CentralizedExamSiteFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CentralizedExamSiteFindBox, "US_US_NKCentralizedExamSite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_US_NKCentralizedExamSite)));
			this.CentralizedExamSiteFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5e20844f-f8b2-45d7-a32d-30d334230408", "Centralized Exam Site");
			this.CentralizedExamSiteFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 32, true);
			this.CentralizedExamSiteFindBox.Name = "CentralizedExamSiteFindBox";
			this.CentralizedExamSiteFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CentralizedExamSiteFindBox.ParentType = null;
			this.CentralizedExamSiteFindBox.PreBoundMaxLength = 4;
			this.CentralizedExamSiteFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CentralizedExamSiteFindBox.TabIndex = 1;
			// 
			// ImportEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportEntryNumberTextBox, "DecEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).DecEntryNumber)));
			this.ImportEntryNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImportEntryNumberTextBox, false);
			this.ImportEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 56, true);
			this.ImportEntryNumberTextBox.Name = "ImportEntryNumberTextBox";
			this.ImportEntryNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ImportEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.ImportEntryNumberTextBox.TabIndex = 4;
			// 
			// EntryFilerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryFilerCodeTextBox, "US_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryFilerCode)));
			this.EntryFilerCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("282bfee7-eee3-4e80-ac1b-cb3fec91c438", "Entry Number");
			this.EntryFilerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 56, true);
			this.EntryFilerCodeTextBox.Name = "EntryFilerCodeTextBox";
			this.EntryFilerCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.EntryFilerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.EntryFilerCodeTextBox.TabIndex = 3;
			// 
			// NonAMSCheckBox
			// 
			this.NonAMSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NonAMSCheckBox, "US_NonAMS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_NonAMS)));
			this.NonAMSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NonAMSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NonAMSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 46, true);
			this.NonAMSCheckBox.Name = "NonAMSCheckBox";
			this.NonAMSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.NonAMSCheckBox.TabIndex = 5;
			this.NonAMSCheckBox.Text = "Non-AMS:";
			this.NonAMSCheckBox.UseVisualStyleBackColor = true;
			// 
			// FTZAdmissionTypeDropEdit
			// 
			this.FTZAdmissionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FTZAdmissionTypeDropEdit, "US_F_AdmissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_F_AdmissionType)));
			this.FTZAdmissionTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5d02cdcc-c650-406b-9829-3678e9652913", "Adm. Type");
			this.FTZAdmissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 113, true);
			this.FTZAdmissionTypeDropEdit.Name = "FTZAdmissionTypeDropEdit";
			this.FTZAdmissionTypeDropEdit.ShouldResizeByMaxLength = true;
			this.FTZAdmissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.FTZAdmissionTypeDropEdit.TabIndex = 6;
			// 
			// DirectDeliveryIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DirectDeliveryIndicatorCheckBox, "US_F_DirectDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_F_DirectDelivery)));
			this.DirectDeliveryIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DirectDeliveryIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DirectDeliveryIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 156, true);
			this.DirectDeliveryIndicatorCheckBox.Name = "DirectDeliveryIndicatorCheckBox";
			this.DirectDeliveryIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.DirectDeliveryIndicatorCheckBox.TabIndex = 9;
			this.DirectDeliveryIndicatorCheckBox.Text = "Direct Delivery?";
			this.DirectDeliveryIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// MasterBillIssuerSCACFindBox
			// 
			this.MasterBillIssuerSCACFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MasterBillIssuerSCACFindBox, "JE_MasterBillIssuerSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MasterBillIssuerSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.USCarrierList)));
			this.MasterBillIssuerSCACFindBox.BindToList = "Lookups+USCarrierList";
			this.MasterBillIssuerSCACFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a2746d1c-dfca-40ab-9f64-e2d7fcb62440", "Issuer", "Issuer SCAC", "");
			this.MasterBillIssuerSCACFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 19, true);
			this.MasterBillIssuerSCACFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.MasterBillIssuerSCACFindBox.Name = "MasterBillIssuerSCACFindBox";
			this.MasterBillIssuerSCACFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.MasterBillIssuerSCACFindBox.ParentType = null;
			this.MasterBillIssuerSCACFindBox.PopupCaption = null;
			this.MasterBillIssuerSCACFindBox.PreBoundMaxLength = 4;
			this.MasterBillIssuerSCACFindBox.ShowDescriptionBox = false;
			this.MasterBillIssuerSCACFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.MasterBillIssuerSCACFindBox.TabIndex = 1;
			// 
			// HouseBillIssuerSCACFindBox
			// 
			this.HouseBillIssuerSCACFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillIssuerSCACFindBox, "JE_HouseBillIssuerSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_HouseBillIssuerSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.USCarrierList)));
			this.HouseBillIssuerSCACFindBox.BindToList = "Lookups+USCarrierList";
			this.HouseBillIssuerSCACFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bf8c845c-8e40-42e9-947b-e00e64be0d03", "Issuer", "Issuer SCAC", "");
			this.HouseBillIssuerSCACFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 16, true);
			this.HouseBillIssuerSCACFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.HouseBillIssuerSCACFindBox.Name = "HouseBillIssuerSCACFindBox";
			this.HouseBillIssuerSCACFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.HouseBillIssuerSCACFindBox.ParentType = null;
			this.HouseBillIssuerSCACFindBox.PopupCaption = null;
			this.HouseBillIssuerSCACFindBox.PreBoundMaxLength = 4;
			this.HouseBillIssuerSCACFindBox.ShowDescriptionBox = false;
			this.HouseBillIssuerSCACFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.HouseBillIssuerSCACFindBox.TabIndex = 1;
			// 
			// CarrierSCACCodeFindBox
			// 
			this.CarrierSCACCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierSCACCodeFindBox, "US_UI_NKCarrierSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_UI_NKCarrierSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.USCarrierList)));
			this.CarrierSCACCodeFindBox.BindToList = "Lookups+USCarrierList";
			this.CarrierSCACCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0144fd73-0fbc-4c5f-844b-43449541fff7", "Carrier SCAC");
			this.CarrierSCACCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 67, true);
			this.CarrierSCACCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.CarrierSCACCodeFindBox.Name = "CarrierSCACCodeFindBox";
			this.CarrierSCACCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CarrierSCACCodeFindBox.ParentType = null;
			this.CarrierSCACCodeFindBox.PopupCaption = null;
			this.CarrierSCACCodeFindBox.PreBoundMaxLength = 4;
			this.CarrierSCACCodeFindBox.ShowDescriptionBox = false;
			this.CarrierSCACCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.CarrierSCACCodeFindBox.TabIndex = 8;
			// 
			// MasterBillForFTZTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillForFTZTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MasterBill)));
			this.MasterBillForFTZTextBox.CaptionResourceString = null;
			this.MasterBillForFTZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 19, true);
			this.MasterBillForFTZTextBox.Name = "MasterBillForFTZTextBox";
			this.MasterBillForFTZTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MasterBillForFTZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MasterBillForFTZTextBox.TabIndex = 2;
			// 
			// DestinationStateDropEdit
			// 
			this.DestinationStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationStateDropEdit, "US_DestinationState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DestinationState)));
			this.DestinationStateDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("99615468-696e-4d79-ac9b-140d8869e66d", "Dest. State");
			this.DestinationStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 85, true);
			this.DestinationStateDropEdit.Name = "DestinationStateDropEdit";
			this.DestinationStateDropEdit.PreBoundMaxLength = 2;
			this.DestinationStateDropEdit.ShouldResizeByMaxLength = true;
			this.DestinationStateDropEdit.ShowDescriptionBox = false;
			this.DestinationStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.DestinationStateDropEdit.TabIndex = 7;
			// 
			// ImportDateOfFirstArrivalDateEdit
			// 
			this.ImportDateOfFirstArrivalDateEdit.AllowDrop = true;
			this.ImportDateOfFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportDateOfFirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportDateOfFirstArrivalDateEdit, "JE_DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_DateOfArrival)));
			this.ImportDateOfFirstArrivalDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("547f35c9-a636-4398-a673-2532ead5aba6", "Arr.");
			this.ImportDateOfFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 115, true);
			this.ImportDateOfFirstArrivalDateEdit.Name = "ImportDateOfFirstArrivalDateEdit";
			this.ImportDateOfFirstArrivalDateEdit.TabIndex = 13;
			// 
			// ITDateDateEdit
			// 
			this.ITDateDateEdit.AllowDrop = true;
			this.ITDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ITDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ITDateDateEdit, "US_ITDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ITDate)));
			this.ITDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("112ab313-a3c7-4849-adcf-467f096b9e08", "IT Date");
			this.ITDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 163, true);
			this.ITDateDateEdit.Name = "ITDateDateEdit";
			this.ITDateDateEdit.TabIndex = 18;
			// 
			// PrimaryITNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrimaryITNumberTextBox, "JE_PrimaryITNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_PrimaryITNumber)));
			this.PrimaryITNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b59293e9-7385-4ade-9a07-926aff6cf5a0", "IT No.", "IT Num.", "IT Number", "");
			this.PrimaryITNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 163, true);
			this.PrimaryITNumberTextBox.Name = "PrimaryITNumberTextBox";
			this.PrimaryITNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PrimaryITNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.PrimaryITNumberTextBox.TabIndex = 17;
			// 
			// EstimatedEntryDateDateEdit
			// 
			this.EstimatedEntryDateDateEdit.AllowDrop = true;
			this.EstimatedEntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedEntryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedEntryDateDateEdit, "US_EstimatedEntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EstimatedEntryDate)));
			this.EstimatedEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1e8da5dd-ecef-45ac-afe1-a0d6b6a1705a", "Est Entry Date", "Est. Entry Date", "Estimated Entry Date", "");
			this.EstimatedEntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 163, true);
			this.EstimatedEntryDateDateEdit.Name = "EstimatedEntryDateDateEdit";
			this.EstimatedEntryDateDateEdit.TabIndex = 16;
			// 
			// TransactionsRelatedDropEdit
			// 
			this.TransactionsRelatedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionsRelatedDropEdit, "US_TransactionsRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TransactionsRelated)));
			this.TransactionsRelatedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c6d6abd6-24e8-495a-bfdc-6e0c5fd926e5", "Tran Related", "Trans. Related", "Transaction Related", "");
			this.TransactionsRelatedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 4, true);
			this.TransactionsRelatedDropEdit.Name = "TransactionsRelatedDropEdit";
			this.TransactionsRelatedDropEdit.PreBoundMaxLength = 1;
			this.TransactionsRelatedDropEdit.ShouldResizeByMaxLength = true;
			this.TransactionsRelatedDropEdit.ShowDescriptionBox = false;
			this.TransactionsRelatedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.TransactionsRelatedDropEdit.TabIndex = 0;
			// 
			// HazardousMaterialDropEdit
			// 
			this.HazardousMaterialDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HazardousMaterialDropEdit, "US_HazardousCargo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_HazardousCargo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_YesNoList)));
			this.HazardousMaterialDropEdit.BindToList = "AddInfoLookups+US_YesNoList";
			this.HazardousMaterialDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b2b80c40-4fe3-4ef1-a220-94b30507658a", "Hazardous");
			this.HazardousMaterialDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 30, true);
			this.HazardousMaterialDropEdit.Name = "HazardousMaterialDropEdit";
			this.HazardousMaterialDropEdit.PreBoundMaxLength = 1;
			this.HazardousMaterialDropEdit.ShouldResizeByMaxLength = true;
			this.HazardousMaterialDropEdit.ShowDescriptionBox = false;
			this.HazardousMaterialDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.HazardousMaterialDropEdit.TabIndex = 1;
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MergeByDropEdit, "JE_MergeBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MergeBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.MergeByList)));
			this.MergeByDropEdit.BindToList = "Lookups+MergeByList";
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 81, true);
			this.MergeByDropEdit.Name = "MergeByDropEdit";
			this.MergeByDropEdit.PreBoundMaxLength = 3;
			this.MergeByDropEdit.ShouldResizeByMaxLength = true;
			this.MergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.MergeByDropEdit.TabIndex = 3;
			// 
			// TariffTypeDropEdit
			// 
			this.TariffTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffTypeDropEdit, "US_TariffType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TariffType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_TariffTypeList)));
			this.TariffTypeDropEdit.BindToList = "AddInfoLookups+US_TariffTypeList";
			this.TariffTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3c207641-d41a-40f8-a0f9-01e4ca600df4", "Tariff Type");
			this.TariffTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 108, true);
			this.TariffTypeDropEdit.Name = "TariffTypeDropEdit";
			this.TariffTypeDropEdit.PreBoundMaxLength = 3;
			this.TariffTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TariffTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.TariffTypeDropEdit.TabIndex = 4;
			// 
			// RoutedTransactionDropEdit
			// 
			this.RoutedTransactionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoutedTransactionDropEdit, "US_RoutedTransaction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_RoutedTransaction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_YesNoList)));
			this.RoutedTransactionDropEdit.BindToList = "AddInfoLookups+US_YesNoList";
			this.RoutedTransactionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bc1925d7-89c2-42b5-bd82-99935568f226", "Routed Tran");
			this.RoutedTransactionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 56, true);
			this.RoutedTransactionDropEdit.Name = "RoutedTransactionDropEdit";
			this.RoutedTransactionDropEdit.PreBoundMaxLength = 1;
			this.RoutedTransactionDropEdit.ShouldResizeByMaxLength = true;
			this.RoutedTransactionDropEdit.ShowDescriptionBox = false;
			this.RoutedTransactionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RoutedTransactionDropEdit.TabIndex = 2;
			// 
			// ExportShipmentTypePanel
			// 
			this.ExportShipmentTypePanel.Controls.Add(this.RoutedTransactionDropEdit);
			this.ExportShipmentTypePanel.Controls.Add(this.TariffTypeDropEdit);
			this.ExportShipmentTypePanel.Controls.Add(this.MergeByDropEdit);
			this.ExportShipmentTypePanel.Controls.Add(this.HazardousMaterialDropEdit);
			this.ExportShipmentTypePanel.Controls.Add(this.TransactionsRelatedDropEdit);
			this.ExportShipmentTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 111, true);
			this.ExportShipmentTypePanel.Name = "ExportShipmentTypePanel";
			this.ExportShipmentTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 133, true);
			this.ExportShipmentTypePanel.TabIndex = 4;
			// 
			// ExportAESTIRShipmentTypePanel
			// 
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.US_FirstPortOfCallCityTextBox);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.US_SoldEnRouteIndicatorDropEdit);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.US_RN_NKFirstPortOfCallCountryCodeFindBox);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.US_CommodityFilingOptionDropEdit);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.AESTIRRoutedTransactionDropEdit);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.AESTIRTariffTypeDropEdit);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.AESTIRHazardousMaterialDropEdit);
			this.ExportAESTIRShipmentTypePanel.Controls.Add(this.AESTIRTransactionsRelatedDropEdit);
			this.ExportAESTIRShipmentTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 111, true);
			this.ExportAESTIRShipmentTypePanel.Name = "ExportAESTIRShipmentTypePanel";
			this.ExportAESTIRShipmentTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 133, true);
			this.ExportAESTIRShipmentTypePanel.TabIndex = 4;
			// 
			// US_FirstPortOfCallCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_FirstPortOfCallCityTextBox, "US_FirstPortOfCallCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_FirstPortOfCallCity)));
			this.US_FirstPortOfCallCityTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("829eb471-2536-45c7-b680-2d85200a4fa6", "1st City", "City of first port of call.");
			this.US_FirstPortOfCallCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 106, true);
			this.US_FirstPortOfCallCityTextBox.Name = "US_FirstPortOfCallCityTextBox";
			this.US_FirstPortOfCallCityTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.US_FirstPortOfCallCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.US_FirstPortOfCallCityTextBox.TabIndex = 8;
			// 
			// US_SoldEnRouteIndicatorDropEdit
			// 
			this.US_SoldEnRouteIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_SoldEnRouteIndicatorDropEdit, "US_SoldEnRouteIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SoldEnRouteIndicator)));
			this.US_SoldEnRouteIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cf47c0f5-d8fa-4cc7-bbd1-c5ef5ea99171", "Sld.En Route", "Sold En Route", "");
			this.US_SoldEnRouteIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 80, true);
			this.US_SoldEnRouteIndicatorDropEdit.Name = "US_SoldEnRouteIndicatorDropEdit";
			this.US_SoldEnRouteIndicatorDropEdit.PreBoundMaxLength = 1;
			this.US_SoldEnRouteIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.US_SoldEnRouteIndicatorDropEdit.ShowDescriptionBox = false;
			this.US_SoldEnRouteIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.US_SoldEnRouteIndicatorDropEdit.TabIndex = 6;
			// 
			// US_RN_NKFirstPortOfCallCountryCodeFindBox
			// 
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_RN_NKFirstPortOfCallCountryCodeFindBox, "US_RN_NKFirstPortOfCallCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_RN_NKFirstPortOfCallCountry)));
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bcee3e5b-dbe7-4c6f-b977-84a75f3b7040", "1st Country", "Country of first port of call.");
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 80, true);
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.Name = "US_RN_NKFirstPortOfCallCountryCodeFindBox";
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.ParentType = null;
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.PreBoundMaxLength = 2;
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.ShowDescriptionBox = false;
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.TabIndex = 7;
			// 
			// US_CommodityFilingOptionDropEdit
			// 
			this.US_CommodityFilingOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_CommodityFilingOptionDropEdit, "US_CommodityFilingOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_CommodityFilingOption)));
			this.US_CommodityFilingOptionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f1ab61f5-87b7-4a6b-9c77-a4df532d44c8", "Filing Option", "Comm. Filing Option", "Commodity Filing Option", "Commodity Shipment Filing Option.");
			this.US_CommodityFilingOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 30, true);
			this.US_CommodityFilingOptionDropEdit.Name = "US_CommodityFilingOptionDropEdit";
			this.US_CommodityFilingOptionDropEdit.PreBoundMaxLength = 1;
			this.US_CommodityFilingOptionDropEdit.ShouldResizeByMaxLength = true;
			this.US_CommodityFilingOptionDropEdit.ShowDescriptionBox = false;
			this.US_CommodityFilingOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.US_CommodityFilingOptionDropEdit.TabIndex = 3;
			// 
			// AESTIRRoutedTransactionDropEdit
			// 
			this.AESTIRRoutedTransactionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AESTIRRoutedTransactionDropEdit, "US_RoutedTransaction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_RoutedTransaction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_YesNoList)));
			this.AESTIRRoutedTransactionDropEdit.BindToList = "AddInfoLookups+US_YesNoList";
			this.AESTIRRoutedTransactionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f21ba1f4-1243-4c00-9ba7-cec2bd34f03e", "Routed Tran");
			this.AESTIRRoutedTransactionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 30, true);
			this.AESTIRRoutedTransactionDropEdit.Name = "AESTIRRoutedTransactionDropEdit";
			this.AESTIRRoutedTransactionDropEdit.PreBoundMaxLength = 1;
			this.AESTIRRoutedTransactionDropEdit.ShouldResizeByMaxLength = true;
			this.AESTIRRoutedTransactionDropEdit.ShowDescriptionBox = false;
			this.AESTIRRoutedTransactionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AESTIRRoutedTransactionDropEdit.TabIndex = 2;
			// 
			// AESTIRTariffTypeDropEdit
			// 
			this.AESTIRTariffTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AESTIRTariffTypeDropEdit, "US_TariffType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TariffType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_TariffTypeList)));
			this.AESTIRTariffTypeDropEdit.BindToList = "AddInfoLookups+US_TariffTypeList";
			this.AESTIRTariffTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("61edc8d3-8b9f-4f73-94ce-f8df92f56469", "Tariff Type");
			this.AESTIRTariffTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 55, true);
			this.AESTIRTariffTypeDropEdit.Name = "AESTIRTariffTypeDropEdit";
			this.AESTIRTariffTypeDropEdit.PreBoundMaxLength = 3;
			this.AESTIRTariffTypeDropEdit.ShouldResizeByMaxLength = true;
			this.AESTIRTariffTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.AESTIRTariffTypeDropEdit.TabIndex = 5;
			// 
			// AESTIRHazardousMaterialDropEdit
			// 
			this.AESTIRHazardousMaterialDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AESTIRHazardousMaterialDropEdit, "US_HazardousCargo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_HazardousCargo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_YesNoList)));
			this.AESTIRHazardousMaterialDropEdit.BindToList = "AddInfoLookups+US_YesNoList";
			this.AESTIRHazardousMaterialDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dc9c4758-c8d1-4a06-9886-ad2685c1c804", "Hazardous");
			this.AESTIRHazardousMaterialDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 4, true);
			this.AESTIRHazardousMaterialDropEdit.Name = "AESTIRHazardousMaterialDropEdit";
			this.AESTIRHazardousMaterialDropEdit.PreBoundMaxLength = 1;
			this.AESTIRHazardousMaterialDropEdit.ShouldResizeByMaxLength = true;
			this.AESTIRHazardousMaterialDropEdit.ShowDescriptionBox = false;
			this.AESTIRHazardousMaterialDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AESTIRHazardousMaterialDropEdit.TabIndex = 1;
			// 
			// AESTIRTransactionsRelatedDropEdit
			// 
			this.AESTIRTransactionsRelatedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AESTIRTransactionsRelatedDropEdit, "US_TransactionsRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TransactionsRelated)));
			this.AESTIRTransactionsRelatedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6d7f19f7-502f-4e7a-845f-357662ccd383", "Tran Related", "Trans. Related", "Transaction Related", "");
			this.AESTIRTransactionsRelatedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 4, true);
			this.AESTIRTransactionsRelatedDropEdit.Name = "AESTIRTransactionsRelatedDropEdit";
			this.AESTIRTransactionsRelatedDropEdit.PreBoundMaxLength = 1;
			this.AESTIRTransactionsRelatedDropEdit.ShouldResizeByMaxLength = true;
			this.AESTIRTransactionsRelatedDropEdit.ShowDescriptionBox = false;
			this.AESTIRTransactionsRelatedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AESTIRTransactionsRelatedDropEdit.TabIndex = 0;
			// 
			// SEDMessageStatusDropEdit
			// 
			this.SEDMessageStatusDropEdit.AllowDrop = true;
			this.SEDMessageStatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SEDMessageStatusDropEdit, "JE_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MessageStatusDescription)));
			this.SEDMessageStatusDropEdit.BindToForDescription = "JE_MessageStatusDescription";
			this.SEDMessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 13, true);
			this.SEDMessageStatusDropEdit.Name = "SEDMessageStatusDropEdit";
			this.SEDMessageStatusDropEdit.PreBoundMaxLength = 2;
			this.SEDMessageStatusDropEdit.ShouldResizeByMaxLength = true;
			this.SEDMessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 20, true);
			this.SEDMessageStatusDropEdit.TabIndex = 6;
			// 
			// OriginalITNTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginalITNTextBox, "US_OriginalITNNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_OriginalITNNumber)));
			this.OriginalITNTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9E89EBC8-446F-4DBA-A5A3-43D2C5F7810B", "Original ITN Number");
			this.OriginalITNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 35, true);
			this.OriginalITNTextBox.Name = "OriginalITNTextBox";
			this.OriginalITNTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.OriginalITNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.OriginalITNTextBox.TabIndex = 2;
			// 
			// US_DateOfExportDateEdit
			// 
			this.US_DateOfExportDateEdit.AllowDrop = true;
			this.US_DateOfExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.US_DateOfExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.US_DateOfExportDateEdit, "US_DateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DateOfExport)));
			this.US_DateOfExportDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5bf0f3b1-3b69-45ae-90f9-05bc7fa647f5", "Export Date");
			this.US_DateOfExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 85, true);
			this.US_DateOfExportDateEdit.Name = "US_DateOfExportDateEdit";
			this.US_DateOfExportDateEdit.TabIndex = 9;
			// 
			// US_UC_NKCountryOfExportCodeFindBox
			// 
			this.US_UC_NKCountryOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_UC_NKCountryOfExportCodeFindBox, "US_UC_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_UC_NKCountryOfExport)));
			this.US_UC_NKCountryOfExportCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("06fe9e45-dedc-44c0-ad1e-38286af090c2", "Ctry/Rgn. of Export", "Ctry/Rgn. of Export", "Country/Region of Export", "The country or region of export.");
			this.US_UC_NKCountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 85, true);
			this.US_UC_NKCountryOfExportCodeFindBox.Name = "US_UC_NKCountryOfExportCodeFindBox";
			this.US_UC_NKCountryOfExportCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.US_UC_NKCountryOfExportCodeFindBox.ParentType = null;
			this.US_UC_NKCountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.US_UC_NKCountryOfExportCodeFindBox.ShowDescriptionBox = false;
			this.US_UC_NKCountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.US_UC_NKCountryOfExportCodeFindBox.TabIndex = 8;
			// 
			// JourneyTextBox
			// 
			this.BindingSource.SetBindingMember(this.JourneyTextBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_VesselName)));
			this.JourneyTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a5036bff-a202-4f8d-937d-5e59879fe6df", "Journey");
			this.JourneyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 43, true);
			this.JourneyTextBox.Name = "JourneyTextBox";
			this.JourneyTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JourneyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.JourneyTextBox.TabIndex = 4;
			// 
			// CarrierNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierNameTextBox, "US_CarrierName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_CarrierName)));
			this.CarrierNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("69A8A0D7-854A-45DD-8B39-B1E6609A32BB", "Carrier", "Carrier Name");
			this.CarrierNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 43, true);
			this.CarrierNameTextBox.Name = "CarrierNameTextBox";
			this.CarrierNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CarrierNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.CarrierNameTextBox.TabIndex = 9;
			// 
			// FTZSPNCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FTZSPNCheckBox, "US_EnableSPN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EnableSPN)));
			this.FTZSPNCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FTZSPNCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FTZSPNCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 136, true);
			this.FTZSPNCheckBox.Name = "FTZSPNCheckBox";
			this.FTZSPNCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.FTZSPNCheckBox.TabIndex = 7;
			this.FTZSPNCheckBox.Text = "Stand Alone Prior Notice";
			this.FTZSPNCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncludePTTCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludePTTCheckBox, "US_F_IncludePTT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_F_IncludePTT)));
			this.IncludePTTCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IncludePTTCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludePTTCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 176, true);
			this.IncludePTTCheckBox.Name = "IncludePTTCheckBox";
			this.IncludePTTCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.IncludePTTCheckBox.TabIndex = 10;
			this.IncludePTTCheckBox.Text = "Include PTT in Admission?";
			this.IncludePTTCheckBox.UseVisualStyleBackColor = true;
			// 
			// PTTWithoutExceptionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PTTWithoutExceptionCheckBox, "US_F_PTTWOExc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_F_PTTWOExc)));
			this.PTTWithoutExceptionCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PTTWithoutExceptionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PTTWithoutExceptionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 196, true);
			this.PTTWithoutExceptionCheckBox.Name = "PTTWithoutExceptionCheckBox";
			this.PTTWithoutExceptionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.PTTWithoutExceptionCheckBox.TabIndex = 11;
			this.PTTWithoutExceptionCheckBox.Text = "PTT without Exception";
			this.PTTWithoutExceptionCheckBox.UseVisualStyleBackColor = true;
			// 
			// CalcTransportationModeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CalcTransportationModeTextBox, "JE_Calc_USTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_Calc_USTransportMode)));
			this.CalcTransportationModeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4fecb9e5-a43d-40b5-b52f-fab3d1410e6f", "Calculated MOT", "Calculated Mode of Transport", "Calc. MOT", "Calculated Mode of Transportation Code");
			this.CalcTransportationModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 65, true);
			this.CalcTransportationModeTextBox.Name = "CalcTransportationModeTextBox";
			this.CalcTransportationModeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CalcTransportationModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.CalcTransportationModeTextBox.TabIndex = 9;
			// 
			// PipelineNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.PipelineNameTextBox, "US_PipelineName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PipelineName)));
			this.PipelineNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fc4a27fd-95a6-41bd-9e6d-5ce04f2d4ccc", "Pipeline");
			this.PipelineNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 43, true);
			this.PipelineNameTextBox.Name = "PipelineNameTextBox";
			this.PipelineNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PipelineNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.PipelineNameTextBox.TabIndex = 3;
			// 
			// BatchTicketTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchTicketTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MasterBill)));
			this.BatchTicketTextBox.CaptionResourceString = null;
			this.BatchTicketTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 19, true);
			this.BatchTicketTextBox.Name = "BatchTicketTextBox";
			this.BatchTicketTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.BatchTicketTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.BatchTicketTextBox.TabIndex = 2;
			// 
			// TransportOrgsControl
			// 
			this.TransportOrgsControl.AllowDrop = true;
			this.TransportOrgsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransportOrgsControl, ".");
			this.TransportOrgsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 178, true);
			this.TransportOrgsControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 112, true);
			this.TransportOrgsControl.Name = "TransportOrgsControl";
			this.TransportOrgsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 112, true);
			this.TransportOrgsControl.TabIndex = 1;
			// 
			// USOrganisationsTabPage
			// 
			this.USOrganisationsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4dd631c2-8111-4cca-8ed0-f84872f27477", "Organizations");
			this.USOrganisationsTabPage.Controls.Add(this.JobMainOrgsControl);
			this.USOrganisationsTabPage.Controls.Add(this.TransportOrgsControl);
			this.USOrganisationsTabPage.Controls.Add(this.JobMiscOrgsControl);
			this.USOrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.USOrganisationsTabPage.Name = "USOrganisationsTabPage";
			this.USOrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.USOrganisationsTabPage.TabIndex = 9;
			this.USOrganisationsTabPage.Text = "Organizations";
			// 
			// JobMainOrgsControl
			// 
			this.JobMainOrgsControl.AllowDrop = true;
			this.JobMainOrgsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobMainOrgsControl, ".");
			this.JobMainOrgsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobMainOrgsControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 178, true);
			this.JobMainOrgsControl.Name = "JobMainOrgsControl";
			this.JobMainOrgsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 178, true);
			this.JobMainOrgsControl.TabIndex = 0;
			// 
			// JobMiscOrgsControl
			// 
			this.JobMiscOrgsControl.AllowDrop = true;
			this.JobMiscOrgsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobMiscOrgsControl, ".");
			this.JobMiscOrgsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.JobMiscOrgsControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 222, true);
			this.JobMiscOrgsControl.Name = "JobMiscOrgsControl";
			this.JobMiscOrgsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 222, true);
			this.JobMiscOrgsControl.TabIndex = 2;
			// 
			// ConsolidatedSummaryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolidatedSummaryCheckBox, "US_ConsolACE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ConsolACE)));
			this.ConsolidatedSummaryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ConsolidatedSummaryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolidatedSummaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 179, true);
			this.ConsolidatedSummaryCheckBox.Name = "ConsolidatedSummaryCheckBox";
			this.ConsolidatedSummaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.ConsolidatedSummaryCheckBox.TabIndex = 11;
			this.ConsolidatedSummaryCheckBox.Text = "Consolidated Summary";
			this.ConsolidatedSummaryCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExpressTrackingCheckBox
			// 
			this.ExpressTrackingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExpressTrackingCheckBox, "JE_MasterBillExpressTracking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MasterBillExpressTracking)));
			this.ExpressTrackingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExpressTrackingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExpressTrackingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 46, true);
			this.ExpressTrackingCheckBox.Name = "ExpressTrackingCheckBox";
			this.ExpressTrackingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExpressTrackingCheckBox.TabIndex = 4;
			this.ExpressTrackingCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExpressTrackingNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExpressTrackingNumberTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MasterBill)));
			this.ExpressTrackingNumberTextBox.CaptionResourceString = null;
			this.ExpressTrackingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 19, true);
			this.ExpressTrackingNumberTextBox.Name = "ExpressTrackingNumberTextBox";
			this.ExpressTrackingNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ExpressTrackingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ExpressTrackingNumberTextBox.TabIndex = 2;
			// 
			// FTZTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTZTextBox, "US_FTZNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_FTZNo)));
			this.FTZTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7d49ef97-b2ce-465a-9f61-8c03de139864", "FTZ No.", "FTZ Number", "");
			this.FTZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 20, true);
			this.FTZTextBox.Name = "FTZTextBox";
			this.FTZTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.FTZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.FTZTextBox.TabIndex = 19;
			// 
			// USJobDeclarationUserControl
			// 
			this.Controls.Add(this.ImportStatusGroupBox);
			this.Controls.Add(this.SEDGroupBox);
			this.Name = "USJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 678, true);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.SEDGroupBox, 0);
			this.Controls.SetChildIndex(this.ImportStatusGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.InsuranceValueCalcFindBox.ResumeLayout(true);
			this.InsuranceValueCalcFindBox.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.JE_MasterBillForAirBoundTextBox.ResumeLayout(true);
			this.JE_MasterBillForAirBoundTextBox.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.JE_ExportDateBoundDateEdit.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit2.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit2.PerformLayout();
			this.JE_ExportDateBoundDateEdit2.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit2.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.OrganisationsTopPanel.ResumeLayout(false);
			this.OrganisationsTopPanel.PerformLayout();
			this.ShippingOrAirLineOrganisationControl.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ShipmentCustomFieldsPage.ResumeLayout(false);
			this.ShipmentCustomFieldsPage.PerformLayout();
			this.shipmentCustomFieldsControl1.ResumeLayout(true);
			this.shipmentCustomFieldsControl1.PerformLayout();
			this.OrdersPanel.ResumeLayout(false);
			this.OrdersPanel.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.OrdersAttachUserControl.ResumeLayout(true);
			this.OrdersAttachUserControl.PerformLayout();
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.FPPIGuidFindBox.ResumeLayout(true);
			this.FPPIGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImportShipmentTypePanel.ResumeLayout(false);
			this.ImportShipmentTypePanel.PerformLayout();
			this.CargoReleaseTypeDropEdit.ResumeLayout(true);
			this.CargoReleaseTypeDropEdit.PerformLayout();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.LiveEntryIndicatorDropEdit.ResumeLayout(true);
			this.LiveEntryIndicatorDropEdit.PerformLayout();
			this.EntryModeDropEdit.ResumeLayout(true);
			this.EntryModeDropEdit.PerformLayout();
			this.EntryTypeDropEdit.ResumeLayout(true);
			this.EntryTypeDropEdit.PerformLayout();
			this.SEDGroupBox.ResumeLayout(false);
			this.SEDGroupBox.PerformLayout();
			this.US_RN_NKCountryOfDestinationCodeFindBox.ResumeLayout(true);
			this.US_RN_NKCountryOfDestinationCodeFindBox.PerformLayout();
			this.StateOfOriginDropEdit.ResumeLayout(true);
			this.StateOfOriginDropEdit.PerformLayout();
			this.ExportCodeDropEdit.ResumeLayout(true);
			this.ExportCodeDropEdit.PerformLayout();
			this.InbondTypeDropEdit.ResumeLayout(true);
			this.InbondTypeDropEdit.PerformLayout();
			this.LicenseTypeCodeFindBox.ResumeLayout(true);
			this.LicenseTypeCodeFindBox.PerformLayout();
			this.PortOfExportPanel.ResumeLayout(false);
			this.PortOfExportPanel.PerformLayout();
			this.PortOfExportSchDCodeFindBox.ResumeLayout(true);
			this.PortOfExportSchDCodeFindBox.PerformLayout();
			this.PortOfExportSchDCodeDropEdit.ResumeLayout(true);
			this.PortOfExportSchDCodeDropEdit.PerformLayout();
			this.DateOfExportDateEdit.ResumeLayout(true);
			this.DateOfExportDateEdit.PerformLayout();
			this.PortOfExportCodeFindBox.ResumeLayout(true);
			this.PortOfExportCodeFindBox.PerformLayout();
			this.PortOfLoadingSchDFindBox.ResumeLayout(true);
			this.PortOfLoadingSchDFindBox.PerformLayout();
			this.PortOfLoadingSchDDropEdit.ResumeLayout(true);
			this.PortOfLoadingSchDDropEdit.PerformLayout();
			this.PortOfDischargeSchDFindBox.ResumeLayout(true);
			this.PortOfDischargeSchDFindBox.PerformLayout();
			this.PortOfDischargeSchDDropEdit.ResumeLayout(true);
			this.PortOfDischargeSchDDropEdit.PerformLayout();
			this.ConsigneeOrganisationControl.ResumeLayout(true);
			this.ConsigneeOrganisationControl.PerformLayout();
			this.PortOfImportPanel.ResumeLayout(false);
			this.PortOfImportPanel.PerformLayout();
			this.HMFPanel.ResumeLayout(false);
			this.HMFPanel.PerformLayout();
			this.HMFApplicableZDropEdit.ResumeLayout(true);
			this.HMFApplicableZDropEdit.PerformLayout();
			this.JE_DateOfFirstArrivalDateEdit.ResumeLayout(true);
			this.JE_DateOfFirstArrivalDateEdit.PerformLayout();
			this.PortOfEntryScheduleCodeFindBox.ResumeLayout(true);
			this.PortOfEntryScheduleCodeFindBox.PerformLayout();
			this.DDTCDetailsTabPage.ResumeLayout(false);
			this.DDTCDetailsTabPage.PerformLayout();
			this.DDTCUSMLCategoryCodeDropEdit.ResumeLayout(true);
			this.DDTCUSMLCategoryCodeDropEdit.PerformLayout();
			this.DDTCPartyCertificationIndicatorDropEdit.ResumeLayout(true);
			this.DDTCPartyCertificationIndicatorDropEdit.PerformLayout();
			this.DDTCMilitaryEquipmentIndicatorDropEdit.ResumeLayout(true);
			this.DDTCMilitaryEquipmentIndicatorDropEdit.PerformLayout();
			this.DDTCITARExemptionNumberDropEdit.ResumeLayout(true);
			this.DDTCITARExemptionNumberDropEdit.PerformLayout();
			this.ImportStatusGroupBox.ResumeLayout(false);
			this.ImportStatusGroupBox.PerformLayout();
			this.SEReleaseAndStandAlonePriorNoticePanel.ResumeLayout(false);
			this.SEReleaseAndStandAlonePriorNoticePanel.PerformLayout();
			this.SPNIDTypeDropEdit.ResumeLayout(true);
			this.SPNIDTypeDropEdit.PerformLayout();
			this.SplitShipmentReleaseCodeForSPNDropEdit.ResumeLayout(true);
			this.SplitShipmentReleaseCodeForSPNDropEdit.PerformLayout();
			this.FTZPanel.ResumeLayout(false);
			this.FTZPanel.PerformLayout();
			this.FTZSPNIDTypeDropEdit.ResumeLayout(true);
			this.FTZSPNIDTypeDropEdit.PerformLayout();
			this.ZoneIDDropDownEdit.ResumeLayout(true);
			this.ZoneIDDropDownEdit.PerformLayout();
			this.DeliveryDropEdit.ResumeLayout(true);
			this.DeliveryDropEdit.PerformLayout();
			this.SEReleasePanel.ResumeLayout(false);
			this.SEReleasePanel.PerformLayout();
			this.SplitShipmentReleaseCodeDropEdit.ResumeLayout(true);
			this.SplitShipmentReleaseCodeDropEdit.PerformLayout();
			this.WarehouseWithdrawalPanel.ResumeLayout(false);
			this.WarehouseWithdrawalPanel.PerformLayout();
			this.WHSDistrictPortCodeFindBox.ResumeLayout(true);
			this.WHSDistrictPortCodeFindBox.PerformLayout();
			this.TIBPanel.ResumeLayout(false);
			this.TIBPanel.PerformLayout();
			this.IORAuthAgentPanel.ResumeLayout(false);
			this.IORAuthAgentPanel.PerformLayout();
			this.PurchasedDropEdit.ResumeLayout(true);
			this.PurchasedDropEdit.PerformLayout();
			this.LocationOfGoodsCodeFindBox.ResumeLayout(true);
			this.LocationOfGoodsCodeFindBox.PerformLayout();
			this.CentralizedExamSiteFindBox.ResumeLayout(true);
			this.CentralizedExamSiteFindBox.PerformLayout();
			this.FTZAdmissionTypeDropEdit.ResumeLayout(true);
			this.FTZAdmissionTypeDropEdit.PerformLayout();
			this.MasterBillIssuerSCACFindBox.ResumeLayout(true);
			this.MasterBillIssuerSCACFindBox.PerformLayout();
			this.HouseBillIssuerSCACFindBox.ResumeLayout(true);
			this.HouseBillIssuerSCACFindBox.PerformLayout();
			this.CarrierSCACCodeFindBox.ResumeLayout(true);
			this.CarrierSCACCodeFindBox.PerformLayout();
			this.DestinationStateDropEdit.ResumeLayout(true);
			this.DestinationStateDropEdit.PerformLayout();
			this.ImportDateOfFirstArrivalDateEdit.ResumeLayout(true);
			this.ImportDateOfFirstArrivalDateEdit.PerformLayout();
			this.ITDateDateEdit.ResumeLayout(true);
			this.ITDateDateEdit.PerformLayout();
			this.EstimatedEntryDateDateEdit.ResumeLayout(true);
			this.EstimatedEntryDateDateEdit.PerformLayout();
			this.TransactionsRelatedDropEdit.ResumeLayout(true);
			this.TransactionsRelatedDropEdit.PerformLayout();
			this.HazardousMaterialDropEdit.ResumeLayout(true);
			this.HazardousMaterialDropEdit.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.TariffTypeDropEdit.ResumeLayout(true);
			this.TariffTypeDropEdit.PerformLayout();
			this.RoutedTransactionDropEdit.ResumeLayout(true);
			this.RoutedTransactionDropEdit.PerformLayout();
			this.ExportShipmentTypePanel.ResumeLayout(false);
			this.ExportShipmentTypePanel.PerformLayout();
			this.ExportAESTIRShipmentTypePanel.ResumeLayout(false);
			this.ExportAESTIRShipmentTypePanel.PerformLayout();
			this.US_SoldEnRouteIndicatorDropEdit.ResumeLayout(true);
			this.US_SoldEnRouteIndicatorDropEdit.PerformLayout();
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.ResumeLayout(true);
			this.US_RN_NKFirstPortOfCallCountryCodeFindBox.PerformLayout();
			this.US_CommodityFilingOptionDropEdit.ResumeLayout(true);
			this.US_CommodityFilingOptionDropEdit.PerformLayout();
			this.AESTIRRoutedTransactionDropEdit.ResumeLayout(true);
			this.AESTIRRoutedTransactionDropEdit.PerformLayout();
			this.AESTIRTariffTypeDropEdit.ResumeLayout(true);
			this.AESTIRTariffTypeDropEdit.PerformLayout();
			this.AESTIRHazardousMaterialDropEdit.ResumeLayout(true);
			this.AESTIRHazardousMaterialDropEdit.PerformLayout();
			this.AESTIRTransactionsRelatedDropEdit.ResumeLayout(true);
			this.AESTIRTransactionsRelatedDropEdit.PerformLayout();
			this.SEDMessageStatusDropEdit.ResumeLayout(true);
			this.SEDMessageStatusDropEdit.PerformLayout();
			this.US_DateOfExportDateEdit.ResumeLayout(true);
			this.US_DateOfExportDateEdit.PerformLayout();
			this.US_UC_NKCountryOfExportCodeFindBox.ResumeLayout(true);
			this.US_UC_NKCountryOfExportCodeFindBox.PerformLayout();
			this.TransportOrgsControl.ResumeLayout(true);
			this.TransportOrgsControl.PerformLayout();
			this.USOrganisationsTabPage.ResumeLayout(false);
			this.USOrganisationsTabPage.PerformLayout();
			this.JobMainOrgsControl.ResumeLayout(true);
			this.JobMainOrgsControl.PerformLayout();
			this.JobMiscOrgsControl.ResumeLayout(true);
			this.JobMiscOrgsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Controls

		ZGuidFindBox ConsigneeOrganisationControl;
		ZGuidFindBox FPPIGuidFindBox;
		internal ZPanel ImportShipmentTypePanel;
		ZGroupBox SEDGroupBox;
		internal ZDropEdit ExportCodeDropEdit;
		internal ZTextBox LicenseNoTextBox;
		internal ZCodeFindBox ECCNCodeFindBox;
		internal ZTextBox ECCNTextBox;
		internal ZDropEdit InbondTypeDropEdit;
		internal ZTextBox ImportEntryNoTextBox;
		internal ZCodeFindBox LicenseTypeCodeFindBox;
		internal ZTextBox ForeignTradeZoneTextBox;
		internal ZDropEdit StateOfOriginDropEdit;
		internal ZPanel PortOfExportPanel;
		internal ZDateEdit DateOfExportDateEdit;
		internal ZCodeFindBox PortOfExportCodeFindBox;
		internal ZTextBox TransportReferenceTextBox;
		protected ZCodeFindBox PortOfDischargeSchDFindBox;
		protected ZDropEdit PortOfDischargeSchDDropEdit;
		protected ZCodeFindBox PortOfLoadingSchDFindBox;
		protected ZDropEdit PortOfLoadingSchDDropEdit;
		protected ZCodeFindBox zCodeFindBox1;
		ZPanel PortOfImportPanel;
		ZDateEdit JE_DateOfFirstArrivalDateEdit;
		internal ZDropEdit EntryTypeDropEdit;
		ZTabPage DDTCDetailsTabPage;
		protected ZCodeFindBox PortOfExportSchDCodeFindBox;
		protected ZDropEdit PortOfExportSchDCodeDropEdit;
		internal ZCheckBox US_EnableAIICheckBox;
		internal ZCheckBox US_EnableCRLCheckBox;
		protected ZCodeFindBox PortOfEntryScheduleCodeFindBox;
		ZGroupBox ImportStatusGroupBox;
		internal ZCodeFindBox HouseBillIssuerSCACFindBox;
		internal ZCodeFindBox MasterBillIssuerSCACFindBox;
		public ZCodeFindBox CarrierSCACCodeFindBox;
		protected internal ZPanel HMFPanel;
		ZDropEdit HMFApplicableZDropEdit;
		internal ZTextBox MasterBillForFTZTextBox;
		ZDropEdit DDTCITARExemptionNumberDropEdit;
		ZDropEdit DDTCPartyCertificationIndicatorDropEdit;
		ZDropEdit DDTCMilitaryEquipmentIndicatorDropEdit;
		ZTextBox DDTCRegistrationNumberTextBox;
		ZTextBox DDTCCategoryXXIDeterminationNumberTextBox;
		ZDropEdit DDTCUSMLCategoryCodeDropEdit;
		ZTextBox ImportEntryNumberTextBox;
		internal ZButton AllocateImportEntryNumberButton;
		internal ZDropEdit DestinationStateDropEdit;
		public ZDateEdit ImportDateOfFirstArrivalDateEdit;
		ZCodeFindBox LocationOfGoodsCodeFindBox;
		internal ZButton Box29Button;
		ZCodeFindBox CentralizedExamSiteFindBox;
		ZCheckBox US_CertifyCargoReleaseCheckBox;
		internal ZTextBox PrimaryITNumberTextBox;
		internal ZDateEdit ITDateDateEdit;
		ZDropEdit EntryModeDropEdit;
		internal ZCheckBox EnableENSCheckBox;
		internal ZCheckBox IsFinalWHSCheckBox;
		ZTextBox WHSEntryNumberTextBox;
		internal ZCodeFindBox WHSDistrictPortCodeFindBox;
		ZTextBox WHSEntryFilerCodeTextBox;
		internal ZCheckBox InvoiceByRequestCheckBox;
		ZDateEdit EstimatedEntryDateDateEdit;
		internal ZPanel WarehouseWithdrawalPanel;
		internal ZLabel WarehouseWithdrawalLabel;
		ZLabel DashLabel;
		ZPanel IORAuthAgentPanel;
		ZRadioButton AuthorizedAgentRadioButton;
		ZRadioButton IORRadioButton;
		ZDropEdit PurchasedDropEdit;
		ZCheckBox ManualEntryCheckBox;
		ZDropEdit LiveEntryIndicatorDropEdit;
		ZTextBox EntryFilerCodeTextBox;
		ZCheckBox US_EnableSPNCheckBox;
		ZPanel ExportShipmentTypePanel;
		ZDropEdit RoutedTransactionDropEdit;
		ZDropEdit TariffTypeDropEdit;
		protected internal ZDropEdit MergeByDropEdit;
		ZDropEdit HazardousMaterialDropEdit;
		ZDropEdit TransactionsRelatedDropEdit;
		ZPanel ExportAESTIRShipmentTypePanel;
		ZTextBox US_FirstPortOfCallCityTextBox;
		ZDropEdit US_SoldEnRouteIndicatorDropEdit;
		ZCodeFindBox US_RN_NKFirstPortOfCallCountryCodeFindBox;
		ZDropEdit US_CommodityFilingOptionDropEdit;
		ZDropEdit AESTIRRoutedTransactionDropEdit;
		ZDropEdit AESTIRTariffTypeDropEdit;
		ZDropEdit AESTIRHazardousMaterialDropEdit;
		ZDropEdit AESTIRTransactionsRelatedDropEdit;
		ZDropEdit SEDMessageStatusDropEdit;
		ZTextBox OriginalITNTextBox;
		internal ZCodeFindBox US_UC_NKCountryOfExportCodeFindBox;
		ZDateEdit US_DateOfExportDateEdit;
		internal ZCalcEdit QtyInWHAfterWithdrawalCalcEdit;
		internal ZCalcEdit QtyBeingWithdrawnCalcEdit;
		internal ZCalcEdit QtyInWhBeforeWithdrawalCalcEdit;
		internal ZDropEdit ApplicationCodeDropEdit;
		ZDropEdit CargoReleaseTypeDropEdit;
		internal ZCheckBox PSCCheckBox;
		ZLabel EntryNumberDividerLabel;
		internal ZTextBox JourneyTextBox;
		internal ZTextBox CarrierNameTextBox;
		internal ZCheckBox AccLiqCheckBox;
		internal ZPanel TIBPanel;
		ZTextBox TIBPurposeTextBox;
		internal ZDropEdit TIBMotorVehiclesDropEdit;
		internal ZCheckBox TIBMVNonConformingCheckBox;
		ZPanel FTZPanel;
		ZDropEdit FTZAdmissionTypeDropEdit;
		ZCheckBox DirectDeliveryIndicatorCheckBox;
		ZTextBox FTZControlNumberTextBox;
		ZTextBox FTZYearTextBox;
		ZDropEdit DeliveryDropEdit;
		internal ZCodeFindBox US_RN_NKCountryOfDestinationCodeFindBox;
		ZCheckBox FTZSPNCheckBox;
		ZTextBox CalcTransportationModeTextBox;
		internal ZPanel SEReleasePanel;
		internal ZPanel SEReleaseAndStandAlonePriorNoticePanel;
		ZDropEdit SplitShipmentReleaseCodeDropEdit;
		internal ZDropEdit SplitShipmentReleaseCodeForSPNDropEdit;
		ZDropEdit SPNIDTypeDropEdit;
		internal ZCheckBox US_MonthlyFilingCheckBox;
		ZCheckBox IncludePTTCheckBox;
		ZDropEdit ZoneIDDropDownEdit;
		ZCheckBox US_PGAExpeditedReleaseCheckBox;
		internal ZButton AllocateButton;
		internal ZTextBox PipelineNameTextBox;
		internal ZTextBox BatchTicketTextBox;
		ZCheckBox PTTWithoutExceptionCheckBox;
		internal ZCheckBox NonAMSCheckBox;
		JobTransportOrgsControl TransportOrgsControl;
		internal ZTabPage USOrganisationsTabPage;
		internal JobMainOrgsControl JobMainOrgsControl;
		ZCheckBox US_ImmediateDeliveryCheckBox;
		internal ZCheckBox ConsolidatedSummaryCheckBox;
		internal JobMiscOrgsControl JobMiscOrgsControl;
		internal ZDropEdit FTZSPNIDTypeDropEdit;
		internal ZCheckBox ExpressTrackingCheckBox;
		ZTextBox ExpressTrackingNumberTextBox;
		internal ZTextBox FTZTextBox;

		#endregion
	}
}
