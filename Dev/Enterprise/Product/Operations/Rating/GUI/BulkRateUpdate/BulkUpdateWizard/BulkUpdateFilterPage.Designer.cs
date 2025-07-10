using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateFilterPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.components = new System.ComponentModel.Container();
			this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GatewayAgentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ApplyChangesToGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntercompanyTariffsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompanyTariffsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CostingsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ActiveQuotesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClientRatesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SupplierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CommodityCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoExpireDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContractNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentConsolidationStatusEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContractNumberLinkedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShowExpiredCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ContainerTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainerTypesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GatewayServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentGatewayServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerTypesGrid)).BeginInit();

			this.ApplyChangesToGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.BulkRateUpdater);
			//
			// ModuleDropEdit
			//
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Module)));
			this.ModuleDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|6bb4a41a-d070-4fe1-9bac-56db2cb94e93", "Module", "Module of Rates to update.");
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 20, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.PreBoundMaxLength = 3;
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ModuleDropEdit.TabIndex = 1;
			//
			// CarrierServiceLevelDropEdit
			//
			this.CarrierServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceLevelDropEdit, "CarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).CarrierServiceLevel)));
			this.CarrierServiceLevelDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|1666bc63-b410-4f76-bd66-110b4cf902bf", "Carrier Svc. Lvl.", "Carrier Service Level", "The Carrier Service Level for the costs to update.");
			this.CarrierServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 110, true);
			this.CarrierServiceLevelDropEdit.Name = "CarrierServiceLevelDropEdit";
			this.CarrierServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.CarrierServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CarrierServiceLevelDropEdit.TabIndex = 19;
			//
			// GatewayAgentTypeDropEdit
			//
			this.GatewayAgentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GatewayAgentTypeDropEdit, "GatewayAgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).GatewayAgentType)));
			this.GatewayAgentTypeDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|f051c499-f0c2-47ed-a4c4-d33062630cea", "Gateway Agent Type", "Gateway Agent Type", "The Gateway Agent Type for the rates to update.");
			this.GatewayAgentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 140, true);
			this.GatewayAgentTypeDropEdit.Name = "GatewayAgentTypeDropEdit";
			this.GatewayAgentTypeDropEdit.PreBoundMaxLength = 3;
			this.GatewayAgentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.GatewayAgentTypeDropEdit.TabIndex = 20;
			//
			// ApplyChangesToGroupBox
			//
			this.ApplyChangesToGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|d38929e2-09a4-46a1-9e93-64a8579af26a", "Apply Changes To");
			this.ApplyChangesToGroupBox.Controls.Add(this.IntercompanyTariffsCheckBox);
			this.ApplyChangesToGroupBox.Controls.Add(this.CompanyTariffsCheckBox);
			this.ApplyChangesToGroupBox.Controls.Add(this.CostingsCheckBox);
			this.ApplyChangesToGroupBox.Controls.Add(this.ActiveQuotesCheckBox);
			this.ApplyChangesToGroupBox.Controls.Add(this.ClientRatesCheckBox);
			this.ApplyChangesToGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(724, 39, true);
			this.ApplyChangesToGroupBox.Name = "ApplyChangesToGroupBox";
			this.ApplyChangesToGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 208, true);
			this.ApplyChangesToGroupBox.TabIndex = 24;
			this.ApplyChangesToGroupBox.TabStop = false;
			//
			// IntercompanyTariffsCheckBox
			//
			this.IntercompanyTariffsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntercompanyTariffsCheckBox, "ShowIntercompanyTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowIntercompanyTariffs)));
			this.IntercompanyTariffsCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|66404b62-f189-419d-b553-2da48ec66c0e", "Intercompany Tariff", "Update Intercompany Tariff.");
			this.IntercompanyTariffsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntercompanyTariffsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 160, true);
			this.IntercompanyTariffsCheckBox.Name = "IntercompanyTariffsCheckBox";
			this.IntercompanyTariffsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.IntercompanyTariffsCheckBox.TabIndex = 5;
			//
			// CompanyTariffsCheckBox
			//
			this.CompanyTariffsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CompanyTariffsCheckBox, "ShowCompanyTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowCompanyTariffs)));
			this.CompanyTariffsCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|2a10af4d-39ec-4cba-b713-81188844dcae", "Company Tariff", "Update Base Company Tariff.");
			this.CompanyTariffsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanyTariffsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 128, true);
			this.CompanyTariffsCheckBox.Name = "CompanyTariffsCheckBox";
			this.CompanyTariffsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.CompanyTariffsCheckBox.TabIndex = 4;
			//
			// CostingsCheckBox
			//
			this.CostingsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CostingsCheckBox, "ShowCostings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowCostings)));
			this.CostingsCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|220e5117-850c-40d0-b1f2-43c7673dbebb", "Costings", "Updating Costings.");
			this.CostingsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CostingsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 96, true);
			this.CostingsCheckBox.Name = "CostingsCheckBox";
			this.CostingsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.CostingsCheckBox.TabIndex = 3;
			//
			// ActiveQuotesCheckBox
			//
			this.ActiveQuotesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActiveQuotesCheckBox, "ShowActiveQuotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowActiveQuotes)));
			this.ActiveQuotesCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|c8f9fec1-d1b5-459a-a6d2-7cd7affa829d", "Active Quotes", "Update Active Quotations (as expired and accepted quotes cannot be updated).");
			this.ActiveQuotesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ActiveQuotesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 64, true);
			this.ActiveQuotesCheckBox.Name = "ActiveQuotesCheckBox";
			this.ActiveQuotesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.ActiveQuotesCheckBox.TabIndex = 2;
			//
			// ClientRatesCheckBox
			//
			this.ClientRatesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ClientRatesCheckBox, "ShowClientRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowClientRates)));
			this.ClientRatesCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|1593fb54-e6ca-4949-a3e5-87d26fda1597", "Client Rates", "Update Client Rates.");
			this.ClientRatesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientRatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 32, true);
			this.ClientRatesCheckBox.Name = "ClientRatesCheckBox";
			this.ClientRatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.ClientRatesCheckBox.TabIndex = 1;
			//
			// CarrierGuidFindBox
			//
			this.CarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Carrier)));
			this.CarrierGuidFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|c35f9520-3246-4a11-863d-8f4f752cbdef", "Carrier", "The Carrier of the rates to update.");
			this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 350, true);
			this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
			this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.CarrierGuidFindBox.TabIndex = 12;
			//
			// SupplierGuidFindBox
			//
			this.SupplierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierGuidFindBox, "Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Supplier)));
			this.SupplierGuidFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|2d8ef3ec-c5ae-4816-9e57-2d8c14cf7682", "Service Provider", "The Service Provider of the rates to update.");
			this.SupplierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 320, true);
			this.SupplierGuidFindBox.Name = "SupplierGuidFindBox";
			this.SupplierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.SupplierGuidFindBox.TabIndex = 11;
			//
			// EndDateEdit
			//
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).EndDate)));
			this.EndDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|9ed0ff57-493f-41e7-bbe3-23d2699277c1", "To", "End Date", "The End Date for the rates to update.");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 80, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 18;
			//
			// StartDateEdit
			//
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).StartDate)));
			this.StartDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|c26f2a5e-8aec-4f33-b8db-78cda2f170f4", "Date", "Start Date", "The Start Date for the rates to update.");
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 80, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 17;
			//
			// CommodityCodeCodeFindBox
			//
			this.CommodityCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeCodeFindBox, "CommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).CommodityCode)));
			this.CommodityCodeCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|e96fc264-1807-4902-b974-77ae4821196b", "Com Code", "Commodity Code", "The Commodity Code of the rates to update.");
			this.CommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 260, true);
			this.CommodityCodeCodeFindBox.Name = "CommodityCodeCodeFindBox";
			this.CommodityCodeCodeFindBox.PreBoundMaxLength = 4;
			this.CommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CommodityCodeCodeFindBox.TabIndex = 9;
			//
			// ServiceLevelCodeFindBox
			//
			this.ServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeFindBox, "ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ServiceLevel)));
			this.ServiceLevelCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|22400c27-f9db-423d-9870-f6ccc0133001", "Svc. Lvl.", "Service Level", "The Service Level of the rates to update.");
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 230, true);
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ServiceLevelCodeFindBox.TabIndex = 8;
			//
			// DestinationCodeFindBox
			//
			this.DestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCodeFindBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Destination)));
			this.DestinationCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|dec3e1e7-52e7-462a-9799-b317f2d16d36", "Destination", "The Destination of the rates to update.");
			this.DestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 200, true);
			this.DestinationCodeFindBox.Name = "DestinationCodeFindBox";
			this.DestinationCodeFindBox.PreBoundMaxLength = 5;
			this.DestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.DestinationCodeFindBox.TabIndex = 7;
			//
			// OriginCodeFindBox
			//
			this.OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Origin)));
			this.OriginCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|f652d294-08e4-475c-a7e1-41f7ac1d7234", "Origin", "The Origin of the rates to update.");
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 170, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.OriginCodeFindBox.PreBoundMaxLength = 5;
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.OriginCodeFindBox.TabIndex = 6;
			//
			// ModeDropEdit
			//
			this.ModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Mode)));
			this.ModeDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|f8f79ab6-2793-4638-a66f-e119ff63aa31", "Mode", "Transport Mode", "The transport mode of the rates to update.");
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 140, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.PreBoundMaxLength = 3;
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ModeDropEdit.TabIndex = 5;
			//
			// TypeDropEdit
			//
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).Type)));
			this.TypeDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|939f9688-b757-46c8-b38c-07e15cd2a14e", "Type", "Type of Rate to update.");
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 50, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.TypeDropEdit.TabIndex = 2;
			//
			// NoExpireDateCheckBox
			//
			this.NoExpireDateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NoExpireDateCheckBox, "NoExpireDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).NoExpireDate)));
			this.NoExpireDateCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|87c4638e-3349-4b6a-a0fb-f47353e5af7a", "No Expiry Date", "Select rates with no expiry date specified.");
			this.NoExpireDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoExpireDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 110, true);
			this.NoExpireDateCheckBox.Name = "NoExpireDateCheckBox";
			this.NoExpireDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.NoExpireDateCheckBox.TabIndex = 4;
			this.NoExpireDateCheckBox.UseVisualStyleBackColor = true;
			//
			// ClientGuidFindBox
			//
			this.ClientGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientGuidFindBox, "SelectedClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).SelectedClient)));
			this.ClientGuidFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|42a7ea36-7927-4497-92bf-da65153c4180", "Client", "The Client of the rates to update.");
			this.ClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 290, true);
			this.ClientGuidFindBox.Name = "ClientGuidFindBox";
			this.ClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.ClientGuidFindBox.TabIndex = 10;
			//
			// ContractNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.ContractNumberTextBox, "ContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ContractNumber)));
			this.ContractNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContractNumberTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|6dbc0537-ea2f-4476-a574-db98d62b5b85", "Contract No.");
			this.ContractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 20, true);
			this.ContractNumberTextBox.Name = "ContractNumberTextBox";
			this.ContractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.ContractNumberTextBox.TabIndex = 15;
			//
			// ShipmentConsolidationStatusEdit
			//
			this.ShipmentConsolidationStatusEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentConsolidationStatusEdit, "ShipmentConsolidationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShipmentConsolidationStatus)));
			this.ShipmentConsolidationStatusEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|92DAB70A-B5BD-4155-A94B-E72CDB21D897", "Ship. Consol. Status", "Shipment Consolidation Status", "The Shipment Consolidation Status for the rates to update.");
			this.ShipmentConsolidationStatusEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 230, true);
			this.ShipmentConsolidationStatusEdit.Name = "ShipmentConsolidationStatusEdit";
			this.ShipmentConsolidationStatusEdit.PreBoundMaxLength = 3;
			this.ShipmentConsolidationStatusEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ShipmentConsolidationStatusEdit.TabIndex = 23;
			//
			// ContractNumberLinkedDropEdit
			//
			this.ContractNumberLinkedDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ContractNumberLinkedDropEdit, "ContractNumberLinked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ContractNumberLinked)));
			this.ContractNumberLinkedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 50, true);
			this.ContractNumberLinkedDropEdit.Name = "ContractNumberLinkedCheckbox";
			this.ContractNumberLinkedDropEdit.TabIndex = 16;
			this.ContractNumberLinkedDropEdit.PreBoundMaxLength = 3;
			this.ContractNumberLinkedDropEdit.ShowDescriptionBox = false;
			this.ContractNumberLinkedDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|8a90df31-1352-4038-8d28-204f26311385", "Contract Linked", "Contract Linked", "Whether the Contract Number is linked to a record in the Contracts Module");
			//
			// ShowExpiredCheckbox
			//
			this.ShowExpiredCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowExpiredCheckbox, "ShowExpired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShowExpired)));
			this.ShowExpiredCheckbox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|b59e068c-3dfd-4118-b7c9-97e64b8bb237", "Show Expired", "Include rates that have expired in results.");
			this.ShowExpiredCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 80, true);
			this.ShowExpiredCheckbox.Name = "ShowExpiredCheckbox";
			this.ShowExpiredCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 19, true);
			this.ShowExpiredCheckbox.TabIndex = 3;
			this.ShowExpiredCheckbox.UseVisualStyleBackColor = true;
			//
			// ContainerTypesGrid
			//
			this.ContainerTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainerTypesGrid, "ContainerTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ContainerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdaterContainerType)(((System.Collections.IList)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ContainerTypes)).SyncRoot)).RC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdaterContainerType)(((System.Collections.IList)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ContainerTypes)).SyncRoot)).RC_Description)));
			this.ContainerTypesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RC_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "RC_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ContainerTypesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContainerTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainerTypesGrid.GridId = "19818f22-0d6e-4135-9fcd-3e8e754e0036";
			this.ContainerTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerTypesGrid.LayoutKey = "zGrid1";
			this.ContainerTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 380, true);
			this.ContainerTypesGrid.Name = "ContainerTypesGrid";
			this.ContainerTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 87, true);
			this.ContainerTypesGrid.TabIndex = 14;
			this.ContainerTypesGrid.IsCustomiseMenuVisible = false;
			this.ContainerTypesGrid.DisableImportDataMenuItem = true;
			this.ContainerTypesGrid.IsCustomiseMenuVisible = false;
			this.ContainerTypesGrid.ShowExcelMenuItems = false;
			//
			// ContainerTypesLabel
			//
			this.ContainerTypesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ContainerTypesLabel.FontType = OFontTypes.Normal;
			this.ContainerTypesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 380, true);
			this.ContainerTypesLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContainerTypesLabel.Name = "ContainerTypesLabel";
			this.ContainerTypesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 14, true);
			this.ContainerTypesLabel.TabIndex = 13;
			this.ContainerTypesLabel.Text = Enterprise.Rating.GUI.Res.GetString("c5cf153e-db60-4aef-bd5f-bd8cbc059ec7", "Container Type");
			this.ContainerTypesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// GatewayServiceLevelCodeFindBox
			//
			this.GatewayServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GatewayServiceLevelCodeFindBox, "GatewayServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).GatewayServiceLevel)));
			this.GatewayServiceLevelCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|E5202C19-58E6-4EB2-B430-F957EA95364F", "G/W Service", "Gateway Service Level");
			this.GatewayServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 170, true);
			this.GatewayServiceLevelCodeFindBox.Name = "GatewayServiceLevelCodeFindBox";
			this.GatewayServiceLevelCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.GatewayServiceLevelCodeFindBox.ParentType = null;
			this.GatewayServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.GatewayServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 18, true);
			this.GatewayServiceLevelCodeFindBox.TabIndex = 21;
			//
			// ShipmentGatewayServiceLevelCodeFindBox
			//
			this.ShipmentGatewayServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentGatewayServiceLevelCodeFindBox, "ShipmentGatewayServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ShipmentGatewayServiceLevel)));
			this.ShipmentGatewayServiceLevelCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateFilterPage|800FDB42-F002-49AB-A33E-8DEFB1BA881F", "Shipment G/W Service", "Shipment Gateway Service Level");
			this.ShipmentGatewayServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 200, true);
			this.ShipmentGatewayServiceLevelCodeFindBox.Name = "ShipmentGatewayServiceLevelCodeFindBox";
			this.ShipmentGatewayServiceLevelCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.ShipmentGatewayServiceLevelCodeFindBox.ParentType = null;
			this.ShipmentGatewayServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ShipmentGatewayServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 18, true);
			this.ShipmentGatewayServiceLevelCodeFindBox.TabIndex = 22;
			//
			// BulkUpdateFilterPage
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GatewayServiceLevelCodeFindBox);
			this.Controls.Add(this.ShipmentGatewayServiceLevelCodeFindBox);
			this.Controls.Add(this.ContainerTypesLabel);
			this.Controls.Add(this.ContainerTypesGrid);
			this.Controls.Add(this.ShowExpiredCheckbox);
			this.Controls.Add(this.ContractNumberLinkedDropEdit);
			this.Controls.Add(this.ContractNumberTextBox);
			this.Controls.Add(this.ClientGuidFindBox);
			this.Controls.Add(this.NoExpireDateCheckBox);
			this.Controls.Add(this.ModuleDropEdit);
			this.Controls.Add(this.CarrierServiceLevelDropEdit);
			this.Controls.Add(this.GatewayAgentTypeDropEdit);
			this.Controls.Add(this.ApplyChangesToGroupBox);
			this.Controls.Add(this.CarrierGuidFindBox);
			this.Controls.Add(this.SupplierGuidFindBox);
			this.Controls.Add(this.EndDateEdit);
			this.Controls.Add(this.StartDateEdit);
			this.Controls.Add(this.CommodityCodeCodeFindBox);
			this.Controls.Add(this.ServiceLevelCodeFindBox);
			this.Controls.Add(this.DestinationCodeFindBox);
			this.Controls.Add(this.OriginCodeFindBox);
			this.Controls.Add(this.ModeDropEdit);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.ShipmentConsolidationStatusEdit);
			this.Name = "BulkUpdateFilterPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 495, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerTypesGrid)).EndInit();
			this.ApplyChangesToGroupBox.ResumeLayout(false);
			this.ApplyChangesToGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit ModuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CarrierServiceLevelDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit GatewayAgentTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ApplyChangesToGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IntercompanyTariffsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CompanyTariffsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CostingsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ActiveQuotesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ClientRatesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox SupplierGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit StartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CommodityCodeCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ServiceLevelCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox NoExpireDateCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox ClientGuidFindBox;
		private ZArchitecture.ZTextBox ContractNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ShipmentConsolidationStatusEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ContractNumberLinkedDropEdit;
		private ZArchitecture.GUI.ZCheckBox ShowExpiredCheckbox;
		private ZArchitecture.ZGrid ContainerTypesGrid;
		private ZArchitecture.ZLabel ContainerTypesLabel;
		private ZArchitecture.GUI.ZCodeFindBox GatewayServiceLevelCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ShipmentGatewayServiceLevelCodeFindBox;
	}
}
