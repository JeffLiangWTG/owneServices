using System.Linq;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignGroup))]
	sealed class GlbCompanyCampaignGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTouches()
		{
			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign3 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign1.G0_GCG_Group = group.PK;
			campaign3.G0_GCG_Group = group.PK;

			AssertContainsExactElementsInAnyOrder(new[] { campaign1.PK, campaign3.PK }, group.Touches.Select(t => t.PK));
		}

		public void TestDelete()
		{
			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();

			var dripMarketing = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			dripMarketing.GCD_GCG_Group = group.PK;

			var sendSetting = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			sendSetting.GSC_GCG_Group = group.PK;

			Factory.Save();

			group.Delete();
			Assert(dripMarketing.IsDeleted);
			Assert(sendSetting.IsDeleted);
		}
	}
}
