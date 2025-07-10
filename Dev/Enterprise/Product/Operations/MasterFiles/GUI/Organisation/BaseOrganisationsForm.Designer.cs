namespace Enterprise.MasterFiles.GUI
{
	public partial class BaseOrganisationsForm
	{

		#region Windows Form Designer generated code

		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl OrganisationsTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage ContactsTabPage;
		public ContactsUserControl ContactsControl;
		public Enterprise.ZArchitecture.GUI.ZStmNoteTabPage StmNoteTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		public MainDetailsUserControl DetailsControl;
		internal Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		public Enterprise.ZArchitecture.GUI.ZTabPage AddressesTabPage;
		internal AddressesUserControl AddressesPageControl2;
		private System.ComponentModel.IContainer components;
		public Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		protected CargoWise.Windows.UI.KSplitContainer AddedInfoPanelSplitContainer;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.OrganisationsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContactsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.AddedInfoPanelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddedInfoPanelSplitContainer)).BeginInit();
			this.AddedInfoPanelSplitContainer.Panel1.SuspendLayout();
			this.AddedInfoPanelSplitContainer.Panel2.SuspendLayout();
			this.AddedInfoPanelSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 663, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(895);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(932, 637, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
			this.ButtonsUserControl.TabIndex = 2;
			// 
			// OrganisationsTabControl
			// 
			this.OrganisationsTabControl.Controls.Add(this.DetailsTabPage);
			this.OrganisationsTabControl.Controls.Add(this.ContactsTabPage);
			this.OrganisationsTabControl.Controls.Add(this.AddressesTabPage);
			this.OrganisationsTabControl.Controls.Add(this.StmNoteTabPage);
			this.OrganisationsTabControl.Controls.Add(this.zLogsTabPage1);
			this.OrganisationsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationsTabControl.Name = "OrganisationsTabControl";
			this.OrganisationsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationsTabControl.SelectedIndex = 0;
			this.OrganisationsTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BaseOrganisationsForm|0040b29e-4a99-4fa1-8e10-bd5f1de27bc1", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 595, true);
			this.DetailsTabPage.TabIndex = 15;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// 
			// ContactsTabPage
			// 
			this.ContactsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BaseOrganisationsForm|ef3a15ad-007e-498f-8685-96cff9760f9b", "Contact");
			this.ContactsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContactsTabPage.Name = "ContactsTabPage";
			this.ContactsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContactsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 595, true);
			this.ContactsTabPage.TabIndex = 8;
			this.ContactsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ContactsTabPage_InitializeTab));
			// 
			// AddressesTabPage
			// 
			this.AddressesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BaseOrganisationsForm|6c6c35e3-8466-432c-9678-d637e410526b", "Address");
			this.AddressesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressesTabPage.Name = "AddressesTabPage";
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 595, true);
			this.AddressesTabPage.TabIndex = 17;
			this.AddressesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AddressesTabPage_InitializeTab));
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 595, true);
			this.StmNoteTabPage.TabIndex = 13;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 595, true);
			this.zLogsTabPage1.TabIndex = 16;
			// 
			// AddedInfoPanelSplitContainer
			// 
			this.AddedInfoPanelSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AddedInfoPanelSplitContainer.IsSplitterFixed = true;
			this.AddedInfoPanelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 637, true);
			this.AddedInfoPanelSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(14);
			this.AddedInfoPanelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddedInfoPanelSplitContainer.Name = "AddedInfoPanelSplitContainer";
			// 
			// AddedInfoPanelSplitContainer.Panel2
			// 
			this.AddedInfoPanelSplitContainer.Panel2.Controls.Add(this.OrganisationsTabControl);
			this.AddedInfoPanelSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.AddedInfoPanelSplitContainer.SplitterWidth = 1;
			// 
			// BaseOrganisationsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BaseOrganisationsForm|38329812-06fa-400c-8129-1678d43fb898", "Base Organizations Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 687, true);
			this.Controls.Add(this.AddedInfoPanelSplitContainer);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "BaseOrganisationsForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "BaseOrganisationsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.AddedInfoPanelSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationsTabControl.ResumeLayout(false);
			this.OrganisationsTabControl.PerformLayout();
			this.AddedInfoPanelSplitContainer.Panel1.ResumeLayout(false);
			this.AddedInfoPanelSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddedInfoPanelSplitContainer)).EndInit();
			this.AddedInfoPanelSplitContainer.ResumeLayout(false);
			this.AddedInfoPanelSplitContainer.PerformLayout();
			this.ResumeLayout(false);
		}

		private void AddressesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AddressesPageControl2 = new Enterprise.MasterFiles.GUI.AddressesUserControl();
			this.AddressesTabPage.SuspendLayout();
			this.AddressesTabPage.Controls.Add(this.AddressesPageControl2);
			// 
			// AddressesPageControl2
			// 
			this.BindingSource.SetBindingMember(this.AddressesPageControl2, ".");
			this.AddressesPageControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesPageControl2.IsModifyAddress = true;
			this.AddressesPageControl2.IsModifyCarrier = false;
			this.AddressesPageControl2.IsModifyCompetitor = false;
			this.AddressesPageControl2.IsModifyConfig = false;
			this.AddressesPageControl2.IsModifyConfigBrandsAndCompanyNames = false;
			this.AddressesPageControl2.IsModifyConfigEDICodeMapping = false;
			this.AddressesPageControl2.IsModifyConfigGeneral = false;
			this.AddressesPageControl2.IsModifyConfigRegistrationNumbers = false;
			this.AddressesPageControl2.IsModifyConsignee = false;
			this.AddressesPageControl2.IsModifyConsigneeDetails = false;
			this.AddressesPageControl2.IsModifyConsigneeLandedCosting = false;
			this.AddressesPageControl2.IsModifyConsigneeRelationships = false;
			this.AddressesPageControl2.IsModifyConsignor = false;
			this.AddressesPageControl2.IsModifyConsignorDetails = false;
			this.AddressesPageControl2.IsModifyConsignorExporterScheme = false;
			this.AddressesPageControl2.IsModifyConsignorRelationships = false;
			this.AddressesPageControl2.IsModifyContact = false;
			this.AddressesPageControl2.IsModifyContactContactDetails = false;
			this.AddressesPageControl2.IsModifyContactDocDeliveryDetails = false;
			this.AddressesPageControl2.IsModifyContactPersonalInformation = false;
			this.AddressesPageControl2.IsModifyCustom = false;
			this.AddressesPageControl2.IsModifyDetails = false;
			this.AddressesPageControl2.IsModifyDetailsNameAndAddress = false;
			this.AddressesPageControl2.IsModifyDetailsOrganisationType = false;
			this.AddressesPageControl2.IsModifyDetailsPhFaxWebDetails = false;
			this.AddressesPageControl2.IsModifyDetailsRatingAndTariffs = false;
			this.AddressesPageControl2.IsModifyDetailsStaffAssignments = false;
			this.AddressesPageControl2.IsModifyDetailsWebSecurity = false;
			this.AddressesPageControl2.IsNewDetailsWebSecurity = false;
			this.AddressesPageControl2.IsModifyForwarder = false;
			this.AddressesPageControl2.IsModifyForwarderDetails = false;
			this.AddressesPageControl2.IsModifyForwarderProfitShare = false;
			this.AddressesPageControl2.IsModifyPayables = false;
			this.AddressesPageControl2.IsModifyReceivables = false;
			this.AddressesPageControl2.IsModifyReceivablesConfig = false;
			this.AddressesPageControl2.IsModifyReceivablesInvoicing = false;
			this.AddressesPageControl2.IsModifySales = false;
			this.AddressesPageControl2.IsModifySalesClientRelationship = false;
			this.AddressesPageControl2.IsModifySalesClientSummary = false;
			this.AddressesPageControl2.IsModifySalesOpportunityManagement = false;
			this.AddressesPageControl2.IsModifySalesTradeProfile = false;
			this.AddressesPageControl2.IsModifyServices = false;
			this.AddressesPageControl2.IsModifyWarehouse = false;
			this.AddressesPageControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressesPageControl2.Name = "AddressesPageControl2";
			this.AddressesPageControl2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AddressesPageControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 579, true);
			this.AddressesPageControl2.TabIndex = 0;
			this.AddressesTabPage.ResumeLayout(true);
		}

		protected virtual ContactsUserControl GetContactsUserControlForm()
		{
			return new ContactsUserControl();
		}

		private void ContactsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ContactsControl = GetContactsUserControlForm();
			this.ContactsTabPage.SuspendLayout();
			this.ContactsTabPage.Controls.Add(this.ContactsControl);
			// 
			// ContactsControl
			// 
			this.BindingSource.SetBindingMember(this.ContactsControl, ".");
			this.ContactsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsControl.IsModifyAddress = false;
			this.ContactsControl.IsModifyCarrier = false;
			this.ContactsControl.IsModifyCompetitor = false;
			this.ContactsControl.IsModifyConfig = false;
			this.ContactsControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.ContactsControl.IsModifyConfigEDICodeMapping = false;
			this.ContactsControl.IsModifyConfigGeneral = false;
			this.ContactsControl.IsModifyConfigRegistrationNumbers = false;
			this.ContactsControl.IsModifyConsignee = false;
			this.ContactsControl.IsModifyConsigneeDetails = false;
			this.ContactsControl.IsModifyConsigneeLandedCosting = false;
			this.ContactsControl.IsModifyConsigneeRelationships = false;
			this.ContactsControl.IsModifyConsignor = false;
			this.ContactsControl.IsModifyConsignorDetails = false;
			this.ContactsControl.IsModifyConsignorExporterScheme = false;
			this.ContactsControl.IsModifyConsignorRelationships = false;
			this.ContactsControl.IsModifyContact = true;
			this.ContactsControl.IsModifyContactContactDetails = true;
			this.ContactsControl.IsModifyContactDocDeliveryDetails = true;
			this.ContactsControl.IsModifyContactPersonalInformation = true;
			this.ContactsControl.IsModifyCustom = false;
			this.ContactsControl.IsModifyDetails = false;
			this.ContactsControl.IsModifyDetailsNameAndAddress = false;
			this.ContactsControl.IsModifyDetailsOrganisationType = false;
			this.ContactsControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ContactsControl.IsModifyDetailsRatingAndTariffs = false;
			this.ContactsControl.IsModifyDetailsStaffAssignments = false;
			this.ContactsControl.IsModifyDetailsWebSecurity = false;
			this.ContactsControl.IsNewDetailsWebSecurity = false;
			this.ContactsControl.IsModifyForwarder = false;
			this.ContactsControl.IsModifyForwarderDetails = false;
			this.ContactsControl.IsModifyForwarderProfitShare = false;
			this.ContactsControl.IsModifyPayables = false;
			this.ContactsControl.IsModifyReceivables = false;
			this.ContactsControl.IsModifyReceivablesConfig = false;
			this.ContactsControl.IsModifyReceivablesInvoicing = false;
			this.ContactsControl.IsModifySales = false;
			this.ContactsControl.IsModifySalesClientRelationship = false;
			this.ContactsControl.IsModifySalesClientSummary = false;
			this.ContactsControl.IsModifySalesOpportunityManagement = false;
			this.ContactsControl.IsModifySalesTradeProfile = false;
			this.ContactsControl.IsModifyServices = false;
			this.ContactsControl.IsModifyWarehouse = false;
			this.ContactsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContactsControl.Name = "ContactsControl";
			this.ContactsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContactsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 573, true);
			this.ContactsControl.TabIndex = 0;
			this.ContactsControl.AfterFirstBinding += new System.EventHandler(ContactsControlOnAfterFirstBinding);
			this.ContactsTabPage.ResumeLayout(true);
		}

		private void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsControl = new Enterprise.MasterFiles.GUI.MainDetailsUserControl();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.DetailsControl);
			// 
			// DetailsControl
			// 
			this.BindingSource.SetBindingMember(this.DetailsControl, ".");
			this.DetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsControl.IsModifyAddress = false;
			this.DetailsControl.IsModifyCarrier = false;
			this.DetailsControl.IsModifyCompetitor = false;
			this.DetailsControl.IsModifyConfig = false;
			this.DetailsControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.DetailsControl.IsModifyConfigEDICodeMapping = false;
			this.DetailsControl.IsModifyConfigGeneral = false;
			this.DetailsControl.IsModifyConfigRegistrationNumbers = false;
			this.DetailsControl.IsModifyConsignee = false;
			this.DetailsControl.IsModifyConsigneeDetails = false;
			this.DetailsControl.IsModifyConsigneeLandedCosting = false;
			this.DetailsControl.IsModifyConsigneeRelationships = false;
			this.DetailsControl.IsModifyConsignor = false;
			this.DetailsControl.IsModifyConsignorDetails = false;
			this.DetailsControl.IsModifyConsignorExporterScheme = false;
			this.DetailsControl.IsModifyConsignorRelationships = false;
			this.DetailsControl.IsModifyContact = false;
			this.DetailsControl.IsModifyContactContactDetails = false;
			this.DetailsControl.IsModifyContactDocDeliveryDetails = false;
			this.DetailsControl.IsModifyContactPersonalInformation = false;
			this.DetailsControl.IsModifyCustom = false;
			this.DetailsControl.IsModifyDetails = true;
			this.DetailsControl.IsModifyDetailsNameAndAddress = true;
			this.DetailsControl.IsModifyDetailsOrganisationType = true;
			this.DetailsControl.IsModifyDetailsPhFaxWebDetails = true;
			this.DetailsControl.IsModifyDetailsRatingAndTariffs = true;
			this.DetailsControl.IsModifyDetailsStaffAssignments = true;
			this.DetailsControl.IsModifyDetailsWebSecurity = true;
			this.DetailsControl.IsNewDetailsWebSecurity = true;
			this.DetailsControl.IsModifyConfigFinancialRegistrationNumbersSecurity = true;
			this.DetailsControl.IsNewConfigModifyFinancialRegistrationNosSecurity = true;
			this.DetailsControl.IsModifyForwarder = false;
			this.DetailsControl.IsModifyForwarderDetails = false;
			this.DetailsControl.IsModifyForwarderProfitShare = false;
			this.DetailsControl.IsModifyPayables = false;
			this.DetailsControl.IsModifyReceivables = false;
			this.DetailsControl.IsModifyReceivablesConfig = false;
			this.DetailsControl.IsModifyReceivablesInvoicing = false;
			this.DetailsControl.IsModifySales = false;
			this.DetailsControl.IsModifySalesClientRelationship = false;
			this.DetailsControl.IsModifySalesClientSummary = false;
			this.DetailsControl.IsModifySalesOpportunityManagement = false;
			this.DetailsControl.IsModifySalesTradeProfile = false;
			this.DetailsControl.IsModifyServices = false;
			this.DetailsControl.IsModifyWarehouse = false;
			this.DetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsControl.Name = "DetailsControl";
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 579, true);
			this.DetailsControl.TabIndex = 0;
			this.DetailsTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
