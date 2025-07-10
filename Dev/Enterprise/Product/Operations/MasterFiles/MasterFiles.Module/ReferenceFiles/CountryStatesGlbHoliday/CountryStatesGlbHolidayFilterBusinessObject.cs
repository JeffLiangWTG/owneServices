using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CountryStatesGlbHolidayFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddCustomCountryStateFilter(filters);
			AddFlagFilters(filters);
			return filters;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var recurringFlag = ResString.GetMultilingualString("03499c46-71d9-4e31-b217-655af28ffe74", "Recurring");
			filters.AddFlagFilter("Recurring", recurringFlag, GlbHolidaySchema.GH_Recurring, ModuleFilterSubGroup.Default)
				.MultilingualDescription = recurringFlag;
			var workingDayFlag = ResString.GetMultilingualString("540db6e5-5620-4ee0-b680-4446e22c4879", "Is Working Day");
			filters.AddFlagFilter("Is Working Day", workingDayFlag, GlbHolidaySchema.GH_IsWorkingDay, ModuleFilterSubGroup.Default)
				.MultilingualDescription = workingDayFlag;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Holiday Name", GlbHolidaySchema.GH_HolidayName)
				.MultilingualDescription = ResString.GetMultilingualString("bcda3522-d0ee-497c-9c4f-661c0dba1d49", "Holiday Name");

			var activeStatusFilter = filters.AddTextFilter(ActiveStatus, GetActiveStatusQuery, CancelledStatusList);
			activeStatusFilter.Property = ActiveStatus;
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			if (IsActiveStatusFilterAlwaysApplied())
			{
				activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
				activeStatusFilter.DefaultProperty = CancelledStatusList[StatusActive].Code;
			}
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("babf0991-6d4c-4b77-a72d-46772a415a24", "Active Status");
		}

		#region Country/State filter

		void AddCustomCountryStateFilter(ModuleFilterCollection filters)
		{
			var stateModuleFilter = new CountryStatesGlbHolidayCountryStateModuleFilter("CountryState", GlbHolidaySchema.GH_ParentID, GlbHolidaySchema.GH_ParentTableCode);
			stateModuleFilter.MultilingualDescription = ResString.GetMultilingualString("B994416E-0444-44EF-8A84-8BAF888B1B8D", "Country/Region/State");
			filters.AddCustomFilter(stateModuleFilter);
		}

		#endregion

		#region CodeDescriptionPairList

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Status string")]
		const string ActiveStatus = "Active Status";
		ZQuery GetActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbHolidaySchema.GH_IsActive, false);
				query.IgnoreActiveFilter = true;
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbHolidaySchema.GH_IsActive, true);
				query.IgnoreActiveFilter = false;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbHolidaySchema.GH_IsActive, true);
				query.AddToFilter(JoinCondition.Or, GlbHolidaySchema.GH_IsActive, false);
				query.IgnoreActiveFilter = true;
			}

			return query;
		}

		#endregion
	}
}
