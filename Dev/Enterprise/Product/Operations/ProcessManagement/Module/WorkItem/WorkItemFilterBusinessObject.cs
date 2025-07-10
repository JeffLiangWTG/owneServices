using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using static Enterprise.ProcessManagement.Module.SearchFieldConstants;
using static Enterprise.ZArchitecture.Business.IndexSearchModuleTextFilter;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleFountainFilter("Work Item Number", WorkItemSchema.WKI_WorkItemNumber, "WI");
			filter.MultilingualDescription = ResString.GetMultilingualString("WorkItemFilter|Number", "Work Item Number");
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddWorkItemFilters(result);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, result);
			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowWithExtraTasksFilterStripsHelper(typeof(WorkItem), JobInvoicingConsumerTypes.WorkItem.Code, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}

		void AddWorkItemFilters(ModuleFilterCollection moduleFilters)
		{
			var registry = ProcessManagementRegistry.Instance;

			var typeFilter = moduleFilters.AddTextFilter("Work Item Type", WorkItemSchema.WKI_WorkItemType, Lookups.ActiveTypes);
			typeFilter.MultilingualDescription = registry.WorkItemTypeLabel.Value;

			var areaFilter = moduleFilters.AddTextFilter("Work Item Area", WorkItemSchema.WKI_WorkItemArea, CreateActiveAreasForSQL);
			areaFilter.MultilingualDescription = registry.WorkItemAreaLabel.Value;

			var activityTypeFilter = moduleFilters.AddTextFilter("Activity Type", WorkItemSchema.WKI_ActivityType, CreateActiveActivityTypesForSQL);
			activityTypeFilter.MultilingualDescription = registry.WorkItemActivityTypeLabel.Value;

			var activitySubtypeFilter = moduleFilters.AddTextFilter("Activity Subtype", WorkItemSchema.WKI_ActivitySubtype, CreateActiveActivitySubtypesForSQL);
			activitySubtypeFilter.MultilingualDescription = registry.WorkItemActivitySubTypeLabel.Value;

			var priorityFilter = moduleFilters.AddTextFilter("Priority", WorkItemSchema.WKI_Priority, CreateActivePrioritiesForSQL);
			priorityFilter.MultilingualDescription = registry.WorkItemPriorityLabel.Value;

			var summaryFilter = moduleFilters.AddTextFilter("Summary", WorkItemSchema.WKI_Summary);
			summaryFilter.MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|Summary", "Summary");

			var statusFilter = moduleFilters.AddTextFilter("Status", GetStatusQuery, StatusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|Status", "Status");

			var companyFilter = moduleFilters.AddGuidFilter("Company", ModuleIDs.GlbCompany, WorkItemSchema.WKI_GC_AssignedCompany, Lookups.AssignedCompanies);
			companyFilter.Category = FilterCategories.Locations;
			companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|Company", "Company");

			var portOrCountryFilter = moduleFilters.AddNkFilter("PortOrCountry", WorkItemSchema.WKI_PortOrCountry, ModuleIDs.Location, Lookups.Locations);
			portOrCountryFilter.Category = FilterCategories.Locations;
			portOrCountryFilter.MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|PortOrCountry", "Country/Region/Port");
			portOrCountryFilter.SupportsFiltersMatchComparisonOperator = false;

			var departmentfilter = moduleFilters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, WorkItemSchema.WKI_GE_AssignedDepartment, Lookups.AssignedDepartments);
			departmentfilter.Category = FilterCategories.Locations;
			departmentfilter.MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|Department", "Department");

			moduleFilters.AddFilter(new RelatedProjectsOfWorkItemFilter("Related Projects", () => new ProjectCollection(Factory)));

			var relatedTicketsFilter = new ModuleGuidPivotFilter("Related Tickets", ModuleIDs.CustomerServiceTicket, WorkItemRequestLinkSchema.WKL_WKR_Request, WorkItemRequestLinkSchema.WKL_WKI_WorkItem, new WorkRequestCollection(Factory), typeof(WorkItem), typeof(WorkItemRequestLink))
			{
				Category = RelatedItemsFilterCategory,
				MultilingualDescription = ResString.GetMultilingualString("WorkItem|Filter|Related Tickets", "Related Customer Service Tickets")
			};
			moduleFilters.AddFilter(relatedTicketsFilter);
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case PortOrCountry:
					var indexNKFilter = new IndexSearchModuleNKFilter(searchField, ModuleIDs.Location, Lookups.Locations, FilterCategories.Locations);
					return new SearchFieldOverride(searchField, indexFilterOverride: indexNKFilter);
				case Company:
				case Department:
					return new SearchFieldOverride(searchField, category: FilterCategories.Locations);
				case WorkItemType:
					var typeFilter = new IndexSearchModuleTextFilter(searchField, Lookups.ActiveTypes);
					return new SearchFieldOverride(searchField, indexFilterOverride: typeFilter);
				case WorkItemArea:
					var areaFilter = new IndexSearchModuleTextFilter(searchField, CreateActiveAreasForIndex);
					return new SearchFieldOverride(searchField, indexFilterOverride: areaFilter);
				case ActivityType:
					var activityFilter = new IndexSearchModuleTextFilter(searchField, CreateActiveActivityTypesForIndex);
					return new SearchFieldOverride(searchField, indexFilterOverride: activityFilter);
				case ActivitySubtype:
					var activitySubTypeFilter = new IndexSearchModuleTextFilter(searchField, CreateActiveActivitySubtypesForIndex);
					return new SearchFieldOverride(searchField, indexFilterOverride: activitySubTypeFilter);
				case WorkItemPriority:
					var priorityFilter = new IndexSearchModuleTextFilter(searchField, CreateActivePrioritiesForIndex);
					return new SearchFieldOverride(searchField, indexFilterOverride: priorityFilter);
				case Status:
					var statusFilter = new StatusIndexFilter(searchField, StatusList);
					return new SearchFieldOverride(searchField, indexFilterOverride: statusFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		#region Lookups

		protected WorkItemActualLookups Lookups
		{
			get { return lookups ?? (lookups = new WorkItemActualLookups(Factory)); }
		}
		WorkItemActualLookups lookups;

		CodeDescriptionPairList CreateActiveAreasForIndex() => CreateActiveAreasCore(SearchType.Index);
		CodeDescriptionPairList CreateActiveAreasForSQL() => CreateActiveAreasCore(SearchType.Sql);
		CodeDescriptionPairList CreateActiveAreasCore(SearchType searchType)
		{
			var values = searchType == SearchType.Sql ? GetSelectedValuesInWorkItemTreeFiltersIfValid(1) : GetSelectedValuesInWorkItemTreeFiltersIfValidForIndex(1);
			if (values != null && values.Count == 1)
			{
				return WorkItemActualLookups.GetAreas(Factory, values.ToArray()[0], true);
			}
			else
			{
				return WorkItemActualLookups.GetAreas(Factory, "", true);
			}
		}

		CodeDescriptionPairList CreateActiveActivityTypesForIndex() => CreateActiveActivityTypesCore(SearchType.Index);
		CodeDescriptionPairList CreateActiveActivityTypesForSQL() => CreateActiveActivityTypesCore(SearchType.Sql);
		CodeDescriptionPairList CreateActiveActivityTypesCore(SearchType searchType)
		{
			var values = searchType == SearchType.Sql ? GetSelectedValuesInWorkItemTreeFiltersIfValid(2) : GetSelectedValuesInWorkItemTreeFiltersIfValidForIndex(2);
			if (values != null && values.Count == 2)
			{
				return WorkItemActualLookups.GetActivityTypeList(Factory, values[0], values[1], true);
			}
			else
			{
				return WorkItemActualLookups.GetActivityTypeList(Factory, "", "", true);
			}
		}

		CodeDescriptionPairList CreateActiveActivitySubtypesForIndex() => CreateActiveActivitySubtypesCore(SearchType.Index);
		CodeDescriptionPairList CreateActiveActivitySubtypesForSQL() => CreateActiveActivitySubtypesCore(SearchType.Sql);
		CodeDescriptionPairList CreateActiveActivitySubtypesCore(SearchType searchType)
		{
			var values = searchType == SearchType.Sql ? GetSelectedValuesInWorkItemTreeFiltersIfValid(3) : GetSelectedValuesInWorkItemTreeFiltersIfValidForIndex(3);
			if (values != null && values.Count == 3)
			{
				return WorkItemActualLookups.GetActivitySubtypeList(Factory, values[0], values[1], values[2], true);
			}
			else
			{
				return WorkItemActualLookups.GetActivitySubtypeList(Factory, "", "", "", true);
			}
		}

		CodeDescriptionPairList CreateActivePrioritiesForIndex() => CreateActivePrioritiesCore(SearchType.Index);
		CodeDescriptionPairList CreateActivePrioritiesForSQL() => CreateActivePrioritiesCore(SearchType.Sql);
		CodeDescriptionPairList CreateActivePrioritiesCore(SearchType searchType)
		{
			var values = searchType == SearchType.Sql ? GetSelectedValuesInWorkItemTreeFiltersIfValid(4) : GetSelectedValuesInWorkItemTreeFiltersIfValidForIndex(4);
			if (values != null && values.Count == 4)
			{
				return WorkItemActualLookups.GetPriorities(Factory, true, values[0], values[1], values[2], values[3]);
			}
			else
			{
				return WorkItemActualLookups.GetPriorities(Factory, true, "", "", "", "");
			}
		}

		List<ZString> GetSelectedValuesInWorkItemTreeFiltersIfValid(int branches)
		{
			var columns = new List<SchemaColumn>();
			if (branches > 0)
			{
				columns.Add(WorkItemSchema.WKI_WorkItemType);
			}
			if (branches > 1)
			{
				columns.Add(WorkItemSchema.WKI_WorkItemArea);
			}
			if (branches > 2)
			{
				columns.Add(WorkItemSchema.WKI_ActivityType);
			}
			if (branches > 3)
			{
				columns.Add(WorkItemSchema.WKI_ActivitySubtype);
			}

			if (moduleFiltersCreated && branches > 0)
			{
				var filters = ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => columns.Contains(x.FilterColumn) &&
					!x.HasNotifications() && (x.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact ||
					x.ComparisonOperator == ModuleTextFilter.ComparisonConstants.StartsWith)).ToList();

				if (filters.Count == branches && filters.Select(x => x.FilterColumn).Distinct().Count() == branches)
				{
					var order = columns.Select((column, index) => new { column, index }).ToDictionary(o => o.column, o => o.index);
					return filters.OrderBy(x => order[x.FilterColumn]).Select(x => x.Property).ToList();
				}
			}
			return null;
		}
		List<ZString> GetSelectedValuesInWorkItemTreeFiltersIfValidForIndex(int branches)
		{
			var columns = new List<string>();
			if (branches > 0)
			{
				columns.Add(WorkItemType);
			}
			if (branches > 1)
			{
				columns.Add(WorkItemArea);
			}
			if (branches > 2)
			{
				columns.Add(ActivityType);
			}
			if (branches > 3)
			{
				columns.Add(ActivitySubtype);
			}

			if (branches > 0)
			{
				var filters = ActiveModuleFilters.OfType<IndexSearchModuleTextFilter>().Where(
					x => columns.Contains(x.SearchFieldName)
						&& !x.HasNotifications()
						&& (x.ComparisonOperator == IndexSearchTextFilterConstants.AnyStartsWith || x.ComparisonOperator == IndexSearchTextFilterConstants.AllExact || x.ComparisonOperator == IndexSearchTextFilterConstants.AnyExact))
					.ToList();

				if (filters.Count == branches && filters.Select(x => x.SearchFieldName).Distinct().Count() == branches)
				{
					var order = columns.Select((column, index) => new { column, index }).ToDictionary(o => o.column, o => o.index);
					return filters.OrderBy(x => order[x.SearchFieldName]).Select(x => x.Property).ToList();
				}
			}
			return null;
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			moduleFiltersCreated = true;
		}

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			moduleFiltersCreated = false;
		}

		bool moduleFiltersCreated;

		#endregion

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get
			{
				if (statusList == null)
				{
					statusList = new ProcessTaskFilterBusinessObject().Statuses;
				}

				return statusList;
			}
		}
		CodeDescriptionPairList statusList;

		ZQuery GetStatusQuery(ZString value)
		{
			return ProcessTaskFilterBusinessObject.GetStatusQuery(value, WorkItemSchema.WKI_Status);
		}

		#endregion

		#region CRM Security

		readonly WorkItemCRMSecurityProvider workItemSecurityProvider = new WorkItemCRMSecurityProvider();

		WorkItemCRMSecurityProvider SecurityProvider
		{
			get
			{
				return workItemSecurityProvider;
			}
		}

		#endregion

		public static FilterCategory RelatedItemsFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("7d279dd9-86db-4bbf-a712-bbd9d72da526", "Related Items"));
	}
}
