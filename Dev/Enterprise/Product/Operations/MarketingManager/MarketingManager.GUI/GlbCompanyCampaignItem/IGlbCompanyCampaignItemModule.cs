using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.GUI
{
	interface IGlbCompanyCampaignItemModule : IZFilterGridModule, IDisposable
	{
		IGlbCompanyCampaign Campaign { get; set; }
	}
}
