using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeDetailValueSupercederTest : TestCaseWithFactory
	{
		public void TestSupercedeOverlappedPeriods()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var salesProductPk = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProductPk;
			prospectSales1.OW_OriginID = ausyd.PK;
			prospectSales1.OW_DestinationID = nzakl.PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var prospectDetail1a = prospectSales1.TradeDetails.AddNew();
			prospectDetail1a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1a.PA_TradeMode = "SEA";
			prospectDetail1a.PA_TradeType = "FCL";
			prospectDetail1a.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1a.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail1a);

			Factory.Save();

			var prospectDetail1b = prospectSales1.TradeDetails.AddNew();
			prospectDetail1b.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1b.PA_TradeMode = "SEA";
			prospectDetail1b.PA_TradeType = "FCL";
			prospectDetail1b.ProspectPeriodStart = new ZDate(2018, 3, 1);
			prospectDetail1b.ProspectPeriodEnd = new ZDate(2018, 4, 1);

			var superceder1b = new OrgTradeDetailValueSuperceder(prospectDetail1b, new ZDate(2018, 3, 1));
			superceder1b.SupercedeOverlappedPeriods();

			Factory.Save();

			var periods = prospectDetail1a.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Should not be superseded as new prospect periods are linked to same opportunity", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Should not be superseded as new prospect periods are linked to same opportunity", false, periods[1].PAS_IsSuperseded);
			AssertEquals("Should not be superseded as new prospect periods are linked to same opportunity", false, periods[2].PAS_IsSuperseded);

			var prospectDetail1c = prospectSales1.TradeDetails.AddNew();
			prospectDetail1c.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1c.PA_TradeMode = "SEA";
			prospectDetail1c.PA_TradeType = "LCL";
			prospectDetail1c.ProspectPeriodStart = new ZDate(2018, 2, 1);
			prospectDetail1c.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			var superceder1c = new OrgTradeDetailValueSuperceder(prospectDetail1c, new ZDate(2018, 2, 1));
			superceder1c.SupercedeOverlappedPeriods();

			var prospectSales2 = Factory.New<OrgSales>();
			prospectSales2.OW_IsTraded = false;
			prospectSales2.OW_OH_Primary = client.PK;

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 2, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail2);

			var superceder2 = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 2, 1));
			superceder2.SupercedeOverlappedPeriods();

			Factory.Save();

			periods = prospectDetail1a.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Should not be superseded as it is not within new prospect periods range", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[1].PAS_IsSuperseded);
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[2].PAS_IsSuperseded);
			AssertEquals("Is forecast", true, periods[11].PAS_IsForecast);
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[11].PAS_IsSuperseded);
			AssertEquals(new ZDate(2018, 2, 1), prospectDetail1a.ProspectDetail.PAP_ExpiryDate);
			AssertEquals(OrgTradeProspectExpiryReasonList.Codes.Superseded, prospectDetail1a.ProspectDetail.PAP_ExpiryReason);

			periods = prospectDetail1b.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[0].PAS_IsSuperseded);
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[1].PAS_IsSuperseded);
			AssertEquals("Is forecast", true, periods[11].PAS_IsForecast);
			AssertEquals("Should be superseded as it is within new prospect periods range", true, periods[11].PAS_IsSuperseded);
			AssertEquals(new ZDate(2018, 2, 1), prospectDetail1b.ProspectDetail.PAP_ExpiryDate);
			AssertEquals(OrgTradeProspectExpiryReasonList.Codes.Superseded, prospectDetail1b.ProspectDetail.PAP_ExpiryReason);

			periods = prospectDetail1c.ProspectPeriods.OrderBy(x => x.PAS_Period).ToList();
			AssertEquals("Should not be superseded as it is not a matched trade lane", false, periods[0].PAS_IsSuperseded);
			AssertEquals("Should not be superseded as it is not a matched trade lane", false, periods[1].PAS_IsSuperseded);
			//AssertEquals(ZDate.Empty, prospectDetail1c.ProspectDetail.PAP_ForecastExpiryDate);
			AssertEquals(ZString.Empty, prospectDetail1c.ProspectDetail.PAP_ExpiryReason);
		}

		public void TestSupercedingWarningMessage()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var salesProductPk = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var prospectSales1 = Factory.New<OrgSales>();
			prospectSales1.OW_MP_Product = salesProductPk;
			prospectSales1.OW_OriginID = ausyd.PK;
			prospectSales1.OW_DestinationID = nzakl.PK;
			prospectSales1.OW_IsTraded = false;
			prospectSales1.OW_OH_Primary = client.PK;

			var prospectDetail1 = prospectSales1.TradeDetails.AddNew();
			prospectDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail1.PA_TradeMode = "SEA";
			prospectDetail1.PA_TradeType = "FCL";
			prospectDetail1.ProspectPeriodStart = new ZDate(2018, 1, 1);
			prospectDetail1.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityID = "O00005893";
			opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail1);

			Factory.Save();

			var prospectSales2 = Factory.New<OrgSales>();
			prospectSales2.OW_IsTraded = false;
			prospectSales2.OW_OH_Primary = client.PK;

			var prospectDetail2 = prospectSales1.TradeDetails.AddNew();
			prospectDetail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectDetail2.PA_TradeMode = "SEA";
			prospectDetail2.PA_TradeType = "FCL";
			prospectDetail2.ProspectPeriodStart = new ZDate(2018, 2, 1);
			prospectDetail2.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectDetail2);

			var superceder = new OrgTradeDetailValueSuperceder(prospectDetail2, new ZDate(2018, 2, 1));
			var expectedMessage =
@"    O00005893 - Forwarding AUSYD NZAKL SEA FCL (Superseded from 01-Feb-18)
";
			AssertEquals(expectedMessage, superceder.SupercedingWarningMessage);
		}
	}
}
