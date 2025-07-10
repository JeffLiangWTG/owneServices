using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
    class ConfigurationProviderFixture
    {
		[Test]
		public void TablesNotUseBulkInsert()
		{
			var tablesNotUseBulkInsert = ConfigurationProvider.TablesNotUseBulkInsert;
			Assert.IsNotNull(tablesNotUseBulkInsert);
			var questionDataSet = DataSetStructureProvider.StructuredDataSets.First(x => x[0] == "RefCusProfileQuestion");
			CollectionAssert.AreEquivalent(questionDataSet, tablesNotUseBulkInsert);
		}
	}
}
