using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class DuplicateFilterStripForReconTest : TestCaseWithFactory
	{
		public void TestDuplicateFilterStrip()
		{
			var collection = new ModuleFilterCollection();
			collection.AddTextFilter(DeclarationFilterConstants.EntrySummaryStatus, DummyBizoSchema.GenericStringSchemaColumn);
			new DuplicateFilterStripForRecon(new FilterStripCollection(collection));
			var stripsList = collection.ToList();
			AssertEquals(2, stripsList.Count);
			var strip0 = stripsList[0];
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, strip0["Property"]);
			var moduleFilter = strip0 as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, moduleFilter.ComparisonOperator);
			var strip1 = stripsList[1];
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryCanceled, strip1["Property"]);
			moduleFilter = strip1 as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, moduleFilter.ComparisonOperator);
		}
	}
}
