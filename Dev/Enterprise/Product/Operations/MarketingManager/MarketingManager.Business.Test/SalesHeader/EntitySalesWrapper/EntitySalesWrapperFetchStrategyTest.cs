using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EntitySalesWrapperFetchStrategyTest : TestCaseWithFactory
	{
		#region FetchForView

		#region Estimate Values

		public void TestFetchForView_EstimatedValues()
		{
			AddSalesForEstimatedValueFetchStrategyTest();

			var properties = new[]
				{
					EntitySalesWrapper.Schema.TotalEstimatedAnnualValue
				};

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgSalesProductSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ OrgTradeProspectSchema.Constants.TableName, 6 },
				{ OrgTradePeriodSchema.Constants.TableName, 18 },
			};
			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}

			properties = new[]
				{
					EntitySalesWrapper.Schema.PipelineValue,
					EntitySalesWrapper.Schema.UnsuccessfulValue
				};
			expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgSalesProductSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ OrgTradeProspectSchema.Constants.TableName, 6 },
				{ OrgTradePeriodSchema.Constants.TableName, 6 },
			};
			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}

			properties = new[]
				{
					EntitySalesWrapper.Schema.CommittedAnnualValue,
					EntitySalesWrapper.Schema.CommittedMonthlyValue
				};
			expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgSalesProductSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ OrgTradePeriodSchema.Constants.TableName, 12 },
			};
			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}
		}

		void AddSalesForEstimatedValueFetchStrategyTest()
		{
			var product = (BusinessObject)Factory.New<IOrgSalesProduct>();
			product.FillWithValidTestData();

			testSalesPks = new List<ZGuid>();

			for (var i = 0; i < 6; i++)
			{
				var prospective = Org.SalesCollection.AddNew();
				prospective.OW_MP_Product = product.PK;
				prospective.OW_IsTraded = false;

				var tradeDetail1 = prospective.TradeDetails.AddNew();
				tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Active;
				tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 30;
				tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

				var tradeDetail2 = prospective.TradeDetails.AddNew();
				tradeDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
				tradeDetail2.CurrentProspectPeriod.PAS_EstimatedProfit = 50;
				tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
				tradeDetail2.ProspectPeriodStart = ZDateTime.Today.Date;
				tradeDetail2.ProspectPeriodEnd = ZDateTime.Today.Date.AddMonths(2);

				var tradeDetail3 = prospective.TradeDetails.AddNew();
				tradeDetail3.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
				tradeDetail3.CurrentProspectPeriod.PAS_EstimatedProfit = 60;
				tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

				testSalesPks.Add(prospective.PK);
			}

			Factory.Save();
		}

		#endregion

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var viewFactory = new BusinessObjectFactory();
			var testSalesCollection = viewFactory.Load<OrgSales>(new ZQuery(OrgSalesSchema.PK, testSalesPks)).Select(x => EntitySalesWrapper.Get(x, opportunity));

			foreach (var entitySales in testSalesCollection)
			{
				entitySales.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var sales in testSalesCollection)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = sales[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		#endregion

		#region Implementation

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
				}

				return org;
			}
		}
		OrgHeader org;

		List<ZGuid> testSalesPks;

		#endregion
	}
}
