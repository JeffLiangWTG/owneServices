using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.GUI
{
	interface IGlbCompanyCampaignClickModule : IZFilterGridModule, IDisposable
	{
		IGlbCompanyCampaignItem CampaignItem { get; set; }
	}
}
