using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobAdPlacementDependentCollection))]
	sealed class HRJobAdPlacementDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			HRRecruitmentJobCampaign parent = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			return new HRJobAdPlacementDependentCollection(parent);
		}

		public void TestSetDefaultsForNewChild()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			ZDateTime startDate = new ZDateTime(2004, 1, 1, 1, 1, 1);
			ZDateTime endDate = new ZDateTime(2004, 2, 2, 2, 2, 2);

			campaign.HV_CampaignStartDate = startDate;
			campaign.HV_CampaignEndDate = endDate;

			HRJobAdPlacement ad = campaign.AdPlacements.AddNew();

			AssertEquals("Effective start date of Ad should default to Campaign start date", startDate, ad.HQ_EffectiveStartDate);
			AssertEquals("Effective end date of Ad should default to Campaign end date", endDate, ad.HQ_EffectiveEndDate);

			campaign.HV_CampaignStartDate = ZDateTime.Invalid;
			campaign.HV_CampaignEndDate = ZDateTime.Invalid;

			HRJobAdPlacement ad2 = campaign.AdPlacements.AddNew();

			AssertEquals("If campaign start date is invalid, Ad2 start date should not default", ZDateTime.Empty, ad2.HQ_EffectiveStartDate);
			AssertEquals("If campaign end date is invalid, Ad2 end date should not default", ZDateTime.Empty, ad2.HQ_EffectiveEndDate);
		}
	}
}
