using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ModuleDependentItemTextFilter))]
	class ModuleDependentItemTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestErrorOnCodeNotPresent()
		{
			var filter = GetNewModuleFilter();
			AssertEquals(true, filter.ErrorOnCodeNotPresent);
		}

		public void TestIsExpensiveQuery()
		{
			var filter = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertEquals(false, filter.IsExpensiveQuery);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			AssertEquals(false, filter.IsExpensiveQuery);
		}

		#endregion

		#region Comparison Operators

		public void TestComparisonOperator_List()
		{
			var filter = GetNewModuleFilter();
			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(ModuleTextFilter.ComparisonConstants.Contains, "Search for records that contain an item matching the supplied text"),
					Tuple.Create(ModuleTextFilter.ComparisonConstants.NotContain, "Search for records that do not contain an item matching the supplied text"),
				},
				filter.ComparisonOperator_List.Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray());
		}

		public void TestAllowedComparisonOperators()
		{
			var filter = GetNewModuleFilter();
			AssertContainsExactElementsInAnyOrder(new[] { ModuleTextFilter.ComparisonConstants.Contains, ModuleTextFilter.ComparisonConstants.NotContain }, filter.AllowedComparisonOperators);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewModuleFilter();
		}

		#endregion

		#region Implementation

		protected ModuleDependentItemTextFilter GetNewModuleFilter()
		{
			return new ModuleDependentItemTextFilter("Test Filter", QueryDelegateForTest, new CodeDescriptionPairList());
		}

		ZQuery QueryDelegateForTest(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		#endregion
	}
}
