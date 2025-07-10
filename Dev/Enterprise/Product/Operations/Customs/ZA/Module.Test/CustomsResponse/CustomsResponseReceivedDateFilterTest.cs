using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CustomsResponseReceivedDateFilter))]
	sealed class CustomsResponseReceivedDateFilterTest : ModuleFilterTestCase<CustomsResponseReceivedDateFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("CustomsResponseReceivedDateFilter.Query is never empty", false, Filter.IsEmpty);
		}

		[TestDate(2016, 12, 27)]
		public void TestSetDefaultValues()
		{
			var filter = new CustomsResponseReceivedDateFilter("Time Received");
			AssertEquals("PropertySearch", ModuleDateFilter.SpecifiedDateRange, filter.PropertySearch);
			AssertEquals("Property1", new ZDateTime(2016, 12, 23), filter.Property1);
			AssertEquals("Property2", new ZDateTime(2016, 12, 27), filter.Property2);
		}

		public void TestPropertySearch_List()
		{
			var filter = new CustomsResponseReceivedDateFilter("Time Received");
			AssertEquals(6, filter.PropertySearch_List.Count);
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.DateRangeSearchTexts.Today));
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.DateRangeSearchTexts.Yesterday));
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.DateRangeSearchTexts.ThisWeek));
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.DateRangeSearchTexts.LastWeek));
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.DateRangeSearchTexts.Last7Days));
			Assert(filter.PropertySearch_List.ContainsCode(ModuleDateFilter.SpecifiedDateRange));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Dates;

		protected override CustomsResponseReceivedDateFilter GetNewModuleFilter() => new CustomsResponseReceivedDateFilter("moo");
	}
}
