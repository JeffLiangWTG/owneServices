using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailSelectionItem))]
	public class TradeDetailSelectionItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			return new TradeDetailSelectionItem(tradeDetail);
		}
	}
}
