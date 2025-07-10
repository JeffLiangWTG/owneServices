using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class TouchSummary : ZUserControl
	{
		public TouchSummaryViewModel ViewModel { get; }

		internal void RemoveCampaign_Executed(object sender, EventArgs e)
		{
		}

		internal void Launch_Executed(object sender, EventArgs e)
		{
		}

		public GlbCompanyCampaign DataContext { get; set; }

		public void SetDataContext(GlbCompanyCampaign dataContext)
		{
		}

		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> BeginLongRefresh;
		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> LongRefreshProgress;
		public event EventHandler EndLongRefresh;
		public event EventHandler<CampaignSelectedEventArgs> CampaignSelected;
		public event EventHandler<CampaignSelectedEventArgs> InitialCampaignSelected;

		public class CampaignSelectedEventArgs : EventArgs
		{
			public GlbCompanyCampaign GlbCompanyCampaign { get; }

			public CampaignSelectedEventArgs(GlbCompanyCampaign glbCompanyCampaign)
			{
				GlbCompanyCampaign = glbCompanyCampaign;
			}
		}
	}
}
