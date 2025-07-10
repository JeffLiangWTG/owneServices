using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Integration
{
	public interface ITradeDetailClonerGUIManager
	{
		void ShowForm(IOrgOpportunity source, IOrgOpportunity target);

#if DEBUG
		void CloneAllForTest(IOrgOpportunity source, IOrgOpportunity target);
#endif
	}
}
