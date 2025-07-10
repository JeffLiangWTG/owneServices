using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class CountryStateModuleFilterTest<T> : ModuleFilterTestCase<CountryStateModuleFilter<T>> where T : IZType
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Locations;
	}
}
