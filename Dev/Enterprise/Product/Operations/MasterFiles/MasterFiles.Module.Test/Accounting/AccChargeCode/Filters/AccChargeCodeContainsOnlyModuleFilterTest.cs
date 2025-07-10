using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChargeCodeContainsOnlyModuleFilter))]
	sealed class AccChargeCodeContainsOnlyModuleFilterTest : ModuleTextFilterTest
	{
		#region TestComparisonOperator

		public void TestContainsOnlyComparisonOperator()
		{
			Filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertEquals("ComparisonOperator must be Contains", SQLComparisonOperator.Contains, Filter.SqlComparisonOperator);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			// without filter data
			Filter.Property = "";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);

			// with filter data
			Filter.Property = "cell";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestDefaultComparisonOperator

		public new void TestDefaultComparisonOperator()
		{
			AssertEquals("Default ComparisonOperator must be Contains for this customization", Filter.SqlComparisonOperator, SQLComparisonOperator.Contains);
		}

		public new void TestClearSetsCorrectComparisonOperator()
		{
			Filter.Clear();
			AssertEquals("ComparisonOperator must be Contains after Clear", Filter.SqlComparisonOperator, SQLComparisonOperator.Contains);
		}

		#region SqlComparisonOperator

		public new void TestSqlComparisonOperator()
		{
			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.Contains, Filter.ComparisonOperator);

			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.Contains, Filter.ComparisonOperator);
		}

		#endregion

		#endregion

		#region TestSettingInvalidComparisonOperatorRevertsToStartsWith

		public new void TestSettingInvalidComparisonOperatorRevertsToStartsWith()
		{
			Filter.ComparisonOperator = "crap";
			AssertEquals("Invalid ComparisonOperator must revert to Contains for this customization", Filter.SqlComparisonOperator, SQLComparisonOperator.Contains);
		}

		#endregion

		#region Implementation

		new AccChargeCodeContainsOnlyModuleFilter Filter
		{
			get { return (AccChargeCodeContainsOnlyModuleFilter)base.Filter; }
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new AccChargeCodeContainsOnlyModuleFilter("moo", AccChargeCodeSchema.AC_DepartmentFilterList);
		}

		protected override string DefaultComparisonOperator
		{
			get
			{
				return ModuleTextBaseFilter.ComparisonConstants.Contains;
			}
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleTextFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.ComparisonOperator) }).ToArray();
		}

		#endregion
	}
}
