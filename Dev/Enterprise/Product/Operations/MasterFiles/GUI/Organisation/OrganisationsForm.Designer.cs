using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZOrganisationsForm
	{

		#region Windows Form Designer generated code

		protected internal ReceivablesUserControl ReceivablesControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage PayablesTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage ReceivablesTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage ConsignorTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage ConsigneeTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage WhsFacilityTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage ForwarderTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage TransportTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage MiscServicesTabPage;
		Enterprise.MasterFiles.GUI.PayablesUserControl PayablesControl;
		Enterprise.MasterFiles.GUI.ConsignorUserControl ConsignorControl;
		protected internal ConsigneeUserControl ConsigneeControl;
		internal WhsFacilityUserControl WhsFacilityUserControl;
		protected internal CarrierUserControl TransportControl;
		Enterprise.MasterFiles.GUI.MiscServicesUserControl MiscServicesControl;
		Enterprise.MasterFiles.GUI.CompetitorUserControl CompetitorControl;
		Enterprise.MasterFiles.GUI.ForwarderUserControl ForwarderControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage SalesTabPage;
		Enterprise.MasterFiles.GUI.SalesUserControl SalesControl;
		Enterprise.ZArchitecture.ZLabel ReceivablesNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel PayablesNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel ConsignorNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel ConsigneeNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel WarehouseNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel ForwarderNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel MiscServicesNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel CompetitorNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel TransportNotSelectedLabel;
		Enterprise.ZArchitecture.ZLabel SalesNotSelectedLabel;
		Enterprise.ZArchitecture.GUI.ZTabPage UserDefinedTabPage;
		Enterprise.MasterFiles.GUI.UserDefinedUserControl UserDefinedControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage CompetitorTabPage;
		protected internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

		new void InitializeComponent()
		{
			this.PayablesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReceivablesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsigneeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WhsFacilityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ForwarderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MiscServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompetitorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SalesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UserDefinedTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.OrganisationsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// OrganisationsTabControl
			//
			this.OrganisationsTabControl.Controls.Add(this.WorkflowTabPage);
			// 
			// OrganisationsTabControl
			// 
			this.OrganisationsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 593, true);
			// 
			// ContactsTabPage
			// 
			this.ContactsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|fc351376-8955-4457-a113-022f3b768aa7", "Contact");
			this.ContactsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 566, true);
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 566, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|1286aa2b-2c43-4b63-9a10-601acb0f4338", "Details");
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 566, true);
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// 
			// AddressesTabPage
			// 
			this.AddressesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|1bd598f5-7d0e-4126-9ea4-fcd1e41aa187", "Address");
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 566, true);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(932, 637, true);
			this.ButtonsUserControl.TabIndex = 3;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 647, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// PayablesTabPage
			// 
			this.PayablesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|1cea984e-37ed-4b15-bc37-cf406181e5eb", "A/P");
			this.PayablesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PayablesTabPage.Name = "PayablesTabPage";
			this.PayablesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.PayablesTabPage.TabIndex = 16;
			this.PayablesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.PayablesTabPage_InitializeTab));
			// 
			// ReceivablesTabPage
			// 
			this.ReceivablesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|99d9d85f-4f68-4a0e-b167-f7f7a5ca628d", "A/R");
			this.ReceivablesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReceivablesTabPage.Name = "ReceivablesTabPage";
			this.ReceivablesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 614, true);
			this.ReceivablesTabPage.TabIndex = 17;
			this.ReceivablesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ReceivablesTabPage_InitializeTab));
			// 
			// ConsignorTabPage
			// 
			this.ConsignorTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|bccddab8-7cff-430d-b2b5-fafa92196a40", "Consignor");
			this.ConsignorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsignorTabPage.Name = "ConsignorTabPage";
			this.ConsignorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ConsignorTabPage.TabIndex = 19;
			this.ConsignorTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConsignorTabPage_InitializeTab));
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|079a0afb-6927-449d-9b6c-37d34e4b24cc", "Consignee");
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ConsigneeTabPage.TabIndex = 18;
			this.ConsigneeTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConsigneeTabPage_InitializeTab));
			// 
			// WhsFacilityTabPage
			// 
			if (WarehouseDataRegistry.Instance.EnableContainerYard.Value)
			{
				this.WhsFacilityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|6fad77e5-59a6-4fd8-94f8-64372ce90bc0", "Whs/Facility");
			}
			else
			{
				this.WhsFacilityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|4f64ab94-c1a2-413e-83f1-bc31d8293850", "Warehouse");
			}
			this.WhsFacilityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WhsFacilityTabPage.Name = "WhsFacilityTabPage";
			this.WhsFacilityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WhsFacilityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.WhsFacilityTabPage.TabIndex = 20;
			this.WhsFacilityTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WarehouseTabPage_InitializeTab));
			// 
			// ForwarderTabPage
			// 
			this.ForwarderTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|d68d6cfc-5643-4fb7-82f3-60c6ebfabe9f", "Fwd/Agent");
			this.ForwarderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ForwarderTabPage.Name = "ForwarderTabPage";
			this.ForwarderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ForwarderTabPage.TabIndex = 21;
			this.ForwarderTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ForwarderTabPage_InitializeTab));
			// 
			// TransportTabPage
			// 
			this.TransportTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|e290e094-98e9-473e-ae43-c0178ad251d4", "Carrier");
			this.TransportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransportTabPage.Name = "TransportTabPage";
			this.TransportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.TransportTabPage.TabIndex = 22;
			this.TransportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TransportTabPage_InitializeTab));
			// 
			// MiscServicesTabPage
			// 
			this.MiscServicesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|12a130a7-bd15-4875-8ad9-4a1dcef464a2", "Services");
			this.MiscServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscServicesTabPage.Name = "MiscServicesTabPage";
			this.MiscServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.MiscServicesTabPage.TabIndex = 24;
			this.MiscServicesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MiscServicesTabPage_InitializeTab));
			// 
			// CompetitorTabPage
			// 
			this.CompetitorTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|2ebaf1cb-0440-4fb9-a00c-f943e8665f8a", "Competitor");
			this.CompetitorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompetitorTabPage.Name = "CompetitorTabPage";
			this.CompetitorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.CompetitorTabPage.TabIndex = 26;
			this.CompetitorTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CompetitorTabPage_InitializeTab));
			// 
			// SalesTabPage
			// 
			this.SalesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|201d2850-c98e-46c7-a9cb-12dd50944eda", "Sales");
			this.SalesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesTabPage.Name = "SalesTabPage";
			this.SalesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.SalesTabPage.TabIndex = 27;
			this.SalesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SalesTabPage_InitializeTab));
			// 
			// UserDefinedTabPage
			// 
			this.UserDefinedTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|a762f157-d98f-4873-933f-016ea9090540", "Custom");
			this.UserDefinedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UserDefinedTabPage.Name = "UserDefinedTabPage";
			this.UserDefinedTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.UserDefinedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.UserDefinedTabPage.TabIndex = 28;
			this.UserDefinedTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UserDefinedTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 458, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// ZOrganisationsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 687, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|91216ff5-7cd5-4f08-b331-2738a52cd5d8", "Maintain Organizations");
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "ZOrganisationsForm";
			this.Load += new System.EventHandler(this.ZOrganisationsForm_Load);
			this.OrganisationsTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private void UserDefinedTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.UserDefinedControl = new Enterprise.MasterFiles.GUI.UserDefinedUserControl();
			this.UserDefinedTabPage.SuspendLayout();
			this.UserDefinedTabPage.Controls.Add(this.UserDefinedControl);
			// 
			// UserDefinedControl
			// 
			this.UserDefinedControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UserDefinedControl, ".");
			this.UserDefinedControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UserDefinedControl.IsModifyAddress = false;
			this.UserDefinedControl.IsModifyCarrier = false;
			this.UserDefinedControl.IsModifyCompetitor = false;
			this.UserDefinedControl.IsModifyConfig = false;
			this.UserDefinedControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.UserDefinedControl.IsModifyConfigEDICodeMapping = false;
			this.UserDefinedControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.UserDefinedControl.IsModifyConfigGeneral = false;
			this.UserDefinedControl.IsModifyConfigRegistrationNumbers = false;
			this.UserDefinedControl.IsModifyConsignee = false;
			this.UserDefinedControl.IsModifyConsigneeDetails = false;
			this.UserDefinedControl.IsModifyConsigneeLandedCosting = false;
			this.UserDefinedControl.IsModifyConsigneeRelationships = false;
			this.UserDefinedControl.IsModifyConsignor = false;
			this.UserDefinedControl.IsModifyConsignorDetails = false;
			this.UserDefinedControl.IsModifyConsignorExporterScheme = false;
			this.UserDefinedControl.IsModifyConsignorRelationships = false;
			this.UserDefinedControl.IsModifyContact = false;
			this.UserDefinedControl.IsModifyContactContactDetails = false;
			this.UserDefinedControl.IsModifyContactDocDeliveryDetails = false;
			this.UserDefinedControl.IsModifyContactPersonalInformation = false;
			this.UserDefinedControl.IsModifyCustom = true;
			this.UserDefinedControl.IsModifyDetails = false;
			this.UserDefinedControl.IsModifyDetailsNameAndAddress = false;
			this.UserDefinedControl.IsModifyDetailsOrganisationType = false;
			this.UserDefinedControl.IsModifyDetailsPhFaxWebDetails = false;
			this.UserDefinedControl.IsModifyDetailsRatingAndTariffs = false;
			this.UserDefinedControl.IsModifyDetailsStaffAssignments = false;
			this.UserDefinedControl.IsModifyDetailsWebSecurity = false;
			this.UserDefinedControl.IsModifyForwarder = false;
			this.UserDefinedControl.IsModifyForwarderDetails = false;
			this.UserDefinedControl.IsModifyForwarderProfitShare = false;
			this.UserDefinedControl.IsModifyPayables = false;
			this.UserDefinedControl.IsModifyReceivables = false;
			this.UserDefinedControl.IsModifyReceivablesConfig = false;
			this.UserDefinedControl.IsModifyReceivablesInvoicing = false;
			this.UserDefinedControl.IsModifySales = false;
			this.UserDefinedControl.IsModifySalesClientRelationship = false;
			this.UserDefinedControl.IsModifySalesClientSummary = false;
			this.UserDefinedControl.IsModifySalesOpportunityManagement = false;
			this.UserDefinedControl.IsModifySalesTradeProfile = false;
			this.UserDefinedControl.IsModifyServices = false;
			this.UserDefinedControl.IsModifyWarehouse = false;
			this.UserDefinedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.UserDefinedControl.Name = "UserDefinedControl";
			this.UserDefinedControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 552, true);
			this.UserDefinedControl.TabIndex = 0;
			this.UserDefinedTabPage.ResumeLayout(true);
		}

		protected virtual void SalesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.SalesNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SalesControl = new Enterprise.MasterFiles.GUI.SalesUserControl();
			this.SalesTabPage.SuspendLayout();
			this.SalesTabPage.Controls.Add(this.SalesNotSelectedLabel);
			this.SalesTabPage.Controls.Add(this.SalesControl);
			// 
			// SalesNotSelectedLabel
			// 
			this.SalesNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|e9eeb27b-1902-482f-a377-a606fbf31172", "You must select an Organization type of Sales from the Details page to use this page.");
			this.SalesNotSelectedLabel.Name = "SalesNotSelectedLabel";
			this.SalesNotSelectedLabel.TabIndex = 1;
			this.SalesNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.SalesNotSelectedLabel.AutoSize = false;
			this.SalesNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// SalesControl
			// 
			this.SalesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesControl, ".");
			this.SalesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SalesControl.IsModifyAddress = false;
			this.SalesControl.IsModifyCarrier = false;
			this.SalesControl.IsModifyCompetitor = false;
			this.SalesControl.IsModifyConfig = false;
			this.SalesControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.SalesControl.IsModifyConfigEDICodeMapping = false;
			this.SalesControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.SalesControl.IsModifyConfigGeneral = false;
			this.SalesControl.IsModifyConfigRegistrationNumbers = false;
			this.SalesControl.IsModifyConsignee = false;
			this.SalesControl.IsModifyConsigneeDetails = false;
			this.SalesControl.IsModifyConsigneeLandedCosting = false;
			this.SalesControl.IsModifyConsigneeRelationships = false;
			this.SalesControl.IsModifyConsignor = false;
			this.SalesControl.IsModifyConsignorDetails = false;
			this.SalesControl.IsModifyConsignorExporterScheme = false;
			this.SalesControl.IsModifyConsignorRelationships = false;
			this.SalesControl.IsModifyContact = false;
			this.SalesControl.IsModifyContactContactDetails = false;
			this.SalesControl.IsModifyContactDocDeliveryDetails = false;
			this.SalesControl.IsModifyContactPersonalInformation = false;
			this.SalesControl.IsModifyCustom = false;
			this.SalesControl.IsModifyDetails = false;
			this.SalesControl.IsModifyDetailsNameAndAddress = false;
			this.SalesControl.IsModifyDetailsOrganisationType = false;
			this.SalesControl.IsModifyDetailsPhFaxWebDetails = false;
			this.SalesControl.IsModifyDetailsRatingAndTariffs = false;
			this.SalesControl.IsModifyDetailsStaffAssignments = false;
			this.SalesControl.IsModifyDetailsWebSecurity = false;
			this.SalesControl.IsModifyForwarder = false;
			this.SalesControl.IsModifyForwarderDetails = false;
			this.SalesControl.IsModifyForwarderProfitShare = false;
			this.SalesControl.IsModifyPayables = false;
			this.SalesControl.IsModifyReceivables = false;
			this.SalesControl.IsModifyReceivablesConfig = false;
			this.SalesControl.IsModifyReceivablesInvoicing = false;
			this.SalesControl.IsModifySales = true;
			this.SalesControl.IsModifySalesClientRelationship = true;
			this.SalesControl.IsModifySalesClientSummary = true;
			this.SalesControl.IsModifySalesOpportunityManagement = true;
			this.SalesControl.IsModifySalesTradeProfile = true;
			this.SalesControl.IsModifyServices = false;
			this.SalesControl.IsModifyWarehouse = false;
			this.SalesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SalesControl.Name = "SalesControl";
			this.SalesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SalesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.SalesControl.TabIndex = 0;
			this.SalesTabPage.ResumeLayout(true);
		}

		private void CompetitorTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CompetitorNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompetitorControl = new Enterprise.MasterFiles.GUI.CompetitorUserControl();
			this.CompetitorTabPage.SuspendLayout();
			this.CompetitorTabPage.Controls.Add(this.CompetitorNotSelectedLabel);
			this.CompetitorTabPage.Controls.Add(this.CompetitorControl);
			// 
			// CompetitorNotSelectedLabel
			// 
			this.CompetitorNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|5977d496-564d-4287-9262-ec67cdffee5e", "You must select an Organization type of Competitor from the Details page to use this page.");
			this.CompetitorNotSelectedLabel.Name = "CompetitorNotSelectedLabel";
			this.CompetitorNotSelectedLabel.TabIndex = 1;
			this.CompetitorNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CompetitorNotSelectedLabel.AutoSize = false;
			this.CompetitorNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// CompetitorControl
			// 
			this.CompetitorControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompetitorControl, ".");
			this.CompetitorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompetitorControl.IsModifyAddress = false;
			this.CompetitorControl.IsModifyCarrier = false;
			this.CompetitorControl.IsModifyCompetitor = true;
			this.CompetitorControl.IsModifyConfig = false;
			this.CompetitorControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.CompetitorControl.IsModifyConfigEDICodeMapping = false;
			this.CompetitorControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.CompetitorControl.IsModifyConfigGeneral = false;
			this.CompetitorControl.IsModifyConfigRegistrationNumbers = false;
			this.CompetitorControl.IsModifyConsignee = false;
			this.CompetitorControl.IsModifyConsigneeDetails = false;
			this.CompetitorControl.IsModifyConsigneeLandedCosting = false;
			this.CompetitorControl.IsModifyConsigneeRelationships = false;
			this.CompetitorControl.IsModifyConsignor = false;
			this.CompetitorControl.IsModifyConsignorDetails = false;
			this.CompetitorControl.IsModifyConsignorExporterScheme = false;
			this.CompetitorControl.IsModifyConsignorRelationships = false;
			this.CompetitorControl.IsModifyContact = false;
			this.CompetitorControl.IsModifyContactContactDetails = false;
			this.CompetitorControl.IsModifyContactDocDeliveryDetails = false;
			this.CompetitorControl.IsModifyContactPersonalInformation = false;
			this.CompetitorControl.IsModifyCustom = false;
			this.CompetitorControl.IsModifyDetails = false;
			this.CompetitorControl.IsModifyDetailsNameAndAddress = false;
			this.CompetitorControl.IsModifyDetailsOrganisationType = false;
			this.CompetitorControl.IsModifyDetailsPhFaxWebDetails = false;
			this.CompetitorControl.IsModifyDetailsRatingAndTariffs = false;
			this.CompetitorControl.IsModifyDetailsStaffAssignments = false;
			this.CompetitorControl.IsModifyDetailsWebSecurity = false;
			this.CompetitorControl.IsModifyForwarder = false;
			this.CompetitorControl.IsModifyForwarderDetails = false;
			this.CompetitorControl.IsModifyForwarderProfitShare = false;
			this.CompetitorControl.IsModifyPayables = false;
			this.CompetitorControl.IsModifyReceivables = false;
			this.CompetitorControl.IsModifyReceivablesConfig = false;
			this.CompetitorControl.IsModifyReceivablesInvoicing = false;
			this.CompetitorControl.IsModifySales = false;
			this.CompetitorControl.IsModifySalesClientRelationship = false;
			this.CompetitorControl.IsModifySalesClientSummary = false;
			this.CompetitorControl.IsModifySalesOpportunityManagement = false;
			this.CompetitorControl.IsModifySalesTradeProfile = false;
			this.CompetitorControl.IsModifyServices = false;
			this.CompetitorControl.IsModifyWarehouse = false;
			this.CompetitorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompetitorControl.Name = "CompetitorControl";
			this.CompetitorControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CompetitorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.CompetitorControl.TabIndex = 0;
			this.CompetitorTabPage.ResumeLayout(true);
		}

		private void MiscServicesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MiscServicesNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MiscServicesControl = new Enterprise.MasterFiles.GUI.MiscServicesUserControl();
			this.MiscServicesTabPage.SuspendLayout();
			this.MiscServicesTabPage.Controls.Add(this.MiscServicesNotSelectedLabel);
			this.MiscServicesTabPage.Controls.Add(this.MiscServicesControl);
			// 
			// MiscServicesNotSelectedLabel
			// 
			this.MiscServicesNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|c0f03d6b-9a6b-42af-beb2-042c76d3f6e4", "You must select an Organization type of Misc. Services from the Details page to use this page.");
			this.MiscServicesNotSelectedLabel.Name = "MiscServicesNotSelectedLabel";
			this.MiscServicesNotSelectedLabel.TabIndex = 1;
			this.MiscServicesNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.MiscServicesNotSelectedLabel.AutoSize = false;
			this.MiscServicesNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// MiscServicesControl
			// 
			this.MiscServicesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MiscServicesControl, ".");
			this.MiscServicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscServicesControl.IsModifyAddress = false;
			this.MiscServicesControl.IsModifyCarrier = false;
			this.MiscServicesControl.IsModifyCompetitor = false;
			this.MiscServicesControl.IsModifyConfig = false;
			this.MiscServicesControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.MiscServicesControl.IsModifyConfigEDICodeMapping = false;
			this.MiscServicesControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.MiscServicesControl.IsModifyConfigGeneral = false;
			this.MiscServicesControl.IsModifyConfigRegistrationNumbers = false;
			this.MiscServicesControl.IsModifyConsignee = false;
			this.MiscServicesControl.IsModifyConsigneeDetails = false;
			this.MiscServicesControl.IsModifyConsigneeLandedCosting = false;
			this.MiscServicesControl.IsModifyConsigneeRelationships = false;
			this.MiscServicesControl.IsModifyConsignor = false;
			this.MiscServicesControl.IsModifyConsignorDetails = false;
			this.MiscServicesControl.IsModifyConsignorExporterScheme = false;
			this.MiscServicesControl.IsModifyConsignorRelationships = false;
			this.MiscServicesControl.IsModifyContact = false;
			this.MiscServicesControl.IsModifyContactContactDetails = false;
			this.MiscServicesControl.IsModifyContactDocDeliveryDetails = false;
			this.MiscServicesControl.IsModifyContactPersonalInformation = false;
			this.MiscServicesControl.IsModifyCustom = false;
			this.MiscServicesControl.IsModifyDetails = false;
			this.MiscServicesControl.IsModifyDetailsNameAndAddress = false;
			this.MiscServicesControl.IsModifyDetailsOrganisationType = false;
			this.MiscServicesControl.IsModifyDetailsPhFaxWebDetails = false;
			this.MiscServicesControl.IsModifyDetailsRatingAndTariffs = false;
			this.MiscServicesControl.IsModifyDetailsStaffAssignments = false;
			this.MiscServicesControl.IsModifyDetailsWebSecurity = false;
			this.MiscServicesControl.IsModifyForwarder = false;
			this.MiscServicesControl.IsModifyForwarderDetails = false;
			this.MiscServicesControl.IsModifyForwarderProfitShare = false;
			this.MiscServicesControl.IsModifyPayables = false;
			this.MiscServicesControl.IsModifyReceivables = false;
			this.MiscServicesControl.IsModifyReceivablesConfig = false;
			this.MiscServicesControl.IsModifyReceivablesInvoicing = false;
			this.MiscServicesControl.IsModifySales = false;
			this.MiscServicesControl.IsModifySalesClientRelationship = false;
			this.MiscServicesControl.IsModifySalesClientSummary = false;
			this.MiscServicesControl.IsModifySalesOpportunityManagement = false;
			this.MiscServicesControl.IsModifySalesTradeProfile = false;
			this.MiscServicesControl.IsModifyServices = true;
			this.MiscServicesControl.IsModifyWarehouse = false;
			this.MiscServicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscServicesControl.Name = "MiscServicesControl";
			this.MiscServicesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.MiscServicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.MiscServicesControl.TabIndex = 0;
			this.MiscServicesTabPage.ResumeLayout(true);
		}

		private void TransportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TransportNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TransportControl = new Enterprise.MasterFiles.GUI.CarrierUserControl();
			this.TransportTabPage.SuspendLayout();
			this.TransportTabPage.Controls.Add(this.TransportNotSelectedLabel);
			this.TransportTabPage.Controls.Add(this.TransportControl);
			// 
			// TransportNotSelectedLabel
			// 
			this.TransportNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|d7247cb6-43ac-47be-9014-7396fc445f01", "You must select an Organization type of Carrier from the Details page to use this page.");
			this.TransportNotSelectedLabel.Name = "TransportNotSelectedLabel";
			this.TransportNotSelectedLabel.TabIndex = 1;
			this.TransportNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.TransportNotSelectedLabel.AutoSize = false;
			this.TransportNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// TransportControl
			// 
			this.TransportControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportControl, ".");
			this.TransportControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportControl.IsModifyAddress = false;
			this.TransportControl.IsModifyCarrier = true;
			this.TransportControl.IsModifyCompetitor = false;
			this.TransportControl.IsModifyConfig = false;
			this.TransportControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.TransportControl.IsModifyConfigEDICodeMapping = false;
			this.TransportControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.TransportControl.IsModifyConfigGeneral = false;
			this.TransportControl.IsModifyConfigRegistrationNumbers = false;
			this.TransportControl.IsModifyConsignee = false;
			this.TransportControl.IsModifyConsigneeDetails = false;
			this.TransportControl.IsModifyConsigneeLandedCosting = false;
			this.TransportControl.IsModifyConsigneeRelationships = false;
			this.TransportControl.IsModifyConsignor = false;
			this.TransportControl.IsModifyConsignorDetails = false;
			this.TransportControl.IsModifyConsignorExporterScheme = false;
			this.TransportControl.IsModifyConsignorRelationships = false;
			this.TransportControl.IsModifyContact = false;
			this.TransportControl.IsModifyContactContactDetails = false;
			this.TransportControl.IsModifyContactDocDeliveryDetails = false;
			this.TransportControl.IsModifyContactPersonalInformation = false;
			this.TransportControl.IsModifyCustom = false;
			this.TransportControl.IsModifyDetails = false;
			this.TransportControl.IsModifyDetailsNameAndAddress = false;
			this.TransportControl.IsModifyDetailsOrganisationType = false;
			this.TransportControl.IsModifyDetailsPhFaxWebDetails = false;
			this.TransportControl.IsModifyDetailsRatingAndTariffs = false;
			this.TransportControl.IsModifyDetailsStaffAssignments = false;
			this.TransportControl.IsModifyDetailsWebSecurity = false;
			this.TransportControl.IsModifyForwarder = false;
			this.TransportControl.IsModifyForwarderDetails = false;
			this.TransportControl.IsModifyForwarderProfitShare = false;
			this.TransportControl.IsModifyPayables = false;
			this.TransportControl.IsModifyReceivables = false;
			this.TransportControl.IsModifyReceivablesConfig = false;
			this.TransportControl.IsModifyReceivablesInvoicing = false;
			this.TransportControl.IsModifySales = false;
			this.TransportControl.IsModifySalesClientRelationship = false;
			this.TransportControl.IsModifySalesClientSummary = false;
			this.TransportControl.IsModifySalesOpportunityManagement = false;
			this.TransportControl.IsModifySalesTradeProfile = false;
			this.TransportControl.IsModifyServices = false;
			this.TransportControl.IsModifyWarehouse = false;
			this.TransportControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportControl.Name = "TransportControl";
			this.TransportControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.TransportControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.TransportControl.TabIndex = 0;
			this.TransportTabPage.ResumeLayout(true);
		}

		private void ForwarderTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ForwarderNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ForwarderControl = new Enterprise.MasterFiles.GUI.ForwarderUserControl();
			this.ForwarderTabPage.SuspendLayout();
			this.ForwarderTabPage.Controls.Add(this.ForwarderNotSelectedLabel);
			this.ForwarderTabPage.Controls.Add(this.ForwarderControl);
			// 
			// ForwarderNotSelectedLabel
			// 
			this.ForwarderNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|ef31b6e3-dc31-41ea-ad41-997ade1cf135", "You must select an Organization type of Forwarder/Agent from the Details page to use this page.");
			this.ForwarderNotSelectedLabel.Name = "ForwarderNotSelectedLabel";
			this.ForwarderNotSelectedLabel.TabIndex = 1;
			this.ForwarderNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ForwarderNotSelectedLabel.AutoSize = false;
			this.ForwarderNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ForwarderControl
			// 
			this.ForwarderControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderControl, ".");
			this.ForwarderControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForwarderControl.IsModifyAddress = false;
			this.ForwarderControl.IsModifyCarrier = false;
			this.ForwarderControl.IsModifyCompetitor = false;
			this.ForwarderControl.IsModifyConfig = false;
			this.ForwarderControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ForwarderControl.IsModifyConfigEDICodeMapping = false;
			this.ForwarderControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ForwarderControl.IsModifyConfigGeneral = false;
			this.ForwarderControl.IsModifyConfigRegistrationNumbers = false;
			this.ForwarderControl.IsModifyConsignee = false;
			this.ForwarderControl.IsModifyConsigneeDetails = false;
			this.ForwarderControl.IsModifyConsigneeLandedCosting = false;
			this.ForwarderControl.IsModifyConsigneeRelationships = false;
			this.ForwarderControl.IsModifyConsignor = false;
			this.ForwarderControl.IsModifyConsignorDetails = false;
			this.ForwarderControl.IsModifyConsignorExporterScheme = false;
			this.ForwarderControl.IsModifyConsignorRelationships = false;
			this.ForwarderControl.IsModifyContact = false;
			this.ForwarderControl.IsModifyContactContactDetails = false;
			this.ForwarderControl.IsModifyContactDocDeliveryDetails = false;
			this.ForwarderControl.IsModifyContactPersonalInformation = false;
			this.ForwarderControl.IsModifyCustom = false;
			this.ForwarderControl.IsModifyDetails = false;
			this.ForwarderControl.IsModifyDetailsNameAndAddress = false;
			this.ForwarderControl.IsModifyDetailsOrganisationType = false;
			this.ForwarderControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ForwarderControl.IsModifyDetailsRatingAndTariffs = false;
			this.ForwarderControl.IsModifyDetailsStaffAssignments = false;
			this.ForwarderControl.IsModifyDetailsWebSecurity = false;
			this.ForwarderControl.IsModifyForwarder = true;
			this.ForwarderControl.IsModifyForwarderDetails = true;
			this.ForwarderControl.IsModifyForwarderProfitShare = true;
			this.ForwarderControl.IsModifyPayables = false;
			this.ForwarderControl.IsModifyReceivables = false;
			this.ForwarderControl.IsModifyReceivablesConfig = false;
			this.ForwarderControl.IsModifyReceivablesInvoicing = false;
			this.ForwarderControl.IsModifySales = false;
			this.ForwarderControl.IsModifySalesClientRelationship = false;
			this.ForwarderControl.IsModifySalesClientSummary = false;
			this.ForwarderControl.IsModifySalesOpportunityManagement = false;
			this.ForwarderControl.IsModifySalesTradeProfile = false;
			this.ForwarderControl.IsModifyServices = false;
			this.ForwarderControl.IsModifyWarehouse = false;
			this.ForwarderControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ForwarderControl.Name = "ForwarderControl";
			this.ForwarderControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ForwarderControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ForwarderControl.TabIndex = 0;
			this.ForwarderTabPage.ResumeLayout(true);
		}

		private void ConsignorTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConsignorNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorControl = new Enterprise.MasterFiles.GUI.ConsignorUserControl();
			this.ConsignorTabPage.SuspendLayout();
			this.ConsignorTabPage.Controls.Add(this.ConsignorNotSelectedLabel);
			this.ConsignorTabPage.Controls.Add(this.ConsignorControl);
			// 
			// ConsignorNotSelectedLabel
			// 
			this.ConsignorNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|e39ee8e6-b41b-4b3e-8941-2265f7d69883", "You must select an Organization type of Consignor from the Details page to use this page.");
			this.ConsignorNotSelectedLabel.Name = "ConsignorNotSelectedLabel";
			this.ConsignorNotSelectedLabel.TabIndex = 1;
			this.ConsignorNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConsignorNotSelectedLabel.AutoSize = false;
			this.ConsignorNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ConsignorControl
			// 
			this.ConsignorControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorControl, ".");
			this.ConsignorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignorControl.IsModifyAddress = false;
			this.ConsignorControl.IsModifyCarrier = false;
			this.ConsignorControl.IsModifyCompetitor = false;
			this.ConsignorControl.IsModifyConfig = false;
			this.ConsignorControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ConsignorControl.IsModifyConfigEDICodeMapping = false;
			this.ConsignorControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ConsignorControl.IsModifyConfigGeneral = false;
			this.ConsignorControl.IsModifyConfigRegistrationNumbers = false;
			this.ConsignorControl.IsModifyConsignee = false;
			this.ConsignorControl.IsModifyConsigneeDetails = false;
			this.ConsignorControl.IsModifyConsigneeLandedCosting = false;
			this.ConsignorControl.IsModifyConsigneeRelationships = false;
			this.ConsignorControl.IsModifyConsignor = true;
			this.ConsignorControl.IsModifyConsignorDetails = true;
			this.ConsignorControl.IsModifyConsignorExporterScheme = true;
			this.ConsignorControl.IsModifyConsignorRelationships = true;
			this.ConsignorControl.IsModifyContact = false;
			this.ConsignorControl.IsModifyContactContactDetails = false;
			this.ConsignorControl.IsModifyContactDocDeliveryDetails = false;
			this.ConsignorControl.IsModifyContactPersonalInformation = false;
			this.ConsignorControl.IsModifyCustom = false;
			this.ConsignorControl.IsModifyDetails = false;
			this.ConsignorControl.IsModifyDetailsNameAndAddress = false;
			this.ConsignorControl.IsModifyDetailsOrganisationType = false;
			this.ConsignorControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ConsignorControl.IsModifyDetailsRatingAndTariffs = false;
			this.ConsignorControl.IsModifyDetailsStaffAssignments = false;
			this.ConsignorControl.IsModifyDetailsWebSecurity = false;
			this.ConsignorControl.IsModifyForwarder = false;
			this.ConsignorControl.IsModifyForwarderDetails = false;
			this.ConsignorControl.IsModifyForwarderProfitShare = false;
			this.ConsignorControl.IsModifyPayables = false;
			this.ConsignorControl.IsModifyReceivables = false;
			this.ConsignorControl.IsModifyReceivablesConfig = false;
			this.ConsignorControl.IsModifyReceivablesInvoicing = false;
			this.ConsignorControl.IsModifySales = false;
			this.ConsignorControl.IsModifySalesClientRelationship = false;
			this.ConsignorControl.IsModifySalesClientSummary = false;
			this.ConsignorControl.IsModifySalesOpportunityManagement = false;
			this.ConsignorControl.IsModifySalesTradeProfile = false;
			this.ConsignorControl.IsModifyServices = false;
			this.ConsignorControl.IsModifyWarehouse = false;
			this.ConsignorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignorControl.Name = "ConsignorControl";
			this.ConsignorControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ConsignorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ConsignorControl.TabIndex = 0;
			this.ConsignorTabPage.ResumeLayout(true);
		}

		private void ConsigneeTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConsigneeNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeControl = new Enterprise.MasterFiles.GUI.ConsigneeUserControl();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeNotSelectedLabel);
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeControl);
			// 
			// ConsigneeNotSelectedLabel
			// 
			this.ConsigneeNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|e7755c16-a4dd-498b-adb9-135675e4844a", "You must select an Organization type of Consignee from the Details page to use this page.");
			this.ConsigneeNotSelectedLabel.Name = "ConsigneeNotSelectedLabel";
			this.ConsigneeNotSelectedLabel.TabIndex = 1;
			this.ConsigneeNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConsigneeNotSelectedLabel.AutoSize = false;
			this.ConsigneeNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ConsigneeControl
			// 
			this.ConsigneeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeControl, ".");
			this.ConsigneeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsigneeControl.IsModifyAddress = false;
			this.ConsigneeControl.IsModifyCarrier = false;
			this.ConsigneeControl.IsModifyCompetitor = false;
			this.ConsigneeControl.IsModifyConfig = false;
			this.ConsigneeControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ConsigneeControl.IsModifyConfigEDICodeMapping = false;
			this.ConsigneeControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ConsigneeControl.IsModifyConfigGeneral = false;
			this.ConsigneeControl.IsModifyConfigRegistrationNumbers = false;
			this.ConsigneeControl.IsModifyConsignee = true;
			this.ConsigneeControl.IsModifyConsigneeDetails = true;
			this.ConsigneeControl.IsModifyConsigneeLandedCosting = true;
			this.ConsigneeControl.IsModifyConsigneeRelationships = true;
			this.ConsigneeControl.IsModifyConsignor = false;
			this.ConsigneeControl.IsModifyConsignorDetails = false;
			this.ConsigneeControl.IsModifyConsignorExporterScheme = false;
			this.ConsigneeControl.IsModifyConsignorRelationships = false;
			this.ConsigneeControl.IsModifyContact = false;
			this.ConsigneeControl.IsModifyContactContactDetails = false;
			this.ConsigneeControl.IsModifyContactDocDeliveryDetails = false;
			this.ConsigneeControl.IsModifyContactPersonalInformation = false;
			this.ConsigneeControl.IsModifyCustom = false;
			this.ConsigneeControl.IsModifyDetails = false;
			this.ConsigneeControl.IsModifyDetailsNameAndAddress = false;
			this.ConsigneeControl.IsModifyDetailsOrganisationType = false;
			this.ConsigneeControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ConsigneeControl.IsModifyDetailsRatingAndTariffs = false;
			this.ConsigneeControl.IsModifyDetailsStaffAssignments = false;
			this.ConsigneeControl.IsModifyDetailsWebSecurity = false;
			this.ConsigneeControl.IsModifyForwarder = false;
			this.ConsigneeControl.IsModifyForwarderDetails = false;
			this.ConsigneeControl.IsModifyForwarderProfitShare = false;
			this.ConsigneeControl.IsModifyPayables = false;
			this.ConsigneeControl.IsModifyReceivables = false;
			this.ConsigneeControl.IsModifyReceivablesConfig = false;
			this.ConsigneeControl.IsModifyReceivablesInvoicing = false;
			this.ConsigneeControl.IsModifySales = false;
			this.ConsigneeControl.IsModifySalesClientRelationship = false;
			this.ConsigneeControl.IsModifySalesClientSummary = false;
			this.ConsigneeControl.IsModifySalesOpportunityManagement = false;
			this.ConsigneeControl.IsModifySalesTradeProfile = false;
			this.ConsigneeControl.IsModifyServices = false;
			this.ConsigneeControl.IsModifyWarehouse = false;
			this.ConsigneeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsigneeControl.Name = "ConsigneeControl";
			this.ConsigneeControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ConsigneeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ConsigneeControl.TabIndex = 0;
			this.ConsigneeTabPage.ResumeLayout(true);
		}

		private void ReceivablesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ReceivablesNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReceivablesControl = new Enterprise.MasterFiles.GUI.ReceivablesUserControl();
			this.ReceivablesTabPage.SuspendLayout();
			this.ReceivablesTabPage.Controls.Add(this.ReceivablesNotSelectedLabel);
			this.ReceivablesTabPage.Controls.Add(this.ReceivablesControl);
			// 
			// ReceivablesNotSelectedLabel
			// 
			this.ReceivablesNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|16c003d8-2728-40c9-9848-37d3767bca3a", "You must select an Organization type of Receivables from the Details page to use this page.");
			this.ReceivablesNotSelectedLabel.Name = "ReceivablesNotSelectedLabel";
			this.ReceivablesNotSelectedLabel.TabIndex = 1;
			this.ReceivablesNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ReceivablesNotSelectedLabel.AutoSize = false;
			this.ReceivablesNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ReceivablesControl
			// 
			this.ReceivablesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivablesControl, ".");
			this.ReceivablesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivablesControl.IsModifyAddress = false;
			this.ReceivablesControl.IsModifyCarrier = false;
			this.ReceivablesControl.IsModifyCompetitor = false;
			this.ReceivablesControl.IsModifyConfig = false;
			this.ReceivablesControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ReceivablesControl.IsModifyConfigEDICodeMapping = false;
			this.ReceivablesControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ReceivablesControl.IsModifyConfigGeneral = false;
			this.ReceivablesControl.IsModifyConfigRegistrationNumbers = false;
			this.ReceivablesControl.IsModifyConsignee = false;
			this.ReceivablesControl.IsModifyConsigneeDetails = false;
			this.ReceivablesControl.IsModifyConsigneeLandedCosting = false;
			this.ReceivablesControl.IsModifyConsigneeRelationships = false;
			this.ReceivablesControl.IsModifyConsignor = false;
			this.ReceivablesControl.IsModifyConsignorDetails = false;
			this.ReceivablesControl.IsModifyConsignorExporterScheme = false;
			this.ReceivablesControl.IsModifyConsignorRelationships = false;
			this.ReceivablesControl.IsModifyContact = false;
			this.ReceivablesControl.IsModifyContactContactDetails = false;
			this.ReceivablesControl.IsModifyContactDocDeliveryDetails = false;
			this.ReceivablesControl.IsModifyContactPersonalInformation = false;
			this.ReceivablesControl.IsModifyCustom = false;
			this.ReceivablesControl.IsModifyDetails = false;
			this.ReceivablesControl.IsModifyDetailsNameAndAddress = false;
			this.ReceivablesControl.IsModifyDetailsOrganisationType = false;
			this.ReceivablesControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ReceivablesControl.IsModifyDetailsRatingAndTariffs = false;
			this.ReceivablesControl.IsModifyDetailsStaffAssignments = false;
			this.ReceivablesControl.IsModifyDetailsWebSecurity = false;
			this.ReceivablesControl.IsModifyForwarder = false;
			this.ReceivablesControl.IsModifyForwarderDetails = false;
			this.ReceivablesControl.IsModifyForwarderProfitShare = false;
			this.ReceivablesControl.IsModifyPayables = false;
			this.ReceivablesControl.IsModifyReceivables = true;
			this.ReceivablesControl.IsModifyReceivablesConfig = true;
			this.ReceivablesControl.IsModifyReceivablesInvoicing = true;
			this.ReceivablesControl.IsModifySales = false;
			this.ReceivablesControl.IsModifySalesClientRelationship = false;
			this.ReceivablesControl.IsModifySalesClientSummary = false;
			this.ReceivablesControl.IsModifySalesOpportunityManagement = false;
			this.ReceivablesControl.IsModifySalesTradeProfile = false;
			this.ReceivablesControl.IsModifyServices = false;
			this.ReceivablesControl.IsModifyWarehouse = false;
			this.ReceivablesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceivablesControl.Name = "ReceivablesControl";
			this.ReceivablesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ReceivablesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.ReceivablesControl.TabIndex = 2;
			this.ReceivablesTabPage.ResumeLayout(true);
		}

		private void PayablesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.PayablesNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PayablesControl = new Enterprise.MasterFiles.GUI.PayablesUserControl();
			this.PayablesTabPage.SuspendLayout();
			this.PayablesTabPage.Controls.Add(this.PayablesNotSelectedLabel);
			this.PayablesTabPage.Controls.Add(this.PayablesControl);
			// 
			// PayablesNotSelectedLabel
			// 
			this.PayablesNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|f3b39b35-56e5-4d07-9d66-9e8b5a51dcbc", "You must select an Organization type of Payables from the Details page to use this page.");
			this.PayablesNotSelectedLabel.Name = "PayablesNotSelectedLabel";
			this.PayablesNotSelectedLabel.TabIndex = 1;
			this.PayablesNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PayablesNotSelectedLabel.AutoSize = false;
			this.PayablesNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// PayablesControl
			// 
			this.PayablesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PayablesControl, ".");
			this.PayablesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayablesControl.IsModifyAddress = false;
			this.PayablesControl.IsModifyCarrier = false;
			this.PayablesControl.IsModifyCompetitor = false;
			this.PayablesControl.IsModifyConfig = false;
			this.PayablesControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.PayablesControl.IsModifyConfigEDICodeMapping = false;
			this.PayablesControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.PayablesControl.IsModifyConfigGeneral = false;
			this.PayablesControl.IsModifyConfigRegistrationNumbers = false;
			this.PayablesControl.IsModifyConsignee = false;
			this.PayablesControl.IsModifyConsigneeDetails = false;
			this.PayablesControl.IsModifyConsigneeLandedCosting = false;
			this.PayablesControl.IsModifyConsigneeRelationships = false;
			this.PayablesControl.IsModifyConsignor = false;
			this.PayablesControl.IsModifyConsignorDetails = false;
			this.PayablesControl.IsModifyConsignorExporterScheme = false;
			this.PayablesControl.IsModifyConsignorRelationships = false;
			this.PayablesControl.IsModifyContact = false;
			this.PayablesControl.IsModifyContactContactDetails = false;
			this.PayablesControl.IsModifyContactDocDeliveryDetails = false;
			this.PayablesControl.IsModifyContactPersonalInformation = false;
			this.PayablesControl.IsModifyCustom = false;
			this.PayablesControl.IsModifyDetails = false;
			this.PayablesControl.IsModifyDetailsNameAndAddress = false;
			this.PayablesControl.IsModifyDetailsOrganisationType = false;
			this.PayablesControl.IsModifyDetailsPhFaxWebDetails = false;
			this.PayablesControl.IsModifyDetailsRatingAndTariffs = false;
			this.PayablesControl.IsModifyDetailsStaffAssignments = false;
			this.PayablesControl.IsModifyDetailsWebSecurity = false;
			this.PayablesControl.IsModifyForwarder = false;
			this.PayablesControl.IsModifyForwarderDetails = false;
			this.PayablesControl.IsModifyForwarderProfitShare = false;
			this.PayablesControl.IsModifyPayables = true;
			this.PayablesControl.IsModifyReceivables = false;
			this.PayablesControl.IsModifyReceivablesConfig = false;
			this.PayablesControl.IsModifyReceivablesInvoicing = false;
			this.PayablesControl.IsModifySales = false;
			this.PayablesControl.IsModifySalesClientRelationship = false;
			this.PayablesControl.IsModifySalesClientSummary = false;
			this.PayablesControl.IsModifySalesOpportunityManagement = false;
			this.PayablesControl.IsModifySalesTradeProfile = false;
			this.PayablesControl.IsModifyServices = false;
			this.PayablesControl.IsModifyWarehouse = false;
			this.PayablesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PayablesControl.Name = "PayablesControl";
			this.PayablesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.PayablesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 562, true);
			this.PayablesControl.TabIndex = 0;
			this.PayablesTabPage.ResumeLayout(true);
		}

		private void WarehouseTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WarehouseNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WhsFacilityUserControl = new Enterprise.MasterFiles.GUI.WhsFacilityUserControl();
			this.WhsFacilityTabPage.SuspendLayout();
			this.WhsFacilityTabPage.Controls.Add(this.WarehouseNotSelectedLabel);
			this.WhsFacilityTabPage.Controls.Add(this.WhsFacilityUserControl);
			// 
			// WarehouseNotSelectedLabel
			// 
			this.WarehouseNotSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZOrganisationsForm|b0062dc4-d355-4d7b-bbbe-6fa6f908039a", "You must select an Organization type of Warehouse from the Details page to use this page.");
			this.WarehouseNotSelectedLabel.Name = "WarehouseNotSelectedLabel";
			this.WarehouseNotSelectedLabel.TabIndex = 1;
			this.WarehouseNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.WarehouseNotSelectedLabel.AutoSize = false;
			this.WarehouseNotSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// WhsFacilityUserControl
			// 
			this.WhsFacilityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsFacilityUserControl, ".");
			this.WhsFacilityUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WhsFacilityUserControl.IsModifyAddress = false;
			this.WhsFacilityUserControl.IsModifyCarrier = false;
			this.WhsFacilityUserControl.IsModifyCompetitor = false;
			this.WhsFacilityUserControl.IsModifyConfig = false;
			this.WhsFacilityUserControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.WhsFacilityUserControl.IsModifyConfigEDICodeMapping = false;
			this.WhsFacilityUserControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.WhsFacilityUserControl.IsModifyConfigGeneral = false;
			this.WhsFacilityUserControl.IsModifyConfigRegistrationNumbers = false;
			this.WhsFacilityUserControl.IsModifyConsignee = false;
			this.WhsFacilityUserControl.IsModifyConsigneeDetails = false;
			this.WhsFacilityUserControl.IsModifyConsigneeLandedCosting = false;
			this.WhsFacilityUserControl.IsModifyConsigneeRelationships = false;
			this.WhsFacilityUserControl.IsModifyConsignor = false;
			this.WhsFacilityUserControl.IsModifyConsignorDetails = false;
			this.WhsFacilityUserControl.IsModifyConsignorExporterScheme = false;
			this.WhsFacilityUserControl.IsModifyConsignorRelationships = false;
			this.WhsFacilityUserControl.IsModifyContact = false;
			this.WhsFacilityUserControl.IsModifyContactContactDetails = false;
			this.WhsFacilityUserControl.IsModifyContactDocDeliveryDetails = false;
			this.WhsFacilityUserControl.IsModifyContactPersonalInformation = false;
			this.WhsFacilityUserControl.IsModifyCustom = false;
			this.WhsFacilityUserControl.IsModifyDetails = false;
			this.WhsFacilityUserControl.IsModifyDetailsNameAndAddress = false;
			this.WhsFacilityUserControl.IsModifyDetailsOrganisationType = false;
			this.WhsFacilityUserControl.IsModifyDetailsPhFaxWebDetails = false;
			this.WhsFacilityUserControl.IsModifyDetailsRatingAndTariffs = false;
			this.WhsFacilityUserControl.IsModifyDetailsStaffAssignments = false;
			this.WhsFacilityUserControl.IsModifyDetailsWebSecurity = false;
			this.WhsFacilityUserControl.IsModifyForwarder = false;
			this.WhsFacilityUserControl.IsModifyForwarderDetails = false;
			this.WhsFacilityUserControl.IsModifyForwarderProfitShare = false;
			this.WhsFacilityUserControl.IsModifyPayables = false;
			this.WhsFacilityUserControl.IsModifyReceivables = false;
			this.WhsFacilityUserControl.IsModifyReceivablesConfig = false;
			this.WhsFacilityUserControl.IsModifyReceivablesInvoicing = false;
			this.WhsFacilityUserControl.IsModifySales = false;
			this.WhsFacilityUserControl.IsModifySalesClientRelationship = false;
			this.WhsFacilityUserControl.IsModifySalesClientSummary = false;
			this.WhsFacilityUserControl.IsModifySalesOpportunityManagement = false;
			this.WhsFacilityUserControl.IsModifySalesTradeProfile = false;
			this.WhsFacilityUserControl.IsModifyServices = false;
			this.WhsFacilityUserControl.IsModifyWarehouse = true;
			this.WhsFacilityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.WhsFacilityUserControl.Name = "WarehouseUserControl";
			this.WhsFacilityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 556, true);
			this.WhsFacilityUserControl.TabIndex = 0;
			this.WhsFacilityTabPage.ResumeLayout(true);
		}

		private void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsTabPage.SuspendLayout();
			// 
			// DetailsControl
			// 
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 566, true);

			InitializeDetailsPageChildTabs(DetailsControl.DetailsTabControl);

			this.DetailsTabPage.ResumeLayout(true);
		}

		protected virtual void InitializeDetailsPageChildTabs(Enterprise.ZArchitecture.GUI.ZTemplateTabControl tabControl)
		{
		}

		#endregion

	}
}
