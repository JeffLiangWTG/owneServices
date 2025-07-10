using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignContactDestinationURLLinkActivityModuleFilter))]
	public class CampaignContactDestinationURLLinkActivityModuleFilterTest : ModuleFilterTestCase<CampaignContactDestinationURLLinkActivityModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override CampaignContactDestinationURLLinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new CampaignContactDestinationURLLinkActivityModuleFilter("moo", new GlbCompanyCampaignContactFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
