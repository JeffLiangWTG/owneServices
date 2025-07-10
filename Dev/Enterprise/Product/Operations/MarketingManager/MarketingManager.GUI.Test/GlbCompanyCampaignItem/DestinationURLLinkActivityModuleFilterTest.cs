using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(DestinationURLLinkActivityModuleFilter))]
	public class DestinationURLLinkActivityModuleFilterTest : ModuleFilterTestCase<DestinationURLLinkActivityModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override DestinationURLLinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new DestinationURLLinkActivityModuleFilter("moo", new GlbCompanyCampaignItemFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
