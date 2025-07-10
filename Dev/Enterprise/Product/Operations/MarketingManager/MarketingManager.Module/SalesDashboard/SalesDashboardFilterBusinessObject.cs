using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class SalesDashboardFilterBusinessObject : FilterStripBusinessObject
	{
		public SalesDashboardFilterBusinessObject()
		{
			LayoutLoaded += OnLayoutLoaded;
		}

		public SalesDashboardFilterBusinessObject(IOrgHeader org)
			: this()
		{
			this.Org = org;
		}

		internal readonly IOrgHeader Org;

		#region System Defined Filter Layout

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter layout unique name not translated")]
		void OnLayoutLoaded(object sender, System.EventArgs e)
		{
			if (Org == null)
			{
				if (LastUsedLayout != null && LastUsedLayout.S9_FilterNameMultilingual.GetUnresolvedString() == "My Activity Tasks" && LastUsedLayout.S9_IsPublished)
				{
					foreach (ModuleFilter filter in ModuleFilters)
					{
						ModuleTextFilter oADfilter = filter as ModuleTextFilter;
						ModuleNkFilter nkFilter = filter as ModuleNkFilter;
						if (nkFilter != null && nkFilter.IsActive &&
							(nkFilter.Description == ActivityStaffAssignmentFilterName || nkFilter.Description == CurrentTaskAssignedToFilterName))
						{
							nkFilter.Property = Env.CurrentUser.Initials;
						}
						else if (oADfilter != null && oADfilter.IsActive && (oADfilter.Description == OverallActivityDispositionFilterName))
						{
							oADfilter.Property = OrgOpportunityOverallDispositionList.Codes.Open;
						}
					}
				}
			}
		}

		#endregion

		#region Filters

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter unique description not translated")]
		internal const string ActivityStaffAssignmentFilterName = "Activity Staff Assignment";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter unique description not translated")]
		internal const string CurrentTaskAssignedToFilterName = "Current Task Assigned To";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter unique description not translated")]
		internal const string OverallActivityDispositionFilterName = "Overall Activity Disposition";

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddRelationshipOrgAndStaffFilters(filters);
			AddWorkflowTasksFilters(filters);

			filters.AddTextFilter(FilterDescription.ActivityDescription, ViewSalesDashboardActivitySchema.VSA_ActivityDescription).MultilingualDescription = ResString.GetMultilingualString("3407247f-0c1c-4131-8af3-856368a07c2b", FilterDescription.ActivityDescription);
			filters.AddTextFilter(FilterDescription.ContactName, ViewSalesDashboardActivitySchema.VSA_ActivityContactName).MultilingualDescription = ResString.GetMultilingualString("153aa7b9-016f-486b-a30a-f0b72aa033ed", FilterDescription.ContactName);
			filters.AddDateFilter(FilterDescription.ActivityDate, ViewSalesDashboardActivitySchema.VSA_ActivityDate, true).MultilingualDescription = ResString.GetMultilingualString("db2ccd2a-615c-461e-bf36-b5c6aa52ad38", FilterDescription.ActivityDate);

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		#region NumbersAndReferences Filters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var activityParentIdFilter = filters.AddTextFilter(FilterDescription.ActivityParentID, ViewSalesDashboardActivitySchema.VSA_ActivityParentID);
			activityParentIdFilter.MultilingualDescription = ResString.GetMultilingualString("fdf094b1-7659-4ead-bdf3-ac9d0c8b379b", FilterDescription.ActivityParentID);
			activityParentIdFilter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region StatusAndFlags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var activityTypeFilter = filters.AddFlagsFilter(FilterDescription.ActivityType,
					new string[]
					{
						SalesDashboardActivityTypeCodeList.Descriptions.Opportunity,
						SalesDashboardActivityTypeCodeList.Descriptions.Inquiry,
						SalesDashboardActivityTypeCodeList.Descriptions.Communication,
						SalesDashboardActivityTypeCodeList.Descriptions.Campaign,
						SalesDashboardActivityTypeCodeList.Descriptions.OneOffQuote,
						SalesDashboardActivityTypeCodeList.Descriptions.Quotation,
						SalesDashboardActivityTypeCodeList.Descriptions.Project
					},
					new GetFlagsQuery[]
					{
						GetOpportunitiesOnlyQuery,
						GetInquiriesOnlyQuery,
						GetCommunicationsOnlyQuery,
						GetCampaignsOnlyQuery,
						GetOneOffQuotesOnlyQuery,
						GetQuotationsOnlyQuery,
						GetProjectsOnlyQuery
					},
					JoinCondition.Or);
			activityTypeFilter.MultilingualDescription = ResString.GetMultilingualString("9b7bde29-4edb-433c-be34-04ff13f15bc1", FilterDescription.ActivityType);
			activityTypeFilter.Category = FilterCategories.StatusAndFlags;

			var opportunityStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetOpportunitiesOnlyQuery(true)));
			var opportunityStatusFilter = filters.AddTextFilter(FilterDescription.OpportunityStatus, opportunityStatusQuery, OpportunityStatuses);
			opportunityStatusFilter.MultilingualDescription = ResString.GetMultilingualString("f91ef5da-1a0a-43e7-9ddf-f2415ac593f9", FilterDescription.OpportunityStatus);
			opportunityStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(opportunityStatusFilter);

			var opportunityStageQuery = new GetTextQueryWithOperator((comparisonOperator, stage) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStage, comparisonOperator, stage), GetOpportunitiesOnlyQuery(true)));
			var opportunityStageFilter = filters.AddTextFilter(FilterDescription.OpportunityStage, opportunityStageQuery, OpportunityStages);
			opportunityStageFilter.MultilingualDescription = ResString.GetMultilingualString("6E537514-AC7F-4DD1-A0E2-2D1FF9074048", FilterDescription.OpportunityStage);
			opportunityStageFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(opportunityStageFilter);

			var inquiryStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetInquiriesOnlyQuery(true)));
			var inquiryStatusFilter = filters.AddTextFilter(FilterDescription.InquiryStatus, inquiryStatusQuery, InquiryStatuses);
			inquiryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("1282b607-dbfb-4645-8f58-cb50305976cb", FilterDescription.InquiryStatus);
			inquiryStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(inquiryStatusFilter);

			var communicationStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetCommunicationsOnlyQuery(true)));
			var communicationStatusFilter = filters.AddTextFilter(FilterDescription.CommunicationStatus, communicationStatusQuery, CommunicationStatuses);
			communicationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("5ecc196b-dc84-41dc-bf19-c21726d6e404", FilterDescription.CommunicationStatus);
			communicationStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(communicationStatusFilter);

			var campaignStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetCampaignsOnlyQuery(true)));
			var campaignStatusFilter = filters.AddTextFilter(FilterDescription.CampaignStatus, campaignStatusQuery, CampaignStatuses);
			campaignStatusFilter.MultilingualDescription = ResString.GetMultilingualString("26D96582-1679-447A-ACF9-1885019E63C7", FilterDescription.CampaignStatus);
			campaignStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(campaignStatusFilter);

			var oneOffQuoteStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetOneOffQuotesOnlyQuery(true)));
			var oneOffQuoteStatusFilter = filters.AddTextFilter(FilterDescription.OneOffQuoteStatus, oneOffQuoteStatusQuery, OneOffQuoteStatuses);
			oneOffQuoteStatusFilter.MultilingualDescription = ResString.GetMultilingualString("219C342B-B871-4DC9-97E0-3345E52860CC", FilterDescription.OneOffQuoteStatus);
			oneOffQuoteStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(oneOffQuoteStatusFilter);

			var quotationStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetQuotationsOnlyQuery(true)));
			var quotationStatusFilter = filters.AddTextFilter(FilterDescription.QuotationStatus, quotationStatusQuery, QuotationStatuses);
			quotationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("04DF344A-7D29-4C4D-BF3E-0DE8E8F35A4A", FilterDescription.QuotationStatus);
			quotationStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(quotationStatusFilter);

			var projectStatusQuery = new GetTextQueryWithOperator((comparisonOperator, status) => new ZQuery(new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, comparisonOperator, status), GetProjectsOnlyQuery(true)));
			var projectStatusFilter = filters.AddTextFilter(FilterDescription.ProjectStatus, projectStatusQuery, ProcessTaskStatuses);
			projectStatusFilter.MultilingualDescription = ResString.GetMultilingualString("69A149E8-D0A1-480A-8CAA-2926951952B4", FilterDescription.ProjectStatus);
			projectStatusFilter.Category = FilterCategories.StatusAndFlags;
			SetComparisonOperators(projectStatusFilter);

			var overallActivityDispositionFilter = filters.AddTextFilter(OverallActivityDispositionFilterName, GetOverallActivityDispositionQuery, OADList);
			overallActivityDispositionFilter.MultilingualDescription = ResString.GetMultilingualString("3E199E9A-E410-4E35-8D38-2A64BC4B0FB8", OverallActivityDispositionFilterName);
			overallActivityDispositionFilter.Category = FilterCategories.StatusAndFlags;
		}

		void SetComparisonOperators(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.Clear();
			filter.ComparisonOperator_List.Add(ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.Exact));
			filter.ComparisonOperator_List.Add(ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.NotEqual));
		}

		#region Overall Activity Disposition Query

		ZQuery GetOverallActivityDispositionQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (!value.IsEmpty)
			{
				ZQuery opportunityStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Opportunity);
				var closedOpportunityStatuses = new List<string>();
				foreach (CodeDescriptionBool status in OrganisationsDataRegistry.Instance.OpportunityStatus.Value)
				{
					if (status.Bool)
					{
						closedOpportunityStatuses.Add(status.Code);
					}
				}
				if (value == OrgOpportunityOverallDispositionList.Codes.Closed)
				{
					opportunityStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, closedOpportunityStatuses);
				}
				else
				{
					opportunityStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, closedOpportunityStatuses);
				}

				ZQuery communicationStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Communication);
				var closedCommunicationStatuses = new List<string>();
				foreach (CommunicationStatus status in OrganisationsDataRegistry.Instance.CommunicationStatusList.Value)
				{
					if (status.Closed)
					{
						closedCommunicationStatuses.Add(status.Code);
					}
				}
				if (value == OrgSalesCallOverallDispositionList.Codes.Closed)
				{
					communicationStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, closedCommunicationStatuses);
				}
				else
				{
					communicationStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, closedCommunicationStatuses);
				}

				ZQuery inquiryStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Inquiry);
				if (value == OrgOpportunityOverallDispositionList.Codes.Open)
				{
					inquiryStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SalesEnquiryStatusCodeList.Codes.Open);
				}
				else
				{
					inquiryStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, SalesEnquiryStatusCodeList.Codes.Open);
				}

				ZQuery campaignStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Campaign);
				if (value == OrgSalesCallOverallDispositionList.Codes.Open)
				{
					campaignStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, TrackingStatusCodes.Codes.QUE);
				}
				else
				{
					campaignStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, TrackingStatusCodes.Codes.QUE);
				}

				ZQuery oneOffQuoteStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.OneOffQuote);
				if (value == OrgSalesCallOverallDispositionList.Codes.Open)
				{
					oneOffQuoteStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, OneOffQuoteStatuses.OpenStatuses);
				}
				else
				{
					oneOffQuoteStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, OneOffQuoteStatuses.OpenStatuses);
				}

				ZQuery quotationStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Quotation);
				if (value == OrgSalesCallOverallDispositionList.Codes.Open)
				{
					quotationStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, QuotationStatuses.OpenStatuses);
				}
				else
				{
					quotationStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, QuotationStatuses.OpenStatuses);
				}

				ZQuery projectStatusQuery = new ZQuery(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Project);
				var closedProjectStatuses = new string[] { ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled };
				if (value == OrgSalesCallOverallDispositionList.Codes.Closed)
				{
					projectStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, closedProjectStatuses);
				}
				else
				{
					projectStatusQuery.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityStatus, SQLComparisonOperator.NotEqual, closedProjectStatuses);
				}

				result.AddToFilter(opportunityStatusQuery, JoinCondition.Or);
				result.AddToFilter(communicationStatusQuery, JoinCondition.Or);
				result.AddToFilter(inquiryStatusQuery, JoinCondition.Or);
				result.AddToFilter(campaignStatusQuery, JoinCondition.Or);
				result.AddToFilter(oneOffQuoteStatusQuery, JoinCondition.Or);
				result.AddToFilter(quotationStatusQuery, JoinCondition.Or);
				result.AddToFilter(projectStatusQuery, JoinCondition.Or);
			}
			return result;
		}
		#endregion

		#region Activity Type

		ZQuery GetOpportunitiesOnlyQuery(ZBool opportunitiesOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (opportunitiesOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Opportunity);
			}
			return query;
		}

		ZQuery GetInquiriesOnlyQuery(ZBool inquiriesOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (inquiriesOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Inquiry);
			}
			return query;
		}

		ZQuery GetCommunicationsOnlyQuery(ZBool communicationsOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (communicationsOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Communication);
			}
			return query;
		}

		ZQuery GetCampaignsOnlyQuery(ZBool campaignOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (campaignOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Campaign);
			}
			return query;
		}

		ZQuery GetOneOffQuotesOnlyQuery(ZBool oneOffQuoteOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (oneOffQuoteOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.OneOffQuote);
			}
			return query;
		}

		ZQuery GetQuotationsOnlyQuery(ZBool quotationOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (quotationOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Quotation);
			}
			return query;
		}

		ZQuery GetProjectsOnlyQuery(ZBool projectOnly)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			if (projectOnly)
			{
				query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_ActivityType, SalesDashboardActivityTypeCodeList.Codes.Project);
			}
			return query;
		}
		#endregion

		#endregion

		#region RelationshipOrgAndStaff Filters

		void AddRelationshipOrgAndStaffFilters(ModuleFilterCollection filters)
		{
			if (Org == null)
			{
				var organizationFilter = filters.AddGuidFilter(FilterDescription.Organization, ModuleIDs.Organisation, ViewSalesDashboardActivitySchema.VSA_OH, SalesOrganisations);
				organizationFilter.MultilingualDescription = ResString.GetMultilingualString("30ea4ab2-7575-411d-965e-5b9079e7b538", FilterDescription.Organization);
				organizationFilter.Category = FilterCategories.RelationshipOrgAndStaff;

				var organizationNameFilter = filters.AddTextFilter(FilterDescription.OrganizationName, ViewSalesDashboardActivitySchema.VSA_OrgFullName);
				organizationNameFilter.MultilingualDescription = ResString.GetMultilingualString("0d7c6689-2272-43ca-9f3d-6437eecf9d01", FilterDescription.OrganizationName);
				organizationNameFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			}

			var activityStaffAssignmentFilter = filters.AddNkFilter(ActivityStaffAssignmentFilterName, ViewSalesDashboardActivitySchema.VSA_ActivityStaffAssignment, ModuleIDs.GlbStaff, Staffs);
			activityStaffAssignmentFilter.MultilingualDescription = ResString.GetMultilingualString("ad8dd928-7445-4895-a22d-989c5ae9a228", ActivityStaffAssignmentFilterName);
			activityStaffAssignmentFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var salesTeamFilter = filters.AddNkFilter(FilterDescription.SalesTeam, GetSalesTeamQuery, ModuleIDs.SalesTeam, SalesTeams);
			salesTeamFilter.MaxLength = GlbGroupSchema.GG_Code.MaxLength;
			salesTeamFilter.MultilingualDescription = ResString.GetMultilingualString("364ebdda-8d34-451a-b86b-0139ad7a6dc7", FilterDescription.SalesTeam);
			salesTeamFilter.Category = FilterCategories.RelationshipOrgAndStaff;
		}

		ZQuery GetSalesTeamQuery(SQLComparisonOperator comparisonOperator, ZString nK)
		{
			var salesTeamsQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
			salesTeamsQuery.AddToFilter(GlbGroupSchema.GG_Code, comparisonOperator, nK);
			salesTeamsQuery.AddToFilter(GlbGroupSchema.GG_IsActive, true);
			salesTeamsQuery.AddToFilter(GlbGroupSchema.GG_IsSales, true);

			var groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
			groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, salesTeamsQuery, JoinCondition.And);

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffQuery.AddSubQuery(groupLinkQuery, JoinCondition.And);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommunicationSalesDashboardActivity));
			result.AddSubQuery(ViewSalesDashboardActivitySchema.VSA_ActivityStaffAssignment, staffQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region WorkflowTasks Filters

		readonly FilterCategory workflowTasksCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("64e42b65-1c9f-42c2-9281-270022094e33", FilterDescription.WorkflowTasks));
		void AddWorkflowTasksFilters(ModuleFilterCollection filters)
		{
			var workflowTasksFilterSubGroup = new WorkflowTasksFilterSubGroup();

			var anyOpenTaskAssignedToFilter = filters.AddNkFilter(FilterDescription.AnyOpenTaskAssignedTo, GetAnyOpenTaskAssignedToQuery, ModuleIDs.GlbStaff, Staffs);
			anyOpenTaskAssignedToFilter.SubGroup = workflowTasksFilterSubGroup;
			anyOpenTaskAssignedToFilter.MultilingualDescription = ResString.GetMultilingualString("47236ebc-68b7-4881-9748-05bcfeee6aba", FilterDescription.AnyOpenTaskAssignedTo);
			anyOpenTaskAssignedToFilter.Category = workflowTasksCategory;

			var currentTaskAssignedToFilter = filters.AddNkFilter(CurrentTaskAssignedToFilterName, GetCurrentTaskAssignedToQuery, ModuleIDs.GlbStaff, Staffs);
			currentTaskAssignedToFilter.MaxLength = ViewSalesDashboardActivitySchema.VSA_GS_NKAssignedStaff.MaxLength;
			currentTaskAssignedToFilter.MultilingualDescription = ResString.GetMultilingualString("2b1611c1-bc29-44de-8184-9a2c5601546e", CurrentTaskAssignedToFilterName);
			currentTaskAssignedToFilter.Category = workflowTasksCategory;

			var currentTaskAssignedGroupFilter = filters.AddGuidFilter(FilterDescription.CurrentTaskAssignedGroup, ModuleIDs.GlbGroup, ViewSalesDashboardActivitySchema.VSA_GG_AssignedGroup, Groups);
			currentTaskAssignedGroupFilter.MultilingualDescription = ResString.GetMultilingualString("fbc9673d-ead5-441d-b19a-56d925e066b1", FilterDescription.CurrentTaskAssignedGroup);
			currentTaskAssignedGroupFilter.Category = workflowTasksCategory;

			var currentTaskAssignedGroupNotStaffFilter = filters.AddGuidFilter(FilterDescription.CurrentGroupWithStaffBlank, ModuleIDs.GlbGroup, GetCurrentTaskAssignedGroupNotStaffQuery, Groups);
			currentTaskAssignedGroupNotStaffFilter.MultilingualDescription = ResString.GetMultilingualString("4a287955-d448-43d7-9287-9c1d93bea4bd", FilterDescription.CurrentGroupWithStaffBlank);
			currentTaskAssignedGroupNotStaffFilter.Category = workflowTasksCategory;

			var taskAssignedToFilter = filters.AddNkFilter(FilterDescription.AnyTaskAssignedTo, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ModuleIDs.GlbStaff, Staffs);
			taskAssignedToFilter.SubGroup = workflowTasksFilterSubGroup;
			taskAssignedToFilter.MultilingualDescription = ResString.GetMultilingualString("fdeab3d4-4448-4bf3-9c41-514a8cda829b", FilterDescription.AnyTaskAssignedTo);
			taskAssignedToFilter.Category = workflowTasksCategory;

			var taskAssignedGroupToFilter = filters.AddGuidFilter(FilterDescription.AnyTaskAssignedGroup, ModuleIDs.GlbGroup, ProcessTasksSchema.P9_GG_AssignedGroup, Groups);
			taskAssignedGroupToFilter.SubGroup = workflowTasksFilterSubGroup;
			taskAssignedGroupToFilter.MultilingualDescription = ResString.GetMultilingualString("d764957d-6664-4a99-b78b-2b968fa6f090", FilterDescription.AnyTaskAssignedGroup);
			taskAssignedGroupToFilter.Category = workflowTasksCategory;

			var taskOverdueFilter = filters.AddFlagsFilter(FilterDescription.TaskOverdue, new string[] { Res.GetString("6a174906-bffd-480d-924b-d5d6aaa06c17", "Task Overdue") }, new GetFlagsQuery[] { GetTaskOverdueQuery });
			taskOverdueFilter.SubGroup = workflowTasksFilterSubGroup;
			taskOverdueFilter.MultilingualDescription = ResString.GetMultilingualString("6a174906-bffd-480d-924b-d5d6aaa06c17", FilterDescription.TaskOverdue);
			taskOverdueFilter.Category = workflowTasksCategory;

			var taskStatusFilter = filters.AddTextFilter(FilterDescription.TaskStatus, ViewSalesDashboardActivitySchema.VSA_Status, ProcessTaskStatuses);
			taskStatusFilter.MultilingualDescription = ResString.GetMultilingualString("9700aa14-526a-4f3d-bf4d-9eb8a5bc297a", FilterDescription.TaskStatus);
			taskStatusFilter.Category = workflowTasksCategory;
		}

		ZDBOnlyQuery GetAnyOpenTaskAssignedToQuery(ZString nK)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));

			query.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
			query.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
			query.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTask.LastCompletedStatusCode);
			query.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, WorkflowExceptionsFilterBusinessObject.ActionedCodes.IsActioned);
			query.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, SQLComparisonOperator.Equal, nK);

			return query;
		}

		ZQuery GetCurrentTaskAssignedToQuery(SQLComparisonOperator comparisonOperator, ZString staffCode)
		{
			ZQuery query = new ZQuery(ViewSalesDashboardActivitySchema.VSA_GS_NKAssignedStaff, comparisonOperator, staffCode);
			if (staffCode.IsEmpty)
			{
				query.AddToFilter(comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsNotBlank ? JoinCondition.And : JoinCondition.Or,
					ViewSalesDashboardActivitySchema.VSA_GS_NKAssignedStaff, comparisonOperator, null);
			}

			return query;
		}

		ZQuery GetCurrentTaskAssignedGroupNotStaffQuery(ZGuid pk)
		{
			ZQuery query = new ZQuery(ViewSalesDashboardActivitySchema.VSA_GG_AssignedGroup, pk);
			query.AddToFilter(ViewSalesDashboardActivitySchema.VSA_GS_NKAssignedStaff, ZString.Empty);
			return query;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		ZQuery GetTaskOverdueQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				ZDBOnlyQuery overdueQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				overdueQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, null);
				string overdueFilter = "DATEADD(minute, ISNULL(DATEPART(minute, " + ProcessTasksSchema.P9_EstDuration.Name + "), 0), DATEADD(hour, ISNULL(DATEPART(hour, " + ProcessTasksSchema.P9_EstDuration.Name + "), 0), " + ProcessTasksSchema.P9_ScheduledDate.Name + ")) < @Now";
				overdueQuery.AddFilterAndZSQLParameterCollection(overdueFilter, new ZSqlParameterCollection(ZSqlParameter.New("@Now", ZDateTime.Now.ToDateTime(), ProcessTasksSchema.P9_EstDuration)));
				query.AddToFilter(overdueQuery);
			}
			return query;
		}

		class WorkflowTasksFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
				taskQuery.AddToFilter(filter);
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
				result.AddSubQuery(ViewSalesDashboardActivitySchema.VSA_ParentId, taskQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Filters Override

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var bmHelper = (IBMFilterStripsHelper)ObjectFactory.Get("IBMFilterStripsHelper");

			bmHelper.SetOverriddenSupportedWorkflowTypes(OverriddenSupportedWorkflowTypes);

			bmHelper.SubColumnOverride = ViewSalesDashboardActivitySchema.PK;

			var businessObjectType = typeof(SalesDashboardActivity);

			bmHelper.Initialise(businessObjectType, Factory);
			bmHelper.BusinessObjectTypeOverride = typeof(SalesDashboardActivity);

			helpers.Add(bmHelper);
			return helpers;
		}

		internal readonly List<string> OverriddenSupportedWorkflowTypes = new List<string>()
		{
			WorkflowDescriptors.CampaignWorkflowDescriptorCode,
			WorkflowDescriptors.CommunicationWorkflowDescriptorCode,
			WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode,
			WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode,
			WorkflowDescriptors.OpportunityWorkflowDescriptorCode,
			ProjectWorkflowType,
			WorkflowDescriptors.QuotationWorkflowDescriptorCode
		};
		internal const string ProjectWorkflowType = "WKP";

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string TaskStatus = "Task Status";
			public const string TaskOverdue = "Task Overdue";
			public const string AnyTaskAssignedGroup = "Any Task Assigned Group";
			public const string AnyTaskAssignedTo = "Any Task Assigned To";
			public const string CurrentGroupWithStaffBlank = "Current Group With Staff Blank";
			public const string CurrentTaskAssignedGroup = "Current Task Assigned Group";
			public const string AnyOpenTaskAssignedTo = "Any Open Task Assigned To";
			public const string WorkflowTasks = "Workflow Tasks";
			public const string SalesTeam = "Sales Team";
			public const string OrganizationName = "Organization Name";
			public const string Organization = "Organization";
			public const string ProjectStatus = "Project Status";
			public const string QuotationStatus = "Quotation Status";
			public const string OneOffQuoteStatus = "One Off Quote Status";
			public const string CampaignStatus = "Campaign Status";
			public const string CommunicationStatus = "Communication Status";
			public const string InquiryStatus = "Inquiry Status";
			public const string OpportunityStatus = "Opportunity Status";
			public const string ActivityType = "Activity Type";
			public const string ActivityParentID = "Activity Parent ID";
			public const string ActivityDate = "Activity Date";
			public const string ContactName = "Contact Name";
			public const string ActivityDescription = "Activity Description";
			public const string OpportunityStage = "Opportunity Stage";

			#endregion
		}

		#endregion

		#endregion

		#region Lookups

		OrgOpportunityOverallDispositionList OADlist;
		OrgOpportunityOverallDispositionList OADList
		{
			get { return OADlist ?? (OADlist = new OrgOpportunityOverallDispositionList()); }
		}

		GlbStaffCollection staffs;
		GlbStaffCollection Staffs
		{
			get { return staffs ?? (staffs = new GlbStaffCollection(Factory)); }
		}

		GlbGroupCollection groups;
		GlbGroupCollection Groups
		{
			get { return groups ?? (groups = new GlbGroupCollection(Factory)); }
		}

		SalesTeamCollection salesTeams;
		SalesTeamCollection SalesTeams
		{
			get { return salesTeams ?? (salesTeams = new SalesTeamCollection(Factory)); }
		}

		ICodeDescriptionBoolList opportunityStatuses;
		ICodeDescriptionBoolList OpportunityStatuses
		{
			get { return opportunityStatuses ?? (opportunityStatuses = OrganisationsDataRegistry.Instance.OpportunityStatus.Value); }
		}

		ICodeDescriptionBoolList opportunityStages;
		ICodeDescriptionBoolList OpportunityStages
		{
			get { return opportunityStages ?? (opportunityStages = OrganisationsDataRegistry.Instance.OpportunityStages.Value); }
		}

		ICodeDescriptionPairList inquiryStatuses;
		ICodeDescriptionPairList InquiryStatuses
		{
			get { return inquiryStatuses ?? (inquiryStatuses = new SalesEnquiryStatusCodeList()); }
		}

		ICodeDescriptionPairList communicationStatuses;
		ICodeDescriptionPairList CommunicationStatuses
		{
			get { return communicationStatuses ?? (communicationStatuses = OrganisationsDataRegistry.Instance.CommunicationStatusList.Value); }
		}

		ICodeDescriptionPairList campaignStatuses;
		ICodeDescriptionPairList CampaignStatuses => campaignStatuses ?? (campaignStatuses = new TrackingStatusCodes());

		OneOffQuoteStatusesCodeList oneOffQuoteStatuses;
		OneOffQuoteStatusesCodeList OneOffQuoteStatuses => oneOffQuoteStatuses ?? (oneOffQuoteStatuses = new OneOffQuoteStatusesCodeList());

		QuotationStatusesCodeList quotationStatuses;
		QuotationStatusesCodeList QuotationStatuses => quotationStatuses ?? (quotationStatuses = new QuotationStatusesCodeList());

		ICodeDescriptionPairList processTaskStatuses;
		ICodeDescriptionPairList ProcessTaskStatuses
		{
			get { return processTaskStatuses ?? (processTaskStatuses = new ProcessTaskStatusCodeList()); }
		}

		SalesOrganisationCollection salesOrganisations;
		SalesOrganisationCollection SalesOrganisations
		{
			get { return salesOrganisations ?? (salesOrganisations = new SalesOrganisationCollection(Factory)); }
		}

		#endregion

		readonly SalesDashboardCRMSecurityProvider SecurityProvider = new SalesDashboardCRMSecurityProvider();
	}
}
