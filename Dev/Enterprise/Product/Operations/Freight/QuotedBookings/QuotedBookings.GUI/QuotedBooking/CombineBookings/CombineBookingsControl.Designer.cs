using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class CombineBookingsControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ControllingAgentAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ControllingCustomerAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.congineeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.consignorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.clientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.carrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.transportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.containerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.destinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.originCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.bookingsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			dischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			loadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			shipmentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			topGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			bottomGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			dischargeCodeFindBox.SuspendLayout();
			loadPortCodeFindBox.SuspendLayout();
			topGroupBox.SuspendLayout();
			ControllingAgentAddressControl.SuspendLayout();
			ControllingCustomerAddressControl.SuspendLayout();
			this.congineeGuidFindBox.SuspendLayout();
			this.consignorGuidFindBox.SuspendLayout();
			this.clientGuidFindBox.SuspendLayout();
			this.carrierGuidFindBox.SuspendLayout();
			this.transportModeDropEdit.SuspendLayout();
			this.containerModeDropEdit.SuspendLayout();
			this.destinationCodeFindBox.SuspendLayout();
			this.originCodeFindBox.SuspendLayout();
			bottomGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bookingsModuleButtonGrid.InnerGrid)).BeginInit();
			this.bookingsModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.CombineBookings);
			// 
			// dischargeCodeFindBox
			// 
			dischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(dischargeCodeFindBox, "MasterQuotedBooking+DischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.DischargePort)));
			dischargeCodeFindBox.CaptionResourceString = Res.GetData("16f5caf5-c072-6ca5-48f2-9d2c908632ec", "Disch.", "Discharge", "The Discharge Port of the Booking");
			dischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 90, true);
			dischargeCodeFindBox.Name = "dischargeCodeFindBox";
			dischargeCodeFindBox.PreBoundMaxLength = 5;
			dischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			dischargeCodeFindBox.TabIndex = 6;
			// 
			// loadPortCodeFindBox
			// 
			loadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(loadPortCodeFindBox, "MasterQuotedBooking+LoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.LoadPort)));
			loadPortCodeFindBox.CaptionResourceString = Res.GetData("2f167686-b758-49a9-490c-f2a2c92855c7", "Load", "The Load Port of the Booking");
			loadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 90, true);
			loadPortCodeFindBox.Name = "loadPortCodeFindBox";
			loadPortCodeFindBox.PreBoundMaxLength = 5;
			loadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			loadPortCodeFindBox.TabIndex = 5;
			// 
			// shipmentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(shipmentNumberTextBox, "MasterQuotedBooking.Booking.JS_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Booking.JS_UniqueConsignRef)));
			shipmentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 16, true);
			shipmentNumberTextBox.Name = "shipmentNumberTextBox";
			shipmentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			shipmentNumberTextBox.TabIndex = 0;
			// 
			// topGroupBox
			// 
			topGroupBox.CaptionResourceString = Res.GetData("19eec049-c3eb-461f-8955-2d86def18142", "Main Booking");
			topGroupBox.Controls.Add(this.ControllingAgentAddressControl);
			topGroupBox.Controls.Add(this.ControllingCustomerAddressControl);
			topGroupBox.Controls.Add(this.congineeGuidFindBox);
			topGroupBox.Controls.Add(this.consignorGuidFindBox);
			topGroupBox.Controls.Add(this.clientGuidFindBox);
			topGroupBox.Controls.Add(this.carrierGuidFindBox);
			topGroupBox.Controls.Add(this.transportModeDropEdit);
			topGroupBox.Controls.Add(this.containerModeDropEdit);
			topGroupBox.Controls.Add(this.destinationCodeFindBox);
			topGroupBox.Controls.Add(this.originCodeFindBox);
			topGroupBox.Controls.Add(this.VesselTextBox);
			topGroupBox.Controls.Add(dischargeCodeFindBox);
			topGroupBox.Controls.Add(loadPortCodeFindBox);
			topGroupBox.Controls.Add(this.VoyageNumberTextBox);
			topGroupBox.Controls.Add(shipmentNumberTextBox);
			topGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(topGroupBox, false);
			topGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topGroupBox.Name = "topGroupBox";
			topGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 224, true);
			topGroupBox.TabIndex = 0;
			topGroupBox.TabStop = false;
			// 
			// ControllingAgentAddressControl
			// 
			this.ControllingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentAddressControl, "MasterQuotedBooking.Booking.ControllingAgentDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Booking.ControllingAgentDocumentaryAddress)));
			this.ControllingAgentAddressControl.BindToOrganisations = "MasterQuotedBooking.Booking.Lookups.ControllingAgentList";
			this.ControllingAgentAddressControl.CaptionResourceString = Res.GetData("6b8cfb9d-d656-468c-be14-8a9d767a4526", "Controlling Agent");
			this.ControllingAgentAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ControllingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 189, true);
			this.ControllingAgentAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingAgentAddressControl.Name = "ControllingAgentAddressControl";
			this.ControllingAgentAddressControl.ReadOnly = true;
			this.ControllingAgentAddressControl.SingleLineNoGroupBoxPanelWidth = 264;
			this.ControllingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.ControllingAgentAddressControl.TabIndex = 14;
			this.ControllingAgentAddressControl.ValidationJustForced = false;
			var controllingAgentLabel = this.ControllingAgentAddressControl.GetExtension<ILabelCaptionRenderer>() as LabelCaptionRenderer;
			if (controllingAgentLabel!= null )
			{
				controllingAgentLabel.Font = new System.Drawing.Font(controllingAgentLabel.Font, System.Drawing.FontStyle.Bold);
			}
			// 
			// ControllingCustomerAddressControl
			// 
			this.ControllingCustomerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerAddressControl, "MasterQuotedBooking.Booking.ControllingCustomerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Booking.ControllingCustomerAddress)));
			this.ControllingCustomerAddressControl.BindToOrganisations = "MasterQuotedBooking.Booking.Lookups.ControllingCustomerList";
			this.ControllingCustomerAddressControl.CaptionResourceString = Res.GetData("4bbbe697-3e34-4fea-8f8d-35a99605abf0", "Controlling Customer");
			this.ControllingCustomerAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ControllingCustomerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 189, true);
			this.ControllingCustomerAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingCustomerAddressControl.Name = "ControllingCustomerAddressControl";
			this.ControllingCustomerAddressControl.ReadOnly = true;
			this.ControllingCustomerAddressControl.SingleLineNoGroupBoxPanelWidth = 264;
			this.ControllingCustomerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ControllingCustomerAddressControl.TabIndex = 13;
			this.ControllingCustomerAddressControl.ValidationJustForced = false;
			var controllingCustomerLabel = this.ControllingCustomerAddressControl.GetExtension<ILabelCaptionRenderer>() as LabelCaptionRenderer;
			if (controllingCustomerLabel!= null )
			{
				controllingCustomerLabel.Font = new System.Drawing.Font(controllingCustomerLabel.Font, System.Drawing.FontStyle.Bold);
			}

			// 
			// congineeGuidFindBox
			// 
			this.congineeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.congineeGuidFindBox, "MasterQuotedBooking.Booking.ConsigneePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Booking.ConsigneePK)));
			this.congineeGuidFindBox.CaptionResourceString = Res.GetData("4cf1e029-081e-4493-aa27-8177b4657643", "Consignee");
			this.congineeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 142, true);
			this.congineeGuidFindBox.Name = "congineeGuidFindBox";
			this.congineeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.congineeGuidFindBox.TabIndex = 10;
			// 
			// consignorGuidFindBox
			// 
			this.consignorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consignorGuidFindBox, "MasterQuotedBooking.Booking.ConsignorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Booking.ConsignorPK)));
			this.consignorGuidFindBox.CaptionResourceString = Res.GetData("c1d73251-a851-45d9-94f6-b760330c7de5", "Consignor");
			this.consignorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 142, true);
			this.consignorGuidFindBox.Name = "consignorGuidFindBox";
			this.consignorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.consignorGuidFindBox.TabIndex = 9;
			// 
			// clientGuidFindBox
			// 
			this.clientGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientGuidFindBox, "MasterQuotedBooking.ClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.ClientPK)));
			this.clientGuidFindBox.CaptionResourceString = Res.GetData("e39c941a-c4cb-48d6-ab52-bbc123001d3d", "Client");
			this.clientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 166, true);
			this.clientGuidFindBox.Name = "clientGuidFindBox";
			this.clientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.clientGuidFindBox.TabIndex = 11;
			// 
			// carrierGuidFindBox
			// 
			this.carrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.carrierGuidFindBox, "MasterQuotedBooking.OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.OH_Carrier)));
			this.carrierGuidFindBox.CaptionResourceString = Res.GetData("b9f271d8-dc18-4144-bac0-763d43f2671b", "Carrier");
			this.carrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 166, true);
			this.carrierGuidFindBox.Name = "carrierGuidFindBox";
			this.carrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.carrierGuidFindBox.TabIndex = 12;
			// 
			// transportModeDropEdit
			// 
			this.transportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.transportModeDropEdit, "MasterQuotedBooking.TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.TransportMode)));
			this.transportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 40, true);
			this.transportModeDropEdit.Name = "transportModeDropEdit";
			this.transportModeDropEdit.PreBoundMaxLength = 3;
			this.transportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.transportModeDropEdit.TabIndex = 1;
			// 
			// containerModeDropEdit
			// 
			this.containerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containerModeDropEdit, "MasterQuotedBooking.ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.ContainerMode)));
			this.containerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 40, true);
			this.containerModeDropEdit.Name = "containerModeDropEdit";
			this.containerModeDropEdit.PreBoundMaxLength = 3;
			this.containerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.containerModeDropEdit.TabIndex = 2;
			// 
			// destinationCodeFindBox
			// 
			this.destinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationCodeFindBox, "MasterQuotedBooking.Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Destination)));
			this.destinationCodeFindBox.CaptionResourceString = Res.GetData("d2715d22-4310-4d9f-8b69-c47e39a9bb8e", "Dest.", "Destination", "");
			this.destinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 116, true);
			this.destinationCodeFindBox.Name = "destinationCodeFindBox";
			this.destinationCodeFindBox.PopupCaption = "Select Destination";
			this.destinationCodeFindBox.PreBoundMaxLength = 5;
			this.destinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.destinationCodeFindBox.TabIndex = 8;
			// 
			// originCodeFindBox
			// 
			this.originCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originCodeFindBox, "MasterQuotedBooking.Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.Origin)));
			this.originCodeFindBox.CaptionResourceString = Res.GetData("b2cedf22-dc33-4aac-a849-9a7cf7ae1203", "Origin");
			this.originCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 116, true);
			this.originCodeFindBox.Name = "originCodeFindBox";
			this.originCodeFindBox.PopupCaption = "Select Origin";
			this.originCodeFindBox.PreBoundMaxLength = 5;
			this.originCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.originCodeFindBox.TabIndex = 7;
			// 
			// VesselTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselTextBox, "MasterQuotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel)));
			this.VesselTextBox.CaptionResourceString = Res.GetData("1612d3b5-5687-41b1-838c-fca0b90c5447", "Vessel");
			this.VesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 64, true);
			this.VesselTextBox.Name = "VesselTextBox";
			this.VesselTextBox.ReadOnly = true;
			this.VesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.VesselTextBox.TabIndex = 4;
			// 
			// VoyageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNumberTextBox, "MasterQuotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).MasterQuotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight)));
			this.VoyageNumberTextBox.CaptionResourceString = Res.GetData("3c9c111e-6583-46a1-b827-a337146867d5", "Voyage No");
			this.VoyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 64, true);
			this.VoyageNumberTextBox.Name = "VoyageNumberTextBox";
			this.VoyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.VoyageNumberTextBox.TabIndex = 3;
			// 
			// bottomGroupBox
			// 
			bottomGroupBox.CaptionResourceString = Res.GetData("4ea5b6ac-c867-4a0d-beae-4bbe89f4679b", "Bookings to Merge Into the Main Booking");
			bottomGroupBox.Controls.Add(this.bookingsModuleButtonGrid);
			bottomGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(bottomGroupBox, false);
			bottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 224, true);
			bottomGroupBox.Name = "bottomGroupBox";
			bottomGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 5, true);
			bottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 349, true);
			bottomGroupBox.TabIndex = 1;
			bottomGroupBox.TabStop = false;
			// 
			// bookingsModuleButtonGrid
			// 
			this.bookingsModuleButtonGrid.AllowDrop = true;
			this.bookingsModuleButtonGrid.AttachButtonText = Res.GetData("CAA468A6-9DC7-42C1-B86F-16FB5FC83AAD", "Add");
			this.bookingsModuleButtonGrid.InnerGrid.RemoveAction = RemoveAction.Remove;
			this.BindingSource.SetBindingMember(this.bookingsModuleButtonGrid, "OtherViewQuotedBookings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).OtherViewQuotedBookings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.CombineBookings)(null)).OtherViewQuotedBookingsLookups)));
			this.bookingsModuleButtonGrid.BindToFindBoxList = "OtherViewQuotedBookingsLookups";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("b21035d4-72e9-4064-b45c-9bea7c203236", "Booking #");
			zTextBoxColumnStyleInfo1.ColumnName = "QuotedBooking+Booking+JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("afcd1019-5ee7-7cb6-4abc-300e23ba7ec0", "Incoterm");
			zTextBoxColumnStyleInfo2.ColumnName = "QuotedBooking+PaymentTerms";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("322c657c-f576-4102-b891-e3a0b3f3d89f", "Service Lvl.", "Service Level");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "QuotedBooking+ServiceLevel";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Res.GetData("7905d4b3-e76b-4ac3-ba6a-7898f14c7584", "Shipper Ref");
			zTextBoxColumnStyleInfo4.ColumnName = "QuotedBooking+Booking+JS_BookingReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Res.GetData("3e351791-4f5f-4d25-bf57-44eaf99ec632", "Cargo Desc.", "Cargo Description");
			zTextBoxColumnStyleInfo5.ColumnName = "QuotedBooking+GoodsDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("4e66b48e-c719-4c12-a3ab-d7b0d3874b73", "Pickup Org.");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "QuotedBooking+Booking+ConsignorPickupAddress+Organisation+MainAddress+OA_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("967f6d28-ac36-4419-ba50-cb5cf76a99d6", "Delivery Org.");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "QuotedBooking+Booking+ConsigneeDeliveryAddress+Organisation+MainAddress+OA_OH";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "QuotedBooking+Booking+JS_OuterPacks";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "QuotedBooking+Booking+JS_F3_NKPackType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("4f494d04-22e1-4494-aec1-93cdd20cb548", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "QuotedBooking+Weight";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Res.GetData("eae740c5-f09b-4e13-ab2c-6366e17d00b9", "Volume");
			zCalcEditColumnStyleInfo2.ColumnName = "QuotedBooking+Volume";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Res.GetData("47fedf4e-9120-43be-8792-13823970494a", "Chargeable");
			zCalcEditColumnStyleInfo3.ColumnName = "QuotedBooking+Chargeable";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Res.GetData("c53add4e-be1d-493a-8a9a-5574e6da6795", "Vessel");
			zTextBoxColumnStyleInfo8.ColumnName = "QuotedBooking+Booking+JS_Calc_CurrentVessel";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Res.GetData("217998a8-7014-4282-914d-a34affbb8260", "Voyage/Flight");
			zTextBoxColumnStyleInfo9.ColumnName = "QuotedBooking+Booking+JS_Calc_CurrentVoyageFlight";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Res.GetData("e674ea90-661b-474d-b549-1f609d28fd17", "Load");
			zTextBoxColumnStyleInfo10.ColumnName = "QuotedBooking+LoadPort";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Res.GetData("998caaf5-bd91-4a3a-84bc-3afab1596d49", "Disch.", "Discharge");
			zTextBoxColumnStyleInfo11.ColumnName = "QuotedBooking+DischargePort";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "QuotedBooking+OH_Carrier";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.bookingsModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.bookingsModuleButtonGrid.DetachButtonText = Res.GetData("72A87189-8FFB-408F-82BF-36EBA4F17982", "Remove");
			this.bookingsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bookingsModuleButtonGrid.GridId = "38c95e18-7db6-4f3b-a737-ffe629f5ed9e";
			// 
			// 
			// 
			this.bookingsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.bookingsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bookingsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.bookingsModuleButtonGrid.InnerGrid.GridId = null;
			this.bookingsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.bookingsModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.bookingsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.bookingsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.bookingsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.bookingsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 290, true);
			this.bookingsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.bookingsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			this.bookingsModuleButtonGrid.Name = "bookingsModuleButtonGrid";
			this.bookingsModuleButtonGrid.NameOfAGridElement = Res.GetData("f571e895-3813-4585-922d-990fab5d32a1", "Quoted Booking");
			this.bookingsModuleButtonGrid.ReadOnly = false;
			this.bookingsModuleButtonGrid.ShowEditButton = false;
			this.bookingsModuleButtonGrid.ShowNewButton = false;
			this.bookingsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 328, true);
			this.bookingsModuleButtonGrid.TabIndex = 15;
			// 
			// CombineBookingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(bottomGroupBox);
			this.Controls.Add(topGroupBox);
			this.Name = "CombineBookingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 573, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			dischargeCodeFindBox.ResumeLayout(true);
			dischargeCodeFindBox.PerformLayout();
			loadPortCodeFindBox.ResumeLayout(true);
			loadPortCodeFindBox.PerformLayout();
			topGroupBox.ResumeLayout(false);
			topGroupBox.PerformLayout();
			this.ControllingAgentAddressControl.ResumeLayout(true);
			this.ControllingAgentAddressControl.PerformLayout();
			this.ControllingCustomerAddressControl.ResumeLayout(true);
			this.ControllingCustomerAddressControl.PerformLayout();
			this.congineeGuidFindBox.ResumeLayout(true);
			this.congineeGuidFindBox.PerformLayout();
			this.consignorGuidFindBox.ResumeLayout(true);
			this.consignorGuidFindBox.PerformLayout();
			this.clientGuidFindBox.ResumeLayout(true);
			this.clientGuidFindBox.PerformLayout();
			this.carrierGuidFindBox.ResumeLayout(true);
			this.carrierGuidFindBox.PerformLayout();
			this.transportModeDropEdit.ResumeLayout(true);
			this.transportModeDropEdit.PerformLayout();
			this.containerModeDropEdit.ResumeLayout(true);
			this.containerModeDropEdit.PerformLayout();
			this.destinationCodeFindBox.ResumeLayout(true);
			this.destinationCodeFindBox.PerformLayout();
			this.originCodeFindBox.ResumeLayout(true);
			this.originCodeFindBox.PerformLayout();
			bottomGroupBox.ResumeLayout(false);
			bottomGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.bookingsModuleButtonGrid.InnerGrid)).EndInit();
			this.bookingsModuleButtonGrid.ResumeLayout(true);
			this.bookingsModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox VesselTextBox;
		private Enterprise.ZArchitecture.ZTextBox VoyageNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox dischargeCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox loadPortCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox shipmentNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox topGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox bottomGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox destinationCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox originCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit transportModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit containerModeDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox carrierGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox congineeGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox consignorGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox clientGuidFindBox;
		private ZArchitecture.GUI.ZModuleButtonGrid bookingsModuleButtonGrid;
		private MasterFiles.GUI.ZDocAddressControl ControllingAgentAddressControl;
		private MasterFiles.GUI.ZDocAddressControl ControllingCustomerAddressControl;
	}
}
