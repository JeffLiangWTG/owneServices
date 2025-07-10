using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(ExamsModuleFilter))]
	class ExamsModuleFilterTest : ModuleFilterTestCase<ExamsModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ExamsModuleFilter GetNewModuleFilter()
		{
			return new ExamsModuleFilter((ZString)"moo", new LearningCentreCampaignCollection(Factory));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
