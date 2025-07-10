using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class NZCustomsDeclarationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ProcessingPortDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit JE_EntryAuthorisationDateDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl CodeInfoTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage PermitCodeTabPage;
		Enterprise.ZArchitecture.ZGrid PermitGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage OtherInfoTabPage;
		Enterprise.ZArchitecture.ZGrid OtherInfoGrid;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTermsDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit JE_SoldOrConsignedDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox CustomsProcessingGroupBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit EDITransmitDateDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcFindBox GoodsValueCalcFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsWeightCalcDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox JE_OriginalEntryNumberTextBox;
		protected ZLinkLabel VesselAndFlightLinkLabel;
		protected ZDropEdit JE_IsZeroRatedAllDropEdit;
		Enterprise.ZArchitecture.ZLabel PackagesLabel;
		protected ZDropEdit TransactionNatureDropEdit;
		System.ComponentModel.IContainer components;
		ZDropEdit GoodsLocatedDropEdit;
		ZDropEdit GoodsLocationDropEdit;
		protected ZArchitecture.ZTextBox TSWCombinedStatusTextBox;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl NotifyPartyDocAddressControl;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl DeliveryDestinationPartyDocAddressControl;
		public ZDropEdit OriginalEntryTypeDropEdit;
		protected ZDropEdit ApplicationCodeDropEdit;
		protected TranshipmentRequestTabPage TranshipmentRequestTabPage;
		protected DeliveryNotificationsTabPage DeliveryNotificationsTabPage;
		ZPanel OrganisationsBottomPanel;
		protected internal ZArchitecture.ZLabel ConsolidatedDeclarationAdviceLabel;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var declaration = JobDeclaration;

				if (declaration != null)
				{
					declaration.PermitCodes.CountChanged -= new CollectionCountChangedEventHandler(PermitCodes_CountChanged);
					declaration.OtherInfos.CountChanged -= new CollectionCountChangedEventHandler(OtherInfos_CountChanged);
					declaration.JE_GoodsLocatedAtInfo.ValueChanged -= JE_GoodsLocatedAtInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProcessingPortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_EntryAuthorisationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CodeInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.OtherInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OtherInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PermitCodeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PermitGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NotifyPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PaymentTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_SoldOrConsignedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EDITransmitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoodsValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.CustomsWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JE_OriginalEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselAndFlightLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.JE_IsZeroRatedAllDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TransactionNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsLocatedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TSWCombinedStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryDestinationPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.OriginalEntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryNotificationsTabPage = new Enterprise.Customs.NZ.GUI.Declaration.DeliveryNotificationsTabPage();
			this.OrganisationsBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TranshipmentRequestTabPage = new TranshipmentRequestTabPage(this.RightTabControl);
			this.ConsolidatedDeclarationAdviceLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.DeliveryNotificationsTabPage.SuspendLayout();
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
			this.ProcessingPortDropEdit.SuspendLayout();
			this.JE_EntryAuthorisationDateDateEdit.SuspendLayout();
			this.CodeInfoTabControl.SuspendLayout();
			this.OtherInfoTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherInfoGrid)).BeginInit();
			this.OtherInfoGrid.SuspendLayout();
			this.PermitCodeTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitGrid)).BeginInit();
			this.PermitGrid.SuspendLayout();
			this.NotifyPartyDocAddressControl.SuspendLayout();
			this.PaymentTermsDropEdit.SuspendLayout();
			this.JE_SoldOrConsignedDropEdit.SuspendLayout();
			this.CustomsProcessingGroupBox.SuspendLayout();
			this.EDITransmitDateDateEdit.SuspendLayout();
			this.GoodsValueCalcFindBox.SuspendLayout();
			this.CustomsWeightCalcDropEdit.SuspendLayout();
			this.JE_IsZeroRatedAllDropEdit.SuspendLayout();
			this.TransactionNatureDropEdit.SuspendLayout();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.GoodsLocatedDropEdit.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.DeliveryDestinationPartyDocAddressControl.SuspendLayout();
			this.OriginalEntryTypeDropEdit.SuspendLayout();
			this.OrganisationsBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|68295156-6b6b-4548-8595-1867eea9cf44", "Entry Type");
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 14, true);
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 58, true);
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Enabled = false;
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 170, true);
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 16;
			this.JE_ContainerModeBoundDropDownEdit.Visible = false;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.JE_MessageSubTypeBoundDropDownEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|ced4cc7b-729f-4158-8859-96ddd686ef4f", "Entry Style");
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 36, true);
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.TabIndex = 7;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.TabIndex = 6;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 233, true);
			this.ScreeningStatusDropEdit.TabIndex = 15;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 233, true);
			this.ScreenButton.TabIndex = 16;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalNoOfPacksCalcDropEdit, false);
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 160, true);
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 11;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 17, true);
			this.GoodsDescriptionTextBox.TabIndex = 5;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 67, true);
			this.FolioNumberTextBox.TabIndex = 7;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 43, true);
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.VesselFindBox.TabIndex = 4;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 67, true);
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 6;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 91, true);
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 115, true);
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 115, true);
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 91, true);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Controls.Add(this.PackagesLabel);
			this.ShipmentDetailsGroupBox.Controls.Add(this.CustomsWeightCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GoodsValueCalcFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.CodeInfoTabControl);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 201, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 384, true);
			this.ShipmentDetailsGroupBox.TabIndex = 6;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.CodeInfoTabControl, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsValueCalcFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.CustomsWeightCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.PackagesLabel, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.OriginalEntryTypeDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.ApplicationCodeDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.TransactionNatureDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_OriginalEntryNumberTextBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_SoldOrConsignedDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.PaymentTermsDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_EntryAuthorisationDateDateEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 304, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 330, true);
			this.ShipmentTypeGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("951409c8-272d-4cd6-a13d-344643fbd5ef", "Entry Details");
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_EntryAuthorisationDateDateEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.PaymentTermsDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_SoldOrConsignedDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_OriginalEntryNumberTextBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.TransactionNatureDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ApplicationCodeDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.OriginalEntryTypeDropEdit, 0);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.TabIndex = 3;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 4;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.TabIndex = 2;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.TabIndex = 1;
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 17, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 0;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 208, true);
			this.IncoTermDropEdit.TabIndex = 13;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.RightTabControl.Controls.Add(this.TranshipmentRequestTabPage);
			this.RightTabControl.Controls.Add(this.DeliveryNotificationsTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 55, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 588, true);
			this.RightTabControl.TabIndex = 8;
			this.RightTabControl.Controls.SetChildIndex(this.DeliveryNotificationsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.NumbersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.ShipmentCustomFieldsPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrdersTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.TranshipmentRequestTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.OrganisationsTabPage, 0);
			this.RightTabControl.Controls.SetChildIndex(this.DocsTabPage, 0);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Controls.Add(this.OrganisationsBottomPanel);
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsBottomPanel, 0);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 206, true);
			// 
			// ShippingOrAirLineOrganisationControl
			// 
			this.ShippingOrAirLineOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 52, true);
			this.ShippingOrAirLineOrganisationControl.TabIndex = 2;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 80, true);
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 7;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.TabIndex = 12;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Enabled = false;
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 12;
			this.JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 208, true);
			this.IncoTermExplainButton.TabIndex = 14;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 16, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 136, true);
			this.VolumeCalcDropEdit.TabIndex = 8;
			// 
			// ContainerTerminalOperatorAddressControl
			// 
			this.ContainerTerminalOperatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 74, true);
			this.ContainerTerminalOperatorAddressControl.TabIndex = 3;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// DepotAddressControl
			// 
			this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 96, true);
			this.DepotAddressControl.TabIndex = 4;
			// 
			// ContainerYardAddressControl
			// 
			this.ContainerYardAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 8, true);
			this.ContainerYardAddressControl.TabIndex = 0;
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 566, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 418, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.VesselAndFlightLinkLabel);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 55, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 141, true);
			this.TransportDetailsGroupBox.TabIndex = 5;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselAndFlightLinkLabel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("71B508CE-BFDD-4CD7-BD0B-6ACC6D4BC198", "Entry No.");
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 16, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 17, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.ConsolidatedDeclarationAdviceLabel);
			this.DeclarationDetailsGroupBox.Controls.Add(this.TSWCombinedStatusTextBox);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 0, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(926, 55, true);
			this.DeclarationDetailsGroupBox.TabIndex = 4;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.TSWCombinedStatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ConsolidatedDeclarationAdviceLabel, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 16, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.JobDeclaration);
			// 
			// ProcessingPortDropEdit
			// 
			this.ProcessingPortDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessingPortDropEdit, "JE_RL_NKProcessingPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_RL_NKProcessingPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.ProcessingPortList)));
			this.ProcessingPortDropEdit.BindToList = "Lookups+ProcessingPortList";
			this.ProcessingPortDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|d2d12230-7650-40df-8461-15446b7b5239", "Processing Port");
			this.ProcessingPortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 16, true);
			this.ProcessingPortDropEdit.Name = "ProcessingPortDropEdit";
			this.ProcessingPortDropEdit.PreBoundMaxLength = 5;
			this.ProcessingPortDropEdit.ShowDescriptionBox = false;
			this.ProcessingPortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 17, true);
			this.ProcessingPortDropEdit.TabIndex = 3;
			// 
			// JE_EntryAuthorisationDateDateEdit
			// 
			this.JE_EntryAuthorisationDateDateEdit.AllowDrop = true;
			this.JE_EntryAuthorisationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_EntryAuthorisationDateDateEdit, "JE_EntryAuthorisationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_EntryAuthorisationDate)));
			this.JE_EntryAuthorisationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 170, true);
			this.JE_EntryAuthorisationDateDateEdit.Name = "JE_EntryAuthorisationDateDateEdit";
			this.JE_EntryAuthorisationDateDateEdit.TabIndex = 18;
			// 
			// CodeInfoTabControl
			// 
			this.CodeInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.CodeInfoTabControl.Controls.Add(this.OtherInfoTabPage);
			this.CodeInfoTabControl.Controls.Add(this.PermitCodeTabPage);
			this.CodeInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 259, true);
			this.CodeInfoTabControl.Name = "CodeInfoTabControl";
			this.CodeInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 121, true);
			this.CodeInfoTabControl.TabIndex = 17;
			// 
			// OtherInfoTabPage
			// 
			this.OtherInfoTabPage.Controls.Add(this.OtherInfoGrid);
			this.OtherInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OtherInfoTabPage.Name = "OtherInfoTabPage";
			this.OtherInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 99, true);
			this.OtherInfoTabPage.TabIndex = 2;
			this.OtherInfoTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4c110c81-39d2-47e8-bed7-c173d1809eec", "Other Infos (0)");
			// 
			// OtherInfoGrid
			// 
			this.OtherInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherInfoGrid, "OtherInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).OtherInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.HeaderOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).OtherInfos)).SyncRoot)).ZO_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.HeaderOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).OtherInfos)).SyncRoot)).ZO_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.HeaderOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).OtherInfos)).SyncRoot)).ZO_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.HeaderOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).OtherInfos)).SyncRoot)).ZO_Description)));
			this.OtherInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ZO_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OtherInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OtherInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OtherInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OtherInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherInfoGrid.GridId = "e436d0c1-1f9b-444c-9080-8c6023b78e58";
			this.OtherInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherInfoGrid.LayoutKey = "OtherInfoGrid";
			this.OtherInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherInfoGrid.Name = "OtherInfoGrid";
			this.OtherInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 99, true);
			this.OtherInfoGrid.TabIndex = 0;
			// 
			// PermitCodeTabPage
			// 
			this.PermitCodeTabPage.Controls.Add(this.PermitGrid);
			this.PermitCodeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PermitCodeTabPage.Name = "PermitCodeTabPage";
			this.PermitCodeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 99, true);
			this.PermitCodeTabPage.TabIndex = 0;
			this.PermitCodeTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("11affd16-554c-4e9c-916c-51e81fdfbbb8", "Permits (0)");
			// 
			// PermitGrid
			// 
			this.PermitGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitGrid, "PermitCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).PermitCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).PermitCodes)).SyncRoot)).ZO_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).PermitCodes)).SyncRoot)).ZO_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).PermitCodes)).SyncRoot)).ZO_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).PermitCodes)).SyncRoot)).ZO_Description)));
			this.PermitGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "ZO_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.PermitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PermitGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PermitGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PermitGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitGrid.GridId = "961e1c70-50d2-433c-90ec-2b108451873d";
			this.PermitGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitGrid.LayoutKey = "PermitGrid";
			this.PermitGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitGrid.Name = "PermitGrid";
			this.PermitGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 99, true);
			this.PermitGrid.TabIndex = 0;
			// 
			// NotifyPartyDocAddressControl
			// 
			this.NotifyPartyDocAddressControl.AddressValidationProcessCmdKey = null;
			this.NotifyPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyDocAddressControl, "NotifyPartyDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).NotifyPartyDocumentaryAddress)));
			this.NotifyPartyDocAddressControl.BindToOrganisations = "Lookups.NotifyPartyOrganisations";
			this.NotifyPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|5153D2FA-6E83-47D9-A274-20D7E4DA5119", "Notify Party", "State the name and address of any Notify party specified in the transport document.");
			this.NotifyPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.NotifyPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 66, true);
			this.NotifyPartyDocAddressControl.Name = "NotifyPartyDocAddressControl";
			this.NotifyPartyDocAddressControl.ReadOnly = false;
			this.NotifyPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.NotifyPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.NotifyPartyDocAddressControl.TabIndex = 10;
			this.NotifyPartyDocAddressControl.ValidationJustForced = false;
			// 
			// PaymentTermsDropEdit
			// 
			this.PaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTermsDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.PaymentPartyList)));
			this.PaymentTermsDropEdit.BindToList = "Lookups+PaymentPartyList";
			this.PaymentTermsDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|5ebf0921-e767-4473-b41e-f0dabe009679", "Payment Terms");
			this.PaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 102, true);
			this.PaymentTermsDropEdit.Name = "PaymentTermsDropEdit";
			this.PaymentTermsDropEdit.PreBoundMaxLength = 1;
			this.PaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.PaymentTermsDropEdit.TabIndex = 9;
			// 
			// JE_SoldOrConsignedDropEdit
			// 
			this.JE_SoldOrConsignedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_SoldOrConsignedDropEdit, "JE_SoldOrConsigned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_SoldOrConsigned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.JE_SoldOrConsigned_List)));
			this.JE_SoldOrConsignedDropEdit.BindToList = "Lookups+JE_SoldOrConsigned_List";
			this.JE_SoldOrConsignedDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|86c8674d-ad66-46da-bdd1-5ebb2002c9b8", "Terms of Sale");
			this.JE_SoldOrConsignedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 124, true);
			this.JE_SoldOrConsignedDropEdit.Name = "JE_SoldOrConsignedDropEdit";
			this.JE_SoldOrConsignedDropEdit.PreBoundMaxLength = 1;
			this.JE_SoldOrConsignedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.JE_SoldOrConsignedDropEdit.TabIndex = 11;
			// 
			// CustomsProcessingGroupBox
			// 
			this.CustomsProcessingGroupBox.Controls.Add(this.EDITransmitDateDateEdit);
			this.CustomsProcessingGroupBox.Controls.Add(this.ProcessingPortDropEdit);
			this.CustomsProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 589, true);
			this.CustomsProcessingGroupBox.Name = "CustomsProcessingGroupBox";
			this.CustomsProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 43, true);
			this.CustomsProcessingGroupBox.TabIndex = 7;
			this.CustomsProcessingGroupBox.TabStop = false;
			this.CustomsProcessingGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("c28026a9-f563-4529-862e-fd044cbb5edb", "Customs Processing");
			// 
			// EDITransmitDateDateEdit
			// 
			this.EDITransmitDateDateEdit.AllowDrop = true;
			this.EDITransmitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EDITransmitDateDateEdit, "JE_EDITransmitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_EDITransmitDate)));
			this.EDITransmitDateDateEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|8c1c78dc-b577-4d37-991a-f116c6762b9c", "EDI Transmit Date");
			this.EDITransmitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.EDITransmitDateDateEdit.Name = "EDITransmitDateDateEdit";
			this.EDITransmitDateDateEdit.TabIndex = 1;
			// 
			// GoodsValueCalcFindBox
			// 
			this.GoodsValueCalcFindBox.AllowDrop = true;
			this.GoodsValueCalcFindBox.BindToAmount = "JE_ECI_InvoiceAmount";
			this.GoodsValueCalcFindBox.BindToList = "Lookups.CurrencyList";
			this.GoodsValueCalcFindBox.BindToUnit = "JE_ECI_InvoiceCurrency";
			this.GoodsValueCalcFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|e63cc52b-91aa-451f-9036-324e344ea910", "Goods Value");
			this.GoodsValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 184, true);
			this.GoodsValueCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.GoodsValueCalcFindBox.Name = "GoodsValueCalcFindBox";
			this.GoodsValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.GoodsValueCalcFindBox.TabIndex = 12;
			// 
			// CustomsWeightCalcDropEdit
			// 
			this.CustomsWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_DeclaredWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_DeclaredWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.WeightUnitList)));
			this.CustomsWeightCalcDropEdit.BindToAmount = "JE_DeclaredWeight";
			this.CustomsWeightCalcDropEdit.BindToList = "Lookups.WeightUnitList";
			this.CustomsWeightCalcDropEdit.BindToUnit = "JE_DeclaredWeightUQ";
			this.CustomsWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|37fc88ae-ec68-4357-a6d9-43fc12bab075", "Customs Wgt.");
			this.CustomsWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.CustomsWeightCalcDropEdit.Name = "CustomsWeightCalcDropEdit";
			this.CustomsWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.CustomsWeightCalcDropEdit.TabIndex = 9;
			this.CustomsWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JE_OriginalEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JE_OriginalEntryNumberTextBox, "JE_OriginalEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_OriginalEntryNumber)));
			this.JE_OriginalEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|7b911ace-8b17-48da-a083-35c76d5728af", "Original Entry #");
			this.JE_OriginalEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 170, true);
			this.JE_OriginalEntryNumberTextBox.Name = "JE_OriginalEntryNumberTextBox";
			this.JE_OriginalEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.JE_OriginalEntryNumberTextBox.TabIndex = 19;
			this.JE_OriginalEntryNumberTextBox.Visible = false;
			// 
			// VesselAndFlightLinkLabel
			// 
			this.VesselAndFlightLinkLabel.AutoSize = true;
			this.VesselAndFlightLinkLabel.IsFontBold = false;
			this.VesselAndFlightLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 69, true);
			this.VesselAndFlightLinkLabel.Name = "VesselAndFlightLinkLabel";
			this.VesselAndFlightLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 13, true);
			this.VesselAndFlightLinkLabel.TabIndex = 8;
			this.VesselAndFlightLinkLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("dfba9ab5-cd65-4973-b0be-e07654bbb885", "Find Vessel/Flight No (Customs Website)");
			this.VesselAndFlightLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.VesselAndFlightLinkLabel_LinkClicked);
			// 
			// JE_IsZeroRatedAllDropEdit
			// 
			this.JE_IsZeroRatedAllDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_IsZeroRatedAllDropEdit, "JE_IsZeroRatedAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_IsZeroRatedAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.YesNoList)));
			this.JE_IsZeroRatedAllDropEdit.BindToList = "Lookups+YesNoList";
			this.JE_IsZeroRatedAllDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|bf7a8440-46ff-44e6-9eff-69ef61e36ea1", "Zero Rate All");
			this.JE_IsZeroRatedAllDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 44, true);
			this.JE_IsZeroRatedAllDropEdit.Name = "JE_IsZeroRatedAllDropEdit";
			this.JE_IsZeroRatedAllDropEdit.PreBoundMaxLength = 2;
			this.JE_IsZeroRatedAllDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.JE_IsZeroRatedAllDropEdit.TabIndex = 9;
			// 
			// PackagesLabel
			// 
			this.PackagesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 160, true);
			this.PackagesLabel.Name = "PackagesLabel";
			this.PackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 21, true);
			this.PackagesLabel.TabIndex = 10;
			this.PackagesLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f6833a1f-3a47-43ef-9d03-5c926fc7d591", "Packages:");
			// 
			// TransactionNatureDropEdit
			// 
			this.TransactionNatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionNatureDropEdit, "JE_TransactionNature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_TransactionNature)));
			this.TransactionNatureDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("05a2e1a4-4868-44c7-a9d9-2ff035eacee2", "Nat. of Trans.", "Nature of Trans.", "Nature of Transaction", "");
			this.TransactionNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 145, true);
			this.TransactionNatureDropEdit.Name = "TransactionNatureDropEdit";
			this.TransactionNatureDropEdit.PreBoundMaxLength = 1;
			this.TransactionNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.TransactionNatureDropEdit.TabIndex = 17;
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_ApplicationCode)));
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 191, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.ApplicationCodeDropEdit.TabIndex = 21;
			// 
			// GoodsLocatedDropEdit
			// 
			this.GoodsLocatedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocatedDropEdit, "JE_GoodsLocatedAt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_GoodsLocatedAt)));
			this.GoodsLocatedDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("NZCustomsDeclarationUserControl|23114CFA-F365-44C2-BBD4-F725ED26C13A", "Goods Located");
			this.GoodsLocatedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.GoodsLocatedDropEdit.Name = "GoodsLocatedDropEdit";
			this.GoodsLocatedDropEdit.PreBoundMaxLength = 2;
			this.GoodsLocatedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.GoodsLocatedDropEdit.TabIndex = 6;
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "JE_Cal_GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_Cal_GoodsLocation)));
			this.GoodsLocationDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("0d47e462-2670-4bcd-aae8-0f1ec66e06bd", "Goods Location");
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 22, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.ShouldResizeByMaxLength = false;
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 17, true);
			this.GoodsLocationDropEdit.TabIndex = 7;
			// 
			// TSWCombinedStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.TSWCombinedStatusTextBox, "JE_TSWCombinedStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_TSWCombinedStatusDesc)));
			this.TSWCombinedStatusTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("596ae8bc-2ed6-423d-81bf-39cbe61dc36b", "TSW", "TSW combined Agency status description");
			this.TSWCombinedStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TSWCombinedStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 16, true);
			this.TSWCombinedStatusTextBox.Name = "TSWCombinedStatusTextBox";
			this.TSWCombinedStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 17, true);
			this.TSWCombinedStatusTextBox.TabIndex = 16;
			// 
			// DeliveryDestinationPartyDocAddressControl
			// 
			this.DeliveryDestinationPartyDocAddressControl.AddressValidationProcessCmdKey = null;
			this.DeliveryDestinationPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDestinationPartyDocAddressControl, "DeliveryDestinationPartyDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).DeliveryDestinationPartyDocAddress)));
			this.DeliveryDestinationPartyDocAddressControl.BindToOrganisations = "Lookups.DeliveryDestinationPartyOrganisations";
			this.DeliveryDestinationPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("84befd82-f383-4ed4-8942-97b0e2cedf37", "D/D Party", "Delivery Dest.", "Delivery Destination Party", "State the Delivery Destination party that the goods will be delivered to if different to the Consignee");
			this.DeliveryDestinationPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DeliveryDestinationPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 88, true);
			this.DeliveryDestinationPartyDocAddressControl.Name = "DeliveryDestinationPartyDocAddressControl";
			this.DeliveryDestinationPartyDocAddressControl.ReadOnly = false;
			this.DeliveryDestinationPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DeliveryDestinationPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DeliveryDestinationPartyDocAddressControl.TabIndex = 12;
			this.DeliveryDestinationPartyDocAddressControl.ValidationJustForced = false;
			// 
			// OriginalEntryTypeDropEdit
			// 
			this.OriginalEntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginalEntryTypeDropEdit, "JE_OriginalEntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_OriginalEntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.OriginalEntryTypeList)));
			this.OriginalEntryTypeDropEdit.BindToList = "Lookups.OriginalEntryTypeList";
			this.OriginalEntryTypeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8ba94300-d3ff-44cc-b0ab-de7738dab36a", "Type");
			this.OriginalEntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 170, true);
			this.OriginalEntryTypeDropEdit.Name = "OriginalEntryTypeDropEdit";
			this.OriginalEntryTypeDropEdit.PreBoundMaxLength = 3;
			this.OriginalEntryTypeDropEdit.ShowDescriptionBox = false;
			this.OriginalEntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.OriginalEntryTypeDropEdit.TabIndex = 20;
			// 
			// DeliveryNotificationsTabPage
			// 
			this.DeliveryNotificationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DeliveryNotificationsTabPage.Name = "DeliveryNotificationsTabPage";
			this.DeliveryNotificationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 488, true);
			this.DeliveryNotificationsTabPage.TabIndex = 2;
			this.DeliveryNotificationsTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("a20e93a0-8fe7-4510-9ee1-86aef9d476e7", "Delivery Notifications");
			// 
			// OrganisationsBottomPanel
			// 
			this.OrganisationsBottomPanel.Controls.Add(this.GoodsLocationDropEdit);
			this.OrganisationsBottomPanel.Controls.Add(this.GoodsLocatedDropEdit);
			this.OrganisationsBottomPanel.Controls.Add(this.DeliveryDestinationPartyDocAddressControl);
			this.OrganisationsBottomPanel.Controls.Add(this.NotifyPartyDocAddressControl);
			this.OrganisationsBottomPanel.Controls.Add(this.JE_IsZeroRatedAllDropEdit);
			this.OrganisationsBottomPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.OrganisationsBottomPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OrganisationsBottomPanel.Name = "OrganisationsBottomPanel";
			this.OrganisationsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 254, true);
			this.OrganisationsBottomPanel.TabIndex = 1;
			// 
			// TranshipmentRequestTabPage
			// 
			this.TranshipmentRequestTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.TranshipmentRequestTabPage.Name = "TranshipmentRequestTabPage";
			this.TranshipmentRequestTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 488, true);
			this.TranshipmentRequestTabPage.TabIndex = 2;
			this.TranshipmentRequestTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("836886a2-cb1e-4e84-801d-7d9cb5b6301e", "Transhipment Request");
			// 
			// ConsolidatedDeclarationAdviceLabel
			// 
			this.ConsolidatedDeclarationAdviceLabel.CaptionResourceString = null;
			this.ConsolidatedDeclarationAdviceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 33, true);
			this.ConsolidatedDeclarationAdviceLabel.Name = "ConsolidatedDeclarationAdviceLabel";
			this.ConsolidatedDeclarationAdviceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.ConsolidatedDeclarationAdviceLabel.TabIndex = 18;
			this.ConsolidatedDeclarationAdviceLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("012f7675-9480-4d11-9c7e-2cc3b7980ef1", "This declaration is attached to a consolidated entry.");
			this.ConsolidatedDeclarationAdviceLabel.Visible = false;
			this.ConsolidatedDeclarationAdviceLabel.BackColor = System.Drawing.Color.Tomato;
			// 
			// NZCustomsDeclarationUserControl
			// 
			this.AutoScroll = true;
			this.Controls.Add(this.CustomsProcessingGroupBox);
			this.Name = "NZCustomsDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1195, 710, true);
			this.Controls.SetChildIndex(this.CustomsProcessingGroupBox, 0);
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
			this.DeliveryNotificationsTabPage.ResumeLayout(false);
			this.DeliveryNotificationsTabPage.PerformLayout();
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
			this.ProcessingPortDropEdit.ResumeLayout(true);
			this.ProcessingPortDropEdit.PerformLayout();
			this.JE_EntryAuthorisationDateDateEdit.ResumeLayout(true);
			this.JE_EntryAuthorisationDateDateEdit.PerformLayout();
			this.CodeInfoTabControl.ResumeLayout(false);
			this.CodeInfoTabControl.PerformLayout();
			this.OtherInfoTabPage.ResumeLayout(false);
			this.OtherInfoTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherInfoGrid)).EndInit();
			this.OtherInfoGrid.ResumeLayout(false);
			this.OtherInfoGrid.PerformLayout();
			this.PermitCodeTabPage.ResumeLayout(false);
			this.PermitCodeTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitGrid)).EndInit();
			this.PermitGrid.ResumeLayout(false);
			this.PermitGrid.PerformLayout();
			this.NotifyPartyDocAddressControl.ResumeLayout(true);
			this.NotifyPartyDocAddressControl.PerformLayout();
			this.PaymentTermsDropEdit.ResumeLayout(true);
			this.PaymentTermsDropEdit.PerformLayout();
			this.JE_SoldOrConsignedDropEdit.ResumeLayout(true);
			this.JE_SoldOrConsignedDropEdit.PerformLayout();
			this.CustomsProcessingGroupBox.ResumeLayout(false);
			this.CustomsProcessingGroupBox.PerformLayout();
			this.EDITransmitDateDateEdit.ResumeLayout(true);
			this.EDITransmitDateDateEdit.PerformLayout();
			this.GoodsValueCalcFindBox.ResumeLayout(true);
			this.GoodsValueCalcFindBox.PerformLayout();
			this.CustomsWeightCalcDropEdit.ResumeLayout(true);
			this.CustomsWeightCalcDropEdit.PerformLayout();
			this.JE_IsZeroRatedAllDropEdit.ResumeLayout(true);
			this.JE_IsZeroRatedAllDropEdit.PerformLayout();
			this.TransactionNatureDropEdit.ResumeLayout(true);
			this.TransactionNatureDropEdit.PerformLayout();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.GoodsLocatedDropEdit.ResumeLayout(true);
			this.GoodsLocatedDropEdit.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.DeliveryDestinationPartyDocAddressControl.ResumeLayout(true);
			this.DeliveryDestinationPartyDocAddressControl.PerformLayout();
			this.OriginalEntryTypeDropEdit.ResumeLayout(true);
			this.OriginalEntryTypeDropEdit.PerformLayout();
			this.OrganisationsBottomPanel.ResumeLayout(false);
			this.OrganisationsBottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
