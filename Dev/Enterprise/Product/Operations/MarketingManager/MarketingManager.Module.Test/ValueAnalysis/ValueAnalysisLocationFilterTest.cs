using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisLocationFilter))]
	public class ValueAnalysisLocationFilterTest : ModuleLocationFilterTest
	{
		protected override ModuleCodeFilter GetNewModuleFilter()
		{
			return new ValueAnalysisLocationFilter("moo", DummyBizoSchema.Z0_Code, Locations, DummyBizoSchema.Z0_Description, Locations, true);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		LocationCollection Locations => locations ?? (locations = new LocationCollection(Factory));
		LocationCollection locations;
	}
}
