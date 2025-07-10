using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public abstract class BusinessObjectMergerTest<T>
			: TestCaseWithFactory where T : BusinessObject
	{
		public abstract void TestMerge();
		public abstract void TestCheckMerge();

		public void TestShouldSpecifyAllColumnsForCompareOrIgnored()
		{
			var merger = GetNewMerger();
			var specifiedColumns = merger.IgnoredColumns.Union(merger.ComparableColumns);

			AssertContainsExactElementsInAnyOrder(
				"Please Specify all columns as compared or ignored!",
				GetAllSchemaColumns().Select(c => c.Name),
				specifiedColumns.Select(c => c.Name));
		}

		protected abstract IEnumerable<SchemaColumn> GetAllSchemaColumns();

		protected abstract BusinessObjectMerger<T> GetNewMerger();
	}
}
