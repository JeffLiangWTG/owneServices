using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCloneItemCollection))]
	public class TradeDetailCloneItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradeDetailCloneItemCollection>
	{
		protected override TradeDetailCloneItemCollection GetCollectionToTest()
		{
			return new TradeDetailCloneItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			return new TradeDetailCloneItem(tradeDetail);
		}
	}
}
