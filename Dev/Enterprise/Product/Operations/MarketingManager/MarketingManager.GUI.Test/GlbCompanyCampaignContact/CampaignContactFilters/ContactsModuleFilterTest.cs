using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ContactsModuleFilter))]
	class ContactsModuleFilterTest : ModuleFilterTestCase<ContactsModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ContactsModuleFilter GetNewModuleFilter()
		{
			return new ContactsModuleFilter("moo", ViewCampaignContactSchema.PK, OrgContactSchema.PK, new OrgContactCollection(Factory), typeof(CampaignContact));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
