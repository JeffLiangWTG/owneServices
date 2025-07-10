using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsDeclarationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ContainerYardAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BondedWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ShipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InsuranceValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JE_TotalNoOfPiecesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncoTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FinalDestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_DateOfArrivalBoundDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_ExportDateBoundDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HouseBillParcelPostTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightzCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JE_ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OwnersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalNoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JE_ApplicationCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_RS_NKServiceLevelBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_MessageSubTypeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_MessageTypeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_TransportModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_ContainerModeBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.SupplierOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.TransportDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverrideValuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JE_ExportDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_DateOfArrivalBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_MasterBillForAirBoundTextBox = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.FolioNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_MasterBillForSeaBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_VoyageFlightNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RightTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocServicesUserControl = new Enterprise.MasterFiles.GUI.DocServicesUserControl();
			this.OrganisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrganisationsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExternalBrokerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ControllingCustomerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ControllingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DepotAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContainerTerminalOperatorAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ForwarderOrganisationControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ShippingOrAirLineOrganisationControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OrdersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrdersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrdersAttachUserControl = new Enterprise.Freight.Forwarding.Orders.GUI.OrdersAttachUserControl();
			this.ShipmentCustomFieldsPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.shipmentCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.NumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerYardAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.InsuranceValueCalcFindBox.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.FinalDestinationFindBox.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit2.SuspendLayout();
			this.JE_ExportDateBoundDateEdit2.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.DocServicesUserControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.OrdersPanel.SuspendLayout();
			this.OrdersAttachUserControl.SuspendLayout();
			this.ShipmentCustomFieldsPage.SuspendLayout();
			this.shipmentCustomFieldsControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 16, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.TabIndex = 3;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 16, true);
			// 
			// ContainerYardAddressControl
			// 
			this.ContainerYardAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerYardAddressControl, "ContainerYardDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ContainerYardDocAddress)));
			this.ContainerYardAddressControl.BindToOrganisations = "Lookups+ContainerYardCollection";
			this.ContainerYardAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|beae04db-6b41-498d-a0c7-91ebbc10d8a5", "Container Yard");
			this.ContainerYardAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ContainerYardAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 96, true);
			this.ContainerYardAddressControl.Name = "ContainerYardAddressControl";
			this.ContainerYardAddressControl.ReadOnly = false;
			this.ContainerYardAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContainerYardAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContainerYardAddressControl.TabIndex = 4;
			this.ContainerYardAddressControl.ValidationJustForced = false;
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BondedWarehouseDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedWarehouseDocAddressControl, "WarehouseDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).WarehouseDocAddress)));
			this.BondedWarehouseDocAddressControl.BindToOrganisations = "Lookups+BondedWarehouseCollection";
			this.BondedWarehouseDocAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|7f792b4d-3ee5-4f7e-8901-1da0decaf1f6", "Bonded Warehouse");
			this.BondedWarehouseDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BondedWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 118, true);
			this.BondedWarehouseDocAddressControl.Name = "BondedWarehouseDocAddressControl";
			this.BondedWarehouseDocAddressControl.ReadOnly = false;
			this.BondedWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.BondedWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BondedWarehouseDocAddressControl.TabIndex = 5;
			this.BondedWarehouseDocAddressControl.ValidationJustForced = false;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|532018f8-230f-4788-91a4-969a574f4347", "Shipment Details");
			this.ShipmentDetailsGroupBox.Controls.Add(this.InsuranceValueCalcFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ScreenButton);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ScreeningStatusDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.IncoTermExplainButton);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_TotalNoOfPiecesBoundCalcEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.IncoTermDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.FinalDestinationFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_DateOfArrivalBoundDateEdit2);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_ExportDateBoundDateEdit2);
			this.ShipmentDetailsGroupBox.Controls.Add(this.OriginFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.HouseBillParcelPostTextEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.VolumeCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.WeightzCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_ContainerCountCalcEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.OwnersReferenceTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.TotalNoOfPacksCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 225, true);
			this.ShipmentDetailsGroupBox.Name = "ShipmentDetailsGroupBox";
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 306, true);
			this.ShipmentDetailsGroupBox.TabIndex = 5;
			this.ShipmentDetailsGroupBox.TabStop = false;
			//
			// InsuranceValueCalcFindBox
			//
			this.InsuranceValueCalcFindBox.AllowDrop = true;
			this.InsuranceValueCalcFindBox.BindToAmount = "JE_InsuranceValue";
			this.InsuranceValueCalcFindBox.BindToUnit = "JE_RX_NKInsuranceCurrency";
			this.InsuranceValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InsuranceValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 259, true);
			this.InsuranceValueCalcFindBox.Name = "InsuranceValueCalcFindBox";
			this.InsuranceValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.InsuranceValueCalcFindBox.Visible = false;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 259, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 0;
			this.ScreenButton.TabStop = false;
			this.ScreenButton.Text = "...";
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatusDropEdit, "JE_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ScreeningStatus)));
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 259, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ScreeningStatusDropEdit.TabIndex = 25;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.IsCaptionOverridden = true;
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 232, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 21, true);
			this.IncoTermExplainButton.TabIndex = 24;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.ToolTipCaption = null;
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_TotalNoOfPiecesBoundCalcEdit, "JE_TotalNoOfPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPieces)));
			this.JE_TotalNoOfPiecesBoundCalcEdit.CaptionResourceString = null;
			this.JE_TotalNoOfPiecesBoundCalcEdit.DecimalPlaces = 2;
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 184, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.Name = "JE_TotalNoOfPiecesBoundCalcEdit";
			this.JE_TotalNoOfPiecesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 19;
			this.JE_TotalNoOfPiecesBoundCalcEdit.Text = "0";
			this.JE_TotalNoOfPiecesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermDropEdit, "JE_ShipmentIncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ShipmentIncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.IncoTermList)));
			this.IncoTermDropEdit.BindToList = "Lookups.IncoTermList";
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 232, true);
			this.IncoTermDropEdit.Name = "IncoTermDropEdit";
			this.IncoTermDropEdit.PreBoundMaxLength = 3;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.IncoTermDropEdit.TabIndex = 23;
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationFindBox, "JE_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKFinalDestination)));
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.FinalDestinationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.FinalDestinationFindBox.Name = "FinalDestinationFindBox";
			this.FinalDestinationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationFindBox.ParentType = null;
			this.FinalDestinationFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.FinalDestinationFindBox.TabIndex = 7;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.AllowDrop = true;
			this.JE_DateOfArrivalBoundDateEdit2.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_DateOfArrivalBoundDateEdit2, "JE_DateAtFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateAtFinalDestination)));
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 64, true);
			this.JE_DateOfArrivalBoundDateEdit2.Name = "JE_DateOfArrivalBoundDateEdit2";
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 9;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.AllowDrop = true;
			this.JE_ExportDateBoundDateEdit2.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_ExportDateBoundDateEdit2, "JE_DateAtOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateAtOrigin)));
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 40, true);
			this.JE_ExportDateBoundDateEdit2.Name = "JE_ExportDateBoundDateEdit2";
			this.JE_ExportDateBoundDateEdit2.TabIndex = 5;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginFindBox, "JE_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKOrigin)));
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OriginFindBox.ParentType = null;
			this.OriginFindBox.PreBoundMaxLength = 5;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.OriginFindBox.TabIndex = 3;
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.HouseBillParcelPostTextEdit, "JE_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_HouseBill)));
			this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.HouseBillParcelPostTextEdit.Name = "HouseBillParcelPostTextEdit";
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 1;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.VolumeUnitList)));
			this.VolumeCalcDropEdit.BindToAmount = "JE_TotalVolume";
			this.VolumeCalcDropEdit.BindToList = "Lookups.VolumeUnitList";
			this.VolumeCalcDropEdit.BindToUnit = "JE_TotalVolumeUnit";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 17;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightzCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.WeightUnitList)));
			this.WeightzCalcDropEdit.BindToAmount = "JE_TotalWeight";
			this.WeightzCalcDropEdit.BindToList = "Lookups.WeightUnitList";
			this.WeightzCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.WeightzCalcDropEdit.Name = "WeightzCalcDropEdit";
			this.WeightzCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightzCalcDropEdit.TabIndex = 15;
			this.WeightzCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_ContainerCountCalcEdit, "JE_ContainerCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ContainerCount)));
			this.JE_ContainerCountCalcEdit.CaptionResourceString = null;
			this.JE_ContainerCountCalcEdit.DecimalPlaces = 2;
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 184, true);
			this.JE_ContainerCountCalcEdit.Name = "JE_ContainerCountCalcEdit";
			this.JE_ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 19;
			this.JE_ContainerCountCalcEdit.Text = "0";
			this.JE_ContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OwnersReferenceTextBox, "JE_OwnerRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OwnerRef)));
			this.OwnersReferenceTextBox.CaptionResourceString = null;
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 112, true);
			this.OwnersReferenceTextBox.Name = "OwnersReferenceTextBox";
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 13;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalNoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.JE_TotalNoOfPacksPackType_List)));
			this.TotalNoOfPacksCalcDropEdit.BindToAmount = "JE_TotalNoOfPacks";
			this.TotalNoOfPacksCalcDropEdit.BindToList = "Lookups.JE_TotalNoOfPacksPackType_List";
			this.TotalNoOfPacksCalcDropEdit.BindToUnit = "JE_TotalNoOfPacksPackType";
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 208, true);
			this.TotalNoOfPacksCalcDropEdit.Name = "TotalNoOfPacksCalcDropEdit";
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 21;
			this.TotalNoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "JE_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = null;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 11;
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|72ae923a-c475-426b-8314-cff5b5a31cd9", "Shipment Type");
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_ApplicationCodeBoundDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_RS_NKServiceLevelBoundFindBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_MessageSubTypeBoundDropDownEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_MessageTypeBoundDropDownEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_TransportModeBoundDropDownEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_ContainerModeBoundDropDownEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.ShipmentTypeGroupBox.Name = "ShipmentTypeGroupBox";
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 185, true);
			this.ShipmentTypeGroupBox.TabIndex = 2;
			this.ShipmentTypeGroupBox.TabStop = false;
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_ApplicationCodeBoundDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ApplicationCodeList)));
			this.JE_ApplicationCodeBoundDropEdit.BindToList = "Lookups+ApplicationCodeList";
			this.JE_ApplicationCodeBoundDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|8f033269-a3a8-465e-ab96-edae7ef516bd", "Sbmt Type", "Submit Type", "The submit type of the declaration. This can be set in Registry > Customs > Integration > Local Country Customs Interface.");
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 146, true);
			this.JE_ApplicationCodeBoundDropEdit.Name = "JE_ApplicationCodeBoundDropEdit";
			this.JE_ApplicationCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 10;
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_RS_NKServiceLevelBoundFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ServiceLevels)));
			this.JE_RS_NKServiceLevelBoundFindBox.BindToList = "Lookups.ServiceLevels";
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 120, true);
			this.JE_RS_NKServiceLevelBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.JE_RS_NKServiceLevelBoundFindBox.Name = "JE_RS_NKServiceLevelBoundFindBox";
			this.JE_RS_NKServiceLevelBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_RS_NKServiceLevelBoundFindBox.ParentType = null;
			this.JE_RS_NKServiceLevelBoundFindBox.PreBoundMaxLength = 3;
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 9;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.JE_MessageSubTypeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_MessageSubTypeBoundDropDownEdit, "JE_MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.MessageSubTypeList)));
			this.JE_MessageSubTypeBoundDropDownEdit.BindToList = "Lookups.MessageSubTypeList";
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 42, true);
			this.JE_MessageSubTypeBoundDropDownEdit.Name = "JE_MessageSubTypeBoundDropDownEdit";
			this.JE_MessageSubTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 3;
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_MessageTypeBoundDropDownEdit, "JE_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.MessageTypeList)));
			this.JE_MessageTypeBoundDropDownEdit.BindToList = "Lookups.MessageTypeList";
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.JE_MessageTypeBoundDropDownEdit.Name = "JE_MessageTypeBoundDropDownEdit";
			this.JE_MessageTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_MessageTypeBoundDropDownEdit.TabIndex = 1;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_TransportModeBoundDropDownEdit, "JE_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.TransportTypeList)));
			this.JE_TransportModeBoundDropDownEdit.BindToList = "Lookups.TransportTypeList";
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 68, true);
			this.JE_TransportModeBoundDropDownEdit.Name = "JE_TransportModeBoundDropDownEdit";
			this.JE_TransportModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 5;
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_ContainerModeBoundDropDownEdit, "JE_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.CargoIdTypeList)));
			this.JE_ContainerModeBoundDropDownEdit.BindToList = "Lookups.CargoIdTypeList";
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 94, true);
			this.JE_ContainerModeBoundDropDownEdit.Name = "JE_ContainerModeBoundDropDownEdit";
			this.JE_ContainerModeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 7;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.ImporterOrganisationControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|76811fc8-ffbd-4733-af48-93c8bab3af08", "Importer");
			this.ImporterOrganisationControl.Captions = new string[] { "Importer" };
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControl.TabIndex = 1;
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierOrganisationControl, "JE_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Supplier)));
			this.SupplierOrganisationControl.BindToMiscellaneousFields = "JE_SupplierMiscFields";
			this.SupplierOrganisationControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|20d6a89f-156f-40f8-b8f1-3ac576c1a4ca", "Main Supplier");
			this.SupplierOrganisationControl.Captions = new string[] { "Main Supplier" };
			this.SupplierOrganisationControl.IsCaptionOverridden = false;
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.SupplierOrganisationControl.Name = "SupplierOrganisationControl";
			this.SupplierOrganisationControl.PopupCaption = "";
			this.SupplierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.SupplierOrganisationControl.TabIndex = 0;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|0d93a078-1ebe-4dce-81b3-65b01a05ecf7", "Transport Details");
			this.TransportDetailsGroupBox.Controls.Add(this.OverrideValuesCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_ExportDateBoundDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_DateOfArrivalBoundDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfDischargeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfLoadingFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_MasterBillForAirBoundTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.FolioNumberTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.VesselFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_MasterBillForSeaBoundTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_VoyageFlightNoBoundTextBox);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 56, true);
			this.TransportDetailsGroupBox.Name = "TransportDetailsGroupBox";
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 168, true);
			this.TransportDetailsGroupBox.TabIndex = 4;
			this.TransportDetailsGroupBox.TabStop = false;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideValuesCheckBox, "JE_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OverrideFreightDefaults)));
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, -2, true);
			this.OverrideValuesCheckBox.Name = "OverrideValuesCheckBox";
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.OverrideValuesCheckBox.TabIndex = 0;
			this.OverrideValuesCheckBox.UseVisualStyleBackColor = false;
			this.OverrideValuesCheckBox.Visible = false;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.AllowDrop = true;
			this.JE_ExportDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_ExportDateBoundDateEdit, "JE_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ExportDate)));
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 88, true);
			this.JE_ExportDateBoundDateEdit.Name = "JE_ExportDateBoundDateEdit";
			this.JE_ExportDateBoundDateEdit.TabIndex = 12;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.AllowDrop = true;
			this.JE_DateOfArrivalBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_DateOfArrivalBoundDateEdit, "JE_DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateOfArrival)));
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 112, true);
			this.JE_DateOfArrivalBoundDateEdit.Name = "JE_DateOfArrivalBoundDateEdit";
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 16;
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeFindBox, "JE_RL_NKPortOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfArrival)));
			this.PortOfDischargeFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|aa5bf342-e42f-4a68-847b-ba1b693b5a7e", "Discharge", "Port Of Discharge", "");
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 112, true);
			this.PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeFindBox.Name = "PortOfDischargeFindBox";
			this.PortOfDischargeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeFindBox.ParentType = null;
			this.PortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PortOfDischargeFindBox.TabIndex = 14;
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingFindBox, "JE_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfLoading)));
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfLoadingFindBox.Name = "PortOfLoadingFindBox";
			this.PortOfLoadingFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingFindBox.ParentType = null;
			this.PortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 10;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.AllowAlphaInMAWP = false;
			this.JE_MasterBillForAirBoundTextBox.AllowDrop = true;
			this.JE_MasterBillForAirBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JE_MasterBillForAirBoundTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MasterBill)));
			this.JE_MasterBillForAirBoundTextBox.FormattedMasterBill = "";
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.JE_MasterBillForAirBoundTextBox.Name = "JE_MasterBillForAirBoundTextBox";
			this.JE_MasterBillForAirBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.JE_MasterBillForAirBoundTextBox.TabIndex = 2;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FolioNumberTextBox, "JE_Folio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_Folio)));
			this.FolioNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FolioNumberTextBox, false);
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 64, true);
			this.FolioNumberTextBox.Name = "FolioNumberTextBox";
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.FolioNumberTextBox.TabIndex = 8;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselFindBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VesselName)));
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.VesselFindBox.Name = "VesselFindBox";
			this.VesselFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselFindBox.ParentType = null;
			this.VesselFindBox.PreBoundMaxLength = 35;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.VesselFindBox.TabIndex = 5;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_MasterBillForSeaBoundTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MasterBill)));
			this.JE_MasterBillForSeaBoundTextBox.CaptionResourceString = null;
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.JE_MasterBillForSeaBoundTextBox.Name = "JE_MasterBillForSeaBoundTextBox";
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.JE_MasterBillForSeaBoundTextBox.TabIndex = 3;
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_VoyageFlightNoBoundTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.JE_VoyageFlightNoBoundTextBox.CaptionResourceString = null;
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.JE_VoyageFlightNoBoundTextBox.Name = "JE_VoyageFlightNoBoundTextBox";
			this.JE_VoyageFlightNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 7;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Controls.Add(this.DocsTabPage);
			this.RightTabControl.Controls.Add(this.OrganisationsTabPage);
			this.RightTabControl.Controls.Add(this.OrdersTabPage);
			this.RightTabControl.Controls.Add(this.ShipmentCustomFieldsPage);
			this.RightTabControl.Controls.Add(this.NumbersTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 52, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 450, true);
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 510, true);
			this.RightTabControl.TabIndex = 6;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|165141f1-3188-401d-8899-246685560037", "Services");
			this.DocsTabPage.Controls.Add(this.DocServicesUserControl);
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocsTabPage.Name = "DocsTabPage";
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.DocsTabPage.TabIndex = 2;
			// 
			// DocServicesUserControl
			// 
			this.DocServicesUserControl.AllowDrop = true;
			this.DocServicesUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.DocServicesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.IHaveServices)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobServiceDependentCollection)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.Services)));
			this.DocServicesUserControl.BindToServices = "DocsAndCartage+Services";
			this.DocServicesUserControl.ContextColumnVisibleInGrid = false;
			this.DocServicesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocServicesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocServicesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 224, true);
			this.DocServicesUserControl.Name = "DocServicesUserControl";
			this.DocServicesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 483, true);
			this.DocServicesUserControl.TabIndex = 0;
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.AutoScroll = true;
			this.OrganisationsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|0b37ddd9-324c-4e8b-b19d-0f8f8d6aadbd", "Organizations");
			this.OrganisationsTabPage.Controls.Add(this.OrganisationsTopPanel);
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrganisationsTabPage.Name = "OrganisationsTabPage";
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.OrganisationsTabPage.TabIndex = 0;
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Controls.Add(this.ExternalBrokerGuidFindBox);
			this.OrganisationsTopPanel.Controls.Add(this.ControllingCustomerGuidFindBox);
			this.OrganisationsTopPanel.Controls.Add(this.ControllingAgentGuidFindBox);
			this.OrganisationsTopPanel.Controls.Add(this.BondedWarehouseDocAddressControl);
			this.OrganisationsTopPanel.Controls.Add(this.ContainerYardAddressControl);
			this.OrganisationsTopPanel.Controls.Add(this.DepotAddressControl);
			this.OrganisationsTopPanel.Controls.Add(this.ContainerTerminalOperatorAddressControl);
			this.OrganisationsTopPanel.Controls.Add(this.ForwarderOrganisationControl);
			this.OrganisationsTopPanel.Controls.Add(this.ShippingOrAirLineOrganisationControl);
			this.OrganisationsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationsTopPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OrganisationsTopPanel.Name = "OrganisationsTopPanel";
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 206, true);
			this.OrganisationsTopPanel.TabIndex = 0;
			// 
			// ExternalBrokerGuidFindBox
			// 
			this.ExternalBrokerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExternalBrokerGuidFindBox, "JE_OH_ExternalBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ExternalBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ExternalBrokers)));
			this.ExternalBrokerGuidFindBox.BindToList = "Lookups.ExternalBrokers";
			this.ExternalBrokerGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fe78ca2f-c9a6-4ef6-915f-4a910916430f", "External Broker");
			this.ExternalBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 184, true);
			this.ExternalBrokerGuidFindBox.Name = "ExternalBrokerGuidFindBox";
			this.ExternalBrokerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExternalBrokerGuidFindBox.ParentType = null;
			this.ExternalBrokerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ExternalBrokerGuidFindBox.TabIndex = 8;
			// 
			// ControllingCustomerGuidFindBox
			// 
			this.ControllingCustomerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerGuidFindBox, "JE_OH_ControllingCustomer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ControllingCustomer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ControllingCustomers)));
			this.ControllingCustomerGuidFindBox.BindToList = "Lookups.ControllingCustomers";
			this.ControllingCustomerGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("802413f8-6747-4476-9686-533978354764", "Controlling Customer");
			this.ControllingCustomerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 162, true);
			this.ControllingCustomerGuidFindBox.Name = "ControllingCustomerGuidFindBox";
			this.ControllingCustomerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ControllingCustomerGuidFindBox.ParentType = null;
			this.ControllingCustomerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ControllingCustomerGuidFindBox.TabIndex = 7;
			// 
			// ControllingAgentGuidFindBox
			// 
			this.ControllingAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentGuidFindBox, "JE_OH_ControllingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ControllingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ControllingAgents)));
			this.ControllingAgentGuidFindBox.BindToList = "Lookups.ControllingAgents";
			this.ControllingAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9f2cf91a-8deb-4e70-bab4-521842f38a3c", "Controlling Agent");
			this.ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 140, true);
			this.ControllingAgentGuidFindBox.Name = "ControllingAgentGuidFindBox";
			this.ControllingAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ControllingAgentGuidFindBox.ParentType = null;
			this.ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ControllingAgentGuidFindBox.TabIndex = 6;
			// 
			// DepotAddressControl
			// 
			this.DepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepotAddressControl, "DepotDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DepotDocAddress)));
			this.DepotAddressControl.BindToOrganisations = "Lookups+DepotCollection";
			this.DepotAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|87fd913c-bc3e-4451-9db7-3ad16f689be9", "Depot");
			this.DepotAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 74, true);
			this.DepotAddressControl.Name = "DepotAddressControl";
			this.DepotAddressControl.ReadOnly = false;
			this.DepotAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DepotAddressControl.TabIndex = 3;
			this.DepotAddressControl.ValidationJustForced = false;
			// 
			// ContainerTerminalOperatorAddressControl
			// 
			this.ContainerTerminalOperatorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerTerminalOperatorAddressControl, "ContainerTerminalOperatorDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ContainerTerminalOperatorDocAddress)));
			this.ContainerTerminalOperatorAddressControl.BindToOrganisations = "Lookups+ContainerTerminalOperatorCollection";
			this.ContainerTerminalOperatorAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|6fb918d1-1f26-45a9-81a3-3bf3acefa7ea", "CTO");
			this.ContainerTerminalOperatorAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ContainerTerminalOperatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 52, true);
			this.ContainerTerminalOperatorAddressControl.Name = "ContainerTerminalOperatorAddressControl";
			this.ContainerTerminalOperatorAddressControl.ReadOnly = false;
			this.ContainerTerminalOperatorAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContainerTerminalOperatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContainerTerminalOperatorAddressControl.TabIndex = 2;
			this.ContainerTerminalOperatorAddressControl.ValidationJustForced = false;
			// 
			// ForwarderOrganisationControl
			// 
			this.ForwarderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderOrganisationControl, "JE_OH_Forwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Forwarder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ForwarderList)));
			this.ForwarderOrganisationControl.BindToList = "Lookups.ForwarderList";
			this.ForwarderOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|14bd31a9-397d-4908-ac98-c9a2c6274a24", "Forwarder");
			this.ForwarderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 30, true);
			this.ForwarderOrganisationControl.Name = "ForwarderOrganisationControl";
			this.ForwarderOrganisationControl.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ForwarderOrganisationControl.ParentType = null;
			this.ForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ForwarderOrganisationControl.TabIndex = 1;
			// 
			// ShippingOrAirLineOrganisationControl
			// 
			this.ShippingOrAirLineOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingOrAirLineOrganisationControl, "JE_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ShippingLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ShippingLineList)));
			this.ShippingOrAirLineOrganisationControl.BindToList = "Lookups.ShippingLineList";
			this.ShippingOrAirLineOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|c5409f2b-6e62-468c-bf33-57865188ae5e", "Carrier", "Carrier", "Carrier", "");
			this.ShippingOrAirLineOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 8, true);
			this.ShippingOrAirLineOrganisationControl.Name = "ShippingOrAirLineOrganisationControl";
			this.ShippingOrAirLineOrganisationControl.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ShippingOrAirLineOrganisationControl.ParentType = null;
			this.ShippingOrAirLineOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ShippingOrAirLineOrganisationControl.TabIndex = 0;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|da2355f9-106e-47e2-9c58-a5f6ada697a6", "Orders");
			this.OrdersTabPage.Controls.Add(this.OrdersPanel);
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Name = "OrdersTabPage";
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.OrdersTabPage.TabIndex = 3;
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Controls.Add(this.OrdersAttachUserControl);
			this.OrdersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersPanel.Name = "OrdersPanel";
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.OrdersPanel.TabIndex = 1;
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrdersAttachUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)))));
			this.OrdersAttachUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrdersAttachUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersAttachUserControl.Name = "OrdersAttachUserControl";
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 418, true);
			this.OrdersAttachUserControl.TabIndex = 0;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|12074bc8-8798-4644-adf6-4e3377fa4f91", "Custom");
			this.ShipmentCustomFieldsPage.Controls.Add(this.shipmentCustomFieldsControl1);
			this.ShipmentCustomFieldsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipmentCustomFieldsPage.Name = "ShipmentCustomFieldsPage";
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.ShipmentCustomFieldsPage.TabIndex = 5;
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.AllowDrop = true;
			this.shipmentCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shipmentCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.shipmentCustomFieldsControl1.Name = "shipmentCustomFieldsControl1";
			this.shipmentCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields" + " in Workflow Manager.";
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.shipmentCustomFieldsControl1.TabIndex = 0;
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|5cea34c8-b76c-425b-8b4c-d742ff0c66bd", "Numbers");
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NumbersTabPage.Name = "NumbersTabPage";
			this.NumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 483, true);
			this.NumbersTabPage.TabIndex = 6;
			this.NumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// BaseCustomsDeclarationUserControl
			// 
			this.Controls.Add(this.RightTabControl);
			this.Controls.Add(this.TransportDetailsGroupBox);
			this.Controls.Add(this.ShipmentDetailsGroupBox);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Controls.Add(this.ShipmentTypeGroupBox);
			this.Controls.Add(this.SupplierOrganisationControl);
			this.Name = "BaseCustomsDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 565, true);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.InsuranceValueCalcFindBox.ResumeLayout(true);
			this.InsuranceValueCalcFindBox.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit2.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit2.PerformLayout();
			this.JE_ExportDateBoundDateEdit2.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit2.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.JE_ExportDateBoundDateEdit.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.JE_MasterBillForAirBoundTextBox.ResumeLayout(true);
			this.JE_MasterBillForAirBoundTextBox.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.DocServicesUserControl.ResumeLayout(true);
			this.DocServicesUserControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.OrganisationsTopPanel.ResumeLayout(false);
			this.OrganisationsTopPanel.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.ShippingOrAirLineOrganisationControl.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.OrdersPanel.ResumeLayout(false);
			this.OrdersPanel.PerformLayout();
			this.OrdersAttachUserControl.ResumeLayout(true);
			this.OrdersAttachUserControl.PerformLayout();
			this.ShipmentCustomFieldsPage.ResumeLayout(false);
			this.ShipmentCustomFieldsPage.PerformLayout();
			this.shipmentCustomFieldsControl1.ResumeLayout(true);
			this.shipmentCustomFieldsControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public Enterprise.ZArchitecture.GUI.ZDropEdit JE_MessageTypeBoundDropDownEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit JE_TransportModeBoundDropDownEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit JE_ContainerModeBoundDropDownEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit JE_MessageSubTypeBoundDropDownEdit;
		public Enterprise.ZArchitecture.GUI.ZCalcDropEdit WeightzCalcDropEdit;
		public Enterprise.ZArchitecture.ZTextBox OwnersReferenceTextBox;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox InsuranceValueCalcFindBox;
		public Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
		public Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		public Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalNoOfPacksCalcDropEdit;
		public Enterprise.ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		public Enterprise.ZArchitecture.ZMasterBillControl JE_MasterBillForAirBoundTextBox;
		public Enterprise.ZArchitecture.ZTextBox FolioNumberTextBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselFindBox;
		public Enterprise.ZArchitecture.ZTextBox JE_MasterBillForSeaBoundTextBox;
		public Enterprise.ZArchitecture.ZTextBox JE_VoyageFlightNoBoundTextBox;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JE_ExportDateBoundDateEdit;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JE_DateOfArrivalBoundDateEdit;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDischargeFindBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfLoadingFindBox;
		public ZGroupBox ShipmentDetailsGroupBox;
		public ZGroupBox ShipmentTypeGroupBox;
		public Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControl;
		public Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous SupplierOrganisationControl;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationFindBox;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JE_DateOfArrivalBoundDateEdit2;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JE_ExportDateBoundDateEdit2;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
		public Enterprise.ZArchitecture.ZTextBox HouseBillParcelPostTextEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit IncoTermDropEdit;
		public Enterprise.ZArchitecture.GUI.ZTabControl RightTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage OrganisationsTabPage;
		protected internal OrganisationsUserControl OrganisationsUserControl;
		public ZPanel OrganisationsTopPanel;
		public ZGuidFindBox ShippingOrAirLineOrganisationControl;
		public ZGuidFindBox ForwarderOrganisationControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage OrdersTabPage;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JE_RS_NKServiceLevelBoundFindBox;
		DocServicesUserControl DocServicesUserControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage DocsTabPage;
		public Enterprise.ZArchitecture.ZCalcEdit JE_ContainerCountCalcEdit;
		public Enterprise.ZArchitecture.ZCalcEdit JE_TotalNoOfPiecesBoundCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZButton IncoTermExplainButton;
		public Enterprise.ZArchitecture.GUI.ZCheckBox OverrideValuesCheckBox;
		public Enterprise.ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		protected internal ZDocAddressControl ContainerTerminalOperatorAddressControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage ShipmentCustomFieldsPage;
		public Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl shipmentCustomFieldsControl1;
		protected ZPanel OrdersPanel;
		protected internal ZDocAddressControl DepotAddressControl;
		public ZDocAddressControl BondedWarehouseDocAddressControl;
		protected ZDocAddressControl ContainerYardAddressControl;
		public ZTabPage NumbersTabPage;
		protected OrdersAttachUserControl OrdersAttachUserControl;
		public ZDropEdit JE_ApplicationCodeBoundDropEdit;
		public ZGuidFindBox ExternalBrokerGuidFindBox;
		public ZGuidFindBox ControllingCustomerGuidFindBox;
		public ZGuidFindBox ControllingAgentGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox TransportDetailsGroupBox;
		public DynamicLayoutPanel DeclarationDetailsLayoutPanel;
		public DynamicLayoutPanel ShipmentTypeLayoutPanel;
		public DynamicLayoutPanel TransportDetailsLayoutPanel;
		public DynamicLayoutPanel ShipmentDetailsLayoutPanel;
		public DynamicLayoutPanel OrganisationDetailsLayoutPanel;

		#endregion
	}
}
