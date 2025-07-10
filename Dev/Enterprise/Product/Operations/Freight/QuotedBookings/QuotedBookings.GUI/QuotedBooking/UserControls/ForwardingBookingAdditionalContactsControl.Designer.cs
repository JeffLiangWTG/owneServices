using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ForwardingBookingAdditionalContactsControl
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

		#region Component Designer Generated Code

		CargoWise.Windows.UI.Layout.RowLayoutPanel ContactsPanel;
		ZOrganisationControl PickupAgentBoundOrganisationControl;
		ZArchitecture.GUI.ZGroupBox ReceivalPointGroupBox;
		ZArchitecture.GUI.ZGroupBox DeliveryPointGroupBox;
		ZArchitecture.GUI.ZAddressControl ReceivalPointAddressControl;
		ZArchitecture.GUI.ZAddressControl DeliveryPointAddressControl;
		ZArchitecture.GUI.ZGroupBox PortTransportGroupBox;
		ZArchitecture.GUI.ZAddressControl JP_OA_PickupCartageCoAddresControl;
		MasterFiles.GUI.ZOrganisationControl DeliveryAgentBoundOrganisationControl;
		MasterFiles.GUI.ZOrganisationControl ExportBrokerCoOrganisationControl;
		MasterFiles.GUI.ZOrganisationControl ImportBrokerCoOrganisationControl;
		ZDocAddressControl ControllingCustomerAddressControl;
		ZDocAddressControl ControllingAgentAddressControl;
		ZArchitecture.GUI.ZGroupBox AdditionalContactsGroupBox;

		void InitializeComponent()
		{
			this.AdditionalContactsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactsPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.PickupAgentBoundOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ReceivalPointGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryPointGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceivalPointAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeliveryPointAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PortTransportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JP_OA_PickupCartageCoAddresControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeliveryAgentBoundOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ExportBrokerCoOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ImportBrokerCoOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ControllingAgentAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ControllingCustomerAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalContactsGroupBox.SuspendLayout();
			this.PickupAgentBoundOrganisationControl.SuspendLayout();
			this.ReceivalPointGroupBox.SuspendLayout();
			this.DeliveryPointGroupBox.SuspendLayout();
			this.ReceivalPointAddressControl.SuspendLayout();
			this.DeliveryPointAddressControl.SuspendLayout();
			this.PortTransportGroupBox.SuspendLayout();
			this.JP_OA_PickupCartageCoAddresControl.SuspendLayout();
			this.DeliveryAgentBoundOrganisationControl.SuspendLayout();
			this.ExportBrokerCoOrganisationControl.SuspendLayout();
			this.ImportBrokerCoOrganisationControl.SuspendLayout();
			this.ControllingAgentAddressControl.SuspendLayout();
			this.ControllingCustomerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// ContactsGroupBox
			// 
			this.AdditionalContactsGroupBox.AutoSize = true;
			this.AdditionalContactsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AdditionalContactsGroupBox.Controls.Add(this.ContactsPanel);
			this.AdditionalContactsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalContactsGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AdditionalContactsGroupBox.Name = "AdditionalContactsGroupBox";
			this.AdditionalContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 335, true);
			this.AdditionalContactsGroupBox.TabIndex = 0;
			this.AdditionalContactsGroupBox.TabStop = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalContactsGroupBox, false);
			// 
			// ContactsPanel
			// 
			this.ContactsPanel.AutoSize = true;
			this.ContactsPanel.Controls.Add(this.PickupAgentBoundOrganisationControl);
			this.ContactsPanel.Controls.Add(this.ReceivalPointGroupBox);
			this.ContactsPanel.Controls.Add(this.DeliveryPointGroupBox);
			this.ContactsPanel.Controls.Add(this.PortTransportGroupBox);
			this.ContactsPanel.Controls.Add(this.DeliveryAgentBoundOrganisationControl);
			this.ContactsPanel.Controls.Add(this.ExportBrokerCoOrganisationControl);
			this.ContactsPanel.Controls.Add(this.ImportBrokerCoOrganisationControl);
			this.ContactsPanel.Controls.Add(this.ControllingAgentAddressControl);
			this.ContactsPanel.Controls.Add(this.ControllingCustomerAddressControl);
			this.ContactsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContactsPanel.Name = "ContactsPanel";
			this.ContactsPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(39);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.ContactsPanel, true);
			this.ContactsPanel.TabIndex = 0;
			this.ContactsPanel.SetRow(this.ReceivalPointGroupBox, 0);
			this.ContactsPanel.SetRow(this.DeliveryPointGroupBox, 1);
			this.ContactsPanel.SetRow(this.PickupAgentBoundOrganisationControl, 2);
			this.ContactsPanel.SetRow(this.DeliveryAgentBoundOrganisationControl, 3);
			this.ContactsPanel.SetRow(this.ExportBrokerCoOrganisationControl, 4);
			this.ContactsPanel.SetRow(this.ImportBrokerCoOrganisationControl, 5);
			this.ContactsPanel.SetRow(this.PortTransportGroupBox, 6);
			this.ContactsPanel.SetRow(this.ControllingCustomerAddressControl, 7);
			this.ContactsPanel.SetRow(this.ControllingAgentAddressControl, 8);
			//
			// PickupAgentBoundOrganisationControl
			// 
			this.PickupAgentBoundOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupAgentBoundOrganisationControl, "Booking+PickupAgentDocumentaryAddress+OrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.PickupAgentDocumentaryAddress.OrganisationPK)));
			this.PickupAgentBoundOrganisationControl.BindToOrganisations = "Booking.Lookups.PickupAgent_List";
			this.PickupAgentBoundOrganisationControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|441E7807-6A83-4084-88D5-7D8CC8D06187", "Pickup Agent");
			this.PickupAgentBoundOrganisationControl.Captions = new string[] {
        "Pickup Agent"};
			this.PickupAgentBoundOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.PickupAgentBoundOrganisationControl.IsCaptionOverridden = false;
			this.PickupAgentBoundOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.PickupAgentBoundOrganisationControl.Name = "PickupAgentBoundOrganisationControl";
			this.PickupAgentBoundOrganisationControl.PopupCaption = "";
			this.PickupAgentBoundOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 32, true);
			this.PickupAgentBoundOrganisationControl.TabIndex = 4;
			//
			// ReceivalPointGroupBox
			//
			this.ReceivalPointGroupBox.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|27f5cd67-72c2-4ed7-ac3b-713c4469d42d", "Pickup CFS");
			this.ReceivalPointGroupBox.Controls.Add(this.ReceivalPointAddressControl);
			this.ReceivalPointGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 10, true);
			this.ReceivalPointGroupBox.Name = "ReceivalPointGroupBox";
			this.ReceivalPointGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 37, true);
			this.ReceivalPointGroupBox.TabIndex = 1;
			this.ReceivalPointGroupBox.TabStop = false;
			// 
			// ReceivalPointAddressControl
			// 
			this.ReceivalPointAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivalPointAddressControl, "ExportReceivingDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ExportReceivingDepot)));
			this.ReceivalPointAddressControl.BindToOrgList = "Receiver_List";
			this.ReceivalPointAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.ReceivalPointAddressControl.Name = "ReceivalPointAddressControl";
			this.ReceivalPointAddressControl.PopupCaption = "";
			this.ReceivalPointAddressControl.ReadOnly = false;
			this.ReceivalPointAddressControl.ShowAddress = false;
			this.ReceivalPointAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 13, true);
			this.ReceivalPointAddressControl.TabIndex = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceivalPointAddressControl, false);
			//
			// DeliveryPointGroupBox
			//
			this.DeliveryPointGroupBox.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|f2cc4167-3ab0-47a8-9c53-5922ce25f3d4", "Delivery CFS");
			this.DeliveryPointGroupBox.Controls.Add(this.DeliveryPointAddressControl);
			this.DeliveryPointGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 49, true);
			this.DeliveryPointGroupBox.Name = "DeliveryPointGroupBox";
			this.DeliveryPointGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 37, true);
			this.DeliveryPointGroupBox.TabIndex = 3;
			this.DeliveryPointGroupBox.TabStop = false;
			//
			// DeliveryPointAddressControl
			//
			this.DeliveryPointAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryPointAddressControl, "ImportReleaseDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ImportReleaseDepot)));
			this.DeliveryPointAddressControl.BindToOrgList = "Delivery_List";
			this.DeliveryPointAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.DeliveryPointAddressControl.Name = "DeliveryPointAddressControl";
			this.DeliveryPointAddressControl.PopupCaption = "";
			this.DeliveryPointAddressControl.ReadOnly = false;
			this.DeliveryPointAddressControl.ShowAddress = false;
			this.DeliveryPointAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 13, true);
			this.DeliveryPointAddressControl.TabIndex = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryPointAddressControl, false);
			//
			// PortTransportGroupBox
			// 
			this.PortTransportGroupBox.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|10828459-01e8-4370-a482-0b359664891a", "Port Transport");
			this.PortTransportGroupBox.Controls.Add(this.JP_OA_PickupCartageCoAddresControl);
			this.PortTransportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 242, true);
			this.PortTransportGroupBox.Name = "PortTransportGroupBox";
			this.PortTransportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 37, true);
			this.PortTransportGroupBox.TabIndex = 6;
			this.PortTransportGroupBox.TabStop = false;
			// 
			// JP_OA_PickupCartageCoAddresControl
			// 
			this.JP_OA_PickupCartageCoAddresControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JP_OA_PickupCartageCoAddresControl, "Booking+DocsAndCartage+JP_OA_PickupCartageCoAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr)));
			this.JP_OA_PickupCartageCoAddresControl.BindToOrgList = "Booking.Lookups.LocalTransport_List";
			this.JP_OA_PickupCartageCoAddresControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.JP_OA_PickupCartageCoAddresControl.Name = "JP_OA_PickupCartageCoAddresControl";
			this.JP_OA_PickupCartageCoAddresControl.PopupCaption = "";
			this.JP_OA_PickupCartageCoAddresControl.ReadOnly = false;
			this.JP_OA_PickupCartageCoAddresControl.ShowAddress = false;
			this.JP_OA_PickupCartageCoAddresControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 13, true);
			this.JP_OA_PickupCartageCoAddresControl.TabIndex = 8;
			//
			// DeliveryAgentBoundOrganisationControl
			// 
			this.DeliveryAgentBoundOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryAgentBoundOrganisationControl, "Booking+JS_OH_DeliveryAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_OH_DeliveryAgent)));
			this.DeliveryAgentBoundOrganisationControl.BindToOrganisations = "Booking.Lookups.Forwarder_List";
			this.DeliveryAgentBoundOrganisationControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|EBB5775E-6FBA-4e20-A87A-96EE9FC9E655", "Delivery Agent");
			this.DeliveryAgentBoundOrganisationControl.Captions = new string[] {
        "Delivery Agent"};
			this.DeliveryAgentBoundOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.DeliveryAgentBoundOrganisationControl.IsCaptionOverridden = false;
			this.DeliveryAgentBoundOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.DeliveryAgentBoundOrganisationControl.Name = "DeliveryAgentBoundOrganisationControl";
			this.DeliveryAgentBoundOrganisationControl.PopupCaption = "";
			this.DeliveryAgentBoundOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 32, true);
			this.DeliveryAgentBoundOrganisationControl.TabIndex = 5;
			//
			// ExportBrokerCoOrganisationControl
			// 
			this.ExportBrokerCoOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportBrokerCoOrganisationControl, "Booking+JS_OH_ExportBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_OH_ExportBroker)));
			this.ExportBrokerCoOrganisationControl.BindToOrganisations = "Booking.Lookups.ExportBroker_List";
			this.ExportBrokerCoOrganisationControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Export Broker");
			this.ExportBrokerCoOrganisationControl.Captions = new string[] {
        "Export Broker"};
			this.ExportBrokerCoOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.ExportBrokerCoOrganisationControl.IsCaptionOverridden = false;
			this.ExportBrokerCoOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 165, true);
			this.ExportBrokerCoOrganisationControl.Name = "ExportBrokerCoOrganisationControl";
			this.ExportBrokerCoOrganisationControl.PopupCaption = "";
			this.ExportBrokerCoOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 32, true);
			this.ExportBrokerCoOrganisationControl.TabIndex = 6;
			//
			// ImportBrokerCoOrganisationControl
			// 
			this.ImportBrokerCoOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportBrokerCoOrganisationControl, "Booking+JS_OH_ImportBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_OH_ImportBroker)));
			this.ImportBrokerCoOrganisationControl.BindToOrganisations = "Booking.Lookups.Broker_List";
			this.ImportBrokerCoOrganisationControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailControl|f6f522c2-442f-48e0-b07d-2273cc37eb71", "Import Broker");
			this.ImportBrokerCoOrganisationControl.Captions = new string[] {
        "Import Broker"};
			this.ImportBrokerCoOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.ImportBrokerCoOrganisationControl.IsCaptionOverridden = false;
			this.ImportBrokerCoOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 204, true);
			this.ImportBrokerCoOrganisationControl.Name = "ImportBrokerCoOrganisationControl";
			this.ImportBrokerCoOrganisationControl.PopupCaption = "";
			this.ImportBrokerCoOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 32, true);
			this.ImportBrokerCoOrganisationControl.TabIndex = 7;
			//
			// ControllingAgentAddressControl
			// 
			this.ControllingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentAddressControl, "Booking+ControllingAgentDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ControllingAgentDocumentaryAddress)));
			this.ControllingAgentAddressControl.BindToOrganisations = "Booking.Lookups.ControllingAgentList";
			this.ControllingAgentAddressControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|d83fbf3a-abf6-4632-93c8-889495004640", "Controlling Agent");
			this.ControllingAgentAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverride;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ControllingAgentAddressControl, false);
			this.ControllingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 325, true);
			this.ControllingAgentAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingAgentAddressControl.Name = "ControllingAgentAddressControl";
			this.ControllingAgentAddressControl.ReadOnly = false;
			this.ControllingAgentAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ControllingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 45, true);
			this.ControllingAgentAddressControl.TabIndex = 9;
			this.ControllingAgentAddressControl.ValidationJustForced = false;
			// 
			// ControllingCustomerAddressControl
			// 
			this.ControllingCustomerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerAddressControl, "Booking+ControllingCustomerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.ControllingCustomerAddress)));
			this.ControllingCustomerAddressControl.BindToOrganisations = "Booking.Lookups.ControllingCustomerList";
			this.ControllingCustomerAddressControl.CaptionResourceString = Res.GetData("ForwarderAdditionalDetailsControl|a36e7471-72c2-489d-8d0b-3466da88cb18", "Controlling Customer");
			this.ControllingCustomerAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverride;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ControllingCustomerAddressControl, false);
			this.ControllingCustomerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 281, true);
			this.ControllingCustomerAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingCustomerAddressControl.Name = "ControllingCustomerAddressControl";
			this.ControllingCustomerAddressControl.ReadOnly = false;
			this.ControllingCustomerAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ControllingCustomerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 45, true);
			this.ControllingCustomerAddressControl.TabIndex = 10;
			this.ControllingCustomerAddressControl.ValidationJustForced = false;
			// 
			// ForwardingBookingAdditionalContactsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalContactsGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "ForwardingBookingAdditionalContactsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalContactsGroupBox.ResumeLayout(false);
			this.AdditionalContactsGroupBox.PerformLayout();
			this.PickupAgentBoundOrganisationControl.ResumeLayout(true);
			this.PickupAgentBoundOrganisationControl.PerformLayout();
			this.ReceivalPointGroupBox.ResumeLayout(false);
			this.ReceivalPointGroupBox.PerformLayout();
			this.ReceivalPointAddressControl.ResumeLayout(true);
			this.ReceivalPointAddressControl.PerformLayout();
			this.DeliveryPointGroupBox.ResumeLayout(false);
			this.DeliveryPointGroupBox.PerformLayout();
			this.DeliveryPointAddressControl.ResumeLayout(true);
			this.DeliveryPointAddressControl.PerformLayout();
			this.PortTransportGroupBox.ResumeLayout(false);
			this.PortTransportGroupBox.PerformLayout();
			this.JP_OA_PickupCartageCoAddresControl.ResumeLayout(true);
			this.JP_OA_PickupCartageCoAddresControl.PerformLayout();
			this.DeliveryAgentBoundOrganisationControl.ResumeLayout(true);
			this.DeliveryAgentBoundOrganisationControl.PerformLayout();
			this.ExportBrokerCoOrganisationControl.ResumeLayout(true);
			this.ExportBrokerCoOrganisationControl.PerformLayout();
			this.ImportBrokerCoOrganisationControl.ResumeLayout(true);
			this.ImportBrokerCoOrganisationControl.PerformLayout();
			this.ControllingAgentAddressControl.ResumeLayout(true);
			this.ControllingAgentAddressControl.PerformLayout();
			this.ControllingCustomerAddressControl.ResumeLayout(true);
			this.ControllingCustomerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
