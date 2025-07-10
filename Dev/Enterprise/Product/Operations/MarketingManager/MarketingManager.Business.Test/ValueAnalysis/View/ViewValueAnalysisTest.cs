using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ViewValueAnalysis))]
	sealed class ViewValueAnalysisTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot insert or delete on a view", true);
		}

		public void TestViewValueAnalysisProperties()
		{
			SetupViewValueAnalysisProperties();

			var obj = Factory.Load<ViewValueAnalysis>(Period.PK) ?? Factory.Load<ViewValueAnalysis>(Period2.PK);

			AssertEquals("Buyer", Buyer.PK, obj.VVA_OH_Buyer);
			AssertEquals("Supplier", Supplier.PK, obj.VVA_OH_Supplier);
			AssertEquals("Primary", Primary.PK, obj.VVA_OH_Primary);
			AssertEquals("Origin", Origin.PK, obj.VVA_Origin);
			AssertEquals("Destination", Destination.PK, obj.VVA_Destination);
			AssertEquals("Location", "DEHAM", obj.Location);
		}

		public override void TestFetchForLoad()
		{
			SetupViewValueAnalysisProperties();

			var factoryForLoad = new BusinessObjectFactory();
			factoryForLoad.ResetDatabaseLoadCount();
			factoryForLoad.LoadTop1<ViewValueAnalysis>(new ZQuery(ViewValueAnalysisSchema.PK, Details.PK));

			AssertMaxDbHits(1, factoryForLoad);
		}

		#region Implementation

		OrgHeader Buyer;
		OrgHeader Supplier;
		OrgHeader Primary;
		ViewLocation Origin;
		ViewLocation Destination;
		OrgSales Sales;
		OrgTradeDetail Details;
		OrgTradePeriod Period;
		OrgTradeValue Values;
		OrgTradeDetail Details2;
		OrgTradePeriod Period2;
		OrgTradeValue Values2;
		OrgTradeValue Values3;

		void SetupViewValueAnalysisProperties()
		{
			Buyer = Factory.NewWithValidTestData<OrgHeader>();
			Supplier = Factory.NewWithValidTestData<OrgHeader>();
			Primary = Factory.NewWithValidTestData<OrgHeader>();
			Origin = ViewLocationHelper.GetLocationFromString(Factory, "DEHAM", "RL");
			Destination = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL");

			Sales = Factory.NewWithValidTestData<OrgSales>();
			Sales.OW_OH_Buyer = Buyer.PK;
			Sales.OW_OH_Supplier = Supplier.PK;
			Sales.OW_OriginID = Origin.PK;
			Sales.OW_DestinationID = Destination.PK;

			Details = Sales.TradeDetails.AddNew();

			Period = Details.TradedPeriods.AddNew();
			Period.PAS_Period = new ZDate(2017, 8, 1);
			Period.PAS_RepeatsMnth = 100;
			Period.PAS_Weight = 200;
			Period.PAS_Volume = 300;
			Period.PAS_TEUQuantity = 400;
			Period.PAS_OH_Client = Primary.PK;

			Values = Period.TradeValues.AddNew();
			Values.PAV_Cost = 1000;
			Values.PAV_Revenue = 2000;
			Values.PAV_GC = Env.CurrentCompanyPK;
			Values2 = Period.TradeValues.AddNew();
			Values2.PAV_Cost = 10000;
			Values2.PAV_Revenue = 20000;
			Values2.PAV_GC = Env.CurrentCompanyPK;

			Details2 = Sales.TradeDetails.AddNew();

			Period2 = Details2.TradedPeriods.AddNew();
			Period2.PAS_RepeatsMnth = 1;
			Period2.PAS_Weight = 2;
			Period2.PAS_Volume = 3;
			Period2.PAS_TEUQuantity = 4;
			Period2.PAS_OH_Client = Primary.PK;

			Values3 = Period2.TradeValues.AddNew();
			Values3.PAV_Cost = 1;
			Values3.PAV_Revenue = 2;
			Values3.PAV_GC = Env.CurrentCompanyPK;

			Sales.OW_IsTraded = true;

			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<ViewValueAnalysisNoDelete>();
		}

		#endregion

		class ViewValueAnalysisNoDelete : ViewValueAnalysis
		{
			public ViewValueAnalysisNoDelete(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => null;
		}
	}
}
