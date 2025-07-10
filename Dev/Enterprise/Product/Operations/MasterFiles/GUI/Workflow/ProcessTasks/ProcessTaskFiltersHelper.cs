using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class ProcessTaskFiltersHelper<T> where T : IWorkflowProvider
	{
		public ProcessTaskFiltersHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		readonly WorkflowTasksFilterSubGroup tasksSubgroup = new WorkflowTasksFilterSubGroup(CurrentTasksSql);
		readonly WorkflowTaskStatusFilterSubGroup taskStatusSubgroup = new WorkflowTaskStatusFilterSubGroup(CurrentTaskStatusRankedSql);
		readonly FilterCategory workflowTasksCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("8f7e626a-320a-47e5-9c54-622d7cfc752b", "Workflow Tasks"));

		#region Current Task

		#region Current Task Assigned To

		public ModuleNkFilter AddCurrentTaskAssignedToFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddNkFilter(FilterDescription.CurrentTaskAssignedTo, GetCurrentTaskAssignedToQuery, ModuleIDs.GlbStaff, Staff);
			filter.MultilingualDescription = ResString.GetMultilingualString("f9fb3768-c93e-47f3-8604-31a8acdc299b", "Current Task Assigned To");
			filter.SubGroup = tasksSubgroup;
			filter.Category = workflowTasksCategory;
			return filter;
		}

		ZQuery GetCurrentTaskAssignedToQuery(SQLComparisonOperator comparisonOperator, ZString staffCode)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, comparisonOperator, staffCode.SubstringSafe(0, ProcessTasksSchema.P9_GS_NKAssignedStaffMember.MaxLength));
			return query;
		}

		#endregion

		#region Current Task Assigned Group

		public ModuleGuidFilter AddCurrentTaskAssignedGroupFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddGuidFilter(FilterDescription.CurrentTaskAssignedGroup, ModuleIDs.GlbGroup, GetCurrentTaskAssignedGroupQuery, Groups);
			filter.MultilingualDescription = ResString.GetMultilingualString("4EE1F12A-E8A4-4982-AA68-00C0F71BEBDC", "Current Task Assigned Group");
			filter.SubGroup = tasksSubgroup;
			filter.Category = workflowTasksCategory;
			return filter;
		}

		ZQuery GetCurrentTaskAssignedGroupQuery(SQLComparisonOperator comparisonOperator, object groupPk)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_GG_AssignedGroup, comparisonOperator, groupPk);
			return query;
		}

		#endregion

		#region Current Task Status

		public ModuleTextFilter AddCurrentTaskStatusFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddTextFilter(FilterDescription.CurrentTaskStatus, GetCurrentTaskStatusQuery, CurrentTaskStatusList);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);

			filter.MultilingualDescription = ResString.GetMultilingualString("bb1fcb9e-1aa2-4a03-8a8e-864584837145", "Current Task Status");
			filter.SubGroup = taskStatusSubgroup;
			filter.Category = workflowTasksCategory;

			return filter;
		}

		ZQuery GetCurrentTaskStatusQuery(SQLComparisonOperator comparisonOperator, ZString status)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_Status, comparisonOperator, status.SubstringSafe(0, ProcessTasksSchema.P9_Status.MaxLength));

			taskStatusSubgroup.ShouldIncludeEmpty = comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.IsBlank;
			return query;
		}

		#endregion

		#region Current Task Scheduled Start

		public ModuleDateFilter AddCurrentTaskScheduledStartFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddDateFilter(FilterDescription.CurrentTaskScheduledStart, GetCurrentTaskScheduledStartQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("be36eac5-3846-41e2-bd69-06d15fce9cb7", "Current Task Scheduled Start");
			filter.SubGroup = tasksSubgroup;
			filter.Category = workflowTasksCategory;
			return filter;
		}

		ZQuery GetCurrentTaskScheduledStartQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var dateQuery = new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, ProcessTasksSchema.P9_ScheduledDateUtc, value1.ToUniversalBranchTime(), value2.ToUniversalBranchTime(), false, false);
			return dateQuery;
		}

		#endregion

		#region Current Task Actual Start

		public ModuleDateFilter AddCurrentTaskActualStartFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddDateFilter(FilterDescription.CurrentTaskActualStart, GetCurrentTaskActualStartQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("be681c5f-0cd7-4dcf-96b3-0d01ce07b5a7", "Current Task Actual Start");
			filter.SubGroup = tasksSubgroup;
			filter.Category = workflowTasksCategory;
			return filter;
		}

		ZQuery GetCurrentTaskActualStartQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var dateQuery = new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, ProcessTasksSchema.P9_ActualDate, value1, value2, false, false);
			return dateQuery;
		}

		#endregion

		#region Sql

		static string CurrentTasksSql
		{
			get
			{
				return ProcessTasksSchema.Constants.PK + " IN (" +
					"SELECT " + ProcessTasksSchema.Constants.PK +
					" FROM (" +
						"SELECT " + ProcessTasksSchema.Constants.PK + ", Rank() over (PARTITION BY " + ProcessTasksSchema.Constants.P9_ParentID + " ORDER BY " + ProcessTasksSchema.Constants.P9_Sequence + ") as Rank" +
						" FROM " + ProcessTasksSchema.Constants.SqlSchemaName + "." + ProcessTasksSchema.Constants.TableName +
						" WHERE " + ProcessTasksSchema.Constants.P9_Status + " in " + CurrentTaskStatusesSql +
					") t" +
					" WHERE Rank = 1" +
				")";
			}
		}

		static string CurrentTaskStatusesSql
		{
			get { return "(" + string.Join(",", ProcessTask.GetCurrentTaskStatuses().Select(status => "'" + status + "'")) + ")"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is SQL Script")]
		static string CurrentTaskStatusRankedSql
		{
			get
			{
				return ProcessTasksSchema.Constants.PK + " IN (" +
					"SELECT " + ProcessTasksSchema.Constants.PK +
					" FROM (" +
						"SELECT " + ProcessTasksSchema.Constants.PK + ", Rank() over (PARTITION BY " + ProcessTasksSchema.Constants.P9_ParentID + " ORDER BY " +
							"CASE " +
								"WHEN " + ProcessTasksSchema.Constants.P9_Status + " IN ('" + ProcessTaskStatusCodeList.Codes.Assigned + "', '" + ProcessTaskStatusCodeList.Codes.Working + "', '" + ProcessTaskStatusCodeList.Codes.Suspended + "') THEN 1 " +
								"WHEN " + ProcessTasksSchema.Constants.P9_Status + " = '" + ProcessTaskStatusCodeList.Codes.Open + "' THEN 2 " +
								"WHEN " + ProcessTasksSchema.Constants.P9_Status + " = '" + ProcessTaskStatusCodeList.Codes.Closed + "' THEN 3 " +
								"WHEN " + ProcessTasksSchema.Constants.P9_Status + " = '" + ProcessTaskStatusCodeList.Codes.Cancelled + "' THEN 4 " +
								"ELSE 5 " +
							"END, " +
							ProcessTasksSchema.Constants.P9_Sequence + ") as Rank " +
						" FROM " + ProcessTasksSchema.Constants.SqlSchemaName + "." + ProcessTasksSchema.Constants.TableName +
					") t" +
					" WHERE Rank = 1" +
				")";
			}
		}

		#endregion

		#endregion

		internal static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string CurrentTaskAssignedTo = "Current Task Assigned To";
			public const string CurrentTaskAssignedGroup = "Current Task Assigned Group";
			public const string CurrentTaskStatus = "Current Task Status";
			public const string CurrentTaskScheduledStart = "Current Task Scheduled Start";
			public const string CurrentTaskActualStart = "Current Task Actual Start";

			#endregion
		}

		#region Lookups

		CodeDescriptionPairList currentTaskStatusList;
		CodeDescriptionPairList CurrentTaskStatusList
		{
			get
			{
				if (currentTaskStatusList == null)
				{
					currentTaskStatusList = new CodeDescriptionPairList();
					var taskStatusList = new ProcessTaskStatusCodeList();
					foreach (var statusCode in taskStatusList.GetAllCodes())
					{
						currentTaskStatusList.Add(taskStatusList[statusCode]);
					}
				}
				return currentTaskStatusList;
			}
		}

		GlbStaffCollection staff;
		GlbStaffCollection Staff
		{
			get { return staff ?? (staff = new GlbStaffCollection(factory)); }
		}

		GlbGroupCollection groups;
		GlbGroupCollection Groups
		{
			get { return groups ?? (groups = new GlbGroupCollection(factory)); }
		}

		#endregion

		class WorkflowTasksFilterSubGroup : ModuleFilterSubGroup
		{
			readonly string sql;

			public WorkflowTasksFilterSubGroup(string sql)
			{
				this.sql = sql;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
				taskQuery.AddToFilter(filter);
				taskQuery.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(), JoinCondition.And);
				var result = new ZDBOnlyQuery(typeof(T));
				result.AddSubQuery(taskQuery, JoinCondition.And);
				return result;
			}
		}

		class WorkflowTaskStatusFilterSubGroup : WorkflowTasksFilterSubGroup
		{
			public WorkflowTaskStatusFilterSubGroup(string sql) : base(sql)
			{
			}

			bool shouldIncludeEmpty;

			public bool ShouldIncludeEmpty
			{
				get { return shouldIncludeEmpty; }
				set { shouldIncludeEmpty = value; }
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = base.GetSubQuery(filter) as ZDBOnlyQuery;

				if (ShouldIncludeEmpty)
				{
					var taskQuery2 = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID, true);
					result.AddSubQuery(taskQuery2, JoinCondition.Or);
				}

				return result;
			}
		}
	}

	public static class ProcessTaskFiltersHelper
	{
		public static void AddNotMilestoneTriggerExceptionClause(ZQuery query)
		{
			query.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Core.Constants.Workflow.ExceptionType);
			query.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Core.Constants.Workflow.MilestoneType);
			query.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Core.Constants.Workflow.WorkflowTriggerType);
		}
	}
}
