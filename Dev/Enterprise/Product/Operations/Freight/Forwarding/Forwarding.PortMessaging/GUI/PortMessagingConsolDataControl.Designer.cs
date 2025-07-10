using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	partial class PortMessagingConsolDataControl
	{
		private System.ComponentModel.IContainer components = null;
		
		private void InitializeComponent()
		{
            this.PortOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShipperGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShipperPortAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ShipperNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ShipperCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ConsolDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DepartureReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ShippingLineTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.BillNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CarrierBookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.VoyageNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.VesselNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SenderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.SenderNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SenderCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OperatorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FaxTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TelephoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OperatorTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TerminalBerthShedTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ShipperEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AgentEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PortOrderGroupBox.SuspendLayout();
            this.ShipperGroupBox.SuspendLayout();
            this.ConsolDataGroupBox.SuspendLayout();
            this.SenderGroupBox.SuspendLayout();
            this.OperatorGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData);
            // 
            // PortOrderGroupBox
            // 
            this.PortOrderGroupBox.Controls.Add(this.ShipperGroupBox);
            this.PortOrderGroupBox.Controls.Add(this.ConsolDataGroupBox);
            this.PortOrderGroupBox.Controls.Add(this.SenderGroupBox);
            this.PortOrderGroupBox.Controls.Add(this.OperatorGroupBox);
            this.PortOrderGroupBox.Controls.Add(this.TerminalBerthShedTextBox);
            this.PortOrderGroupBox.Controls.Add(this.DateEdit);
            this.PortOrderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PortOrderGroupBox.Name = "PortOrderGroupBox";
            this.PortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 270, true);
            this.PortOrderGroupBox.TabIndex = 0;
            this.PortOrderGroupBox.TabStop = false;
            // 
            // ShipperGroupBox
            // 
            this.ShipperGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("bfa61e4a-2e80-4de0-86a1-58e4a784d8e5", "Agent / Issuer");
            this.ShipperGroupBox.Controls.Add(this.ShipperPortAccountTextBox);
            this.ShipperGroupBox.Controls.Add(this.ShipperNameTextBox);
            this.ShipperGroupBox.Controls.Add(this.ShipperCodeTextBox);
            this.ShipperGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 176, true);
            this.ShipperGroupBox.Name = "ShipperGroupBox";
            this.ShipperGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 86, true);
            this.ShipperGroupBox.TabIndex = 12;
            this.ShipperGroupBox.TabStop = false;
            // 
            // ShipperPortAccountTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShipperPortAccountTextBox, "ShipperPortAccount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).ShipperPortAccount)));
            this.ShipperPortAccountTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("27cfc2f3-4e76-4258-8ce5-685f2abef3dc", "Port Account");
            this.ShipperPortAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
            this.ShipperPortAccountTextBox.Name = "ShipperPortAccountTextBox";
            this.ShipperPortAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.ShipperPortAccountTextBox.TabIndex = 14;
            // 
            // ShipperNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShipperNameTextBox, "ShipperName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).ShipperName)));
            this.ShipperNameTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("bcebbf10-e18c-427b-892f-c711c66f70ff", "Name");
            this.ShipperNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 61, true);
            this.ShipperNameTextBox.Name = "ShipperNameTextBox";
            this.ShipperNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.ShipperNameTextBox.TabIndex = 15;
            // 
            // ShipperCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShipperCodeTextBox, "ShipperCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).ShipperCode)));
            this.ShipperCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("b70c4f10-de08-476a-bc11-a18445ec7609", "Code");
            this.ShipperCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
            this.ShipperCodeTextBox.Name = "ShipperCodeTextBox";
            this.ShipperCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.ShipperCodeTextBox.TabIndex = 13;
            // 
            // ConsolDataGroupBox
            // 
            this.ConsolDataGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("0844d326-f960-47b0-b0ed-b0a219f94d74", "Consol Data");
            this.ConsolDataGroupBox.Controls.Add(this.AgentEORITextBox);
            this.ConsolDataGroupBox.Controls.Add(this.ShipperEORITextBox);
            this.ConsolDataGroupBox.Controls.Add(this.DepartureReferenceTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.WarehouseTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.ShippingLineTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.DestinationTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.DepartureDateEdit);
            this.ConsolDataGroupBox.Controls.Add(this.BillNoTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.CarrierBookingRefTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.VoyageNoTextBox);
            this.ConsolDataGroupBox.Controls.Add(this.VesselNameTextBox);
            this.ConsolDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 85, true);
            this.ConsolDataGroupBox.Name = "ConsolDataGroupBox";
            this.ConsolDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 177, true);
            this.ConsolDataGroupBox.TabIndex = 16;
            this.ConsolDataGroupBox.TabStop = false;
            // 
            // DepartureReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.DepartureReferenceTextBox, "DepartureReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).DepartureReference)));
            this.DepartureReferenceTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("216e02cc-fb36-4ab9-9aad-497ac74bf14a", "Departure Ref.");
            this.DepartureReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 42, true);
            this.DepartureReferenceTextBox.Name = "DepartureReferenceTextBox";
            this.DepartureReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
            this.DepartureReferenceTextBox.TabIndex = 22;
			// 
			// WarehouseCTOTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseTextBox, "Warehouse");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Warehouse)));
            this.WarehouseTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("562a5dfe-151d-4c55-804d-2489914a8726", "Warehouse");
            this.WarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 152, true);
            this.WarehouseTextBox.Name = "WarehouseCTOTextBox";
            this.WarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.WarehouseTextBox.TabIndex = 23;
            // 
            // ShippingLineTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShippingLineTextBox, "ShippingLine");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).ShippingLine)));
            this.ShippingLineTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("0ba90fc7-2a8e-4644-9143-58e7a3396e4d", "Shipping Line");
            this.ShippingLineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 65, true);
            this.ShippingLineTextBox.Name = "ShippingLineTextBox";
            this.ShippingLineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.ShippingLineTextBox.TabIndex = 22;
            // 
            // DestinationTextBox
            // 
            this.BindingSource.SetBindingMember(this.DestinationTextBox, "Destination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Destination)));
            this.DestinationTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("11340786-bb8e-4fbb-be82-6c4b4b4a0324", "Destination");
            this.DestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 129, true);
            this.DestinationTextBox.Name = "DestinationTextBox";
            this.DestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.DestinationTextBox.TabIndex = 21;
            // 
            // DepartureDateEdit
            // 
            this.DepartureDateEdit.AllowDrop = true;
            this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateEdit, "Departure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Departure)));
            this.DepartureDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("e1725c8c-ffe2-4ecd-b712-47dac2946c18", "Departure");
            this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 19, true);
            this.DepartureDateEdit.Name = "DepartureDateEdit";
            this.DepartureDateEdit.TabIndex = 19;
            // 
            // BillNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.BillNoTextBox, "BillNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).BillNo)));
            this.BillNoTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("83b1e99f-5942-47a3-b8df-3658513b77d1", "Bill No");
            this.BillNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 106, true);
            this.BillNoTextBox.Name = "BillNoTextBox";
            this.BillNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.BillNoTextBox.TabIndex = 18;
            // 
            // CarrierBookingRefTextBox
            // 
            this.BindingSource.SetBindingMember(this.CarrierBookingRefTextBox, "BookingReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).BookingReference)));
            this.CarrierBookingRefTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("7284604b-d56f-44b2-b513-a93af9e463a1", "Booking Ref.");
            this.CarrierBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 106, true);
            this.CarrierBookingRefTextBox.Name = "CarrierBookingRefTextBox";
            this.CarrierBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
            this.CarrierBookingRefTextBox.TabIndex = 19;
            // 
            // VoyageNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.VoyageNoTextBox, "VoyageNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).VoyageNo)));
            this.VoyageNoTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("a05de77c-e88b-4752-865f-b564c24e6aed", "Voyage No");
            this.VoyageNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 42, true);
            this.VoyageNoTextBox.Name = "VoyageNoTextBox";
            this.VoyageNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.VoyageNoTextBox.TabIndex = 20;
            // 
            // VesselNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.VesselNameTextBox, "VesselName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).VesselName)));
            this.VesselNameTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("2ac72f0e-f9bf-4be7-8732-ad6c2f7a0b74", "Vessel Name");
            this.VesselNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 19, true);
            this.VesselNameTextBox.Name = "VesselNameTextBox";
            this.VesselNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.VesselNameTextBox.TabIndex = 17;
            // 
            // SenderGroupBox
            // 
            this.SenderGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("cf5217ce-33a5-4c90-8e14-c22df1d9e5d8", "Paying Party");
            this.SenderGroupBox.Controls.Add(this.SenderNameTextBox);
            this.SenderGroupBox.Controls.Add(this.AccountNoTextBox);
            this.SenderGroupBox.Controls.Add(this.SenderCodeTextBox);
            this.SenderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 85, true);
            this.SenderGroupBox.Name = "SenderGroupBox";
            this.SenderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 85, true);
            this.SenderGroupBox.TabIndex = 8;
            this.SenderGroupBox.TabStop = false;
            // 
            // SenderNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.SenderNameTextBox, "SenderName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).SenderName)));
            this.SenderNameTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("87c89cee-033c-4cae-b7e0-2dbdef2b78b3", "Name");
            this.SenderNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 61, true);
            this.SenderNameTextBox.Name = "SenderNameTextBox";
            this.SenderNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.SenderNameTextBox.TabIndex = 11;
            // 
            // AccountNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.AccountNoTextBox, "AccountNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).AccountNo)));
            this.AccountNoTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("d6e2cf6e-6460-493c-a2a1-4e8065d32d5a", "Port Account");
            this.AccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
            this.AccountNoTextBox.Name = "AccountNoTextBox";
            this.AccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.AccountNoTextBox.TabIndex = 10;
            // 
            // SenderCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.SenderCodeTextBox, "SenderCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).SenderCode)));
            this.SenderCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("85d201fd-4755-4332-a28c-5142de24aebd", "Code");
            this.SenderCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
            this.SenderCodeTextBox.Name = "SenderCodeTextBox";
            this.SenderCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.SenderCodeTextBox.TabIndex = 9;
            // 
            // OperatorGroupBox
            // 
            this.OperatorGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("6661c573-0e46-4257-900b-02d1e70cad70", "Operator");
            this.OperatorGroupBox.Controls.Add(this.EmailTextBox);
            this.OperatorGroupBox.Controls.Add(this.FaxTextBox);
            this.OperatorGroupBox.Controls.Add(this.TelephoneTextBox);
            this.OperatorGroupBox.Controls.Add(this.OperatorTextBox);
            this.OperatorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 10, true);
            this.OperatorGroupBox.Name = "OperatorGroupBox";
            this.OperatorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 69, true);
            this.OperatorGroupBox.TabIndex = 3;
            this.OperatorGroupBox.TabStop = false;
            // 
            // EmailTextBox
            // 
            this.BindingSource.SetBindingMember(this.EmailTextBox, "OperatorEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).OperatorEmail)));
            this.EmailTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("ace04e68-2279-4e2f-bcd6-d39c689c0db0", "Email");
            this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 40, true);
            this.EmailTextBox.Name = "EmailTextBox";
            this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.EmailTextBox.TabIndex = 6;
            // 
            // FaxTextBox
            // 
            this.BindingSource.SetBindingMember(this.FaxTextBox, "OperatorFax");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).OperatorFax)));
            this.FaxTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("5c731ad7-92e0-4a23-a5b1-cb355e1ec6fe", "Fax");
            this.FaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 40, true);
            this.FaxTextBox.Name = "FaxTextBox";
            this.FaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.FaxTextBox.TabIndex = 7;
            // 
            // TelephoneTextBox
            // 
            this.BindingSource.SetBindingMember(this.TelephoneTextBox, "OperatorPhone");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).OperatorPhone)));
            this.TelephoneTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("23143922-22e7-4aa7-a098-f809700cbfec", "Telephone");
            this.TelephoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 14, true);
            this.TelephoneTextBox.Name = "TelephoneTextBox";
            this.TelephoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.TelephoneTextBox.TabIndex = 5;
            // 
            // OperatorTextBox
            // 
            this.BindingSource.SetBindingMember(this.OperatorTextBox, "Operator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Operator)));
            this.OperatorTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("198b31f0-8b57-446d-9d9e-05a132e7825b", "Name");
            this.OperatorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 14, true);
            this.OperatorTextBox.Name = "OperatorTextBox";
            this.OperatorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.OperatorTextBox.TabIndex = 4;
            // 
            // TerminalBerthShedTextBox
            // 
            this.BindingSource.SetBindingMember(this.TerminalBerthShedTextBox, "Berth");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Berth)));
            this.TerminalBerthShedTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("0b8f8789-b876-4886-ad1c-765a73a73745", "Terminal/Berth/Shed");
            this.TerminalBerthShedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 50, true);
            this.TerminalBerthShedTextBox.Name = "TerminalBerthShedTextBox";
            this.TerminalBerthShedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.TerminalBerthShedTextBox.TabIndex = 2;
            // 
            // DateEdit
            // 
            this.DateEdit.AllowDrop = true;
            this.DateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DateEdit, "Date");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).Date)));
            this.DateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("3b7d2f12-61bb-4959-af60-40d9e4fd4b8d", "Date");
            this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 24, true);
            this.DateEdit.Name = "DateEdit";
            this.DateEdit.TabIndex = 0;
			// 
			// ShipperEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperEORITextBox, "ShipperEORI");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).ShipperEORI)));
			this.ShipperEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 129, true);
            this.ShipperEORITextBox.Name = "shipperEORITextBox";
            this.ShipperEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
            this.ShipperEORITextBox.TabIndex = 24;
            // 
            // AgentEORITextBox
            // 
            this.BindingSource.SetBindingMember(this.AgentEORITextBox, "AgentEORI");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(null)).AgentEORI)));
            this.AgentEORITextBox.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("7DF2E87C-1D03-42E0-AE7E-7CDDDE66DDBD", "Agent EORI");
            this.AgentEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 152, true);
            this.AgentEORITextBox.Name = "agentEORITextBox";
            this.AgentEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
            this.AgentEORITextBox.TabIndex = 25;
            // 
            // PortMessagingConsolDataControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PortOrderGroupBox);
            this.Name = "PortMessagingConsolDataControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 270, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PortOrderGroupBox.ResumeLayout(false);
            this.PortOrderGroupBox.PerformLayout();
            this.ShipperGroupBox.ResumeLayout(false);
            this.ShipperGroupBox.PerformLayout();
            this.ConsolDataGroupBox.ResumeLayout(false);
            this.ConsolDataGroupBox.PerformLayout();
            this.DepartureDateEdit.ResumeLayout(true);
            this.DepartureDateEdit.PerformLayout();
            this.SenderGroupBox.ResumeLayout(false);
            this.SenderGroupBox.PerformLayout();
            this.OperatorGroupBox.ResumeLayout(false);
            this.OperatorGroupBox.PerformLayout();
            this.ResumeLayout(false);

		}

		private ZArchitecture.GUI.ZGroupBox PortOrderGroupBox;
		private ZArchitecture.GUI.ZDateEdit DateEdit;
		private ZArchitecture.ZTextBox TerminalBerthShedTextBox;
		private ZArchitecture.GUI.ZGroupBox OperatorGroupBox;
		private ZArchitecture.ZTextBox EmailTextBox;
		private ZArchitecture.ZTextBox FaxTextBox;
		private ZArchitecture.ZTextBox TelephoneTextBox;
		private ZArchitecture.ZTextBox OperatorTextBox;
		private ZArchitecture.GUI.ZGroupBox SenderGroupBox;
		private ZArchitecture.ZTextBox SenderNameTextBox;
		private ZArchitecture.ZTextBox AccountNoTextBox;
		private ZArchitecture.ZTextBox SenderCodeTextBox;
		private ZArchitecture.GUI.ZGroupBox ConsolDataGroupBox;
		private ZArchitecture.ZTextBox DestinationTextBox;
		private ZArchitecture.GUI.ZDateEdit DepartureDateEdit;
		private ZArchitecture.ZTextBox BillNoTextBox;
		private ZArchitecture.ZTextBox CarrierBookingRefTextBox;
		private ZArchitecture.ZTextBox VoyageNoTextBox;
		private ZArchitecture.ZTextBox VesselNameTextBox;
		private ZArchitecture.GUI.ZGroupBox ShipperGroupBox;
		private ZArchitecture.ZTextBox ShipperPortAccountTextBox;
		private ZArchitecture.ZTextBox ShipperNameTextBox;
		private ZArchitecture.ZTextBox ShipperCodeTextBox;
		private ZArchitecture.ZTextBox ShippingLineTextBox;
		private ZArchitecture.ZTextBox WarehouseTextBox;
		private ZArchitecture.ZTextBox DepartureReferenceTextBox;
		private ZArchitecture.ZTextBox AgentEORITextBox;
		private ZArchitecture.ZTextBox ShipperEORITextBox;
	}
}
