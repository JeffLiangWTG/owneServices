using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Integration
{
	public interface ITradeDetailCommitmentUpdaterGUIManager
	{
		ZDialogResult ShowForm(IOrgOpportunity opportunity, EventArgs e);
	}
}
