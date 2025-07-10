using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowWithExtraTasksFilterStripsHelper : WorkflowFilterStripsHelper
	{
		public WorkflowWithExtraTasksFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			base.AddFilterStrips(filters);
			AddExtraTaskFilters(filters);
		}

		#region Extra Task Filters

		protected void AddExtraTaskFilters(ModuleFilterCollection filters)
		{
			var currentTaskOnlyMultilingualString = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|CurrentTaskOnly", "Startable Task Only");
			var currentTaskOnlyFilter = filters.AddFlagsFilter(FilterDescription.CurrentTaskOnly, new string[] { currentTaskOnlyMultilingualString }, new GetFlagsQuery[] { GetOpenTasksFilter });
			currentTaskOnlyFilter.MultilingualDescription = currentTaskOnlyMultilingualString;

			var assignedToFilter = filters.AddNkFilter(FilterDescription.TaskAssignedTo, GetTaskNKFilter(currentTaskOnlyFilter, nk => new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, nk)), ModuleIDs.GlbStaff, GetStaffList(Factory));
			assignedToFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|AssignedTo", FilterDescription.TaskAssignedTo);

			var assignedGroupFilter = filters.AddGuidFilter(FilterDescription.TaskAssignedGroup, ModuleIDs.GlbGroup, GetTaskGuidQuery(currentTaskOnlyFilter, pk => new ZQuery(ProcessTasksSchema.P9_GG_AssignedGroup, pk)), new GlbGroupCollection(Factory));
			assignedGroupFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|AssignedGroup", FilterDescription.TaskAssignedGroup);

			var taskTypeFilter = filters.AddTextFilter(FilterDescription.TaskType, GetTaskTextQuery(currentTaskOnlyFilter, (op, text) => new ZQuery(ProcessTasksSchema.P9_Type, op, text)), GetTaskTypeList());
			taskTypeFilter.MaxLength = ProcessTasksSchema.P9_Type.MaxLength;
			taskTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|TaskType", FilterDescription.TaskType);

			var taskStatusList = new CodeDescriptionPairList(new ProcessTasksLookups(Factory).Statuses);
			taskStatusList.AddPair("NCM", ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|NotComplete", FilterDescription.NotComplete));
			var taskStatusFilter = filters.AddTextFilter(FilterDescription.TaskStatus, GetTaskTextQuery(currentTaskOnlyFilter, GetTaskStatusQuery), taskStatusList);
			taskStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|TaskStatus", FilterDescription.TaskStatus);

			var taskNumberFilter = filters.AddGuidFilter(FilterDescription.TaskNumber, ModuleIDs.ProcessTasks, GetTaskGuidQuery(currentTaskOnlyFilter, pk => new ZQuery(ProcessTasksSchema.PK, pk)), new ProcessTaskCollection(Factory));
			taskNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|TaskNumber", FilterDescription.TaskNumber);

			foreach (var filter in new ModuleFilter[] { assignedToFilter, assignedGroupFilter, taskTypeFilter, taskStatusFilter, currentTaskOnlyFilter, taskNumberFilter })
			{
				filter.Category = TasksCategory;
			}
		}

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string TaskNumber = "Task #";
			public const string TaskStatus = "Task Status";
			public const string NotComplete = "Not Complete";
			public const string TaskType = "Task Type";
			public const string TaskAssignedGroup = "Task Assigned Group";
			public const string TaskAssignedTo = "Task Assigned To";
			public const string CurrentTaskOnly = "Current Task Only";

			#endregion
		}

		#endregion

		ZQuery GetOpenTasksFilter(ZBool value)
		{
			if (value)
			{
				return GetTaskJoin(false, GetTaskStatusQuery("NCM"));
			}
			else
			{
				return new ZQuery();
			}
		}

		#region Extra Task Implementation

		ZQuery GetTaskStatusQuery(ZString status)
		{
			ZQuery query = new ZQuery();
			if (status == "NCM") // Not Complete
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, status);
			}
			return query;
		}

		CodeDescriptionPairList GetTaskTypeList()
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription pair in WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(TemplateCode))
			{
				result.AddPair(pair.Code, pair.Description);
			}

			return result;
		}

		GetNkQuery GetTaskNKFilter(ModuleFlagsFilter currentTaskFilter, GetNkQuery getSubFilter)
		{
			return nk => GetTaskJoin(currentTaskFilter.Property0, getSubFilter(nk));
		}

		GetGuidQuery GetTaskGuidQuery(ModuleFlagsFilter currentTaskFilter, GetGuidQuery getSubFilter)
		{
			return pk => GetTaskJoin(currentTaskFilter.Property0, getSubFilter(pk));
		}

		GetTextQueryWithOperator GetTaskTextQuery(ModuleFlagsFilter currentTaskFilter, GetTextQueryWithOperator getSubFilter)
		{
			return (op, text) => GetTaskJoin(currentTaskFilter.Property0, getSubFilter(op, text));
		}

		GetTextQuery GetTaskTextQuery(ModuleFlagsFilter currentTaskFilter, GetTextQuery getSubFilter)
		{
			return (text) => GetTaskJoin(currentTaskFilter.Property0, getSubFilter(text));
		}

		ZDBOnlyQuery GetTaskJoin(bool currentTasksOnly, ZQuery subFilter)
		{
			var result = new ZDBOnlyQuery(BusinessObjectType);
			var taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			taskQuery.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, new[] {
						Core.Constants.Workflow.MilestoneType,
						Core.Constants.Workflow.ExceptionType,
						Core.Constants.Workflow.WorkflowTriggerType });
			taskQuery.AddToFilter(subFilter);

			if (currentTasksOnly)
			{
				taskQuery.AddToFilter(ProcessTaskFilterBusinessObject.GetCurrentTaskOnlyQuery(currentTasksOnly));
			}

			result.AddSubQuery(taskQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion
	}
}
