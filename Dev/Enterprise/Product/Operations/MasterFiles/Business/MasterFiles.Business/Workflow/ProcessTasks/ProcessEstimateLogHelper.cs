using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class ProcessEstimateLogHelper
	{
		public static void CreateProcessTaskEstimateLog(ProcessTask task)
		{
			if (!ShouldEstimatesBeLogged(task))
			{
				return;
			}

			var processEstimateLog = task.Factory.New<IProcessEstimateLog>();

			processEstimateLog.P9E_ParentId = task.PK;
			processEstimateLog.P9E_ParentTableCode = task.TablePrefix;
			processEstimateLog.P9E_LogDateTime = ZDateTimeOffset.Now;
			processEstimateLog.P9E_GS_NKUser = task.P9_GS_NKAssignedStaffMember;

			if (!task.P9_EstimateVariationFactorInfo.OriginalValue.IsEmpty && !task.P9_EstDurationInfo.OriginalValue.IsEmpty)
			{
				var previousLowEstimate = (task.P9_EstDurationInfo.OriginalValue as ZDateTime?).Value;
				var previousVariationFactor = (task.P9_EstimateVariationFactorInfo.OriginalValue as ZDecimal?).Value;

				processEstimateLog.P9E_PreviousLowEstimateMinutes = TaskDurationCalculator.GetHoursFromDuration(previousLowEstimate).ToMinutes();

				var previousHighEstimate = TaskDurationCalculator.GetDurationWithFactor(previousLowEstimate, previousVariationFactor);
				processEstimateLog.P9E_PreviousHighEstimateMinutes = TaskDurationCalculator.GetHoursFromDuration(previousHighEstimate).ToMinutes();
			}

			processEstimateLog.P9E_NewLowEstimateMinutes = task.LowEstimatedDurationHours.ToMinutes();
			processEstimateLog.P9E_NewHighEstimateMinutes = task.HighEstimatedDurationHours.ToMinutes();

			processEstimateLog.P9E_HasWorkStarted = WasWorkStarted(task);
		}

		public static void CreateProcessHeaderEstimateLog(IProcessHeader workflow)
		{
			if (!WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.Value)
			{
				return;
			}

			var newLowEstimate = workflow.TotalLowEstimate;
			var newHighEstimate = workflow.TotalHighEstimate;

			if (newLowEstimate == 0 && newHighEstimate == 0)
			{
				return;
			}

			var previousLowEstimate = workflow.PreviousTotalLowEstimate;
			var previousHighEstimate = workflow.PreviousTotalHighEstimate;

			if (previousLowEstimate == newLowEstimate && previousHighEstimate == newHighEstimate)
			{
				return;
			}

			var processEstimateLog = workflow.Factory.New<IProcessEstimateLog>();

			processEstimateLog.P9E_ParentId = workflow.PK;
			processEstimateLog.P9E_ParentTableCode = ProcessHeaderSchema.Constants.Prefix;
			processEstimateLog.P9E_LogDateTime = ZDateTimeOffset.Now;
			processEstimateLog.P9E_GS_NKUser = GlbStaff.CurrentUser.GS_Code;

			processEstimateLog.P9E_PreviousLowEstimateMinutes = previousLowEstimate.ToMinutes();
			processEstimateLog.P9E_PreviousHighEstimateMinutes = previousHighEstimate.ToMinutes();

			processEstimateLog.P9E_NewLowEstimateMinutes = newLowEstimate.ToMinutes();
			processEstimateLog.P9E_NewHighEstimateMinutes = newHighEstimate.ToMinutes();

			processEstimateLog.P9E_HasWorkStarted = WasWorkStarted(workflow);

			workflow.PreviousTotalLowEstimate = newLowEstimate;
			workflow.PreviousTotalHighEstimate = newHighEstimate;
		}

		static ZInt ToMinutes(this double hours) => new ZDecimal(hours).ToMinutes();
		static ZInt ToMinutes(this ZDecimal hours) => (ZInt)Utilities.Round(hours * 60, 0);

		static bool ShouldEstimatesBeLogged(ProcessTask task)
		{
			return WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.Value &&
					!task.IsDeleted &&
					!task.IsTemplateTask &&
					task.P9_FH_ProcessHeader.IsValid &&
					(task.P9_EstDurationInfo.HasChanges || task.P9_EstimateVariationFactorInfo.HasChanges || !task.IsInDatabase);
		}

		static bool WasWorkStarted(ProcessTask task)
		{
			return WasWorkStartedCore(task.ProcessHeader, task.WorkflowType);
		}

		static bool WasWorkStarted(IProcessHeader workflow)
		{
			return WasWorkStartedCore(workflow, workflow.FH_WorkflowType);
		}

		static bool WasWorkStartedCore(IProcessHeader workflow, string workflowType)
		{
			var workingOrClosedStatuses = new[] { ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Working, ProcessTaskStatusCodeList.Codes.Suspended };
			var processHeader = workflow;

			if (processHeader == null || processHeader.IsDeleted)
			{
				return false;
			}

			var workingOrClosedTasks = workflow.Tasks
				.Cast<ProcessTask>()
				.Where(processTask => !processTask.P9_ActualDuration.IsEmpty || processTask.P9_Status.ToString().In(workingOrClosedStatuses))
				.ToArray();

			if (!workingOrClosedTasks.Any())
			{
				return false;
			}

			var workflowTaskTypesCategories = WorkflowDataRegistry.Instance.TaskTypes.Value.Cast<CategorisedWorkflowTaskTypes>();
			var taskTypes = workflowTaskTypesCategories.Where(workflowTaskType => workflowTaskType.Code == workflowType);
			var workProductionTaskTypeCodes = taskTypes
				.SelectMany(item => item.TaskTypes)
				.Cast<WorkflowTaskType>()
				.Where(workflowTaskType => workflowTaskType.IsWorkProduction)
				.Select(taskType => taskType.Code);

			return workingOrClosedTasks.Any(tasks => tasks.P9_Type.In(workProductionTaskTypeCodes));
		}
	}
}
