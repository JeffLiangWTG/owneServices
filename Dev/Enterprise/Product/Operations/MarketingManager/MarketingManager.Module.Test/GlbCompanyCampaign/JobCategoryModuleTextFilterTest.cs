using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(JobCategoryModuleTextFilter))]
	public class JobCategoryModuleTextFilterTest : ModuleFilterTestCase<JobCategoryModuleTextFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override JobCategoryModuleTextFilter GetNewModuleFilter()
		{
			return new JobCategoryModuleTextFilter("moo", QueryDelegateForTest(), ListDelegateTest());
		}

		GetTextQueryWithOperator QueryDelegateForTest()
		{
			return delegate
			{
				return new ZQuery();
			};
		}

		GetList ListDelegateTest()
		{
			return () => new CodeDescriptionPairList();
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;
	}
}
