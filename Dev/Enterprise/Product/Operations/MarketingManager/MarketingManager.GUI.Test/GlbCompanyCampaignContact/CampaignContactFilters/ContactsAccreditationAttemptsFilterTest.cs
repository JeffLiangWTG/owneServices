using CargoWise.Application;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(PersonAccreditationAttemptsFilter))]
	class ContactsAccreditationAttemptsFilterTest : ModuleFilterTestCase<PersonAccreditationAttemptsFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override PersonAccreditationAttemptsFilter GetNewModuleFilter()
		{
			return new PersonAccreditationAttemptsFilter("moo", GlbPersonSchema.PK, GlbAccreditationAttemptSchema.HAA_PER, ObjectFactory.Get<IGlbAccreditationAttemptCollection>("IGlbAccreditationAttemptCollection", Factory), typeof(GlbPerson));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
