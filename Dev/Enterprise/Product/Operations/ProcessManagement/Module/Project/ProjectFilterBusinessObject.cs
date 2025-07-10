using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using static Enterprise.ProcessManagement.Module.SearchFieldConstants;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleFountainFilter(FilterDescription.ProjectNumber, WorkProjectSchema.WKP_ProjectNumber, "PRJ");
			filter.MultilingualDescription = ResString.GetMultilingualString("ProjectFilter|Number", FilterDescription.ProjectNumber);
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddProjectFilters(result);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowWithExtraTasksFilterStripsHelper(typeof(Project), JobInvoicingConsumerTypes.Project.Code, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}

		void AddProjectFilters(ModuleFilterCollection moduleFilters)
		{
			var registry = ProcessManagementRegistry.Instance;

			var typeFilter = moduleFilters.AddTextFilter("Project Type", WorkProjectSchema.WKP_Type, Lookups.ActiveTypes);
			typeFilter.MultilingualDescription = registry.ProjectTypeLabel.Value;

			var activitySubtypeFilter = moduleFilters.AddTextFilter("Project Sub Type", WorkProjectSchema.WKP_SubType, Lookups.ActiveSubtypes);
			activitySubtypeFilter.MultilingualDescription = registry.ProjectSubtypeLabel.Value;

			var moduleFilter = moduleFilters.AddTextFilter("Project Module", WorkProjectSchema.WKP_Module, Lookups.ActiveModules);
			moduleFilter.MultilingualDescription = registry.ProjectModuleLabel.Value;

			var deliveryFilter = moduleFilters.AddTextFilter("Priority", WorkProjectSchema.WKP_Priority, Lookups.ActivePriorities);
			deliveryFilter.MultilingualDescription = registry.ProjectPriorityLabel.Value;

			var summaryFilter = moduleFilters.AddTextFilter(FilterDescription.Summary, WorkProjectSchema.WKP_Summary);
			summaryFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|Summary", FilterDescription.Summary);
			summaryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			summaryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var statusFilter = moduleFilters.AddTextFilter(FilterDescription.Status, GetStatusQuery, StatusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|Status", FilterDescription.Status);

			var clientFilter = moduleFilters.AddGuidFilter(FilterDescription.Client, ModuleIDs.Organisation, GetClientQuery, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|Client", FilterDescription.Client);

			var clientNameFilter = moduleFilters.AddTextFilter(FilterDescription.ClientName, GetClientQuery);
			clientNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			clientNameFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|ClientName", FilterDescription.ClientName);
			clientNameFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			clientNameFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var projectManager = moduleFilters.AddNkFilter(FilterDescription.ProjectManager, WorkProjectSchema.WKP_GS_NKProjectManager, ModuleIDs.GlbStaff, Lookups.ProjectManagers);
			projectManager.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|ProjectManager", FilterDescription.ProjectManager);

			var opportunity = moduleFilters.AddGuidFilter(FilterDescription.Opportunity, ModuleIDs.Opportunity, WorkProjectSchema.WKP_P8_Opportunity, Opportunities);
			opportunity.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|Opportunity", FilterDescription.Opportunity);

			var opportunitySalesPerson = moduleFilters.AddNkFilter(FilterDescription.OpportunitySalesPerson, GetOpportunitySalesPersonQuery, ModuleIDs.GlbStaff, Lookups.Staff);
			opportunitySalesPerson.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|OpportunitySalesPerson", FilterDescription.OpportunitySalesPerson);

			var locationFilter = moduleFilters.AddNkFilter(FilterDescription.Location, GetLocationQuery, ModuleIDs.Location, Locations);
			locationFilter.MaxLength = OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength;
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|Location", FilterDescription.Location);

			var startedFilter = moduleFilters.AddDateFilter(FilterDescription.ProjectStarted, WorkProjectSchema.WKP_SystemCreateTimeUtc, true);
			startedFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|ProjectStarted", FilterDescription.ProjectStarted);

			var completedFilter = moduleFilters.AddDateFilter(FilterDescription.ProjectCompleted, WorkProjectSchema.WKP_ClosedDate);
			completedFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|ProjectCompleted", FilterDescription.ProjectCompleted);

			var deferredFilter = moduleFilters.AddDateFilter(FilterDescription.ProjectDeferred, GetDeferredQuery);
			deferredFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|ProjectDeferred", FilterDescription.ProjectDeferred);

			var deferDateFilter = moduleFilters.AddFlagsFilter(FilterDescription.DefDateMet, new string[] { Res.GetString("E36A624A-71B1-4CA7-8055-3AC47C67EBF6", "Def. Date Met") }, new GetFlagsQuery[] { GetDeferredDateMetQuery });
			deferDateFilter.MultilingualDescription = ResString.GetMultilingualString("Project|Filter|DeferDateMet", FilterDescription.DefDateMet);
			deferDateFilter.Category = FilterCategories.Dates;

			moduleFilters.AddFilter(new RelatedWorkItemsOfProjectFilter("Related Work Items", () => new WorkItemCollection(Factory)));
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case Status:
					var statusFilter = new StatusIndexFilter(searchField, StatusList);
					return new SearchFieldOverride(searchField, indexFilterOverride: statusFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		#region Lookups

		protected WorkProjectLookups Lookups
		{
			get { return lookups ?? (lookups = new WorkProjectLookups(Factory)); }
		}
		WorkProjectLookups lookups;

		#region Clients

		public OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (clients == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True);
					clients = new OrganisationsFindBoxCollection(Factory, filter);
				}

				return clients;
			}
		}
		OrganisationsFindBoxCollection clients;

		static ZQuery GetClientQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var projectQuery = new ZDBOnlyQuery(typeof(Project));

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				projectQuery.AddToFilter(WorkProjectSchema.WKP_OA_ClientAddress, null);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				projectQuery.AddToFilter(WorkProjectSchema.WKP_OA_ClientAddress, SQLComparisonOperator.NotEqual, null);
			}

			if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				var notIn = comparisonOperator == SQLComparisonOperator.NotEqual;
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK, notIn);

				if (ZGuid.TryParse(value, out var clientPK))
				{
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, clientPK);
				}

				projectQuery.AddSubQuery(WorkProjectSchema.WKP_OA_ClientAddress, orgAddressQuery, JoinCondition.Or);
			}

			return projectQuery;
		}

		ZQuery GetClientQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlySubQuery orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

			ZDBOnlySubQuery orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgQuery, JoinCondition.And);

			ZDBOnlyQuery projectQuery = new ZDBOnlyQuery(typeof(Project));
			projectQuery.AddSubQuery(WorkProjectSchema.WKP_OA_ClientAddress, orgAddressQuery, JoinCondition.And);

			return projectQuery;
		}

		#endregion

		#region Deferred

		ZQuery GetDeferredQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery deferredQuery = new ZDBOnlyQuery(typeof(Project));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.FH_ParentId);
			var dateQuery = new DateQueryBuilder(true).CreateDateTimeRange(comparisonOperator, ProcessHeaderSchema.FH_DoNotStartBeforeDate, value1, value2, false, false);

			subQuery.AddToFilter(dateQuery);

			deferredQuery.AddSubQuery(WorkProjectSchema.PK, subQuery, JoinCondition.And);

			return deferredQuery;
		}

		ZQuery GetDeferredDateMetQuery(ZBool value)
		{
			ZDBOnlyQuery deferredQuery = new ZDBOnlyQuery(typeof(Project));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.FH_ParentId);
			if (value)
			{
				subQuery.AddToFilter(ProcessHeaderSchema.FH_DoNotStartBeforeDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			}
			else
			{
				subQuery.AddToFilter(ProcessHeaderSchema.FH_DoNotStartBeforeDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow);
			}

			deferredQuery.AddSubQuery(WorkProjectSchema.PK, subQuery, JoinCondition.And);

			return deferredQuery;
		}

		#endregion

		#region Location

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		ZQuery GetLocationQuery(ZString clientBranch)
		{
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgSubQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, clientBranch);

			ZDBOnlyQuery projectQuery = new ZDBOnlyQuery(typeof(Project));
			projectQuery.AddSubQuery(WorkProjectSchema.WKP_OA_ClientAddress, orgSubQuery, JoinCondition.And);

			return projectQuery;
		}

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
			return ProcessTaskFilterBusinessObject.GetStatusQuery(value, WorkProjectSchema.WKP_Status);
		}

		#endregion

		#region Opportunit

		public OrgOpportunityCollection Opportunities => opportunities ?? (opportunities = new OrgOpportunityCollection(Factory));
		OrgOpportunityCollection opportunities;

		ZQuery GetOpportunitySalesPersonQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlySubQuery opporunityQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			opporunityQuery.AddToFilter(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, comparisonOperator, value);

			ZDBOnlyQuery projectQuery = new ZDBOnlyQuery(typeof(Project));
			projectQuery.AddSubQuery(WorkProjectSchema.WKP_P8_Opportunity, opporunityQuery, JoinCondition.And);

			return projectQuery;
		}

		#endregion

		#endregion

		public static FilterCategory RelatedWorkItemsFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("094954d6-0ce9-4368-8dfc-f652d3c2dc79", FilterDescription.RelatedWorkItems));

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string RelatedWorkItems = "Related Work Items";
			public const string DefDateMet = "Def. Date Met";
			public const string ProjectDeferred = "Project Deferred";
			public const string ProjectCompleted = "Project Completed";
			public const string ProjectStarted = "Project Started";
			public const string Location = "Location";
			public const string ProjectManager = "Project Manager";
			public const string Opportunity = "Opportunity #";
			public const string OpportunitySalesPerson = "Opportunity Sales Person";
			public const string ClientName = "Client Name";
			public const string Client = "Client";
			public const string Status = "Status";
			public const string Summary = "Summary";
			public const string ProjectNumber = "Project Number";

			#endregion
		}

		#endregion

		#region CRM Security

		readonly ProjectCRMSecurityProvider SecurityProvider = new ProjectCRMSecurityProvider();

		#endregion
	}
}
