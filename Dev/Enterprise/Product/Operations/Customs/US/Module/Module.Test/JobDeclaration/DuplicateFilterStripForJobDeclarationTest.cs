using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class DuplicateFilterStripForJobDeclarationTest : TestCaseWithFactory
	{
		public void TestDuplicateFilterStrip()
		{
			var collection = new ModuleFilterCollection();
			collection.AddTextFilter(DeclarationFilterConstants.EntryType, DummyBizoSchema.GenericStringSchemaColumn);
			new DuplicateFilterStripForJobDeclaration(new FilterStripCollection(collection));
			var stripsList = collection.ToList();
			AssertEquals(2, stripsList.Count);
			var strip0 = stripsList[0];
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, strip0["Property"]);
			var moduleFilter = strip0 as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, moduleFilter.ComparisonOperator);
			var strip1 = stripsList[1];
			AssertEquals(EntryTypeList.Codes.InformalFreeDutiable, strip1["Property"]);
			moduleFilter = strip1 as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, moduleFilter.ComparisonOperator);
		}
	}
}
