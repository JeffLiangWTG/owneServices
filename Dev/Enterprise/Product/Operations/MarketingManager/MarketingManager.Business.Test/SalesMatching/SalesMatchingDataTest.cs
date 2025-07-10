using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesMatchingData))]
	sealed class SalesMatchingDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			return new SalesMatchingData(sales, tradeDetail);
		}
	}
}
