using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class WorkingTaskProvider : IWorkingTaskProvider<ProcessTaskWrapper>
	{
		public ProcessTaskWrapper GetWorkingTask(ProcessTaskWrapper processTaskWrapper, string staffCode)
		{
			if (string.IsNullOrEmpty(staffCode))
			{
				return null;
			}

			ZQuery query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staffCode);
			query.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
			query.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, processTaskWrapper.ProcessTask.PK);

			var processTask = processTaskWrapper.ProcessTask.Factory.LoadTop1<ProcessTask>(query);

			return processTask == null ? null : processTask.ProcessTaskWrapper;
		}

		public bool CanContinueSwitchingWorkingTask(ProcessTaskWrapper processTaskWrapper, string staffCode, out ProcessTaskWrapper workingTaskWrapper, out ProcessTaskWrapper taskToStartWrapper)
		{
			workingTaskWrapper = GetWorkingTask(processTaskWrapper, staffCode);
			if (workingTaskWrapper == null)
			{
				taskToStartWrapper = null;
				return true;
			}

			var conflictResolver = processTaskWrapper.ProcessTask.Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => new DefaultConflictResolver());
			var result = conflictResolver.GetExistingWorkTaskExists(workingTaskWrapper.ProcessTask);
			taskToStartWrapper = result.AlternativeUpdate == null ? null : ((ProcessTask)result.AlternativeUpdate).ProcessTaskWrapper;
			return !result.CancelUpdate;
		}

		class DefaultConflictResolver : ITaskStatusChangeConflictResolver
		{
			public ITaskStatusChangeConflictResolution GetExistingWorkTaskExists(IProcessTask task) => TaskStatusChangeConflictResolution.Continue();
		}
	}
}
