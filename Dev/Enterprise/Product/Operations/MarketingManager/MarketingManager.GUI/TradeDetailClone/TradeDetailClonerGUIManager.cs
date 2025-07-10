using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TradeDetailClonerGUIManager : ITradeDetailClonerGUIManager
	{
		public void ShowForm(IOrgOpportunity source, IOrgOpportunity target)
		{
			var sourceOpportunity = source as OrgOpportunity;
			var targetOpportunity = target as OrgOpportunity;

			if (sourceOpportunity != null && targetOpportunity != null)
			{
				var cloner = new TradeDetailCloner(sourceOpportunity, targetOpportunity);
				ZFormModaliser.ShowDialogAndDispose(new TradeDetailClonerForm(cloner));
			}
		}

#if DEBUG
		void ITradeDetailClonerGUIManager.CloneAllForTest(IOrgOpportunity source, IOrgOpportunity target)
		{
			var cloner = new TradeDetailCloner(source as OrgOpportunity, target as OrgOpportunity);
			cloner.SelectAll();
			cloner.CopySelectionToTarget();
		}
#endif
	}
}
