namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class ShipmentDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.ShipmentValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ShipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipmentControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortOrPointOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOrPointOfLoadingDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransferDestinationFIRMSCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.B0_PlaceOfReceiptTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionOfCargoControl = new Enterprise.Customs.GUI.LongTextControl();
			this.ExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.FDAFreightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WasOutOfUSFor45DaysOrLessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BoardedQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BillIssuerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Shipment);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|59c10a8b-89b4-4677-90b1-a341a2d1118e", "Shipment Details");
			this.ShipmentDetailsGroupBox.Controls.Add(this.SplitContainer);
			this.ShipmentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentDetailsGroupBox.Name = "ShipmentDetailsGroupBox";
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 253, true);
			this.ShipmentDetailsGroupBox.TabIndex = 0;
			this.ShipmentDetailsGroupBox.TabStop = false;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ShipmentTypeDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.ShipmentControlNumberTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.ShipmentIdentifierTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.PortOrPointOfLoadingCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.PortOrPointOfLoadingDCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.TransferDestinationFIRMSCodeCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.B0_PlaceOfReceiptTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.ServiceTypeDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.BillIssuerCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.ShipmentValueCalcDropEdit);
			this.SplitContainer.Panel1MinSize = 350;
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.DescriptionOfCargoControl);
			this.SplitContainer.Panel2.Controls.Add(this.ExportDateEdit);
			this.SplitContainer.Panel2.Controls.Add(this.VolumeCalcDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.QuantityCalcDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.WeightCalcDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.FDAFreightCheckBox);
			this.SplitContainer.Panel2.Controls.Add(this.WasOutOfUSFor45DaysOrLessCheckBox);
			this.SplitContainer.Panel2.Controls.Add(this.BoardedQuantityCalcEdit);
			this.SplitContainer.Panel2MinSize = 350;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 234, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.SplitContainer.TabIndex = 0;
			// 
			// ShipmentTypeDropEdit
			// 
			this.ShipmentTypeDropEdit.AllowDrop = true;
			this.ShipmentTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShipmentTypeDropEdit, "B0_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_ShipmentType)));
			this.ShipmentTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|4095a626-ba1b-4fc3-b375-59a8307d334f", "Type", "Shipment Type", "Shipment Release Type", "Determines how the shipment will be released.");
			this.ShipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 3, true);
			this.ShipmentTypeDropEdit.Name = "ShipmentTypeDropEdit";
			this.ShipmentTypeDropEdit.PreBoundMaxLength = 3;
			this.ShipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.ShipmentTypeDropEdit.TabIndex = 0;
			// BillIssuerCodeFindBox
			// 
			this.BillIssuerCodeFindBox.AllowDrop = true;
			this.BillIssuerCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BillIssuerCodeFindBox, "B0_IssuerSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_IssuerSCAC)));
			this.BillIssuerCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|381667ed-30e7-4a9c-a4b7-98f1be922861", "Bill Issuer (SCAC)", "SCAC of Bill Issuer");
			this.BillIssuerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 29, true);
			this.BillIssuerCodeFindBox.Name = "BillIssuerCodeFindBox";
			this.BillIssuerCodeFindBox.PreBoundMaxLength = 4;
			this.BillIssuerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.BillIssuerCodeFindBox.TabIndex = 1;
			// 
			// ShipmentControlNumberTextBox
			// 
			this.ShipmentControlNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShipmentControlNumberTextBox, "B0_MasterBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_MasterBillNumber)));
			this.ShipmentControlNumberTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|76bb8c15-450d-4904-a400-c5ce9263df2f", "House Bill Number", "A unique BOL number used by the reporting trade participant(s) to identify the shipment or consolidation. Ideally, a house number, identifying the transaction throughout its life cycle for both commercial and transportation purposes.");
			this.ShipmentControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 55, true);
			this.ShipmentControlNumberTextBox.Name = "ShipmentControlNumberTextBox";
			this.ShipmentControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.ShipmentControlNumberTextBox.TabIndex = 2;
			// 
			// ShipmentIdentifierTextBox
			// 
			this.ShipmentIdentifierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShipmentIdentifierTextBox, "B0_ReferenceID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_ReferenceID)));
			this.ShipmentIdentifierTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|69a1a0b6-f7b5-4c14-92e3-1b0a65bcc626", "Shipment Identifier", "Trader Reference Number. Number provided by the shipper to be passed through to the broker on the Customs download.");
			this.ShipmentIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 81, true);
			this.ShipmentIdentifierTextBox.Name = "ShipmentIdentifierTextBox";
			this.ShipmentIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.ShipmentIdentifierTextBox.TabIndex = 3;
			// 
			// PortOrPointOfLoadingCodeFindBox
			// 
			this.PortOrPointOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOrPointOfLoadingCodeFindBox, "B0_RL_NKPortOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_RL_NKPortOfLading)));
			this.PortOrPointOfLoadingCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|fc6c7555-4c38-47ea-8024-dbd88fcd1f3c", "Port Of Lading", "Port/Point at which merchandise is loaded on the conveyance that will cross the border (UNLOCO).");
			this.PortOrPointOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 107, true);
			this.PortOrPointOfLoadingCodeFindBox.Name = "PortOrPointOfLoadingCodeFindBox";
			this.PortOrPointOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOrPointOfLoadingCodeFindBox.ShowDescriptionBox = false;
			this.PortOrPointOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOrPointOfLoadingCodeFindBox.TabIndex = 4;
			// 
			// PortOrPointOfLoadingDCodeFindBox
			// 
			this.PortOrPointOfLoadingDCodeFindBox.AllowDrop = true;
			this.PortOrPointOfLoadingDCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PortOrPointOfLoadingDCodeFindBox, "B0_PortOfLadingKCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_PortOfLadingKCode)));
			this.PortOrPointOfLoadingDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|f7f17549-0091-4401-90d1-298e20f65e17", "K", "Schedule K", "Port Of Lading (Schedule K)", "Port/Point at which merchandise is loaded on the conveyance that will cross the border (Schedule K).");
			this.PortOrPointOfLoadingDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 107, true);
			this.PortOrPointOfLoadingDCodeFindBox.Name = "PortOrPointOfLoadingDCodeFindBox";
			this.PortOrPointOfLoadingDCodeFindBox.PreBoundMaxLength = 5;
			this.PortOrPointOfLoadingDCodeFindBox.ShowDescriptionBox = false;
			this.PortOrPointOfLoadingDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOrPointOfLoadingDCodeFindBox.TabIndex = 5;
			// 
			// B0_PlaceOfReceiptTextBox
			// 
			this.B0_PlaceOfReceiptTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.B0_PlaceOfReceiptTextBox, "B0_PlaceOfReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_PlaceOfReceipt)));
			this.B0_PlaceOfReceiptTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|b264fa72-bb34-434e-aafa-0f7f90944c2a", "Place of Receipt", "City where carrier took receipt of goods (actual city free form) If different from Port/Point of loading.");
			this.B0_PlaceOfReceiptTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 133, true);
			this.B0_PlaceOfReceiptTextBox.Name = "B0_PlaceOfReceiptTextBox";
			this.B0_PlaceOfReceiptTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.B0_PlaceOfReceiptTextBox.TabIndex = 6;
			// 
			// ServiceTypeDropEdit
			// 
			this.ServiceTypeDropEdit.AllowDrop = true;
			this.ServiceTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceTypeDropEdit, "B0_ServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_ServiceType)));
			this.ServiceTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|9c660f1e-4643-4a4f-a3c8-670226794f0d", "Service Type", "Type of shipping contract. If the shipment is being delivered under one of the listed service types, then the appropriate service type must be reported; otherwise, do not report the service type.");
			this.ServiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 159, true);
			this.ServiceTypeDropEdit.Name = "ServiceTypeDropEdit";
			this.ServiceTypeDropEdit.PreBoundMaxLength = 2;
			this.ServiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.ServiceTypeDropEdit.TabIndex = 7;
			// 
			// TransferDestinationFIRMSCodeCodeFindBox
			// 
			this.TransferDestinationFIRMSCodeCodeFindBox.AllowDrop = true;
			this.TransferDestinationFIRMSCodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransferDestinationFIRMSCodeCodeFindBox, "B0_Firms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_Firms)));
			this.TransferDestinationFIRMSCodeCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|6615ffc7-21d8-42d8-ac58-3adb868138c3", "Transfer Destination FIRMS", "This would be used to request a local transfer from the carrier to a bonded facility such as a CFS or for In-Bond.");
			this.TransferDestinationFIRMSCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 185, true);
			this.TransferDestinationFIRMSCodeCodeFindBox.Name = "TransferDestinationFIRMSCodeCodeFindBox";
			this.TransferDestinationFIRMSCodeCodeFindBox.PreBoundMaxLength = 4;
			this.TransferDestinationFIRMSCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.TransferDestinationFIRMSCodeCodeFindBox.TabIndex = 8;

			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.CountryOfOriginCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "B0_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_RN_NKCountryOfExport)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|54466B39-504B-45D8-9AD9-5C7B44B0A4A7", "Origin", "Country", "Country of Origin", "");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 211, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginEditBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 9;

			// 
			// ShipmentValueCalcDropEdit
			// 
			this.ShipmentValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_GoodsValue)));
			this.ShipmentValueCalcDropEdit.BindToAmount = "B0_GoodsValue";
			this.ShipmentValueCalcDropEdit.BindToUnit = "B0_RX_NKGoodsValueCurrency";
			this.ShipmentValueCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommodityUserControl|1B1AFFA2-F2E0-45FE-9E06-B507496C02E1", "Shipment Value", "Total shipment commodities customs value.");
			this.ShipmentValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 237, true);
			this.ShipmentValueCalcDropEdit.Name = "ShipmentValueCalcDropEdit";
			this.ShipmentValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 18, true);
			this.ShipmentValueCalcDropEdit.TabIndex = 10;
			this.ShipmentValueCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// DescriptionOfCargoControl
			// 
			this.DescriptionOfCargoControl.AllowDrop = true;
			this.DescriptionOfCargoControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionOfCargoControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("755e5eda-efa2-4850-a070-253802017b0b", "Desc.", "Description", "Description Of Cargo", "");
			this.DescriptionOfCargoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 107, true);
			this.DescriptionOfCargoControl.Name = "DescriptionOfCargoControl";
			this.DescriptionOfCargoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.DescriptionOfCargoControl.TabIndex = 4;
			// 
			// ExportDateEdit
			// 
			this.ExportDateEdit.AllowDrop = true;
			this.ExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExportDateEdit, "B0_DateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_DateOfExport)));
			this.ExportDateEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("2812316e-6cbf-434f-84f7-e4fdbac6e3c7", "Export Date", "The exit date from the U.S.in the case of shipments re-entering the country and claiming release as free astray.");
			this.ExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 133, true);
			this.ExportDateEdit.Name = "ExportDateEdit";
			this.ExportDateEdit.TabIndex = 6;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.VolumeCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_VolumeUQ)));
			this.VolumeCalcDropEdit.BindToAmount = "B0_Volume";
			this.VolumeCalcDropEdit.BindToUnit = "B0_VolumeUQ";
			this.VolumeCalcDropEdit.CaptionResourceString = null;
			this.VolumeCalcDropEdit.Decimals = 2;
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 81, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 3;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// QuantityCalcDropEdit
			// 
			this.QuantityCalcDropEdit.AllowDrop = true;
			this.QuantityCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_ManifestQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_ManifestUQ)));
			this.QuantityCalcDropEdit.BindToAmount = "B0_ManifestQty";
			this.QuantityCalcDropEdit.BindToUnit = "B0_ManifestUQ";
			this.QuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|582e3600-5cd2-4169-a40c-7a3252db3c1d", "Shipment Quantity", "Total shipment quantity.");
			this.QuantityCalcDropEdit.Decimals = 2;
			this.QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 3, true);
			this.QuantityCalcDropEdit.Name = "QuantityCalcDropEdit";
			this.QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.QuantityCalcDropEdit.TabIndex = 0;
			this.QuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.WeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_WeightUQ)));
			this.WeightCalcDropEdit.BindToAmount = "B0_Weight";
			this.WeightCalcDropEdit.BindToUnit = "B0_WeightUQ";
			this.WeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|97a78b10-b5e2-4fcf-be62-c523b636dad3", "Gross Weight", "Total shipment gross weight.");
			this.WeightCalcDropEdit.Decimals = 2;
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 55, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.WeightCalcDropEdit.TabIndex = 2;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// FDAFreightCheckBox
			// 
			this.FDAFreightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FDAFreightCheckBox, "B0_IsFDAFreight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_IsFDAFreight)));
			this.FDAFreightCheckBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|e7824d63-4311-4ed8-9767-8bf6275ffa05", "Shipment is subject to FDA requirements", "US Customs && Border Protection is required by FDA", "US Customs && Border Protection is required by FDA (Food and Drug Administration)", "US Customs & Border Protection is required by FDA (Food and Drug Administration) in compliance with the Bioterrorism Act to capture the FDA indication from carriers transporting animal or human food into the U.S.");
			this.FDAFreightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FDAFreightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 188, true);
			this.FDAFreightCheckBox.Name = "FDAFreightCheckBox";
			this.FDAFreightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FDAFreightCheckBox.TabIndex = 8;
			this.FDAFreightCheckBox.UseVisualStyleBackColor = true;
			// 
			// WasOutOfUSFor45DaysOrLessCheckBox
			// 
			this.WasOutOfUSFor45DaysOrLessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WasOutOfUSFor45DaysOrLessCheckBox, "B0_WasOutOfUSFor45DaysOrLess");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_WasOutOfUSFor45DaysOrLess)));
			this.WasOutOfUSFor45DaysOrLessCheckBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|0983c47c-9401-4bef-9ead-75e9707708ef", "Carrier/Foreign Customs Control", "Shipment was out of US under control for 45 days or less", "Shipment was out of US under carrier/foreign customs control for 45 days or less", "To indicate whether or not the shipment re-entering the U.S. has been out of the country under foreign Customs custody or under custody of the carrier for 45 days or less since the date of exportation.");
			this.WasOutOfUSFor45DaysOrLessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WasOutOfUSFor45DaysOrLessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 162, true);
			this.WasOutOfUSFor45DaysOrLessCheckBox.Name = "WasOutOfUSFor45DaysOrLessCheckBox";
			this.WasOutOfUSFor45DaysOrLessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WasOutOfUSFor45DaysOrLessCheckBox.TabIndex = 7;
			this.WasOutOfUSFor45DaysOrLessCheckBox.UseVisualStyleBackColor = true;
			// 
			// BoardedQuantityCalcEdit
			// 
			this.BoardedQuantityCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BoardedQuantityCalcEdit, "B0_BoardedQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).B0_BoardedQuantity)));
			this.BoardedQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentDetailsUserControl|9d02ddaf-33f2-4267-bac4-7c0816c0319f", "Boarded Quantity", "This quantity is what is actually boarded by the carrier when loading the shipment. Specify for Split Shipments.");
			this.BoardedQuantityCalcEdit.DecimalPlaces = 2;
			this.BoardedQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 29, true);
			this.BoardedQuantityCalcEdit.Name = "BoardedQuantityCalcEdit";
			this.BoardedQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.BoardedQuantityCalcEdit.TabIndex = 1;
			this.BoardedQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShipmentDetailsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 253, true);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel1.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ShipmentDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit ShipmentTypeDropEdit;
		private ZArchitecture.ZTextBox ShipmentControlNumberTextBox;
		private ZArchitecture.ZTextBox ShipmentIdentifierTextBox;
		private ZArchitecture.GUI.ZCodeFindBox PortOrPointOfLoadingDCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox PortOrPointOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox B0_PlaceOfReceiptTextBox;
		private ZArchitecture.GUI.ZDropEdit ServiceTypeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox TransferDestinationFIRMSCodeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCheckBox FDAFreightCheckBox;
		private ZArchitecture.GUI.ZCheckBox WasOutOfUSFor45DaysOrLessCheckBox;
		private ZArchitecture.ZCalcEdit BoardedQuantityCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit QuantityCalcDropEdit;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.GUI.ZDateEdit ExportDateEdit;
		private Customs.GUI.LongTextControl DescriptionOfCargoControl;
		private ZArchitecture.GUI.ZCodeFindBox BillIssuerCodeFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentValueCalcDropEdit;
	}
}
