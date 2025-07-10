using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesMatchingDataCollection))]
	sealed class SalesMatchingDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesMatchingDataCollection>
	{
		protected override SalesMatchingDataCollection GetCollectionToTest()
		{
			return new SalesMatchingDataCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			return new SalesMatchingData(sales, tradeDetail);
		}
	}
}
