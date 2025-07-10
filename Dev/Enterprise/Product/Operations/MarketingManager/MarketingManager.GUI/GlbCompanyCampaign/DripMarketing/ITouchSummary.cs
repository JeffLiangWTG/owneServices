using System;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public interface ITouchSummary
	{
		void SetDataContext(GlbCompanyCampaign campaign);
		TouchSummaryViewModel ViewModel { get; }

		event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> BeginLongRefresh;
		event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> LongRefreshProgress;
		event EventHandler EndLongRefresh;
		event EventHandler<CampaignSelectedEventArgs> CampaignSelected;
		event EventHandler<CampaignSelectedEventArgs> InitialCampaignSelected;
	}
}
