using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CustomsRulesFilterStripBusinessObject))]
	public class CustomsRulesFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilters()
		{
			var filterBusinessObject = new CustomsRulesFilterStripBusinessObject();
			var filters = filterBusinessObject.ModuleFilters;
			AssertEquals(1, filters.Count());

			var permitHolderFilter = filters.First(x => x.Description == CustomsRulesFilterStripBusinessObject.FilterConstants.Organization);
			AssertNotNull(permitHolderFilter);
			AssertEquals(typeof(ModuleGuidFilter), permitHolderFilter.GetType());
			AssertEquals(FilterCategories.Organisations, permitHolderFilter.Category);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CustomsRulesFilterStripBusinessObject();
		}
	}
}
