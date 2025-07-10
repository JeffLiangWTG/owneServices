using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesHeaderFetchStrategyTest : TestCaseWithFactory
	{
		[TestDate(2015, 5, 5)]
		public void TestFetchForView_TotalEstimatedAnnualValue()
		{
			var salesHeaders = CreateSalesHeaders();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgTradeDetailSchema.Constants.TableName, 0 },
				{ RefCurrencySchema.Constants.TableName, 0 },
				{ RefExchangeRateSchema.Constants.TableName, 0 },
				{ OrgTradeProspectSchema.Constants.TableName, 4 },
				{ OrgTradePeriodSchema.Constants.TableName, 4 }
			};
			// All prospect currencies are validated on load, so accessing the prospect annual total should not trigger any additional loads
			AssertFetchForViewDbHits(salesHeaders, new[] { SalesHeader.Schema.TotalEstimatedAnnualValue }, expectedDbHits);
		}

		[TestDate(2015, 5, 5)]
		public void TestFetchForView_TotalEstimatedMonthlyAverage()
		{
			var salesHeaders = CreateSalesHeaders();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgTradeDetailSchema.Constants.TableName, 0 },
				{ RefCurrencySchema.Constants.TableName, 0 },
				{ RefExchangeRateSchema.Constants.TableName, 0 },
				{ OrgTradeProspectSchema.Constants.TableName, 4 },
				{ OrgTradePeriodSchema.Constants.TableName, 4 }
			};
			// All prospect currencies are validated on load, so accessing the prospect monthly average should not trigger any additional loads
			AssertFetchForViewDbHits(salesHeaders, new[] { SalesHeader.Schema.TotalEstimatedMonthlyAverage }, expectedDbHits);
		}

		IEnumerable<SalesHeader> CreateSalesHeaders()
		{
			var salesHeaders = new List<SalesHeader>(5);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			for (var i = 0; i < 5; i++)
			{
				var product = Factory.NewWithValidTestData<OrgSalesProduct>();
				var salesHeader = salesHeaderCollection.AddNew(product);
				for (var j = 0; j < 10; j++)
				{
					var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
					sales.OW_MP_Product = product.PK;
					sales.OW_IsTraded = (j < 5);

					for (var k = 0; k < 10; k++)
					{
						var tradeDetail = sales.EntityTradeDetailsCollection.AddNew();

						if (!sales.OW_IsTraded)
						{
							tradeDetail.CurrentProspectPeriod.PAS_EstimatedProfit = 10;
						}
						else
						{
							var tradePeriod = tradeDetail.TradedPeriods.AddNew();
							tradePeriod.PAS_Period = new ZDate(2015, 1 + j * 2, 1);
							tradePeriod.PAS_OH_Client = org.PK;

							var tradeValue = tradePeriod.TradeValues.AddNew();
							tradeValue.PAV_GC = Env.CurrentCompanyPK;
							tradeValue.PAV_Revenue = 10;
							tradeValue.PAV_RX_NKCurrency =
									sales.IsActual
									? (k < 3 ? "AUD" : k < 6 ? "USD" : "GBP")
									: (k < 3 ? "NZD" : k < 6 ? "EUR" : "JPY");
						}
					}
				}

				salesHeaders.Add(new SalesHeader(org, product));
			}

			Factory.Save();

			return salesHeaders;
		}

		void AssertFetchForViewDbHits(IEnumerable<SalesHeader> salesHeaders, string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var salesHeadersInViewFactory = GetInAnotherFactory(viewFactory, salesHeaders);
			foreach (var salesHeader in salesHeadersInViewFactory)
			{
				var populator = new OrgSalesProspectCompanyBulkPopulater(viewFactory, salesHeader.ViewingOrg.PK);
				populator.Execute(salesHeader.EntitySales);
			}

			viewFactory.ResetDatabaseLoadCount();

			foreach (var salesHeader in salesHeadersInViewFactory)
			{
				salesHeader.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var salesHeader in salesHeadersInViewFactory)
			{
				object hitProperty;
				salesHeader.CompanyFilter = ZGuid.Empty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = salesHeader[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		static IEnumerable<SalesHeader> GetInAnotherFactory(BusinessObjectFactory targetFactory, IEnumerable<SalesHeader> salesHeaders)
		{
			var salesHeadersInTargetFactory = new List<SalesHeader>(salesHeaders.Count());
			foreach (var salesHeader in salesHeaders)
			{
				var org = targetFactory.Load<OrgHeader>(salesHeader.ViewingOrg.PK);
				var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
				var salesHeaderInTargetFactory = salesHeaderCollection.Cast<SalesHeader>().Single(x => x.SalesProduct.PK == salesHeader.SalesProduct.PK);

				salesHeadersInTargetFactory.Add(salesHeaderInTargetFactory);
			}

			return salesHeadersInTargetFactory;
		}
	}
}
