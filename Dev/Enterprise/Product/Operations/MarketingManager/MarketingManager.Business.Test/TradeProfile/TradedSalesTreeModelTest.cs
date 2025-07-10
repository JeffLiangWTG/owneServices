using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradedSalesTreeModel))]
	sealed class TradedSalesTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Build

		public void TestBuild()
		{
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var nswStateQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, auCountry.RN_Code);
			nswStateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, "NSW");
			var nswState = Factory.LoadTop1<RefCountryStates>(nswStateQuery);
			var qldStateQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, auCountry.RN_Code);
			qldStateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, "QLD");
			var qldState = Factory.LoadTop1<RefCountryStates>(qldStateQuery);
			var usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			product.MP_Code = "XXX";
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var salesNsw = org.SalesCollection.AddNew();
			salesNsw.OW_MP_Product = product.PK;
			salesNsw.OW_OriginID = nswState.PK;
			salesNsw.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;
			var tradeDetailsNswA = salesNsw.TradeDetails.AddNew();
			var periodNswA = tradeDetailsNswA.TradedPeriods.AddNew();
			periodNswA.PAS_OH_Client = org.PK;
			var tradeDetailsNswB = salesNsw.TradeDetails.AddNew();
			var periodNswB = tradeDetailsNswB.TradedPeriods.AddNew();
			periodNswB.PAS_OH_Client = org.PK;

			var salesQld = org.SalesCollection.AddNew();
			salesQld.OW_MP_Product = product.PK;
			salesQld.OW_OriginID = qldState.PK;
			salesQld.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;
			var tradeDetailsQldA = salesQld.TradeDetails.AddNew();
			var periodQldA = tradeDetailsQldA.TradedPeriods.AddNew();
			periodQldA.PAS_OH_Client = org.PK;
			var tradeDetailsQldB = salesQld.TradeDetails.AddNew();
			var periodQldB = tradeDetailsQldB.TradedPeriods.AddNew();
			periodQldB.PAS_OH_Client = org.PK;

			var salesUs = org.SalesCollection.AddNew();
			salesUs.OW_MP_Product = product.PK;
			salesUs.OW_OriginID = usCountry.PK;
			salesUs.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;
			var tradeDetailsUsA = salesUs.TradeDetails.AddNew();
			var periodUsA = tradeDetailsUsA.TradedPeriods.AddNew();
			periodUsA.PAS_OH_Client = org.PK;
			var tradeDetailsUsB = salesUs.TradeDetails.AddNew();
			var periodUsB = tradeDetailsUsB.TradedPeriods.AddNew();
			periodUsB.PAS_OH_Client = org.PK;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var productInOtherFactory = anotherFactory.Load<OrgSalesProduct>(product.PK);
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			var salesHeader = new SalesHeader(orgInOtherFactory, productInOtherFactory);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			var model = new TradedSalesTreeModel(salesAnalysis);
			var tradePeriods = orgInOtherFactory.SalesCollection.Cast<OrgSales>().SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>()).SelectMany(t => t.TradedPeriods).ToList();

			anotherFactory.ResetDatabaseLoadCount();
			using (model.GetRebuildSuspender())
			{
				model.TradePeriods = tradePeriods;
				model.Groupers = new TradePeriodGrouper[] { new TradePeriodCountryToCountryGrouper(), new TradePeriodStateToStateGrouper() };
			}

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AU (Australia) -> (Unknown)",
					"US (United States) -> (Unknown)"
				},
				model.RootNodes.Select(x => x.BizObj.Description));

			var auNode = model.RootNodes.Single(x => x.BizObj.Description == "AU (Australia) -> (Unknown)");
			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AU, New South Wales -> (Unknown)",
					"AU, Queensland -> (Unknown)"
				},
				auNode.ChildNodes.Select(x => x.BizObj.Description));

			var expectedDbHits = new Dictionary<string, int>
			{
				{ ViewLocationSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			return new TradedSalesTreeModel(salesAnalysis);
		}

		#endregion
	}
}
