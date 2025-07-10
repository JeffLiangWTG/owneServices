using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityForm : ZTemplateForm
	{
		public OpportunityForm(OrgOpportunity opportunity)
			: base(opportunity)
		{
			InitializeComponent();
			SetupOpportunityManagementDetailsControl();
			WorkflowTabPage.Initialize(opportunity);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			if (!DesignModeFinder.IsDesigning)
			{
				ObjectFactory.Get<IOpportunityViewControllerProvider>().CreateController(this);
			}
		}

		public new ZTabControl TopLevelTabControl
		{
			get { return base.TopLevelTabControl; }
		}

		#region Navigate

		public bool NavigateToCommissionAgreement(OrgCommissionAgreement commissionAgreement)
		{
			MainTabControl.SelectedTab = MainTabPage;
			DetailsControl.OpportunityTabControl.SelectedTab = DetailsControl.CommissionAgreementTabPage;
			DetailsControl.CommissionAgreementsControl.AgreementsGrid.UnSelectAll();

			var list = DetailsControl.CommissionAgreementsControl.AgreementsGrid.List;
			for (int i = 0; i < list.Count; i++)
			{
				if (((OrgCommissionAgreement)list[i]).MainVersion.PK == commissionAgreement.PK)
				{
					DetailsControl.CommissionAgreementsControl.AgreementsGrid.ListManager.Position = i;
					DetailsControl.CommissionAgreementsControl.AgreementsGrid.Select(i);
					return true;
				}
			}

			return false;
		}

		public void NavigateToCommissionAgreementRecipientRate(OrgCommissionAgreementRecipientRate commissionAgreementRecipientRate)
		{
			var commissionAgreement = commissionAgreementRecipientRate.CommissionAgreement;
			if (commissionAgreement != null && NavigateToCommissionAgreement(commissionAgreement))
			{
				DetailsControl.CommissionAgreementsControl.CommissionAgreementControl.NavigateToCommissionAgreementRecipientRate(commissionAgreementRecipientRate);
			}
		}

		#endregion

		#region ZForm Overrides

		public override string FormCaption
		{
			get
			{
				string caption = base.FormCaption;
				if (BusinessEntity != null)
				{
					caption += " " + BusinessEntity.P8_OpportunityID;

					if (BusinessEntity.Address != null)
					{
						caption += " - " + BusinessEntity.Address.Header.OH_FullName;
					}
				}

				return caption;
			}
		}

		#endregion

		public new OrgOpportunity BusinessEntity
		{
			get { return (OrgOpportunity)base.BusinessEntity; }
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			base.Save(factories);
			DetailsControl.Refresh();
		}

		void SetupOpportunityManagementDetailsControl()
		{
			this.DetailsControl = CreateOpportunityManagementDetailsControl();
			this.MainTabPage.Controls.Add(this.DetailsControl);
			this.DetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsControl, ".");
			this.DetailsControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|a69cb205-ab2d-4e6f-8de8-cd677c1535af", "Opportunity");
			this.DetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1173, 500, true);
			this.DetailsControl.Name = "DetailsControl";
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 621, true);
			this.DetailsControl.TabIndex = 0;
		}

		protected virtual OpportunityManagementDetailsControl CreateOpportunityManagementDetailsControl()
		{
			return new OpportunityManagementDetailsControl();
		}
	}
}
