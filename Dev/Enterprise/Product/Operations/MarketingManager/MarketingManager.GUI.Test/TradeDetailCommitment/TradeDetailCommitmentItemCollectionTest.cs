using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCommitmentItemCollection))]
	public class TradeDetailCommitmentItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradeDetailCommitmentItemCollection>
	{
		protected override TradeDetailCommitmentItemCollection GetCollectionToTest()
		{
			return new TradeDetailCommitmentItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			return new TradeDetailCommitmentItem(tradeDetail, "AAA");
		}
	}
}
