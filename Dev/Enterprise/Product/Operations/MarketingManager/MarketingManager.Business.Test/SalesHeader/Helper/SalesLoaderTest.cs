using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesLoaderTest : TestCaseWithFactory
	{
		public void TestLoadPeriods()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "TRN");

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var actualSales1 = Factory.New<OrgSales>();
			actualSales1.OW_MP_Product = forwardingProduct.PK;
			actualSales1.OW_IsTraded = true;
			actualSales1.OW_OriginID = ausyd.PK;
			actualSales1.OW_OriginTableCode = "RL";
			actualSales1.OW_DestinationID = uslax.PK;
			actualSales1.OW_DestinationTableCode = "RL";
			actualSales1.OW_OH_Buyer = org.PK;
			var detail1 = actualSales1.TradeDetails.AddNew();
			detail1.PA_TradeMode = "AIR";
			detail1.PA_TradeType = "LSE";

			var period1a = detail1.TradedPeriods.AddNew();
			period1a.PAS_Period = new ZDate(2017, 8, 1);
			period1a.PAS_OH_Client = org.PK;
			var period1aa = detail1.TradedPeriods.AddNew();
			period1aa.PAS_Period = new ZDate(2017, 8, 1);
			period1aa.PAS_OH_Client = org.PK;
			period1aa.PAS_IsJobValue = true;
			var value1aa = period1aa.TradeValues.AddNew();
			value1aa.PAV_GC = Env.CurrentCompanyPK;

			var period1b = detail1.TradedPeriods.AddNew();
			period1b.PAS_Period = new ZDate(2017, 9, 1);
			period1b.PAS_OH_Client = org.PK;
			var period1bb = detail1.TradedPeriods.AddNew();
			period1bb.PAS_Period = new ZDate(2017, 9, 1);
			period1bb.PAS_OH_Client = org.PK;
			period1bb.PAS_IsJobValue = true;
			var value1bb = period1bb.TradeValues.AddNew();
			value1bb.PAV_GC = Env.CurrentCompanyPK;

			var period1c = detail1.TradedPeriods.AddNew();
			period1c.PAS_Period = new ZDate(2017, 10, 1);
			period1c.PAS_OH_Client = org.PK;
			var period1cc = detail1.TradedPeriods.AddNew();
			period1cc.PAS_Period = new ZDate(2017, 10, 1);
			period1cc.PAS_OH_Client = org.PK;
			period1cc.PAS_IsJobValue = true;
			var value1cc = period1cc.TradeValues.AddNew();
			value1cc.PAV_GC = Env.CurrentCompanyPK;

			var actualSales2 = Factory.New<OrgSales>();
			actualSales2.OW_MP_Product = transportProduct.PK;
			actualSales2.OW_IsTraded = true;
			actualSales2.OW_OriginID = uslax.PK;
			actualSales2.OW_OriginTableCode = "RL";
			actualSales2.OW_DestinationID = nzakl.PK;
			actualSales2.OW_DestinationTableCode = "RL";
			actualSales2.OW_OH_Supplier = org.PK;
			var detail2a = actualSales2.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";

			var period2a = detail2a.TradedPeriods.AddNew();
			period2a.PAS_Period = new ZDate(2017, 8, 1);
			period2a.PAS_OH_Client = org.PK;
			period2a.PAS_IsJobValue = true;
			var value2a = period2a.TradeValues.AddNew();
			value2a.PAV_GC = Env.CurrentCompanyPK;

			var period2b = detail2a.TradedPeriods.AddNew();
			period2b.PAS_Period = new ZDate(2017, 9, 1);
			period2b.PAS_OH_Client = org.PK;
			var value2b = period2b.TradeValues.AddNew();
			value2b.PAV_GC = Env.CurrentCompanyPK;

			var period2c = detail2a.TradedPeriods.AddNew();
			period2c.PAS_Period = new ZDate(2017, 10, 1);
			period2c.PAS_OH_Client = org.PK;
			period2c.PAS_IsExpired = true;

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = forwardingProduct.PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OriginID = uslax.PK;
			prospectSales1.OW_OriginTableCode = "RL";
			prospectSales1.OW_DestinationID = nzakl.PK;
			prospectSales1.OW_DestinationTableCode = "RL";
			prospectSales1.OW_OH_Primary = org.PK;
			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";

			var prospectPeriod1a = prospectDetail1.ProspectPeriods.AddNew();
			prospectPeriod1a.PAS_Period = new ZDate(2017, 9, 1);
			prospectPeriod1a.PAS_OH_Client = org.PK;

			var prospectPeriod1b = prospectDetail1.ProspectPeriods.AddNew();
			prospectPeriod1b.PAS_Period = new ZDate(2017, 10, 1);
			prospectPeriod1b.PAS_OH_Client = org.PK;

			var prospectPeriod1c = prospectDetail1.ProspectPeriods.AddNew();
			prospectPeriod1c.PAS_Period = new ZDate(2017, 11, 1);
			prospectPeriod1c.PAS_OH_Client = org.PK;

			Factory.Save();

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = org.PK,
				PeriodFrom = new ZDate(2017, 8, 1),
				PeriodTo = new ZDate(2017, 10, 1),
				IsPeriodToInclusive = true,
				IsTraded = true,
				IsForecast = false,
				TradeStatus = string.Empty,
				SalesProduct = null,
				SelectedSalesPks = null
			};

			context.LoadByOption = SalesLoader.LoadingContext.LoadBy.Product;
			var tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(8, tradedPeriods.Count());

			context.IsPeriodToInclusive = false;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(6, tradedPeriods.Count());

			context.PeriodTo = ZDate.Empty;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(8, tradedPeriods.Count());

			context.IsPeriodToInclusive = true;
			context.PeriodTo = new ZDate(2017, 10, 1);
			context.SalesProduct = forwardingProduct;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(6, tradedPeriods.Count());

			context.SalesProduct = transportProduct;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(2, tradedPeriods.Count());

			context.PeriodTo = new ZDate(2017, 9, 1);
			context.SalesProduct = null;
			context.SelectedSalesPks = new ZGuid[] { actualSales1.PK };
			context.LoadByOption = SalesLoader.LoadingContext.LoadBy.SelectedSales;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(4, tradedPeriods.Count());

			context.SelectedSalesPks = System.Array.Empty<ZGuid>();
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(0, tradedPeriods.Count());

			context.PeriodFrom = new ZDate(2017, 10, 1);
			context.PeriodTo = new ZDate(2018, 3, 1);
			context.IsTraded = false;
			context.SelectedSalesPks = null;
			context.LoadByOption = SalesLoader.LoadingContext.LoadBy.Product;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(2, tradedPeriods.Count());

			context.PeriodFrom = new ZDate(2017, 9, 1);
			context.TradeStatus = OpportunityTradeStatus.Codes.Successful;
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(0, tradedPeriods.Count());

			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			Factory.Save();
			tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
			AssertEquals(3, tradedPeriods.Count());

			context.PeriodFrom = new ZDate(2017, 8, 1);
			context.PeriodTo = new ZDate(2017, 10, 1);
			context.IsTraded = true;
			context.IsJobValue = true;
			var tradedValues = SalesLoader.LoadTradedValues(Factory, context);
			AssertEquals(4, tradedValues.Count());

			context.IsJobValue = false;
			tradedValues = SalesLoader.LoadTradedValues(Factory, context);
			AssertEquals(1, tradedValues.Count());

			context.IsJobValue = true;
			context.SalesProduct = forwardingProduct;
			tradedValues = SalesLoader.LoadTradedValues(Factory, context);
			AssertEquals(3, tradedValues.Count());

			context.SalesProduct = null;
			context.SelectedSalesPks = new ZGuid[] { actualSales2.PK };
			tradedValues = SalesLoader.LoadTradedValues(Factory, context);
			AssertEquals(1, tradedValues.Count());
		}

		public void TestLoadPeriods_ProspectCompanyFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			Factory.Save();

			// should not do the hacky way by setting TH_GC directly
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var opp1 = org.SalesOpportunities.AddNew();
				opp1.P8_GC = company1.PK;
				var opp2 = org.SalesOpportunities.AddNew();
				opp2.P8_GC = company2.PK;

				var quote = Factory.NewWithValidTestData<Quote>();
				var rateEntry = quote.AddRateEntry("AIR");

				var prospectSales = Factory.New<OrgSales>();
				prospectSales.OW_MP_Product = forwardingProduct.PK;
				prospectSales.OW_IsTraded = false;
				prospectSales.OW_OriginID = uslax.PK;
				prospectSales.OW_OriginTableCode = "RL";
				prospectSales.OW_DestinationID = uslax.PK;
				prospectSales.OW_DestinationTableCode = "RL";
				prospectSales.OW_OH_Primary = org.PK;

				#region Opportunity 1

				var prospectDetail1 = prospectSales.TradeDetails.AddNew();
				prospectDetail1.PA_TradeMode = "SEA";
				prospectDetail1.PA_TradeType = "FCL";

				var prospectPeriod1a = prospectDetail1.ProspectPeriods.AddNew();
				prospectPeriod1a.PAS_Period = new ZDate(2017, 9, 1);
				prospectPeriod1a.PAS_OH_Client = org.PK;

				var prospectPeriod1b = prospectDetail1.ProspectPeriods.AddNew();
				prospectPeriod1b.PAS_Period = new ZDate(2017, 10, 1);
				prospectPeriod1b.PAS_OH_Client = org.PK;

				opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectSales);
				opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail1);

				#endregion

				#region Opportunity 2

				var prospectDetail2 = prospectSales.TradeDetails.AddNew();
				prospectDetail2.PA_TradeMode = "SEA";
				prospectDetail2.PA_TradeType = "LCL";

				var prospectPeriod2a = prospectDetail2.ProspectPeriods.AddNew();
				prospectPeriod2a.PAS_Period = new ZDate(2017, 9, 1);
				prospectPeriod2a.PAS_OH_Client = org.PK;

				var prospectPeriod2b = prospectDetail2.ProspectPeriods.AddNew();
				prospectPeriod2b.PAS_Period = new ZDate(2017, 10, 1);
				prospectPeriod2b.PAS_OH_Client = org.PK;

				opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectSales);
				opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail2);

				#endregion

				#region Org

				var prospectDetail3 = prospectSales.TradeDetails.AddNew();
				prospectDetail3.PA_TradeMode = "AIR";
				prospectDetail3.PA_TradeType = "LSE";

				var prospectPeriod3a = prospectDetail3.ProspectPeriods.AddNew();
				prospectPeriod3a.PAS_Period = new ZDate(2017, 9, 1);
				prospectPeriod3a.PAS_OH_Client = org.PK;

				prospectDetail3.SalesAssociationPivotCollectionGlobal.AddNew(org);

				#endregion

				#region Quote

				var prospectDetail4 = prospectSales.TradeDetails.AddNew();
				prospectDetail4.PA_TradeMode = "AIR";
				prospectDetail4.PA_TradeType = "LSE";

				var prospectPeriod4a = prospectDetail4.ProspectPeriods.AddNew();
				prospectPeriod4a.PAS_Period = new ZDate(2017, 9, 1);
				prospectPeriod4a.PAS_OH_Client = org.PK;

				prospectDetail4.SalesAssociationPivotCollectionGlobal.AddNew(rateEntry);

				#endregion

				Factory.Save();

				var context = new SalesLoader.LoadingContext()
				{
					OrgPk = org.PK,
					PeriodFrom = new ZDate(2017, 8, 1),
					PeriodTo = new ZDate(2017, 10, 1),
					IsPeriodToInclusive = true,
					IsTraded = false,
					IsForecast = false,
					TradeStatus = string.Empty,
					SalesProduct = null,
					SelectedSalesPks = null,
					LoadByOption = SalesLoader.LoadingContext.LoadBy.Product
				};

				context.CompanyPk = company1.PK;
				var tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
				AssertContainsExactElementsInAnyOrder(new OrgTradePeriod[] { prospectPeriod1a, prospectPeriod1b, prospectPeriod3a }, tradedPeriods);

				context.CompanyPk = company2.PK;
				tradedPeriods = SalesLoader.LoadPeriods(Factory, context);
				AssertContainsExactElementsInAnyOrder(new OrgTradePeriod[] { prospectPeriod2a, prospectPeriod2b, prospectPeriod3a, prospectPeriod4a }, tradedPeriods);
			}
		}
	}
}
