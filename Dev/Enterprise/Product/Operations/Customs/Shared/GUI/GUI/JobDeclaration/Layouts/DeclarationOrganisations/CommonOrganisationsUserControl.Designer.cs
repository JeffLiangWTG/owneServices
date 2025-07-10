namespace Enterprise.Customs.GUI
{
	partial class CommonOrganisationsUserControl
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
		private void InitializeComponent()
		{
			this.ExternalBrokerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ControllingCustomerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ControllingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BondedWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContainerYardAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DepotAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContainerTerminalOperatorAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForwarderOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ShippingOrAirLineOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RepresentativeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeclarantOfficeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SellerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.BuyerOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SoldToPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ConsigneeOrganisationGuidFindBox.SuspendLayout();
			this.ForwarderOrganisationGuidFindBox.SuspendLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.BuyerOrganisationGuidFindBox.SuspendLayout();
			this.SoldToPartyAddressControl.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ExternalBrokerGuidFindBox
			// 
			this.ExternalBrokerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExternalBrokerGuidFindBox, "JE_OH_ExternalBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ExternalBroker)));
			this.ExternalBrokerGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fe78ca2f-c9a6-4ef6-915f-4a910916430f", "External Broker");
			this.ExternalBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 229, true);
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
			this.ControllingCustomerGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("802413f8-6747-4476-9686-533978354764", "Controlling Customer");
			this.ControllingCustomerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 203, true);
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
			this.ControllingAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9f2cf91a-8deb-4e70-bab4-521842f38a3c", "Controlling Agent");
			this.ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 177, true);
			this.ControllingAgentGuidFindBox.Name = "ControllingAgentGuidFindBox";
			this.ControllingAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ControllingAgentGuidFindBox.ParentType = null;
			this.ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ControllingAgentGuidFindBox.TabIndex = 6;
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
			this.BondedWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 151, true);
			this.BondedWarehouseDocAddressControl.Name = "BondedWarehouseDocAddressControl";
			this.BondedWarehouseDocAddressControl.ReadOnly = false;
			this.BondedWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.BondedWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BondedWarehouseDocAddressControl.TabIndex = 5;
			this.BondedWarehouseDocAddressControl.ValidationJustForced = false;
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
			this.ContainerYardAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 125, true);
			this.ContainerYardAddressControl.Name = "ContainerYardAddressControl";
			this.ContainerYardAddressControl.ReadOnly = false;
			this.ContainerYardAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContainerYardAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContainerYardAddressControl.TabIndex = 4;
			this.ContainerYardAddressControl.ValidationJustForced = false;
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
			this.DepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 99, true);
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
			this.ContainerTerminalOperatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 73, true);
			this.ContainerTerminalOperatorAddressControl.Name = "ContainerTerminalOperatorAddressControl";
			this.ContainerTerminalOperatorAddressControl.ReadOnly = false;
			this.ContainerTerminalOperatorAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ContainerTerminalOperatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ContainerTerminalOperatorAddressControl.TabIndex = 2;
			this.ContainerTerminalOperatorAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeOrganisationGuidFindBox
			// 
			this.ConsigneeOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeOrganisationGuidFindBox, "JE_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Consignee)));
			this.ConsigneeOrganisationGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|5EC79F39-C373-4757-B06B-EA9AE1C0A72F", "Consignee");
			this.ConsigneeOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 21, true);
			this.ConsigneeOrganisationGuidFindBox.Name = "ConsigneeOrganisationGuidFindBox";
			this.ConsigneeOrganisationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsigneeOrganisationGuidFindBox.ParentType = null;
			this.ConsigneeOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ConsigneeOrganisationGuidFindBox.TabIndex = 16;
			// 
			// ForwarderOrganisationGuidFindBox
			// 
			this.ForwarderOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderOrganisationGuidFindBox, "JE_OH_Forwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Forwarder)));
			this.ForwarderOrganisationGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|14bd31a9-397d-4908-ac98-c9a2c6274a24", "Forwarder");
			this.ForwarderOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 47, true);
			this.ForwarderOrganisationGuidFindBox.Name = "ForwarderOrganisationGuidFindBox";
			this.ForwarderOrganisationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ForwarderOrganisationGuidFindBox.ParentType = null;
			this.ForwarderOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ForwarderOrganisationGuidFindBox.TabIndex = 1;
			// 
			// ShippingOrAirLineOrganisationGuidFindBox
			// 
			this.ShippingOrAirLineOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingOrAirLineOrganisationGuidFindBox, "JE_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_ShippingLine)));
			this.ShippingOrAirLineOrganisationGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|c5409f2b-6e62-468c-bf33-57865188ae5e", "Carrier", "Carrier", "Carrier", "");
			this.ShippingOrAirLineOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 21, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.Name = "ShippingOrAirLineOrganisationGuidFindBox";
			this.ShippingOrAirLineOrganisationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ShippingOrAirLineOrganisationGuidFindBox.ParentType = null;
			this.ShippingOrAirLineOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.TabIndex = 0;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "JE_OA_Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_Representative)));
			this.RepresentativeAddressControl.BindToOrgList = "Lookups+RepresentativeList";
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 255, true);
			this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
			this.RepresentativeAddressControl.PopupCaption = "";
			this.RepresentativeAddressControl.ReadOnly = false;
			this.RepresentativeAddressControl.ShowAddress = false;
			this.RepresentativeAddressControl.ShowOrganisationName = true;
			this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.RepresentativeAddressControl.TabIndex = 9;
			// 
			// DeclarantOfficeAddressControl
			// 
			this.DeclarantOfficeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantOfficeAddressControl, "JE_OA_DeclarantAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_DeclarantAddress)));
			this.DeclarantOfficeAddressControl.BindToOrgList = "Lookups+DeclarantOfficeList";
			this.DeclarantOfficeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 281, true);
			this.DeclarantOfficeAddressControl.Name = "DeclarantOfficeAddressControl";
			this.DeclarantOfficeAddressControl.PopupCaption = "";
			this.DeclarantOfficeAddressControl.ReadOnly = false;
			this.DeclarantOfficeAddressControl.ShowAddress = false;
			this.DeclarantOfficeAddressControl.ShowOrganisationName = true;
			this.DeclarantOfficeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.DeclarantOfficeAddressControl.TabIndex = 10;
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerAddressControl, "JE_OA_SellerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_SellerAddress)));
			this.SellerAddressControl.BindToOrgList = "Lookups+SellerList";
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 307, true);
			this.SellerAddressControl.Name = "SellerAddressControl";
			this.SellerAddressControl.PopupCaption = "";
			this.SellerAddressControl.ReadOnly = false;
			this.SellerAddressControl.ShowAddress = false;
			this.SellerAddressControl.ShowOrganisationName = true;
			this.SellerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.SellerAddressControl.TabIndex = 11;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "JE_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 333, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.ShowOrganisationName = true;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ManufacturerAddressControl.TabIndex = 12;
			// 
			// BuyerOrganisationGuidFindBox
			// 
			this.BuyerOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerOrganisationGuidFindBox, "JE_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Buyer)));
			this.BuyerOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 411, true);
			this.BuyerOrganisationGuidFindBox.Name = "BuyerOrganisationGuidFindBox";
			this.BuyerOrganisationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BuyerOrganisationGuidFindBox.ParentType = null;
			this.BuyerOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BuyerOrganisationGuidFindBox.TabIndex = 15;
			// 
			// SoldToPartyAddressControl
			// 
			this.SoldToPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SoldToPartyAddressControl, "JE_OA_SoldToPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_SoldToPartyAddress)));
			this.SoldToPartyAddressControl.BindToOrgList = "Lookups+ConsigneeOrganisations";
			this.SoldToPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 359, true);
			this.SoldToPartyAddressControl.Name = "SoldToPartyAddressControl";
			this.SoldToPartyAddressControl.PopupCaption = "";
			this.SoldToPartyAddressControl.ReadOnly = false;
			this.SoldToPartyAddressControl.ShowAddress = false;
			this.SoldToPartyAddressControl.ShowOrganisationName = true;
			this.SoldToPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.SoldToPartyAddressControl.TabIndex = 13;
			// 
			// ConsigneeAddressControl
			// 
			this.ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressControl, "JE_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_ConsigneeAddress)));
			this.ConsigneeAddressControl.BindToOrgList = "Lookups+ConsigneeList";
			this.ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 385, true);
			this.ConsigneeAddressControl.Name = "ConsigneeAddressControl";
			this.ConsigneeAddressControl.PopupCaption = "";
			this.ConsigneeAddressControl.ReadOnly = false;
			this.ConsigneeAddressControl.ShowAddress = false;
			this.ConsigneeAddressControl.ShowOrganisationName = true;
			this.ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ConsigneeAddressControl.TabIndex = 14;
			// 
			// CommonOrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BuyerOrganisationGuidFindBox);
			this.Controls.Add(this.ExternalBrokerGuidFindBox);
			this.Controls.Add(this.ControllingCustomerGuidFindBox);
			this.Controls.Add(this.ControllingAgentGuidFindBox);
			this.Controls.Add(this.BondedWarehouseDocAddressControl);
			this.Controls.Add(this.ContainerYardAddressControl);
			this.Controls.Add(this.DepotAddressControl);
			this.Controls.Add(this.ContainerTerminalOperatorAddressControl);
			this.Controls.Add(this.ForwarderOrganisationGuidFindBox);
			this.Controls.Add(this.ShippingOrAirLineOrganisationGuidFindBox);
			this.Controls.Add(this.RepresentativeAddressControl);
			this.Controls.Add(this.DeclarantOfficeAddressControl);
			this.Controls.Add(this.SellerAddressControl);
			this.Controls.Add(this.ManufacturerAddressControl);
			this.Controls.Add(this.SoldToPartyAddressControl);
			this.Controls.Add(this.ConsigneeAddressControl);
			this.Controls.Add(this.ConsigneeOrganisationGuidFindBox);
			this.Name = "CommonOrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ConsigneeOrganisationGuidFindBox.ResumeLayout(true);
			this.ConsigneeOrganisationGuidFindBox.PerformLayout();
			this.ForwarderOrganisationGuidFindBox.ResumeLayout(true);
			this.ForwarderOrganisationGuidFindBox.PerformLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationGuidFindBox.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.BuyerOrganisationGuidFindBox.ResumeLayout(true);
			this.BuyerOrganisationGuidFindBox.PerformLayout();
			this.SoldToPartyAddressControl.ResumeLayout(true);
			this.SoldToPartyAddressControl.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox ExternalBrokerGuidFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox ControllingCustomerGuidFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox ControllingAgentGuidFindBox;
		internal MasterFiles.GUI.ZDocAddressControl BondedWarehouseDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ContainerYardAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl DepotAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ContainerTerminalOperatorAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox ForwarderOrganisationGuidFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox ShippingOrAirLineOrganisationGuidFindBox;
		internal ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		internal ZArchitecture.GUI.ZAddressControl DeclarantOfficeAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl SellerAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox BuyerOrganisationGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl SoldToPartyAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ConsigneeAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox ConsigneeOrganisationGuidFindBox;
	}
}
