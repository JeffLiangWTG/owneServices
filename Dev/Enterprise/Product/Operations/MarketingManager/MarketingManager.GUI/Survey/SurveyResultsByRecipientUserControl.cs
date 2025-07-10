using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SurveyResultsByRecipientUserControl : ResultsByRecipientUserControl
	{
		public SurveyResultsByRecipientUserControl(GlbCompanyCampaign campaign)
			: base(campaign)
		{
			InitializeComponent();
		}

		protected override LearningCentreCampaignItemFilterBusinessObject CreateResultsByRecipientFilter()
			=> new SurveyResultsByRecipientFilterBusinessObject();
	}
}
