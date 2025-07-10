using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationGroupFilterBusinessObject))]
	public class GlbAccreditationGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDescriptionFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group1.HAG_Description = "description 1";
			var group2 = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group2.HAG_Description = "description 2";
			var filter = GetNewFilterStripBusinessObject()["Description"] as ModuleTextFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "description 1";
			var collection = new GlbAccreditationGroupCollection(Factory, filter.Query);
			AssertCollectionContains(group1, collection);
			filter.Property = "description";
			collection = new GlbAccreditationGroupCollection(Factory, filter.Query);
			AssertCollectionContains(group1, collection);
			AssertCollectionContains(group2, collection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbAccreditationGroupFilterBusinessObject();
		}
	}
}
