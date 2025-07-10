namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class NVOCCAdditionalDetailsControl
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
			this.servicesControl = new Enterprise.Freight.QuotedBookings.GUI.ForwardingBookingServicesControl();
			this.referenceNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			this.JP_OrderItemsAsStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrderItemsEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JS_A_RCVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_InterimReceiptBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BrokerageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CustomsEntryNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsEntryNumberTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntriesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntriesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryInvoiceLinesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MonetaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JP_InsuranceRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ValueOfInsuranceCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValueOfGoodsCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfGoodsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ClientOrgControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.JobHeaderClientCoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CreditorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ViaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CommodityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JS_A_BKDBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.additionalContactsControl = new Enterprise.Freight.QuotedBookings.GUI.ForwardingBookingAdditionalContactsControl();
			this.OnBoardDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReleaseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargesApplyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BookingPartyDocAddressControl = new Enterprise.Freight.QuotedBookings.GUI.SingleLineDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.servicesControl.SuspendLayout();
			this.referenceNumbersControl.SuspendLayout();
			this.JS_A_RCVBoundDateEdit.SuspendLayout();
			this.DeliveryDocAddressControl.SuspendLayout();
			this.PickupDocAddressControl.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.BrokerageDetailsGroupBox.SuspendLayout();
			this.EntryNumberPanel.SuspendLayout();
			this.CustomsEntryNumberTypeBoundDropEdit.SuspendLayout();
			this.EntriesPanel.SuspendLayout();
			this.MonetaryGroupBox.SuspendLayout();
			this.ValueOfInsuranceCurrencyFindbox.SuspendLayout();
			this.ValueOfGoodsCurrencyFindbox.SuspendLayout();
			this.ClientOrgControl.SuspendLayout();
			this.CarrierGuidFindBox.SuspendLayout();
			this.CreditorGuidFindBox.SuspendLayout();
			this.ViaCodeFindBox.SuspendLayout();
			this.CommodityFindBox.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.JS_A_BKDBoundReadOnlyDateEdit.SuspendLayout();
			this.additionalContactsControl.SuspendLayout();
			this.OnBoardDropEdit.SuspendLayout();
			this.ReleaseTypeDropEdit.SuspendLayout();
			this.ChargesApplyDropEdit.SuspendLayout();
			this.BookingPartyDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// servicesControl
			// 
			this.servicesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.servicesControl, ".");
			this.servicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.servicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 549, true);
			this.servicesControl.Name = "servicesControl";
			this.servicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 51, true);
			this.servicesControl.TabIndex = 23;
			this.servicesControl.TabStop = false;
			// 
			// referenceNumbersControl
			// 
			this.referenceNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.referenceNumbersControl, "Booking.Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.Numbers)));
			this.referenceNumbersControl.DisplayDetailPanel = false;
			this.referenceNumbersControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.referenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 549, true);
			this.referenceNumbersControl.Name = "referenceNumbersControl";
			this.referenceNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 51, true);
			this.referenceNumbersControl.TabIndex = 1;
			// 
			// JP_OrderItemsAsStringTextBox
			// 
			this.JP_OrderItemsAsStringTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JP_OrderItemsAsStringTextBox, "Booking+DocsAndCartage+JP_OrderItemsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.JP_OrderItemsAsString)));
			this.JP_OrderItemsAsStringTextBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|db8e784b-c014-45b2-b4bc-766540d1f516", "Order References", "The order references - if any - for this movement.");
			this.JP_OrderItemsAsStringTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JP_OrderItemsAsStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 53, true);
			this.JP_OrderItemsAsStringTextBox.Name = "JP_OrderItemsAsStringTextBox";
			this.JP_OrderItemsAsStringTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.JP_OrderItemsAsStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 13, true);
			this.JP_OrderItemsAsStringTextBox.TabIndex = 7;
			// 
			// OrderItemsEditButton
			// 
			this.OrderItemsEditButton.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|afae98b3-f31c-462e-84c6-70b15ab81844", "More...");
			this.OrderItemsEditButton.IsCaptionOverridden = false;
			this.OrderItemsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 52, true);
			this.OrderItemsEditButton.Name = "OrderItemsEditButton";
			this.OrderItemsEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OrderItemsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 22, true);
			this.OrderItemsEditButton.TabIndex = 8;
			this.OrderItemsEditButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OrderItemsEditButton.ToolTipCaption = null;
			this.OrderItemsEditButton.Click += new System.EventHandler(this.OrderItemsEditButton_Click);
			// 
			// JS_A_RCVBoundDateEdit
			// 
			this.JS_A_RCVBoundDateEdit.AllowDrop = true;
			this.JS_A_RCVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_A_RCVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_A_RCVBoundDateEdit, "Booking+JS_A_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_A_RCV)));
			this.JS_A_RCVBoundDateEdit.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|aa476879-d7f7-4822-b0c0-11d16b980502", "Warehouse Rec.");
			this.JS_A_RCVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_A_RCVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 9, true);
			this.JS_A_RCVBoundDateEdit.Name = "JS_A_RCVBoundDateEdit";
			this.JS_A_RCVBoundDateEdit.TabIndex = 3;
			// 
			// JS_InterimReceiptBoundTextEdit
			// 
			this.JS_InterimReceiptBoundTextEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JS_InterimReceiptBoundTextEdit, "Booking+JS_InterimReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_InterimReceipt)));
			this.JS_InterimReceiptBoundTextEdit.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|f7472f27-5eca-47ac-b661-019643675745", "Interim Receipt");
			this.JS_InterimReceiptBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 31, true);
			this.JS_InterimReceiptBoundTextEdit.Name = "JS_InterimReceiptBoundTextEdit";
			this.JS_InterimReceiptBoundTextEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JS_InterimReceiptBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 13, true);
			this.JS_InterimReceiptBoundTextEdit.TabIndex = 5;
			// 
			// DeliveryDocAddressControl
			// 
			this.DeliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "Booking+ConsigneeDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ConsigneeDeliveryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "Organisations";
			this.DeliveryDocAddressControl.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|D36239EC-1A3E-40ed-B41A-B9A107808EBD", "Delivery Address");
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 284, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.ReadOnly = false;
			this.DeliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 18;
			this.DeliveryDocAddressControl.ValidationJustForced = false;
			// 
			// PickupDocAddressControl
			// 
			this.PickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupDocAddressControl, "Booking+ConsignorPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ConsignorPickupAddress)));
			this.PickupDocAddressControl.BindToOrganisations = "Organisations";
			this.PickupDocAddressControl.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|71a08ddd-5f27-4abc-a225-1deb1a189517", "Pickup Address");
			this.PickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 264, true);
			this.PickupDocAddressControl.Name = "PickupDocAddressControl";
			this.PickupDocAddressControl.ReadOnly = false;
			this.PickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.PickupDocAddressControl.TabIndex = 17;
			this.PickupDocAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Consignee_List";
			this.ConsigneeDocAddressControl.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|2BD8D65E-684B-4e5a-81D9-07496D60B502", "Consignee");
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 75, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 1;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// BrokerageDetailsGroupBox
			// 
			this.BrokerageDetailsGroupBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|5e27a938-050e-46e0-b205-91deb6b1d702", "Brokerage Details");
			this.BrokerageDetailsGroupBox.Controls.Add(this.EntryNumberPanel);
			this.BrokerageDetailsGroupBox.Controls.Add(this.EntriesPanel);
			this.BrokerageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 123, true);
			this.BrokerageDetailsGroupBox.Name = "BrokerageDetailsGroupBox";
			this.BrokerageDetailsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 1, 3, 1, true);
			this.BrokerageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 84, true);
			this.BrokerageDetailsGroupBox.TabIndex = 28;
			this.BrokerageDetailsGroupBox.TabStop = false;
			// 
			// EntryNumberPanel
			// 
			this.EntryNumberPanel.Controls.Add(this.CustomsEntryNumberBoundTextBox);
			this.EntryNumberPanel.Controls.Add(this.CustomsEntryNumberTypeBoundDropEdit);
			this.EntryNumberPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntryNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
			this.EntryNumberPanel.Name = "EntryNumberPanel";
			this.EntryNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 25, true);
			this.EntryNumberPanel.TabIndex = 1;
			// 
			// CustomsEntryNumberBoundTextBox
			// 
			this.CustomsEntryNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberBoundTextBox, "Booking+CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.CustomsEntryNumber)));
			this.CustomsEntryNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 0, true);
			this.CustomsEntryNumberBoundTextBox.Name = "CustomsEntryNumberBoundTextBox";
			this.CustomsEntryNumberBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomsEntryNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 13, true);
			this.CustomsEntryNumberBoundTextBox.TabIndex = 1;
			// 
			// CustomsEntryNumberTypeBoundDropEdit
			// 
			this.CustomsEntryNumberTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTypeBoundDropEdit, "Booking+CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.CustomsEntryNumberType)));
			this.CustomsEntryNumberTypeBoundDropEdit.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|d727453c-8f44-4503-ac96-0f6132d913e7", "Entry", "Entry Number Type", "Customs Entry Number Type", "The Customs Entry Type and Number for this shipment.");
			this.CustomsEntryNumberTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 0, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Name = "CustomsEntryNumberTypeBoundDropEdit";
			this.CustomsEntryNumberTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.CustomsEntryNumberTypeBoundDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsEntryNumberTypeBoundDropEdit.ShowDescriptionBox = false;
			this.CustomsEntryNumberTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.CustomsEntryNumberTypeBoundDropEdit.TabIndex = 0;
			// 
			// EntriesPanel
			// 
			this.EntriesPanel.Controls.Add(this.EntriesCalcEdit);
			this.EntriesPanel.Controls.Add(this.EntryInvoiceLinesCalcEdit);
			this.EntriesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntriesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.EntriesPanel.Name = "EntriesPanel";
			this.EntriesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 44, true);
			this.EntriesPanel.TabIndex = 0;
			// 
			// EntriesCalcEdit
			// 
			this.EntriesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EntriesCalcEdit, "Quote+CurrentOneOffQuote+TT_NumberOfEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_NumberOfEntries)));
			this.EntriesCalcEdit.DecimalPlaces = 0;
			this.EntriesCalcEdit.Decimals = 0;
			this.EntriesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 0, true);
			this.EntriesCalcEdit.Name = "EntriesCalcEdit";
			this.EntriesCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.EntriesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.EntriesCalcEdit.TabIndex = 1;
			this.EntriesCalcEdit.Text = "0";
			this.EntriesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EntryInvoiceLinesCalcEdit
			// 
			this.EntryInvoiceLinesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EntryInvoiceLinesCalcEdit, "Quote+CurrentOneOffQuote+TT_NumberOfEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_NumberOfEntryLines)));
			this.EntryInvoiceLinesCalcEdit.CaptionResourceString = null;
			this.EntryInvoiceLinesCalcEdit.DecimalPlaces = 0;
			this.EntryInvoiceLinesCalcEdit.Decimals = 0;
			this.EntryInvoiceLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 22, true);
			this.EntryInvoiceLinesCalcEdit.Name = "EntryInvoiceLinesCalcEdit";
			this.EntryInvoiceLinesCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.EntryInvoiceLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.EntryInvoiceLinesCalcEdit.TabIndex = 3;
			this.EntryInvoiceLinesCalcEdit.Text = "0";
			this.EntryInvoiceLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MonetaryGroupBox
			// 
			this.MonetaryGroupBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|9e61ebd8-8301-41bc-be80-e56393034582", "Monetary Values");
			this.MonetaryGroupBox.Controls.Add(this.JP_InsuranceRequiredCheckBox);
			this.MonetaryGroupBox.Controls.Add(this.ValueOfInsuranceCurrencyFindbox);
			this.MonetaryGroupBox.Controls.Add(this.ValueOfInsuranceCalcEdit);
			this.MonetaryGroupBox.Controls.Add(this.ValueOfGoodsCurrencyFindbox);
			this.MonetaryGroupBox.Controls.Add(this.ValueOfGoodsCalcEdit);
			this.MonetaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 168, true);
			this.MonetaryGroupBox.Name = "MonetaryGroupBox";
			this.MonetaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 87, true);
			this.MonetaryGroupBox.TabIndex = 15;
			this.MonetaryGroupBox.TabStop = false;
			// 
			// JP_InsuranceRequiredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JP_InsuranceRequiredCheckBox, "Booking+DocsAndCartage+JP_InsuranceRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.JP_InsuranceRequired)));
			this.JP_InsuranceRequiredCheckBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|0a3a74a5-c387-4f0b-bb11-99c99e069b8e", "Insurance Required");
			this.JP_InsuranceRequiredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JP_InsuranceRequiredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JP_InsuranceRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 59, true);
			this.JP_InsuranceRequiredCheckBox.Name = "JP_InsuranceRequiredCheckBox";
			this.JP_InsuranceRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JP_InsuranceRequiredCheckBox.TabIndex = 6;
			// 
			// ValueOfInsuranceCurrencyFindbox
			// 
			this.ValueOfInsuranceCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCurrencyFindbox, "InsuranceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceCurrency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValueOfInsuranceCurrencyFindbox, false);
			this.ValueOfInsuranceCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 37, true);
			this.ValueOfInsuranceCurrencyFindbox.Name = "ValueOfInsuranceCurrencyFindbox";
			this.ValueOfInsuranceCurrencyFindbox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ValueOfInsuranceCurrencyFindbox.ParentType = null;
			this.ValueOfInsuranceCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfInsuranceCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfInsuranceCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.ValueOfInsuranceCurrencyFindbox.TabIndex = 5;
			// 
			// ValueOfInsuranceCalcEdit
			// 
			this.ValueOfInsuranceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCalcEdit, "InsuranceValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceValue)));
			this.ValueOfInsuranceCalcEdit.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|6e8fb115-69ce-4772-a1bd-fab9f42fd2ed", "Insurance Value", "The value of goods for insurance purposes.");
			this.ValueOfInsuranceCalcEdit.DecimalPlaces = 2;
			this.ValueOfInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 37, true);
			this.ValueOfInsuranceCalcEdit.Name = "ValueOfInsuranceCalcEdit";
			this.ValueOfInsuranceCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ValueOfInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.ValueOfInsuranceCalcEdit.TabIndex = 4;
			this.ValueOfInsuranceCalcEdit.Text = "0.00";
			this.ValueOfInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ValueOfGoodsCurrencyFindbox
			// 
			this.ValueOfGoodsCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCurrencyFindbox, "GoodsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsCurrency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValueOfGoodsCurrencyFindbox, false);
			this.ValueOfGoodsCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 15, true);
			this.ValueOfGoodsCurrencyFindbox.Name = "ValueOfGoodsCurrencyFindbox";
			this.ValueOfGoodsCurrencyFindbox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ValueOfGoodsCurrencyFindbox.ParentType = null;
			this.ValueOfGoodsCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfGoodsCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfGoodsCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.ValueOfGoodsCurrencyFindbox.TabIndex = 2;
			// 
			// ValueOfGoodsCalcEdit
			// 
			this.ValueOfGoodsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCalcEdit, "GoodsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsValue)));
			this.ValueOfGoodsCalcEdit.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|3c398173-15e8-46c2-92af-c9e28d2a602c", "Goods Value");
			this.ValueOfGoodsCalcEdit.DecimalPlaces = 2;
			this.ValueOfGoodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.ValueOfGoodsCalcEdit.Name = "ValueOfGoodsCalcEdit";
			this.ValueOfGoodsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ValueOfGoodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.ValueOfGoodsCalcEdit.TabIndex = 1;
			this.ValueOfGoodsCalcEdit.Text = "0.00";
			this.ValueOfGoodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClientOrgControl
			// 
			this.ClientOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientOrgControl, "ClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ClientPK)));
			this.ClientOrgControl.BindToOrganisations = "Clients";
			this.ClientOrgControl.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|1E040726-A4D0-4d55-962F-BBE3E53DA8BC", "Client");
			this.ClientOrgControl.Captions = new string[0];
			this.ClientOrgControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.ClientOrgControl.IsCaptionOverridden = false;
			this.ClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClientOrgControl.Name = "ClientOrgControl";
			this.ClientOrgControl.PopupCaption = "";
			this.ClientOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 51, true);
			this.ClientOrgControl.TabIndex = 0;
			// 
			// JobHeaderClientCoveringLabel
			//
			this.JobHeaderClientCoveringLabel.CaptionResourceString = Res.GetData("ClientControl|788b77c9-4b70-476c-9a54-86b8aee108aa", "Job Header Lock Error Label");
			this.JobHeaderClientCoveringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.JobHeaderClientCoveringLabel.ForeColor = System.Drawing.Color.Red;
			this.JobHeaderClientCoveringLabel.IsFontBold = true;
			this.JobHeaderClientCoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobHeaderClientCoveringLabel.Name = "JobHeaderClientCoveringLabel";
			this.JobHeaderClientCoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 52, true);
			this.JobHeaderClientCoveringLabel.TabIndex = 7;
			this.JobHeaderClientCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.JobHeaderClientCoveringLabel.Visible = false;
			// 
			// CarrierGuidFindBox
			// 
			this.CarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OH_Carrier)));
			this.CarrierGuidFindBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|c6a3aa6e-d059-4846-9d67-25fe1e9cd053", "Carrier", "The Carrier/Shipping Line for this movement.");
			this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 97, true);
			this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
			this.CarrierGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CarrierGuidFindBox.ParentType = null;
			this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 13, true);
			this.CarrierGuidFindBox.TabIndex = 12;
			// 
			// CreditorGuidFindBox
			// 
			this.CarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorGuidFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OH_Carrier)));
			this.CreditorGuidFindBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|c6a3aa6e-d059-4846-9d67-25fe1e9cd054", "Creditor", "The Creditor for this movement."); //prolly change this
			this.CreditorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 117, true);
			this.CreditorGuidFindBox.Name = "CreditorGuidFindBox";
			this.CreditorGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorGuidFindBox.ParentType = null;
			this.CreditorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 13, true);
			this.CreditorGuidFindBox.TabIndex = 13;
			// 
			// ViaCodeFindBox
			// 
			this.ViaCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ViaCodeFindBox, "Via");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Via)));
			this.ViaCodeFindBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|6b00aa61-38e9-4442-b746-479840293b1e", "Via", "The Transhipment/Via port.");
			this.ViaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 75, true);
			this.ViaCodeFindBox.Name = "ViaCodeFindBox";
			this.ViaCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ViaCodeFindBox.ParentType = null;
			this.ViaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 13, true);
			this.ViaCodeFindBox.TabIndex = 10;
			// 
			// CommodityFindBox
			// 
			this.CommodityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityFindBox, "Commodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Commodity)));
			this.CommodityFindBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|c13d26b1-8af8-419e-a75a-42ff5b3aa43c", "Commodity", "Commodity Code", "The Commodity Code for this movement.");
			this.CommodityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 139, true);
			this.CommodityFindBox.Name = "CommodityFindBox";
			this.CommodityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CommodityFindBox.ParentType = null;
			this.CommodityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 13, true);
			this.CommodityFindBox.TabIndex = 14;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.JS_A_BKDBoundReadOnlyDateEdit);
			this.zPanel1.Controls.Add(this.BrokerageDetailsGroupBox);
			this.zPanel1.Controls.Add(this.additionalContactsControl);
			this.zPanel1.Controls.Add(this.OnBoardDropEdit);
			this.zPanel1.Controls.Add(this.ReleaseTypeDropEdit);
			this.zPanel1.Controls.Add(this.ChargesApplyDropEdit);
			this.zPanel1.Controls.Add(this.BookingPartyDocAddressControl);
			this.zPanel1.Controls.Add(this.JobHeaderClientCoveringLabel);
			this.zPanel1.Controls.Add(this.ClientOrgControl);
			this.zPanel1.Controls.Add(this.PickupDocAddressControl);
			this.zPanel1.Controls.Add(this.CommodityFindBox);
			this.zPanel1.Controls.Add(this.DeliveryDocAddressControl);
			this.zPanel1.Controls.Add(this.CarrierGuidFindBox);
			this.zPanel1.Controls.Add(this.CreditorGuidFindBox);
			this.zPanel1.Controls.Add(this.ViaCodeFindBox);
			this.zPanel1.Controls.Add(this.JS_InterimReceiptBoundTextEdit);
			this.zPanel1.Controls.Add(this.JS_A_RCVBoundDateEdit);
			this.zPanel1.Controls.Add(this.OrderItemsEditButton);
			this.zPanel1.Controls.Add(this.MonetaryGroupBox);
			this.zPanel1.Controls.Add(this.JP_OrderItemsAsStringTextBox);
			this.zPanel1.Controls.Add(this.ConsigneeDocAddressControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 0, true);
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 592, true);
			this.zPanel1.TabIndex = 24;
			// 
			// JS_A_BKDBoundReadOnlyDateEdit
			// 
			this.JS_A_BKDBoundReadOnlyDateEdit.AllowDrop = true;
			this.JS_A_BKDBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_A_BKDBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_A_BKDBoundReadOnlyDateEdit, "Booking+JS_A_BKD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_A_BKD)));
			this.JS_A_BKDBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 32, true);
			this.JS_A_BKDBoundReadOnlyDateEdit.Name = "JS_A_BKDBoundReadOnlyDateEdit";
			this.JS_A_BKDBoundReadOnlyDateEdit.TabIndex = 30;
			this.JS_A_BKDBoundReadOnlyDateEdit.TabStop = false;
			// 
			// additionalContactsControl
			// 
			this.additionalContactsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalContactsControl, ".");
			this.additionalContactsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 209, true);
			this.additionalContactsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.additionalContactsControl.Name = "additionalContactsControl";
			this.additionalContactsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 383, true);
			this.additionalContactsControl.TabIndex = 31;
			// 
			// OnBoardDropEdit
			// 
			this.OnBoardDropEdit.AllowDrop = true;
			this.OnBoardDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OnBoardDropEdit, "Booking+JS_ShippedOnBoard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ShippedOnBoard)));
			this.OnBoardDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 98, true);
			this.OnBoardDropEdit.Name = "OnBoardDropEdit";
			this.OnBoardDropEdit.PreBoundMaxLength = 3;
			this.OnBoardDropEdit.ShouldResizeByMaxLength = true;
			this.OnBoardDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 13, true);
			this.OnBoardDropEdit.TabIndex = 27;
			// 
			// ReleaseTypeDropEdit
			// 
			this.ReleaseTypeDropEdit.AllowDrop = true;
			this.ReleaseTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReleaseTypeDropEdit, "Booking+JS_ReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ReleaseType)));
			this.ReleaseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 54, true);
			this.ReleaseTypeDropEdit.Name = "ReleaseTypeDropEdit";
			this.ReleaseTypeDropEdit.PreBoundMaxLength = 3;
			this.ReleaseTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ReleaseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 13, true);
			this.ReleaseTypeDropEdit.TabIndex = 25;
			// 
			// ChargesApplyDropEdit
			// 
			this.ChargesApplyDropEdit.AllowDrop = true;
			this.ChargesApplyDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChargesApplyDropEdit, "HBLAWBChargesDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).HBLAWBChargesDisplay)));
			this.ChargesApplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 76, true);
			this.ChargesApplyDropEdit.Name = "ChargesApplyDropEdit";
			this.ChargesApplyDropEdit.PreBoundMaxLength = 3;
			this.ChargesApplyDropEdit.ShouldResizeByMaxLength = true;
			this.ChargesApplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 13, true);
			this.ChargesApplyDropEdit.TabIndex = 26;
			// 
			// BookingPartyDocAddressControl
			// 
			this.BookingPartyDocAddressControl.AllowDrop = true;
			this.BookingPartyDocAddressControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BookingPartyDocAddressControl, "Booking+BookingPartyDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.BookingPartyDocumentaryAddress)));
			this.BookingPartyDocAddressControl.BindToOrganisations = "Organisations";
			this.BookingPartyDocAddressControl.CaptionResourceString = Res.GetData("24edb719-042c-4678-a0c3-39403963f1bf", "Bkg. Party", "Bkg. Party", "Booking Party", "");
			this.BookingPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BookingPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 9, true);
			this.BookingPartyDocAddressControl.Name = "BookingPartyDocAddressControl";
			this.BookingPartyDocAddressControl.ReadOnly = false;
			this.BookingPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.BookingPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.BookingPartyDocAddressControl.TabIndex = 24;
			this.BookingPartyDocAddressControl.ValidationJustForced = false;
			// 
			// NVOCCAdditionalDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.servicesControl);
			this.Controls.Add(this.referenceNumbersControl);
			this.Controls.Add(this.zPanel1);
			this.Name = "NVOCCAdditionalDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.servicesControl.ResumeLayout(true);
			this.servicesControl.PerformLayout();
			this.referenceNumbersControl.ResumeLayout(true);
			this.referenceNumbersControl.PerformLayout();
			this.JS_A_RCVBoundDateEdit.ResumeLayout(true);
			this.JS_A_RCVBoundDateEdit.PerformLayout();
			this.DeliveryDocAddressControl.ResumeLayout(true);
			this.DeliveryDocAddressControl.PerformLayout();
			this.PickupDocAddressControl.ResumeLayout(true);
			this.PickupDocAddressControl.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.BrokerageDetailsGroupBox.ResumeLayout(false);
			this.BrokerageDetailsGroupBox.PerformLayout();
			this.EntryNumberPanel.ResumeLayout(false);
			this.EntryNumberPanel.PerformLayout();
			this.CustomsEntryNumberTypeBoundDropEdit.ResumeLayout(true);
			this.CustomsEntryNumberTypeBoundDropEdit.PerformLayout();
			this.EntriesPanel.ResumeLayout(false);
			this.EntriesPanel.PerformLayout();
			this.MonetaryGroupBox.ResumeLayout(false);
			this.MonetaryGroupBox.PerformLayout();
			this.ValueOfInsuranceCurrencyFindbox.ResumeLayout(true);
			this.ValueOfInsuranceCurrencyFindbox.PerformLayout();
			this.ValueOfGoodsCurrencyFindbox.ResumeLayout(true);
			this.ValueOfGoodsCurrencyFindbox.PerformLayout();
			this.ClientOrgControl.ResumeLayout(true);
			this.ClientOrgControl.PerformLayout();
			this.CarrierGuidFindBox.ResumeLayout(true);
			this.CarrierGuidFindBox.PerformLayout();
			this.CreditorGuidFindBox.ResumeLayout(true);
			this.CreditorGuidFindBox.PerformLayout();
			this.ViaCodeFindBox.ResumeLayout(true);
			this.ViaCodeFindBox.PerformLayout();
			this.CommodityFindBox.ResumeLayout(true);
			this.CommodityFindBox.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.JS_A_BKDBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_A_BKDBoundReadOnlyDateEdit.PerformLayout();
			this.additionalContactsControl.ResumeLayout(true);
			this.additionalContactsControl.PerformLayout();
			this.OnBoardDropEdit.ResumeLayout(true);
			this.OnBoardDropEdit.PerformLayout();
			this.ReleaseTypeDropEdit.ResumeLayout(true);
			this.ReleaseTypeDropEdit.PerformLayout();
			this.ChargesApplyDropEdit.ResumeLayout(true);
			this.ChargesApplyDropEdit.PerformLayout();
			this.BookingPartyDocAddressControl.ResumeLayout(true);
			this.BookingPartyDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ForwardingBookingServicesControl servicesControl;
		private Enterprise.ZArchitecture.ZTextBox JP_OrderItemsAsStringTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton OrderItemsEditButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_A_RCVBoundDateEdit;
		private Enterprise.ZArchitecture.ZTextBox JS_InterimReceiptBoundTextEdit;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl PickupDocAddressControl;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BrokerageDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel EntryNumberPanel;
		private Enterprise.ZArchitecture.ZTextBox CustomsEntryNumberBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CustomsEntryNumberTypeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel EntriesPanel;
		private Enterprise.ZArchitecture.ZCalcEdit EntriesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit EntryInvoiceLinesCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MonetaryGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JP_InsuranceRequiredCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ValueOfInsuranceCurrencyFindbox;
		private Enterprise.ZArchitecture.ZCalcEdit ValueOfInsuranceCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ValueOfGoodsCurrencyFindbox;
		private Enterprise.ZArchitecture.ZCalcEdit ValueOfGoodsCalcEdit;
		internal Enterprise.MasterFiles.GUI.ZOrganisationControl ClientOrgControl;
		internal Enterprise.ZArchitecture.ZLabel JobHeaderClientCoveringLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CreditorGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ViaCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CommodityFindBox;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersControl;
		private SingleLineDocAddressControl BookingPartyDocAddressControl;
		private ZArchitecture.GUI.ZDropEdit OnBoardDropEdit;
		private ZArchitecture.GUI.ZDropEdit ReleaseTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ChargesApplyDropEdit;
		private ForwardingBookingAdditionalContactsControl additionalContactsControl;
		private ZArchitecture.GUI.ZDateEdit JS_A_BKDBoundReadOnlyDateEdit;
	}
}
