using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradePeriodCollection))]
	sealed class OrgTradePeriodCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgTradePeriodCollection>
	{
		protected override OrgTradePeriodCollection GetCollectionToTest()
		{
			var sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();
			return new OrgTradePeriodCollection(tradeDetail, true);
		}
	}
}
