using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(LeaveDateRangeWithTypeFilter))]
	internal class LeaveDateRangeWithTypeFilterTest : ModuleFilterTestCase<LeaveDateRangeWithTypeFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		public void TestClearResetsToDefaults()
		{
			var searchValue = new ZDateTime(2000, 1, 1);
			ZString workHolidayType = "ANN";

			Filter.Property1 = searchValue;
			Filter.WorkHolidayType = workHolidayType;

			AssertEquals("Precondition", searchValue, Filter.Property1);
			AssertEquals("Precondition", workHolidayType, Filter.WorkHolidayType);

			Filter.Clear();
			AssertEquals(ZDateTime.Empty, Filter.Property1);
			AssertEquals(ZString.Empty, Filter.WorkHolidayType);
		}

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Filter.WorkHolidayType = "";
			AssertEquals(true, Filter.IsEmpty);

			Filter.Property1 = new ZDateTime(2000, 1, 1);
			AssertEquals(false, Filter.IsEmpty);

			Filter.WorkHolidayType = "ANN";
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			AssertEquals(false, Filter.IsEmpty);

			Filter.WorkHolidayType = "";
			AssertEquals(true, Filter.IsEmpty);
		}

		public void TestDateAndTypeQuery()
		{
			TestCaseHelper.ClearTable(AutoGlbStaff.Schema.TableName);

			var holiday1 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday1.GA_StartTime = new ZDateTime(2018, 8, 8);
			holiday1.GA_EndTime = new ZDateTime(2018, 8, 15);
			holiday1.GA_WorkHolidayType = "ANN";
			var staff1 = holiday1.Staff;
			var holiday2 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday2.GA_StartTime = new ZDateTime(2018, 8, 10);
			holiday2.GA_EndTime = new ZDateTime(2018, 8, 20);
			holiday2.GA_WorkHolidayType = "ANN";
			var staff2 = holiday2.Staff;
			var holiday3 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday3.GA_StartTime = new ZDateTime(2018, 9, 8);
			holiday3.GA_EndTime = new ZDateTime(2018, 9, 18);
			holiday3.GA_WorkHolidayType = "SIC";
			var staff3 = holiday3.Staff;
			var holiday4 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday4.GA_StartTime = new ZDateTime(2018, 8, 14);
			holiday4.GA_EndTime = new ZDateTime(2018, 8, 15);
			holiday4.GA_WorkHolidayType = "ANN";
			var staff4 = holiday4.Staff;
			var holiday5 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday5.GA_StartTime = new ZDateTime(2018, 7, 8);
			holiday5.GA_EndTime = new ZDateTime(2018, 10, 8);
			holiday5.GA_WorkHolidayType = "ANN";
			var staff5 = holiday5.Staff;
			var holiday6 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday6.GA_StartTime = new ZDateTime(2008, 9, 16, 6, 14, 0);
			holiday6.GA_EndTime = new ZDateTime(2008, 9, 17, 6, 14, 0);
			holiday6.GA_WorkHolidayType = "ANN";
			var staff6 = holiday6.Staff;

			var staffNoHoliday = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var filterBizO = new DummyFilterBizOForLeaveDataRangeWithTypeFilter();
			var leaveRangeFilter = filterBizO["moo"] as LeaveDateRangeWithTypeFilter;
			leaveRangeFilter.Property1 = ZDateTime.Empty;
			leaveRangeFilter.Property2 = ZDateTime.Empty;
			leaveRangeFilter.WorkHolidayType = ZString.Empty;
			leaveRangeFilter.PropertySearch = ZString.Empty;
			leaveRangeFilter.IsActive = true;

			var staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(7, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "ANN";
			leaveRangeFilter.PropertySearch = ZString.Empty;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(5, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "SIC";
			leaveRangeFilter.Property1 = new ZDateTime(2014, 1, 1);
			leaveRangeFilter.Property2 = new ZDateTime(2019, 1, 1);
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(1, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "";
			leaveRangeFilter.Property1 = new ZDateTime(2018, 8, 9);
			leaveRangeFilter.Property2 = new ZDateTime(2018, 8, 15);
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(4, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "";
			leaveRangeFilter.Property1 = new ZDateTime(2012, 8, 9);
			leaveRangeFilter.Property2 = ZDateTime.Empty;
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(5, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "";
			leaveRangeFilter.Property1 = new ZDateTime(2018, 8, 19);
			leaveRangeFilter.Property2 = new ZDateTime(2018, 9, 10);
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(3, staffCollection.Length);
			AssertCollectionContains(staff2, staffCollection);
			AssertCollectionContains(staff3, staffCollection);
			AssertCollectionContains(staff5, staffCollection);

			leaveRangeFilter.WorkHolidayType = "";
			leaveRangeFilter.Property1 = new ZDateTime(2008, 9, 16, 6, 12, 0);
			leaveRangeFilter.Property2 = new ZDateTime(2008, 9, 16, 6, 13, 0);
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(0, staffCollection.Length);

			leaveRangeFilter.WorkHolidayType = "";
			leaveRangeFilter.Property1 = new ZDateTime(2008, 9, 16, 6, 14, 0);
			leaveRangeFilter.Property2 = new ZDateTime(2008, 9, 17, 6, 14, 0);
			leaveRangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(1, staffCollection.Length);
			AssertCollectionContains(staff6, staffCollection);

			leaveRangeFilter.Property1 = ZDateTime.Empty;
			leaveRangeFilter.Property2 = ZDateTime.Empty;
			leaveRangeFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(6, staffCollection.Length);

			leaveRangeFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			leaveRangeFilter.IsActive = true;
			staffCollection = Factory.Load<GlbStaff>(filterBizO.Filter);
			AssertEquals(1, staffCollection.Length);
			AssertCollectionContains(staffNoHoliday, staffCollection);
		}

		protected class DummyFilterBizOForLeaveDataRangeWithTypeFilter : DummyFilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var collection = new ModuleFilterCollection();
				collection.AddCustomFilter(new LeaveDateRangeWithTypeFilterTest().Filter);
				return collection;
			}
		}

		protected override LeaveDateRangeWithTypeFilter GetNewModuleFilter()
		{
			return new LeaveDateRangeWithTypeFilter("moo");
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Dates;
	}
}
