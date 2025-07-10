using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradeDetailTimeline))]
	sealed class TradeDetailTimelineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var opp = org.SalesOpportunities.AddNew();
			opp.P8_OpportunityID = "O000A001";

			var tradeLane = Factory.New<OrgSales>();
			tradeLane.OW_OH_Primary = org.PK;

			var detail = tradeLane.TradeDetails.AddNew();
			opp.AssociatedTradeLanesPivots.AddPivotFor(detail);
			detail.PA_TradeMode = "SEA";
			detail.PA_TradeType = "FCL";
			detail.PA_Status = OpportunityTradeStatus.Codes.Successful;

			detail.ProspectDetail.PAP_ExpiryReason = OrgTradeProspectExpiryReasonList.Codes.Lost;

			var period = detail.ProspectPeriods.AddNew();
			period.PAS_OH_Client = org.PK;
			period.PAS_Period = new ZDate(2018, 3, 1);
			period.PAS_IsExpired = true;

			var timeLine = new TradeDetailTimeline(Factory, period);

			CombineAssertions(() =>
			{
				AssertEquals("O000A001", timeLine.ActivityID);
				AssertEquals(new ZDate(2018, 3, 1), timeLine.Period);
				AssertEquals(false, timeLine.IsSuperceded);
				AssertEquals(true, timeLine.IsExpired);
				AssertEquals(OrgTradeProspectExpiryReasonList.Descriptions.Lost, timeLine.ExpiryReason);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TradeDetailTimeline(Factory, Factory.New<OrgTradePeriod>());
		}
	}
}
