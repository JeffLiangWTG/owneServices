using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public static class TaskManagementHelper
	{
		public static bool PlannedReceiveCanSetUnloadCompleteTime(WhsReceive receive, BusinessObjectFactory factory, GlbStaff staff)
		{
			return !receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned) || ReceiveHasNoActiveWorkingTasks();

			bool ReceiveHasNoActiveWorkingTasks()
			{
				var tasksQuery = GetJobTasksQuery(receive, WarehouseTaskFormFlowTypes.UnloadJob);
				tasksQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
				tasksQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, SQLComparisonOperator.NotEqual, staff.GS_Code);

				var workingUnloadJobTaskForReceive = factory.LoadTop1<ProcessTask>(tasksQuery);
				return workingUnloadJobTaskForReceive is null;
			}
		}

		public static ZQuery GetJobTasksQuery(WhsDocket docket, string taskFormFlowType, GlbStaff staff = null)
		{
			var jobTaskQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, docket.PK);
			jobTaskQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, taskFormFlowType);
			if (staff != null)
			{
				jobTaskQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code);
			}
			return jobTaskQuery;
		}

		public static void CloseRelatedTasks(BusinessObjectFactory factory, Guid docketPK, string taskFormFlowType)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, docketPK);
			query.AddToFilter(ProcessTasksSchema.P9_FormFlowType, taskFormFlowType);

			var statusToFind = new[] { ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned };
			query.AddToFilter(ProcessTasksSchema.P9_Status, statusToFind);

			var tasks = factory.Load<ProcessTask>(query);
			foreach (var task in tasks)
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				}
			}
		}
	}
}
