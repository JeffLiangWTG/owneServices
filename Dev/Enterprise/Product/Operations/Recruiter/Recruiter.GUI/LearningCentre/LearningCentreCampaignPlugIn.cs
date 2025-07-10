using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI
{
	public abstract class LearningCentreCampaignPlugIn : SurveyCampaignPlugIn
	{
		public LearningCentreCampaignPlugIn(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		protected override string SupportedCampaignType
		{
			get { return Core.Constants.Recruiter.LearningCentreCampaignType; }
		}

		protected override void HookValueChangedEvents()
		{
			base.HookValueChangedEvents();
			HookValueChangedEventToToggleEnabled(BusinessEntity.G0_TypeInfo);
		}

		protected override bool ShouldBeEnabled
		{
			get { return base.ShouldBeEnabled && BusinessEntity.G0_Type == LearningCentreTestType; }
		}

		protected abstract string LearningCentreTestType { get; }
	}
}

// Tested in concrete classes
