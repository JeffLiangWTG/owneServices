using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class ContainmentBarrierCreator : IContainmentBarrierCreator
	{
		public ContainmentBarrierCreator()
			: this(null)
		{
		}

		public ContainmentBarrierCreator(ILogger logger)
			: this(logger, () => new BusinessObjectFactory { NameForDebugging = nameof(ContainmentBarrierCreator) })
		{
		}

		public ContainmentBarrierCreator(ILogger logger, Func<BusinessObjectFactory> factoryProvider)
		{
			Logger = logger;
			FactoryProvider = factoryProvider;
		}

		const string defaultQcbTaskNewStatus = ProcessTaskStatusCodeList.Codes.Closed;

		public IEnumerable<string> TaskTypesToNotRepeat { get; set; }
		public IEnumerable<string> TaskTypesToSuspendOnRepeatIfQcbTask { get; set; }
		public string IterationType { get; set; }
		public string QcbTaskNewStatus { get; set; } = defaultQcbTaskNewStatus;
		public string QcbCreatingUserLoginName { get; set; } = "";
		public string ResourceUnderReviewNk { get; set; }

		Func<BusinessObjectFactory> FactoryProvider { get; }
		ILogger Logger { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logs are English only.")]
		public void CreateQualityIteration(ZGuid qcbTaskPk, ZGuid iterateFromTaskPk, ZGuid reasonPk, Action<IProcessHeader> adjustTasksAfterCopyTasks = null, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks = null)
		{
			int retries = 3;
			while (retries-- > 0)
			{
				try
				{
					CreateQualityIterationCore(qcbTaskPk, iterateFromTaskPk, reasonPk, adjustTasksAfterCopyTasks, customQualityIterationTasks);
					retries = 0;
				}
				catch (ZSaveException ex)
				{
					if (retries > 0)
					{
						Logger?.Warning("ConcurrencyError on save:" + ex.Message);
					}
					else
					{
						throw;
					}
				}
			}
		}

		void CreateQualityIterationCore(ZGuid qcbTaskPk, ZGuid iterateFromTaskPk, ZGuid reasonPk, Action<IProcessHeader> adjustTasksAfterCopyTasks, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks)
		{
			var factory = FactoryProvider();
			var task = factory.Load<ProcessTask>(qcbTaskPk);
			using (var viewModel = new ContainmentBarrierViewModel(task, ProcessTaskStatusCodeList.Codes.Closed, ResourceUnderReviewNk, deselectCancelledTasksFromIteration: false))
			{
				viewModel.IterationType = IterationType;
				viewModel.TaskTypesToNotRepeat = TaskTypesToNotRepeat;
				viewModel.TaskTypesToSuspendOnRepeatIfQcbTask = TaskTypesToSuspendOnRepeatIfQcbTask;
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = iterateFromTaskPk;
				viewModel.IterateReasonPK = reasonPk;
				viewModel.QcbCreatingUserLoginName = QcbCreatingUserLoginName;

				viewModel.CommitResponse(adjustTasksAfterCopyTasks, customQualityIterationTasks);
			}

			using (task.SupressContainmentBarrierStatusChangeResponder())
			{
				task.P9_Status = QcbTaskNewStatus;
			}

			factory.Save();
		}

		public ZGuid FindBestIterateFromTask(ZGuid qcbTaskPK, ContainmentBarrierIterateFromTaskSelectionMode mode, IEnumerable<string> eligibleTaskTypes)
		{
			var task = FactoryProvider().Load<ProcessTask>(qcbTaskPK);

			if (task.IsQualityContainmentBarrierTask())
			{
				using (var viewModel = new ContainmentBarrierViewModel(task, defaultQcbTaskNewStatus, deselectCancelledTasksFromIteration: true))
				{
					return viewModel.FindBestIterateFromTask(mode, eligibleTaskTypes);
				}
			}
			else
			{
				return ZGuid.Empty;
			}
		}
	}
}
