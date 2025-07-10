namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressesUserControl
	{

		#region Component Designer generated code

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer OrgAddressSplitContainer;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DeliveryGroupBox;
		private CargoWise.Windows.UI.KTableLayoutPanel AddressesMainLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl ExtraDetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage LoadingConstraintsTabPage;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_CommunicationRequiredDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_LabourRequiredDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_AccessPointDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_ContainerHandlingDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_Dock_HeightDropEdit;
		private Enterprise.ZArchitecture.ZTextBox OA_LoadingUnloadingConstraintsTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage WarehousingTabPage;
		private Enterprise.ZArchitecture.ZTextBox OA_OtherWarehouseFacilitiesTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OA_PalletJackCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OA_ForkLiftCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OA_DockLevelerCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OA_VerifiesContainerGrossWeightCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox AddressDetailsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox RelatedPortFindBox;
		private Enterprise.ZArchitecture.ZTextBox OA_EmailTextBox;
		private PhoneNumberUserControl FaxNumberControl;
		private PhoneNumberUserControl MobilePhoneNumberControl;
		private PhoneNumberUserControl PhoneNumberControl;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OA_StateBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OA_LanguageBoundDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox OA_Address1BoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox OA_CityBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox OA_PostCodeBoundTextBox;
		protected Enterprise.ZArchitecture.ZTextBox OA_Address2BoundTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton ClearFieldsButton;
		protected Enterprise.ZArchitecture.ZTextBox OA_CompanyNameOverrideBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage CartageEquipmentTabPage;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_EXAirEquipmentNeededBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_EXLCLEquipmentNeededBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_EXFCLEquipmentNeededBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox RequiredCartageEquipmentGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AddressCapabilities;
		private Enterprise.ZArchitecture.GUI.ZTabPage PickupDeliveryAndAttendanceTimesTabControl;
		protected Enterprise.ZArchitecture.ZGrid TimetableGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TimetableGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton NotApplicableRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton DefaultRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton AdvancedRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton WeekdayRadioButton;
		private Enterprise.ZArchitecture.ZGrid AddressCapability;
		internal Enterprise.ZArchitecture.GUI.ZTabPage USKnownShipperTabPage;
		private Enterprise.ZArchitecture.ZTextBox OV_EXExportPermissionDetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OV_EXSiteInspectionDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OV_EXApprovedOrMajorExporterDropEdit;
		private Enterprise.ZArchitecture.ZTextBox OV_SystemLastEditUserTextBox;
		private Enterprise.ZArchitecture.ZTextBox OV_SystemCreateUserTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OV_SystemLastEditTimeDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OV_SystemCreateTimeDateEdit;
		private Enterprise.ZArchitecture.ZTextBox OV_EXApprovalNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox TSAE3NotificationSignedCheckbox;
		internal Enterprise.ZArchitecture.GUI.ZTabPage GPSTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CoordinatesGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit LongitudeCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LatitudeCalcEdit;
		protected Enterprise.ZArchitecture.ZLabel CustomsAddressSecurityLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AddressesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShowInactiveAddressesCheckBox;
		protected internal AddressZGrid OrgAddressBoundGrid;
		internal Enterprise.ZArchitecture.GUI.ZTabPage GenericKnownShipperTabPage;
		private Enterprise.ZArchitecture.ZTextBox JP_OV_SystemLastEditUserTextBox;
		private Enterprise.ZArchitecture.ZTextBox JP_OV_SystemCreateUserTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_OV_SystemLastEditTimeDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_OV_SystemCreateTimeDateEdit;
		private Enterprise.ZArchitecture.ZTextBox JP_OV_EXApprovalNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox JP_OV_EXExportPermissionDetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JP_OV_EXApprovalExpiryDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JP_OV_EXApprovedOrMajorExporterDropEdit;
		private System.ComponentModel.IContainer components;
		internal Enterprise.ZArchitecture.GUI.ZTabPage DeliveryRouteTabPage;
		private Enterprise.ZArchitecture.ZCalcEdit DeliveryRouteSequenceCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage FreeWaitingTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalAddressInfoTabPage;
		protected internal Enterprise.ZArchitecture.ZGrid OrgFreeWaitingTimeBoundGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UseCumulative;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OA_DeliveryRouteDropEdit;
		private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AuthorityToLeaveDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton ValidateAddressButton;
		protected Enterprise.ZArchitecture.GUI.ZButton RemoveLocalAddressButton_Exposed;
		protected Enterprise.ZArchitecture.GUI.ZButton AddLocalAddressButton;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit SelectLocalAddressDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit OA_JobLoadingDurationCalcEdit;
		private Enterprise.ZArchitecture.ZLabel ServiceDurationLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsOverrideJobLoadingDurationCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZUserControl AddressAdditionalInfoUserControl;
		protected Enterprise.ZArchitecture.GUI.ZUserControl TranslatedAddressAdditionalInfoUserControl;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OrgAddressSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShowInactiveAddressesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgAddressBoundGrid = new AddressZGrid();
			this.AddressCapabilities = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressCapability = new Enterprise.ZArchitecture.ZGrid();
			this.AddressesMainLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AddressDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemoveLocalAddressButton_Exposed = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddLocalAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectLocalAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CustomsAddressSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RelatedPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OA_EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.OA_StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_LanguageBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OA_CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_Address2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_CompanyNameOverrideBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExtraDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LoadingConstraintsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IsOverrideJobLoadingDurationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OA_JobLoadingDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ServiceDurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OA_CommunicationRequiredDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_LabourRequiredDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_AccessPointDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_ContainerHandlingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_Dock_HeightDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_LoadingUnloadingConstraintsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WarehousingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OA_OtherWarehouseFacilitiesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_PalletJackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OA_ForkLiftCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OA_DockLevelerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OA_VerifiesContainerGrossWeightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CartageEquipmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RequiredCartageEquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_EXFCLEquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_EXAirEquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_EXLCLEquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PickupDeliveryAndAttendanceTimesTabControl = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TimetableGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TimetableGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefaultRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WeekdayRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AdvancedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NotApplicableRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DeliveryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorityToLeaveDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.USKnownShipperTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TSAE3NotificationSignedCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OV_SystemLastEditUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OV_SystemCreateUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OV_SystemLastEditTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OV_SystemCreateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OV_EXApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OV_EXExportPermissionDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OV_EXSiteInspectionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OV_EXApprovedOrMajorExporterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GenericKnownShipperTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JP_OV_SystemLastEditUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JP_OV_SystemCreateUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JP_OV_SystemLastEditTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_OV_SystemCreateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_OV_EXApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JP_OV_EXExportPermissionDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JP_OV_EXApprovalExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_OV_EXApprovedOrMajorExporterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GPSTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CoordinatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LatitudeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LongitudeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeliveryRouteTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OA_DeliveryRouteDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryRouteSequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FreeWaitingTabControl = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalAddressInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TranslatedAddressAdditionalInfoUserControl = new OrgTranslatedAdressAdditionalInfoUserControl();
			this.AddressAdditionalInfoUserControl = new OrgAddressAdditionalInfoUserControl();
			this.OrgFreeWaitingTimeBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UseCumulative = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.OrgAddressSplitContainer.SuspendLayout();
			this.AddressesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgAddressBoundGrid)).BeginInit();
			this.OrgAddressBoundGrid.SuspendLayout();
			this.AddressCapabilities.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressCapability)).BeginInit();
			this.AddressCapability.SuspendLayout();
			this.AddressesMainLayoutPanel.SuspendLayout();
			this.AddressDetailsGroupBox.SuspendLayout();
			this.SelectLocalAddressDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.RelatedPortFindBox.SuspendLayout();
			this.FaxNumberControl.SuspendLayout();
			this.MobilePhoneNumberControl.SuspendLayout();
			this.PhoneNumberControl.SuspendLayout();
			this.OA_StateBoundDropEdit.SuspendLayout();
			this.OA_LanguageBoundDropEdit.SuspendLayout();
			this.ExtraDetailsTabControl.SuspendLayout();
			this.LoadingConstraintsTabPage.SuspendLayout();
			this.OA_CommunicationRequiredDropEdit.SuspendLayout();
			this.OA_LabourRequiredDropEdit.SuspendLayout();
			this.OA_AccessPointDropEdit.SuspendLayout();
			this.OA_ContainerHandlingDropEdit.SuspendLayout();
			this.OA_Dock_HeightDropEdit.SuspendLayout();
			this.WarehousingTabPage.SuspendLayout();
			this.CartageEquipmentTabPage.SuspendLayout();
			this.RequiredCartageEquipmentGroupBox.SuspendLayout();
			this.OM_EXFCLEquipmentNeededBoundDropEdit.SuspendLayout();
			this.OM_EXAirEquipmentNeededBoundDropEdit.SuspendLayout();
			this.OM_EXLCLEquipmentNeededBoundDropEdit.SuspendLayout();
			this.PickupDeliveryAndAttendanceTimesTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TimetableGrid)).BeginInit();
			this.TimetableGroupBox.SuspendLayout();
			this.AuthorityToLeaveDropEdit.SuspendLayout();
			this.USKnownShipperTabPage.SuspendLayout();
			this.OV_SystemLastEditTimeDateEdit.SuspendLayout();
			this.OV_SystemCreateTimeDateEdit.SuspendLayout();
			this.OV_EXSiteInspectionDateDateEdit.SuspendLayout();
			this.OV_EXApprovedOrMajorExporterDropEdit.SuspendLayout();
			this.GenericKnownShipperTabPage.SuspendLayout();
			this.JP_OV_SystemLastEditTimeDateEdit.SuspendLayout();
			this.JP_OV_SystemCreateTimeDateEdit.SuspendLayout();
			this.JP_OV_EXApprovalExpiryDateEdit.SuspendLayout();
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.SuspendLayout();
			this.GPSTabPage.SuspendLayout();
			this.CoordinatesGroupBox.SuspendLayout();
			this.DeliveryRouteTabPage.SuspendLayout();
			this.OA_DeliveryRouteDropEdit.SuspendLayout();
			this.FreeWaitingTabControl.SuspendLayout();
			this.AdditionalAddressInfoTabPage.SuspendLayout();
			this.AddressAdditionalInfoUserControl.SuspendLayout();
			this.TranslatedAddressAdditionalInfoUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgFreeWaitingTimeBoundGrid)).BeginInit();
			this.OrgFreeWaitingTimeBoundGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.OrgAddressSplitContainer);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.AddressesMainLayoutPanel);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 580, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(211);
			this.MainSplitContainer.TabIndex = 20;
			// 
			// OrgAddressTableLayoutPanel
			// 
			this.OrgAddressSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrgAddressSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.OrgAddressSplitContainer.Panel1.Controls.Add(this.AddressesGroupBox);
			this.OrgAddressSplitContainer.Panel2.Controls.Add(this.AddressCapabilities);
			this.OrgAddressSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.OrgAddressSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgAddressSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 188, true);
			this.OrgAddressSplitContainer.Name = "OrgAddressSplitContainer";
			this.OrgAddressSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			this.OrgAddressSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 211, true);
			this.OrgAddressSplitContainer.TabIndex = 0;
			// 
			// OrgAddressSplitContainer.Panel2
			// 
			this.OrgAddressSplitContainer.Panel2.Controls.Add(this.AddressCapabilities);
			this.OrgAddressSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 211, true);
			this.OrgAddressSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			this.OrgAddressSplitContainer.TabIndex = 0;
			// 
			// AddressesGroupBox
			// 
			this.AddressesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|47c719c3-a9cb-44f6-b004-6c0a40942f3d", "Addresses");
			this.AddressesGroupBox.Controls.Add(this.OrgAddressBoundGrid);
			this.AddressesGroupBox.Controls.Add(this.ShowInactiveAddressesCheckBox);
			this.AddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddressesGroupBox.Name = "AddressesGroupBox";
			this.AddressesGroupBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			this.AddressesGroupBox.TabIndex = 0;
			this.AddressesGroupBox.TabStop = false;
			// 
			// ShowInactiveAddressesCheckBox
			// 
			this.ShowInactiveAddressesCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BindingSource.SetBindingMember(this.ShowInactiveAddressesCheckBox, "IncludeInactiveAddresses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).IncludeInactiveAddresses)));
			this.ShowInactiveAddressesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|33e604e7-d450-4a47-8341-5989d53af036", "Show Inactive", "Show Inactive Addresses. Inactive addresses with errors or unsaved contents will always be displayed until they are saved.");
			this.ShowInactiveAddressesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowInactiveAddressesCheckBox.Name = "ShowInactiveAddressesCheckBox";
			this.ShowInactiveAddressesCheckBox.TabIndex = 2;
			this.ShowInactiveAddressesCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgAddressBoundGrid
			// 
			this.OrgAddressBoundGrid.AllowNavigation = false;
			this.OrgAddressBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BindingSource.SetBindingMember(this.OrgAddressBoundGrid, "ActiveOrAllAddresses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_CompanyNameOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_AdditionalAddressInformation)));
			this.OrgAddressBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "OA_Address1";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|e3213f2a-47a9-4354-8565-f5f56810b0d4", "Address Short Code");
			zTextBoxColumnStyleInfo10.ColumnName = "OA_Code";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo11.ColumnName = "OA_Address2";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo12.ColumnName = "OA_City";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.ColumnName = "OA_PostCode";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo14.ColumnName = "OA_State";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|19db6e54-3c5f-4c3d-94b0-35051562c346", "Company Name Override");
			zTextBoxColumnStyleInfo15.ColumnName = "OA_CompanyNameOverride";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo3.ColumnName = "OA_Language";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "OA_IsActive";
			zCheckBoxColumnStyleInfo4.ToolTip = "Is the selected address still a valid address?";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgAddressBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.OrgAddressBoundGrid.CopySelectedRowsAllowed = true;
			this.OrgAddressBoundGrid.GridId = "53823044-9e95-4887-a623-a4438645c7ed";
			this.OrgAddressBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgAddressBoundGrid.LayoutKey = "OrgAddressBoundGrid";
			this.OrgAddressBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrgAddressBoundGrid.Name = "OrgAddressBoundGrid";
			this.OrgAddressBoundGrid.TabIndex = 1;
			// 
			// AddressCapabilities
			// 
			this.AddressCapabilities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressCapabilities.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|bd75fcac-9583-4e95-bac5-241760cd00fb", "Address Capabilities");
			this.AddressCapabilities.Controls.Add(this.AddressCapability);
			this.AddressCapabilities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 3, true);
			this.AddressCapabilities.Name = "AddressCapabilities";
			this.AddressCapabilities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 205, true);
			this.AddressCapabilities.TabIndex = 2;
			this.AddressCapabilities.TabStop = false;
			// 
			// AddressCapability
			// 
			this.AddressCapability.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressCapability, "ActiveOrAllAddresses.AddressCapability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AddressCapability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddressCapabilityWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AddressCapability)).SyncRoot)).Enabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddressCapabilityWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AddressCapability)).SyncRoot)).AddressTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddressCapabilityWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AddressCapability)).SyncRoot)).Main)));
			this.AddressCapability.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|30afcdf5-2b06-42da-bbdc-0eddc6914dd9", "Has");
			zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|a5ff31e4-6d1b-41bf-b58d-dc2ecff3cf38", "Capability");
			zTextBoxColumnStyleInfo1.ColumnName = "AddressTypeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|d921814d-7942-4b7d-bfda-c19bf251dca0", "Main");
			zCheckBoxColumnStyleInfo5.ColumnName = "Main";
			zCheckBoxColumnStyleInfo5.IsMandatory = true;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.AddressCapability.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AddressCapability.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressCapability.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.AddressCapability.CopySelectedRowsAllowed = true;
			this.AddressCapability.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressCapability.GridId = "c28fe8a7-9267-4912-a777-790114902b53";
			this.AddressCapability.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressCapability.LayoutKey = "zGrid1";
			this.AddressCapability.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddressCapability.Name = "AddressCapability";
			this.AddressCapability.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AddressCapability.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 186, true);
			this.AddressCapability.TabIndex = 0;
			// 
			// AddressesMainLayoutPanel
			// 
			this.AddressesMainLayoutPanel.AutoScroll = true;
			this.AddressesMainLayoutPanel.ColumnCount = 2;
			this.AddressesMainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.AddressesMainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
			this.AddressesMainLayoutPanel.Controls.Add(this.AddressDetailsGroupBox, 0, 0);
			this.AddressesMainLayoutPanel.Controls.Add(this.ExtraDetailsTabControl, 1, 0);
			this.AddressesMainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesMainLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressesMainLayoutPanel.Name = "AddressesMainLayoutPanel";
			this.AddressesMainLayoutPanel.RowCount = 1;
			this.AddressesMainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.AddressesMainLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 350, true);
			this.AddressesMainLayoutPanel.TabIndex = 0;
			// 
			// AddressDetailsGroupBox
			// 
			this.AddressDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|1fc67e37-77cf-4fa3-8701-236687e34405", "Address Details");
			this.AddressDetailsGroupBox.Controls.Add(this.RemoveLocalAddressButton_Exposed);
			this.AddressDetailsGroupBox.Controls.Add(this.AddLocalAddressButton);
			this.AddressDetailsGroupBox.Controls.Add(this.SelectLocalAddressDropEdit);
			this.AddressDetailsGroupBox.Controls.Add(this.CountryFindBox);
			this.AddressDetailsGroupBox.Controls.Add(this.ValidateAddressButton);
			this.AddressDetailsGroupBox.Controls.Add(this.CustomsAddressSecurityLabel);
			this.AddressDetailsGroupBox.Controls.Add(this.RelatedPortFindBox);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_EmailTextBox);
			this.AddressDetailsGroupBox.Controls.Add(this.FaxNumberControl);
			this.AddressDetailsGroupBox.Controls.Add(this.MobilePhoneNumberControl);
			this.AddressDetailsGroupBox.Controls.Add(this.PhoneNumberControl);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_StateBoundDropEdit);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_LanguageBoundDropEdit);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_Address1BoundTextBox);
			this.AddressDetailsGroupBox.Controls.Add(this.ClearFieldsButton);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_CityBoundTextBox);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_PostCodeBoundTextBox);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_Address2BoundTextBox);
			this.AddressDetailsGroupBox.Controls.Add(this.OA_CompanyNameOverrideBoundTextBox);
			this.AddressDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddressDetailsGroupBox.Name = "AddressDetailsGroupBox";
			this.AddressDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 350, true);
			this.AddressDetailsGroupBox.TabIndex = 0;
			this.AddressDetailsGroupBox.TabStop = false;
			// 
			// RemoveLocalAddressButton
			// 
			this.RemoveLocalAddressButton_Exposed.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|79D5A28E-1A19-4203-A348-63F9498226A1", "Remove");
			this.RemoveLocalAddressButton_Exposed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 17, true);
			this.RemoveLocalAddressButton_Exposed.Name = "RemoveLocalAddressButton";
			this.RemoveLocalAddressButton_Exposed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 22, true);
			this.RemoveLocalAddressButton_Exposed.TabIndex = 2;
			this.RemoveLocalAddressButton_Exposed.UseVisualStyleBackColor = true;
			this.RemoveLocalAddressButton_Exposed.Click += RemoveLocalAddressButton_Click;
			// 
			// AddLocalAddressButton
			// 
			this.AddLocalAddressButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|A6677568-D0FC-43D4-9E02-D399567CD2C7", "New Translated Address");
			this.AddLocalAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 17, true);
			this.AddLocalAddressButton.Name = "AddLocalAddressButton";
			this.AddLocalAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 22, true);
			this.AddLocalAddressButton.TabIndex = 1;
			this.AddLocalAddressButton.UseVisualStyleBackColor = true;
			this.AddLocalAddressButton.Click += AddLocalAddressButton_Click;
			// 
			// SelectLocalAddressDropEdit
			// 
			this.SelectLocalAddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectLocalAddressDropEdit, "ActiveOrAllAddresses.LocalAddressDisplayText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).LocalAddressDisplayText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AddressLanguagePack)));
			this.SelectLocalAddressDropEdit.BindToList = "ActiveOrAllAddresses.AddressLanguagePack";
			this.SelectLocalAddressDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|93990B12-47C9-4BFD-A2DE-EB0B5A9395BF", "Selected Address");
			this.SelectLocalAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.SelectLocalAddressDropEdit.Name = "SelectLocalAddressDropEdit";
			this.SelectLocalAddressDropEdit.PreBoundMaxLength = 25;
			this.SelectLocalAddressDropEdit.ShowDescriptionBox = false;
			this.SelectLocalAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.SelectLocalAddressDropEdit.TabIndex = 0;
			this.SelectLocalAddressDropEdit.BoundValueCommitted += SelectLocalAddressDropEdit_BoundValueCommitted;
			this.SelectLocalAddressDropEdit.Validated += SelectLocalAddressDropEdit_Validated;
			this.SelectLocalAddressDropEdit.EditableInViewMode = true;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "ActiveOrAllAddresses.OA_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 127, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 9;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 79, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 7;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// CustomsAddressSecurityLabel
			// 
			this.CustomsAddressSecurityLabel.AutoSize = true;
			this.CustomsAddressSecurityLabel.ForeColor = System.Drawing.Color.Red;
			this.CustomsAddressSecurityLabel.IsFontBold = true;
			this.CustomsAddressSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 0, true);
			this.CustomsAddressSecurityLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 0, true);
			this.CustomsAddressSecurityLabel.Name = "CustomsAddressSecurityLabel";
			this.CustomsAddressSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 13, true);
			this.CustomsAddressSecurityLabel.TabIndex = 22;
			this.CustomsAddressSecurityLabel.Visible = false;
			// 
			// RelatedPortFindBox
			// 
			this.RelatedPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedPortFindBox, "ActiveOrAllAddresses.OA_RL_NKRelatedPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_RL_NKRelatedPortCode)));
			this.RelatedPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 175, true);
			this.RelatedPortFindBox.Name = "RelatedPortFindBox";
			this.RelatedPortFindBox.ShowDescriptionBox = false;
			this.RelatedPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.RelatedPortFindBox.TabIndex = 13;
			// 
			// OA_EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_EmailTextBox, "ActiveOrAllAddresses.OA_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Email)));
			this.OA_EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 273, true);
			this.OA_EmailTextBox.Name = "OA_EmailTextBox";
			this.OA_EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_EmailTextBox.TabIndex = 17;
			// 
			// FaxNumberControl
			// 
			this.FaxNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FaxNumberControl, "ActiveOrAllAddresses.FaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FaxNumber)));
			this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|OA_Fax_Formatted", "Fax");
			this.FaxNumberControl.EnableValidStateColor = true;
			this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 248, true);
			this.FaxNumberControl.Name = "FaxNumberControl";
			this.FaxNumberControl.ShowDiallerControl = false;
			this.FaxNumberControl.ShowLocalNumberLabel = false;
			this.FaxNumberControl.ShowPublishedCheckBox = false;
			this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.FaxNumberControl.TabIndex = 16;
			// 
			// MobilePhoneNumberControl
			// 
			this.MobilePhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "ActiveOrAllAddresses.MobilePhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).MobilePhoneNumber)));
			this.MobilePhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|OA_Mobile_Formatted", "Mobile");
			this.MobilePhoneNumberControl.EnableValidStateColor = true;
			this.MobilePhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 225, true);
			this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
			this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
			this.MobilePhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.MobilePhoneNumberControl.TabIndex = 15;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "ActiveOrAllAddresses.PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).PhoneNumber)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|OA_Phone_Formatted", "Phone");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 200, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.PhoneNumberControl.TabIndex = 14;
			// 
			// OA_StateBoundDropEdit
			// 
			this.OA_StateBoundDropEdit.AllowDrop = true;
			this.OA_StateBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|92B57D2C-20BB-4D1C-AE19-FF080D78B6EB", "State");
			this.BindingSource.SetBindingMember(this.OA_StateBoundDropEdit, "ActiveOrAllAddresses.SelectedTranslatedAddress+StateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_State)));
			this.OA_StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 150, true);
			this.OA_StateBoundDropEdit.Name = "OA_StateBoundDropEdit";
			this.OA_StateBoundDropEdit.PreBoundMaxLength = 25;
			this.OA_StateBoundDropEdit.ShowDescriptionBox = false;
			this.OA_StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.OA_StateBoundDropEdit.TabIndex = 12;
			// 
			// OA_LanguageBoundDropEdit
			// 
			this.OA_LanguageBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_LanguageBoundDropEdit, "ActiveOrAllAddresses.SelectedTranslatedAddress+Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.Language)));
			this.OA_LanguageBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|729EF047-B107-4365-A7F6-2D532A442823", "Language");
			this.OA_LanguageBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 298, true);
			this.OA_LanguageBoundDropEdit.Name = "OA_LanguageBoundDropEdit";
			this.OA_LanguageBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_LanguageBoundDropEdit.TabIndex = 18;
			OA_LanguageBoundDropEdit.SelectedIndexChanged += OA_LanguageBoundDropEdit_SelectedIndexChanged;
			// 
			// OA_Address1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_Address1BoundTextBox, "ActiveOrAllAddresses.SelectedTranslatedAddress+Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.Address1)));
			this.OA_Address1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|7133F41E-F0AD-4EFB-B1F0-F99F649B40E3", "Address 1");
			this.OA_Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 79, true);
			this.OA_Address1BoundTextBox.Name = "OA_Address1BoundTextBox";
			this.OA_Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.OA_Address1BoundTextBox.TabIndex = 5;
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 79, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 6;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += ClearFieldsButton_Click;
			// 
			// OA_CityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_CityBoundTextBox, "ActiveOrAllAddresses.SelectedTranslatedAddress+City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.City)));
			this.OA_CityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|7211AFD9-2C22-44DA-99E7-F265CB494EFC", "City");
			this.OA_CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 127, true);
			this.OA_CityBoundTextBox.Name = "OA_CityBoundTextBox";
			this.OA_CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.OA_CityBoundTextBox.TabIndex = 11;
			// 
			// OA_PostCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_PostCodeBoundTextBox, "ActiveOrAllAddresses.SelectedTranslatedAddress+Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.Postcode)));
			this.OA_PostCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|221751C0-24A8-4510-B3C2-2877024C1767", "Postcode");
			this.OA_PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 150, true);
			this.OA_PostCodeBoundTextBox.Name = "OA_PostCodeBoundTextBox";
			this.OA_PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OA_PostCodeBoundTextBox.TabIndex = 10;
			// 
			// OA_Address2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_Address2BoundTextBox, "ActiveOrAllAddresses.SelectedTranslatedAddress+Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.Address2)));
			this.OA_Address2BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|E3E4DEE6-5621-4896-8179-68DD630CE9A8", "Address 2");
			this.OA_Address2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 103, true);
			this.OA_Address2BoundTextBox.Name = "OA_Address2BoundTextBox";
			this.OA_Address2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_Address2BoundTextBox.TabIndex = 8;
			// 
			// OA_CompanyNameOverrideBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_CompanyNameOverrideBoundTextBox, "ActiveOrAllAddresses.SelectedTranslatedAddress+CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).SelectedTranslatedAddress.CompanyName)));
			this.OA_CompanyNameOverrideBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|06DE595A-E081-4162-B55E-8FE4C29B8072", "Company Name");
			this.OA_CompanyNameOverrideBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 52, true);
			this.OA_CompanyNameOverrideBoundTextBox.Name = "OA_CompanyNameOverrideBoundTextBox";
			this.OA_CompanyNameOverrideBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_CompanyNameOverrideBoundTextBox.TabIndex = 3;
			// 
			// ExtraDetailsTabControl
			// 
			this.ExtraDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExtraDetailsTabControl.Controls.Add(this.LoadingConstraintsTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.WarehousingTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.CartageEquipmentTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.PickupDeliveryAndAttendanceTimesTabControl);
			this.ExtraDetailsTabControl.Controls.Add(this.USKnownShipperTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.GenericKnownShipperTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.GPSTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.DeliveryRouteTabPage);
			this.ExtraDetailsTabControl.Controls.Add(this.FreeWaitingTabControl);
			this.ExtraDetailsTabControl.Controls.Add(this.AdditionalAddressInfoTabPage);
			this.ExtraDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtraDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 3, true);
			this.ExtraDetailsTabControl.Name = "ExtraDetailsTabControl";
			this.ExtraDetailsTabControl.SelectedIndex = 0;
			this.ExtraDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 329, true);
			this.ExtraDetailsTabControl.TabIndex = 1;
			// 
			// LoadingConstraintsTabPage
			// 
			this.LoadingConstraintsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|666709bd-7e28-4269-a622-f313adcf5a79", "Loading/Unloading Constraints");
			this.LoadingConstraintsTabPage.Controls.Add(this.IsOverrideJobLoadingDurationCheckBox);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_JobLoadingDurationCalcEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.ServiceDurationLabel);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_CommunicationRequiredDropEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_LabourRequiredDropEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_AccessPointDropEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_ContainerHandlingDropEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_Dock_HeightDropEdit);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_LoadingUnloadingConstraintsTextBox);
			this.LoadingConstraintsTabPage.Controls.Add(this.OA_VerifiesContainerGrossWeightCheckBox);
			this.LoadingConstraintsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LoadingConstraintsTabPage.Name = "LoadingConstraintsTabPage";
			this.LoadingConstraintsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.LoadingConstraintsTabPage.TabIndex = 1;
			// 
			// IsOverrideJobLoadingDurationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsOverrideJobLoadingDurationCheckBox, "ActiveOrAllAddresses.IsOverridenJobLoadingDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).IsOverridenJobLoadingDuration)));
			this.IsOverrideJobLoadingDurationCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("237e7096-bb21-4136-ad07-e0c5d1a02af7", "Override registry default");
			this.IsOverrideJobLoadingDurationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsOverrideJobLoadingDurationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsOverrideJobLoadingDurationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 98, true);
			this.IsOverrideJobLoadingDurationCheckBox.Name = "IsOverrideJobLoadingDurationCheckBox";
			this.IsOverrideJobLoadingDurationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.IsOverrideJobLoadingDurationCheckBox.TabIndex = 7;
			// 
			// OA_JobLoadingDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OA_JobLoadingDurationCalcEdit, "ActiveOrAllAddresses.OA_JobLoadingDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_JobLoadingDuration)));
			this.OA_JobLoadingDurationCalcEdit.DecimalPlaces = 0;
			this.OA_JobLoadingDurationCalcEdit.Decimals = 0;
			this.OA_JobLoadingDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 95, true);
			this.OA_JobLoadingDurationCalcEdit.Name = "OA_JobLoadingDurationCalcEdit";
			this.OA_JobLoadingDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.OA_JobLoadingDurationCalcEdit.TabIndex = 8;
			// 
			// ServiceDurationLabel
			// 
			this.ServiceDurationLabel.AutoSize = true;
			this.ServiceDurationLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("78b48fff-6347-46a6-914b-e92d7e35bbe2", "Service Duration (Minutes):");
			this.ServiceDurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceDurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 80, true);
			this.ServiceDurationLabel.Name = "ServiceDurationLabel";
			this.ServiceDurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 13, true);
			this.ServiceDurationLabel.TabIndex = 6;
			// 
			// OA_CommunicationRequiredDropEdit
			// 
			this.OA_CommunicationRequiredDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_CommunicationRequiredDropEdit, "ActiveOrAllAddresses.OA_CommunicationRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_CommunicationRequired)));
			this.OA_CommunicationRequiredDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.OA_CommunicationRequiredDropEdit.Name = "OA_CommunicationRequiredDropEdit";
			this.OA_CommunicationRequiredDropEdit.PreBoundMaxLength = 3;
			this.OA_CommunicationRequiredDropEdit.ShowDescriptionBox = false;
			this.OA_CommunicationRequiredDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_CommunicationRequiredDropEdit.TabIndex = 2;
			// 
			// OA_LabourRequiredDropEdit
			// 
			this.OA_LabourRequiredDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_LabourRequiredDropEdit, "ActiveOrAllAddresses.OA_LabourRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_LabourRequired)));
			this.OA_LabourRequiredDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 32, true);
			this.OA_LabourRequiredDropEdit.Name = "OA_LabourRequiredDropEdit";
			this.OA_LabourRequiredDropEdit.PreBoundMaxLength = 3;
			this.OA_LabourRequiredDropEdit.ShowDescriptionBox = false;
			this.OA_LabourRequiredDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_LabourRequiredDropEdit.TabIndex = 3;
			// 
			// OA_AccessPointDropEdit
			// 
			this.OA_AccessPointDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_AccessPointDropEdit, "ActiveOrAllAddresses.OA_AccessPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_AccessPoint)));
			this.OA_AccessPointDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.OA_AccessPointDropEdit.Name = "OA_AccessPointDropEdit";
			this.OA_AccessPointDropEdit.PreBoundMaxLength = 3;
			this.OA_AccessPointDropEdit.ShowDescriptionBox = false;
			this.OA_AccessPointDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_AccessPointDropEdit.TabIndex = 0;
			// 
			// OA_ContainerHandlingDropEdit
			// 
			this.OA_ContainerHandlingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_ContainerHandlingDropEdit, "ActiveOrAllAddresses.OA_ContainerHandling");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_ContainerHandling)));
			this.OA_ContainerHandlingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 8, true);
			this.OA_ContainerHandlingDropEdit.Name = "OA_ContainerHandlingDropEdit";
			this.OA_ContainerHandlingDropEdit.PreBoundMaxLength = 3;
			this.OA_ContainerHandlingDropEdit.ShowDescriptionBox = false;
			this.OA_ContainerHandlingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_ContainerHandlingDropEdit.TabIndex = 1;
			// 
			// OA_Dock_HeightDropEdit
			// 
			this.OA_Dock_HeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_Dock_HeightDropEdit, "ActiveOrAllAddresses.OA_Dock_Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Dock_Height)));
			this.OA_Dock_HeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			this.OA_Dock_HeightDropEdit.Name = "OA_Dock_HeightDropEdit";
			this.OA_Dock_HeightDropEdit.PreBoundMaxLength = 3;
			this.OA_Dock_HeightDropEdit.ShowDescriptionBox = false;
			this.OA_Dock_HeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_Dock_HeightDropEdit.TabIndex = 4;
			// 
			// OA_LoadingUnloadingConstraintsTextBox
			// 
			this.OA_LoadingUnloadingConstraintsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OA_LoadingUnloadingConstraintsTextBox, "ActiveOrAllAddresses.OA_LoadingUnloadingConstraints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_LoadingUnloadingConstraints)));
			this.OA_LoadingUnloadingConstraintsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|f955f5f5-2aa3-4089-92c6-867069a54bf5", "Further Constraints");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OA_LoadingUnloadingConstraintsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OA_LoadingUnloadingConstraintsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 131, true);
			this.OA_LoadingUnloadingConstraintsTextBox.Multiline = true;
			this.OA_LoadingUnloadingConstraintsTextBox.Name = "OA_LoadingUnloadingConstraintsTextBox";
			this.OA_LoadingUnloadingConstraintsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OA_LoadingUnloadingConstraintsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 150, true);
			this.OA_LoadingUnloadingConstraintsTextBox.TabIndex = 9;
			// 
			// OA_VerifiesContainerGrossWeightCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OA_VerifiesContainerGrossWeightCheckBox, "ActiveOrAllAddresses.OA_VerifiesContainerGrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_VerifiesContainerGrossWeight)));
			this.OA_VerifiesContainerGrossWeightCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|a95f5bd6-fb4e-49a5-a7e2-dca3cbd9d410", "Reports Own VGM");
			this.OA_VerifiesContainerGrossWeightCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OA_VerifiesContainerGrossWeightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OA_VerifiesContainerGrossWeightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 56, true);
			this.OA_VerifiesContainerGrossWeightCheckBox.Name = "OA_VerifiesContainerGrossWeightCheckBox";
			this.OA_VerifiesContainerGrossWeightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
			this.OA_VerifiesContainerGrossWeightCheckBox.TabIndex = 5;
			// 
			// WarehousingTabPage
			// 
			this.WarehousingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|1ece812f-b72e-40aa-bbb0-429306949a69", "Warehouse Equipment");
			this.WarehousingTabPage.Controls.Add(this.OA_OtherWarehouseFacilitiesTextBox);
			this.WarehousingTabPage.Controls.Add(this.OA_PalletJackCheckBox);
			this.WarehousingTabPage.Controls.Add(this.OA_ForkLiftCheckBox);
			this.WarehousingTabPage.Controls.Add(this.OA_DockLevelerCheckBox);
			this.WarehousingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WarehousingTabPage.Name = "WarehousingTabPage";
			this.WarehousingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.WarehousingTabPage.TabIndex = 0;
			// 
			// OA_OtherWarehouseFacilitiesTextBox
			// 
			this.OA_OtherWarehouseFacilitiesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OA_OtherWarehouseFacilitiesTextBox, "ActiveOrAllAddresses.OA_OtherWarehouseFacilities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_OtherWarehouseFacilities)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OA_OtherWarehouseFacilitiesTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OA_OtherWarehouseFacilitiesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.OA_OtherWarehouseFacilitiesTextBox.Multiline = true;
			this.OA_OtherWarehouseFacilitiesTextBox.Name = "OA_OtherWarehouseFacilitiesTextBox";
			this.OA_OtherWarehouseFacilitiesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OA_OtherWarehouseFacilitiesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 243, true);
			this.OA_OtherWarehouseFacilitiesTextBox.TabIndex = 3;
			// 
			// OA_PalletJackCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OA_PalletJackCheckBox, "ActiveOrAllAddresses.OA_PalletJack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_PalletJack)));
			this.OA_PalletJackCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|514a71cd-5a3a-48f3-9ffa-8df7866a728c", "Has Pallet Jack");
			this.OA_PalletJackCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OA_PalletJackCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OA_PalletJackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 8, true);
			this.OA_PalletJackCheckBox.Name = "OA_PalletJackCheckBox";
			this.OA_PalletJackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 24, true);
			this.OA_PalletJackCheckBox.TabIndex = 1;
			this.OA_PalletJackCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// OA_ForkLiftCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OA_ForkLiftCheckBox, "ActiveOrAllAddresses.OA_ForkLift");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_ForkLift)));
			this.OA_ForkLiftCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|ef314ef7-3dbe-4f64-91ef-45321064ac24", "Has Forklift");
			this.OA_ForkLiftCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OA_ForkLiftCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OA_ForkLiftCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 8, true);
			this.OA_ForkLiftCheckBox.Name = "OA_ForkLiftCheckBox";
			this.OA_ForkLiftCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 24, true);
			this.OA_ForkLiftCheckBox.TabIndex = 2;
			this.OA_ForkLiftCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// OA_DockLevelerCheckBox
			// 
			this.OA_DockLevelerCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.OA_DockLevelerCheckBox, "ActiveOrAllAddresses.OA_DockLeveler");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_DockLeveler)));
			this.OA_DockLevelerCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|c3259c0a-c98a-4d01-a0c0-a65ed9577ee0", "Has Dock Leveler");
			this.OA_DockLevelerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OA_DockLevelerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OA_DockLevelerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OA_DockLevelerCheckBox.Name = "OA_DockLevelerCheckBox";
			this.OA_DockLevelerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.OA_DockLevelerCheckBox.TabIndex = 0;
			this.OA_DockLevelerCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OA_DockLevelerCheckBox.UseVisualStyleBackColor = false;
			// 
			// CartageEquipmentTabPage
			// 
			this.CartageEquipmentTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|79c5ad61-88ac-43bb-971b-4000a35c987a", "Drop Mode", "Port Transport Drop Mode", "");
			this.CartageEquipmentTabPage.Controls.Add(this.RequiredCartageEquipmentGroupBox);
			this.CartageEquipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CartageEquipmentTabPage.Name = "CartageEquipmentTabPage";
			this.CartageEquipmentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CartageEquipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.CartageEquipmentTabPage.TabIndex = 2;
			// 
			// RequiredCartageEquipmentGroupBox
			// 
			this.RequiredCartageEquipmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|26fe5e44-8a05-451b-b958-27bc25cadadd", "Drop Modes", "Port Transport Drop Modes", "");
			this.RequiredCartageEquipmentGroupBox.Controls.Add(this.OM_EXFCLEquipmentNeededBoundDropEdit);
			this.RequiredCartageEquipmentGroupBox.Controls.Add(this.OM_EXAirEquipmentNeededBoundDropEdit);
			this.RequiredCartageEquipmentGroupBox.Controls.Add(this.OM_EXLCLEquipmentNeededBoundDropEdit);
			this.RequiredCartageEquipmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequiredCartageEquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.RequiredCartageEquipmentGroupBox.Name = "RequiredCartageEquipmentGroupBox";
			this.RequiredCartageEquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 292, true);
			this.RequiredCartageEquipmentGroupBox.TabIndex = 11;
			this.RequiredCartageEquipmentGroupBox.TabStop = false;
			// 
			// OM_EXFCLEquipmentNeededBoundDropEdit
			// 
			this.OM_EXFCLEquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_EXFCLEquipmentNeededBoundDropEdit, "ActiveOrAllAddresses.OA_FCLEquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_FCLEquipmentNeeded)));
			this.OM_EXFCLEquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 75, true);
			this.OM_EXFCLEquipmentNeededBoundDropEdit.Name = "OM_EXFCLEquipmentNeededBoundDropEdit";
			this.OM_EXFCLEquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_EXFCLEquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.OM_EXFCLEquipmentNeededBoundDropEdit.TabIndex = 2;
			// 
			// OM_EXAirEquipmentNeededBoundDropEdit
			// 
			this.OM_EXAirEquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_EXAirEquipmentNeededBoundDropEdit, "ActiveOrAllAddresses.OA_AIREquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_AIREquipmentNeeded)));
			this.OM_EXAirEquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 23, true);
			this.OM_EXAirEquipmentNeededBoundDropEdit.Name = "OM_EXAirEquipmentNeededBoundDropEdit";
			this.OM_EXAirEquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_EXAirEquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.OM_EXAirEquipmentNeededBoundDropEdit.TabIndex = 0;
			// 
			// OM_EXLCLEquipmentNeededBoundDropEdit
			// 
			this.OM_EXLCLEquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_EXLCLEquipmentNeededBoundDropEdit, "ActiveOrAllAddresses.OA_LCLEquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_LCLEquipmentNeeded)));
			this.OM_EXLCLEquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 49, true);
			this.OM_EXLCLEquipmentNeededBoundDropEdit.Name = "OM_EXLCLEquipmentNeededBoundDropEdit";
			this.OM_EXLCLEquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_EXLCLEquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.OM_EXLCLEquipmentNeededBoundDropEdit.TabIndex = 1;
			// 
			// PickupDeliveryAndAttendanceTimesTabControl
			// 
			this.PickupDeliveryAndAttendanceTimesTabControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|ea2455a4-2220-43fe-b674-f037d5471bca", "Pickup && Delivery");
			this.PickupDeliveryAndAttendanceTimesTabControl.Controls.Add(this.DeliveryGroupBox);
			this.PickupDeliveryAndAttendanceTimesTabControl.Controls.Add(this.TimetableGroupBox);
			this.PickupDeliveryAndAttendanceTimesTabControl.Controls.Add(this.TimetableGrid);
			this.PickupDeliveryAndAttendanceTimesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PickupDeliveryAndAttendanceTimesTabControl.Name = "PickupDeliveryAndAttendanceTimesTabControl";
			this.PickupDeliveryAndAttendanceTimesTabControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PickupDeliveryAndAttendanceTimesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 600, true);
			this.PickupDeliveryAndAttendanceTimesTabControl.TabIndex = 3;
			// 
			// TimetableGroupBox
			// 
			this.TimetableGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|9f06815e-1b46-4ee7-8218-82e8d7b74a4b", "Pickup And Delivery Times");
			this.TimetableGroupBox.Controls.Add(this.DefaultRadioButton);
			this.TimetableGroupBox.Controls.Add(this.WeekdayRadioButton);
			this.TimetableGroupBox.Controls.Add(this.AdvancedRadioButton);
			this.TimetableGroupBox.Controls.Add(this.NotApplicableRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TimetableGroupBox, false);
			this.TimetableGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.TimetableGroupBox.Name = "TimetableGroupBox";
			this.TimetableGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 38, true);
			this.TimetableGroupBox.TabIndex = 3;
			this.TimetableGroupBox.TabStop = false;
			// 
			// DefaultRadioButton
			// 
			this.DefaultRadioButton.AutoCheck = false;
			this.DefaultRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.DefaultRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|F6C0BA0C-1764-4359-A24C-812733EEB9A8", "Default");
			this.DefaultRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DefaultRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.DefaultRadioButton.Name = "DefaultRadioButton";
			this.DefaultRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.DefaultRadioButton.TabIndex = 1;
			this.DefaultRadioButton.UseVisualStyleBackColor = false;
			//
			// WeekdayRadioButton
			//
			this.WeekdayRadioButton.AutoCheck = false;
			this.WeekdayRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.WeekdayRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|03348eb4-0131-4c4b-ae1e-bdd7ee558473", "Weekday");
			this.WeekdayRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WeekdayRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 16, true);
			this.WeekdayRadioButton.Name = "WeekdayRadioButton";
			this.WeekdayRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.WeekdayRadioButton.TabIndex = 2;
			this.WeekdayRadioButton.UseVisualStyleBackColor = false;
			//
			// AdvancedRadioButton
			//
			this.AdvancedRadioButton.AutoCheck = false;
			this.AdvancedRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.AdvancedRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|20c1e6a5-ed26-4ca6-997f-5c48dc73e474", "Advanced");
			this.AdvancedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AdvancedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 16, true);
			this.AdvancedRadioButton.Name = "AdvancedRadioButton";
			this.AdvancedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.AdvancedRadioButton.TabIndex = 4;
			this.AdvancedRadioButton.UseVisualStyleBackColor = false;
			// 
			// NotApplicableRadioButton
			// 
			this.NotApplicableRadioButton.AutoCheck = false;
			this.NotApplicableRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.NotApplicableRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|20c1e6a5-ed26-4ca6-997f-5c48wc73e474", "Not Applicable");
			this.NotApplicableRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NotApplicableRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 16, true);
			this.NotApplicableRadioButton.Name = "NotApplicableRadioButton";
			this.NotApplicableRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.NotApplicableRadioButton.TabIndex = 5;
			this.NotApplicableRadioButton.UseVisualStyleBackColor = false;
			// 
			//  
			// 
			this.TimetableGrid.AllowNavigation = false;
			this.TimetableGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TimetableGrid, "ActiveOrAllAddresses.Timetables");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).OTT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).OTT_TimeFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).OTT_TimeTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).OTT_CutOffTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Timetables)).SyncRoot)).ProcessingTimeInHours)));
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|ba250bed-c8d3-4414-8ded-b4a53c83a0ed", "Day");
			zDropEditColumnStyleInfo2.ColumnName = "DayOfWeek";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|d07dcae9-b22f-4b43-b980-e79dd1330c16", "Direction");
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.ColumnName = "OTT_Type";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|2d561856-cb6c-429b-bba3-31eac6a55ebe", "Direction Description");
			zTextBoxColumnStyleInfo16.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|5ec13363-907f-453e-b03e-89169285aa6b", "From");
			zDateEditColumnStyleInfo1.ColumnName = "OTT_TimeFrom";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|5d0433ee-d8db-4920-9642-46dbc4da4428", "To");
			zDateEditColumnStyleInfo2.ColumnName = "OTT_TimeTo";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|9018a3d6-0fbe-4ea9-b3da-f131f269f9c3", "Cut-Off Time");
			zDateEditColumnStyleInfo3.ColumnName = "OTT_CutOffTime";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zTimeEditExColumnStyleInfo13.ColumnName = "ProcessingTimeInHours";
			zTimeEditExColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|8b71b0cd-1caf-4670-85e1-fbb21fb20a96", "Processing Time (Hours)");
			zTimeEditExColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo13.AllowNegative = false;
			this.TimetableGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TimetableGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TimetableGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TimetableGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo13);
			this.TimetableGrid.CopySelectedRowsAllowed = true;
			this.TimetableGrid.GridId = "11322018-4279-46f8-9625-cd385b801b93";
			this.TimetableGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TimetableGrid.LayoutKey = "zGrid2";
			this.TimetableGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 120, true);
			this.TimetableGrid.Name = "TimetableGrid";
			this.TimetableGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 450, true);
			this.TimetableGrid.TabIndex = 4;
			this.TimetableGrid.CaptionVisible = false;
			this.TimetableGrid.AfterBind += TimetableGrid_AfterBind;
			// 
			// DeliveryGroupBox
			// 
			this.DeliveryGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|DC988153-51CD-4F86-99B0-C343625BD9D7", "Delivery");
			this.DeliveryGroupBox.Controls.Add(this.AuthorityToLeaveDropEdit);
			this.DeliveryGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.DeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.DeliveryGroupBox.Name = "DeliveryGroupBox";
			this.DeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 55, true);
			this.DeliveryGroupBox.TabIndex = 3;
			this.DeliveryGroupBox.TabStop = false;
			// 
			// AuthorityToLeaveDropEdit
			// 
			this.AuthorityToLeaveDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorityToLeaveDropEdit, "ActiveOrAllAddresses.OA_AuthorityToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_AuthorityToLeave)));
			this.AuthorityToLeaveDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 19, true);
			this.AuthorityToLeaveDropEdit.Name = "AuthorityToLeaveDropEdit";
			this.AuthorityToLeaveDropEdit.PreBoundMaxLength = 3;
			this.AuthorityToLeaveDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.AuthorityToLeaveDropEdit.TabIndex = 0;
			// 
			// USKnownShipperTabPage
			// 
			this.USKnownShipperTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|a786541e-0e14-4361-9ca1-e7c2980e6b46", "TSA Known Shipper");
			this.USKnownShipperTabPage.Controls.Add(this.TSAE3NotificationSignedCheckbox);
			this.USKnownShipperTabPage.Controls.Add(this.OV_SystemLastEditUserTextBox);
			this.USKnownShipperTabPage.Controls.Add(this.OV_SystemCreateUserTextBox);
			this.USKnownShipperTabPage.Controls.Add(this.OV_SystemLastEditTimeDateEdit);
			this.USKnownShipperTabPage.Controls.Add(this.OV_SystemCreateTimeDateEdit);
			this.USKnownShipperTabPage.Controls.Add(this.OV_EXApprovalNumberTextBox);
			this.USKnownShipperTabPage.Controls.Add(this.OV_EXExportPermissionDetailsTextBox);
			this.USKnownShipperTabPage.Controls.Add(this.OV_EXSiteInspectionDateDateEdit);
			this.USKnownShipperTabPage.Controls.Add(this.OV_EXApprovedOrMajorExporterDropEdit);
			this.USKnownShipperTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.USKnownShipperTabPage.Name = "USKnownShipperTabPage";
			this.USKnownShipperTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.USKnownShipperTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.USKnownShipperTabPage.TabIndex = 4;
			// 
			// TSAE3NotificationSignedCheckbox
			// 
			this.BindingSource.SetBindingMember(this.TSAE3NotificationSignedCheckbox, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXE3Signed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXE3Signed)));
			this.TSAE3NotificationSignedCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.TSAE3NotificationSignedCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TSAE3NotificationSignedCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 30, true);
			this.TSAE3NotificationSignedCheckbox.Name = "TSAE3NotificationSignedCheckbox";
			this.TSAE3NotificationSignedCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 24, true);
			this.TSAE3NotificationSignedCheckbox.TabIndex = 4;
			// 
			// OV_SystemLastEditUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.OV_SystemLastEditUserTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemLastEditUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemLastEditUser)));
			this.OV_SystemLastEditUserTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|fcdcfbe6-9ff5-427d-b867-5f6ce7cab38e", "By");
			this.OV_SystemLastEditUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 237, true);
			this.OV_SystemLastEditUserTextBox.Name = "OV_SystemLastEditUserTextBox";
			this.OV_SystemLastEditUserTextBox.ReadOnly = true;
			this.OV_SystemLastEditUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.OV_SystemLastEditUserTextBox.TabIndex = 16;
			// 
			// OV_SystemCreateUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.OV_SystemCreateUserTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemCreateUser)));
			this.OV_SystemCreateUserTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|162d2843-31c4-45ab-9c55-7a133fd58eb7", "By");
			this.OV_SystemCreateUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 212, true);
			this.OV_SystemCreateUserTextBox.Name = "OV_SystemCreateUserTextBox";
			this.OV_SystemCreateUserTextBox.ReadOnly = true;
			this.OV_SystemCreateUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.OV_SystemCreateUserTextBox.TabIndex = 13;
			// 
			// OV_SystemLastEditTimeDateEdit
			// 
			this.OV_SystemLastEditTimeDateEdit.AllowDrop = true;
			this.OV_SystemLastEditTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.OV_SystemLastEditTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OV_SystemLastEditTimeDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemLastEditTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemLastEditTimeUtc)));
			this.OV_SystemLastEditTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 237, true);
			this.OV_SystemLastEditTimeDateEdit.Name = "OV_SystemLastEditTimeDateEdit";
			this.OV_SystemLastEditTimeDateEdit.TabIndex = 14;
			// 
			// OV_SystemCreateTimeDateEdit
			// 
			this.OV_SystemCreateTimeDateEdit.AllowDrop = true;
			this.OV_SystemCreateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.OV_SystemCreateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OV_SystemCreateTimeDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemCreateTimeUtc)));
			this.OV_SystemCreateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 212, true);
			this.OV_SystemCreateTimeDateEdit.Name = "OV_SystemCreateTimeDateEdit";
			this.OV_SystemCreateTimeDateEdit.TabIndex = 10;
			// 
			// OV_EXApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OV_EXApprovalNumberTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXApprovalNumber)));
			this.OV_EXApprovalNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("61402368-0f76-4168-b427-3fa3e8f2f0c3", "TSA ID Number");
			this.OV_EXApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 59, true);
			this.OV_EXApprovalNumberTextBox.Name = "OV_EXApprovalNumberTextBox";
			this.OV_EXApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.OV_EXApprovalNumberTextBox.TabIndex = 6;
			// 
			// OV_EXExportPermissionDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.OV_EXExportPermissionDetailsTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXExportPermissionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXExportPermissionDetails)));
			this.OV_EXExportPermissionDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|31321b07-97a0-49d3-a525-a9f1aea5d455", "Notes");
			this.OV_EXExportPermissionDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 86, true);
			this.OV_EXExportPermissionDetailsTextBox.Multiline = true;
			this.OV_EXExportPermissionDetailsTextBox.Name = "OV_EXExportPermissionDetailsTextBox";
			this.OV_EXExportPermissionDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 113, true);
			this.OV_EXExportPermissionDetailsTextBox.TabIndex = 8;
			// 
			// OV_EXSiteInspectionDateDateEdit
			// 
			this.OV_EXSiteInspectionDateDateEdit.AllowDrop = true;
			this.OV_EXSiteInspectionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.OV_EXSiteInspectionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OV_EXSiteInspectionDateDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXSiteInspectionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXSiteInspectionDate)));
			this.OV_EXSiteInspectionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.OV_EXSiteInspectionDateDateEdit.Name = "OV_EXSiteInspectionDateDateEdit";
			this.OV_EXSiteInspectionDateDateEdit.TabIndex = 3;
			// 
			// OV_EXApprovedOrMajorExporterDropEdit
			// 
			this.OV_EXApprovedOrMajorExporterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OV_EXApprovedOrMajorExporterDropEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXApprovedOrMajorExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXApprovedOrMajorExporter)));
			this.OV_EXApprovedOrMajorExporterDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|f9612d59-663f-4aa5-95d2-5682835c25a8", "Known Shipper");
			this.OV_EXApprovedOrMajorExporterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 6, true);
			this.OV_EXApprovedOrMajorExporterDropEdit.Name = "OV_EXApprovedOrMajorExporterDropEdit";
			this.OV_EXApprovedOrMajorExporterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.OV_EXApprovedOrMajorExporterDropEdit.TabIndex = 1;
			// 
			// GenericKnownShipperTabPage
			// 
			this.GenericKnownShipperTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GenericKnownShipperTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|eaa18735-9b59-41a9-a9a6-a02349ce5ef3", "Supply Chain Security (JP)");
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_SystemLastEditUserTextBox);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_SystemCreateUserTextBox);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_SystemLastEditTimeDateEdit);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_SystemCreateTimeDateEdit);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_EXApprovalNumberTextBox);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_EXExportPermissionDetailsTextBox);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_EXApprovalExpiryDateEdit);
			this.GenericKnownShipperTabPage.Controls.Add(this.JP_OV_EXApprovedOrMajorExporterDropEdit);
			this.GenericKnownShipperTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GenericKnownShipperTabPage.Name = "GenericKnownShipperTabPage";
			this.GenericKnownShipperTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GenericKnownShipperTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.GenericKnownShipperTabPage.TabIndex = 6;
			this.GenericKnownShipperTabPage.Text = "Supply Chain Security";
			// 
			// JP_OV_SystemLastEditUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.JP_OV_SystemLastEditUserTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemLastEditUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemLastEditUser)));
			this.JP_OV_SystemLastEditUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 237, true);
			this.JP_OV_SystemLastEditUserTextBox.Name = "JP_OV_SystemLastEditUserTextBox";
			this.JP_OV_SystemLastEditUserTextBox.ReadOnly = true;
			this.JP_OV_SystemLastEditUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.JP_OV_SystemLastEditUserTextBox.TabIndex = 24;
			// 
			// JP_OV_SystemCreateUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.JP_OV_SystemCreateUserTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemCreateUser)));
			this.JP_OV_SystemCreateUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 212, true);
			this.JP_OV_SystemCreateUserTextBox.Name = "JP_OV_SystemCreateUserTextBox";
			this.JP_OV_SystemCreateUserTextBox.ReadOnly = true;
			this.JP_OV_SystemCreateUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.JP_OV_SystemCreateUserTextBox.TabIndex = 22;
			// 
			// JP_OV_SystemLastEditTimeDateEdit
			// 
			this.JP_OV_SystemLastEditTimeDateEdit.AllowDrop = true;
			this.JP_OV_SystemLastEditTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_OV_SystemLastEditTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_OV_SystemLastEditTimeDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemLastEditTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemLastEditTimeUtc)));
			this.JP_OV_SystemLastEditTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 237, true);
			this.JP_OV_SystemLastEditTimeDateEdit.Name = "JP_OV_SystemLastEditTimeDateEdit";
			this.JP_OV_SystemLastEditTimeDateEdit.TabIndex = 23;
			// 
			// JP_OV_SystemCreateTimeDateEdit
			// 
			this.JP_OV_SystemCreateTimeDateEdit.AllowDrop = true;
			this.JP_OV_SystemCreateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_OV_SystemCreateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_OV_SystemCreateTimeDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_SystemCreateTimeUtc)));
			this.JP_OV_SystemCreateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 212, true);
			this.JP_OV_SystemCreateTimeDateEdit.Name = "JP_OV_SystemCreateTimeDateEdit";
			this.JP_OV_SystemCreateTimeDateEdit.TabIndex = 21;
			// 
			// JP_OV_EXApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JP_OV_EXApprovalNumberTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXApprovalNumber)));
			this.JP_OV_EXApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 33, true);
			this.JP_OV_EXApprovalNumberTextBox.Name = "JP_OV_EXApprovalNumberTextBox";
			this.JP_OV_EXApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.JP_OV_EXApprovalNumberTextBox.TabIndex = 18;
			// 
			// JP_OV_EXExportPermissionDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.JP_OV_EXExportPermissionDetailsTextBox, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXExportPermissionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXExportPermissionDetails)));
			this.JP_OV_EXExportPermissionDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 85, true);
			this.JP_OV_EXExportPermissionDetailsTextBox.Multiline = true;
			this.JP_OV_EXExportPermissionDetailsTextBox.Name = "JP_OV_EXExportPermissionDetailsTextBox";
			this.JP_OV_EXExportPermissionDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 113, true);
			this.JP_OV_EXExportPermissionDetailsTextBox.TabIndex = 20;
			// 
			// JP_OV_EXApprovalExpiryDateEdit
			// 
			this.JP_OV_EXApprovalExpiryDateEdit.AllowDrop = true;
			this.JP_OV_EXApprovalExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_OV_EXApprovalExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_OV_EXApprovalExpiryDateEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXApprovalExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXApprovalExpiryDate)));
			this.JP_OV_EXApprovalExpiryDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressUserControl|fa0ae563-b6ce-4b53-aca7-54aa090f3e1c", "Expiry Date");
			this.JP_OV_EXApprovalExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 59, true);
			this.JP_OV_EXApprovalExpiryDateEdit.Name = "JP_OV_EXApprovalExpiryDateEdit";
			this.JP_OV_EXApprovalExpiryDateEdit.TabIndex = 19;
			// 
			// JP_OV_EXApprovedOrMajorExporterDropEdit
			// 
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JP_OV_EXApprovedOrMajorExporterDropEdit, "ActiveOrAllAddresses.KnownShipperDetails.OV_EXApprovedOrMajorExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).KnownShipperDetails)).SyncRoot)).OV_EXApprovedOrMajorExporter)));
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressUserControl|a499432e-07c0-401d-ae2b-405c58838259", "Known/Approved");
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 6, true);
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.Name = "JP_OV_EXApprovedOrMajorExporterDropEdit";
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.TabIndex = 17;
			// 
			// GPSTabPage
			// 
			this.GPSTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|3863911f-539c-4e78-b551-84c1a3ed5538", "GPS");
			this.GPSTabPage.Controls.Add(this.CoordinatesGroupBox);
			this.GPSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GPSTabPage.Name = "GPSTabPage";
			this.GPSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GPSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.GPSTabPage.TabIndex = 5;
			// 
			// CoordinatesGroupBox
			// 
			this.CoordinatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|a2f9aac1-058c-4cde-9421-1d2d354e7b61", "Coordinates");
			this.CoordinatesGroupBox.Controls.Add(this.LatitudeCalcEdit);
			this.CoordinatesGroupBox.Controls.Add(this.LongitudeCalcEdit);
			this.CoordinatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.CoordinatesGroupBox.Name = "CoordinatesGroupBox";
			this.CoordinatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 104, true);
			this.CoordinatesGroupBox.TabIndex = 3;
			this.CoordinatesGroupBox.TabStop = false;
			// 
			// LatitudeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LatitudeCalcEdit, "ActiveOrAllAddresses.OA_Latitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Latitude)));
			this.LatitudeCalcEdit.DecimalPlaces = 5;
			this.LatitudeCalcEdit.Decimals = 5;
			this.LatitudeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 59, true);
			this.LatitudeCalcEdit.Name = "LatitudeCalcEdit";
			this.LatitudeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.LatitudeCalcEdit.TabIndex = 4;
			this.LatitudeCalcEdit.Text = "0.00000";
			this.LatitudeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LongitudeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LongitudeCalcEdit, "ActiveOrAllAddresses.OA_Longitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_Longitude)));
			this.LongitudeCalcEdit.DecimalPlaces = 5;
			this.LongitudeCalcEdit.Decimals = 5;
			this.LongitudeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 33, true);
			this.LongitudeCalcEdit.Name = "LongitudeCalcEdit";
			this.LongitudeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.LongitudeCalcEdit.TabIndex = 3;
			this.LongitudeCalcEdit.Text = "0.00000";
			this.LongitudeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DeliveryRouteTabPage
			// 
			this.DeliveryRouteTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.DeliveryRouteTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|8002FF2E-7F19-4A64-8F1C-A52ADCB3B62F", "Delivery Route");
			this.DeliveryRouteTabPage.Controls.Add(this.OA_DeliveryRouteDropEdit);
			this.DeliveryRouteTabPage.Controls.Add(this.DeliveryRouteSequenceCalcEdit);
			this.DeliveryRouteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryRouteTabPage.Name = "DeliveryRouteTabPage";
			this.DeliveryRouteTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeliveryRouteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.DeliveryRouteTabPage.TabIndex = 6;
			// 
			// OA_DeliveryRouteDropEdit
			// 
			this.OA_DeliveryRouteDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_DeliveryRouteDropEdit, "ActiveOrAllAddresses.OA_DeliveryRoute");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_DeliveryRoute)));
			this.OA_DeliveryRouteDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DeliveryRoute|51D9CBFA-6678-4906-BF3A-A4BABE283036", "Delivery Route");
			this.OA_DeliveryRouteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 36, true);
			this.OA_DeliveryRouteDropEdit.Name = "OA_DeliveryRouteDropEdit";
			this.OA_DeliveryRouteDropEdit.PreBoundMaxLength = 3;
			this.OA_DeliveryRouteDropEdit.ShowDescriptionBox = false;
			this.OA_DeliveryRouteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OA_DeliveryRouteDropEdit.TabIndex = 0;
			// 
			// DeliveryRouteSequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeliveryRouteSequenceCalcEdit, "ActiveOrAllAddresses.OA_DeliveryRouteSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_DeliveryRouteSequence)));
			this.DeliveryRouteSequenceCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DeliveryRouteSequence|D89CC623-5B9D-4B97-AECE-F68EDC8A81BC", "Seq", "Route Sequence", "Delivery Route Sequence");
			this.DeliveryRouteSequenceCalcEdit.DecimalPlaces = 2;
			this.DeliveryRouteSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 69, true);
			this.DeliveryRouteSequenceCalcEdit.Name = "DeliveryRouteSequenceCalcEdit";
			this.DeliveryRouteSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.DeliveryRouteSequenceCalcEdit.TabIndex = 0;
			this.DeliveryRouteSequenceCalcEdit.Text = "0";
			this.DeliveryRouteSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FreeWaitingTabControl
			// 
			this.FreeWaitingTabControl.BackColor = System.Drawing.SystemColors.Control;
			this.FreeWaitingTabControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("139b1aee-1c2b-4659-bf2f-1653377f7576", "Free Waiting");
			this.FreeWaitingTabControl.Controls.Add(this.OrgFreeWaitingTimeBoundGrid);
			this.FreeWaitingTabControl.Controls.Add(this.UseCumulative);
			this.FreeWaitingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FreeWaitingTabControl.Name = "FreeWaitingTabControl";
			this.FreeWaitingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.FreeWaitingTabControl.TabIndex = 7;
			// 
			// OrgFreeWaitingTimeBoundGrid
			// 
			this.OrgFreeWaitingTimeBoundGrid.AllowNavigation = false;
			this.OrgFreeWaitingTimeBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgFreeWaitingTimeBoundGrid, "ActiveOrAllAddresses.FreeWaitingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_RC_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).ContainerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).DropModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_CFSFreeWaitingTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_CNEFreeWaitingTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_CNRFreeWaitingTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_CTOFreeWaitingTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_CYDFreeWaitingTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgFreeWaitingTime)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).FreeWaitingCollection)).SyncRoot)).OY_OtherFreeWaitingTime)));
			this.OrgFreeWaitingTimeBoundGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.BindToList = "ContainerTypes";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1d78693e-36a7-4a4b-843b-d18eb268e79f", "CNT Type");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OY_RC_ContainerType";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "DropModes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fa5e07c-896a-41c5-816b-8074353112c3", "Drop Mode");
			zDropEditColumnStyleInfo1.ColumnName = "OY_DropMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo7.AllowNegative = false;
			zTimeEditExColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a8f2a08c-5865-4e1b-b2de-1b6d9b970d98", "CFS");
			zTimeEditExColumnStyleInfo7.ColumnName = "OY_CFSFreeWaitingTime";
			zTimeEditExColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo8.AllowNegative = false;
			zTimeEditExColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5ed9b64d-f6f2-4469-a412-d22d47d8fcc2", "CNE");
			zTimeEditExColumnStyleInfo8.ColumnName = "OY_CNEFreeWaitingTime";
			zTimeEditExColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo9.AllowNegative = false;
			zTimeEditExColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e0c66262-4c07-4c60-a06f-71e3080b336d", "CNR");
			zTimeEditExColumnStyleInfo9.ColumnName = "OY_CNRFreeWaitingTime";
			zTimeEditExColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo10.AllowNegative = false;
			zTimeEditExColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4ba3de70-4916-40b4-830f-dde01b6ee4c9", "CTO");
			zTimeEditExColumnStyleInfo10.ColumnName = "OY_CTOFreeWaitingTime";
			zTimeEditExColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo11.AllowNegative = false;
			zTimeEditExColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("52247602-71a8-40b3-8440-68cd3309b595", "CYD");
			zTimeEditExColumnStyleInfo11.ColumnName = "OY_CYDFreeWaitingTime";
			zTimeEditExColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo12.AllowNegative = false;
			zTimeEditExColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cb5a53cc-6055-447f-8c42-c12116f9bf19", "Other");
			zTimeEditExColumnStyleInfo12.ColumnName = "OY_OtherFreeWaitingTime";
			zTimeEditExColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo7);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo8);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo9);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo10);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo11);
			this.OrgFreeWaitingTimeBoundGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo12);
			this.OrgFreeWaitingTimeBoundGrid.CopySelectedRowsAllowed = true;
			this.OrgFreeWaitingTimeBoundGrid.GridId = "53823044-9e95-4887-a623-a4438645c7ed";
			this.OrgFreeWaitingTimeBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgFreeWaitingTimeBoundGrid.LayoutKey = "OrgAddressBoundGrid";
			this.OrgFreeWaitingTimeBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 41, true);
			this.OrgFreeWaitingTimeBoundGrid.Name = "OrgFreeWaitingTimeBoundGrid";
			this.OrgFreeWaitingTimeBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 251, true);
			this.OrgFreeWaitingTimeBoundGrid.TabIndex = 4;
			// 
			// UseCumulative
			// 
			this.UseCumulative.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.UseCumulative, "ActiveOrAllAddresses.OA_UseCumulativeFreeWaitingTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).OA_UseCumulativeFreeWaitingTime)));
			this.UseCumulative.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ffa59355-9463-4ea6-9897-7f723e584724", "Use Cumulative");
			this.UseCumulative.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCumulative.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 12, true);
			this.UseCumulative.Name = "UseCumulative";
			this.UseCumulative.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.UseCumulative.TabIndex = 3;
			this.UseCumulative.UseVisualStyleBackColor = false;
			// 
			// AdditionalAddressInfoTabPage
			// 
			this.AdditionalAddressInfoTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AdditionalAddressInfoTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f34e729f-8ecb-4fc6-92dc-cead026d0fa6", "Additional Address Info");
			this.AdditionalAddressInfoTabPage.Controls.Add(this.AddressAdditionalInfoUserControl);
			this.AdditionalAddressInfoTabPage.Controls.Add(this.TranslatedAddressAdditionalInfoUserControl);
			this.AdditionalAddressInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalAddressInfoTabPage.Name = "AdditionalAddressInfoTabPage";
			this.AdditionalAddressInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.AdditionalAddressInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 302, true);
			this.AdditionalAddressInfoTabPage.TabIndex = 8;
			// 
			// AddressAdditionalInfoUserControl
			// 
			this.AddressAdditionalInfoUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressAdditionalInfoUserControl.Name = "AddressAdditionalInfoUserControl";
			this.AddressAdditionalInfoUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			// 
			// TranslatedAddressAdditionalInfoUserControl
			//
			this.TranslatedAddressAdditionalInfoUserControl.Visible = false;
			this.TranslatedAddressAdditionalInfoUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TranslatedAddressAdditionalInfoUserControl.Name = "TranslatedAddressAdditionalInfoUserControl";
			this.TranslatedAddressAdditionalInfoUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			// 
			// AddressesUserControl
			// 
			this.Controls.Add(this.MainSplitContainer);
			this.IsModifyAddress = true;
			this.IsModifyAddressCapabilities = true;
			this.IsModifyAddressCapabilitiesARAP = true;
			this.IsModifyAddressCapabilitiesNonARAP = true;
			this.IsModifyConsignorExporterScheme = true;
			this.Name = "AddressesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 580, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.OrgAddressSplitContainer.ResumeLayout(false);
			this.OrgAddressSplitContainer.PerformLayout();
			this.AddressesGroupBox.ResumeLayout(false);
			this.AddressesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgAddressBoundGrid)).EndInit();
			this.OrgAddressBoundGrid.ResumeLayout(false);
			this.OrgAddressBoundGrid.PerformLayout();
			this.AddressCapabilities.ResumeLayout(false);
			this.AddressCapabilities.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressCapability)).EndInit();
			this.AddressCapability.ResumeLayout(false);
			this.AddressCapability.PerformLayout();
			this.AddressesMainLayoutPanel.ResumeLayout(false);
			this.AddressesMainLayoutPanel.PerformLayout();
			this.AddressDetailsGroupBox.ResumeLayout(false);
			this.AddressDetailsGroupBox.PerformLayout();
			this.SelectLocalAddressDropEdit.ResumeLayout(true);
			this.SelectLocalAddressDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.RelatedPortFindBox.ResumeLayout(true);
			this.RelatedPortFindBox.PerformLayout();
			this.FaxNumberControl.ResumeLayout(true);
			this.FaxNumberControl.PerformLayout();
			this.MobilePhoneNumberControl.ResumeLayout(true);
			this.MobilePhoneNumberControl.PerformLayout();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.OA_StateBoundDropEdit.ResumeLayout(true);
			this.OA_StateBoundDropEdit.PerformLayout();
			this.OA_LanguageBoundDropEdit.ResumeLayout(true);
			this.OA_LanguageBoundDropEdit.PerformLayout();
			this.ExtraDetailsTabControl.ResumeLayout(false);
			this.ExtraDetailsTabControl.PerformLayout();
			this.LoadingConstraintsTabPage.ResumeLayout(false);
			this.LoadingConstraintsTabPage.PerformLayout();
			this.OA_CommunicationRequiredDropEdit.ResumeLayout(true);
			this.OA_CommunicationRequiredDropEdit.PerformLayout();
			this.OA_LabourRequiredDropEdit.ResumeLayout(true);
			this.OA_LabourRequiredDropEdit.PerformLayout();
			this.OA_AccessPointDropEdit.ResumeLayout(true);
			this.OA_AccessPointDropEdit.PerformLayout();
			this.OA_ContainerHandlingDropEdit.ResumeLayout(true);
			this.OA_ContainerHandlingDropEdit.PerformLayout();
			this.OA_Dock_HeightDropEdit.ResumeLayout(true);
			this.OA_Dock_HeightDropEdit.PerformLayout();
			this.WarehousingTabPage.ResumeLayout(false);
			this.WarehousingTabPage.PerformLayout();
			this.CartageEquipmentTabPage.ResumeLayout(false);
			this.CartageEquipmentTabPage.PerformLayout();
			this.RequiredCartageEquipmentGroupBox.ResumeLayout(false);
			this.RequiredCartageEquipmentGroupBox.PerformLayout();
			this.OM_EXFCLEquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.OM_EXFCLEquipmentNeededBoundDropEdit.PerformLayout();
			this.OM_EXAirEquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.OM_EXAirEquipmentNeededBoundDropEdit.PerformLayout();
			this.OM_EXLCLEquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.OM_EXLCLEquipmentNeededBoundDropEdit.PerformLayout();
			this.PickupDeliveryAndAttendanceTimesTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TimetableGrid)).EndInit();
			this.TimetableGroupBox.ResumeLayout(false);
			this.TimetableGroupBox.PerformLayout();
			this.DeliveryGroupBox.ResumeLayout(false);
			this.DeliveryGroupBox.PerformLayout();
			this.AuthorityToLeaveDropEdit.ResumeLayout(true);
			this.AuthorityToLeaveDropEdit.PerformLayout();
			this.USKnownShipperTabPage.ResumeLayout(false);
			this.USKnownShipperTabPage.PerformLayout();
			this.OV_SystemLastEditTimeDateEdit.ResumeLayout(true);
			this.OV_SystemLastEditTimeDateEdit.PerformLayout();
			this.OV_SystemCreateTimeDateEdit.ResumeLayout(true);
			this.OV_SystemCreateTimeDateEdit.PerformLayout();
			this.OV_EXSiteInspectionDateDateEdit.ResumeLayout(true);
			this.OV_EXSiteInspectionDateDateEdit.PerformLayout();
			this.OV_EXApprovedOrMajorExporterDropEdit.ResumeLayout(true);
			this.OV_EXApprovedOrMajorExporterDropEdit.PerformLayout();
			this.GenericKnownShipperTabPage.ResumeLayout(false);
			this.GenericKnownShipperTabPage.PerformLayout();
			this.JP_OV_SystemLastEditTimeDateEdit.ResumeLayout(true);
			this.JP_OV_SystemLastEditTimeDateEdit.PerformLayout();
			this.JP_OV_SystemCreateTimeDateEdit.ResumeLayout(true);
			this.JP_OV_SystemCreateTimeDateEdit.PerformLayout();
			this.JP_OV_EXApprovalExpiryDateEdit.ResumeLayout(true);
			this.JP_OV_EXApprovalExpiryDateEdit.PerformLayout();
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.ResumeLayout(true);
			this.JP_OV_EXApprovedOrMajorExporterDropEdit.PerformLayout();
			this.GPSTabPage.ResumeLayout(false);
			this.GPSTabPage.PerformLayout();
			this.CoordinatesGroupBox.ResumeLayout(false);
			this.CoordinatesGroupBox.PerformLayout();
			this.DeliveryRouteTabPage.ResumeLayout(false);
			this.DeliveryRouteTabPage.PerformLayout();
			this.OA_DeliveryRouteDropEdit.ResumeLayout(true);
			this.OA_DeliveryRouteDropEdit.PerformLayout();
			this.FreeWaitingTabControl.ResumeLayout(false);
			this.FreeWaitingTabControl.PerformLayout();
			this.AdditionalAddressInfoTabPage.ResumeLayout(false);
			this.AdditionalAddressInfoTabPage.PerformLayout();
			this.AddressAdditionalInfoUserControl.ResumeLayout(false);
			this.AddressAdditionalInfoUserControl.PerformLayout();
			this.TranslatedAddressAdditionalInfoUserControl.ResumeLayout(false);
			this.TranslatedAddressAdditionalInfoUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgFreeWaitingTimeBoundGrid)).EndInit();
			this.OrgFreeWaitingTimeBoundGrid.ResumeLayout(false);
			this.OrgFreeWaitingTimeBoundGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
