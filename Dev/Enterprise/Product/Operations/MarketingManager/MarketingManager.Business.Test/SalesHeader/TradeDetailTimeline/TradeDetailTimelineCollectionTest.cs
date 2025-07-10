using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradeDetailTimelineCollection))]
	sealed class TradeDetailTimelineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradeDetailTimelineCollection>
	{
		public void TestLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_OpportunityID = "O000A001";
			var opp2 = org.SalesOpportunities.AddNew();
			opp2.P8_OpportunityID = "O000B328";

			var tradeLane1 = CreateTradeLane("SHP", "AUSYD", "NZAKL");
			tradeLane1.OW_OH_Primary = org.PK;

			var detail1a = tradeLane1.TradeDetails.AddNew();
			opp1.AssociatedTradeLanesPivots.AddPivotFor(detail1a);
			detail1a.PA_TradeMode = "SEA";
			detail1a.PA_TradeType = "FCL";
			detail1a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail1a.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 3, 4);
			detail1a.ProspectPeriodStart = new ZDate(2018, 3, 1);
			detail1a.ProspectPeriodEnd = new ZDate(2018, 4, 1);

			var tradeLane2 = CreateTradeLane("SHP", "AUMEL", "JPTYO");
			tradeLane2.OW_OH_Primary = org.PK;

			var detail2 = tradeLane2.TradeDetails.AddNew();
			detail2.PA_TradeMode = "AIR";
			detail2.PA_TradeType = "LSE";
			detail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail2.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 6, 19);
			detail2.ProspectPeriodStart = new ZDate(2018, 4, 1);
			detail2.ProspectPeriodEnd = new ZDate(2018, 5, 1);

			Factory.Save();

			var detail1b = tradeLane1.TradeDetails.AddNew();
			opp2.AssociatedTradeLanesPivots.AddPivotFor(detail1b);

			detail1b.PA_TradeMode = "SEA";
			detail1b.PA_TradeType = "FCL";
			detail1b.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail1b.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 5, 21);
			detail1b.ProspectPeriodStart = new ZDate(2018, 4, 1);
			detail1b.ProspectPeriodEnd = new ZDate(2018, 6, 1);

			Factory.Save();

			var collection = new TradeDetailTimelineCollection(detail1a);
			collection.Load();

			AssertEquals(5, collection.Cast<TradeDetailTimeline>().Count(x => !x.IsForecast));

			var ordreredTimeline = collection.Cast<TradeDetailTimeline>().Where(x => !x.IsForecast).OrderBy(x => x.Period).ThenBy(x => x.ActivityID).ToList();

			AssertTimeline(ordreredTimeline[0], "O000A001", new ZDate(2018, 3, 1), false);
			AssertTimeline(ordreredTimeline[1], "O000A001", new ZDate(2018, 4, 1), true);
			AssertTimeline(ordreredTimeline[2], "O000B328", new ZDate(2018, 4, 1), false);
			AssertTimeline(ordreredTimeline[3], "O000B328", new ZDate(2018, 5, 1), false);
			AssertTimeline(ordreredTimeline[4], "O000B328", new ZDate(2018, 6, 1), false);
		}

		OrgSales CreateTradeLane(ZString productCode, ZString originCode, ZString destinationCode)
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, originCode);
			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, destinationCode);
			var tradeLane = Factory.New<OrgSales>();
			tradeLane.OW_MP_Product = product.PK;
			tradeLane.OW_OriginID = origin.PK;
			tradeLane.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane.OW_DestinationID = destination.PK;
			tradeLane.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;

			return tradeLane;
		}

		void AssertTimeline(TradeDetailTimeline timeline, ZString id, ZDate period, ZBool isSuperceded)
		{
			AssertEquals("Activity ID", id, timeline.ActivityID);
			AssertEquals("Period", period, timeline.Period);
			AssertEquals("Superceded", isSuperceded, timeline.IsSuperceded);
		}

		#region Overrides

		protected override TradeDetailTimelineCollection GetCollectionToTest()
		{
			var detail = Factory.New<OrgTradeDetail>();
			return new TradeDetailTimelineCollection(detail);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var period = Factory.New<OrgTradePeriod>();
			return new TradeDetailTimeline(Factory, period);
		}

		#endregion
	}
}
