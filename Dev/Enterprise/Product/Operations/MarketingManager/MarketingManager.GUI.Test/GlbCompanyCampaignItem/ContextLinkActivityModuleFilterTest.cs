using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ContextLinkActivityModuleFilter))]
	public class ContextLinkActivityModuleFilterTest : ModuleFilterTestCase<ContextLinkActivityModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override ContextLinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new ContextLinkActivityModuleFilter("moo", new GlbCompanyCampaignItemFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
