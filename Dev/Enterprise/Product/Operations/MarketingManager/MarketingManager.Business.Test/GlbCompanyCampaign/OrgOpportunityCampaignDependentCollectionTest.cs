using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgOpportunityCampaignDependentCollection))]
	sealed class OrgOpportunityCampaignDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgOpportunityCampaignDependentCollection>
	{
		public void TestFilter()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1 = master.AllTouches.AddNew();
			touch1.G0_HorizontalId = 1;
			touch1.G0_CampaignName = "Touch 1";

			var touch2 = master.AllTouches.AddNew();
			touch2.G0_HorizontalId = 2;
			touch2.G0_CampaignName = "Touch 2";

			var opp1A = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1A.P8_Status = "WON";
			opp1A.P8_G0 = touch1.PK;
			opp1A.P8_OpportunityDescription = "opp1A";

			var opp1B = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1B.P8_Status = "WON";
			opp1B.P8_G0 = touch1.PK;
			opp1B.P8_OpportunityDescription = "opp1B";

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_Status = "WON";
			opp2.P8_G0 = touch2.PK;
			opp2.P8_OpportunityDescription = "opp2";

			var pivot1A = Factory.New<ViewRelatedActivityPivot>();
			pivot1A.ParentActivity = touch1;
			pivot1A.ChildActivity = opp1A;

			var pivot1B = Factory.New<ViewRelatedActivityPivot>();
			pivot1B.ParentActivity = touch1;
			pivot1B.ChildActivity = opp1B;

			var pivot2 = Factory.New<ViewRelatedActivityPivot>();
			pivot2.ParentActivity = touch2;
			pivot2.ChildActivity = opp2;
			Factory.Save();

			var collection = new OrgOpportunityCampaignDependentCollection(touch1);
			AssertContainsExactElementsInAnyOrder(new[] { opp1A, opp1B }, collection);

			var opp1C = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1C.P8_Status = "WON";
			opp1C.P8_G0 = touch1.PK;
			opp1C.P8_OpportunityDescription = "opp1C";

			var pivot1C = Factory.New<ViewRelatedActivityPivot>();
			pivot1C.ParentActivity = touch1;
			pivot1C.ChildActivity = opp1C;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(x => x.P8_OpportunityDescription, new[] { opp1A, opp1B, opp1C }, collection);
		}

		protected override OrgOpportunityCampaignDependentCollection GetCollectionToTest()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return new OrgOpportunityCampaignDependentCollection(campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgOpportunity>();
		}
	}
}
