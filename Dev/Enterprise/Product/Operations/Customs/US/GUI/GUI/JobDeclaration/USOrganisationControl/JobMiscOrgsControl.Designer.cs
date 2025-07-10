namespace Enterprise.Customs.US.GUI
{
	partial class JobMiscOrgsControl
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
			this.CBPBrokerOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExporterOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.InvoicerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ExternalBrokerOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SellingAgentOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BuyerOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BuyerAgentOrgControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SellerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ControllingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ControllingCustomerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DistributorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PackagerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CBPBrokerOrgControl.SuspendLayout();
			this.ExporterOrgControl.SuspendLayout();
			this.InvoicerAddressControl.SuspendLayout();
			this.ExternalBrokerOrgControl.SuspendLayout();
			this.SellingAgentOrgControl.SuspendLayout();
			this.BuyerOrgControl.SuspendLayout();
			this.BuyerAgentOrgControl.SuspendLayout();
			this.SellerAddressControl.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ShipperAddressControl.SuspendLayout();
			this.DistributorAddressControl.SuspendLayout();
			this.PackagerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// CBPBrokerOrgControl
			// 
			this.CBPBrokerOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CBPBrokerOrgControl, "JE_OH_CBPBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_CBPBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.Brokers)));
			this.CBPBrokerOrgControl.BindToList = "AddInfoLookups+Brokers";
			this.CBPBrokerOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("43ab82a1-0a6a-4ea5-8ac1-c74f711de967", "CBP Broker");
			this.CBPBrokerOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.CBPBrokerOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.CBPBrokerOrgControl.Name = "CBPBrokerOrgControl";
			this.CBPBrokerOrgControl.ShouldResize = true;
			this.CBPBrokerOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.CBPBrokerOrgControl.TabIndex = 0;
			// 
			// ExporterOrgControl
			// 
			this.ExporterOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterOrgControl, "JE_OH_Exporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Exporter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.Forwarders)));
			this.ExporterOrgControl.BindToList = "AddInfoLookups+Consignors";
			this.ExporterOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("94deb84b-4d54-43fd-8e65-201fceff21b1", "Exporter");
			this.ExporterOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.ExporterOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 24, true);
			this.ExporterOrgControl.Name = "ExporterOrgControl";
			this.ExporterOrgControl.ShouldResize = true;
			this.ExporterOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ExporterOrgControl.TabIndex = 1;
			// 
			// InvoicerAddressControl
			// 
			this.InvoicerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoicerAddressControl, "JE_OA_InvoicerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_InvoicerAddress)));
			this.InvoicerAddressControl.BindToOrgList = "Lookups+Suppliers";
			this.InvoicerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6530f0ec-78c3-42da-8265-43ed50a45ab3", "Invoicer");
			this.InvoicerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.InvoicerAddressControl.Name = "InvoicerAddressControl";
			this.InvoicerAddressControl.PopupCaption = "";
			this.InvoicerAddressControl.ReadOnly = false;
			this.InvoicerAddressControl.ShowAddress = false;
			this.InvoicerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.InvoicerAddressControl.TabIndex = 5;
			// 
			// ExternalBrokerOrgControl
			// 
			this.ExternalBrokerOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExternalBrokerOrgControl, "JE_OH_ExternalBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_ExternalBroker)));
			this.ExternalBrokerOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d14962df-9607-42d3-93dc-5585686817ce", "External Broker");
			this.ExternalBrokerOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.ExternalBrokerOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 156, true);
			this.ExternalBrokerOrgControl.Name = "ExternalBrokerOrgControl";
			this.ExternalBrokerOrgControl.ShouldResize = true;
			this.ExternalBrokerOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ExternalBrokerOrgControl.TabIndex = 9;
			// 
			// SellingAgentOrgControl
			// 
			this.SellingAgentOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellingAgentOrgControl, "JE_OH_SellingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_SellingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.SellingAgents)));
			this.SellingAgentOrgControl.BindToList = "Lookups+SellingAgents";
			this.SellingAgentOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d8f8e35e-09e0-4232-9721-ddc6376a9e13", "Selling Agent");
			this.SellingAgentOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.SellingAgentOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.SellingAgentOrgControl.Name = "SellingAgentOrgControl";
			this.SellingAgentOrgControl.ShouldResize = true;
			this.SellingAgentOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.SellingAgentOrgControl.TabIndex = 4;
			// 
			// BuyerOrgControl
			// 
			this.BuyerOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerOrgControl, "JE_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.Consignees)));
			this.BuyerOrgControl.BindToList = "AddInfoLookups+Consignees";
			this.BuyerOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a5a29097-d697-4913-b97c-9a4215726c19", "Buyer");
			this.BuyerOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.BuyerOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.BuyerOrgControl.Name = "BuyerOrgControl";
			this.BuyerOrgControl.ShouldResize = true;
			this.BuyerOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BuyerOrgControl.TabIndex = 7;
			// 
			// BuyerAgentOrgControl
			// 
			this.BuyerAgentOrgControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerAgentOrgControl, "JE_OH_BuyingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_BuyingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.BuyingAgents)));
			this.BuyerAgentOrgControl.BindToList = "Lookups+BuyingAgents";
			this.BuyerAgentOrgControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("62d2e0df-718d-4944-b5dc-ed45af690c05", "Buyer Agent");
			this.BuyerAgentOrgControl.IsPrimaryKeyFromCodeRequired = false;
			this.BuyerAgentOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 134, true);
			this.BuyerAgentOrgControl.Name = "BuyerAgentOrgControl";
			this.BuyerAgentOrgControl.ShouldResize = true;
			this.BuyerAgentOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BuyerAgentOrgControl.TabIndex = 8;
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerAddressControl, "JE_OA_SellerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_SellerAddress)));
			this.SellerAddressControl.BindToOrgList = "AddInfoLookups+Consignors";
			this.SellerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("848b082d-d12b-418f-9f47-00176bf86977", "Seller");
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.SellerAddressControl.Name = "SellerAddressControl";
			this.SellerAddressControl.PopupCaption = "";
			this.SellerAddressControl.ReadOnly = false;
			this.SellerAddressControl.ShowAddress = false;
			this.SellerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.SellerAddressControl.TabIndex = 2;
			// 
			// ControllingAgentGuidFindBox
			// 
			this.ControllingAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentGuidFindBox, "JE_OH_ControllingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_ControllingAgent)));
			this.ControllingAgentGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ff8d61a6-9bac-4ba1-91da-55cc8a7d7ff4", "Controlling Agent");
			this.ControllingAgentGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 178, true);
			this.ControllingAgentGuidFindBox.Name = "ControllingAgentGuidFindBox";
			this.ControllingAgentGuidFindBox.ShouldResize = true;
			this.ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ControllingAgentGuidFindBox.TabIndex = 10;
			// 
			// zGuidFindBox2
			// 
			this.ControllingCustomerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerGuidFindBox, "JE_OH_ControllingCustomer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_ControllingCustomer)));
			this.ControllingCustomerGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("65bb452a-6969-4631-98b8-3db14087ecda", "Controlling Customer");
			this.ControllingCustomerGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ControllingCustomerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 200, true);
			this.ControllingCustomerGuidFindBox.Name = "ControllingCustomerGuidFindBox";
			this.ControllingCustomerGuidFindBox.ShouldResize = true;
			this.ControllingCustomerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ControllingCustomerGuidFindBox.TabIndex = 11;
			// 
			// PackagerAddressControl
			// 
			this.PackagerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagerAddressControl, "JE_OA_PackagerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_PackagerAddress)));
			this.PackagerAddressControl.BindToOrgList = "Lookups+Packagers";
			this.PackagerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("149dba1f-d619-4ba1-84c9-9b5e67ccd035", "Packager");
			this.PackagerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 134, true);
			this.PackagerAddressControl.Name = "PackagerAddressControl";
			this.PackagerAddressControl.PopupCaption = "";
			this.PackagerAddressControl.ReadOnly = false;
			this.PackagerAddressControl.ShowAddress = false;
			this.PackagerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.PackagerAddressControl.TabIndex = 8;
			// 
			// DistributorAddressControl
			// 
			this.DistributorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DistributorAddressControl, "JE_OA_DistributorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_DistributorAddress)));
			this.DistributorAddressControl.BindToOrgList = "Lookups+Distributors";
			this.DistributorAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89057034-c600-445c-8ca2-250d4fd2f20f", "Distributor");
			this.DistributorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.DistributorAddressControl.Name = "DistributorAddressControl";
			this.DistributorAddressControl.PopupCaption = "";
			this.DistributorAddressControl.ReadOnly = false;
			this.DistributorAddressControl.ShowAddress = false;
			this.DistributorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DistributorAddressControl.TabIndex = 5;
			// 
			// ShipperAddressControl
			// 
			this.ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressControl, "JE_OA_ShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OA_ShipperAddress)));
			this.ShipperAddressControl.BindToOrgList = "Lookups+Shippers";
			this.ShipperAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("98d1a2a2-366f-4d92-9956-2bca38a06bd9", "Shipper");
			this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.ShipperAddressControl.Name = "ShipperAddressControl";
			this.ShipperAddressControl.PopupCaption = "";
			this.ShipperAddressControl.ReadOnly = false;
			this.ShipperAddressControl.ShowAddress = false;
			this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShipperAddressControl.TabIndex = 4;
			// 
			// JobMiscOrgsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackagerAddressControl);
			this.Controls.Add(this.DistributorAddressControl);
			this.Controls.Add(this.ShipperAddressControl);
			this.Controls.Add(this.ControllingCustomerGuidFindBox);
			this.Controls.Add(this.ControllingAgentGuidFindBox);
			this.Controls.Add(this.ExternalBrokerOrgControl);
			this.Controls.Add(this.BuyerAgentOrgControl);
			this.Controls.Add(this.BuyerOrgControl);
			this.Controls.Add(this.InvoicerAddressControl);
			this.Controls.Add(this.SellingAgentOrgControl);
			this.Controls.Add(this.SellerAddressControl);
			this.Controls.Add(this.ExporterOrgControl);
			this.Controls.Add(this.CBPBrokerOrgControl);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 222, true);
			this.Name = "JobMiscOrgsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 222, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CBPBrokerOrgControl.ResumeLayout(true);
			this.CBPBrokerOrgControl.PerformLayout();
			this.ExporterOrgControl.ResumeLayout(true);
			this.ExporterOrgControl.PerformLayout();
			this.InvoicerAddressControl.ResumeLayout(true);
			this.InvoicerAddressControl.PerformLayout();
			this.ExternalBrokerOrgControl.ResumeLayout(true);
			this.ExternalBrokerOrgControl.PerformLayout();
			this.SellingAgentOrgControl.ResumeLayout(true);
			this.SellingAgentOrgControl.PerformLayout();
			this.BuyerOrgControl.ResumeLayout(true);
			this.BuyerOrgControl.PerformLayout();
			this.BuyerAgentOrgControl.ResumeLayout(true);
			this.BuyerAgentOrgControl.PerformLayout();
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ShipperAddressControl.ResumeLayout(true);
			this.ShipperAddressControl.PerformLayout();
			this.DistributorAddressControl.ResumeLayout(true);
			this.DistributorAddressControl.PerformLayout();
			this.PackagerAddressControl.ResumeLayout(true);
			this.PackagerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox CBPBrokerOrgControl;
		private ZArchitecture.GUI.ZGuidFindBox ExporterOrgControl;
		internal ZArchitecture.GUI.ZAddressControl InvoicerAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox ExternalBrokerOrgControl;
		internal ZArchitecture.GUI.ZGuidFindBox SellingAgentOrgControl;
		private ZArchitecture.GUI.ZGuidFindBox BuyerOrgControl;
		internal ZArchitecture.GUI.ZGuidFindBox BuyerAgentOrgControl;
		private ZArchitecture.GUI.ZAddressControl SellerAddressControl;
		private ZArchitecture.GUI.ZGuidFindBox ControllingAgentGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox ControllingCustomerGuidFindBox;
		internal ZArchitecture.GUI.ZAddressControl ShipperAddressControl;
		internal ZArchitecture.GUI.ZAddressControl DistributorAddressControl;
		internal ZArchitecture.GUI.ZAddressControl PackagerAddressControl;
	}
}
