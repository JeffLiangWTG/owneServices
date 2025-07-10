using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffHolidayFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddDateFilters(filters);
			AddTextFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Work Holiday Type", GlbStaffHolidaySchema.GA_WorkHolidayType).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffHolidayFilter|WorkHolidayType", "Work Holiday Type");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var startDateFilter = filters.AddDateFilter("Start Date", GlbStaffHolidaySchema.GA_StartTime);
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffHolidayFilter|StartDate", "Start Date");

			var endDateFilter = filters.AddDateFilter("End Date", GlbStaffHolidaySchema.GA_EndTime);
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffHolidayFilter|EndDate", "End Date");
		}
	}
}
