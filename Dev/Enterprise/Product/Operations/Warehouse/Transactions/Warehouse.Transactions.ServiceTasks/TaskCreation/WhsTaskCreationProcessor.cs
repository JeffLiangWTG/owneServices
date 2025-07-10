using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class WhsTaskCreationProcessor : IWhsTaskCreationProcessor
	{
		public WhsTaskCreationProcessor(
			IReadyForPlanningJobLoader jobLoader,
			ITaskCreationJobStrategyFactory jobStrategyFactory,
			IWhsTaskFactory taskFactory)
		{
			JobLoader = Argument.NotNull(jobLoader, nameof(jobLoader));
			JobStrategyFactory = Argument.NotNull(jobStrategyFactory, nameof(jobStrategyFactory));
			TaskFactory = Argument.NotNull(taskFactory, nameof(taskFactory));
		}

		IReadyForPlanningJobLoader JobLoader { get; }
		ITaskCreationJobStrategyFactory JobStrategyFactory { get; }
		IWhsTaskFactory TaskFactory { get; }

		public void ProcessQueue(INotifications notifications, CancellationToken token)
		{
			Argument.NotNull(notifications, nameof(notifications));

			AppLockedItem<WhsReadyForPlanningJobsView> nextJobToProcess = null;
			ITaskCreationJobStrategy strategy = null;

			while (!token.IsCancellationRequested && (nextJobToProcess = GetNextJobToProcessInNewFactory()) is not null)
			{
				using (nextJobToProcess)
				{
					var job = nextJobToProcess.Item;

					try
					{
						strategy = JobStrategyFactory.GetJobStrategy(nextJobToProcess.Item.WRV_JobType) ?? throw new InvalidOperationException($"Unexpected job type '{nextJobToProcess.Item.WRV_JobType}'.");
						ProcessQueueForSingleJob(job, strategy, notifications, token);
					}
					catch (OperationCanceledException operationCanceledException) when (operationCanceledException.CancellationToken == token)
					{
						throw;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("CriticalErrorInWhsTaskCreationProcessor", ex);
						notifications.AddError(string.Format(CultureInfo.InvariantCulture, (NoResString)"Unexpected error occurred: {0}", ex.Message));
						SetJobStatusAndSave(job, strategy, notifications, TaskPlanningStatus.Codes.Error, useNewFactory: true);
					}
				}
			}

			AppLockedItem<WhsReadyForPlanningJobsView> GetNextJobToProcessInNewFactory()
			{
				var newFactory = new BusinessObjectFactory { NameForDebugging = $"{nameof(WhsTaskCreationProcessor)}_{nameof(JobLoader)}", RefreshEnabled = false };
				return JobLoader.GetNextJobToProcess(newFactory);
			}
		}

		void ProcessQueueForSingleJob(
			WhsReadyForPlanningJobsView job,
			ITaskCreationJobStrategy strategy,
			INotifications notifications,
			CancellationToken token)
		{
			var workflowInfo = strategy.GetWorkflowInfo(job);
			notifications.Add(NotificationType.Information, $"Processing Job: {workflowInfo.NameForLog}.");

			var factory = job.Factory;

			using (DisposableEnvironment.ForBranch(workflowInfo.BranchPK.ToGuid()))
			{
				if (ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowInfo.WorkflowProvider, factory))
				{
					var taskMapping = new Dictionary<ZGuid, ProcessTask>();
					var tasksToCreateResult = strategy.GetTasksToCreate(job, workflowInfo, token);

					if (!token.IsCancellationRequested && tasksToCreateResult.Status == ResultStatus.Success)
					{
						var tasksToCreate = tasksToCreateResult.Tasks.ToArray();
						notifications.Add(NotificationType.Information, $"Creating {tasksToCreate.Length} task(s).");

						foreach (var taskToCreate in tasksToCreate)
						{
							var task = TaskFactory.CreateTask(
								workflowInfo.WorkflowProvider,
								taskToCreate.FormflowType,
								taskToCreate.WorkflowName,
								taskToCreate.TaskName,
								taskToCreate.StaffCode,
								taskToCreate.RawNudge,
								taskToCreate.CapabilityCode,
								taskToCreate.ReleaseGroupPk,
								taskToCreate.TaskType);

							taskMapping.Add(taskToCreate.ID, task);
						}

						strategy.LinkTasks(job, taskMapping, tasksToCreateResult.Lines);

						if (SetJobStatusAndSave(job, strategy, notifications, TaskPlanningStatus.Codes.Planned))
						{
							notifications.Add(NotificationType.Information, $"Succesfully processed and saved Job: {workflowInfo.NameForLog}.");
						}
					}
					else if (tasksToCreateResult.Status == ResultStatus.Error)
					{
						notifications.AddError(string.Format(CultureInfo.InvariantCulture, (NoResString)"Error Processing Job: {0}, Message: {1}", workflowInfo.NameForLog, tasksToCreateResult.Notifications));
						SetJobStatusAndSave(job, strategy, notifications, TaskPlanningStatus.Codes.Error);
					}
				}
				else
				{
					notifications.Add(NotificationType.Warning, $"Buffer Management is not enabled for the Workflow Type: {workflowInfo.WorkflowProvider.WorkflowType}, clearing the Task Planning Status for Job: {workflowInfo.NameForLog}.");
					SetJobStatusAndSave(job, strategy, notifications, string.Empty);
				}
			}
		}

		static bool SetJobStatusAndSave(
			WhsReadyForPlanningJobsView job,
			ITaskCreationJobStrategy strategy,
			INotifications notifications,
			string taskPlanningStatus,
			bool useNewFactory = false)
		{
			if (strategy is not null)
			{
				var factory = !useNewFactory ? job.Factory : new BusinessObjectFactory { NameForDebugging = $"{nameof(WhsTaskCreationProcessor)}_{nameof(SetJobStatusAndSave)}", RefreshEnabled = false };
				strategy.SetJobPlanningStatus(factory, job, taskPlanningStatus);
				return SaveFactoryWithErrorHandlingAndLogging(notifications, factory);
			}

			return false;

			static bool SaveFactoryWithErrorHandlingAndLogging(INotifications notifications, BusinessObjectFactory factory)
			{
				var result = false;
				try
				{
					factory.Save();
					result = true;
				}
				catch (ZSaveConcurrencyException)
				{
					notifications.AddError((NoResString)"Concurrency error occurred while saving the results of the processed job.");
				}
				// Other exceptions should be caught at the top level handler, set the planning status to ERR and be sent via ErrorReporter!

				return result;
			}
		}
	}
}
