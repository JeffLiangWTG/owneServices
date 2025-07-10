namespace Enterprise.MasterFiles.GUI
{
	public partial class ZClientIntelligenceForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZTabPage SalesTabPage;
		private Enterprise.MasterFiles.GUI.SalesUserControl SalesControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ConsigneeTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ConsignorTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ReceivablesTabPage;
		private Enterprise.MasterFiles.GUI.ConsigneeUserControl ConsigneePageControl;
		private Enterprise.MasterFiles.GUI.ConsignorUserControl ConsignorPageControl;
		private Enterprise.ZArchitecture.ZLabel ReceivablesNotSelectedLabel;
		private Enterprise.ZArchitecture.ZLabel ConsignorNotSelectedLabel;
		private Enterprise.ZArchitecture.ZLabel ConsigneeNotSelectedLabel;
		private Enterprise.MasterFiles.GUI.ReceivablesUserControl ReceivablesUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

		protected override void InitializeComponent()
		{
			this.SalesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsigneeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReceivablesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.OrganisationsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrganisationsTabControl
			// 
			this.OrganisationsTabControl.Controls.Add(this.SalesTabPage);
			this.OrganisationsTabControl.Controls.Add(this.ReceivablesTabPage);
			this.OrganisationsTabControl.Controls.Add(this.ConsignorTabPage);
			this.OrganisationsTabControl.Controls.Add(this.ConsigneeTabPage);
			this.OrganisationsTabControl.Controls.Add(this.WorkflowTabPage);
			this.OrganisationsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 596, true);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.StmNoteTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.ConsigneeTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.ConsignorTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.ReceivablesTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.SalesTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.AddressesTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.ContactsTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.DetailsTabPage, 0);
			// 
			// ContactsTabPage
			// 
			this.ContactsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// 
			// AddressesTabPage
			// 
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(932, 637, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 640, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 23, true);
			// 
			// SalesTabPage
			// 
			this.SalesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZClientIntelligenceForm|4f1ff12c-61ee-4415-ae60-1db7572d7a86", "Sales");
			this.SalesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SalesTabPage.Name = "SalesTabPage";
			this.SalesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.SalesTabPage.TabIndex = 18;
			this.SalesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SalesTabPage_InitializeTab));
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZClientIntelligenceForm|cc8c0e29-acff-40f1-822e-f7addf1e460d", "Consignee");
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ConsigneeTabPage.TabIndex = 19;
			this.ConsigneeTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConsigneeTabPage_InitializeTab));
			// 
			// ConsignorTabPage
			// 
			this.ConsignorTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZClientIntelligenceForm|8d3a5dcd-343d-47fd-bf1d-76b349f56d1d", "Consignor");
			this.ConsignorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsignorTabPage.Name = "ConsignorTabPage";
			this.ConsignorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ConsignorTabPage.TabIndex = 20;
			this.ConsignorTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConsignorTabPage_InitializeTab));
			// 
			// ReceivablesTabPage
			// 
			this.ReceivablesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZClientIntelligenceForm|8be574f1-c988-4825-95a3-9be7d3273d2f", "A/R");
			this.ReceivablesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReceivablesTabPage.Name = "ReceivablesTabPage";
			this.ReceivablesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ReceivablesTabPage.TabIndex = 21;
			this.ReceivablesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ReceivablesTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 458, true);
			this.WorkflowTabPage.TabIndex = 22;
			// 
			// ZClientIntelligenceForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZClientIntelligenceForm|38329812-06fa-400c-8129-1678d43fb898", "Client Intelligence");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 687, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 725, true);
			this.Name = "ZClientIntelligenceForm";
			this.OrganisationsTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		private void SalesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.SalesControl = new Enterprise.MasterFiles.GUI.SalesUserControl();
			this.SalesTabPage.SuspendLayout();
			this.SalesTabPage.Controls.Add(this.SalesControl);
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
			this.SalesControl.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.SalesControl.IsNewDetailsWebSecurity = false;
			this.SalesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SalesControl.Name = "SalesControl";
			this.SalesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.SalesControl.TabIndex = 0;
			this.SalesTabPage.ResumeLayout(true);
		}

		private void ConsigneeTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConsigneeNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneePageControl = new Enterprise.MasterFiles.GUI.ConsigneeUserControl();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeNotSelectedLabel);
			this.ConsigneeTabPage.Controls.Add(this.ConsigneePageControl);
			// 
			// ConsigneeNotSelectedLabel
			// 
			this.ConsigneeNotSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 236, true);
			this.ConsigneeNotSelectedLabel.Name = "ConsigneeNotSelectedLabel";
			this.ConsigneeNotSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 37, true);
			this.ConsigneeNotSelectedLabel.TabIndex = 2;
			this.ConsigneeNotSelectedLabel.Text = "You must select an Organization type of Consignee from the Details page to use th" +
				"is page.";
			this.ConsigneeNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ConsigneePageControl
			// 
			this.ConsigneePageControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneePageControl, ".");
			this.ConsigneePageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsigneePageControl.IsModifyAddress = false;
			this.ConsigneePageControl.IsModifyCarrier = false;
			this.ConsigneePageControl.IsModifyCompetitor = false;
			this.ConsigneePageControl.IsModifyConfig = false;
			this.ConsigneePageControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ConsigneePageControl.IsModifyConfigEDICodeMapping = false;
			this.ConsigneePageControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ConsigneePageControl.IsModifyConfigGeneral = false;
			this.ConsigneePageControl.IsModifyConfigRegistrationNumbers = false;
			this.ConsigneePageControl.IsModifyConsignee = true;
			this.ConsigneePageControl.IsModifyConsigneeDetails = true;
			this.ConsigneePageControl.IsModifyConsigneeLandedCosting = true;
			this.ConsigneePageControl.IsModifyConsigneeRelationships = true;
			this.ConsigneePageControl.IsModifyConsignor = false;
			this.ConsigneePageControl.IsModifyConsignorDetails = false;
			this.ConsigneePageControl.IsModifyConsignorExporterScheme = false;
			this.ConsigneePageControl.IsModifyConsignorRelationships = false;
			this.ConsigneePageControl.IsModifyContact = false;
			this.ConsigneePageControl.IsModifyContactContactDetails = false;
			this.ConsigneePageControl.IsModifyContactDocDeliveryDetails = false;
			this.ConsigneePageControl.IsModifyContactPersonalInformation = false;
			this.ConsigneePageControl.IsModifyCustom = false;
			this.ConsigneePageControl.IsModifyDetails = false;
			this.ConsigneePageControl.IsModifyDetailsNameAndAddress = false;
			this.ConsigneePageControl.IsModifyDetailsOrganisationType = false;
			this.ConsigneePageControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ConsigneePageControl.IsModifyDetailsRatingAndTariffs = false;
			this.ConsigneePageControl.IsModifyDetailsStaffAssignments = false;
			this.ConsigneePageControl.IsModifyDetailsWebSecurity = false;
			this.ConsigneePageControl.IsModifyForwarder = false;
			this.ConsigneePageControl.IsModifyForwarderDetails = false;
			this.ConsigneePageControl.IsModifyForwarderProfitShare = false;
			this.ConsigneePageControl.IsModifyPayables = false;
			this.ConsigneePageControl.IsModifyReceivables = false;
			this.ConsigneePageControl.IsModifyReceivablesConfig = false;
			this.ConsigneePageControl.IsModifyReceivablesInvoicing = false;
			this.ConsigneePageControl.IsModifySales = false;
			this.ConsigneePageControl.IsModifySalesClientRelationship = false;
			this.ConsigneePageControl.IsModifySalesClientSummary = false;
			this.ConsigneePageControl.IsModifySalesOpportunityManagement = false;
			this.ConsigneePageControl.IsModifySalesTradeProfile = false;
			this.ConsigneePageControl.IsModifyServices = false;
			this.ConsigneePageControl.IsModifyWarehouse = true;
			this.ConsigneePageControl.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.ConsigneePageControl.IsNewDetailsWebSecurity = false;
			this.ConsigneePageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsigneePageControl.Name = "ConsigneePageControl";
			this.ConsigneePageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ConsigneePageControl.TabIndex = 0;
			this.ConsigneeTabPage.ResumeLayout(true);
		}

		private void ConsignorTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConsignorNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorPageControl = new Enterprise.MasterFiles.GUI.ConsignorUserControl();
			this.ConsignorTabPage.SuspendLayout();
			this.ConsignorTabPage.Controls.Add(this.ConsignorNotSelectedLabel);
			this.ConsignorTabPage.Controls.Add(this.ConsignorPageControl);
			// 
			// ConsignorNotSelectedLabel
			// 
			this.ConsignorNotSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 236, true);
			this.ConsignorNotSelectedLabel.Name = "ConsignorNotSelectedLabel";
			this.ConsignorNotSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 37, true);
			this.ConsignorNotSelectedLabel.TabIndex = 2;
			this.ConsignorNotSelectedLabel.Text = "You must select an Organization type of Consignor from the Details page to use th" +
				"is page.";
			this.ConsignorNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ConsignorPageControl
			// 
			this.ConsignorPageControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorPageControl, ".");
			this.ConsignorPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignorPageControl.IsModifyAddress = false;
			this.ConsignorPageControl.IsModifyCarrier = false;
			this.ConsignorPageControl.IsModifyCompetitor = false;
			this.ConsignorPageControl.IsModifyConfig = false;
			this.ConsignorPageControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ConsignorPageControl.IsModifyConfigEDICodeMapping = false;
			this.ConsignorPageControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ConsignorPageControl.IsModifyConfigGeneral = false;
			this.ConsignorPageControl.IsModifyConfigRegistrationNumbers = false;
			this.ConsignorPageControl.IsModifyConsignee = false;
			this.ConsignorPageControl.IsModifyConsigneeDetails = false;
			this.ConsignorPageControl.IsModifyConsigneeLandedCosting = false;
			this.ConsignorPageControl.IsModifyConsigneeRelationships = false;
			this.ConsignorPageControl.IsModifyConsignor = true;
			this.ConsignorPageControl.IsModifyConsignorDetails = true;
			this.ConsignorPageControl.IsModifyConsignorExporterScheme = true;
			this.ConsignorPageControl.IsModifyConsignorRelationships = true;
			this.ConsignorPageControl.IsModifyContact = false;
			this.ConsignorPageControl.IsModifyContactContactDetails = false;
			this.ConsignorPageControl.IsModifyContactDocDeliveryDetails = false;
			this.ConsignorPageControl.IsModifyContactPersonalInformation = false;
			this.ConsignorPageControl.IsModifyCustom = false;
			this.ConsignorPageControl.IsModifyDetails = false;
			this.ConsignorPageControl.IsModifyDetailsNameAndAddress = false;
			this.ConsignorPageControl.IsModifyDetailsOrganisationType = false;
			this.ConsignorPageControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ConsignorPageControl.IsModifyDetailsRatingAndTariffs = false;
			this.ConsignorPageControl.IsModifyDetailsStaffAssignments = false;
			this.ConsignorPageControl.IsModifyDetailsWebSecurity = false;
			this.ConsignorPageControl.IsModifyForwarder = false;
			this.ConsignorPageControl.IsModifyForwarderDetails = false;
			this.ConsignorPageControl.IsModifyForwarderProfitShare = false;
			this.ConsignorPageControl.IsModifyPayables = false;
			this.ConsignorPageControl.IsModifyReceivables = false;
			this.ConsignorPageControl.IsModifyReceivablesConfig = false;
			this.ConsignorPageControl.IsModifyReceivablesInvoicing = false;
			this.ConsignorPageControl.IsModifySales = false;
			this.ConsignorPageControl.IsModifySalesClientRelationship = false;
			this.ConsignorPageControl.IsModifySalesClientSummary = false;
			this.ConsignorPageControl.IsModifySalesOpportunityManagement = false;
			this.ConsignorPageControl.IsModifySalesTradeProfile = false;
			this.ConsignorPageControl.IsModifyServices = false;
			this.ConsignorPageControl.IsModifyWarehouse = false;
			this.ConsignorPageControl.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.ConsignorPageControl.IsNewDetailsWebSecurity = false;
			this.ConsignorPageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignorPageControl.Name = "ConsignorPageControl";
			this.ConsignorPageControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ConsignorPageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ConsignorPageControl.TabIndex = 0;
			this.ConsignorTabPage.ResumeLayout(true);
		}

		private void ReceivablesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ReceivablesUserControl = new Enterprise.MasterFiles.GUI.ReceivablesUserControl();
			this.ReceivablesNotSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReceivablesTabPage.SuspendLayout();
			this.ReceivablesTabPage.Controls.Add(this.ReceivablesUserControl);
			this.ReceivablesTabPage.Controls.Add(this.ReceivablesNotSelectedLabel);
			// 
			// ReceivablesUserControl
			// 
			this.ReceivablesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivablesUserControl, ".");
			this.ReceivablesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivablesUserControl.IsModifyAddress = false;
			this.ReceivablesUserControl.IsModifyCarrier = false;
			this.ReceivablesUserControl.IsModifyCompetitor = false;
			this.ReceivablesUserControl.IsModifyConfig = false;
			this.ReceivablesUserControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ReceivablesUserControl.IsModifyConfigEDICodeMapping = false;
			this.ReceivablesUserControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ReceivablesUserControl.IsModifyConfigGeneral = false;
			this.ReceivablesUserControl.IsModifyConfigRegistrationNumbers = false;
			this.ReceivablesUserControl.IsModifyConsignee = false;
			this.ReceivablesUserControl.IsModifyConsigneeDetails = false;
			this.ReceivablesUserControl.IsModifyConsigneeLandedCosting = false;
			this.ReceivablesUserControl.IsModifyConsigneeRelationships = false;
			this.ReceivablesUserControl.IsModifyConsignor = false;
			this.ReceivablesUserControl.IsModifyConsignorDetails = false;
			this.ReceivablesUserControl.IsModifyConsignorExporterScheme = false;
			this.ReceivablesUserControl.IsModifyConsignorRelationships = false;
			this.ReceivablesUserControl.IsModifyContact = false;
			this.ReceivablesUserControl.IsModifyContactContactDetails = false;
			this.ReceivablesUserControl.IsModifyContactDocDeliveryDetails = false;
			this.ReceivablesUserControl.IsModifyContactPersonalInformation = false;
			this.ReceivablesUserControl.IsModifyCustom = false;
			this.ReceivablesUserControl.IsModifyDetails = false;
			this.ReceivablesUserControl.IsModifyDetailsNameAndAddress = false;
			this.ReceivablesUserControl.IsModifyDetailsOrganisationType = false;
			this.ReceivablesUserControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ReceivablesUserControl.IsModifyDetailsRatingAndTariffs = false;
			this.ReceivablesUserControl.IsModifyDetailsStaffAssignments = false;
			this.ReceivablesUserControl.IsModifyDetailsWebSecurity = false;
			this.ReceivablesUserControl.IsModifyForwarder = false;
			this.ReceivablesUserControl.IsModifyForwarderDetails = false;
			this.ReceivablesUserControl.IsModifyForwarderProfitShare = false;
			this.ReceivablesUserControl.IsModifyPayables = false;
			this.ReceivablesUserControl.IsModifyReceivables = true;
			this.ReceivablesUserControl.IsModifyReceivablesConfig = true;
			this.ReceivablesUserControl.IsModifyReceivablesInvoicing = true;
			this.ReceivablesUserControl.IsModifySales = false;
			this.ReceivablesUserControl.IsModifySalesClientRelationship = false;
			this.ReceivablesUserControl.IsModifySalesClientSummary = false;
			this.ReceivablesUserControl.IsModifySalesOpportunityManagement = false;
			this.ReceivablesUserControl.IsModifySalesTradeProfile = false;
			this.ReceivablesUserControl.IsModifyServices = false;
			this.ReceivablesUserControl.IsModifyWarehouse = false;
			this.ReceivablesUserControl.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.ReceivablesUserControl.IsNewDetailsWebSecurity = false;
			this.ReceivablesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceivablesUserControl.Name = "ReceivablesUserControl";
			this.ReceivablesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.ReceivablesUserControl.TabIndex = 3;
			// 
			// ReceivablesNotSelectedLabel
			// 
			this.ReceivablesNotSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 236, true);
			this.ReceivablesNotSelectedLabel.Name = "ReceivablesNotSelectedLabel";
			this.ReceivablesNotSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 37, true);
			this.ReceivablesNotSelectedLabel.TabIndex = 2;
			this.ReceivablesNotSelectedLabel.Text = "You must select an Organization type of Receivables from the Details page to use " +
				"this page.";
			this.ReceivablesNotSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ReceivablesTabPage.ResumeLayout(true);
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
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 569, true);
			this.DetailsTabPage.ResumeLayout(true);
		}
		#endregion

	}
}
