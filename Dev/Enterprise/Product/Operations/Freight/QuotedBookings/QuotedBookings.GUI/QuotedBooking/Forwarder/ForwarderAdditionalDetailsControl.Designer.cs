using CargoWise.Windows.UI;
using SpotRateType = Enterprise.Freight.Business.FreightConstants.SpotRateType;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ForwarderAdditionalDetailsControl
	{
		private void InitializeComponent()
		{
			this.ServicesAndReferenceNumberDynamicControlSplitContainer = new KSplitContainer();
			this.AdditionalContacts = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.servicesControl = new Enterprise.Freight.QuotedBookings.GUI.ForwardingBookingServicesControl();
			this.referenceNumbersGroupBoxControl = new Enterprise.Freight.QuotedBookings.GUI.ForwardingReferenceNumbersControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.scheduleChooserControl = new Enterprise.Freight.QuotedBookings.GUI.ScheduleChooserControl();
			this.RightTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AdditionalDetails1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RightTopRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.BookingPartyDocAddressControl = new Enterprise.Freight.QuotedBookings.GUI.SingleLineDocAddressControl();
			this.JS_A_BKDBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_ClientRequestedETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_A_RCVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CFSReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_InterimReceiptBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.RightBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MiddleBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FreightGatewaySellRate = new Enterprise.Freight.GUI.FreightRateControl();
			this.FreightCostRate = new Enterprise.Freight.GUI.FreightRateControl();
			this.MiddlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LeftBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.MiddleTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BottomFillPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalContacts.SuspendLayout();
			this.servicesControl.SuspendLayout();
			this.referenceNumbersGroupBoxControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.scheduleChooserControl.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.AdditionalDetails1.SuspendLayout();
			this.RightTopRowPanel.SuspendLayout();
			this.BookingPartyDocAddressControl.SuspendLayout();
			this.JS_A_BKDBoundReadOnlyDateEdit.SuspendLayout();
			this.JS_ClientRequestedETADateEdit.SuspendLayout();
			this.JS_A_RCVBoundDateEdit.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.MiddleBottomPanel.SuspendLayout();
			this.FreightGatewaySellRate.SuspendLayout();
			this.FreightCostRate.SuspendLayout();
			this.MiddlePanel.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.PickupDocAddressControl.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.DeliveryDocAddressControl.SuspendLayout();
			this.BottomFillPanel.SuspendLayout();
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel1.SuspendLayout();
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel2.SuspendLayout();
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// AdditionalContacts
			// 
			this.AdditionalContacts.AllowDrop = true;
			this.AdditionalContacts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalContacts.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalContacts.Name = "AdditionalContacts";
			this.AdditionalContacts.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 353, true);
			this.AdditionalContacts.TabIndex = 18;
			this.AdditionalContacts.UserControlType = typeof(Enterprise.Freight.QuotedBookings.GUI.ForwardingBookingAdditionalContactsControl);
			// 
			// servicesControl
			// 
			this.servicesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.servicesControl, ".");
			this.servicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.servicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.servicesControl.Name = "servicesControl";
			this.servicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 100, true);
			this.servicesControl.TabIndex = 21;
			this.servicesControl.TabStop = false;
			// 
			// referenceNumbersGroupBoxControl
			// 
			this.referenceNumbersGroupBoxControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.referenceNumbersGroupBoxControl, ".");
			this.referenceNumbersGroupBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.referenceNumbersGroupBoxControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.referenceNumbersGroupBoxControl.Name = "referenceNumbersGroupBoxControl";
			this.referenceNumbersGroupBoxControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 100, true);
			this.referenceNumbersGroupBoxControl.TabIndex = 21;
			this.referenceNumbersGroupBoxControl.TabStop = false;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Controls.Add(this.RightBottomPanel);
			this.MainPanel.Controls.Add(this.MiddleBottomPanel);
			this.MainPanel.Controls.Add(this.MiddlePanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 593, true);
			this.MainPanel.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.scheduleChooserControl);
			this.TopPanel.Controls.Add(this.RightTopPanel);
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1110, 225, true);
			this.TopPanel.TabIndex = 1;
			// 
			// scheduleChooserControl
			// 
			this.scheduleChooserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.scheduleChooserControl, ".");
			this.scheduleChooserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 7, true);
			this.scheduleChooserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 177, true);
			this.scheduleChooserControl.Name = "scheduleChooserControl";
			this.scheduleChooserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 212, true);
			this.scheduleChooserControl.TabIndex = 1;
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Controls.Add(this.AdditionalDetails1);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopPanel, true);
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 205, true);
			this.RightTopPanel.TabIndex = 2;
			// 
			// AdditionalDetails1
			// 
			this.AdditionalDetails1.Controls.Add(this.RightTopRowPanel);
			this.AdditionalDetails1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.AdditionalDetails1.Name = "AdditionalDetails1";
			this.AdditionalDetails1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 203, true);
			this.AdditionalDetails1.TabIndex = 23;
			this.AdditionalDetails1.TabStop = false;
			// 
			// RightTopRowPanel
			// 
			this.RightTopRowPanel.Controls.Add(this.BookingPartyDocAddressControl);
			this.RightTopRowPanel.Controls.Add(this.JS_A_BKDBoundReadOnlyDateEdit);
			this.RightTopRowPanel.Controls.Add(this.JS_ClientRequestedETADateEdit);
			this.RightTopRowPanel.Controls.Add(this.JS_A_RCVBoundDateEdit);
			this.RightTopRowPanel.Controls.Add(this.CFSReferenceTextBox);
			this.RightTopRowPanel.Controls.Add(this.JS_InterimReceiptBoundTextEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopRowPanel, true);
			this.RightTopRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 19, true);
			this.RightTopRowPanel.Name = "RightTopRowPanel";
			this.RightTopRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(27);
			this.RightTopRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 160, true);
			this.RightTopRowPanel.TabIndex = 22;
			// 
			// BookingPartyDocAddressControl
			// 
			this.BookingPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookingPartyDocAddressControl, "Booking+BookingPartyDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.BookingPartyDocumentaryAddress)));
			this.BookingPartyDocAddressControl.BindToOrganisations = "Organisations";
			this.BookingPartyDocAddressControl.CaptionResourceString = Res.GetData("16c9a247-28d3-4d2c-acf7-5fe2c9fa166e", "Bkg. Party", "Bkg. Party", "Booking Party", "");
			this.BookingPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BookingPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			this.BookingPartyDocAddressControl.Name = "BookingPartyDocAddressControl";
			this.BookingPartyDocAddressControl.ReadOnly = false;
			this.RightTopRowPanel.SetRow(this.BookingPartyDocAddressControl, 5);
			this.BookingPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.BookingPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.BookingPartyDocAddressControl.TabIndex = 6;
			this.BookingPartyDocAddressControl.ValidationJustForced = false;
			// 
			// JS_A_BKDBoundReadOnlyDateEdit
			// 
			this.JS_A_BKDBoundReadOnlyDateEdit.AllowDrop = true;
			this.JS_A_BKDBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JS_A_BKDBoundReadOnlyDateEdit, "Booking+JS_A_BKD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_A_BKD)));
			this.JS_A_BKDBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JS_A_BKDBoundReadOnlyDateEdit.Name = "JS_A_BKDBoundReadOnlyDateEdit";
			this.RightTopRowPanel.SetRow(this.JS_A_BKDBoundReadOnlyDateEdit, 0);
			this.JS_A_BKDBoundReadOnlyDateEdit.TabIndex = 1;
			this.JS_A_BKDBoundReadOnlyDateEdit.TabStop = false;
			// 
			// JS_ClientRequestedETADateEdit
			// 
			this.JS_ClientRequestedETADateEdit.AllowDrop = true;
			this.JS_ClientRequestedETADateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JS_ClientRequestedETADateEdit, "Booking+JS_ClientRequestedETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_ClientRequestedETA)));
			this.JS_ClientRequestedETADateEdit.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|7c36aa13-cd92-4d03-8ee5-ede511552ed9", "Client Req. ETA", "Client Requested ETA.");
			this.JS_ClientRequestedETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.JS_ClientRequestedETADateEdit.Name = "JS_ClientRequestedETADateEdit";
			this.RightTopRowPanel.SetRow(this.JS_ClientRequestedETADateEdit, 1);
			this.JS_ClientRequestedETADateEdit.TabIndex = 2;
			// 
			// JS_A_RCVBoundDateEdit
			// 
			this.JS_A_RCVBoundDateEdit.AllowDrop = true;
			this.JS_A_RCVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JS_A_RCVBoundDateEdit, "Booking+JS_A_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_A_RCV)));
			this.JS_A_RCVBoundDateEdit.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|04daf5a1-0e22-4ff6-9ee6-b9427606eef4", "Warehouse Rec.");
			this.JS_A_RCVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_A_RCVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.JS_A_RCVBoundDateEdit.Name = "JS_A_RCVBoundDateEdit";
			this.RightTopRowPanel.SetRow(this.JS_A_RCVBoundDateEdit, 2);
			this.JS_A_RCVBoundDateEdit.TabIndex = 3;
			// 
			// CFSReferenceTextBox
			// 
			this.CFSReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CFSReferenceTextBox, "Booking+JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_CFSReference)));
			this.CFSReferenceTextBox.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|b9cb0b6f-a5ef-4c98-8e81-44ce0cac9875", "CFS Reference");
			this.CFSReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.CFSReferenceTextBox.Name = "CFSReferenceTextBox";
			this.RightTopRowPanel.SetRow(this.CFSReferenceTextBox, 4);
			this.CFSReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 13, true);
			this.CFSReferenceTextBox.TabIndex = 5;
			// 
			// JS_InterimReceiptBoundTextEdit
			// 
			this.JS_InterimReceiptBoundTextEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JS_InterimReceiptBoundTextEdit, "Booking+JS_InterimReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_InterimReceipt)));
			this.JS_InterimReceiptBoundTextEdit.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|d1519c10-a82b-44cb-b820-853856f4fe26", "Interim Receipt");
			this.JS_InterimReceiptBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.JS_InterimReceiptBoundTextEdit.Name = "JS_InterimReceiptBoundTextEdit";
			this.RightTopRowPanel.SetRow(this.JS_InterimReceiptBoundTextEdit, 3);
			this.JS_InterimReceiptBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 13, true);
			this.JS_InterimReceiptBoundTextEdit.TabIndex = 4;
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.AdditionalContacts);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightBottomPanel, true);
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 207, true);
			this.RightBottomPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 1, 3, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 383, true);
			this.RightBottomPanel.TabIndex = 4;
			// 
			// MiddleBottomPanel
			// 
			this.MiddleBottomPanel.AutoSize = true;
			this.MiddleBottomPanel.Controls.Add(this.FreightGatewaySellRate);
			this.MiddleBottomPanel.Controls.Add(this.FreightCostRate);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleBottomPanel, true);
			this.MiddleBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 421, true);
			this.MiddleBottomPanel.Name = "MiddleBottomPanel";
			this.MiddleBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 137, true);
			this.MiddleBottomPanel.TabIndex = 3;
			// 
			// FreightGatewaySellRate
			// 
			this.FreightGatewaySellRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightGatewaySellRate, "Booking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking)));
			this.FreightGatewaySellRate.CaptionResourceString = Res.GetData("abd90d10-c2aa-4a78-b73c-9c334e6af1c0", "Gateway Sell");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FreightGatewaySellRate, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FreightGatewaySellRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 55, true);
			this.FreightGatewaySellRate.Name = "FreightGatewaySellRate";
			this.FreightGatewaySellRate.RateType = Enterprise.Freight.Business.FreightConstants.SpotRateType.ShipmentGatewaySellRate;
			this.FreightGatewaySellRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 21, true);
			this.FreightGatewaySellRate.TabIndex = 16;
			// 
			// FreightCostRate
			// 
			this.FreightCostRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightCostRate, "Booking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking)));
			this.FreightCostRate.CaptionResourceString = Res.GetData("32320f9c-899e-4f96-a5b7-66b8d0201a35", "Negotiated Cost");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FreightCostRate, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FreightCostRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 55, true);
			this.FreightCostRate.Name = "FreightCostRate";
			this.FreightCostRate.RateType = Enterprise.Freight.Business.FreightConstants.SpotRateType.ShipmentCostRate;
			this.FreightCostRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 21, true);
			this.FreightCostRate.TabIndex = 17;
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Controls.Add(this.LeftBottomPanel);
			this.MiddlePanel.Controls.Add(this.MiddleTopPanel);
			this.MiddlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.MiddlePanel.Name = "MiddlePanel";
			this.MiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 209, true);
			this.MiddlePanel.TabIndex = 2;
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.PickupDocAddressControl);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftBottomPanel, true);
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 3, true);
			this.LeftBottomPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 3, 3, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 204, true);
			this.LeftBottomPanel.TabIndex = 1;
			// 
			// PickupDocAddressControl
			// 
			this.PickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupDocAddressControl, "Booking+ConsignorPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ConsignorPickupAddress)));
			this.PickupDocAddressControl.BindToOrganisations = "PickUps";
			this.PickupDocAddressControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|71a08ddd-5f27-4abc-a225-1deb1a189517", "Pickup Address");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupDocAddressControl, false);
			this.PickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.PickupDocAddressControl.Name = "PickupDocAddressControl";
			this.PickupDocAddressControl.ReadOnly = false;
			this.PickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.PickupDocAddressControl.TabIndex = 15;
			this.PickupDocAddressControl.ValidationJustForced = false;
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.DeliveryDocAddressControl);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleTopPanel, true);
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 3, true);
			this.MiddleTopPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 3, 3, 3, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 203, true);
			this.MiddleTopPanel.TabIndex = 2;
			// 
			// DeliveryDocAddressControl
			// 
			this.DeliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "Booking+ConsigneeDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ConsigneeDeliveryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "Deliveries";
			this.DeliveryDocAddressControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|D36239EC-1A3E-40ed-B41A-B9A107808EBD", "Delivery Address");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryDocAddressControl, false);
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.ReadOnly = false;
			this.DeliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 16;
			this.DeliveryDocAddressControl.ValidationJustForced = false;
			//
			// ServicesAndReferenceNumberDynamicControlSplitContainer
			//
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Name = "ServicesAndReferenceNumberDynamicControlSplitContainer";
			//
			// ServicesAndReferenceNumberDynamicControlSplitContainer.Panel1
			//
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel1.Controls.Add(this.servicesControl);
			//
			// BookingContainersAndPackLinesSplitContainer.Panel2
			//
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel2.Controls.Add(this.referenceNumbersGroupBoxControl);
			// 
			// 
			// BottomFillPanel
			// 
			this.BottomFillPanel.Controls.Add(this.ServicesAndReferenceNumberDynamicControlSplitContainer);
			this.BottomFillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomFillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
			this.BottomFillPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 100, true);
			this.BottomFillPanel.Name = "BottomFillPanel";
			this.BottomFillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 100, true);
			this.BottomFillPanel.TabIndex = 4;
			// 
			// ForwarderAdditionalDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomFillPanel);
			this.Controls.Add(this.MainPanel);
			this.Name = "ForwarderAdditionalDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 580, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalContacts.ResumeLayout(true);
			this.AdditionalContacts.PerformLayout();
			this.servicesControl.ResumeLayout(true);
			this.servicesControl.PerformLayout();
			this.referenceNumbersGroupBoxControl.ResumeLayout(true);
			this.referenceNumbersGroupBoxControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.scheduleChooserControl.ResumeLayout(true);
			this.scheduleChooserControl.PerformLayout();
			this.RightTopPanel.ResumeLayout(false);
			this.RightTopPanel.PerformLayout();
			this.AdditionalDetails1.ResumeLayout(false);
			this.AdditionalDetails1.PerformLayout();
			this.RightTopRowPanel.ResumeLayout(false);
			this.RightTopRowPanel.PerformLayout();
			this.BookingPartyDocAddressControl.ResumeLayout(true);
			this.BookingPartyDocAddressControl.PerformLayout();
			this.JS_A_BKDBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_A_BKDBoundReadOnlyDateEdit.PerformLayout();
			this.JS_ClientRequestedETADateEdit.ResumeLayout(true);
			this.JS_ClientRequestedETADateEdit.PerformLayout();
			this.JS_A_RCVBoundDateEdit.ResumeLayout(true);
			this.JS_A_RCVBoundDateEdit.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.MiddleBottomPanel.ResumeLayout(false);
			this.MiddleBottomPanel.PerformLayout();
			this.FreightGatewaySellRate.ResumeLayout(true);
			this.FreightGatewaySellRate.PerformLayout();
			this.FreightCostRate.ResumeLayout(true);
			this.FreightCostRate.PerformLayout();
			this.MiddlePanel.ResumeLayout(false);
			this.MiddlePanel.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.PickupDocAddressControl.ResumeLayout(true);
			this.PickupDocAddressControl.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.DeliveryDocAddressControl.ResumeLayout(true);
			this.DeliveryDocAddressControl.PerformLayout();
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel1.ResumeLayout(false);
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ServicesAndReferenceNumberDynamicControlSplitContainer)).EndInit();
			this.ServicesAndReferenceNumberDynamicControlSplitContainer.ResumeLayout(false);
			this.BottomFillPanel.ResumeLayout(false);
			this.BottomFillPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZPanel RightTopPanel;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel BottomFillPanel;
		private ZArchitecture.GUI.ZPanel MiddlePanel;
		private ZArchitecture.GUI.ZPanel MiddleTopPanel;
		private ZArchitecture.GUI.ZPanel LeftBottomPanel;
		private ZArchitecture.GUI.ZPanel RightBottomPanel;
		private ZArchitecture.GUI.ZPanel MiddleBottomPanel;
		private ScheduleChooserControl scheduleChooserControl;
		private ZArchitecture.GUI.ZGroupBox AdditionalDetails1;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel RightTopRowPanel;
		protected internal ZArchitecture.GUI.ZDateEdit JS_A_BKDBoundReadOnlyDateEdit;
		private ZArchitecture.GUI.ZDateEdit JS_ClientRequestedETADateEdit;
		private ZArchitecture.GUI.ZDateEdit JS_A_RCVBoundDateEdit;
		private ZArchitecture.ZTextBox CFSReferenceTextBox;
		private ZArchitecture.ZTextBox JS_InterimReceiptBoundTextEdit;
		private MasterFiles.GUI.ZDocAddressControl PickupDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
		private ForwardingBookingServicesControl servicesControl;
		private ForwardingReferenceNumbersControl referenceNumbersGroupBoxControl;
		private SingleLineDocAddressControl BookingPartyDocAddressControl;
		private Freight.GUI.FreightRateControl FreightCostRate;
		private Freight.GUI.FreightRateControl FreightGatewaySellRate;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl AdditionalContacts;
		KSplitContainer ServicesAndReferenceNumberDynamicControlSplitContainer;
	}
}
