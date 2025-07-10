using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateWizard : WizardForm
	{
		public override string FormHeading
		{
			get { return Res.GetString("61cca131-19c0-43a5-b027-e0517861299b", "Bulk Rate / Cost Update"); }
		}

		#region Ctor

		public BulkUpdateWizard(string defaultRateType)
			: this()
		{
			Updater.ShowActiveQuotes = (defaultRateType == RatingConstants.RatingHeaderTypes.Quote);
			Updater.ShowClientRates = (defaultRateType == RatingConstants.RatingHeaderTypes.ClientRate);
			Updater.ShowCostings = (defaultRateType == RatingConstants.RatingHeaderTypes.Costing);
			Updater.ShowCompanyTariffs = (defaultRateType == RatingConstants.RatingHeaderTypes.Tariff);
			Updater.ShowIntercompanyTariffs = (defaultRateType == RatingConstants.RatingHeaderTypes.IntercompanyTariff);
		}

		public BulkUpdateWizard()
			: base(new BulkRateUpdater())
		{
			InitializeComponent();

			Construct();
		}

		#endregion

		#region Properties

		public BulkRateUpdater Updater
		{
			get { return (BulkRateUpdater)base.BusinessEntity; }
		}

		#endregion

		#region Implementation

		void Construct()
		{
			Pages.Add(new WizardPageStart()
			{
				Image = BulkUpdateWizardImages.WizardStartImage,
				Title = Res.GetString("8e017eb7-a612-4183-8c6a-1c31398de09a", "Welcome to Bulk Rate / Cost Update Wizard"),
				Description = Res.GetString("91b7e5ea-1cdd-4525-9997-1f45b4172c87", "This wizard will guide you through a series of steps to update many rate entries / trade lane specifications for a charge code at once")
			});
			Pages.Add(new BulkUpdateFilterPage());
			Pages.Add(new BulkUpdateEntriesPage());
			Pages.Add(new BulkUpdateActionsPage());
			Pages.Add(new BulkUpdatePreviewPage());
			Pages.Add(new BulkUpdateFinishPage());
		}

		#endregion
	}
}

