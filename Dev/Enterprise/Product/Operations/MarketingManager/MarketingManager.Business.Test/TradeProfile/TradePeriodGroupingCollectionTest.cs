using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradePeriodGroupingCollection))]
	sealed class TradePeriodGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradePeriodGroupingCollection>
	{
		#region Allowed Actions

		public void TestAllowNew()
		{
			var collection = new TradePeriodGroupingCollection();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new TradePeriodGroupingCollection();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Overrides

		protected override TradePeriodGroupingCollection GetCollectionToTest()
		{
			return new TradePeriodGroupingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			return new TradePeriodGrouping(SalesAnalysis, new[] { tradePeriod }, null);
		}

		TradedSalesAnalysis SalesAnalysis
		{
			get
			{
				if (salesAnalysis == null)
				{
					var product = Factory.New<OrgSalesProduct>();
					var org = Factory.New<OrgHeader>();
					var salesHeader = new SalesHeader(org, product);
					salesAnalysis = new TradedSalesAnalysis(salesHeader);
				}

				return salesAnalysis;
			}
		}
		TradedSalesAnalysis salesAnalysis;

		#endregion
	}
}
