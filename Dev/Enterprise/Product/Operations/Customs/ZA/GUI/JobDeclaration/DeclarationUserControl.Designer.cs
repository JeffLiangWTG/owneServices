using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ZADeclarationUserControl
	{


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            this.issuedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.issuedAtCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.houseBillIssuedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.uZ_CargoCarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.paidByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.marksAndNumbersStmNotePopupButton = new Enterprise.ZArchitecture.GUI.ZStmNotePopupButton();
            this.marksAndNumbersZTextbox = new Enterprise.ZArchitecture.ZTextBox();
            this.transitManifestTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.agentsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.masterCargoCarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.jE_CustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.jE_LocationOfGoodsCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.jE_GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.rulesOfOriginCertificateTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.agentCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.rooTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UZ_RadioCallSignTextBox = new Enterprise.Customs.ZA.GUI.RadioCallSignCodeFindBox();
            this.uZ_CarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.uZ_Trailer1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.uZ_Trailer2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.jE_MasterBillForAirBoundNonIATATextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.nonIATAFormatCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.removalTransportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zCodeFindBoxVesselAgent = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
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
            this.OrdersAttachUserControl.SuspendLayout();
            this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
            this.ExternalBrokerGuidFindBox.SuspendLayout();
            this.ControllingCustomerGuidFindBox.SuspendLayout();
            this.ControllingAgentGuidFindBox.SuspendLayout();
            this.TransportDetailsGroupBox.SuspendLayout();
            this.DeclarationDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.issuedDateDateEdit.SuspendLayout();
            this.issuedAtCodeFindBox.SuspendLayout();
            this.houseBillIssuedDateDateEdit.SuspendLayout();
            this.uZ_CargoCarrierCodeFindBox.SuspendLayout();
            this.paidByDropEdit.SuspendLayout();
            this.masterCargoCarrierCodeFindBox.SuspendLayout();
            this.jE_CustomsOfficeDropEdit.SuspendLayout();
            this.jE_LocationOfGoodsCodeFindBox.SuspendLayout();
            this.jE_GoodsOriginCodeFindBox.SuspendLayout();
            this.rooTypeDropEdit.SuspendLayout();
            this.UZ_RadioCallSignTextBox.SuspendLayout();
            this.uZ_CarrierCodeFindBox.SuspendLayout();
            this.removalTransportCodeDropEdit.SuspendLayout();
            this.zCodeFindBoxVesselAgent.SuspendLayout();
            this.SuspendLayout();
            // 
            // JE_MessageTypeBoundDropDownEdit
            // 
            this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 62, true);
            this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.JE_MessageTypeBoundDropDownEdit.TabIndex = 2;
            // 
            // JE_TransportModeBoundDropDownEdit
            // 
            this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 106, true);
            this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.JE_TransportModeBoundDropDownEdit.TabIndex = 4;
            // 
            // JE_ContainerModeBoundDropDownEdit
            // 
            this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 172, true);
            this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_MessageSubTypeBoundDropDownEdit, JobDeclaration.Schema.JE_ExportGoodsType);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_ExportGoodsType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.JE_ExportGoodsType_List)));
			this.JE_MessageSubTypeBoundDropDownEdit.BindToList = "Lookups+JE_ExportGoodsType_List";
            this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 194, true);
            this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 8;
            this.JE_MessageSubTypeBoundDropDownEdit.Visible = false;
            // 
            // WeightzCalcDropEdit
            // 
            this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 190, true);
            this.WeightzCalcDropEdit.TabIndex = 12;
            // 
            // OwnersReferenceTextBox
            // 
            this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 168, true);
            this.OwnersReferenceTextBox.TabIndex = 11;
            // 
            // ScreeningStatusDropEdit
            // 
            this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 256, true);
            this.ScreeningStatusDropEdit.TabIndex = 18;
            // 
            // ScreenButton
            // 
            this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 256, true);
            this.ScreenButton.TabIndex = 19;
            // 
            // TotalNoOfPacksCalcDropEdit
            // 
            this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 212, true);
            this.TotalNoOfPacksCalcDropEdit.TabIndex = 15;
            // 
            // GoodsDescriptionTextBox
            // 
            this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 146, true);
            this.GoodsDescriptionTextBox.TabIndex = 10;
            // 
            // JE_MasterBillForAirBoundTextBox
            // 
            this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
            this.JE_MasterBillForAirBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 18, true);
            this.JE_MasterBillForAirBoundTextBox.TabIndex = 1;
            // 
            // FolioNumberTextBox
            // 
            this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 88, true);
            this.FolioNumberTextBox.TabStop = false;
            this.FolioNumberTextBox.Visible = false;
            // 
            // VesselFindBox
            // 
            this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
            // 
            // JE_MasterBillForSeaBoundTextBox
            // 
            this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
            this.JE_MasterBillForSeaBoundTextBox.TabIndex = 1;
            // 
            // JE_VoyageFlightNoBoundTextBox
            // 
            this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 88, true);
            // 
            // JE_ExportDateBoundDateEdit
            // 
            this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 159, true);
            this.JE_ExportDateBoundDateEdit.TabIndex = 16;
            // 
            // JE_DateOfArrivalBoundDateEdit
            // 
            this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 183, true);
            this.JE_DateOfArrivalBoundDateEdit.TabIndex = 18;
            // 
            // PortOfDischargeFindBox
            // 
            this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 183, true);
            this.PortOfDischargeFindBox.TabIndex = 17;
            // 
            // PortOfLoadingFindBox
            // 
            this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 159, true);
            this.PortOfLoadingFindBox.TabIndex = 15;
            // 
            // ShipmentDetailsGroupBox
            // 
            this.ShipmentDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ShipmentDetailsGroupBox.Controls.Add(this.rooTypeDropEdit);
            this.ShipmentDetailsGroupBox.Controls.Add(this.rulesOfOriginCertificateTextBox);
            this.ShipmentDetailsGroupBox.Controls.Add(this.jE_GoodsOriginCodeFindBox);
            this.ShipmentDetailsGroupBox.Controls.Add(this.agentsReferenceTextBox);
            this.ShipmentDetailsGroupBox.Controls.Add(this.marksAndNumbersStmNotePopupButton);
            this.ShipmentDetailsGroupBox.Controls.Add(this.uZ_CargoCarrierCodeFindBox);
            this.ShipmentDetailsGroupBox.Controls.Add(this.houseBillIssuedDateDateEdit);
            this.ShipmentDetailsGroupBox.Controls.Add(this.marksAndNumbersZTextbox);
            this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 288, true);
            this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 326, true);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.marksAndNumbersZTextbox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.houseBillIssuedDateDateEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.uZ_CargoCarrierCodeFindBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.marksAndNumbersStmNotePopupButton, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.agentsReferenceTextBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.jE_GoodsOriginCodeFindBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.rulesOfOriginCertificateTextBox, 0);
            this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.rooTypeDropEdit, 0);
            // 
            // ShipmentTypeGroupBox
            // 
            this.ShipmentTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ShipmentTypeGroupBox.Controls.Add(this.removalTransportCodeDropEdit);
            this.ShipmentTypeGroupBox.Controls.Add(this.agentCodeTextBox);
            this.ShipmentTypeGroupBox.Controls.Add(this.jE_CustomsOfficeDropEdit);
            this.ShipmentTypeGroupBox.Controls.Add(this.paidByDropEdit);
            this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
            this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 286, true);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.paidByDropEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.jE_CustomsOfficeDropEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.agentCodeTextBox, 0);
            this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.removalTransportCodeDropEdit, 0);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Captions = new string[] {
		"Importer" };
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 168, true);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Captions = new string[] {
		"Main Supplier" };
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 80, true);
            this.FinalDestinationFindBox.TabIndex = 5;
            // 
            // JE_DateOfArrivalBoundDateEdit2
            // 
            this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 81, true);
            this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 6;
            // 
            // JE_ExportDateBoundDateEdit2
            // 
            this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 59, true);
            this.JE_ExportDateBoundDateEdit2.TabIndex = 4;
            // 
            // OriginFindBox
            // 
            this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 58, true);
            // 
            // HouseBillParcelPostTextEdit
            // 
            this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 14, true);
            this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.HouseBillParcelPostTextEdit.TabIndex = 0;
            // 
            // IncoTermDropEdit
            // 
            this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 234, true);
            this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.IncoTermDropEdit.TabIndex = 16;
            // 
            // RightTabControl
            // 
            this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 8, true);
            this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 606, true);
            // 
            // OrganisationsTabPage
            // 
            this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // OrganisationsTopPanel
            // 
            this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 206, true);
            // 
            // OrdersTabPage
            // 
            this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // JE_RS_NKServiceLevelBoundFindBox
            // 
            this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 84, true);
            this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 3;
            // 
            // DocsTabPage
            // 
            this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // JE_ContainerCountCalcEdit
            // 
            this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 190, true);
            this.JE_ContainerCountCalcEdit.TabIndex = 14;
            // 
            // JE_TotalNoOfPiecesBoundCalcEdit
            // 
            this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 189, true);
            this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 16;
            // 
            // IncoTermExplainButton
            // 
            this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 235, true);
            this.IncoTermExplainButton.TabIndex = 17;
            // 
            // OverrideValuesCheckBox
            // 
            this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
            // 
            // VolumeCalcDropEdit
            // 
            this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 191, true);
            this.VolumeCalcDropEdit.TabIndex = 13;
            // 
            // ShipmentCustomFieldsPage
            // 
            this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // shipmentCustomFieldsControl1
            // 
            this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // OrdersPanel
            // 
            this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // NumbersTabPage
            // 
            this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 579, true);
            // 
            // OrdersAttachUserControl
            // 
            this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 418, true);
            // 
            // JE_ApplicationCodeBoundDropEdit
            // 
            this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 40, true);
            this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.JE_ApplicationCodeBoundDropEdit.TabIndex = 1;
            // 
            // TransportDetailsGroupBox
            // 
            this.TransportDetailsGroupBox.Controls.Add(this.zCodeFindBoxVesselAgent);
            this.TransportDetailsGroupBox.Controls.Add(this.uZ_Trailer2TextBox);
            this.TransportDetailsGroupBox.Controls.Add(this.uZ_Trailer1TextBox);
            this.TransportDetailsGroupBox.Controls.Add(this.uZ_CarrierCodeFindBox);
            this.TransportDetailsGroupBox.Controls.Add(this.UZ_RadioCallSignTextBox);
            this.TransportDetailsGroupBox.Controls.Add(this.jE_LocationOfGoodsCodeFindBox);
            this.TransportDetailsGroupBox.Controls.Add(this.masterCargoCarrierCodeFindBox);
            this.TransportDetailsGroupBox.Controls.Add(this.issuedAtCodeFindBox);
            this.TransportDetailsGroupBox.Controls.Add(this.issuedDateDateEdit);
            this.TransportDetailsGroupBox.Controls.Add(this.nonIATAFormatCheckBox);
            this.TransportDetailsGroupBox.Controls.Add(this.jE_MasterBillForAirBoundNonIATATextBox);
            this.TransportDetailsGroupBox.Controls.Add(this.transitManifestTextBox);
            this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 75, true);
            this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 208, true);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.transitManifestTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.jE_MasterBillForAirBoundNonIATATextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.nonIATAFormatCheckBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.issuedDateDateEdit, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.issuedAtCodeFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.masterCargoCarrierCodeFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.jE_LocationOfGoodsCodeFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.UZ_RadioCallSignTextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.uZ_CarrierCodeFindBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.uZ_Trailer1TextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.uZ_Trailer2TextBox, 0);
            this.TransportDetailsGroupBox.Controls.SetChildIndex(this.zCodeFindBoxVesselAgent, 0);
            // 
            // ExportDeclarationNumberBoundTextBox
            // 
            this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            this.ExportDeclarationNumberBoundTextBox.TabIndex = 0;
            // 
            // DeclarationDetailsGroupBox
            // 
            this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 62, true);
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 37, true);
            this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            this.StatusTextBox.TabIndex = 1;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobDeclaration);
            // 
            // issuedDateDateEdit
            // 
            this.issuedDateDateEdit.AllowDrop = true;
            this.issuedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.issuedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.issuedDateDateEdit, JobDeclaration.Schema.JE_MasterBillIssuedDate);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_MasterBillIssuedDate)));
			this.issuedDateDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|492baeca-54c4-48e4-a357-f992f302fe34", "Date");
            this.issuedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 40, true);
            this.issuedDateDateEdit.Name = "issuedDateDateEdit";
            this.issuedDateDateEdit.TabIndex = 4;
            // 
            // issuedAtCodeFindBox
            // 
            this.issuedAtCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.issuedAtCodeFindBox, JobDeclaration.Schema.JE_RL_NKMasterBillIssuedAt);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_RL_NKMasterBillIssuedAt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.UZ_RL_NKMasterBillIssuedAtList)));
			this.issuedAtCodeFindBox.BindToList = "Lookups.UZ_RL_NKMasterBillIssuedAtList";
            this.issuedAtCodeFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|db4b2a40-2011-4f41-a1cd-dd35cfaa7634", "Issued At");
            this.issuedAtCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
            this.issuedAtCodeFindBox.Name = "issuedAtCodeFindBox";
            this.issuedAtCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.issuedAtCodeFindBox.ParentType = null;
            this.issuedAtCodeFindBox.PreBoundMaxLength = 5;
            this.issuedAtCodeFindBox.ShowDescriptionBox = false;
            this.issuedAtCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.issuedAtCodeFindBox.TabIndex = 3;
            // 
            // houseBillIssuedDateDateEdit
            // 
            this.houseBillIssuedDateDateEdit.AllowDrop = true;
            this.houseBillIssuedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.houseBillIssuedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.houseBillIssuedDateDateEdit, "HouseBillIssuedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).HouseBillIssuedDate)));
            this.houseBillIssuedDateDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|5952dbd0-acd4-41be-be39-6e87171ac5bc", "Date");
            this.houseBillIssuedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 14, true);
            this.houseBillIssuedDateDateEdit.Name = "houseBillIssuedDateDateEdit";
            this.houseBillIssuedDateDateEdit.TabIndex = 1;
            // 
            // uZ_CargoCarrierCodeFindBox
            // 
            this.uZ_CargoCarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uZ_CargoCarrierCodeFindBox, JobDeclaration.Schema.JE_CargoCarrier);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_CargoCarrier)));
			this.uZ_CargoCarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|8db4e283-5da1-4780-a537-3c8d31410553", "Cargo Carrier");
            this.uZ_CargoCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 36, true);
            this.uZ_CargoCarrierCodeFindBox.Name = "uZ_CargoCarrierCodeFindBox";
            this.uZ_CargoCarrierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.uZ_CargoCarrierCodeFindBox.ParentType = null;
            this.uZ_CargoCarrierCodeFindBox.ShowDescriptionBox = false;
            this.uZ_CargoCarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.uZ_CargoCarrierCodeFindBox.TabIndex = 2;
            // 
            // paidByDropEdit
            // 
            this.paidByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.paidByDropEdit, JobDeclaration.Schema.JE_PaymentMethod);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.PaymentPartyList)));
			this.paidByDropEdit.BindToList = "Lookups.PaymentPartyList";
            this.paidByDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|B741ACE8-BCCB-436B-97FA-5B2694EE47B5", "Paid By");
            this.paidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 150, true);
            this.paidByDropEdit.Name = "paidByDropEdit";
            this.paidByDropEdit.PreBoundMaxLength = 3;
            this.paidByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.paidByDropEdit.TabIndex = 6;
            // 
            // marksAndNumbersStmNotePopupButton
            // 
            this.marksAndNumbersStmNotePopupButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("46aabe6e-976b-496f-8cdc-ffc7b47e0ea3", "More ...");
            this.marksAndNumbersStmNotePopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 278, true);
            this.marksAndNumbersStmNotePopupButton.Name = "marksAndNumbersStmNotePopupButton";
            this.marksAndNumbersStmNotePopupButton.NoteType = "Marks & Numbers";
            this.marksAndNumbersStmNotePopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
            this.marksAndNumbersStmNotePopupButton.TabIndex = 21;
            this.marksAndNumbersStmNotePopupButton.ToolTipCaption = null;
			// 
			// marksAndNumbersZTextbox
			// 
			this.BindingSource.SetBindingMember(this.marksAndNumbersZTextbox, JobDeclaration.Schema.JE_MarksAndNumbersShort);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_MarksAndNumbersShort)));
			this.marksAndNumbersZTextbox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|7bf1c575-f35c-467b-84a2-294129819d1d", "Marks & Numbers");
            this.marksAndNumbersZTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 278, true);
            this.marksAndNumbersZTextbox.Name = "marksAndNumbersZTextbox";
            this.marksAndNumbersZTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.marksAndNumbersZTextbox.TabIndex = 20;
			this.marksAndNumbersStmNotePopupButton.NoteHasChangesChanged += MarksAndNumbersStmNotePopupButton_NoteHasChangesChanged;
			// 
			// transitManifestTextBox
			// 
			this.BindingSource.SetBindingMember(this.transitManifestTextBox, JobDeclaration.Schema.JE_MasterBill);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_MasterBill)));
			this.transitManifestTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
            this.transitManifestTextBox.Name = "transitManifestTextBox";
            this.transitManifestTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
            this.transitManifestTextBox.TabIndex = 1;
			// 
			// agentsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.agentsReferenceTextBox, JobDeclaration.Schema.JE_AgentsReference);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_AgentsReference)));
			this.agentsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 300, true);
            this.agentsReferenceTextBox.Name = "agentsReferenceTextBox";
            this.agentsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
            this.agentsReferenceTextBox.TabIndex = 22;
            // 
            // masterCargoCarrierCodeFindBox
            // 
            this.masterCargoCarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.masterCargoCarrierCodeFindBox, JobDeclaration.Schema.JE_CarrierCode);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_CarrierCode)));
			this.masterCargoCarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|854bce25-d8b3-404f-af1c-21b95990d86e", "Master Cargo Carrier");
            this.masterCargoCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 110, true);
            this.masterCargoCarrierCodeFindBox.Name = "masterCargoCarrierCodeFindBox";
            this.masterCargoCarrierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.masterCargoCarrierCodeFindBox.ParentType = null;
            this.masterCargoCarrierCodeFindBox.PreBoundMaxLength = 5;
            this.masterCargoCarrierCodeFindBox.ShowDescriptionBox = false;
            this.masterCargoCarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.masterCargoCarrierCodeFindBox.TabIndex = 12;
            // 
            // jE_CustomsOfficeDropEdit
            // 
            this.jE_CustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jE_CustomsOfficeDropEdit, JobDeclaration.Schema.JE_CustomsOffice);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_CustomsOffice)));
			this.jE_CustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 128, true);
            this.jE_CustomsOfficeDropEdit.Name = "jE_CustomsOfficeDropEdit";
            this.jE_CustomsOfficeDropEdit.PreBoundMaxLength = 3;
            this.jE_CustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.jE_CustomsOfficeDropEdit.TabIndex = 5;
            // 
            // jE_LocationOfGoodsCodeFindBox
            // 
            this.jE_LocationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jE_LocationOfGoodsCodeFindBox, JobDeclaration.Schema.JE_LocationOfGoods);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_LocationOfGoods)));
			this.jE_LocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 132, true);
            this.jE_LocationOfGoodsCodeFindBox.ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusCodeList;
            this.jE_LocationOfGoodsCodeFindBox.Name = "jE_LocationOfGoodsCodeFindBox";
            this.jE_LocationOfGoodsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.jE_LocationOfGoodsCodeFindBox.ParentType = null;
            this.jE_LocationOfGoodsCodeFindBox.PreBoundMaxLength = 3;
            this.jE_LocationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
            this.jE_LocationOfGoodsCodeFindBox.TabIndex = 13;
            // 
            // jE_GoodsOriginCodeFindBox
            // 
            this.jE_GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jE_GoodsOriginCodeFindBox, JobDeclaration.Schema.JE_GoodsOrigin);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_GoodsOrigin)));
			this.jE_GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 102, true);
            this.jE_GoodsOriginCodeFindBox.Name = "jE_GoodsOriginCodeFindBox";
            this.jE_GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.jE_GoodsOriginCodeFindBox.ParentType = null;
            this.jE_GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
            this.jE_GoodsOriginCodeFindBox.ShowDescriptionBox = false;
            this.jE_GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
            this.jE_GoodsOriginCodeFindBox.TabIndex = 7;
			// 
			// rulesOfOriginCertificateTextBox
			// 
			this.BindingSource.SetBindingMember(this.rulesOfOriginCertificateTextBox, JobDeclaration.Schema.JE_ROOCert);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_ROOCert)));
			this.rulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 125, true);
            this.rulesOfOriginCertificateTextBox.Name = "rulesOfOriginCertificateTextBox";
            this.rulesOfOriginCertificateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
            this.rulesOfOriginCertificateTextBox.TabIndex = 9;
            // 
            // agentCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.agentCodeTextBox, "AgentCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).AgentCode)));
            this.agentCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 17, true);
            this.agentCodeTextBox.Name = "agentCodeTextBox";
            this.agentCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.agentCodeTextBox.TabIndex = 0;
            // 
            // rooTypeDropEdit
            // 
            this.rooTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rooTypeDropEdit, JobDeclaration.Schema.JE_ROOType);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_ROOType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.ROOTypesList)));
			this.rooTypeDropEdit.BindToList = "Lookups.ROOTypesList";
            this.rooTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("389dd58b-6a56-453f-8f7d-b1b9782246fb", "ROO Type");
            this.rooTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 124, true);
            this.rooTypeDropEdit.Name = "rooTypeDropEdit";
            this.rooTypeDropEdit.ShowDescriptionBox = false;
            this.rooTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.rooTypeDropEdit.TabIndex = 8;
            // 
            // UZ_RadioCallSignTextBox
            // 
            this.UZ_RadioCallSignTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UZ_RadioCallSignTextBox, JobDeclaration.Schema.JE_RadioCallSign);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_RadioCallSign)));
			this.UZ_RadioCallSignTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|2a562d6c-0897-401d-a611-d59836dd7912", "Radio Call Sign");
            this.UZ_RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 88, true);
            this.UZ_RadioCallSignTextBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVesselZZ;
            this.UZ_RadioCallSignTextBox.Name = "UZ_RadioCallSignTextBox";
            this.UZ_RadioCallSignTextBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.UZ_RadioCallSignTextBox.ParentType = null;
            this.UZ_RadioCallSignTextBox.ShowDescriptionBox = false;
            this.UZ_RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.UZ_RadioCallSignTextBox.TabIndex = 9;
            // 
            // uZ_CarrierCodeFindBox
            // 
            this.uZ_CarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uZ_CarrierCodeFindBox, JobDeclaration.Schema.JE_Carrier);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_Carrier)));
			this.uZ_CarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|7684666B-21CE-4F7D-9E4B-28D147D42852", "Carrier");
            this.uZ_CarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 110, true);
            this.uZ_CarrierCodeFindBox.Name = "uZ_CarrierCodeFindBox";
            this.uZ_CarrierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.uZ_CarrierCodeFindBox.ParentType = null;
            this.uZ_CarrierCodeFindBox.PreBoundMaxLength = 4;
            this.uZ_CarrierCodeFindBox.ShowDescriptionBox = false;
            this.uZ_CarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
            this.uZ_CarrierCodeFindBox.TabIndex = 11;
            // 
            // uZ_Trailer1TextBox
            // 
            this.uZ_Trailer1TextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uZ_Trailer1TextBox, JobDeclaration.Schema.JE_Trailer1);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_Trailer1)));
			this.uZ_Trailer1TextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|897c96ad-eb74-4973-bf9f-5fefae3772fc", "Trailer 1 Reg No");
            this.uZ_Trailer1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 64, true);
            this.uZ_Trailer1TextBox.Name = "uZ_Trailer1TextBox";
            this.uZ_Trailer1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
            this.uZ_Trailer1TextBox.TabIndex = 6;
            // 
            // uZ_Trailer2TextBox
            // 
            this.uZ_Trailer2TextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uZ_Trailer2TextBox, JobDeclaration.Schema.JE_Trailer2);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_Trailer2)));
			this.uZ_Trailer2TextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|783787d6-745e-498e-ba59-3edd56f3d05f", "Trailer 2 Reg No");
            this.uZ_Trailer2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 88, true);
            this.uZ_Trailer2TextBox.Name = "uZ_Trailer2TextBox";
            this.uZ_Trailer2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
            this.uZ_Trailer2TextBox.TabIndex = 10;
            // 
            // jE_MasterBillForAirBoundNonIATATextBox
            // 
            this.jE_MasterBillForAirBoundNonIATATextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jE_MasterBillForAirBoundNonIATATextBox, JobDeclaration.Schema.JE_MasterBill);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_MasterBill)));
			this.jE_MasterBillForAirBoundNonIATATextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|edc0c926-8ad1-4dcc-a6bf-763e46619d06", "Air Waybill");
            this.jE_MasterBillForAirBoundNonIATATextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
            this.jE_MasterBillForAirBoundNonIATATextBox.Name = "jE_MasterBillForAirBoundNonIATATextBox";
            this.jE_MasterBillForAirBoundNonIATATextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
            this.jE_MasterBillForAirBoundNonIATATextBox.TabIndex = 1;
            this.jE_MasterBillForAirBoundNonIATATextBox.Visible = false;
            // 
            // nonIATAFormatCheckBox
            // 
            this.nonIATAFormatCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.nonIATAFormatCheckBox, JobDeclaration.Schema.JE_IsNonIATAFormatAirWayBill);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((JobDeclaration)(null)).JE_IsNonIATAFormatAirWayBill)));
			this.nonIATAFormatCheckBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZADeclarationUserControl|8aa2ead1-c763-4003-b73f-a3536c8ccb04", "Non-IATA Format");
            this.nonIATAFormatCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 16, true);
            this.nonIATAFormatCheckBox.Name = "nonIATAFormatCheckBox";
            this.nonIATAFormatCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
            this.nonIATAFormatCheckBox.TabIndex = 2;
            this.nonIATAFormatCheckBox.UseVisualStyleBackColor = false;
            this.nonIATAFormatCheckBox.Visible = false;
            // 
            // removalTransportCodeDropEdit
            // 
            this.removalTransportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.removalTransportCodeDropEdit, JobDeclaration.Schema.JE_RemovalTransportCode);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_RemovalTransportCode)));
			this.removalTransportCodeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("MiscOptionsUserControl|f4745067-98ee-4de0-9d64-b7f7089ffa83", "Removal Mode");
            this.removalTransportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 216, true);
            this.removalTransportCodeDropEdit.Name = "removalTransportCodeDropEdit";
            this.removalTransportCodeDropEdit.PreBoundMaxLength = 3;
            this.removalTransportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
            this.removalTransportCodeDropEdit.TabIndex = 9;
            // 
            // zCodeFindBoxVesselAgent
            // 
            this.zCodeFindBoxVesselAgent.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBoxVesselAgent, JobDeclaration.Schema.JE_VesselAgent);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobDeclaration)(null)).JE_VesselAgent)));
			this.zCodeFindBoxVesselAgent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 110, true);
            this.zCodeFindBoxVesselAgent.Name = "zCodeFindBoxVesselAgent";
            this.zCodeFindBoxVesselAgent.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxVesselAgent.ParentType = null;
            this.zCodeFindBoxVesselAgent.PreBoundMaxLength = 4;
            this.zCodeFindBoxVesselAgent.ShowDescriptionBox = false;
            this.zCodeFindBoxVesselAgent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
            this.zCodeFindBoxVesselAgent.TabIndex = 13;
            // 
            // ZADeclarationUserControl
            // 
            this.Name = "ZADeclarationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 619, true);
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
            this.OrdersAttachUserControl.ResumeLayout(true);
            this.OrdersAttachUserControl.PerformLayout();
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
            this.issuedDateDateEdit.ResumeLayout(true);
            this.issuedDateDateEdit.PerformLayout();
            this.issuedAtCodeFindBox.ResumeLayout(true);
            this.issuedAtCodeFindBox.PerformLayout();
            this.houseBillIssuedDateDateEdit.ResumeLayout(true);
            this.houseBillIssuedDateDateEdit.PerformLayout();
            this.uZ_CargoCarrierCodeFindBox.ResumeLayout(true);
            this.uZ_CargoCarrierCodeFindBox.PerformLayout();
            this.paidByDropEdit.ResumeLayout(true);
            this.paidByDropEdit.PerformLayout();
            this.masterCargoCarrierCodeFindBox.ResumeLayout(true);
            this.masterCargoCarrierCodeFindBox.PerformLayout();
            this.jE_CustomsOfficeDropEdit.ResumeLayout(true);
            this.jE_CustomsOfficeDropEdit.PerformLayout();
            this.jE_LocationOfGoodsCodeFindBox.ResumeLayout(true);
            this.jE_LocationOfGoodsCodeFindBox.PerformLayout();
            this.jE_GoodsOriginCodeFindBox.ResumeLayout(true);
            this.jE_GoodsOriginCodeFindBox.PerformLayout();
            this.rooTypeDropEdit.ResumeLayout(true);
            this.rooTypeDropEdit.PerformLayout();
            this.UZ_RadioCallSignTextBox.ResumeLayout(true);
            this.UZ_RadioCallSignTextBox.PerformLayout();
            this.uZ_CarrierCodeFindBox.ResumeLayout(true);
            this.uZ_CarrierCodeFindBox.PerformLayout();
            this.removalTransportCodeDropEdit.ResumeLayout(true);
            this.removalTransportCodeDropEdit.PerformLayout();
            this.zCodeFindBoxVesselAgent.ResumeLayout(true);
            this.zCodeFindBoxVesselAgent.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

	}
}
