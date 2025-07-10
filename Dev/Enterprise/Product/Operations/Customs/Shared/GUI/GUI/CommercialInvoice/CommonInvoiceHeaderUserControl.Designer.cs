using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class CommonInvoiceHeaderUserControl
	{
		protected ZTabControl RightTabControl;
		protected ZTabPage CustomFieldsTabPage;
		protected ZTabPage OrdersTabPage;
		protected OrdersAttachUserControl ordersAttachUserControl;

		#region Windows Forms Designer Generated
		protected ZGroupBox ChargesGroupBox;
		protected Enterprise.MasterFiles.GUI.ZOrganisationControl ImporterOrganisationControl;
		protected Enterprise.MasterFiles.GUI.ZOrganisationControl SupplierOrganisationControl;
		protected internal Enterprise.ZArchitecture.ZGrid InvoiceChargesGrid;
		protected internal ZGroupBox ShipmentTypeGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit JE_MessageTypeDropDownEdit;
		protected InvoiceHeaderCustomFieldsUserControl InvCustomFieldsUserControl;
		protected ZGroupBox DetailsGroupBox;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImporterOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.SupplierOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ShipmentTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JE_MessageTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RightTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvCustomFieldsUserControl = new Enterprise.Customs.GUI.InvoiceHeaderCustomFieldsUserControl();
			this.OrdersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ordersAttachUserControl = new Enterprise.Freight.Forwarding.Orders.GUI.OrdersAttachUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D0779682-5637-4CD9-947D-B9ACE4C8E717", "Details");
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 5, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 360, true);
			this.DetailsGroupBox.TabIndex = 3;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|F55AD732-D3A6-438E-8C3B-5DFB98501332", "Charges");
			this.ChargesGroupBox.Controls.Add(this.InvoiceChargesGrid);
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 374, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 119, true);
			this.ChargesGroupBox.TabIndex = 5;
			this.ChargesGroupBox.TabStop = false;
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceChargesGrid, "Invoices.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_IsIncludedInITOT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).J7_PrepaidCollect)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Charges)).SyncRoot)).Lookups.PrepaidCollectList)));
			this.InvoiceChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ChargeTypeList";
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|B30470A9-0439-4340-BCA1-3751952E0AB3", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "J7_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|B3AC8A32-8331-4FF7-95E5-712557605A86", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|6f5c5191-7e64-4210-8599-ba32aeb4fa9d", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "J7_Amount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|6f5c5191-7e64-4210-8599-ba32aeb4fa9d", "Amount");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.Currencies";
			zCodeFindBoxColumnStyleInfo1.Caption = "";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|E1A989AD-3D35-4EE4-8EEA-0F5CC23B43A8", "Curr");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "J7_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|6f5c5191-7e64-4210-8599-ba32aeb4fa9d", "Amount");
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|79F749C6-FB54-439E-8A92-53C9AB82544D", "Dutiable");
			zCheckBoxColumnStyleInfo1.ColumnName = "J7_IsDutiable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zCheckBoxColumnStyleInfo2.Caption = "";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|E26849E9-AF0E-4756-96F2-D6FC26BCD07E", "GST Apply");
			zCheckBoxColumnStyleInfo2.ColumnName = "J7_IsGSTApplicable";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zCheckBoxColumnStyleInfo3.Caption = "";
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|4B22D479-1C4E-48EA-8476-76AB7A9CBD80", "Included in Lines");
			zCheckBoxColumnStyleInfo3.ColumnName = "J7_IsIncludedInITOT";
			zCheckBoxColumnStyleInfo3.ToolTip = Res.GetString("InvoiceHeaderUserControl|C321A0A4-2E61-4949-B126-0846D5D3CD0A", "If you tick this flag, it indicates that this charge is included in Lines. It won\'t reduce the ITOT you have to enter");
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.PrepaidCollectList";
			zDropEditColumnStyleInfo2.Caption = "";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|B1ADEDF9-EEAF-4595-9452-3353F91773C7", "PPD/CCX");
			zDropEditColumnStyleInfo2.ColumnName = "J7_PrepaidCollect";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			this.InvoiceChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.InvoiceChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InvoiceChargesGrid.GridId = "9f825c23-99ae-4543-81f8-c2b50637d131";
			this.InvoiceChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceChargesGrid.LayoutKey = "InvoiceChargesGrid";
			this.InvoiceChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.InvoiceChargesGrid.Name = "InvoiceChargesGrid";
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 104, true);
			this.InvoiceChargesGrid.TabIndex = 0;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "Invoices.JZ_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Buyer)));
			this.ImporterOrganisationControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BC5E7E64-4006-4A44-90D1-F7D505B38F46", "Importer");
			this.ImporterOrganisationControl.Captions = new string[] {
        "Importer"};
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 163, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControl.TabIndex = 1;
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierOrganisationControl, "Invoices.JZ_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Supplier)));
			this.SupplierOrganisationControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F140CBAF-2EBD-48BD-A526-D20CD8A8EBFF", "Supplier");
			this.SupplierOrganisationControl.Captions = new string[] {
        "Supplier"};
			this.SupplierOrganisationControl.IsCaptionOverridden = false;
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.SupplierOrganisationControl.Name = "SupplierOrganisationControl";
			this.SupplierOrganisationControl.PopupCaption = "";
			this.SupplierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.SupplierOrganisationControl.TabIndex = 0;
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7A5862F9-0796-4588-90FC-C80019E60154", "Shipment Type");
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_MessageTypeDropDownEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 321, true);
			this.ShipmentTypeGroupBox.Name = "ShipmentTypeGroupBox";
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 44, true);
			this.ShipmentTypeGroupBox.TabIndex = 2;
			this.ShipmentTypeGroupBox.TabStop = false;
			// 
			// JE_MessageTypeDropDownEdit
			// 
			this.JE_MessageTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_MessageTypeDropDownEdit, "Invoices.JZ_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_MessageType)));
			this.JE_MessageTypeDropDownEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|4d1f7320-ac68-47bc-882c-923d1aa18861", "Type");
			this.JE_MessageTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.JE_MessageTypeDropDownEdit.Name = "JE_MessageTypeDropDownEdit";
			this.JE_MessageTypeDropDownEdit.PreBoundMaxLength = 3;
			this.JE_MessageTypeDropDownEdit.ShouldResizeByMaxLength = true;
			this.JE_MessageTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 25, true);
			this.JE_MessageTypeDropDownEdit.TabIndex = 1;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.RightTabControl.Controls.Add(this.OrdersTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 10, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 450, true);
			this.RightTabControl.Multiline = true;
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.SelectedIndex = 0;
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 472, true);
			this.RightTabControl.TabIndex = 4;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|9ebffa14-f296-4aa3-b3fe-10a0b16cd5fd", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.InvCustomFieldsUserControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 442, true);
			this.CustomFieldsTabPage.TabIndex = 0;
			this.CustomFieldsTabPage.UseVisualStyleBackColor = true;
			// 
			// InvCustomFieldsUserControl
			// 
			this.BindingSource.SetBindingMember(this.InvCustomFieldsUserControl, "Invoices");
			this.InvCustomFieldsUserControl.AllowDrop = true;
			this.InvCustomFieldsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InvCustomFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvCustomFieldsUserControl.Name = "InvCustomFieldsUserControl";
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 418, true);
			this.InvCustomFieldsUserControl.TabIndex = 1;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|65ec1839-Cfe7-4b2c-9799-129ff0ebf8f4", "Orders");
			this.OrdersTabPage.Controls.Add(this.ordersAttachUserControl);
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.OrdersTabPage.Name = "OrdersTabPage";
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 442, true);
			this.OrdersTabPage.TabIndex = 1;
			// 
			// ordersAttachUserControl
			// 
			this.ordersAttachUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ordersAttachUserControl, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)))));
			this.ordersAttachUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ordersAttachUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ordersAttachUserControl.Name = "ordersAttachUserControl";
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 442, true);
			this.ordersAttachUserControl.TabIndex = 0;
			// 
			// CommonInvoiceHeaderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RightTabControl);
			this.Controls.Add(this.ShipmentTypeGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Controls.Add(this.SupplierOrganisationControl);
			this.Controls.Add(this.ChargesGroupBox);
			this.Name = "CommonInvoiceHeaderUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 503, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ordersAttachUserControl.ResumeLayout(true);
			this.ordersAttachUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.ComponentModel.IContainer components;
	}
}
