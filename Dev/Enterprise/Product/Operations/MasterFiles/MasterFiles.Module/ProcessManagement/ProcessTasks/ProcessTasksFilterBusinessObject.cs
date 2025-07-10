using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ProcessTaskFilterBusinessObject : WorkflowFilterBusinessObjectBase, IRelatedModuleFilterBusinessObject
	{
		public ProcessTaskFilterBusinessObject()
		{
			QueryObjectType = typeof(ProcessTask);
		}

		#region Filters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Task ID", ProcessTasksSchema.P9_TaskID, "T")
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|TaskID", "Task ID")
			};
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			processHeaderSubGroup = new ProcessHeaderSubGroup();

			var result = new ModuleFilterCollection();

			AddRelatedItemFilters(result);
			AddTextFilters(result);
			AddFlagsFilters(result);
			AddDateFilters(result);

			return result;
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return new[] { typeof(WorkflowFilterStripsHelper), typeof(IBMFilterStripsHelper) }; }
		}

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var tagLinkSubGroup = new TagLinkSubGroup();

			ModuleNkFilter staffFilter = filters.AddNkFilter("Staff", ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ModuleIDs.GlbStaff, StaffListGetter);
			staffFilter.Category = FilterCategories.Organisations;
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Staff", "Staff");

			var tasksStaffCanDoFilter = new TasksStaffCanDoModuleFilter("TasksStaffCanDo", StaffListGetter);
			tasksStaffCanDoFilter.Category = FilterCategories.Organisations;
			tasksStaffCanDoFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|TasksStaffCanDo", "Tasks Staff can do");
			filters.AddCustomFilter(tasksStaffCanDoFilter);

			ModuleGuidFilter groupFilter = filters.AddGuidFilter("Group", ModuleIDs.GlbGroup, ProcessTasksSchema.P9_GG_AssignedGroup, GroupListGetter);
			groupFilter.Category = FilterCategories.Organisations;
			groupFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Group", "Group");

			ModuleGuidFilter clientFilter = filters.AddGuidFilter("Client", ModuleIDs.Organisation, GetClientFilter, OrganisationList);
			clientFilter.Category = FilterCategories.Organisations;
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Client", "Client");

			var currentComponentFilter = filters.AddGuidFilter("Current Component", ModuleIDs.BMComponent, ProcessHeaderSchema.FH_FC_CurrentComponent, ComponentListGetter);
			currentComponentFilter.Category = FilterCategories.Other;
			currentComponentFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|CurrentComponent", "Current Component");
			currentComponentFilter.SubGroup = processHeaderSubGroup;

			var dedicatedBufferFilter = filters.AddGuidFilter("Dedicated Buffer", ModuleIDs.BMComponent, ProcessHeaderSchema.FH_FC_DedicatedBuffer, ComponentListGetter);
			dedicatedBufferFilter.Category = FilterCategories.Other;
			dedicatedBufferFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|DedicatedBuffer", "Dedicated Buffer");
			dedicatedBufferFilter.SubGroup = processHeaderSubGroup;

			var capabilityFilter = filters.AddGuidFilter("Capability", ModuleIDs.GlbCapability, ProcessTasksSchema.P9_G4_RequiredCapability, CapabilityListGetter);
			capabilityFilter.Category = FilterCategories.Other;
			capabilityFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Capability", "Capability");

			var tagDefinitionFilter = filters.AddGuidFilter("Tag Definition Code", ModuleIDs.BMTagDefinition, TagMagnitudeSchema.TGM_TGD_Tag, ObjectFactory.Get<ITagDefinitionCollection>("ITagDefinitionCollection", Factory));
			tagDefinitionFilter.Category = FilterCategories.Other;
			tagDefinitionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskFilterBusinessObject|TagDefinitionCode", "Tag Group Code");
			tagDefinitionFilter.SubGroup = new TagDefinitionSubGroup();

			var tagMagnitudeFilter = filters.AddGuidFilter("Tag Magnitude", ModuleIDs.BMTagMagnitude, TagLinkSchema.TGL_TGM_Magnitude, ObjectFactory.Get<ITagMagnitudeCollection>("ITagMagnitudeCollection", Factory));
			tagMagnitudeFilter.Category = FilterCategories.Other;
			tagMagnitudeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskFilterBusinessObject|TagMagnitude", "Tag");
			tagMagnitudeFilter.SubGroup = tagLinkSubGroup;

			filters.AddFilter(new ParentJobModuleFilter("Parent Job", ProcessTasksSchema.P9_ParentID, Factory));

			if (ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				filters.AddFilter(new WorkflowJobModuleFilter("Workflow", ModuleIDs.ProcessHeader, ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetCollection(Factory)));
			}
		}

		ProcessHeaderSubGroup processHeaderSubGroup;

		class ProcessHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ProcessTask));
				var subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
		}

		ZDBOnlyQuery GetClientFilter(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.DefaultJoinCondition = JoinCondition.And;

			ZDBOnlySubQuery contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), ProcessTasksSchema.P9_OC);
			contactSubQuery.AddToFilter(OrgContactSchema.OC_OH, value);

			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), ProcessTasksSchema.P9_OA);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

			query.AddSubQuery(contactSubQuery, JoinCondition.Or);
			query.AddSubQuery(addressSubQuery, JoinCondition.Or);

			return query;
		}

		class TagLinkSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessTask));

				var linkSubQuery = new ZDBOnlySubQuery(typeof(ITagLink), TagLinkSchema.TGL_ParentId);

				linkSubQuery.AddToFilter(filter);

				query.AddSubQuery(linkSubQuery, JoinCondition.And);

				return query;
			}
		}

		class TagDefinitionSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessTask));

				var linkSubQuery = new ZDBOnlySubQuery(typeof(ITagLink), TagLinkSchema.TGL_ParentId);

				var magnitudeSubQuery = new ZDBOnlySubQuery(typeof(ITagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);
				magnitudeSubQuery.AddToFilter(filter);

				linkSubQuery.AddSubQuery(magnitudeSubQuery, JoinCondition.And);

				query.AddSubQuery(linkSubQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", ProcessTasksSchema.P9_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Description", "Description");
			filters.AddFilter(new WorkflowTypeFilter(Factory, typeof(ProcessTask), ProcessTasksSchema.P9_ParentID));
			var filter = filters.AddTextFilter("Completion Statement", ProcessHeaderSchema.FH_CompletionStatement);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Completion Statement", "Completion Statement");
			filter.SubGroup = processHeaderSubGroup;
			filter = filters.AddTextFilter("Task Type", GetTaskTypeQuery, GetTaskTypeList);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|TaskType", "Task Type");
			filter.MaxLength = ProcessTasksSchema.P9_Type.MaxLength;
			filters.AddTextFilter("Task Completion Event", ProcessTasksSchema.P9_SE_NKTaskCompletionEvent).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|TaskCompletionEvent", "Task Completion Event");
			filters.AddFilter(new ProcessTaskTemplatedItemTextFilter(this));
		}

		public ICodeDescriptionPairList GetTaskTypeList()
		{
			var taskTypeList = new CodeDescriptionPairList();
			var currentWorkflowTypes = GetCurrentWorkFlowTypes().Distinct().ToList();

			if (currentWorkflowTypes.Count > 0)
			{
				foreach (var currentWorkflowType in currentWorkflowTypes)
				{
					foreach (ICodeDescription pair in WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(currentWorkflowType))
					{
						taskTypeList.AddPair(pair.Code, pair.Description);
					}
				}

				AddAdditionalTextFilters(currentWorkflowTypes, taskTypeList);
			}
			else
			{
				foreach (CategorisedWorkflowTaskTypes workflowType in WorkflowDataRegistry.Instance.TaskTypes.Value)
				{
					foreach (ICodeDescription taskType in workflowType.TaskTypes)
					{
						taskTypeList.AddPairIfNotExist(taskType.Code, taskType.Description);
					}
				}
			}

			taskTypeList.Sort();

			return taskTypeList;
		}

		protected virtual void AddAdditionalTextFilters(List<ZString> currentWorkflowTypes, CodeDescriptionPairList taskTypeList)
		{
		}

		bool moduleFiltersCreated;
		protected IEnumerable<ZString> GetCurrentWorkFlowTypes()
		{
			if (moduleFiltersCreated)
			{
				var workflowTypeRegExp = new Regex(@"^" + "Workflow Type" + @"(\s\([0-9]\))?", RegexOptions.IgnoreCase);
				var workItemTypeRegExp = new Regex(@"^" + "Work Item Type" + @"(\s\([0-9]\))?", RegexOptions.IgnoreCase);

				foreach (var filter in ActiveModuleFilters)
				{
					if (filter is ModuleTextFilter && workflowTypeRegExp.IsMatch(filter.Description))
					{
						yield return ((ModuleTextFilter)filter).Property;
					}
					else if (workItemTypeRegExp.IsMatch(filter.Description))
					{
						yield return JobInvoicingConsumerTypes.WorkItem.Code;
					}
				}
			}
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

		protected virtual ZQuery GetTaskTypeQuery(SQLComparisonOperator comparisonOperator, ZString type)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_Type, comparisonOperator, type.SubstringSafe(0, ProcessTasksSchema.P9_Type.MaxLength));

			return query;
		}

		#endregion

		#region Status and Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("Overdue", new string[] { Res.GetString("MasterFiles|ProcessTasksFilter|OverdueOnly", "Overdue Only") }, new GetFlagsQuery[] { GetOverdueFilter })
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Overdue", "Overdue");

			ModuleFilter filter = filters.AddTextFilter("Status", GetStatusQuery, Statuses);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|Status", "Status");

			filter = filters.AddFlagsFilter("Current Task Only", new string[] { Res.GetString("MasterFiles|ProcessTasksFilter|StartableTaskOnly", "Startable Task Only") }, new GetFlagsQuery[] { GetCurrentTaskOnlyQuery });
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|CurrentTaskOnly", "Startable Task Only");
		}

		ZQuery GetOverdueFilter(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				var now = ZDateTime.UtcNow;
				var fuzzyNow = now.AddHours(26); // Filter by this first, because then we can use the index on P9_ScheduledDateUtc and the query can be faster.
				ZDBOnlyQuery overdueQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				overdueQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, null);
				overdueQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThan, fuzzyNow);
				string overdueFilter = (NoResString)"DATEADD(minute, ISNULL(DATEPART(minute, " + ProcessTasksSchema.P9_EstDuration.Name + (NoResString)"), 0), DATEADD(hour, ISNULL(DATEPART(hour, " + ProcessTasksSchema.P9_EstDuration.Name + (NoResString)"), 0), " + ProcessTasksSchema.P9_ScheduledDateUtc.Name + (NoResString)")) < @UtcNow";
				overdueQuery.AddFilterAndZSQLParameterCollection(overdueFilter, new ZSqlParameterCollection(ZSqlParameter.New("@UtcNow", now.ToDateTime(), ProcessTasksSchema.P9_EstDuration)));
				query.AddToFilter(overdueQuery);
			}
			return query;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			return GetStatusQuery(value, ProcessTasksSchema.P9_Status);
		}

		public static ZQuery GetStatusQuery(ZString value, SchemaColumn schemaColumn)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				if (value == NotCompleteStatus)
				{
					query.AddToFilter(schemaColumn, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
					query.AddToFilter(JoinCondition.And, schemaColumn, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
					query.AddToFilter(JoinCondition.And, schemaColumn, SQLComparisonOperator.NotEqual, "");
				}
				else if (value == AssignedAndSuspendedStatus)
				{
					query.AddToFilter(schemaColumn, SQLComparisonOperator.Equal, ProcessTaskStatusCodeList.Codes.Assigned);
					query.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, ProcessTaskStatusCodeList.Codes.Suspended);
				}
				else if (value == WorkingAndSuspendedStatus)
				{
					query.AddToFilter(schemaColumn, SQLComparisonOperator.Equal, ProcessTaskStatusCodeList.Codes.Working);
					query.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, ProcessTaskStatusCodeList.Codes.Suspended);
				}
				else
				{
					query.AddToFilter(schemaColumn, value);
				}
			}

			return query;
		}

		internal static ZDBOnlyQuery GetCurrentTaskOnlyQuery(ZBool currentTaskOnly)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var p9_PK = ProcessTasksSchema.Constants.PK;

			if (currentTaskOnly)
			{
				var registry = ObjectFactory.Get<IBMSRegistry>();
				var functionHelper = ObjectFactory.Get<IBMSQLFunctionHelper>();
				var dbFunctionToRun = registry.BufferManagementEnabled ? functionHelper.GetCurrentTasks + "()" : "dbo.GetCurrentTasksNotInWorkflows()";
				var sql = string.Format(CultureInfo.InvariantCulture, p9_PK + " IN (SELECT " + p9_PK + " FROM " + dbFunctionToRun + ")");
				query.AddFilterAndZSQLParameterCollection(sql, null);
			}
			return query;
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Actual Start", ProcessTasksSchema.P9_ActualDate, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_ActualDate", "Actual Start Time");
			filters.AddDateFilter("Actual Start Time (Local)", ProcessTasksSchema.P9_ActualDateUtc, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_ActualDateLocalForBinding", "Actual Start Time (Local)");
			filters.AddDateFilter("Actual Start Time (UTC)", ProcessTasksSchema.P9_ActualDateUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_ActualDateUtc", "Actual Start Time (UTC)");

			filters.AddDateFilter("Completed (Local)", ProcessTasksSchema.P9_CompletedTimeUtc, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|CompletedTimeLocal", "Completed Time (Local)");
			filters.AddDateFilter("Completed (UTC)", ProcessTasksSchema.P9_CompletedTimeUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_CompletedTimeUtc", "Completed Time (UTC)");

			filters.AddDateFilter("Scheduled Start", ProcessTasksSchema.P9_ScheduledDate, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|ScheduledDate", "Scheduled Start Time");
			filters.AddDateFilter("Scheduled Start Time (Local)", ProcessTasksSchema.P9_ScheduledDateUtc, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_ScheduledDateLocalForBinding", "Scheduled Start Time (Local)");
			filters.AddDateFilter("Scheduled Start Time (UTC)", ProcessTasksSchema.P9_ScheduledDateUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|P9_ScheduledDateUtc", "Scheduled Start Time (UTC)");

			if (ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				filters.AddDateFilter("Earliest Start Date", GetProcessHeaderRelatedDateQuery(ProcessHeaderSchema.FH_DoNotStartBeforeDate)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|DoNotStartBeforeUtc", "Workflow Earliest Start Date");
				filters.AddDateFilter("Agreed Delivery Date", GetProcessHeaderRelatedDateQuery(ProcessHeaderSchema.FH_AgreedDeliveryDate)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|AgreedDeliveryDateUtc", "Workflow Agreed Delivery Date");
				filters.AddDateFilter("Last Transfer Date", GetProcessHeaderRelatedDateQuery(ProcessHeaderSchema.FH_ReleaseDateTime)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|LastTransferDateUtc", "Workflow Last Transfer Date");
			}
		}

		GetDateQuery GetProcessHeaderRelatedDateQuery(SchemaDateTimeColumn column)
		{
			return (comparisonOperator, dateTime1, dateTime2) =>
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
					AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, column, dateTime1.Date, dateTime2, convertFromLocalToUtc: true);
					query.AddSubQuery(subQuery, JoinCondition.And);
					return query;
				};
		}

		#endregion

		#region Overall

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				if (this.GetContexts<BufferManagementBusinessContext>().FirstOrDefault() != BufferManagementBusinessContext.IgnoreDefaultFilters)
				{
					ProcessTaskFiltersHelper.AddNotMilestoneTriggerExceptionClause(result);
				}
				return result;
			}
		}

		internal bool ShouldAddNonTemplateFilter
		{
			get => shouldAddNonTemplateFilter && QueryObjectType != typeof(TemplateProcessTask);
			set => shouldAddNonTemplateFilter = value;
		}
		bool shouldAddNonTemplateFilter = true;

		#endregion

		#endregion

		#region Lookups

		#region Staff

		GetList StaffListGetter => () => new GlbStaffCollection(Factory);

		#endregion

		#region Groups

		GetList GroupListGetter => () => new GlbGroupCollection(Factory);

		#endregion

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get
			{
				if (fStatuses == null)
				{
					fStatuses = new CodeDescriptionPairList(new ProcessTasksLookups(Factory).Statuses);
					fStatuses.AddPair(NotCompleteStatus, Res.GetString("MasterFiles|ProcessTasksFilter|NotComplete", "Not Complete"));
					fStatuses.AddPair(AssignedAndSuspendedStatus, Res.GetString("MasterFiles|ProcessTasksFilter|AssignedSuspended", "Assigned + Suspended"));
					fStatuses.AddPair(WorkingAndSuspendedStatus, Res.GetString("MasterFiles|ProcessTasksFilter|WorkingSuspended", "Working + Suspended"));
				}
				return fStatuses;
			}
		}

		CodeDescriptionPairList fStatuses;

		const string NotCompleteStatus = "NCM";
		const string AssignedAndSuspendedStatus = "A+S";
		const string WorkingAndSuspendedStatus = "W+S";

		#endregion

		GetList CapabilityListGetter => () => new GlbCapabilityCollection(Factory);

		GetList ComponentListGetter => () => ObjectFactory.Get<IBMComponentCollection>("IBMComponentCollection", Factory);

		#region Organisations

		OrgHeaderCollection OrganisationList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#endregion
	}
}
