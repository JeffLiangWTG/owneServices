using System;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentDetails
	{
		#region Component Designer generated code

		public ZOrganisationControl ClientOrganisationControl;
		protected internal ZTemplateTabControl ShipmentServicesTabControl;
		protected ZTabPage ShipmentTabPage;
		public ZStmNotePopupButton DescriptionNotesButton;
		private ZDropEdit TransportModeDropEdit;
		private ZCheckBox JS_TranshipToOtherCFSCheckBox;
		private ZTextBox JS_HouseBillTextBox;
		private ZTextBox CustomsEntryNumberTextBox;
		private ZTextBox JS_ConsolReferenceTextBox;
		private ZTextBox JS_GoodsDescriptionTextBox;
		private ZCodeFindBox JS_RL_NKDestinationCodeFindBox;
		private ZCodeFindBox JS_RL_NKOriginCodeFindBox;
		private ZDateEdit JS_A_RCVBoundTextEdit;
		private ZTextBox JS_InterimReceiptBoundTextEdit;
		private ZTextBox JS_CartageWaybillTextBox;
		private ZTabPage ServicesTabPage;
		private ZTemplateTabControl TransportTabControl;
		private ZTabPage LoadListTabPage;
		private CargoWise.Windows.UI.KPanel panel1;
		public ZGroupBox SailingGroupBox;
		public ZPanel LoadListSailingPanel;
		private ZDateEdit zDateEdit1;
		private ZDateEdit zDateEdit2;
		private ZTextBox zTextBox2;
		private ZTextBox zTextBox3;
		private ZTextBox zTextBox4;
		private ZTextBox zTextBox5;
		private ZTabPage CoLoadTabPage;
		private ZTextBox ColoadMasterShipmentHouseBillBoundTextBox;
		private ZGuidFindBox JS_JS_ColoadMasterShipmentFindbox;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZGroupBox PacksGroupBox;
		private ZPanel PackLineDetailsPanel;
		private CargoWise.Windows.UI.KSplitter splitter2;
		public ZPanel LeftPanel;
		private ZPanel TopPanel;
		internal ZTextBox JK_MasterBillNumTextBox;
		private ZDateEdit LCLStorageDateEdit;
		private ZDateEdit LCLAvailableDateEdit;
		protected internal ZDocAddressControl ConsignorDocumentaryDocAddressControl;
		protected internal ZDocAddressControl ConsigneeDocumentaryDocAddressControl;
		internal ZTabPage DatesAndCartageTabPage;
		protected ZTabPage PickupTabPage;
		protected ZTabPage DeliveryTabPage;
		protected internal ZDocAddressControl ConsignorPickupDocAddressControl;
		protected internal ZDocAddressControl ConsigneePickupDocAddressControl;
		private ZTemplateTabControl MinorDetailsTabControl;
		private ZTabPage LocationsTabPage;
		private ZTabPage MarksAndNumbersTabPage;
		private ZTextBox MarksAndNumbersTextBox;
		private ZCodeFindBox JS_RS_NKServiceLevelFindbox;
		protected ZTextBox WarehouseTextBox;
		public ZGuidDropEdit WarehouseDropEdit;
		protected ZTextBox LocationTextBox;
		private ZGroupBox DeliveryCartageCoGroupBox;
		private ZAddressControl zAddressControl1;
		public ZPanel ShipmentSailingPanel;
		private ZDateEdit ARVDateEdit;
		private ZDateEdit DEPDateEdit;
		private ZTextBox JS_Calc_NKDischargePortTextBox;
		private ZTextBox JS_Calc_NKLoadPortTextBox;
		internal ZTextBox VesselNameTextBox;
		internal ZTextBox VoyageNoTextBox;
		public ZButton SelectSailingButton;
		private CFSShipmentConsolsModuleButtonGrid LoadListConsolModuleButtonGrid;
		private ZGrid CoLoadShipmentsBoundGrid;
		private ZCalcDropEdit ShipmentTotalPacksCalcDropEdit;
		private ZCalcDropEdit ShipmentTotalVolumeCalcDropEdit;
		private ZCalcDropEdit ShipmentTotalWeightCalcDropEdit;
		ServicesControl ServicesControl;
		ZPanel ServicesPanel;
		protected ZGrid LocationsGrid;
		private ZGrid PacklinesGrid;
		private ZDropEdit JS_ShipmentTypeBoundDropDownEdit;
		private ZTextBox masterConsolsTextBox;
		private ZTabPage NumbersTabPage;
		ZPanel AddtionalPanel;
		NumbersControl ReferenceNumbersControl;

		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			this.ClientOrganisationControl = new ZOrganisationControl();
			this.LeftPanel = new ZPanel();
			this.JS_ShipmentTypeBoundDropDownEdit = new ZDropEdit();
			this.ConsignorDocumentaryDocAddressControl = new ZDocAddressControl();
			this.ConsigneeDocumentaryDocAddressControl = new ZDocAddressControl();
			this.TopPanel = new ZPanel();
			this.TransportTabControl = new ZTemplateTabControl();
			this.LoadListTabPage = new ZTabPage();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.JK_MasterBillNumTextBox = new ZTextBox();
			this.LoadListConsolModuleButtonGrid = new CFSShipmentConsolsModuleButtonGrid();
			this.SailingGroupBox = new ZGroupBox();
			this.ShipmentSailingPanel = new ZPanel();
			this.ARVDateEdit = new ZDateEdit();
			this.DEPDateEdit = new ZDateEdit();
			this.JS_Calc_NKDischargePortTextBox = new ZTextBox();
			this.JS_Calc_NKLoadPortTextBox = new ZTextBox();
			this.VesselNameTextBox = new ZTextBox();
			this.VoyageNoTextBox = new ZTextBox();
			this.SelectSailingButton = new ZButton();
			this.LoadListSailingPanel = new ZPanel();
			this.zDateEdit1 = new ZDateEdit();
			this.zDateEdit2 = new ZDateEdit();
			this.zTextBox2 = new ZTextBox();
			this.zTextBox3 = new ZTextBox();
			this.zTextBox4 = new ZTextBox();
			this.zTextBox5 = new ZTextBox();
			this.CoLoadTabPage = new ZTabPage();
			this.JS_JS_ColoadMasterShipmentFindbox = new ZGuidFindBox();
			this.ColoadMasterShipmentHouseBillBoundTextBox = new ZTextBox();
			this.masterConsolsTextBox = new ZTextBox();
			this.CoLoadShipmentsBoundGrid = new ZGrid();
			this.ShipmentServicesTabControl = new ZTemplateTabControl();
			this.ShipmentTabPage = new ZTabPage();
			this.WarehouseTextBox = new ZTextBox();
			this.WarehouseDropEdit = new ZGuidDropEdit();
			this.LocationTextBox = new ZTextBox();
			this.JS_RS_NKServiceLevelFindbox = new ZCodeFindBox();
			this.DescriptionNotesButton = new ZStmNotePopupButton();
			this.TransportModeDropEdit = new ZDropEdit();
			this.ShipmentTotalPacksCalcDropEdit = new ZCalcDropEdit();
			this.ShipmentTotalVolumeCalcDropEdit = new ZCalcDropEdit();
			this.JS_TranshipToOtherCFSCheckBox = new ZCheckBox();
			this.JS_HouseBillTextBox = new ZTextBox();
			this.CustomsEntryNumberTextBox = new ZTextBox();
			this.JS_ConsolReferenceTextBox = new ZTextBox();
			this.JS_GoodsDescriptionTextBox = new ZTextBox();
			this.ShipmentTotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.JS_RL_NKDestinationCodeFindBox = new ZCodeFindBox();
			this.JS_RL_NKOriginCodeFindBox = new ZCodeFindBox();
			this.JS_A_RCVBoundTextEdit = new ZDateEdit();
			this.JS_InterimReceiptBoundTextEdit = new ZTextBox();
			this.JS_CartageWaybillTextBox = new ZTextBox();
			this.DatesAndCartageTabPage = new ZTabPage();
			this.DeliveryCartageCoGroupBox = new ZGroupBox();
			this.zAddressControl1 = new ZAddressControl();
			this.LCLAvailableDateEdit = new ZDateEdit();
			this.LCLStorageDateEdit = new ZDateEdit();
			this.PickupTabPage = new ZTabPage();
			this.ConsignorPickupDocAddressControl = new ZDocAddressControl();
			this.DeliveryTabPage = new ZTabPage();
			this.ConsigneePickupDocAddressControl = new ZDocAddressControl();
			this.ServicesTabPage = new ZTabPage();
			this.ServicesPanel = new ZPanel();
			this.ServicesControl = new ServicesControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.PacksGroupBox = new ZGroupBox();
			this.PackLineDetailsPanel = new ZPanel();
			this.MinorDetailsTabControl = new ZTemplateTabControl();
			this.LocationsTabPage = new ZTabPage();
			this.LocationsGrid = new ZGrid();
			this.MarksAndNumbersTabPage = new ZTabPage();
			this.MarksAndNumbersTextBox = new ZTextBox();
			this.splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.PacklinesGrid = new ZGrid();
			this.NumbersTabPage = new ZTabPage();
			this.AddtionalPanel = new ZPanel();
			this.ReferenceNumbersControl = new NumbersControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LeftPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.TransportTabControl.SuspendLayout();
			this.LoadListTabPage.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LoadListConsolModuleButtonGrid.InnerGrid)).BeginInit();
			this.SailingGroupBox.SuspendLayout();
			this.ShipmentSailingPanel.SuspendLayout();
			this.LoadListSailingPanel.SuspendLayout();
			this.CoLoadTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CoLoadShipmentsBoundGrid)).BeginInit();
			this.ShipmentServicesTabControl.SuspendLayout();
			this.ShipmentTabPage.SuspendLayout();
			this.DatesAndCartageTabPage.SuspendLayout();
			this.DeliveryCartageCoGroupBox.SuspendLayout();
			this.PickupTabPage.SuspendLayout();
			this.DeliveryTabPage.SuspendLayout();
			this.ServicesTabPage.SuspendLayout();
			this.ServicesPanel.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.AddtionalPanel.SuspendLayout();
			this.PacksGroupBox.SuspendLayout();
			this.PackLineDetailsPanel.SuspendLayout();
			this.MinorDetailsTabControl.SuspendLayout();
			this.LocationsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocationsGrid)).BeginInit();
			this.MarksAndNumbersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PacklinesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSShipment);
			// 
			// ClientOrganisationControl
			// 
			this.ClientOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientOrganisationControl, "JS_OH_HandledOnBehalfOfForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CFSShipment)(null)).JS_OH_HandledOnBehalfOfForwarder)));
			this.ClientOrganisationControl.BindToOrganisations = "Lookups.Forwarder_List";
			this.ClientOrganisationControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Client");
			this.ClientOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.ClientOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientOrganisationControl.Name = "ClientOrganisationControl";
			this.ClientOrganisationControl.PopupCaption = "";
			this.ClientOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 52, true);
			this.ClientOrganisationControl.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.JS_ShipmentTypeBoundDropDownEdit);
			this.LeftPanel.Controls.Add(this.ConsignorDocumentaryDocAddressControl);
			this.LeftPanel.Controls.Add(this.ConsigneeDocumentaryDocAddressControl);
			this.LeftPanel.Controls.Add(this.ClientOrganisationControl);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 488, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// JS_ShipmentTypeBoundDropDownEdit
			// 
			this.JS_ShipmentTypeBoundDropDownEdit.AllowDrop = true;
			this.JS_ShipmentTypeBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JS_ShipmentTypeBoundDropDownEdit, "JS_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).JS_ShipmentType)));
			this.JS_ShipmentTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 85, true);
			this.JS_ShipmentTypeBoundDropDownEdit.Name = "JS_ShipmentTypeBoundDropDownEdit";
			this.JS_ShipmentTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JS_ShipmentTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.JS_ShipmentTypeBoundDropDownEdit.TabIndex = 3;
			// 
			// ConsignorDocumentaryDocAddressControl
			// 
			this.ConsignorDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocumentaryDocAddressControl, "ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((CFSShipment)(null)).ConsignorDocumentaryAddress)));
			this.ConsignorDocumentaryDocAddressControl.BindToOrganisations = "Lookups.ConsignorForwarder_List";
			this.ConsignorDocumentaryDocAddressControl.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			this.ConsignorDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.ConsignorDocumentaryDocAddressControl.Name = "ConsignorDocumentaryDocAddressControl";
			this.ConsignorDocumentaryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsignorDocumentaryDocAddressControl.TabIndex = 1;
			// 
			// ConsigneeDocumentaryDocAddressControl
			// 
			this.ConsigneeDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocumentaryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((CFSShipment)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocumentaryDocAddressControl.BindToOrganisations = "Lookups.ConsigneeForwarder_List";
			this.ConsigneeDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|0D25BB9C-1680-4491-A459-78D4E3064BDE", "Consignee");
			this.ConsigneeDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 299, true);
			this.ConsigneeDocumentaryDocAddressControl.Name = "ConsigneeDocumentaryDocAddressControl";
			this.ConsigneeDocumentaryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeDocumentaryDocAddressControl.TabIndex = 2;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.TransportTabControl);
			this.TopPanel.Controls.Add(this.ShipmentServicesTabControl);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 256, true);
			this.TopPanel.TabIndex = 1;
			// 
			// TransportTabControl
			// 
			this.TransportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransportTabControl.Controls.Add(this.LoadListTabPage);
			this.TransportTabControl.Controls.Add(this.CoLoadTabPage);
			this.TransportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 0, true);
			this.TransportTabControl.Name = "TransportTabControl";
			this.TransportTabControl.SelectedIndex = 0;
			this.TransportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 256, true);
			this.TransportTabControl.TabIndex = 1;
			// 
			// LoadListTabPage
			// 
			this.LoadListTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|56785b2c-4ce7-48ee-a21f-a4883c6d1c07", "Load List");
			this.LoadListTabPage.Controls.Add(this.panel1);
			this.LoadListTabPage.Controls.Add(this.SailingGroupBox);
			this.LoadListTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LoadListTabPage.Name = "LoadListTabPage";
			this.LoadListTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 229, true);
			this.LoadListTabPage.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.JK_MasterBillNumTextBox);
			this.panel1.Controls.Add(this.LoadListConsolModuleButtonGrid);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Font = new System.Drawing.Font("Tahoma", 8F);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 109, true);
			this.panel1.TabIndex = 1;
			// 
			// JK_MasterBillNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_MasterBillNumTextBox, "Consols.JK_MasterBillNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_MasterBillNum)));
			this.JK_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 88, true);
			this.JK_MasterBillNumTextBox.Name = "JK_MasterBillNumTextBox";
			this.JK_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.JK_MasterBillNumTextBox.TabIndex = 4;
			// 
			// LoadListConsolModuleButtonGrid
			// 
			this.LoadListConsolModuleButtonGrid.AllowDrop = true;
			this.LoadListConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.LoadListConsolModuleButtonGrid, "Consols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSShipment)(null)).Consols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSShipment)(null)).Lookups.Consols_List)));
			this.LoadListConsolModuleButtonGrid.BindToFindBoxList = "Lookups+Consols_List";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|a129a919-a2c1-4afa-9282-7081079ab49a", "Load List ID");
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			this.LoadListConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LoadListConsolModuleButtonGrid.DetachMessage = Enterprise.Freight.CFS.GUI.Res.GetData("882E20A3-29C2-4C5A-9D6B-4C365134FC14", "Are you sure you want to detach the selected Load List Consol?");
			this.LoadListConsolModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.LoadListConsolModuleButtonGrid.GridId = "c422b1a1-a24b-45d2-bac0-f72637bcff4f";
			// 
			// 
			// 
			this.LoadListConsolModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.LoadListConsolModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LoadListConsolModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.LoadListConsolModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.LoadListConsolModuleButtonGrid.InnerGrid.GridId = null;
			this.LoadListConsolModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LoadListConsolModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.LoadListConsolModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LoadListConsolModuleButtonGrid.InnerGrid.Name = "Grid";
			this.LoadListConsolModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.LoadListConsolModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 45, true);
			this.LoadListConsolModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.LoadListConsolModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LoadListConsolModuleButtonGrid.Name = "LoadListConsolModuleButtonGrid";
			this.LoadListConsolModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.CFS.GUI.Res.GetData("BF4305CB-16D8-4761-91EC-8A8047E00064", "Load List Consol");
			this.LoadListConsolModuleButtonGrid.ReadOnly = true;
			this.LoadListConsolModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 83, true);
			this.LoadListConsolModuleButtonGrid.TabIndex = 0;
			// 
			// SailingGroupBox
			// 
			this.SailingGroupBox.Controls.Add(this.ShipmentSailingPanel);
			this.SailingGroupBox.Controls.Add(this.LoadListSailingPanel);
			this.SailingGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SailingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.SailingGroupBox.Name = "SailingGroupBox";
			this.SailingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 120, true);
			this.SailingGroupBox.TabIndex = 2;
			this.SailingGroupBox.TabStop = false;
			// 
			// ShipmentSailingPanel
			// 
			this.ShipmentSailingPanel.Controls.Add(this.ARVDateEdit);
			this.ShipmentSailingPanel.Controls.Add(this.DEPDateEdit);
			this.ShipmentSailingPanel.Controls.Add(this.JS_Calc_NKDischargePortTextBox);
			this.ShipmentSailingPanel.Controls.Add(this.JS_Calc_NKLoadPortTextBox);
			this.ShipmentSailingPanel.Controls.Add(this.VesselNameTextBox);
			this.ShipmentSailingPanel.Controls.Add(this.VoyageNoTextBox);
			this.ShipmentSailingPanel.Controls.Add(this.SelectSailingButton);
			this.ShipmentSailingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 16, true);
			this.ShipmentSailingPanel.Name = "ShipmentSailingPanel";
			this.ShipmentSailingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 100, true);
			this.ShipmentSailingPanel.TabIndex = 0;
			// 
			// ARVDateEdit
			// 
			this.ARVDateEdit.AllowDrop = true;
			this.ARVDateEdit.AutoCompleteMonthThreshold = 1;
			this.ARVDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ARVDateEdit, "JS_Calc_CurrentETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).JS_Calc_CurrentETA)));
			this.ARVDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|532c700e-0c8f-4cbb-af73-166a3dab2fb3", "ETA", "Arrival Date.");
			this.ARVDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 27, true);
			this.ARVDateEdit.Name = "ARVDateEdit";
			this.ARVDateEdit.TabIndex = 7;
			// 
			// DEPDateEdit
			// 
			this.DEPDateEdit.AllowDrop = true;
			this.DEPDateEdit.AutoCompleteMonthThreshold = 1;
			this.DEPDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DEPDateEdit, "JS_Calc_CurrentETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).JS_Calc_CurrentETD)));
			this.DEPDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|476e9774-88ee-44b6-b373-c34a616e6ef9", "ETD", "Departure Date.", "");
			this.DEPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 3, true);
			this.DEPDateEdit.Name = "DEPDateEdit";
			this.DEPDateEdit.TabIndex = 5;
			// 
			// JS_Calc_NKDischargePortTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_Calc_NKDischargePortTextBox, "JS_Calc_CurrentDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_Calc_CurrentDischargePort)));
			this.JS_Calc_NKDischargePortTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|47b4c2d0-e026-44f1-8832-8dea3d32892b", "Disch.", "Discharge", "Port of discharge.");
			this.JS_Calc_NKDischargePortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 27, true);
			this.JS_Calc_NKDischargePortTextBox.Name = "JS_Calc_NKDischargePortTextBox";
			this.JS_Calc_NKDischargePortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JS_Calc_NKDischargePortTextBox.TabIndex = 3;
			// 
			// JS_Calc_NKLoadPortTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_Calc_NKLoadPortTextBox, "JS_Calc_CurrentLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_Calc_CurrentLoadPort)));
			this.JS_Calc_NKLoadPortTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|d9d9c25b-7c84-48e6-8657-214611f7fc86", "Load", "Port of loading.");
			this.JS_Calc_NKLoadPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 3, true);
			this.JS_Calc_NKLoadPortTextBox.Name = "JS_Calc_NKLoadPortTextBox";
			this.JS_Calc_NKLoadPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JS_Calc_NKLoadPortTextBox.TabIndex = 1;
			// 
			// VesselNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselNameTextBox, "JS_Calc_CurrentVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_Calc_CurrentVessel)));
			this.VesselNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 51, true);
			this.VesselNameTextBox.Name = "VesselNameTextBox";
			this.VesselNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.VesselNameTextBox.TabIndex = 9;
			// 
			// VoyageNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNoTextBox, "JS_Calc_CurrentVoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_Calc_CurrentVoyageFlight)));
			this.VoyageNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 75, true);
			this.VoyageNoTextBox.Name = "VoyageNoTextBox";
			this.VoyageNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.VoyageNoTextBox.TabIndex = 11;
			// 
			// SelectSailingButton
			// 
			this.SelectSailingButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|eab73ce2-c2f1-4447-af1d-226e6642bef4", "Select Schedule", "Select Schedule", "");
			this.SelectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 75, true);
			this.SelectSailingButton.Name = "SelectSailingButton";
			this.SelectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.SelectSailingButton.TabIndex = 12;
			// 
			// LoadListSailingPanel
			// 
			this.LoadListSailingPanel.Controls.Add(this.zDateEdit1);
			this.LoadListSailingPanel.Controls.Add(this.zDateEdit2);
			this.LoadListSailingPanel.Controls.Add(this.zTextBox2);
			this.LoadListSailingPanel.Controls.Add(this.zTextBox3);
			this.LoadListSailingPanel.Controls.Add(this.zTextBox4);
			this.LoadListSailingPanel.Controls.Add(this.zTextBox5);
			this.LoadListSailingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 16, true);
			this.LoadListSailingPanel.Name = "LoadListSailingPanel";
			this.LoadListSailingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 100, true);
			this.LoadListSailingPanel.TabIndex = 0;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = false;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "Consols.JK_JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JB_E_ARV)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|b30ba4c3-8959-4073-8341-d9a12e13fb6b", "ETA", "Arrival Date.");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 27, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 7;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "Consols.JK_JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JA_E_DEP)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|60c27844-f18b-47cc-ae76-ba4088405ecf", "ETD", "Departure Date.");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 3, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 5;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "Consols.JK_JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JB_RL_NKPortOfDischarge)));
			this.zTextBox2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|05246d4c-752a-40e1-a303-8a7c431e19da", "Disch.", "Discharge", "Port of discharge.");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 27, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "Consols.JK_JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JA_RL_NKPortOfLoading)));
			this.zTextBox3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|916b288f-5285-4a3e-a410-33535eeb786d", "Load", "Port Of Loading.");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 3, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox3.TabIndex = 1;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "Consols.JK_JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JV_NKVessel)));
			this.zTextBox4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|1b30a201-3ce7-423a-b36b-69115e71a745", "Vessel");
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 51, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.zTextBox4.TabIndex = 9;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "Consols.JK_JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSLoadListConsol)(((System.Collections.IList)(((CFSShipment)(null)).Consols)).SyncRoot)).JK_JX_JV_VoyageFlight)));
			this.zTextBox5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|c3d878a3-4c0d-48f0-a887-02887391b751", "Voyage", "Voyage / Flight No", "");
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 75, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zTextBox5.TabIndex = 11;
			// 
			// CoLoadTabPage
			// 
			this.CoLoadTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|48339b50-5c61-46e7-8fc8-9296558bc525", "Co Load");
			this.CoLoadTabPage.Controls.Add(this.JS_JS_ColoadMasterShipmentFindbox);
			this.CoLoadTabPage.Controls.Add(this.ColoadMasterShipmentHouseBillBoundTextBox);
			this.CoLoadTabPage.Controls.Add(this.masterConsolsTextBox);
			this.CoLoadTabPage.Controls.Add(this.CoLoadShipmentsBoundGrid);
			this.CoLoadTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CoLoadTabPage.Name = "CoLoadTabPage";
			this.CoLoadTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 229, true);
			this.CoLoadTabPage.TabIndex = 1;
			// 
			// JS_JS_ColoadMasterShipmentFindbox
			// 
			this.JS_JS_ColoadMasterShipmentFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_JS_ColoadMasterShipmentFindbox, "JS_JS_ColoadMasterShipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CFSShipment)(null)).JS_JS_ColoadMasterShipment)));
			this.JS_JS_ColoadMasterShipmentFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 0, true);
			this.JS_JS_ColoadMasterShipmentFindbox.Name = "JS_JS_ColoadMasterShipmentFindbox";
			this.JS_JS_ColoadMasterShipmentFindbox.ShowDescriptionBox = false;
			this.JS_JS_ColoadMasterShipmentFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_JS_ColoadMasterShipmentFindbox.TabIndex = 0;
			// 
			// ColoadMasterShipmentHouseBillBoundTextBox
			// 
			this.ColoadMasterShipmentHouseBillBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ColoadMasterShipmentHouseBillBoundTextBox, "ColoadMasterShipmentHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).ColoadMasterShipmentHouseBill)));
			this.ColoadMasterShipmentHouseBillBoundTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|5baede78-2284-4cfe-9843-dcc7dad7b49e", "Co-Load Master Bill", "Master Shipment House Bill", "");
			this.ColoadMasterShipmentHouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 24, true);
			this.ColoadMasterShipmentHouseBillBoundTextBox.Name = "ColoadMasterShipmentHouseBillBoundTextBox";
			this.ColoadMasterShipmentHouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.ColoadMasterShipmentHouseBillBoundTextBox.TabIndex = 1;
			// 
			// masterConsolsTextBox
			// 
			this.masterConsolsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.masterConsolsTextBox, "CoLoadMasterShipment+JS_JK_ConsolID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).CoLoadMasterShipment.JS_JK_ConsolID)));
			this.masterConsolsTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|54fb3c79-aeec-44e1-b85b-a8e65e0a1f14", "Load List ID", "Comma separated list of related load list job numbers.");
			this.masterConsolsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 50, true);
			this.masterConsolsTextBox.Name = "masterConsolsTextBox";
			this.masterConsolsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.masterConsolsTextBox.TabIndex = 2;
			// 
			// CoLoadShipmentsBoundGrid
			// 
			this.CoLoadShipmentsBoundGrid.AllowNavigation = false;
			this.CoLoadShipmentsBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CoLoadShipmentsBoundGrid, "CoLoadShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)).SyncRoot)).JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)).SyncRoot)).JS_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonShipment)(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)).SyncRoot)).ConsignorPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonShipment)(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)).SyncRoot)).ConsigneePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(((System.Collections.IList)(((CFSShipment)(null)).CoLoadShipments)).SyncRoot)).JS_JK_ConsolID)));
			this.CoLoadShipmentsBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|f73b8726-29b4-4922-8a89-8d83c7b71581", "Unique ID", "Co-Load Shipment Unique Consignment Reference.");
			zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo2.ToolTip = "Co-Load Shipment Unique Consignment Reference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|794d2ff6-44b2-41a8-a8bd-080b820d7ee0", "House Bill", "Co load shipment house bill.");
			zTextBoxColumnStyleInfo3.ColumnName = "JS_HouseBill";
			zGuidFindBoxColumnStyleInfo1.Caption = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ConsignorPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|45e88baf-e18b-49ba-9ec7-9d358b122b5b", "Consignee");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ConsigneePK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|99e57060-523c-4d27-9e39-88bd45530800", "Load List ID", "Comma separated list of related load list job numbers.");
			zTextBoxColumnStyleInfo4.ColumnName = "JS_JK_ConsolID";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			this.CoLoadShipmentsBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CoLoadShipmentsBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CoLoadShipmentsBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CoLoadShipmentsBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CoLoadShipmentsBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CoLoadShipmentsBoundGrid.CopySelectedRowsAllowed = true;
			this.CoLoadShipmentsBoundGrid.GridId = "3b30463c-7e43-45c5-a80c-1b4af3b6543c";
			this.CoLoadShipmentsBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CoLoadShipmentsBoundGrid.IsWholeRowSelectedOnClick = true;
			this.CoLoadShipmentsBoundGrid.LayoutKey = "CoLoadShipmentsBoundGrid";
			this.CoLoadShipmentsBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 76, true);
			this.CoLoadShipmentsBoundGrid.Name = "CoLoadShipmentsBoundGrid";
			this.CoLoadShipmentsBoundGrid.ReadOnly = true;
			this.CoLoadShipmentsBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 142, true);
			this.CoLoadShipmentsBoundGrid.TabIndex = 3;
			// 
			// ShipmentServicesTabControl
			// 
			this.ShipmentServicesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentServicesTabControl.Controls.Add(this.ShipmentTabPage);
			this.ShipmentServicesTabControl.Controls.Add(this.DatesAndCartageTabPage);
			this.ShipmentServicesTabControl.Controls.Add(this.PickupTabPage);
			this.ShipmentServicesTabControl.Controls.Add(this.DeliveryTabPage);
			this.ShipmentServicesTabControl.Controls.Add(this.ServicesTabPage);
			this.ShipmentServicesTabControl.Controls.Add(this.NumbersTabPage);
			this.ShipmentServicesTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.ShipmentServicesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentServicesTabControl.Name = "ShipmentServicesTabControl";
			this.ShipmentServicesTabControl.SelectedIndex = 0;
			this.ShipmentServicesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 256, true);
			this.ShipmentServicesTabControl.TabIndex = 0;
			// 
			// ShipmentTabPage
			// 
			this.ShipmentTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|d46f2316-5aa7-4f74-a2a7-8af8463b370a", "Shipment");
			this.ShipmentTabPage.Controls.Add(this.WarehouseTextBox);
			this.ShipmentTabPage.Controls.Add(this.WarehouseDropEdit);
			this.ShipmentTabPage.Controls.Add(this.LocationTextBox);
			this.ShipmentTabPage.Controls.Add(this.JS_RS_NKServiceLevelFindbox);
			this.ShipmentTabPage.Controls.Add(this.DescriptionNotesButton);
			this.ShipmentTabPage.Controls.Add(this.TransportModeDropEdit);
			this.ShipmentTabPage.Controls.Add(this.ShipmentTotalPacksCalcDropEdit);
			this.ShipmentTabPage.Controls.Add(this.ShipmentTotalVolumeCalcDropEdit);
			this.ShipmentTabPage.Controls.Add(this.JS_TranshipToOtherCFSCheckBox);
			this.ShipmentTabPage.Controls.Add(this.JS_HouseBillTextBox);
			this.ShipmentTabPage.Controls.Add(this.CustomsEntryNumberTextBox);
			this.ShipmentTabPage.Controls.Add(this.JS_ConsolReferenceTextBox);
			this.ShipmentTabPage.Controls.Add(this.JS_GoodsDescriptionTextBox);
			this.ShipmentTabPage.Controls.Add(this.ShipmentTotalWeightCalcDropEdit);
			this.ShipmentTabPage.Controls.Add(this.JS_RL_NKDestinationCodeFindBox);
			this.ShipmentTabPage.Controls.Add(this.JS_RL_NKOriginCodeFindBox);
			this.ShipmentTabPage.Controls.Add(this.JS_A_RCVBoundTextEdit);
			this.ShipmentTabPage.Controls.Add(this.JS_InterimReceiptBoundTextEdit);
			this.ShipmentTabPage.Controls.Add(this.JS_CartageWaybillTextBox);
			this.ShipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipmentTabPage.Name = "ShipmentTabPage";
			this.ShipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.ShipmentTabPage.TabIndex = 0;
			// 
			// WarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseTextBox, "JS_WarehouseLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_WarehouseLocation)));
			this.WarehouseTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|f27c68b1-5310-47e1-8ddd-84cf7271a578", "Whs. Location", "The location within the Warehouse of the goods on this shipment.");
			this.WarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 204, true);
			this.WarehouseTextBox.Name = "WarehouseTextBox";
			this.WarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.WarehouseTextBox.TabIndex = 222;
			// 
			// WarehouseDropEdit
			// 
			this.WarehouseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseDropEdit, "LocationWhsGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).LocationWhsGuid)));
			this.WarehouseDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|c3da92cd-b67c-4a2a-a8df-1cc3a093f853", "Whs. Location", "Warehouse Loc.", "Warehouse Location", "");
			this.WarehouseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 204, true);
			this.WarehouseDropEdit.MaxItemsToShowInDropDown = 20;
			this.WarehouseDropEdit.Name = "WarehouseDropEdit";
			this.WarehouseDropEdit.PreBoundMaxLength = 15;
			this.WarehouseDropEdit.ShowDescriptionBox = false;
			this.WarehouseDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.WarehouseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.WarehouseDropEdit.TabIndex = 220;
			// 
			// LocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationTextBox, "LocationString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).LocationString)));
			this.LocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 204, true);
			this.LocationTextBox.Name = "LocationTextBox";
			this.LocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.LocationTextBox.TabIndex = 219;
			// 
			// JS_RS_NKServiceLevelFindbox
			// 
			this.JS_RS_NKServiceLevelFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RS_NKServiceLevelFindbox, "JS_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_RS_NKServiceLevel)));
			this.JS_RS_NKServiceLevelFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 182, true);
			this.JS_RS_NKServiceLevelFindbox.Name = "JS_RS_NKServiceLevelFindbox";
			this.JS_RS_NKServiceLevelFindbox.PreBoundMaxLength = 3;
			this.JS_RS_NKServiceLevelFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.JS_RS_NKServiceLevelFindbox.TabIndex = 33;
			// 
			// DescriptionNotesButton
			// 
			this.DescriptionNotesButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|94ed2ba6-73d5-44c5-881d-f8c68ac88bd3", "More..", "More...", "");
			this.DescriptionNotesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 136, true);
			this.DescriptionNotesButton.Name = "DescriptionNotesButton";
			this.DescriptionNotesButton.NoteType = "Detailed Goods Description";
			this.DescriptionNotesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.DescriptionNotesButton.TabIndex = 24;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "JS_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).JS_TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|71a884c7-4f5e-4dcf-864e-59d783dd574f", "Transport", "The Transport Mode for this shipment registration.");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 6, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TransportModeDropEdit.TabIndex = 1;
			// 
			// ShipmentTotalPacksCalcDropEdit
			// 
			this.ShipmentTotalPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSShipment)(null)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_F3_NKPackType)));
			this.ShipmentTotalPacksCalcDropEdit.BindToAmount = "JS_OuterPacks";
			this.ShipmentTotalPacksCalcDropEdit.BindToUnit = "JS_F3_NKPackType";
			this.ShipmentTotalPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|f1abfd4e-e43b-4cb6-9db8-146b187ab059", "Total Packs", "The total number of outer packages.");
			this.ShipmentTotalPacksCalcDropEdit.Decimals = 0;
			this.ShipmentTotalPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 50, true);
			this.ShipmentTotalPacksCalcDropEdit.Name = "ShipmentTotalPacksCalcDropEdit";
			this.ShipmentTotalPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentTotalPacksCalcDropEdit.TabIndex = 11;
			this.ShipmentTotalPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ShipmentTotalVolumeCalcDropEdit
			// 
			this.ShipmentTotalVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSShipment)(null)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_UnitOfVolume)));
			this.ShipmentTotalVolumeCalcDropEdit.BindToAmount = "JS_ActualVolume";
			this.ShipmentTotalVolumeCalcDropEdit.BindToUnit = "JS_UnitOfVolume";
			this.ShipmentTotalVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|a8905f5a-b8b7-4f8b-8d0d-1d5335d7ce66", "Total Volume", "The total volume of the shipment.");
			this.ShipmentTotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 94, true);
			this.ShipmentTotalVolumeCalcDropEdit.Name = "ShipmentTotalVolumeCalcDropEdit";
			this.ShipmentTotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentTotalVolumeCalcDropEdit.TabIndex = 19;
			this.ShipmentTotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JS_TranshipToOtherCFSCheckBox
			// 
			this.JS_TranshipToOtherCFSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JS_TranshipToOtherCFSCheckBox, "JS_TranshipToOtherCFS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CFSShipment)(null)).JS_TranshipToOtherCFS)));
			this.JS_TranshipToOtherCFSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JS_TranshipToOtherCFSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JS_TranshipToOtherCFSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 116, true);
			this.JS_TranshipToOtherCFSCheckBox.Name = "JS_TranshipToOtherCFSCheckBox";
			this.JS_TranshipToOtherCFSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JS_TranshipToOtherCFSCheckBox.TabIndex = 27;
			// 
			// JS_HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_HouseBillTextBox, "JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_HouseBill)));
			this.JS_HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 6, true);
			this.JS_HouseBillTextBox.Name = "JS_HouseBillTextBox";
			this.JS_HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.JS_HouseBillTextBox.TabIndex = 3;
			// 
			// CustomsEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTextBox, "CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).CustomsEntryNumber)));
			this.CustomsEntryNumberTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|6124dd88-7e6a-4e2a-a08b-4a79cc29baa7", "Entry No.", "Customs Entry Number", "");
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 160, true);
			this.CustomsEntryNumberTextBox.Name = "CustomsEntryNumberTextBox";
			this.CustomsEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.CustomsEntryNumberTextBox.TabIndex = 26;
			// 
			// JS_ConsolReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_ConsolReferenceTextBox, "JS_ConsolReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_ConsolReference)));
			this.JS_ConsolReferenceTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|7fa25560-c86d-4df0-b4e2-376288650ce5", "Client Ref.");
			this.JS_ConsolReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 50, true);
			this.JS_ConsolReferenceTextBox.Name = "JS_ConsolReferenceTextBox";
			this.JS_ConsolReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_ConsolReferenceTextBox.TabIndex = 9;
			// 
			// JS_GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_GoodsDescriptionTextBox, "JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_GoodsDescription)));
			this.JS_GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 138, true);
			this.JS_GoodsDescriptionTextBox.Name = "JS_GoodsDescriptionTextBox";
			this.JS_GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.JS_GoodsDescriptionTextBox.TabIndex = 23;
			// 
			// ShipmentTotalWeightCalcDropEdit
			// 
			this.ShipmentTotalWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSShipment)(null)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_UnitOfWeight)));
			this.ShipmentTotalWeightCalcDropEdit.BindToAmount = "JS_ActualWeight";
			this.ShipmentTotalWeightCalcDropEdit.BindToUnit = "JS_UnitOfWeight";
			this.ShipmentTotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|6ed00644-313c-4649-978e-6f01cd495e1e", "Total Weight", "The total weight of this shipment.");
			this.ShipmentTotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 72, true);
			this.ShipmentTotalWeightCalcDropEdit.Name = "ShipmentTotalWeightCalcDropEdit";
			this.ShipmentTotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentTotalWeightCalcDropEdit.TabIndex = 15;
			this.ShipmentTotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JS_RL_NKDestinationCodeFindBox
			// 
			this.JS_RL_NKDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKDestinationCodeFindBox, "JS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_RL_NKDestination)));
			this.JS_RL_NKDestinationCodeFindBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|e4ea5130-629f-49ba-876c-c7a1af65b4fc", "Destination", "The the port where the shipment is intended to go.");
			this.JS_RL_NKDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 28, true);
			this.JS_RL_NKDestinationCodeFindBox.Name = "JS_RL_NKDestinationCodeFindBox";
			this.JS_RL_NKDestinationCodeFindBox.ShowDescriptionBox = false;
			this.JS_RL_NKDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_RL_NKDestinationCodeFindBox.TabIndex = 7;
			// 
			// JS_RL_NKOriginCodeFindBox
			// 
			this.JS_RL_NKOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKOriginCodeFindBox, "JS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_RL_NKOrigin)));
			this.JS_RL_NKOriginCodeFindBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|1431093a-c548-49d1-bd43-df250ff07972", "Origin", "The port from which the shipment first departs.");
			this.JS_RL_NKOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 28, true);
			this.JS_RL_NKOriginCodeFindBox.Name = "JS_RL_NKOriginCodeFindBox";
			this.JS_RL_NKOriginCodeFindBox.ShowDescriptionBox = false;
			this.JS_RL_NKOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_RL_NKOriginCodeFindBox.TabIndex = 5;
			// 
			// JS_A_RCVBoundTextEdit
			// 
			this.JS_A_RCVBoundTextEdit.AllowDrop = true;
			this.JS_A_RCVBoundTextEdit.AutoCompleteMonthThreshold = 1;
			this.JS_A_RCVBoundTextEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_A_RCVBoundTextEdit, "JS_A_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).JS_A_RCV)));
			this.JS_A_RCVBoundTextEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|9728b1ef-1bf2-40d2-9391-f2748231dbeb", "Whs. Recd.", "Interim Receipt Date", "The date the goods are receipted.");
			this.JS_A_RCVBoundTextEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_A_RCVBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 94, true);
			this.JS_A_RCVBoundTextEdit.Name = "JS_A_RCVBoundTextEdit";
			this.JS_A_RCVBoundTextEdit.TabIndex = 17;
			// 
			// JS_InterimReceiptBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_InterimReceiptBoundTextEdit, "JS_InterimReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_InterimReceipt)));
			this.JS_InterimReceiptBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 72, true);
			this.JS_InterimReceiptBoundTextEdit.Name = "JS_InterimReceiptBoundTextEdit";
			this.JS_InterimReceiptBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_InterimReceiptBoundTextEdit.TabIndex = 13;
			// 
			// JS_CartageWaybillTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_CartageWaybillTextBox, "JS_CartageWaybill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSShipment)(null)).JS_CartageWaybill)));
			this.JS_CartageWaybillTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|cb092428-1595-413a-b182-c978cfd36167", "Con Note", "Port Transport Waybill.");
			this.JS_CartageWaybillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 116, true);
			this.JS_CartageWaybillTextBox.Name = "JS_CartageWaybillTextBox";
			this.JS_CartageWaybillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_CartageWaybillTextBox.TabIndex = 21;
			// 
			// DatesAndCartageTabPage
			// 
			this.DatesAndCartageTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|c70f03e2-9734-4819-b0f4-be623f3cce89", "Dates / Port Transport");
			this.DatesAndCartageTabPage.Controls.Add(this.DeliveryCartageCoGroupBox);
			this.DatesAndCartageTabPage.Controls.Add(this.LCLAvailableDateEdit);
			this.DatesAndCartageTabPage.Controls.Add(this.LCLStorageDateEdit);
			this.DatesAndCartageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DatesAndCartageTabPage.Name = "DatesAndCartageTabPage";
			this.DatesAndCartageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.DatesAndCartageTabPage.TabIndex = 4;
			// 
			// DeliveryCartageCoGroupBox
			// 
			this.DeliveryCartageCoGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|d561841f-1109-4422-926d-cdcedfeb039d", "Local Transport Provider");
			this.DeliveryCartageCoGroupBox.Controls.Add(this.zAddressControl1);
			this.DeliveryCartageCoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 65, true);
			this.DeliveryCartageCoGroupBox.Name = "DeliveryCartageCoGroupBox";
			this.DeliveryCartageCoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 67, true);
			this.DeliveryCartageCoGroupBox.TabIndex = 9;
			this.DeliveryCartageCoGroupBox.TabStop = false;
			// 
			// zAddressControl1
			// 
			this.zAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zAddressControl1, "JS_OA_CartageCoAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CFSShipment)(null)).JS_OA_CartageCoAddr)));
			this.zAddressControl1.BindToOrgList = "Lookups.LocalTransport_List";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zAddressControl1, false);
			this.zAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zAddressControl1.Name = "zAddressControl1";
			this.zAddressControl1.PopupCaption = "";
			this.zAddressControl1.ShowAddress = false;
			this.zAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 42, true);
			this.zAddressControl1.StackControls = true;
			this.zAddressControl1.TabIndex = 7;
			// 
			// LCLAvailableDateEdit
			// 
			this.LCLAvailableDateEdit.AllowDrop = true;
			this.LCLAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLAvailableDateEdit, "DocsAndCartage+JP_LCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).DocsAndCartage.JP_LCLAvailable)));
			this.LCLAvailableDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|e335687f-71e3-433a-8213-e64ac748ac0e", "CFS Available", "The actual date/time the goods have been delivered to the Consignee.");
			this.LCLAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			this.LCLAvailableDateEdit.Name = "LCLAvailableDateEdit";
			this.LCLAvailableDateEdit.TabIndex = 3;
			// 
			// LCLStorageDateEdit
			// 
			this.LCLStorageDateEdit.AllowDrop = true;
			this.LCLStorageDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLStorageDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLStorageDateEdit, "DocsAndCartage+JP_LCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSShipment)(null)).DocsAndCartage.JP_LCLStorageCommences)));
			this.LCLStorageDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|6d855d22-7abe-4ce7-9cf7-3c88bf7d3c9e", "CFS Storage", "Storage Start Date", "The actual date/time the goods have been delivered to the Consignee.");
			this.LCLStorageDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 8, true);
			this.LCLStorageDateEdit.Name = "LCLStorageDateEdit";
			this.LCLStorageDateEdit.TabIndex = 1;
			// 
			// PickupTabPage
			// 
			this.PickupTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|9fe33c1b-bf4d-42de-923a-aa64586fd5d9", "Pickup");
			this.PickupTabPage.Controls.Add(this.ConsignorPickupDocAddressControl);
			this.PickupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PickupTabPage.Name = "PickupTabPage";
			this.PickupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.PickupTabPage.TabIndex = 5;
			// 
			// ConsignorPickupDocAddressControl
			// 
			this.ConsignorPickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorPickupDocAddressControl, "ConsignorPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((CFSShipment)(null)).ConsignorPickupAddress)));
			this.ConsignorPickupDocAddressControl.BindToOrganisations = "Lookups.Consignor_List";
			this.ConsignorPickupDocAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|B4B039C2-05D0-463b-A7F8-9D6FBB260B12", "Pickup From");
			this.ConsignorPickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.ConsignorPickupDocAddressControl.Name = "ConsignorPickupDocAddressControl";
			this.ConsignorPickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorPickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsignorPickupDocAddressControl.TabIndex = 214;
			// 
			// DeliveryTabPage
			// 
			this.DeliveryTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|73050543-a15d-465f-b855-440fcca87753", "Delivery");
			this.DeliveryTabPage.Controls.Add(this.ConsigneePickupDocAddressControl);
			this.DeliveryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryTabPage.Name = "DeliveryTabPage";
			this.DeliveryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.DeliveryTabPage.TabIndex = 6;
			// 
			// ConsigneePickupDocAddressControl
			// 
			this.ConsigneePickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneePickupDocAddressControl, "ConsigneeDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((CFSShipment)(null)).ConsigneeDeliveryAddress)));
			this.ConsigneePickupDocAddressControl.BindToOrganisations = "Lookups.Consignee_List";
			this.ConsigneePickupDocAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|9F0411A7-43C0-4b87-82CE-B8FBD7163586", "Deliver To");
			this.ConsigneePickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.ConsigneePickupDocAddressControl.Name = "ConsigneePickupDocAddressControl";
			this.ConsigneePickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneePickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneePickupDocAddressControl.TabIndex = 251;
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|4882ef25-eb5e-4ffc-8b9b-a831ed0d40a1", "Services");
			this.ServicesTabPage.Controls.Add(this.ServicesPanel);
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.ServicesTabPage.TabIndex = 1;
			// 
			// ServicesPanel
			// 
			this.ServicesPanel.Controls.Add(this.ServicesControl);
			this.ServicesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesPanel.Name = "ServicesPanel";
			this.ServicesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.ServicesPanel.TabIndex = 0;
			// 
			// ServicesControl
			// 
			this.ServicesControl.AllowDrop = true;
			this.ServicesControl.BindToServices = "DocsAndCartage+Services";
			this.ServicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesControl.Name = "ServicesControl";
			this.ServicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.ServicesControl.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 256, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 3, true);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// PacksGroupBox
			// 
			this.PacksGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|876fc561-d8af-41d9-a21f-b32f27f1e511", "Packs");
			this.PacksGroupBox.Controls.Add(this.PackLineDetailsPanel);
			this.PacksGroupBox.Controls.Add(this.splitter2);
			this.PacksGroupBox.Controls.Add(this.PacklinesGrid);
			this.PacksGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 259, true);
			this.PacksGroupBox.Name = "PacksGroupBox";
			this.PacksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 229, true);
			this.PacksGroupBox.TabIndex = 1;
			this.PacksGroupBox.TabStop = false;
			// 
			// PackLineDetailsPanel
			// 
			this.PackLineDetailsPanel.Controls.Add(this.MinorDetailsTabControl);
			this.PackLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 123, true);
			this.PackLineDetailsPanel.Name = "PackLineDetailsPanel";
			this.PackLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 103, true);
			this.PackLineDetailsPanel.TabIndex = 3;
			// 
			// MinorDetailsTabControl
			// 
			this.MinorDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MinorDetailsTabControl.Controls.Add(this.LocationsTabPage);
			this.MinorDetailsTabControl.Controls.Add(this.MarksAndNumbersTabPage);
			this.MinorDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinorDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MinorDetailsTabControl.Name = "MinorDetailsTabControl";
			this.MinorDetailsTabControl.SelectedIndex = 0;
			this.MinorDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 103, true);
			this.MinorDetailsTabControl.TabIndex = 0;
			// 
			// LocationsTabPage
			// 
			this.LocationsTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|ca68a72f-1461-4dae-8319-fc08f707b333", "Locations");
			this.LocationsTabPage.Controls.Add(this.LocationsGrid);
			this.LocationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LocationsTabPage.Name = "LocationsTabPage";
			this.LocationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LocationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 76, true);
			this.LocationsTabPage.TabIndex = 0;
			this.LocationsTabPage.UseVisualStyleBackColor = true;
			// 
			// LocationsGrid
			// 
			this.LocationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocationsGrid, "OuterPackLines.PackLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).PackLocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLocation)(((System.Collections.IList)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).JQ_NoPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLocation)(((System.Collections.IList)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).JQ_WarehouseLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CFSPackLocation)(((System.Collections.IList)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).LocationWhsGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLocation)(((System.Collections.IList)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).LocationString)));
			this.LocationsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JQ_NoPackages";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "JQ_WarehouseLocation";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|046c726b-5dff-41f0-bd8d-8467a8e19ddf", "Warehouse");
			zGuidDropEditColumnStyleInfo1.ColumnName = "LocationWhsGuid";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|8504c8a0-9a17-4656-a691-aa3f3c4683ba", "Location");
			zTextBoxColumnStyleInfo6.ColumnName = "LocationString";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.LocationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LocationsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.LocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LocationsGrid.CopySelectedRowsAllowed = true;
			this.LocationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationsGrid.GridId = "619b5e13-43ed-4057-a748-aeb0173a871c";
			this.LocationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocationsGrid.LayoutKey = "JobPackLocGrid";
			this.LocationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LocationsGrid.Name = "LocationsGrid";
			this.LocationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 70, true);
			this.LocationsGrid.TabIndex = 0;
			// 
			// MarksAndNumbersTabPage
			// 
			this.MarksAndNumbersTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|82791789-84b3-4f30-90e1-bf4335a38d19", "Marks & Numbers");
			this.MarksAndNumbersTabPage.Controls.Add(this.MarksAndNumbersTextBox);
			this.MarksAndNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MarksAndNumbersTabPage.Name = "MarksAndNumbersTabPage";
			this.MarksAndNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MarksAndNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 76, true);
			this.MarksAndNumbersTabPage.TabIndex = 1;
			this.MarksAndNumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "OuterPackLines.JL_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 70, true);
			this.MarksAndNumbersTextBox.TabIndex = 0;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 3, true);
			this.splitter2.TabIndex = 1;
			this.splitter2.TabStop = false;
			// 
			// PacklinesGrid
			// 
			this.PacklinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PacklinesGrid, "OuterPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_UnitOfDimension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CFSPackLine)(((System.Collections.IList)(((CFSShipment)(null)).OuterPackLines)).SyncRoot)).JL_ContainerPackingOrder)));
			this.PacklinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.ColumnName = "JL_F3_NKPackType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo7.ColumnName = "JL_Description";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|70c4ba42-2c1f-4e12-928a-d585c6ba1cf4", "Weight");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|70c4ba42-2c1f-4e12-928a-d585c6ba1cf4", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|8cc8fba0-a8c5-41de-ba0e-956404e564c9", "Volume");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|8cc8fba0-a8c5-41de-ba0e-956404e564c9", "Volume");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|31db7446-e4f7-4035-bf45-500a3f00f12b", "Dims");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|31db7446-e4f7-4035-bf45-500a3f00f12b", "Dims");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo7.Decimals = 3;
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|31db7446-e4f7-4035-bf45-500a3f00f12b", "Dims");
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zDropEditColumnStyleInfo4.ColumnName = "JL_UnitOfDimension";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|31db7446-e4f7-4035-bf45-500a3f00f12b", "Dims");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JL_RH_NKCommodityCode";
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "JL_ContainerPackingOrder";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PacklinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PacklinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PacklinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PacklinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PacklinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PacklinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PacklinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.PacklinesGrid.CopySelectedRowsAllowed = true;
			this.PacklinesGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.PacklinesGrid.GridId = "5687bb0f-1b57-4d12-b4e2-ffc3999e7bd4";
			this.PacklinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PacklinesGrid.LayoutKey = "JobPackLinesGrid";
			this.PacklinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PacklinesGrid.Name = "PacklinesGrid";
			this.PacklinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 104, true);
			this.PacklinesGrid.TabIndex = 0;
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Controls.Add(this.AddtionalPanel);
			this.NumbersTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ShipmentDetails|76414f02-6e7f-44be-bdf9-cf60b8784dbe", "Reference Numbers");
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NumbersTabPage.Name = "NumbersTabPage";
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.NumbersTabPage.TabIndex = 7;
			// 
			// AddtionalPanel
			// 
			this.AddtionalPanel.Controls.Add(this.ReferenceNumbersControl);
			this.AddtionalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddtionalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddtionalPanel.Name = "AddtionalPanel";
			this.AddtionalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 229, true);
			this.AddtionalPanel.TabIndex = 0;
			// 
			// ReferenceNumbersControl
			// 
			this.ReferenceNumbersControl.AllowDrop = true;
			this.ReferenceNumbersControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ReferenceNumbersControl, "Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((CFSShipment)(null)).Numbers)));
			this.ReferenceNumbersControl.DisplayDetailPanel = true;
			this.ReferenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersControl.Name = "ReferenceNumbersControl";
			this.ReferenceNumbersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbersControl.TabIndex = 0;
			// 
			// ShipmentDetails
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PacksGroupBox);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.LeftPanel);
			this.Name = "ShipmentDetails";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 488, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LeftPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.TransportTabControl.ResumeLayout(false);
			this.LoadListTabPage.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LoadListConsolModuleButtonGrid.InnerGrid)).EndInit();
			this.SailingGroupBox.ResumeLayout(false);
			this.ShipmentSailingPanel.ResumeLayout(false);
			this.ShipmentSailingPanel.PerformLayout();
			this.LoadListSailingPanel.ResumeLayout(false);
			this.LoadListSailingPanel.PerformLayout();
			this.CoLoadTabPage.ResumeLayout(false);
			this.CoLoadTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CoLoadShipmentsBoundGrid)).EndInit();
			this.ShipmentServicesTabControl.ResumeLayout(false);
			this.ShipmentTabPage.ResumeLayout(false);
			this.ShipmentTabPage.PerformLayout();
			this.DatesAndCartageTabPage.ResumeLayout(false);
			this.DeliveryCartageCoGroupBox.ResumeLayout(false);
			this.PickupTabPage.ResumeLayout(false);
			this.DeliveryTabPage.ResumeLayout(false);
			this.NumbersTabPage.ResumeLayout(false);
			this.AddtionalPanel.ResumeLayout(false);
			this.ServicesTabPage.ResumeLayout(false);
			this.ServicesPanel.ResumeLayout(false);
			this.PacksGroupBox.ResumeLayout(false);
			this.PackLineDetailsPanel.ResumeLayout(false);
			this.MinorDetailsTabControl.ResumeLayout(false);
			this.LocationsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LocationsGrid)).EndInit();
			this.MarksAndNumbersTabPage.ResumeLayout(false);
			this.MarksAndNumbersTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PacklinesGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}