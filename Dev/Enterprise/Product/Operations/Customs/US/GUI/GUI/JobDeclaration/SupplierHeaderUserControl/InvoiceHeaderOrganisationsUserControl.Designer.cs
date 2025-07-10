namespace Enterprise.Customs.US.GUI
{
	partial class InvoiceHeaderOrganisationsUserControl
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
			this.JZ_OA_ManufacturerAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_SupplierAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.InvoicerDocAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.AIIBuyingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AIIBuyerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AIIExporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AIIImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ACEForeignExporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_SellerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.AIISellingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JZ_OA_ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_SoldToPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_ShipToPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_DistributorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_PackagerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JZ_OA_ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JZ_OA_ManufacturerAddressAddressControl.SuspendLayout();
			this.JZ_OA_SupplierAddressAddressControl.SuspendLayout();
			this.InvoicerDocAddressAddressControl.SuspendLayout();
			this.AIIBuyingAgentGuidFindBox.SuspendLayout();
			this.AIIBuyerGuidFindBox.SuspendLayout();
			this.AIIExporterGuidFindBox.SuspendLayout();
			this.AIIImporterGuidFindBox.SuspendLayout();
			this.ACEForeignExporterAddressControl.SuspendLayout();
			this.JZ_OA_SellerAddressControl.SuspendLayout();
			this.AIISellingAgentGuidFindBox.SuspendLayout();
			this.JZ_OA_ConsigneeAddressControl.SuspendLayout();
			this.JZ_OA_SoldToPartyAddressControl.SuspendLayout();
			this.JZ_OA_ShipToPartyAddressControl.SuspendLayout();
			this.JZ_OA_DistributorAddressControl.SuspendLayout();
			this.JZ_OA_PackagerAddressControl.SuspendLayout();
			this.JZ_OA_ShipperAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.IInvoicesProvider);
			// 
			// JZ_OA_ManufacturerAddressAddressControl
			// 
			this.JZ_OA_ManufacturerAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_ManufacturerAddressAddressControl, "FilteredInvoices.JZ_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_ManufacturerAddress)));
			this.JZ_OA_ManufacturerAddressAddressControl.BindToOrgList = "FilteredInvoices.Lookups+SupplierList";
			this.JZ_OA_ManufacturerAddressAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|6544c5bf-deb3-41d0-bce7-c708104a10be", "Manufacturer");
			this.JZ_OA_ManufacturerAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 3, true);
			this.JZ_OA_ManufacturerAddressAddressControl.Name = "JZ_OA_ManufacturerAddressAddressControl";
			this.JZ_OA_ManufacturerAddressAddressControl.PopupCaption = "";
			this.JZ_OA_ManufacturerAddressAddressControl.ReadOnly = false;
			this.JZ_OA_ManufacturerAddressAddressControl.ShowAddress = false;
			this.JZ_OA_ManufacturerAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_ManufacturerAddressAddressControl.TabIndex = 21;
			// 
			// JZ_OA_SupplierAddressAddressControl
			// 
			this.JZ_OA_SupplierAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_SupplierAddressAddressControl, "FilteredInvoices.JZ_OA_SupplierAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_SupplierAddress)));
			this.JZ_OA_SupplierAddressAddressControl.BindToOrgList = "FilteredInvoices.Lookups+SupplierList";
			this.JZ_OA_SupplierAddressAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|018631ce-e53d-4d82-b1bc-2bb92ebbda4b", "Supplier");
			this.JZ_OA_SupplierAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 29, true);
			this.JZ_OA_SupplierAddressAddressControl.Name = "JZ_OA_SupplierAddressAddressControl";
			this.JZ_OA_SupplierAddressAddressControl.PopupCaption = "";
			this.JZ_OA_SupplierAddressAddressControl.ReadOnly = false;
			this.JZ_OA_SupplierAddressAddressControl.ShowAddress = false;
			this.JZ_OA_SupplierAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_SupplierAddressAddressControl.TabIndex = 23;
			// 
			// InvoicerDocAddressAddressControl
			// 
			this.InvoicerDocAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoicerDocAddressAddressControl, "FilteredInvoices.JZ_OA_InvoicerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_InvoicerDocAddress)));
			this.InvoicerDocAddressAddressControl.BindToOrgList = "FilteredInvoices.Lookups+SupplierList";
			this.InvoicerDocAddressAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|b2916025-f272-4ebf-a001-cf0688ad42dc", "Invoicing Org.", "Invoicing Organization", "");
			this.InvoicerDocAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 80, true);
			this.InvoicerDocAddressAddressControl.Name = "InvoicerDocAddressAddressControl";
			this.InvoicerDocAddressAddressControl.PopupCaption = "";
			this.InvoicerDocAddressAddressControl.ReadOnly = false;
			this.InvoicerDocAddressAddressControl.ShowAddress = false;
			this.InvoicerDocAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.InvoicerDocAddressAddressControl.TabIndex = 27;
			// 
			// AIIBuyingAgentGuidFindBox
			// 
			this.AIIBuyingAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIBuyingAgentGuidFindBox, "FilteredInvoices.JZ_OH_BuyerAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OH_BuyerAgent)));
			this.AIIBuyingAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|b2acbb8c-06f5-485c-a431-f971075f1d17", "Buying Agent");
			this.AIIBuyingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 206, true);
			this.AIIBuyingAgentGuidFindBox.Name = "AIIBuyingAgentGuidFindBox";
			this.AIIBuyingAgentGuidFindBox.PopupCaption = null;
			this.AIIBuyingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.AIIBuyingAgentGuidFindBox.TabIndex = 32;
			// 
			// AIIBuyerGuidFindBox
			// 
			this.AIIBuyerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIBuyerGuidFindBox, "FilteredInvoices.BuyerOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).BuyerOrgPK)));
			this.AIIBuyerGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|3e2fa946-41cf-4fcc-8e61-b596cedf104d", "Buyer");
			this.AIIBuyerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 181, true);
			this.AIIBuyerGuidFindBox.Name = "AIIBuyerGuidFindBox";
			this.AIIBuyerGuidFindBox.PopupCaption = null;
			this.AIIBuyerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.AIIBuyerGuidFindBox.TabIndex = 31;
			// 
			// AIIExporterGuidFindBox
			// 
			this.AIIExporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIExporterGuidFindBox, "FilteredInvoices.ExporterOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).ExporterOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).AddInfoLookups.Forwarders)));
			this.AIIExporterGuidFindBox.BindToList = "FilteredInvoices.AddInfoLookups+Forwarders";
			this.AIIExporterGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|3b3eda13-3d13-4d73-ada8-1ca7bc5a99bd", "Exporter");
			this.AIIExporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 55, true);
			this.AIIExporterGuidFindBox.Name = "AIIExporterGuidFindBox";
			this.AIIExporterGuidFindBox.PopupCaption = null;
			this.AIIExporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.AIIExporterGuidFindBox.TabIndex = 25;
			// 
			// AIIImporterGuidFindBox
			// 
			this.AIIImporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIImporterGuidFindBox, "FilteredInvoices.JZ_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OH_Buyer)));
			this.AIIImporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 156, true);
			this.AIIImporterGuidFindBox.Name = "AIIImporterGuidFindBox";
			this.AIIImporterGuidFindBox.PopupCaption = null;
			this.AIIImporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.AIIImporterGuidFindBox.TabIndex = 30;
			// 
			// ACEForeignExporterAddressControl
			// 
			this.ACEForeignExporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEForeignExporterAddressControl, "FilteredInvoices.JZ_OA_ExporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_ExporterAddress)));
			this.ACEForeignExporterAddressControl.BindToOrgList = "FilteredInvoices.Lookups+SupplierList";
			this.ACEForeignExporterAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderOrganisationsUserControl|6544c5bf-deb3-41d0-bce7-c708104a10bf", "Exporter");
			this.ACEForeignExporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 55, true);
			this.ACEForeignExporterAddressControl.Name = "ACEForeignExporterAddressControl";
			this.ACEForeignExporterAddressControl.PopupCaption = "";
			this.ACEForeignExporterAddressControl.ReadOnly = false;
			this.ACEForeignExporterAddressControl.ShowAddress = false;
			this.ACEForeignExporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ACEForeignExporterAddressControl.TabIndex = 24;
			// 
			// JZ_OA_SellerAddressControl
			// 
			this.JZ_OA_SellerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_SellerAddressControl, "FilteredInvoices.JZ_OA_SellerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_SellerAddress)));
			this.JZ_OA_SellerAddressControl.BindToOrgList = "FilteredInvoices.AddInfoLookups+Consignors";
			this.JZ_OA_SellerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8c8259c4-963a-4fa5-a367-661d73f213d0", "Seller");
			this.JZ_OA_SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 105, true);
			this.JZ_OA_SellerAddressControl.Name = "JZ_OA_SellerAddressControl";
			this.JZ_OA_SellerAddressControl.PopupCaption = "";
			this.JZ_OA_SellerAddressControl.ReadOnly = false;
			this.JZ_OA_SellerAddressControl.ShowAddress = false;
			this.JZ_OA_SellerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_SellerAddressControl.TabIndex = 28;
			// 
			// AIISellingAgentGuidFindBox
			// 
			this.AIISellingAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIISellingAgentGuidFindBox, "FilteredInvoices.JZ_OH_SellingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OH_SellingAgent)));
			this.AIISellingAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b4a7df7b-a3c7-44f1-9f6b-1bef299dcb7b", "Selling Agent");
			this.AIISellingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 131, true);
			this.AIISellingAgentGuidFindBox.Name = "AIISellingAgentGuidFindBox";
			this.AIISellingAgentGuidFindBox.PopupCaption = null;
			this.AIISellingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.AIISellingAgentGuidFindBox.TabIndex = 29;
			// 
			// US_OA_UltimateConsigneeAddressControl
			// 
			this.JZ_OA_ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_ConsigneeAddressControl, "FilteredInvoices.JZ_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_ConsigneeAddress)));
			this.JZ_OA_ConsigneeAddressControl.BindToOrgList = "FilteredInvoices.AddInfoLookups+Consignees";
			this.JZ_OA_ConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("504b9f2c-affa-4fdb-9f78-92c16eddce07", "Ultimate Consignee");
			this.JZ_OA_ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 230, true);
			this.JZ_OA_ConsigneeAddressControl.Name = "JZ_OA_ConsigneeAddressControl";
			this.JZ_OA_ConsigneeAddressControl.PopupCaption = "";
			this.JZ_OA_ConsigneeAddressControl.ReadOnly = false;
			this.JZ_OA_ConsigneeAddressControl.ShowAddress = false;
			this.JZ_OA_ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_ConsigneeAddressControl.TabIndex = 33;
			// 
			// JZ_OA_SoldToPartyAddressControl
			// 
			this.JZ_OA_SoldToPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_SoldToPartyAddressControl, "FilteredInvoices.JZ_OA_SoldToPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_SoldToPartyAddress)));
			this.JZ_OA_SoldToPartyAddressControl.BindToOrgList = "FilteredInvoices.AddInfoLookups+Consignees";
			this.JZ_OA_SoldToPartyAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d0f01cb0-3a46-4d4d-bd1e-466f3ee5f4c8", "Sold To Party");
			this.JZ_OA_SoldToPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 255, true);
			this.JZ_OA_SoldToPartyAddressControl.Name = "JZ_OA_SoldToPartyAddressControl";
			this.JZ_OA_SoldToPartyAddressControl.PopupCaption = "";
			this.JZ_OA_SoldToPartyAddressControl.ReadOnly = false;
			this.JZ_OA_SoldToPartyAddressControl.ShowAddress = false;
			this.JZ_OA_SoldToPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_SoldToPartyAddressControl.TabIndex = 34;
			// 
			// JZ_OA_ShipToPartyAddressControl
			// 
			this.JZ_OA_ShipToPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_ShipToPartyAddressControl, "FilteredInvoices.JZ_OA_ShipToPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_ShipToPartyAddress)));
			this.JZ_OA_ShipToPartyAddressControl.BindToOrgList = "FilteredInvoices.AddInfoLookups+Consignees";
			this.JZ_OA_ShipToPartyAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5ab0ac5b-22d7-4c92-94ff-a5e5a7f01137", "Ship To Party");
			this.JZ_OA_ShipToPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 280, true);
			this.JZ_OA_ShipToPartyAddressControl.Name = "JZ_OA_ShipToPartyAddressControl";
			this.JZ_OA_ShipToPartyAddressControl.PopupCaption = "";
			this.JZ_OA_ShipToPartyAddressControl.ReadOnly = false;
			this.JZ_OA_ShipToPartyAddressControl.ShowAddress = false;
			this.JZ_OA_ShipToPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_ShipToPartyAddressControl.TabIndex = 35;
			// 
			// JZ_OA_ShipperAddressControl
			// 
			this.JZ_OA_ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_ShipperAddressControl, "FilteredInvoices.JZ_OA_ShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_ShipperAddress)));
			this.JZ_OA_ShipperAddressControl.BindToOrgList = "FilteredInvoices.Lookups+Shippers";
			this.JZ_OA_ShipperAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("98d1a2a2-366f-4d92-9956-2bca38a06bd9", "Shipper");
			this.JZ_OA_ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 80, true);
			this.JZ_OA_ShipperAddressControl.Name = "JZ_OA_ShipperAddressControl";
			this.JZ_OA_ShipperAddressControl.PopupCaption = "";
			this.JZ_OA_ShipperAddressControl.ReadOnly = false;
			this.JZ_OA_ShipperAddressControl.ShowAddress = false;
			this.JZ_OA_ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_ShipperAddressControl.TabIndex = 27;
			// 
			// JZ_OA_DistributorAddressControl
			// 
			this.JZ_OA_DistributorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_DistributorAddressControl, "FilteredInvoices.JZ_OA_DistributorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_DistributorAddress)));
			this.JZ_OA_DistributorAddressControl.BindToOrgList = "FilteredInvoices.Lookups+Distributors";
			this.JZ_OA_DistributorAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89057034-c600-445c-8ca2-250d4fd2f20f", "Distributor");
			this.JZ_OA_DistributorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 131, true);
			this.JZ_OA_DistributorAddressControl.Name = "JZ_OA_DistributorAddressControl";
			this.JZ_OA_DistributorAddressControl.PopupCaption = "";
			this.JZ_OA_DistributorAddressControl.ReadOnly = false;
			this.JZ_OA_DistributorAddressControl.ShowAddress = false;
			this.JZ_OA_DistributorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_DistributorAddressControl.TabIndex = 29;
			// 
			// JZ_OA_PackagerAddressControl
			// 
			this.JZ_OA_PackagerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_OA_PackagerAddressControl, "FilteredInvoices.JZ_OA_PackagerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_OA_PackagerAddress)));
			this.JZ_OA_PackagerAddressControl.BindToOrgList = "FilteredInvoices.Lookups+Packagers";
			this.JZ_OA_PackagerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("149dba1f-d619-4ba1-84c9-9b5e67ccd035", "Packager");
			this.JZ_OA_PackagerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 206, true);
			this.JZ_OA_PackagerAddressControl.Name = "JZ_OA_PackagerAddressControl";
			this.JZ_OA_PackagerAddressControl.PopupCaption = "";
			this.JZ_OA_PackagerAddressControl.ReadOnly = false;
			this.JZ_OA_PackagerAddressControl.ShowAddress = false;
			this.JZ_OA_PackagerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.JZ_OA_PackagerAddressControl.TabIndex = 32;
			// 
			// InvoiceHeaderOrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JZ_OA_PackagerAddressControl);
			this.Controls.Add(this.JZ_OA_DistributorAddressControl);
			this.Controls.Add(this.JZ_OA_ShipperAddressControl);
			this.Controls.Add(this.JZ_OA_ShipToPartyAddressControl);
			this.Controls.Add(this.JZ_OA_SoldToPartyAddressControl);
			this.Controls.Add(this.JZ_OA_ConsigneeAddressControl);
			this.Controls.Add(this.JZ_OA_SellerAddressControl);
			this.Controls.Add(this.JZ_OA_ManufacturerAddressAddressControl);
			this.Controls.Add(this.JZ_OA_SupplierAddressAddressControl);
			this.Controls.Add(this.AIISellingAgentGuidFindBox);
			this.Controls.Add(this.InvoicerDocAddressAddressControl);
			this.Controls.Add(this.AIIBuyingAgentGuidFindBox);
			this.Controls.Add(this.AIIBuyerGuidFindBox);
			this.Controls.Add(this.AIIImporterGuidFindBox);
			this.Controls.Add(this.ACEForeignExporterAddressControl);
			this.Controls.Add(this.AIIExporterGuidFindBox);
			this.Name = "InvoiceHeaderOrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 301, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JZ_OA_ManufacturerAddressAddressControl.ResumeLayout(true);
			this.JZ_OA_ManufacturerAddressAddressControl.PerformLayout();
			this.JZ_OA_SupplierAddressAddressControl.ResumeLayout(true);
			this.JZ_OA_SupplierAddressAddressControl.PerformLayout();
			this.InvoicerDocAddressAddressControl.ResumeLayout(true);
			this.InvoicerDocAddressAddressControl.PerformLayout();
			this.AIIBuyingAgentGuidFindBox.ResumeLayout(true);
			this.AIIBuyingAgentGuidFindBox.PerformLayout();
			this.AIIBuyerGuidFindBox.ResumeLayout(true);
			this.AIIBuyerGuidFindBox.PerformLayout();
			this.AIIExporterGuidFindBox.ResumeLayout(true);
			this.AIIExporterGuidFindBox.PerformLayout();
			this.AIIImporterGuidFindBox.ResumeLayout(true);
			this.AIIImporterGuidFindBox.PerformLayout();
			this.ACEForeignExporterAddressControl.ResumeLayout(true);
			this.ACEForeignExporterAddressControl.PerformLayout();
			this.JZ_OA_SellerAddressControl.ResumeLayout(true);
			this.JZ_OA_SellerAddressControl.PerformLayout();
			this.AIISellingAgentGuidFindBox.ResumeLayout(true);
			this.AIISellingAgentGuidFindBox.PerformLayout();
			this.JZ_OA_ConsigneeAddressControl.ResumeLayout(true);
			this.JZ_OA_ConsigneeAddressControl.PerformLayout();
			this.JZ_OA_SoldToPartyAddressControl.ResumeLayout(true);
			this.JZ_OA_SoldToPartyAddressControl.PerformLayout();
			this.JZ_OA_ShipToPartyAddressControl.ResumeLayout(true);
			this.JZ_OA_ShipToPartyAddressControl.PerformLayout();
			this.JZ_OA_DistributorAddressControl.ResumeLayout(true);
			this.JZ_OA_DistributorAddressControl.PerformLayout();
			this.JZ_OA_PackagerAddressControl.ResumeLayout(true);
			this.JZ_OA_PackagerAddressControl.PerformLayout();
			this.JZ_OA_ShipperAddressControl.ResumeLayout(true);
			this.JZ_OA_ShipperAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZAddressControl JZ_OA_ManufacturerAddressAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl JZ_OA_SupplierAddressAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl InvoicerDocAddressAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox AIIBuyingAgentGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox AIIBuyerGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox AIIExporterGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox AIIImporterGuidFindBox;
		internal ZArchitecture.GUI.ZAddressControl ACEForeignExporterAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_SellerAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox AIISellingAgentGuidFindBox;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_ConsigneeAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_SoldToPartyAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_ShipToPartyAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_DistributorAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_PackagerAddressControl;
		internal ZArchitecture.GUI.ZAddressControl JZ_OA_ShipperAddressControl;
	}
}
