using System;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignSelectedEventArgs : EventArgs
	{
		public GlbCompanyCampaign GlbCompanyCampaign { get; }

		public CampaignSelectedEventArgs(GlbCompanyCampaign campaign)
		{
			GlbCompanyCampaign = campaign;
		}
	}
}
