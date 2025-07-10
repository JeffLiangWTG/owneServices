using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class CustomerServiceTicketFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter(FilterDescriptions.TicketNumber, WorkRequestSchema.WKR_RequestNumber, "CST")
			{
				MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.TicketNumber", "Ticket Number")
			};
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddTextFilter(FilterDescriptions.Summary, WorkRequestSchema.WKR_Summary).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Summary", "Summary");
			result.AddTextFilter(FilterDescriptions.Description, WorkRequestSchema.WKR_Description).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Description", "Description");
			result.AddTextFilter(FilterDescriptions.Status, WorkRequestSchema.WKR_Status, () => new TicketStatusList()).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Status", "Status");

			result.AddTextFilter(FilterDescriptions.SelectionCriterion1, WorkRequestSchema.WKR_SelectionCriteria1).MultilingualDescription = ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value;
			result.AddTextFilter(FilterDescriptions.SelectionCriterion2, WorkRequestSchema.WKR_SelectionCriteria2).MultilingualDescription = ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value;
			result.AddTextFilter(FilterDescriptions.SelectionCriterion3, WorkRequestSchema.WKR_SelectionCriteria3).MultilingualDescription = ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value;
			result.AddTextFilter(FilterDescriptions.SelectionCriterion4, WorkRequestSchema.WKR_SelectionCriteria4).MultilingualDescription = ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value;
			result.AddTextFilter(FilterDescriptions.SelectionCriterion5, WorkRequestSchema.WKR_SelectionCriteria5).MultilingualDescription = ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value;

			result.AddGuidFilter(FilterDescriptions.Branch, ModuleIDs.GlbBranch, WorkRequestSchema.WKR_GB_Branch, () => new GlbBranchCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Branch", "Branch");
			result.AddGuidFilter(FilterDescriptions.Department, ModuleIDs.GlbDepartment, WorkRequestSchema.WKR_GE_Department, () => new GlbDepartmentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Department", "Department");
			result.AddGuidFilter(FilterDescriptions.Client, ModuleIDs.OrgContacts, WorkRequestSchema.WKR_OC_Client, () => new OrgContactCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Client", "Client");
			result.AddFilter(new CustomerServiceTicketOrganizationFilter(() => new OrgHeaderCollection(Factory)));

			result.AddNkFilter(FilterDescriptions.Country, WorkRequestSchema.WKR_RN_NKCountry, ModuleIDs.RefCountry, () => new RefCountryCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Country", "Country/Region");

			var relatedWorkItemsFilter = new ModuleGuidPivotFilter(FilterDescriptions.RelatedWorkItems, ModuleIDs.WorkItem, WorkItemRequestLinkSchema.WKL_WKI_WorkItem, WorkItemRequestLinkSchema.WKL_WKR_Request, new WorkItemCollection(Factory), typeof(WorkRequest), typeof(WorkItemRequestLink))
			{
				Category = RelatedItemsFilterCategory,
				MultilingualDescription = ResString.GetMultilingualString("CustomerServiceTicketFilterBusinessObject.Related Work Items", "Related Work Items")
			};
			result.AddFilter(relatedWorkItemsFilter);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var list = base.GetCustomFilterStripsHelpersCore();

			list.Add(new WorkflowWithExtraTasksFilterStripsHelper(typeof(WorkRequest), WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode, Factory));

			return list;
		}

		public static class FilterDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Branch = "Branch";
			public const string Client = "Client";
			public const string Country = "Country";
			public const string Department = "Department";
			public const string Description = "Description";
			public const string Organization = "Organization";
			public const string Status = "Status";
			public const string RelatedWorkItems = "Related Work Items";
			public const string TicketNumber = "Ticket Number";
			public const string SelectionCriterion1 = "Selection Criterion 1";
			public const string SelectionCriterion2 = "Selection Criterion 2";
			public const string SelectionCriterion3 = "Selection Criterion 3";
			public const string SelectionCriterion4 = "Selection Criterion 4";
			public const string SelectionCriterion5 = "Selection Criterion 5";
			public const string Summary = "Summary";

			#endregion
		}

		public static FilterCategory RelatedItemsFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("d248b3a8-4369-4ed3-8d1b-f5378d7e8638", "Related Items"));
	}
}
