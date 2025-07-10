using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.GUI
{
	public interface IGlbCompanyCampaignContactModule : IZFilterGridModule, IDisposable
	{
		IGlbCompanyCampaign Campaign { get; set; }
	}
}
