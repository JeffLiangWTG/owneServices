using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignDripMarketingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEligibleParentHorizontalIds()
		{
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<CodeDescriptionPair>(), touch1a.TransitionRulesToThisCampaign[0].Lookups.EligibleParentHorizontalIds);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<CodeDescriptionPair>(), touch1b.TransitionRulesToThisCampaign[0].Lookups.EligibleParentHorizontalIds);

			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair("1", "Touch 1") }, touch2a.TransitionRulesToThisCampaign[0].Lookups.EligibleParentHorizontalIds);
			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair("1", "Touch 1") }, touch2b.TransitionRulesToThisCampaign[0].Lookups.EligibleParentHorizontalIds);

			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair("1", "Touch 1"), new CodeDescriptionPair("2", "Touch 2") }, touch3a.TransitionRulesToThisCampaign[0].Lookups.EligibleParentHorizontalIds);
		}

		public void TestEligibleTouchParents()
		{
			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 0;
			AssertContainsExactElementsInAnyOrder(new[] { master }, touch3a.TransitionRulesToThisCampaign[0].Lookups.EligibleTouchParents);

			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			AssertContainsExactElementsInAnyOrder(new[] { touch1a, touch1b }, touch3a.TransitionRulesToThisCampaign[0].Lookups.EligibleTouchParents);

			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;
			AssertContainsExactElementsInAnyOrder(new[] { touch2a, touch2b }, touch3a.TransitionRulesToThisCampaign[0].Lookups.EligibleTouchParents);
		}

		GlbCompanyCampaign master;
		GlbCompanyCampaign touch1a;
		GlbCompanyCampaign touch1b;
		GlbCompanyCampaign touch2a;
		GlbCompanyCampaign touch2b;
		GlbCompanyCampaign touch3a;

		protected override void SetUp()
		{
			master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);

			touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "B";
			master.AllTouches.Add(touch2b);

			touch3a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "A";
			master.AllTouches.Add(touch3a);
		}
	}
}
