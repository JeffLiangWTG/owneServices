using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignContactContextLinkActivityModuleFilter))]
	public class CampaignContactContextLinkActivityModuleFilterTest : ModuleFilterTestCase<CampaignContactContextLinkActivityModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override CampaignContactContextLinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new CampaignContactContextLinkActivityModuleFilter("moo", new GlbCompanyCampaignContactFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
