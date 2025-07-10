using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestsSubclassesOf(typeof(BaseLoader<>))]
	public abstract class BaseLoaderAbstractTest<T> : TestCaseWithFactory
		where T : EnterpriseBusinessObject, ITariffEffectiveDatesRelatedBusinessObject
	{
		public void TestGetEmptyCriteriaTable()
		{
			var tvpDataTable = BaseLoaderForTest.GetEmptyCriteriaTable();

			CombineAssertions(() =>
			{
				AssertNotNull("Created TVP_Data Criteria Data Table", tvpDataTable);
				AssertEquals("TVP Row Count", 0, tvpDataTable.Rows.Count);
				AssertContainsExactElementsInAnyOrder(ExpectedCriteriaTableColumns, tvpDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());
			});
		}

		protected abstract string[] ExpectedCriteriaTableColumns { get; }

		protected abstract BaseLoader<T> BaseLoaderForTest { get; }

		protected void AssertCriteriaSetCached(string criteriaDesc, TariffCriteriaSet<T> criteriaSet, Dictionary<string, T[]> dictionary, bool expected)
		{
			AssertEquals($"{criteriaDesc}", expected, dictionary.ContainsKey(criteriaSet.CacheKey));
		}

		protected void AssertLoadedBizosCount(string bizoDesc, IEnumerable<T> loadedData, ZGuid pk, int expected)
		{
			AssertEquals($"{bizoDesc} count", expected, loadedData.Count(r => r.PK == pk));
		}
	}
}
