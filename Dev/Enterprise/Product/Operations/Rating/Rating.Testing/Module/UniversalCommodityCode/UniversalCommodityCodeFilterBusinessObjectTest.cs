using System.Linq;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(UniversalCommodityCodeFilterBusinessObject))]
	public class UniversalCommodityCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilters()
		{
			var filterBizo = new UniversalCommodityCodeFilterBusinessObject();
			filterBizo.QueryObjectType = typeof(UniversalCommodityCodeBizo);
			var moduleFilters = filterBizo.ModuleFilters;
			AssertEquals(4, moduleFilters.Count());

			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Universal Group"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Universal Group Description"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Commodity Code"));
			AssertNotNull("Expected filter strip is loaded", moduleFilters.Single(x => x.Description == "Commodity Description"));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UniversalCommodityCodeFilterBusinessObject();
		}
	}
}
