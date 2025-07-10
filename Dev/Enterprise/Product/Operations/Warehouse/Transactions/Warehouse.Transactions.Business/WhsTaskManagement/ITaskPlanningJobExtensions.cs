using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ITaskPlanningJobExtensions
	{
		public static bool IsUnplanned(this ITaskPlanningJob job) => job.TaskPlanningStatus.IsEmpty
			|| job.TaskPlanningStatus == TaskPlanningStatus.Codes.NotReady
			|| job.TaskPlanningStatus == TaskPlanningStatus.Codes.Error;

		public static bool IsTaskManagementEnabled(this ITaskPlanningJob job)
		{
			var result = job.WarehousePK.IsValid;
			if (result)
			{
				var warehouse = job.Factory.Load<WhsWarehouse>(job.WarehousePK);
				result = warehouse != null && warehouse.WW_GG_ReleaseGroup.IsValid;
			}

			return result;
		}

		public static IProcessTask[] GetRelatedProcessTasksOffJob(this ITaskPlanningJob job)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, job.PK);
			query.AddToFilter(ProcessTasksSchema.P9_FormFlowType, SQLComparisonOperator.IsNotBlank, ZString.Empty);
			return job.Factory.Load<ProcessTask>(query);
		}

		public static string GetCannotUpdateTaskPlanningStatusReason(this ITaskPlanningJob job, bool changeStatusToReady)
		{
			var reason = job.SpecialCannotUpdateTaskPlanningStatusReason;

			if (string.IsNullOrEmpty(reason))
			{
				if (!job.IsInDatabase || job.HasChanges)
				{
					reason = Res.GetString("4feb721e-9ba9-48a0-b063-262cb503cef9", "Cannot change Task Planning Status as the {0} is not saved.", job.HumanReadableNameWithoutID);
				}
				else if (job.IsFinalisedOrCancelled)
				{
					reason = Res.GetString("de5fed66-a05d-4fce-8a57-ead8a299b392", "Cannot change Task Planning Status as the {0} is finalized or canceled.", job.HumanReadableNameWithoutID);
				}
				else if (!changeStatusToReady)
				{
					var tasks = job is ITaskPlanningJobWithExternalTasks jobWithExternalTasks ? jobWithExternalTasks.GetRelatedProcessTasks() : job.GetRelatedProcessTasksOffJob();
					reason = GetCannotChangeStatusToNotReadyMessage(tasks);
				}
			}

			return reason;
		}

		static string GetCannotChangeStatusToNotReadyMessage(IProcessTask[] processTasks)
		{
			var reason = string.Empty;

			foreach (var task in processTasks.Cast<ProcessTask>())
			{
				if (task.P9_CompletedTime.IsValid)
				{
					reason = Res.GetString("9663ab27-493b-4e53-bbc8-38f49a6b0bcc", "Cannot change Task Planning Status to Not Ready for job with completed task(s).");
					break;
				}
				else if (string.IsNullOrEmpty(reason))
				{
					if (task.P9_ActualDateUtc.IsValid && task.P9_ActualDateUtc.AddHours(24) > ZDateTime.UtcNow
						|| task.P9_SuspendedAtUtc.IsValid && task.P9_SuspendedAtUtc.AddHours(24) > ZDateTime.UtcNow)
					{
						reason = Res.GetString("af59b357-ec57-474f-b207-bb541acab76d", "Cannot change Task Planning Status to Not Ready for job with task(s) started/suspended within the last 24 hours.");
					}
				}
			}

			return reason;
		}
	}
}
