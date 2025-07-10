namespace Enterprise.MasterFiles.GUI
{
	public partial class ZCompetitorIntelligenceForm
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CompetitorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrganisationsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrganisationsTabControl
			// 
			this.OrganisationsTabControl.Controls.Add(this.CompetitorTabPage);
			this.OrganisationsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 612, true);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.AddressesTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.CompetitorTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.StmNoteTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.ContactsTabPage, 0);
			this.OrganisationsTabControl.Controls.SetChildIndex(this.DetailsTabPage, 0);
			// 
			// ContactsTabPage
			// 
			this.ContactsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585, true);
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585, true);
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// 
			// AddressesTabPage
			//
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 664, true);
			// 
			// CompetitorTabPage
			// 
			this.CompetitorTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZCompetitorIntelligenceForm|5fb64e06-c917-4af9-b54a-651553a3b3d1", "Competitor");
			this.CompetitorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompetitorTabPage.Name = "CompetitorTabPage";
			this.CompetitorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585, true);
			this.CompetitorTabPage.TabIndex = 16;
			this.CompetitorTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CompetitorTabPage_InitializeTab));
			// 
			// ZCompetitorIntelligenceForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZCompetitorIntelligenceForm|38329812-06fa-400c-8129-1678d43fb898", "Competitor Intelligence");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 688, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 724, true);
			this.Name = "ZCompetitorIntelligenceForm";
			this.OrganisationsTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		#region Auto-generated code

		void CompetitorTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CompetitorControl = new Enterprise.MasterFiles.GUI.CompetitorUserControl();
			this.CompetitorTabPage.SuspendLayout();
			this.CompetitorTabPage.Controls.Add(this.CompetitorControl);
			// 
			// CompetitorControl
			// 
			this.BindingSource.SetBindingMember(this.CompetitorControl, ".");
			this.CompetitorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompetitorControl.IsModifyAddress = false;
			this.CompetitorControl.IsModifyCarrier = false;
			this.CompetitorControl.IsModifyCompetitor = true;
			this.CompetitorControl.IsModifyConfig = false;
			this.CompetitorControl.IsModifyConfigBrandsAndCompanyNames = false;
			this.CompetitorControl.IsModifyConfigEDICodeMapping = false;
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
			this.CompetitorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.CompetitorControl.Name = "CompetitorControl";
			this.CompetitorControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5);
			this.CompetitorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585);
			this.CompetitorControl.TabIndex = 0;
			this.CompetitorTabPage.ResumeLayout(true);
		}

		void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsTabPage.SuspendLayout();
			// 
			// DetailsControl
			// 
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 585);
			this.DetailsTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
