namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class SGJobDeclarationUserControl
	{
		#region Controls

		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		protected internal Enterprise.ZArchitecture.ZLabel zLabel16;
		protected internal Enterprise.ZArchitecture.ZTextBox InwardHousebillTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox InwardBerthFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OutwardBerthCodeFindBox;
		protected internal Enterprise.ZArchitecture.ZMasterBillControl OutwardMasterBillForAirControl;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OutwardVesselCodeFindBox;
		protected internal Enterprise.ZArchitecture.ZTextBox OutwardOceanBillTextBox;
		protected internal Enterprise.ZArchitecture.ZTextBox OutwardVoyageFlightTextBox;
		protected internal Enterprise.ZArchitecture.ZTextBox OutwardHouseBillTextBox;
		protected internal Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZTextBox zTextBox6;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox placeOfReleaseFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox placeOfReceiptFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox OutwardCarrierAgentGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox InwardCarrierAgentGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		protected internal Enterprise.ZArchitecture.ZTextBox zTextBox1;
		public Enterprise.ZArchitecture.GUI.ZCheckBox IsOutwardHandCarriedCheckBox;
		protected internal ZArchitecture.ZTextBox CharterRegistrationTextBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox IsInwardHandCarriedCheckBox;
		protected internal ZArchitecture.ZCalcEdit OutwardVesselNRTCalcEdit;
		protected internal ZArchitecture.GUI.ZCodeFindBox OutwardVesselNationalityFindBox;
		protected internal ZArchitecture.GUI.ZDropEdit OutwardVesselTypeDropDownEdit;

		#endregion

		#region InitializeComponent

		void InitializeComponent()
		{
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InwardBerthFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InwardHousebillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.OutwardBerthCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OutwardMasterBillForAirControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.OutwardVesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OutwardOceanBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutwardVoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutwardHouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.placeOfReleaseFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.placeOfReceiptFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OutwardCarrierAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.InwardCarrierAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.IsInwardHandCarriedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsOutwardHandCarriedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CharterRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutwardVesselNRTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OutwardVesselNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OutwardVesselTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
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
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.InwardBerthFindBox.SuspendLayout();
			this.OutwardBerthCodeFindBox.SuspendLayout();
			this.OutwardMasterBillForAirControl.SuspendLayout();
			this.OutwardVesselCodeFindBox.SuspendLayout();
			this.placeOfReleaseFindBox.SuspendLayout();
			this.placeOfReceiptFindBox.SuspendLayout();
			this.OutwardCarrierAgentGuidFindBox.SuspendLayout();
			this.InwardCarrierAgentGuidFindBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.OutwardVesselNationalityFindBox.SuspendLayout();
			this.OutwardVesselTypeDropDownEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.BindToList = "Lookups+MessageTypeList";
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 13, true);
			this.JE_MessageTypeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|3cdb5312-498a-4c83-a50f-0dd81b797c0a", "Inward Transport");
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 76, true);
			this.JE_TransportModeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 8;
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|29063164-523c-45f3-ad97-fad2ab15314b", "Packing Type");
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 34, true);
			this.JE_ContainerModeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 4;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JE_MessageSubTypeBoundDropDownEdit, false);
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 13, true);
			this.JE_MessageSubTypeBoundDropDownEdit.ShowDescriptionBox = false;
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 2;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 15, true);
			this.WeightzCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.WeightzCalcDropEdit.TabIndex = 1;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 121, true);
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 11;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 164, true);
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 164, true);
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.BindToAmount = "JE_TotalNoOfPacksDecimal";
			this.TotalNoOfPacksCalcDropEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|a119e5f2-7e37-4f80-ae1a-2a314d04959c", "Packs (Outer)");
			this.TotalNoOfPacksCalcDropEdit.Decimals = 4;
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 36, true);
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 3;
			this.TotalNoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 0;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 142, true);
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 13;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 77, true);
			this.JE_MasterBillForAirBoundTextBox.TabIndex = 9;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|698e14ef-5a68-4b92-98e5-33bf18dc868a", "Chartered aircraft registration number.");
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 143, true);
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FolioNumberTextBox.TabIndex = 18;
			this.FolioNumberTextBox.Visible = false;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 121, true);
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselFindBox.TabIndex = 15;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 77, true);
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JE_MasterBillForSeaBoundTextBox.TabIndex = 10;
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 143, true);
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 17;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 214, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 25;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 33, true);
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 5;
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.BindToList = "Lookups+SGLocoList";
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 214, true);
			this.PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfDischargeFindBox.TabIndex = 23;
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.BindToList = "Lookups+SGLocoList";
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 33, true);
			this.PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 3;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 398, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 190, true);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.placeOfReleaseFindBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.placeOfReceiptFindBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.zDropEdit1);
			this.ShipmentTypeGroupBox.Controls.Add(this.zDropEdit2);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 358, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 210, true);
			this.ShipmentTypeGroupBox.TabIndex = 3;
			this.ShipmentTypeGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8C437665-E95D-4D24-9300-D6A800644ED3", "Declaration");
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.zDropEdit2, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.placeOfReceiptFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.placeOfReleaseFindBox, 0);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 153, true);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 99, true);
			this.FinalDestinationFindBox.ShowDescriptionBox = false;
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.FinalDestinationFindBox.TabIndex = 9;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 99, true);
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 22;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 78, true);
			this.JE_ExportDateBoundDateEdit2.TabIndex = 20;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 78, true);
			this.OriginFindBox.ShowDescriptionBox = false;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.OriginFindBox.TabIndex = 7;
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 247, true);
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 24;
			this.HouseBillParcelPostTextEdit.Visible = false;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 36, true);
			this.IncoTermDropEdit.ShowDescriptionBox = false;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IncoTermDropEdit.TabIndex = 17;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 89, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 505, true);
			this.RightTabControl.TabIndex = 7;
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 206, true);
			// 
			// ShippingOrAirLineOrganisationControl
			// 
			this.ShippingOrAirLineOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// ForwarderOrganisationControl
			// 
			this.ForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 162, true);
			this.JE_RS_NKServiceLevelBoundFindBox.ShowDescriptionBox = false;
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 15;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|b61309a3-52f1-4d96-a83c-51815d81d8ac", "Containers");
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 15, true);
			this.JE_ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 20, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 15;
			this.JE_ContainerCountCalcEdit.Visible = false;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 247, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 25;
			this.JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 36, true);
			this.IncoTermExplainButton.TabIndex = 18;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 1, true);
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 57, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 5;
			// 
			// ContainerTerminalOperatorAddressControl
			// 
			this.ContainerTerminalOperatorAddressControl.SingleLineNoGroupBoxPanelWidth = 267;
			this.ContainerTerminalOperatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// DepotAddressControl
			// 
			this.DepotAddressControl.SingleLineNoGroupBoxPanelWidth = 267;
			this.DepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BondedWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 267;
			this.BondedWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// ContainerYardAddressControl
			// 
			this.ContainerYardAddressControl.SingleLineNoGroupBoxPanelWidth = 267;
			this.ContainerYardAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 478, true);
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 185, true);
			this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 16;
			// 
			// ExternalBrokerGuidFindBox
			// 
			this.ExternalBrokerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// ControllingCustomerGuidFindBox
			// 
			this.ControllingCustomerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// ControllingAgentGuidFindBox
			// 
			this.ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardVesselTypeDropDownEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardVesselNationalityFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardVesselNRTCalcEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.CharterRegistrationTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.IsOutwardHandCarriedCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.IsInwardHandCarriedCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardCarrierAgentGuidFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.InwardCarrierAgentGuidFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.zLabel3);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardVesselCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardMasterBillForAirControl);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardOceanBillTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardVoyageFlightTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.zLabel16);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardHouseBillTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.InwardHousebillTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.InwardBerthFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.OutwardBerthCodeFindBox);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 1, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 393, true);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardBerthCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InwardBerthFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InwardHousebillTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardHouseBillTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.zLabel16, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardVoyageFlightTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardOceanBillTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardMasterBillForAirControl, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardVesselCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.zLabel3, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InwardCarrierAgentGuidFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardCarrierAgentGuidFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.IsInwardHandCarriedCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.IsOutwardHandCarriedCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CharterRegistrationTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardVesselNRTCalcEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardVesselNationalityFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OutwardVesselTypeDropDownEdit, 0);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|c1eeb997-7fb0-4d2f-8d48-f8bb76786d88", "Permit No");
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 15, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.Controls.Add(this.zTextBox6);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 1, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 80, true);
			this.DeclarationDetailsGroupBox.TabIndex = 6;
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("3D658833-3493-4AB6-88E3-356FC88963C3", "Declaration Summary");
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox6, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 38, true);
			this.StatusTextBox.Multiline = true;
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 36, true);
			this.StatusTextBox.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.JobDeclaration);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.PaymentPartyList)));
			this.zDropEdit1.BindToList = "Lookups+PaymentPartyList";
			this.zDropEdit1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|116f5dd7-1096-40d6-bca7-09307f2f8ef4", "BG Indicator");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 55, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zDropEdit1.TabIndex = 6;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "SG_OutwardTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).AddInfoLookups.TransportTypeList)));
			this.zDropEdit2.BindToList = "AddInfoLookups+TransportTypeList";
			this.zDropEdit2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|41905e6a-ebd4-4db3-852a-36712a4a997a", "Outward Transport");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 97, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.ShowDescriptionBox = false;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zDropEdit2.TabIndex = 10;
			// 
			// InwardBerthFindBox
			// 
			this.InwardBerthFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InwardBerthFindBox, "SG_US_NKInwardVesselBerth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKInwardVesselBerth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).AddInfoLookups.SGCPlacesList)));
			this.InwardBerthFindBox.BindToList = "AddInfoLookups+SGCPlacesList";
			this.InwardBerthFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|cd666141-4fde-44fe-acf0-1eff8b2b2195", "Berth");
			this.InwardBerthFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 165, true);
			this.InwardBerthFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.InwardBerthFindBox.Name = "InwardBerthFindBox";
			this.InwardBerthFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InwardBerthFindBox.ParentType = null;
			this.InwardBerthFindBox.PreBoundMaxLength = 6;
			this.InwardBerthFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.InwardBerthFindBox.TabIndex = 20;
			// 
			// InwardHousebillTextBox
			// 
			this.BindingSource.SetBindingMember(this.InwardHousebillTextBox, "JE_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_HouseBill)));
			this.InwardHousebillTextBox.CaptionResourceString = null;
			this.InwardHousebillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 99, true);
			this.InwardHousebillTextBox.Name = "InwardHousebillTextBox";
			this.InwardHousebillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.InwardHousebillTextBox.TabIndex = 13;
			// 
			// zLabel16
			// 
			this.zLabel16.AutoSize = true;
			this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel16.IsFontBold = true;
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 195, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.zLabel16.TabIndex = 21;
			this.zLabel16.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8A6D263E-332F-4CEF-9678-548F8D78E0D4", "Outward");
			// 
			// OutwardBerthCodeFindBox
			// 
			this.OutwardBerthCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutwardBerthCodeFindBox, "SG_US_NKOutwardVesselBerth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKOutwardVesselBerth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).AddInfoLookups.SGCPlacesList)));
			this.OutwardBerthCodeFindBox.BindToList = "AddInfoLookups+SGCPlacesList";
			this.OutwardBerthCodeFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|3ffaecca-90f3-47e3-9ba9-af071dd107bf", "Berth");
			this.OutwardBerthCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 346, true);
			this.OutwardBerthCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.OutwardBerthCodeFindBox.Name = "OutwardBerthCodeFindBox";
			this.OutwardBerthCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OutwardBerthCodeFindBox.ParentType = null;
			this.OutwardBerthCodeFindBox.PreBoundMaxLength = 6;
			this.OutwardBerthCodeFindBox.ShowDescriptionBox = false;
			this.OutwardBerthCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.OutwardBerthCodeFindBox.TabIndex = 41;
			// 
			// OutwardMasterBillForAirControl
			// 
			this.OutwardMasterBillForAirControl.AllowAlphaInMAWP = false;
			this.OutwardMasterBillForAirControl.AllowDrop = true;
			this.OutwardMasterBillForAirControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.OutwardMasterBillForAirControl, "SG_OutwardMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardMAWB)));
			this.OutwardMasterBillForAirControl.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|1a7fb694-1201-4ade-9b1e-15e78c6c0bf1", "Master Bill");
			this.OutwardMasterBillForAirControl.FormattedMasterBill = "";
			this.OutwardMasterBillForAirControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 258, true);
			this.OutwardMasterBillForAirControl.Name = "OutwardMasterBillForAirControl";
			this.OutwardMasterBillForAirControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.OutwardMasterBillForAirControl.TabIndex = 29;
			// 
			// OutwardVesselCodeFindBox
			// 
			this.OutwardVesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutwardVesselCodeFindBox, "SG_OutwardVesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardVesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Vessels)));
			this.OutwardVesselCodeFindBox.BindToList = "Lookups+Vessels";
			this.OutwardVesselCodeFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|f484325c-a407-408b-b3fd-a398c1085012", "Vessel");
			this.OutwardVesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 302, true);
			this.OutwardVesselCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.OutwardVesselCodeFindBox.Name = "OutwardVesselCodeFindBox";
			this.OutwardVesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OutwardVesselCodeFindBox.ParentType = null;
			this.OutwardVesselCodeFindBox.PreBoundMaxLength = 35;
			this.OutwardVesselCodeFindBox.ShowDescriptionBox = false;
			this.OutwardVesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OutwardVesselCodeFindBox.TabIndex = 35;
			// 
			// OutwardOceanBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutwardOceanBillTextBox, "SG_OutwardMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardMAWB)));
			this.OutwardOceanBillTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|5f5a358e-9ec4-4d95-871c-e04b2f3b0f05", "Ocean Bill");
			this.OutwardOceanBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 258, true);
			this.OutwardOceanBillTextBox.Name = "OutwardOceanBillTextBox";
			this.OutwardOceanBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OutwardOceanBillTextBox.TabIndex = 30;
			// 
			// OutwardVoyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutwardVoyageFlightTextBox, "SG_OutwardVoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardVoyageFlightNo)));
			this.OutwardVoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 324, true);
			this.OutwardVoyageFlightTextBox.Name = "OutwardVoyageFlightTextBox";
			this.OutwardVoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.OutwardVoyageFlightTextBox.TabIndex = 37;
			// 
			// OutwardHouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutwardHouseBillTextBox, "SG_OutwardHAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardHAWB)));
			this.OutwardHouseBillTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|0698a557-a514-4c9e-813f-219ee65f5a73", "House Bill");
			this.OutwardHouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 280, true);
			this.OutwardHouseBillTextBox.Name = "OutwardHouseBillTextBox";
			this.OutwardHouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OutwardHouseBillTextBox.TabIndex = 33;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9611F1C8-4BAC-4B59-8D59-C733B33E43AD", "Inward");
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.zLabel3.TabIndex = 1;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "CertificateNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CertificateNumber)));
			this.zTextBox6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|50691510-aeee-445d-b8f9-35226010ed13", "Cert. No.");
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 15, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.zTextBox6.TabIndex = 3;
			// 
			// placeOfReleaseFindBox
			// 
			this.placeOfReleaseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.placeOfReleaseFindBox, "SG_US_NKPlaceOfCargoRelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKPlaceOfCargoRelease)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGPlacesCodeList)));
			this.placeOfReleaseFindBox.BindToList = "Lookups+SGPlacesCodeList";
			this.placeOfReleaseFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|81461fc8-6919-4af0-adaf-b467b7a2e03f", "Place of Release");
			this.placeOfReleaseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 118, true);
			this.placeOfReleaseFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.placeOfReleaseFindBox.Name = "placeOfReleaseFindBox";
			this.placeOfReleaseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.placeOfReleaseFindBox.ParentType = null;
			this.placeOfReleaseFindBox.PreBoundMaxLength = 6;
			this.placeOfReleaseFindBox.ShowDescriptionBox = false;
			this.placeOfReleaseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.placeOfReleaseFindBox.TabIndex = 12;
			// 
			// placeOfReceiptFindBox
			// 
			this.placeOfReceiptFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.placeOfReceiptFindBox, "SG_US_NKPlaceOfReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKPlaceOfReceipt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGPlacesCodeList)));
			this.placeOfReceiptFindBox.BindToList = "Lookups+SGPlacesCodeList";
			this.placeOfReceiptFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|ef30117e-63dc-4064-b807-3211c2aadf51", "Place of Receipt");
			this.placeOfReceiptFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 140, true);
			this.placeOfReceiptFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.placeOfReceiptFindBox.Name = "placeOfReceiptFindBox";
			this.placeOfReceiptFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.placeOfReceiptFindBox.ParentType = null;
			this.placeOfReceiptFindBox.PreBoundMaxLength = 6;
			this.placeOfReceiptFindBox.ShowDescriptionBox = false;
			this.placeOfReceiptFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.placeOfReceiptFindBox.TabIndex = 14;
			// 
			// OutwardCarrierAgentGuidFindBox
			// 
			this.OutwardCarrierAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutwardCarrierAgentGuidFindBox, "OutwardShippingLineForwarderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).OutwardShippingLineForwarderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
			this.OutwardCarrierAgentGuidFindBox.BindToList = "Lookups+Organisations";
			this.OutwardCarrierAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|c197bf74-6ece-4884-9626-3b2ef3e3d3a3", "Carrier Agent");
			this.OutwardCarrierAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 236, true);
			this.OutwardCarrierAgentGuidFindBox.Name = "OutwardCarrierAgentGuidFindBox";
			this.OutwardCarrierAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OutwardCarrierAgentGuidFindBox.ParentType = null;
			this.OutwardCarrierAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OutwardCarrierAgentGuidFindBox.TabIndex = 27;
			// 
			// InwardCarrierAgentGuidFindBox
			// 
			this.InwardCarrierAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InwardCarrierAgentGuidFindBox, "JE_OH_InwardCarrierAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_InwardCarrierAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
			this.InwardCarrierAgentGuidFindBox.BindToList = "Lookups+Organisations";
			this.InwardCarrierAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|d19d57d3-06f7-41a7-b95c-bed26088b5f7", "Carrier Agent");
			this.InwardCarrierAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 55, true);
			this.InwardCarrierAgentGuidFindBox.Name = "InwardCarrierAgentGuidFindBox";
			this.InwardCarrierAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InwardCarrierAgentGuidFindBox.ParentType = null;
			this.InwardCarrierAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.InwardCarrierAgentGuidFindBox.TabIndex = 7;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 307, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 45, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8F015DB8-5296-4B20-BEEF-F2C8BD7D9AC5", "Importer Name Override");
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "SG_ImporterNameOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ImporterNameOverride)));
			this.zTextBox1.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 18, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.zTextBox1.TabIndex = 32;
			// 
			// IsInwardHandCarriedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsInwardHandCarriedCheckBox, "SG_IsInwardHandCarried");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_IsInwardHandCarried)));
			this.IsInwardHandCarriedCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|61F2AD78-A694-4BD9-A3A1-998E0A839D0D", "Is Hand-carried");
			this.IsInwardHandCarriedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 77, true);
			this.IsInwardHandCarriedCheckBox.Name = "IsInwardHandCarriedCheckBox";
			this.IsInwardHandCarriedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.IsInwardHandCarriedCheckBox.TabIndex = 11;
			this.IsInwardHandCarriedCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsOutwardHandCarriedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsOutwardHandCarriedCheckBox, "SG_IsOutwardHandCarried");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_IsOutwardHandCarried)));
			this.IsOutwardHandCarriedCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|61F2AD78-A694-4BD9-A3A1-998E0A839D0D", "Is Hand-carried");
			this.IsOutwardHandCarriedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 258, true);
			this.IsOutwardHandCarriedCheckBox.Name = "IsOutwardHandCarriedCheckBox";
			this.IsOutwardHandCarriedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.IsOutwardHandCarriedCheckBox.TabIndex = 31;
			this.IsOutwardHandCarriedCheckBox.UseVisualStyleBackColor = true;
			// 
			// CharterRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.CharterRegistrationTextBox, "SG_OutwardFolio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_OutwardFolio)));
			this.CharterRegistrationTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGJobDeclarationUserControl|e9f9d06c-1761-4b73-8fc6-b9ce2b24dbb4", "", "Chartered aircraft registration number.");
			this.CharterRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 324, true);
			this.CharterRegistrationTextBox.Name = "CharterRegistrationTextBox";
			this.CharterRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CharterRegistrationTextBox.TabIndex = 40;
			// 
			// OutwardVesselNRTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OutwardVesselNRTCalcEdit, "SGE_OutwardVesselNRT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SGE_OutwardVesselNRT)));
			this.OutwardVesselNRTCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9678d4cd-47b2-4771-ba06-9163afe71553", "Net Registered Tonnage", "Outward Vessel Net Registered Tonnage");
			this.OutwardVesselNRTCalcEdit.DecimalPlaces = 0;
			this.OutwardVesselNRTCalcEdit.Decimals = 0;
			this.OutwardVesselNRTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 324, true);
			this.OutwardVesselNRTCalcEdit.Name = "OutwardVesselNRTCalcEdit";
			this.OutwardVesselNRTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.OutwardVesselNRTCalcEdit.TabIndex = 42;
			this.OutwardVesselNRTCalcEdit.Text = "0";
			this.OutwardVesselNRTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OutwardVesselNationalityFindBox
			// 
			this.OutwardVesselNationalityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutwardVesselNationalityFindBox, "SGE_RN_NKOutwardVesselNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SGE_RN_NKOutwardVesselNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.CountryCodeList)));
			this.OutwardVesselNationalityFindBox.BindToList = "Lookups+CountryCodeList";
			this.OutwardVesselNationalityFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("eb43fdf0-8707-4a55-8498-93c61481a5df", "", "Nationality", "Ctry", "Outward Vessel Nationality");
			this.OutwardVesselNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 346, true);
			this.OutwardVesselNationalityFindBox.Name = "OutwardVesselNationalityFindBox";
			this.OutwardVesselNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OutwardVesselNationalityFindBox.ParentType = null;
			this.OutwardVesselNationalityFindBox.PopupCaption = "Select the Vessel Registered Nationality";
			this.OutwardVesselNationalityFindBox.PreBoundMaxLength = 2;
			this.OutwardVesselNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.OutwardVesselNationalityFindBox.TabIndex = 44;
			// 
			// OutwardVesselTypeDropDownEdit
			// 
			this.OutwardVesselTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutwardVesselTypeDropDownEdit, "SGE_OutwardVesselType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SGE_OutwardVesselType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.VesselTypeList)));
			this.OutwardVesselTypeDropDownEdit.BindToList = "Lookups+VesselTypeList";
			this.OutwardVesselTypeDropDownEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6cd401f4-50ef-4684-82dd-0c849f6ff825", "Vessel Type");
			this.OutwardVesselTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 367, true);
			this.OutwardVesselTypeDropDownEdit.Name = "OutwardVesselTypeDropDownEdit";
			this.OutwardVesselTypeDropDownEdit.PreBoundMaxLength = 3;
			this.OutwardVesselTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.OutwardVesselTypeDropDownEdit.TabIndex = 45;
			// 
			// SGJobDeclarationUserControl
			// 
			this.Controls.Add(this.zGroupBox1);
			this.Name = "SGJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 597, true);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
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
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.InwardBerthFindBox.ResumeLayout(true);
			this.InwardBerthFindBox.PerformLayout();
			this.OutwardBerthCodeFindBox.ResumeLayout(true);
			this.OutwardBerthCodeFindBox.PerformLayout();
			this.OutwardMasterBillForAirControl.ResumeLayout(true);
			this.OutwardMasterBillForAirControl.PerformLayout();
			this.OutwardVesselCodeFindBox.ResumeLayout(true);
			this.OutwardVesselCodeFindBox.PerformLayout();
			this.placeOfReleaseFindBox.ResumeLayout(true);
			this.placeOfReleaseFindBox.PerformLayout();
			this.placeOfReceiptFindBox.ResumeLayout(true);
			this.placeOfReceiptFindBox.PerformLayout();
			this.OutwardCarrierAgentGuidFindBox.ResumeLayout(true);
			this.OutwardCarrierAgentGuidFindBox.PerformLayout();
			this.InwardCarrierAgentGuidFindBox.ResumeLayout(true);
			this.InwardCarrierAgentGuidFindBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.OutwardVesselNationalityFindBox.ResumeLayout(true);
			this.OutwardVesselNationalityFindBox.PerformLayout();
			this.OutwardVesselTypeDropDownEdit.ResumeLayout(true);
			this.OutwardVesselTypeDropDownEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
