using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class RegistrationDetails
	{
		#region Component Designer generated code

		ZGroupBox ModeGroupBox;
		ZDropEdit JC_TransportModeDropEdit;
		ZDropEdit JC_ContainerModeDropEdit;
		MasterFiles.GUI.ZOrganisationControl ClientOrganisationControl;
		ZGuidFindBox LoadListConsolGuidFindBox;
		internal ZTemplateTabControl ArrivalDispatchTabControl;
		ZTabPage ContainerArrivalTabPage;
		ZDateEdit JC_ArrivalTimeDateEdit;
		ZTextBox JC_ArrivalTruckDriversLicenseTextBox;
		ZTextBox JC_ArrivalTruckRegistrationTextBox;
		internal ZPanel ArrivalEmptyPanel;
		ZGroupBox zGroupBox3;
		ZAddressControl ContainerYardAddressAddressControl1;
		internal ZPanel ArrivalCTOSlotPanel;
		ZDateEdit JC_SlotDateDateEdit1;
		ZTextBox JC_SlotReferenceTextBox1;
		internal ZTabPage ContainerDispatchTabPage;
		internal ZPanel DispatchEmptyPanel;
		ZGroupBox zGroupBox4;
		ZAddressControl ContainerYardAddressAddressControl2;
		ZTextBox JC_DepartureTruckRegistrationTextBox;
		ZDateEdit DepartureDateEdit;
		ZTextBox JC_DepartureTruckDriversLicenseTextBox;
		internal ZPanel DispatchCTOSlotPanel;
		ZDateEdit JC_SlotDateDateEdit2;
		ZTextBox JC_SlotReferenceTextBox2;
		internal ZPanel AUContainerArrivalPanel;
		ZTextBox JS_GatePassStatusBoundTextBox;
		internal ZPanel AUContainerDispatchPanel;
		ZTextBox CargoStatusAdviceTextBox;
		ZDateEdit zDateEdit3;
		ZDateEdit zDateEdit4;
		ZTextBox UnpackShedTextBox_ArrivalTab;
		ZTextBox StorageLocationTextBox_ArrivalTab;
		ZTextBox StorageLocationTextBox_DispatchTab;
		internal ZGroupBox PackUnpackDatesGroupBox;
		ZPanel ContainerArrivalTopPanel;
		ZPanel zPanel1;
		ZDateEdit EmptyReturnByDateEdit;
		internal ZTemplateTabControl PurposeSpecificDetailsTabControl;
		internal ZTabPage CFSTabPage;
		internal ZTabPage StorageTabPage;
		ZGroupBox PurposeGroupBox;
		ZTabPage ContainerDetailsTab;
		ZTabPage DimensionsTab;
		ZTabPage ReeferTab;
		ZGroupBox AdditionalContainerDetailsGroupBox;
		ZTextBox JC_ReleaseNumTextBox;
		ZGuidFindBox JC_RCGuidFindBox;
		ZTextBox JC_ContainerNumTextBox;
		ZTextBox JC_SealNumBoundTextEdit;
		ZTextBox JY_TempRecorderSerialNoTextBox;
		ZTextBox JC_AdditionalSealNumTextBox;
		ZTextBox JC_ExportDepotCustomsReferenceTextBox;
		internal ZTextBox JC_Calc_MasterBillNumTextBox;
		ZTextBox EntryNumberTextBox;
		ZTextBox CarrierBookingRefTextBox;
		ZDateEdit StorageDateDateEdit;
		internal ZTextBox StorageMasterBillNumberTextBox;
		ZTextBox JC_TrainWagonNumberTextBox;
		ZTextBox zTextBox3;
		ZTabPage ServicesTabPage;
		ZGroupBox ServicesGroupBox;
		ZGrid ServicesGrid;
		ZDropEdit JC_PurposeDropEdit;
		internal ZCheckBox JC_IsSealOkCheckBox;
		internal ZDateEdit JC_PackUnpackDate_ReadonlyDateEdit;
		internal ZPanel AvailableStorageDatesPanel;
		ZDateEdit JC_LCLAvailable_ReadonlyDateEdit;
		ZDateEdit JC_LCLStorageCommences_ReadonlyDateEdit;
		ZDropEdit ServiceTypeDropEdit;
		ZDateEdit ServiceDateBookedDateEdit;
		ZDateEdit ServicesDateCompletedDateEdit;
		ZTextBox ServiceReferencesTextBox;
		ZDropEdit ServiceLocationAddressDropEdit;
		ZTextBox ServiceNoteTextBox;
		ZGuidFindBox ServiceContractorGuidFindBox;
		ZCalcEdit ServiceCountCalcEdit;
		ZTimeEditEx ServiceDurationTimeEditEx;
		ZTextBox zTextBox2;
		ZTextBox zTextBox4;
		ZCodeFindBox CommodityCodeFindBox;
		ZTextBox ReeferGeneratorTextBox;
		ZCalcEdit JC_HumidityPercentCalcEdit;
		ZCheckBox ExportIsControlledAtmosphereCheckBox;
		ZCheckBox ExportIsNonOperatingReeferCheckBox;
		ZCheckBox ExportIsChillerCheckBox;
		ZCheckBox ExportIsFrozenCheckBox;
		ZCheckBox IsArrivingAtCTOByRailCheckBox;
		ZCheckBox ExportIsDamagedCheckBox;
		ZCheckBox ExportIsEmptyContainerCheckBox;
		ZDateEdit FCLStorageUnderbondClearedDateEdit;
		ZCheckBox FCLStorageArrivedUnderbondCheckBox;
		private ZAddressControl zAddressControl1;
		private ZAddressControl zAddressControl2;
		internal RegistrationSailingDetailsControl registrationSailingDetailsControl1;
		internal RegistrationSailingDetailsControl registrationSailingDetailsControl2;
		private ZTabPage WeightTab;
		private ZDropEdit zDropEdit1;
		private ZCalcEdit zCalcEdit3;
		private ZCalcEdit zCalcEdit4;
		private ZCalcEdit zCalcEdit5;
		private ZCalcEdit zCalcEdit6;
		private ZCalcEdit zCalcEdit11;
		private ZTemplateTabControl ContainerTabControl;
		private ZCalcDropEdit JY_SetPointTempCalcDropEdit;
		private ZCalcDropEdit JC_AirVentBoundCalcDropEdit;
		private ZCalcEdit zCalcEdit7;
		private ZDropEdit JC_SealPartyDropEdit;
		private ZDropEdit JC_AdditionalSealPartyDropEdit;
		private OverhangControl overhangControl;

		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			this.ContainerTabControl = new ZTemplateTabControl();
			this.ContainerDetailsTab = new ZTabPage();
			this.CommodityCodeFindBox = new ZCodeFindBox();
			this.JC_RCGuidFindBox = new ZGuidFindBox();
			this.JC_ContainerNumTextBox = new ZTextBox();
			this.JC_SealNumBoundTextEdit = new ZTextBox();
			this.DimensionsTab = new ZTabPage();
			this.overhangControl = new OverhangControl();
			this.ReeferTab = new ZTabPage();
			this.ReeferGeneratorTextBox = new ZTextBox();
			this.JC_HumidityPercentCalcEdit = new ZCalcEdit();
			this.ExportIsControlledAtmosphereCheckBox = new ZCheckBox();
			this.ExportIsNonOperatingReeferCheckBox = new ZCheckBox();
			this.ExportIsChillerCheckBox = new ZCheckBox();
			this.ExportIsFrozenCheckBox = new ZCheckBox();
			this.JY_TempRecorderSerialNoTextBox = new ZTextBox();
			this.JY_SetPointTempCalcDropEdit = new ZCalcDropEdit();
			this.JC_AirVentBoundCalcDropEdit = new ZCalcDropEdit();
			this.WeightTab = new ZTabPage();
			this.zCalcEdit7 = new ZCalcEdit();
			this.zDropEdit1 = new ZDropEdit();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zCalcEdit4 = new ZCalcEdit();
			this.zCalcEdit5 = new ZCalcEdit();
			this.zCalcEdit6 = new ZCalcEdit();
			this.zCalcEdit11 = new ZCalcEdit();
			this.ModeGroupBox = new ZGroupBox();
			this.JC_TransportModeDropEdit = new ZDropEdit();
			this.JC_ContainerModeDropEdit = new ZDropEdit();
			this.ClientOrganisationControl = new MasterFiles.GUI.ZOrganisationControl();
			this.LoadListConsolGuidFindBox = new ZGuidFindBox();
			this.ArrivalDispatchTabControl = new ZTemplateTabControl();
			this.ContainerArrivalTabPage = new ZTabPage();
			this.AUContainerArrivalPanel = new ZPanel();
			this.zDateEdit3 = new ZDateEdit();
			this.JS_GatePassStatusBoundTextBox = new ZTextBox();
			this.ArrivalEmptyPanel = new ZPanel();
			this.zGroupBox3 = new ZGroupBox();
			this.ContainerYardAddressAddressControl1 = new ZAddressControl();
			this.ArrivalCTOSlotPanel = new ZPanel();
			this.JC_SlotDateDateEdit1 = new ZDateEdit();
			this.JC_SlotReferenceTextBox1 = new ZTextBox();
			this.ContainerArrivalTopPanel = new ZPanel();
			this.zAddressControl1 = new ZAddressControl();
			this.zTextBox2 = new ZTextBox();
			this.JC_IsSealOkCheckBox = new ZCheckBox();
			this.JC_TrainWagonNumberTextBox = new ZTextBox();
			this.JC_ArrivalTruckRegistrationTextBox = new ZTextBox();
			this.StorageLocationTextBox_ArrivalTab = new ZTextBox();
			this.UnpackShedTextBox_ArrivalTab = new ZTextBox();
			this.JC_ArrivalTruckDriversLicenseTextBox = new ZTextBox();
			this.JC_ArrivalTimeDateEdit = new ZDateEdit();
			this.ContainerDispatchTabPage = new ZTabPage();
			this.IsArrivingAtCTOByRailCheckBox = new ZCheckBox();
			this.ExportIsDamagedCheckBox = new ZCheckBox();
			this.ExportIsEmptyContainerCheckBox = new ZCheckBox();
			this.AUContainerDispatchPanel = new ZPanel();
			this.zDateEdit4 = new ZDateEdit();
			this.CargoStatusAdviceTextBox = new ZTextBox();
			this.DispatchEmptyPanel = new ZPanel();
			this.zGroupBox4 = new ZGroupBox();
			this.ContainerYardAddressAddressControl2 = new ZAddressControl();
			this.DispatchCTOSlotPanel = new ZPanel();
			this.JC_SlotDateDateEdit2 = new ZDateEdit();
			this.JC_SlotReferenceTextBox2 = new ZTextBox();
			this.zPanel1 = new ZPanel();
			this.zAddressControl2 = new ZAddressControl();
			this.zTextBox4 = new ZTextBox();
			this.zTextBox3 = new ZTextBox();
			this.EmptyReturnByDateEdit = new ZDateEdit();
			this.StorageLocationTextBox_DispatchTab = new ZTextBox();
			this.JC_DepartureTruckDriversLicenseTextBox = new ZTextBox();
			this.DepartureDateEdit = new ZDateEdit();
			this.JC_DepartureTruckRegistrationTextBox = new ZTextBox();
			this.PackUnpackDatesGroupBox = new ZGroupBox();
			this.JC_Calc_MasterBillNumTextBox = new ZTextBox();
			this.EntryNumberTextBox = new ZTextBox();
			this.CarrierBookingRefTextBox = new ZTextBox();
			this.PurposeSpecificDetailsTabControl = new ZTemplateTabControl();
			this.CFSTabPage = new ZTabPage();
			this.registrationSailingDetailsControl1 = new RegistrationSailingDetailsControl();
			this.AdditionalContainerDetailsGroupBox = new ZGroupBox();
			this.JC_PackUnpackDate_ReadonlyDateEdit = new ZDateEdit();
			this.AvailableStorageDatesPanel = new ZPanel();
			this.JC_LCLAvailable_ReadonlyDateEdit = new ZDateEdit();
			this.JC_LCLStorageCommences_ReadonlyDateEdit = new ZDateEdit();
			this.JC_ReleaseNumTextBox = new ZTextBox();
			this.JC_AdditionalSealNumTextBox = new ZTextBox();
			this.JC_ExportDepotCustomsReferenceTextBox = new ZTextBox();
			this.StorageTabPage = new ZTabPage();
			this.registrationSailingDetailsControl2 = new RegistrationSailingDetailsControl();
			this.StorageDateDateEdit = new ZDateEdit();
			this.StorageMasterBillNumberTextBox = new ZTextBox();
			this.FCLStorageArrivedUnderbondCheckBox = new ZCheckBox();
			this.FCLStorageUnderbondClearedDateEdit = new ZDateEdit();
			this.ServicesTabPage = new ZTabPage();
			this.ServiceDurationTimeEditEx = new ZTimeEditEx();
			this.ServiceLocationAddressDropEdit = new ZDropEdit();
			this.ServiceNoteTextBox = new ZTextBox();
			this.ServiceContractorGuidFindBox = new ZGuidFindBox();
			this.ServiceCountCalcEdit = new ZCalcEdit();
			this.ServicesDateCompletedDateEdit = new ZDateEdit();
			this.ServiceReferencesTextBox = new ZTextBox();
			this.ServiceDateBookedDateEdit = new ZDateEdit();
			this.ServiceTypeDropEdit = new ZDropEdit();
			this.ServicesGroupBox = new ZGroupBox();
			this.ServicesGrid = new ZGrid();
			this.PurposeGroupBox = new ZGroupBox();
			this.JC_PurposeDropEdit = new ZDropEdit();
			this.JC_SealPartyDropEdit = new ZDropEdit();
			this.JC_AdditionalSealPartyDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerTabControl.SuspendLayout();
			this.ContainerDetailsTab.SuspendLayout();
			this.DimensionsTab.SuspendLayout();
			this.ReeferTab.SuspendLayout();
			this.WeightTab.SuspendLayout();
			this.ModeGroupBox.SuspendLayout();
			this.ArrivalDispatchTabControl.SuspendLayout();
			this.ContainerArrivalTabPage.SuspendLayout();
			this.AUContainerArrivalPanel.SuspendLayout();
			this.ArrivalEmptyPanel.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.ArrivalCTOSlotPanel.SuspendLayout();
			this.ContainerArrivalTopPanel.SuspendLayout();
			this.ContainerDispatchTabPage.SuspendLayout();
			this.AUContainerDispatchPanel.SuspendLayout();
			this.DispatchEmptyPanel.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.DispatchCTOSlotPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.PackUnpackDatesGroupBox.SuspendLayout();
			this.PurposeSpecificDetailsTabControl.SuspendLayout();
			this.CFSTabPage.SuspendLayout();
			this.AdditionalContainerDetailsGroupBox.SuspendLayout();
			this.AvailableStorageDatesPanel.SuspendLayout();
			this.StorageTabPage.SuspendLayout();
			this.ServicesTabPage.SuspendLayout();
			this.ServicesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.PurposeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSContainer);
			// 
			// ContainerTabControl
			// 
			this.ContainerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ContainerTabControl.Controls.Add(this.ContainerDetailsTab);
			this.ContainerTabControl.Controls.Add(this.DimensionsTab);
			this.ContainerTabControl.Controls.Add(this.ReeferTab);
			this.ContainerTabControl.Controls.Add(this.WeightTab);
			this.ContainerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 280, true);
			this.ContainerTabControl.Name = "ContainerTabControl";
			this.ContainerTabControl.SelectedIndex = 0;
			this.ContainerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 277, true);
			this.ContainerTabControl.TabIndex = 3;
			// 
			// ContainerDetailsTab
			// 
			this.ContainerDetailsTab.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|cb061595-2b55-4039-9425-dc187c43f690", "Container Details");
			this.ContainerDetailsTab.Controls.Add(this.JC_SealPartyDropEdit);
			this.ContainerDetailsTab.Controls.Add(this.CommodityCodeFindBox);
			this.ContainerDetailsTab.Controls.Add(this.JC_RCGuidFindBox);
			this.ContainerDetailsTab.Controls.Add(this.JC_ContainerNumTextBox);
			this.ContainerDetailsTab.Controls.Add(this.JC_SealNumBoundTextEdit);
			this.ContainerDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerDetailsTab.Name = "ContainerDetailsTab";
			this.ContainerDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 250, true);
			this.ContainerDetailsTab.TabIndex = 0;
			// 
			// CommodityCodeFindBox
			// 
			this.CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeFindBox, "JC_RH_NKContainerCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_RH_NKContainerCommodityCode)));
			this.CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 65, true);
			this.CommodityCodeFindBox.Name = "CommodityCodeFindBox";
			this.CommodityCodeFindBox.PreBoundMaxLength = 4;
			this.CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.CommodityCodeFindBox.TabIndex = 2;
			// 
			// JC_RCGuidFindBox
			// 
			this.JC_RCGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_RCGuidFindBox, "JC_RC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).JC_RC)));
			this.JC_RCGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 39, true);
			this.JC_RCGuidFindBox.Name = "JC_RCGuidFindBox";
			this.JC_RCGuidFindBox.PreBoundMaxLength = 4;
			this.JC_RCGuidFindBox.ShowDescriptionBox = false;
			this.JC_RCGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.JC_RCGuidFindBox.TabIndex = 1;
			// 
			// JC_ContainerNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ContainerNumTextBox, "JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ContainerNum)));
			this.JC_ContainerNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 15, true);
			this.JC_ContainerNumTextBox.Name = "JC_ContainerNumTextBox";
			this.JC_ContainerNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JC_ContainerNumTextBox.TabIndex = 0;
			// 
			// JC_SealNumBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JC_SealNumBoundTextEdit, "JC_SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_SealNum)));
			this.JC_SealNumBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 91, true);
			this.JC_SealNumBoundTextEdit.Name = "JC_SealNumBoundTextEdit";
			this.JC_SealNumBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JC_SealNumBoundTextEdit.TabIndex = 3;
			// 
			// DimensionsTab
			// 
			this.DimensionsTab.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|ca4fb535-21c2-4b0a-ab8a-ead755d8ab19", "Dimensions");
			this.DimensionsTab.Controls.Add(this.overhangControl);
			this.DimensionsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DimensionsTab.Name = "DimensionsTab";
			this.DimensionsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DimensionsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 250, true);
			this.DimensionsTab.TabIndex = 1;
			// 
			// overhangControl
			// 
			this.overhangControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.overhangControl, ".");
			this.overhangControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.overhangControl.Name = "overhangControl";
			this.overhangControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 250, true);
			this.overhangControl.TabIndex = 0;
			// 
			// ReeferTab
			// 
			this.ReeferTab.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|56579883-75d8-4265-8b3e-55eefc37b63e", "Reefer");
			this.ReeferTab.Controls.Add(this.ReeferGeneratorTextBox);
			this.ReeferTab.Controls.Add(this.JC_HumidityPercentCalcEdit);
			this.ReeferTab.Controls.Add(this.ExportIsControlledAtmosphereCheckBox);
			this.ReeferTab.Controls.Add(this.ExportIsNonOperatingReeferCheckBox);
			this.ReeferTab.Controls.Add(this.ExportIsChillerCheckBox);
			this.ReeferTab.Controls.Add(this.ExportIsFrozenCheckBox);
			this.ReeferTab.Controls.Add(this.JY_TempRecorderSerialNoTextBox);
			this.ReeferTab.Controls.Add(this.JY_SetPointTempCalcDropEdit);
			this.ReeferTab.Controls.Add(this.JC_AirVentBoundCalcDropEdit);
			this.ReeferTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReeferTab.Name = "ReeferTab";
			this.ReeferTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReeferTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 250, true);
			this.ReeferTab.TabIndex = 2;
			// 
			// ReeferGeneratorTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReeferGeneratorTextBox, "JC_RefrigGeneratorID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_RefrigGeneratorID)));
			this.ReeferGeneratorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 173, true);
			this.ReeferGeneratorTextBox.Name = "ReeferGeneratorTextBox";
			this.ReeferGeneratorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.ReeferGeneratorTextBox.TabIndex = 7;
			// 
			// JC_HumidityPercentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JC_HumidityPercentCalcEdit, "JC_HumidityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_HumidityPercent)));
			this.JC_HumidityPercentCalcEdit.DecimalPlaces = 0;
			this.JC_HumidityPercentCalcEdit.Decimals = 0;
			this.JC_HumidityPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 149, true);
			this.JC_HumidityPercentCalcEdit.Name = "JC_HumidityPercentCalcEdit";
			this.JC_HumidityPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JC_HumidityPercentCalcEdit.TabIndex = 6;
			this.JC_HumidityPercentCalcEdit.Text = "0";
			this.JC_HumidityPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExportIsControlledAtmosphereCheckBox
			// 
			this.ExportIsControlledAtmosphereCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsControlledAtmosphereCheckBox, "JC_IsControlledAtmosphere");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_IsControlledAtmosphere)));
			this.ExportIsControlledAtmosphereCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsControlledAtmosphereCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 29, true);
			this.ExportIsControlledAtmosphereCheckBox.Name = "ExportIsControlledAtmosphereCheckBox";
			this.ExportIsControlledAtmosphereCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.ExportIsControlledAtmosphereCheckBox.TabIndex = 1;
			//
			//ExportIsNonOperatingReeferCheckBox
			//
			this.ExportIsNonOperatingReeferCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsNonOperatingReeferCheckBox, "JC_IsNonOperativeReefer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_IsNonOperativeReefer)));
			this.ExportIsNonOperatingReeferCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsNonOperatingReeferCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 10, true);
			this.ExportIsNonOperatingReeferCheckBox.Name = "ExportIsNonOperatingReeferCheckBox";
			this.ExportIsNonOperatingReeferCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.ExportIsNonOperatingReeferCheckBox.TabIndex = 0;
			// 
			// ExportIsChillerCheckBox
			// 
			this.ExportIsChillerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsChillerCheckBox, "IsChiller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).IsChiller)));
			this.ExportIsChillerCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|80a0d8e4-2d90-4b7e-8818-5c447d8defa0", "Is Chiller");
			this.ExportIsChillerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsChillerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 52, true);
			this.ExportIsChillerCheckBox.Name = "ExportIsChillerCheckBox";
			this.ExportIsChillerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.ExportIsChillerCheckBox.TabIndex = 2;
			// 
			// ExportIsFrozenCheckBox
			// 
			this.ExportIsFrozenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsFrozenCheckBox, "IsFreezer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).IsFreezer)));
			this.ExportIsFrozenCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|a0001d8a-9485-45a3-8b2d-008e9f64d786", "Is Frozen");
			this.ExportIsFrozenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsFrozenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 52, true);
			this.ExportIsFrozenCheckBox.Name = "ExportIsFrozenCheckBox";
			this.ExportIsFrozenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.ExportIsFrozenCheckBox.TabIndex = 3;
			// 
			// JY_TempRecorderSerialNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.JY_TempRecorderSerialNoTextBox, "JC_TempRecorderSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_TempRecorderSerialNo)));
			this.JY_TempRecorderSerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 125, true);
			this.JY_TempRecorderSerialNoTextBox.Name = "JY_TempRecorderSerialNoTextBox";
			this.JY_TempRecorderSerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JY_TempRecorderSerialNoTextBox.TabIndex = 5;
			// 
			// JY_SetPointTempCalcDropEdit
			// 
			this.JY_SetPointTempCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_SetPointTempCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_SetPointTempUnit)));
			this.JY_SetPointTempCalcDropEdit.BindToAmount = "JC_SetPointTemp";
			this.JY_SetPointTempCalcDropEdit.BindToUnit = "JC_SetPointTempUnit";
			this.JY_SetPointTempCalcDropEdit.Decimals = 2;
			this.JY_SetPointTempCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 78, true);
			this.JY_SetPointTempCalcDropEdit.Name = "JY_SetPointTempCalcDropEdit";
			this.JY_SetPointTempCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JY_SetPointTempCalcDropEdit.TabIndex = 3;
			this.JY_SetPointTempCalcDropEdit.UnitPreBoundMaxLength = 1;
			// 
			// JC_AirVentBoundCalcDropEdit
			// 
			this.JC_AirVentBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_AirVentBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_AirVentFlow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_AirVentFlowRateUnit)));
			this.JC_AirVentBoundCalcDropEdit.BindToAmount = "JC_AirVentFlow";
			this.JC_AirVentBoundCalcDropEdit.BindToUnit = "JC_AirVentFlowRateUnit";
			this.JC_AirVentBoundCalcDropEdit.Decimals = 2;
			this.JC_AirVentBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 101, true);
			this.JC_AirVentBoundCalcDropEdit.Name = "JC_AirVentBoundCalcDropEdit";
			this.JC_AirVentBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JC_AirVentBoundCalcDropEdit.TabIndex = 4;
			this.JC_AirVentBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// WeightTab
			// 
			this.WeightTab.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|6d2f25b0-b586-4487-99f9-fde5ec5b54ee", "Weight");
			this.WeightTab.Controls.Add(this.zCalcEdit7);
			this.WeightTab.Controls.Add(this.zDropEdit1);
			this.WeightTab.Controls.Add(this.zCalcEdit3);
			this.WeightTab.Controls.Add(this.zCalcEdit4);
			this.WeightTab.Controls.Add(this.zCalcEdit5);
			this.WeightTab.Controls.Add(this.zCalcEdit6);
			this.WeightTab.Controls.Add(this.zCalcEdit11);
			this.WeightTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WeightTab.Name = "WeightTab";
			this.WeightTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WeightTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 250, true);
			this.WeightTab.TabIndex = 3;
			// 
			// zCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "JC_Calc_Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_Calc_Height)));
			this.zCalcEdit7.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|72609929-d4fd-4c6d-a9b6-980340b600ad", "Capacity (M3.)");
			this.zCalcEdit7.DecimalPlaces = 2;
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 143, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit7.TabIndex = 25;
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "JC_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_GrossWeightUQ)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 91, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.zDropEdit1.TabIndex = 23;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "JC_DunnageWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_DunnageWeight)));
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 36, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit3.TabIndex = 21;
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_GrossWeight)));
			this.zCalcEdit4.DecimalPlaces = 2;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 65, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit4.TabIndex = 22;
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit5, "JC_Calc_NetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_Calc_NetWeight)));
			this.zCalcEdit5.DecimalPlaces = 2;
			this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 117, true);
			this.zCalcEdit5.Name = "zCalcEdit5";
			this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit5.TabIndex = 24;
			this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "JC_TareWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_TareWeight)));
			this.zCalcEdit6.DecimalPlaces = 2;
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 10, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit6.TabIndex = 20;
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit11, "JC_Calc_MaxGrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSContainer)(null)).JC_Calc_MaxGrossWeight)));
			this.zCalcEdit11.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|cfc3498e-468b-4d0b-b244-67494c44cbb0", "Max Gross Wt", "Max Gross Weight");
			this.zCalcEdit11.DecimalPlaces = 2;
			this.zCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 169, true);
			this.zCalcEdit11.Name = "zCalcEdit11";
			this.zCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCalcEdit11.TabIndex = 25;
			this.zCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ModeGroupBox
			// 
			this.ModeGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|7d3b6daf-035c-4a1f-a54b-1fc3229193ee", "Mode");
			this.ModeGroupBox.Controls.Add(this.JC_TransportModeDropEdit);
			this.ModeGroupBox.Controls.Add(this.JC_ContainerModeDropEdit);
			this.ModeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.ModeGroupBox.Name = "ModeGroupBox";
			this.ModeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 69, true);
			this.ModeGroupBox.TabIndex = 1;
			this.ModeGroupBox.TabStop = false;
			// 
			// JC_TransportModeDropEdit
			// 
			this.JC_TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_TransportModeDropEdit, "JC_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_TransportMode)));
			this.JC_TransportModeDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|07cba120-5031-4d3f-8f97-5f04689b0188", "Trans.", "Transport", "Transport Mode", "Containers Transportation Mode.");
			this.JC_TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 16, true);
			this.JC_TransportModeDropEdit.MaxItemsToShowInDropDown = 20;
			this.JC_TransportModeDropEdit.Name = "JC_TransportModeDropEdit";
			this.JC_TransportModeDropEdit.PreBoundMaxLength = 3;
			this.JC_TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.JC_TransportModeDropEdit.TabIndex = 0;
			// 
			// JC_ContainerModeDropEdit
			// 
			this.JC_ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_ContainerModeDropEdit, "JC_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_ContainerMode)));
			this.JC_ContainerModeDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|19c3498e-602c-4c08-916f-b1d39844a306", "Container", "The Container Mode.");
			this.JC_ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.JC_ContainerModeDropEdit.MaxItemsToShowInDropDown = 20;
			this.JC_ContainerModeDropEdit.Name = "JC_ContainerModeDropEdit";
			this.JC_ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.JC_ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.JC_ContainerModeDropEdit.TabIndex = 1;
			// 
			// ClientOrganisationControl
			// 
			this.ClientOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientOrganisationControl, "JC_OH_CFSClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).JC_OH_CFSClient)));
			this.ClientOrganisationControl.BindToOrganisations = "OrgDebtor_List";
			this.ClientOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 126, true);
			this.ClientOrganisationControl.Name = "ClientOrganisationControl";
			this.ClientOrganisationControl.PopupCaption = "";
			this.ClientOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ClientOrganisationControl.TabIndex = 2;
			// 
			// LoadListConsolGuidFindBox
			// 
			this.LoadListConsolGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadListConsolGuidFindBox, "JC_JK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).JC_JK)));
			this.LoadListConsolGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 25, true);
			this.LoadListConsolGuidFindBox.Name = "LoadListConsolGuidFindBox";
			this.LoadListConsolGuidFindBox.ShowDescriptionBox = false;
			this.LoadListConsolGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.LoadListConsolGuidFindBox.TabIndex = 0;
			// 
			// ArrivalDispatchTabControl
			// 
			this.ArrivalDispatchTabControl.Controls.Add(this.ContainerArrivalTabPage);
			this.ArrivalDispatchTabControl.Controls.Add(this.ContainerDispatchTabPage);
			this.ArrivalDispatchTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 11, true);
			this.ArrivalDispatchTabControl.Name = "ArrivalDispatchTabControl";
			this.ArrivalDispatchTabControl.SelectedIndex = 0;
			this.ArrivalDispatchTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 546, true);
			this.ArrivalDispatchTabControl.TabIndex = 5;
			// 
			// ContainerArrivalTabPage
			// 
			this.ContainerArrivalTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|8f4867d9-2677-40c1-bf3c-17be39da5a07", "Container Arrival");
			this.ContainerArrivalTabPage.Controls.Add(this.AUContainerArrivalPanel);
			this.ContainerArrivalTabPage.Controls.Add(this.ArrivalEmptyPanel);
			this.ContainerArrivalTabPage.Controls.Add(this.ArrivalCTOSlotPanel);
			this.ContainerArrivalTabPage.Controls.Add(this.ContainerArrivalTopPanel);
			this.ContainerArrivalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerArrivalTabPage.Name = "ContainerArrivalTabPage";
			this.ContainerArrivalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 519, true);
			this.ContainerArrivalTabPage.TabIndex = 0;
			// 
			// AUContainerArrivalPanel
			// 
			this.AUContainerArrivalPanel.Controls.Add(this.zDateEdit3);
			this.AUContainerArrivalPanel.Controls.Add(this.JS_GatePassStatusBoundTextBox);
			this.AUContainerArrivalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 414, true);
			this.AUContainerArrivalPanel.Name = "AUContainerArrivalPanel";
			this.AUContainerArrivalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 65, true);
			this.AUContainerArrivalPanel.TabIndex = 2;
			this.AUContainerArrivalPanel.Visible = false;
			// 
			// zDateEdit3
			// 
			this.zDateEdit3.AllowDrop = true;
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit3, "JC_ImpendingArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_ImpendingArrivalDate)));
			this.zDateEdit3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|68efaa08-a1e1-420c-8278-2d30b7ec8aa8", "Cargo Status Advice", "Impending Arrival Date.");
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 2, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 0;
			// 
			// JS_GatePassStatusBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_GatePassStatusBoundTextBox, "JC_ImpendingArrivalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ImpendingArrivalStatus)));
			this.JS_GatePassStatusBoundTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|49c75d3d-c91a-4a04-8938-d79d71643925", "Impending Arrival Status", "Customs Controlled Status");
			this.JS_GatePassStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.JS_GatePassStatusBoundTextBox.Name = "JS_GatePassStatusBoundTextBox";
			this.JS_GatePassStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.JS_GatePassStatusBoundTextBox.TabIndex = 1;
			// 
			// ArrivalEmptyPanel
			// 
			this.ArrivalEmptyPanel.Controls.Add(this.zGroupBox3);
			this.ArrivalEmptyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
			this.ArrivalEmptyPanel.Name = "ArrivalEmptyPanel";
			this.ArrivalEmptyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 90, true);
			this.ArrivalEmptyPanel.TabIndex = 2;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|3c634c71-92c3-4bba-8830-9b6f27422ecb", "Empty");
			this.zGroupBox3.Controls.Add(this.ContainerYardAddressAddressControl1);
			this.zGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 90, true);
			this.zGroupBox3.TabIndex = 0;
			this.zGroupBox3.TabStop = false;
			// 
			// ContainerYardAddressAddressControl1
			// 
			this.ContainerYardAddressAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerYardAddressAddressControl1, "ContainerYardAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).ContainerYardAddress)));
			this.ContainerYardAddressAddressControl1.BindToOrgList = "OrgContainerYardList";
			this.ContainerYardAddressAddressControl1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|9ed6f838-fc43-4055-a17b-5c504b72ac20", "Container Yard Address");
			this.ContainerYardAddressAddressControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardAddressAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainerYardAddressAddressControl1.Name = "ContainerYardAddressAddressControl1";
			this.ContainerYardAddressAddressControl1.PopupCaption = "";
			this.ContainerYardAddressAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 59, true);
			this.ContainerYardAddressAddressControl1.TabIndex = 0;
			// 
			// ArrivalCTOSlotPanel
			// 
			this.ArrivalCTOSlotPanel.Controls.Add(this.JC_SlotDateDateEdit1);
			this.ArrivalCTOSlotPanel.Controls.Add(this.JC_SlotReferenceTextBox1);
			this.ArrivalCTOSlotPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ArrivalCTOSlotPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.ArrivalCTOSlotPanel.Name = "ArrivalCTOSlotPanel";
			this.ArrivalCTOSlotPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 48, true);
			this.ArrivalCTOSlotPanel.TabIndex = 1;
			// 
			// JC_SlotDateDateEdit1
			// 
			this.JC_SlotDateDateEdit1.AllowDrop = true;
			this.JC_SlotDateDateEdit1.AutoCompleteMonthThreshold = 1;
			this.JC_SlotDateDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_SlotDateDateEdit1, "JC_ArrivalSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_ArrivalSlotDateTime)));
			this.JC_SlotDateDateEdit1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|d9f9a02a-cd96-4318-a4e6-eceb3e896e49", "CTO Slot Date", "Slot Date.");
			this.JC_SlotDateDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_SlotDateDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 24, true);
			this.JC_SlotDateDateEdit1.Name = "JC_SlotDateDateEdit1";
			this.JC_SlotDateDateEdit1.TabIndex = 1;
			// 
			// JC_SlotReferenceTextBox1
			// 
			this.BindingSource.SetBindingMember(this.JC_SlotReferenceTextBox1, "JC_ArrivalSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ArrivalSlotReference)));
			this.JC_SlotReferenceTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 1, true);
			this.JC_SlotReferenceTextBox1.Name = "JC_SlotReferenceTextBox1";
			this.JC_SlotReferenceTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JC_SlotReferenceTextBox1.TabIndex = 0;
			// 
			// ContainerArrivalTopPanel
			// 
			this.ContainerArrivalTopPanel.Controls.Add(this.zAddressControl1);
			this.ContainerArrivalTopPanel.Controls.Add(this.zTextBox2);
			this.ContainerArrivalTopPanel.Controls.Add(this.JC_IsSealOkCheckBox);
			this.ContainerArrivalTopPanel.Controls.Add(this.JC_TrainWagonNumberTextBox);
			this.ContainerArrivalTopPanel.Controls.Add(this.JC_ArrivalTruckRegistrationTextBox);
			this.ContainerArrivalTopPanel.Controls.Add(this.StorageLocationTextBox_ArrivalTab);
			this.ContainerArrivalTopPanel.Controls.Add(this.UnpackShedTextBox_ArrivalTab);
			this.ContainerArrivalTopPanel.Controls.Add(this.JC_ArrivalTruckDriversLicenseTextBox);
			this.ContainerArrivalTopPanel.Controls.Add(this.JC_ArrivalTimeDateEdit);
			this.ContainerArrivalTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ContainerArrivalTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerArrivalTopPanel.Name = "ContainerArrivalTopPanel";
			this.ContainerArrivalTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 206, true);
			this.ContainerArrivalTopPanel.TabIndex = 0;
			// 
			// zAddressControl1
			// 
			this.zAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zAddressControl1, "CFSArrival+EU_OA_TransportProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).CFSArrival.EU_OA_TransportProvider)));
			this.zAddressControl1.BindToOrgList = "LocalTransportList";
			this.zAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 7, true);
			this.zAddressControl1.Name = "zAddressControl1";
			this.zAddressControl1.PopupCaption = "";
			this.zAddressControl1.ShowAddress = false;
			this.zAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 42, true);
			this.zAddressControl1.StackControls = true;
			this.zAddressControl1.TabIndex = 0;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "JC_ArrivalTruckDrivers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ArrivalTruckDrivers)));
			this.zTextBox2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|282700b3-b62b-4ad7-a447-7fda79b19d6e", "Non-Staff Drivers");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 50, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.zTextBox2.TabIndex = 1;
			// 
			// JC_IsSealOkCheckBox
			// 
			this.JC_IsSealOkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JC_IsSealOkCheckBox, "JC_IsSealOk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_IsSealOk)));
			this.JC_IsSealOkCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|92cf8a3b-92a4-4ab1-88c7-dd227e341126", "Is Seal OK");
			this.JC_IsSealOkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JC_IsSealOkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 211, true);
			this.JC_IsSealOkCheckBox.Name = "JC_IsSealOkCheckBox";
			this.JC_IsSealOkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.JC_IsSealOkCheckBox.TabIndex = 8;
			this.JC_IsSealOkCheckBox.UseVisualStyleBackColor = false;
			// 
			// JC_TrainWagonNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_TrainWagonNumberTextBox, "JC_TrainWagonNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_TrainWagonNumber)));
			this.JC_TrainWagonNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 116, true);
			this.JC_TrainWagonNumberTextBox.Name = "JC_TrainWagonNumberTextBox";
			this.JC_TrainWagonNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JC_TrainWagonNumberTextBox.TabIndex = 4;
			// 
			// JC_ArrivalTruckRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ArrivalTruckRegistrationTextBox, "JC_ArrivalTruckRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ArrivalTruckRegistration)));
			this.JC_ArrivalTruckRegistrationTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|e4518568-db5f-44f1-ba3a-2ed59b09f3ab", "Vehicle Registration", "Truck Registration No.", "");
			this.JC_ArrivalTruckRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 94, true);
			this.JC_ArrivalTruckRegistrationTextBox.Name = "JC_ArrivalTruckRegistrationTextBox";
			this.JC_ArrivalTruckRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JC_ArrivalTruckRegistrationTextBox.TabIndex = 3;
			// 
			// StorageLocationTextBox_ArrivalTab
			// 
			this.BindingSource.SetBindingMember(this.StorageLocationTextBox_ArrivalTab, "JC_ContainerStorageLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ContainerStorageLocation)));
			this.StorageLocationTextBox_ArrivalTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 182, true);
			this.StorageLocationTextBox_ArrivalTab.Name = "StorageLocationTextBox_ArrivalTab";
			this.StorageLocationTextBox_ArrivalTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.StorageLocationTextBox_ArrivalTab.TabIndex = 7;
			// 
			// UnpackShedTextBox_ArrivalTab
			// 
			this.BindingSource.SetBindingMember(this.UnpackShedTextBox_ArrivalTab, "JC_UnpackShed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_UnpackShed)));
			this.UnpackShedTextBox_ArrivalTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 160, true);
			this.UnpackShedTextBox_ArrivalTab.Name = "UnpackShedTextBox_ArrivalTab";
			this.UnpackShedTextBox_ArrivalTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.UnpackShedTextBox_ArrivalTab.TabIndex = 6;
			// 
			// JC_ArrivalTruckDriversLicenseTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ArrivalTruckDriversLicenseTextBox, "JC_ArrivalTruckDriversLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ArrivalTruckDriversLicense)));
			this.JC_ArrivalTruckDriversLicenseTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|72495c6d-de86-4a66-83d1-cb68391cdc85", "Driver License", "Driver\'s Name.");
			this.JC_ArrivalTruckDriversLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 72, true);
			this.JC_ArrivalTruckDriversLicenseTextBox.Name = "JC_ArrivalTruckDriversLicenseTextBox";
			this.JC_ArrivalTruckDriversLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.JC_ArrivalTruckDriversLicenseTextBox.TabIndex = 2;
			// 
			// JC_ArrivalTimeDateEdit
			// 
			this.JC_ArrivalTimeDateEdit.AllowDrop = true;
			this.JC_ArrivalTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_ArrivalTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_ArrivalTimeDateEdit, "JC_ArrivalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_ArrivalTime)));
			this.JC_ArrivalTimeDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|d15b99c1-6fcf-425c-9d25-cda8e2c235d9", "Arrival Date", "Arrival Time.");
			this.JC_ArrivalTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_ArrivalTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 138, true);
			this.JC_ArrivalTimeDateEdit.Name = "JC_ArrivalTimeDateEdit";
			this.JC_ArrivalTimeDateEdit.TabIndex = 5;
			// 
			// ContainerDispatchTabPage
			// 
			this.ContainerDispatchTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|a9e7a3f3-d4f7-466f-88c0-8f7409f29b2d", "Container Dispatch");
			this.ContainerDispatchTabPage.Controls.Add(this.IsArrivingAtCTOByRailCheckBox);
			this.ContainerDispatchTabPage.Controls.Add(this.ExportIsDamagedCheckBox);
			this.ContainerDispatchTabPage.Controls.Add(this.ExportIsEmptyContainerCheckBox);
			this.ContainerDispatchTabPage.Controls.Add(this.AUContainerDispatchPanel);
			this.ContainerDispatchTabPage.Controls.Add(this.DispatchEmptyPanel);
			this.ContainerDispatchTabPage.Controls.Add(this.DispatchCTOSlotPanel);
			this.ContainerDispatchTabPage.Controls.Add(this.zPanel1);
			this.ContainerDispatchTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerDispatchTabPage.Name = "ContainerDispatchTabPage";
			this.ContainerDispatchTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 519, true);
			this.ContainerDispatchTabPage.TabIndex = 1;
			// 
			// IsArrivingAtCTOByRailCheckBox
			// 
			this.IsArrivingAtCTOByRailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsArrivingAtCTOByRailCheckBox, "JC_DepartureDeliveryByRail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_DepartureDeliveryByRail)));
			this.IsArrivingAtCTOByRailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsArrivingAtCTOByRailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 230, true);
			this.IsArrivingAtCTOByRailCheckBox.Name = "IsArrivingAtCTOByRailCheckBox";
			this.IsArrivingAtCTOByRailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsArrivingAtCTOByRailCheckBox.TabIndex = 1;
			this.IsArrivingAtCTOByRailCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExportIsDamagedCheckBox
			// 
			this.ExportIsDamagedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsDamagedCheckBox, "JC_IsDamaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_IsDamaged)));
			this.ExportIsDamagedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsDamagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 271, true);
			this.ExportIsDamagedCheckBox.Name = "ExportIsDamagedCheckBox";
			this.ExportIsDamagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsDamagedCheckBox.TabIndex = 3;
			// 
			// ExportIsEmptyContainerCheckBox
			// 
			this.ExportIsEmptyContainerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsEmptyContainerCheckBox, "JC_IsEmptyContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_IsEmptyContainer)));
			this.ExportIsEmptyContainerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsEmptyContainerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 250, true);
			this.ExportIsEmptyContainerCheckBox.Name = "ExportIsEmptyContainerCheckBox";
			this.ExportIsEmptyContainerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsEmptyContainerCheckBox.TabIndex = 2;
			// 
			// AUContainerDispatchPanel
			// 
			this.AUContainerDispatchPanel.Controls.Add(this.zDateEdit4);
			this.AUContainerDispatchPanel.Controls.Add(this.CargoStatusAdviceTextBox);
			this.AUContainerDispatchPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.AUContainerDispatchPanel.Name = "AUContainerDispatchPanel";
			this.AUContainerDispatchPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 51, true);
			this.AUContainerDispatchPanel.TabIndex = 5;
			// 
			// zDateEdit4
			// 
			this.zDateEdit4.AllowDrop = true;
			this.zDateEdit4.AutoCompleteMonthThreshold = 1;
			this.zDateEdit4.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit4, "JC_CargoStatusAdviceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_CargoStatusAdviceDate)));
			this.zDateEdit4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|68108867-2336-4578-bce3-84f48332f6e0", "Cargo Status Advice", "Cargo Status Advice Date", "");
			this.zDateEdit4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 2, true);
			this.zDateEdit4.Name = "zDateEdit4";
			this.zDateEdit4.TabIndex = 1;
			// 
			// CargoStatusAdviceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoStatusAdviceTextBox, "JC_CargoStatusAdviceStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_CargoStatusAdviceStatus)));
			this.CargoStatusAdviceTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|2eaf8d3e-b366-4b59-aac6-05ef41f878b0", "Cargo Status Advice Status", "Customs Controlled Status");
			this.CargoStatusAdviceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.CargoStatusAdviceTextBox.Name = "CargoStatusAdviceTextBox";
			this.CargoStatusAdviceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.CargoStatusAdviceTextBox.TabIndex = 2;
			// 
			// DispatchEmptyPanel
			// 
			this.DispatchEmptyPanel.Controls.Add(this.zGroupBox4);
			this.DispatchEmptyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 363, true);
			this.DispatchEmptyPanel.Name = "DispatchEmptyPanel";
			this.DispatchEmptyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 90, true);
			this.DispatchEmptyPanel.TabIndex = 1;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|b662ae93-c356-49eb-a9bc-5521bccdaac4", "Empty");
			this.zGroupBox4.Controls.Add(this.ContainerYardAddressAddressControl2);
			this.zGroupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 90, true);
			this.zGroupBox4.TabIndex = 0;
			this.zGroupBox4.TabStop = false;
			// 
			// ContainerYardAddressAddressControl2
			// 
			this.ContainerYardAddressAddressControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerYardAddressAddressControl2, "ContainerYardAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).ContainerYardAddress)));
			this.ContainerYardAddressAddressControl2.BindToOrgList = "OrgContainerYardList";
			this.ContainerYardAddressAddressControl2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|c2801fc8-33e6-4dd8-9daf-b1ca3cb0605f", "Container Yard Address");
			this.ContainerYardAddressAddressControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardAddressAddressControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainerYardAddressAddressControl2.Name = "ContainerYardAddressAddressControl2";
			this.ContainerYardAddressAddressControl2.PopupCaption = "";
			this.ContainerYardAddressAddressControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 59, true);
			this.ContainerYardAddressAddressControl2.TabIndex = 0;
			// 
			// DispatchCTOSlotPanel
			// 
			this.DispatchCTOSlotPanel.Controls.Add(this.JC_SlotDateDateEdit2);
			this.DispatchCTOSlotPanel.Controls.Add(this.JC_SlotReferenceTextBox2);
			this.DispatchCTOSlotPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 288, true);
			this.DispatchCTOSlotPanel.Name = "DispatchCTOSlotPanel";
			this.DispatchCTOSlotPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 54, true);
			this.DispatchCTOSlotPanel.TabIndex = 4;
			// 
			// JC_SlotDateDateEdit2
			// 
			this.JC_SlotDateDateEdit2.AllowDrop = true;
			this.JC_SlotDateDateEdit2.AutoCompleteMonthThreshold = 1;
			this.JC_SlotDateDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_SlotDateDateEdit2, "JC_DepartureSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_DepartureSlotDateTime)));
			this.JC_SlotDateDateEdit2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|600ce8ec-eb9d-4782-a736-ec8699c1ab32", "CTO Slot Date", "Departure Slot Date.");
			this.JC_SlotDateDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_SlotDateDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 29, true);
			this.JC_SlotDateDateEdit2.Name = "JC_SlotDateDateEdit2";
			this.JC_SlotDateDateEdit2.TabIndex = 3;
			// 
			// JC_SlotReferenceTextBox2
			// 
			this.BindingSource.SetBindingMember(this.JC_SlotReferenceTextBox2, "JC_DepartureSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_DepartureSlotReference)));
			this.JC_SlotReferenceTextBox2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|04c97636-a017-45c2-9878-f5deb563df99", "CTO Slot Number", "Departure Slot Reference.");
			this.JC_SlotReferenceTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 5, true);
			this.JC_SlotReferenceTextBox2.Name = "JC_SlotReferenceTextBox2";
			this.JC_SlotReferenceTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JC_SlotReferenceTextBox2.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zAddressControl2);
			this.zPanel1.Controls.Add(this.zTextBox4);
			this.zPanel1.Controls.Add(this.zTextBox3);
			this.zPanel1.Controls.Add(this.EmptyReturnByDateEdit);
			this.zPanel1.Controls.Add(this.StorageLocationTextBox_DispatchTab);
			this.zPanel1.Controls.Add(this.JC_DepartureTruckDriversLicenseTextBox);
			this.zPanel1.Controls.Add(this.DepartureDateEdit);
			this.zPanel1.Controls.Add(this.JC_DepartureTruckRegistrationTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 206, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zAddressControl2
			// 
			this.zAddressControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zAddressControl2, "CFSDispatch+EU_OA_TransportProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).CFSDispatch.EU_OA_TransportProvider)));
			this.zAddressControl2.BindToOrgList = "LocalTransportList";
			this.zAddressControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 7, true);
			this.zAddressControl2.Name = "zAddressControl2";
			this.zAddressControl2.PopupCaption = "";
			this.zAddressControl2.ShowAddress = false;
			this.zAddressControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 42, true);
			this.zAddressControl2.StackControls = true;
			this.zAddressControl2.TabIndex = 1;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "JC_DepartureTruckDrivers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_DepartureTruckDrivers)));
			this.zTextBox4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|4bf5516a-9775-4fb1-85d6-b86219211c55", "Non-Staff Drivers");
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 50, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.zTextBox4.TabIndex = 3;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "JC_TrainWagonNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_TrainWagonNumber)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 116, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.zTextBox3.TabIndex = 9;
			// 
			// EmptyReturnByDateEdit
			// 
			this.EmptyReturnByDateEdit.AllowDrop = true;
			this.EmptyReturnByDateEdit.AutoCompleteMonthThreshold = 1;
			this.EmptyReturnByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EmptyReturnByDateEdit, "JC_EmptyReturnedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_EmptyReturnedBy)));
			this.EmptyReturnByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 182, true);
			this.EmptyReturnByDateEdit.Name = "EmptyReturnByDateEdit";
			this.EmptyReturnByDateEdit.TabIndex = 15;
			// 
			// StorageLocationTextBox_DispatchTab
			// 
			this.BindingSource.SetBindingMember(this.StorageLocationTextBox_DispatchTab, "JC_ContainerStorageLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ContainerStorageLocation)));
			this.StorageLocationTextBox_DispatchTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 160, true);
			this.StorageLocationTextBox_DispatchTab.Name = "StorageLocationTextBox_DispatchTab";
			this.StorageLocationTextBox_DispatchTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.StorageLocationTextBox_DispatchTab.TabIndex = 13;
			// 
			// JC_DepartureTruckDriversLicenseTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_DepartureTruckDriversLicenseTextBox, "JC_DepartureTruckDriversLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_DepartureTruckDriversLicense)));
			this.JC_DepartureTruckDriversLicenseTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|0c99c53c-f025-418f-9b34-1f35862d72c7", "Driver License", "Driver\'s Name.");
			this.JC_DepartureTruckDriversLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 72, true);
			this.JC_DepartureTruckDriversLicenseTextBox.Name = "JC_DepartureTruckDriversLicenseTextBox";
			this.JC_DepartureTruckDriversLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.JC_DepartureTruckDriversLicenseTextBox.TabIndex = 5;
			// 
			// DepartureDateEdit
			// 
			this.DepartureDateEdit.AllowDrop = true;
			this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateEdit, "JC_DepartureTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_DepartureTime)));
			this.DepartureDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|3ae2ac13-1660-4c5a-84bd-e8f4f6d5865b", "Departure Date", "Departure Time.");
			this.DepartureDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 138, true);
			this.DepartureDateEdit.Name = "DepartureDateEdit";
			this.DepartureDateEdit.TabIndex = 11;
			// 
			// JC_DepartureTruckRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_DepartureTruckRegistrationTextBox, "JC_DepartureTruckRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_DepartureTruckRegistration)));
			this.JC_DepartureTruckRegistrationTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|79c9ae63-7bed-4890-8847-73eacfc1e418", "Vehicle Registration", "Truck Registration No.", "");
			this.JC_DepartureTruckRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 94, true);
			this.JC_DepartureTruckRegistrationTextBox.Name = "JC_DepartureTruckRegistrationTextBox";
			this.JC_DepartureTruckRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JC_DepartureTruckRegistrationTextBox.TabIndex = 7;
			// 
			// PackUnpackDatesGroupBox
			// 
			this.PackUnpackDatesGroupBox.Controls.Add(this.JC_Calc_MasterBillNumTextBox);
			this.PackUnpackDatesGroupBox.Controls.Add(this.EntryNumberTextBox);
			this.PackUnpackDatesGroupBox.Controls.Add(this.CarrierBookingRefTextBox);
			this.PackUnpackDatesGroupBox.Controls.Add(this.LoadListConsolGuidFindBox);
			this.PackUnpackDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 220, true);
			this.PackUnpackDatesGroupBox.Name = "PackUnpackDatesGroupBox";
			this.PackUnpackDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 133, true);
			this.PackUnpackDatesGroupBox.TabIndex = 1;
			this.PackUnpackDatesGroupBox.TabStop = false;
			// 
			// JC_Calc_MasterBillNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_Calc_MasterBillNumTextBox, "JC_Calc_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_Calc_MasterBillNum)));
			this.JC_Calc_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 91, true);
			this.JC_Calc_MasterBillNumTextBox.Name = "JC_Calc_MasterBillNumTextBox";
			this.JC_Calc_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.JC_Calc_MasterBillNumTextBox.TabIndex = 3;
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "JC_Calc_ConsolEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_Calc_ConsolEntryNumber)));
			this.EntryNumberTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|7d50f0f8-248f-4afb-ba61-1194e2344c51", "Entry Number", "Customs Entry Number", "");
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 69, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.EntryNumberTextBox.TabIndex = 2;
			// 
			// CarrierBookingRefTextBox
			// 
			this.CarrierBookingRefTextBox.AccessibleDescription = "";
			this.BindingSource.SetBindingMember(this.CarrierBookingRefTextBox, "JC_Calc_ConsolBookingRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_Calc_ConsolBookingRef)));
			this.CarrierBookingRefTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|2688802e-1a73-456d-ab7c-6fb922942917", "Carrier Booking Ref", "Booking Reference.");
			this.CarrierBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 47, true);
			this.CarrierBookingRefTextBox.Name = "CarrierBookingRefTextBox";
			this.CarrierBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.CarrierBookingRefTextBox.TabIndex = 1;
			// 
			// PurposeSpecificDetailsTabControl
			// 
			this.PurposeSpecificDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.PurposeSpecificDetailsTabControl.Controls.Add(this.CFSTabPage);
			this.PurposeSpecificDetailsTabControl.Controls.Add(this.StorageTabPage);
			this.PurposeSpecificDetailsTabControl.Controls.Add(this.ServicesTabPage);
			this.PurposeSpecificDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 11, true);
			this.PurposeSpecificDetailsTabControl.Name = "PurposeSpecificDetailsTabControl";
			this.PurposeSpecificDetailsTabControl.SelectedIndex = 0;
			this.PurposeSpecificDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 546, true);
			this.PurposeSpecificDetailsTabControl.TabIndex = 4;
			// 
			// CFSTabPage
			// 
			this.CFSTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|4ceebb4b-43ea-496a-ab6d-0fd5d7879096", "CFS");
			this.CFSTabPage.Controls.Add(this.registrationSailingDetailsControl1);
			this.CFSTabPage.Controls.Add(this.AdditionalContainerDetailsGroupBox);
			this.CFSTabPage.Controls.Add(this.PackUnpackDatesGroupBox);
			this.CFSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CFSTabPage.Name = "CFSTabPage";
			this.CFSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CFSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 519, true);
			this.CFSTabPage.TabIndex = 0;
			// 
			// registrationSailingDetailsControl1
			// 
			this.registrationSailingDetailsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.registrationSailingDetailsControl1, ".");
			this.registrationSailingDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 358, true);
			this.registrationSailingDetailsControl1.Name = "registrationSailingDetailsControl1";
			this.registrationSailingDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 190, true);
			this.registrationSailingDetailsControl1.TabIndex = 2;
			// 
			// AdditionalContainerDetailsGroupBox
			// 
			this.AdditionalContainerDetailsGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|2dcf73f4-0cf1-4dcd-a566-d51deddf7085", "Additional Container Details");
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.JC_PackUnpackDate_ReadonlyDateEdit);
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.AvailableStorageDatesPanel);
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.JC_ReleaseNumTextBox);
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.JC_AdditionalSealNumTextBox);
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.JC_AdditionalSealPartyDropEdit);
			this.AdditionalContainerDetailsGroupBox.Controls.Add(this.JC_ExportDepotCustomsReferenceTextBox);
			this.AdditionalContainerDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalContainerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalContainerDetailsGroupBox.Name = "AdditionalContainerDetailsGroupBox";
			this.AdditionalContainerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 210, true);
			this.AdditionalContainerDetailsGroupBox.TabIndex = 0;
			this.AdditionalContainerDetailsGroupBox.TabStop = false;
			// 
			// JC_PackUnpackDate_ReadonlyDateEdit
			// 
			this.JC_PackUnpackDate_ReadonlyDateEdit.AllowDrop = true;
			this.JC_PackUnpackDate_ReadonlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_PackUnpackDate_ReadonlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_PackUnpackDate_ReadonlyDateEdit, "JC_PackUnpackDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_PackUnpackDate)));
			this.JC_PackUnpackDate_ReadonlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_PackUnpackDate_ReadonlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 125, true);
			this.JC_PackUnpackDate_ReadonlyDateEdit.Name = "JC_PackUnpackDate_ReadonlyDateEdit";
			this.JC_PackUnpackDate_ReadonlyDateEdit.TabIndex = 4;
			// 
			// AvailableStorageDatesPanel
			// 
			this.AvailableStorageDatesPanel.Controls.Add(this.JC_LCLAvailable_ReadonlyDateEdit);
			this.AvailableStorageDatesPanel.Controls.Add(this.JC_LCLStorageCommences_ReadonlyDateEdit);
			this.AvailableStorageDatesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 148, true);
			this.AvailableStorageDatesPanel.Name = "AvailableStorageDatesPanel";
			this.AvailableStorageDatesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 48, true);
			this.AvailableStorageDatesPanel.TabIndex = 5;
			// 
			// JC_LCLAvailable_ReadonlyDateEdit
			// 
			this.JC_LCLAvailable_ReadonlyDateEdit.AllowDrop = true;
			this.JC_LCLAvailable_ReadonlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_LCLAvailable_ReadonlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_LCLAvailable_ReadonlyDateEdit, "JC_LCLAvailable_Readonly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_LCLAvailable_Readonly)));
			this.JC_LCLAvailable_ReadonlyDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|3d0bfef9-3a95-4a3a-a7a9-6114df473729", "Available From");
			this.JC_LCLAvailable_ReadonlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_LCLAvailable_ReadonlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 2, true);
			this.JC_LCLAvailable_ReadonlyDateEdit.Name = "JC_LCLAvailable_ReadonlyDateEdit";
			this.JC_LCLAvailable_ReadonlyDateEdit.TabIndex = 0;
			// 
			// JC_LCLStorageCommences_ReadonlyDateEdit
			// 
			this.JC_LCLStorageCommences_ReadonlyDateEdit.AllowDrop = true;
			this.JC_LCLStorageCommences_ReadonlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_LCLStorageCommences_ReadonlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_LCLStorageCommences_ReadonlyDateEdit, "JC_LCLStorageCommences_Readonly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_LCLStorageCommences_Readonly)));
			this.JC_LCLStorageCommences_ReadonlyDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|c776a878-4972-457f-9efe-5bffff099391", "Storage Date", "LCL Storage Commences", "");
			this.JC_LCLStorageCommences_ReadonlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_LCLStorageCommences_ReadonlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 24, true);
			this.JC_LCLStorageCommences_ReadonlyDateEdit.Name = "JC_LCLStorageCommences_ReadonlyDateEdit";
			this.JC_LCLStorageCommences_ReadonlyDateEdit.TabIndex = 1;
			// 
			// JC_ReleaseNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ReleaseNumTextBox, "JC_ReleaseNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ReleaseNum)));
			this.JC_ReleaseNumTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|9d7260f3-11f3-4095-b888-cae8867f0f23", "Booking Ref/Release", "Booking Ref/Release number.");
			this.JC_ReleaseNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 25, true);
			this.JC_ReleaseNumTextBox.Name = "JC_ReleaseNumTextBox";
			this.JC_ReleaseNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.JC_ReleaseNumTextBox.TabIndex = 0;
			// 
			// JC_AdditionalSealNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_AdditionalSealNumTextBox, "JC_AdditionalSealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_AdditionalSealNum)));
			this.JC_AdditionalSealNumTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|b7d81d25-a371-48ac-a2c8-984dc87ab886", "DPI Seal", "DPI Seal number.");
			this.JC_AdditionalSealNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 48, true);
			this.JC_AdditionalSealNumTextBox.Name = "JC_AdditionalSealNumTextBox";
			this.JC_AdditionalSealNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.JC_AdditionalSealNumTextBox.TabIndex = 1;
			// 
			// JC_ExportDepotCustomsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ExportDepotCustomsReferenceTextBox, "JC_ExportDepotCustomsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_ExportDepotCustomsReference)));
			this.JC_ExportDepotCustomsReferenceTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|4ed56457-a834-4300-8167-d1f1f6c70386", "ECN / CRN", "Export Depot Customs Reference.");
			this.JC_ExportDepotCustomsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 94, true);
			this.JC_ExportDepotCustomsReferenceTextBox.Name = "JC_ExportDepotCustomsReferenceTextBox";
			this.JC_ExportDepotCustomsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.JC_ExportDepotCustomsReferenceTextBox.TabIndex = 3;
			// 
			// StorageTabPage
			// 
			this.StorageTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|e53999e4-6943-44b1-926e-e7dbe88ebf82", "Storage");
			this.StorageTabPage.Controls.Add(this.registrationSailingDetailsControl2);
			this.StorageTabPage.Controls.Add(this.StorageDateDateEdit);
			this.StorageTabPage.Controls.Add(this.StorageMasterBillNumberTextBox);
			this.StorageTabPage.Controls.Add(this.FCLStorageArrivedUnderbondCheckBox);
			this.StorageTabPage.Controls.Add(this.FCLStorageUnderbondClearedDateEdit);
			this.StorageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StorageTabPage.Name = "StorageTabPage";
			this.StorageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StorageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 519, true);
			this.StorageTabPage.TabIndex = 1;
			// 
			// registrationSailingDetailsControl2
			// 
			this.registrationSailingDetailsControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.registrationSailingDetailsControl2, ".");
			this.registrationSailingDetailsControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 108, true);
			this.registrationSailingDetailsControl2.Name = "registrationSailingDetailsControl2";
			this.registrationSailingDetailsControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 190, true);
			this.registrationSailingDetailsControl2.TabIndex = 8;
			// 
			// StorageDateDateEdit
			// 
			this.StorageDateDateEdit.AllowDrop = true;
			this.StorageDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StorageDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StorageDateDateEdit, "JC_ArrivalCTOStorageStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_ArrivalCTOStorageStartDate)));
			this.StorageDateDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|d2f5e0f2-bcec-4523-ae68-f4b7060ecfad", "Storage Date", "FCL Storage Commences", "");
			this.StorageDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.StorageDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.StorageDateDateEdit.Name = "StorageDateDateEdit";
			this.StorageDateDateEdit.TabIndex = 3;
			// 
			// StorageMasterBillNumberTextBox
			// 
			this.StorageMasterBillNumberTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.StorageMasterBillNumberTextBox, "JC_FCLStorageModuleOnlyMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_FCLStorageModuleOnlyMaster)));
			this.StorageMasterBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 7, true);
			this.StorageMasterBillNumberTextBox.Name = "StorageMasterBillNumberTextBox";
			this.StorageMasterBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.StorageMasterBillNumberTextBox.TabIndex = 1;
			// 
			// FCLStorageArrivedUnderbondCheckBox
			// 
			this.FCLStorageArrivedUnderbondCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FCLStorageArrivedUnderbondCheckBox, "JC_FCLStorageArrivedUnderbond");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CFSContainer)(null)).JC_FCLStorageArrivedUnderbond)));
			this.FCLStorageArrivedUnderbondCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|d2d1d990-f815-4e6a-b417-dd72f953583b", "Arrived Underbond?");
			this.FCLStorageArrivedUnderbondCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FCLStorageArrivedUnderbondCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FCLStorageArrivedUnderbondCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 54, true);
			this.FCLStorageArrivedUnderbondCheckBox.Name = "FCLStorageArrivedUnderbondCheckBox";
			this.FCLStorageArrivedUnderbondCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FCLStorageArrivedUnderbondCheckBox.TabIndex = 7;
			this.FCLStorageArrivedUnderbondCheckBox.UseVisualStyleBackColor = true;
			// 
			// FCLStorageUnderbondClearedDateEdit
			// 
			this.FCLStorageUnderbondClearedDateEdit.AllowDrop = true;
			this.FCLStorageUnderbondClearedDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCLStorageUnderbondClearedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCLStorageUnderbondClearedDateEdit, "JC_FCLStorageUnderbondCleared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_FCLStorageUnderbondCleared)));
			this.FCLStorageUnderbondClearedDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|e9e422b5-0ae2-4daf-b116-12152f604322", "Underbond Cleared", "Underbond cleared date.");
			this.FCLStorageUnderbondClearedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 79, true);
			this.FCLStorageUnderbondClearedDateEdit.Name = "FCLStorageUnderbondClearedDateEdit";
			this.FCLStorageUnderbondClearedDateEdit.TabIndex = 6;
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|97706436-60b2-4da3-9f65-0c9e9a85ba5b", "Services");
			this.ServicesTabPage.Controls.Add(this.ServiceDurationTimeEditEx);
			this.ServicesTabPage.Controls.Add(this.ServiceLocationAddressDropEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceNoteTextBox);
			this.ServicesTabPage.Controls.Add(this.ServiceContractorGuidFindBox);
			this.ServicesTabPage.Controls.Add(this.ServiceCountCalcEdit);
			this.ServicesTabPage.Controls.Add(this.ServicesDateCompletedDateEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceReferencesTextBox);
			this.ServicesTabPage.Controls.Add(this.ServiceDateBookedDateEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceTypeDropEdit);
			this.ServicesTabPage.Controls.Add(this.ServicesGroupBox);
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 519, true);
			this.ServicesTabPage.TabIndex = 2;
			// 
			// ServiceDurationTimeEditEx
			// 
			this.ServiceDurationTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ServiceDurationTimeEditEx, "Services.ES_Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Duration)));
			this.ServiceDurationTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 319, true);
			this.ServiceDurationTimeEditEx.Name = "ServiceDurationTimeEditEx";
			this.ServiceDurationTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ServiceDurationTimeEditEx.TabIndex = 14;
			// 
			// ServiceLocationAddressDropEdit
			// 
			this.ServiceLocationAddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLocationAddressDropEdit, "Services.ES_Calc_LocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Calc_LocationCode)));
			this.ServiceLocationAddressDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|4ea82406-16b2-4c3f-acb7-3987c30171bb", "Location Address", "Containers Transportation Mode");
			this.ServiceLocationAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 365, true);
			this.ServiceLocationAddressDropEdit.MaxItemsToShowInDropDown = 20;
			this.ServiceLocationAddressDropEdit.Name = "ServiceLocationAddressDropEdit";
			this.ServiceLocationAddressDropEdit.PreBoundMaxLength = 35;
			this.ServiceLocationAddressDropEdit.ShowDescriptionBox = false;
			this.ServiceLocationAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ServiceLocationAddressDropEdit.TabIndex = 20;
			// 
			// ServiceNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServiceNoteTextBox, "Services.ES_ServiceNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			this.ServiceNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 388, true);
			this.ServiceNoteTextBox.Name = "ServiceNoteTextBox";
			this.ServiceNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.ServiceNoteTextBox.TabIndex = 22;
			// 
			// ServiceContractorGuidFindBox
			// 
			this.ServiceContractorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceContractorGuidFindBox, "Services.ES_OH_Contractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			this.ServiceContractorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 342, true);
			this.ServiceContractorGuidFindBox.Name = "ServiceContractorGuidFindBox";
			this.ServiceContractorGuidFindBox.PreBoundMaxLength = 6;
			this.ServiceContractorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.ServiceContractorGuidFindBox.TabIndex = 16;
			// 
			// ServiceCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ServiceCountCalcEdit, "Services.ES_ServiceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			this.ServiceCountCalcEdit.DecimalPlaces = 0;
			this.ServiceCountCalcEdit.Decimals = 0;
			this.ServiceCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 296, true);
			this.ServiceCountCalcEdit.Name = "ServiceCountCalcEdit";
			this.ServiceCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.ServiceCountCalcEdit.TabIndex = 12;
			this.ServiceCountCalcEdit.Text = "0";
			this.ServiceCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ServicesDateCompletedDateEdit
			// 
			this.ServicesDateCompletedDateEdit.AllowDrop = true;
			this.ServicesDateCompletedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ServicesDateCompletedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ServicesDateCompletedDateEdit, "Services.ES_Completed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Completed)));
			this.ServicesDateCompletedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ServicesDateCompletedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 273, true);
			this.ServicesDateCompletedDateEdit.Name = "ServicesDateCompletedDateEdit";
			this.ServicesDateCompletedDateEdit.TabIndex = 10;
			// 
			// ServiceReferencesTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServiceReferencesTextBox, "Services.ES_References");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_References)));
			this.ServiceReferencesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 250, true);
			this.ServiceReferencesTextBox.Name = "ServiceReferencesTextBox";
			this.ServiceReferencesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.ServiceReferencesTextBox.TabIndex = 8;
			// 
			// ServiceDateBookedDateEdit
			// 
			this.ServiceDateBookedDateEdit.AllowDrop = true;
			this.ServiceDateBookedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ServiceDateBookedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ServiceDateBookedDateEdit, "Services.ES_Booked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Booked)));
			this.ServiceDateBookedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ServiceDateBookedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 227, true);
			this.ServiceDateBookedDateEdit.Name = "ServiceDateBookedDateEdit";
			this.ServiceDateBookedDateEdit.TabIndex = 6;
			// 
			// ServiceTypeDropEdit
			// 
			this.ServiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceTypeDropEdit, "Services.ES_ServiceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			this.ServiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 203, true);
			this.ServiceTypeDropEdit.MaxItemsToShowInDropDown = 20;
			this.ServiceTypeDropEdit.Name = "ServiceTypeDropEdit";
			this.ServiceTypeDropEdit.PreBoundMaxLength = 3;
			this.ServiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.ServiceTypeDropEdit.TabIndex = 2;
			// 
			// ServicesGroupBox
			// 
			this.ServicesGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|92593710-8b74-483b-896c-887cc198cf43", "Services");
			this.ServicesGroupBox.Controls.Add(this.ServicesGrid);
			this.ServicesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ServicesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServicesGroupBox.Name = "ServicesGroupBox";
			this.ServicesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 190, true);
			this.ServicesGroupBox.TabIndex = 0;
			this.ServicesGroupBox.TabStop = false;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSContainer)(null)).Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Calc_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_Calc_LocationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSService)(((System.Collections.IList)(((CFSContainer)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|de02e2e8-262f-4986-a92f-845e307c791f", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ES_Calc_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ES_ServiceCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ES_Duration";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ES_OH_Contractor";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|47fb6dee-9e63-4e7b-be2b-ea33921ad8f7", "Location Address", "Containers Transportation Mode");
			zDropEditColumnStyleInfo2.ColumnName = "ES_Calc_LocationCode";
			zTextBoxColumnStyleInfo3.ColumnName = "ES_ServiceNote";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ServicesGrid.CopySelectedRowsAllowed = true;
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGrid.GridId = "d77ad819-2730-4b23-8921-7e19cd224aaf";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 171, true);
			this.ServicesGrid.TabIndex = 0;
			// 
			// PurposeGroupBox
			// 
			this.PurposeGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|6679f5b5-7ab2-4f59-81ca-ed2769bb9da2", "Purpose");
			this.PurposeGroupBox.Controls.Add(this.JC_PurposeDropEdit);
			this.PurposeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PurposeGroupBox.Name = "PurposeGroupBox";
			this.PurposeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 49, true);
			this.PurposeGroupBox.TabIndex = 0;
			this.PurposeGroupBox.TabStop = false;
			// 
			// JC_PurposeDropEdit
			// 
			this.JC_PurposeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_PurposeDropEdit, "JC_Purpose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_Purpose)));
			this.JC_PurposeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 17, true);
			this.JC_PurposeDropEdit.MaxItemsToShowInDropDown = 20;
			this.JC_PurposeDropEdit.Name = "JC_PurposeDropEdit";
			this.JC_PurposeDropEdit.PreBoundMaxLength = 3;
			this.JC_PurposeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.JC_PurposeDropEdit.TabIndex = 0;
			// 
			// JS_SealPartyDropEdit
			// 
			this.JC_SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_SealPartyDropEdit, "JC_SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_SealParty)));
			this.JC_SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 117, true);
			this.JC_SealPartyDropEdit.MaxItemsToShowInDropDown = 20;
			this.JC_SealPartyDropEdit.Name = "JC_SealPartyDropEdit";
			this.JC_SealPartyDropEdit.PreBoundMaxLength = 3;
			this.JC_SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JC_SealPartyDropEdit.ShowDescriptionBox = false;
			this.JC_SealPartyDropEdit.TabIndex = 4;
			// 
			// JC_AdditionalSealPartyDropEdit
			// 
			this.JC_AdditionalSealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_AdditionalSealPartyDropEdit, "JC_AdditionalSealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_AdditionalSealParty)));
			this.JC_AdditionalSealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 71, true);
			this.JC_AdditionalSealPartyDropEdit.MaxItemsToShowInDropDown = 20;
			this.JC_AdditionalSealPartyDropEdit.Name = "JC_AdditionalSealPartyDropEdit";
			this.JC_AdditionalSealPartyDropEdit.PreBoundMaxLength = 3;
			this.JC_AdditionalSealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JC_AdditionalSealPartyDropEdit.ShowDescriptionBox = false;
			this.JC_AdditionalSealPartyDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationDetails|6fd20f5d-13aa-4428-a7f3-2e115b8026ac", "DPI Sealed By");
			this.JC_AdditionalSealPartyDropEdit.TabIndex = 2;
			// 
			// RegistrationDetails
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PurposeGroupBox);
			this.Controls.Add(this.ModeGroupBox);
			this.Controls.Add(this.ClientOrganisationControl);
			this.Controls.Add(this.PurposeSpecificDetailsTabControl);
			this.Controls.Add(this.ContainerTabControl);
			this.Controls.Add(this.ArrivalDispatchTabControl);
			this.Name = "RegistrationDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 558, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerTabControl.ResumeLayout(false);
			this.ContainerDetailsTab.ResumeLayout(false);
			this.ContainerDetailsTab.PerformLayout();
			this.DimensionsTab.ResumeLayout(false);
			this.ReeferTab.ResumeLayout(false);
			this.ReeferTab.PerformLayout();
			this.WeightTab.ResumeLayout(false);
			this.WeightTab.PerformLayout();
			this.ModeGroupBox.ResumeLayout(false);
			this.ArrivalDispatchTabControl.ResumeLayout(false);
			this.ContainerArrivalTabPage.ResumeLayout(false);
			this.AUContainerArrivalPanel.ResumeLayout(false);
			this.AUContainerArrivalPanel.PerformLayout();
			this.ArrivalEmptyPanel.ResumeLayout(false);
			this.zGroupBox3.ResumeLayout(false);
			this.ArrivalCTOSlotPanel.ResumeLayout(false);
			this.ArrivalCTOSlotPanel.PerformLayout();
			this.ContainerArrivalTopPanel.ResumeLayout(false);
			this.ContainerArrivalTopPanel.PerformLayout();
			this.ContainerDispatchTabPage.ResumeLayout(false);
			this.ContainerDispatchTabPage.PerformLayout();
			this.AUContainerDispatchPanel.ResumeLayout(false);
			this.AUContainerDispatchPanel.PerformLayout();
			this.DispatchEmptyPanel.ResumeLayout(false);
			this.zGroupBox4.ResumeLayout(false);
			this.DispatchCTOSlotPanel.ResumeLayout(false);
			this.DispatchCTOSlotPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.PackUnpackDatesGroupBox.ResumeLayout(false);
			this.PackUnpackDatesGroupBox.PerformLayout();
			this.PurposeSpecificDetailsTabControl.ResumeLayout(false);
			this.CFSTabPage.ResumeLayout(false);
			this.AdditionalContainerDetailsGroupBox.ResumeLayout(false);
			this.AdditionalContainerDetailsGroupBox.PerformLayout();
			this.AvailableStorageDatesPanel.ResumeLayout(false);
			this.StorageTabPage.ResumeLayout(false);
			this.StorageTabPage.PerformLayout();
			this.ServicesTabPage.ResumeLayout(false);
			this.ServicesTabPage.PerformLayout();
			this.ServicesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.PurposeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}