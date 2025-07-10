using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ConcurrencyResolver;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateApplicationProcessTasksRaceConditionHandler
	{
		public TemplateApplicationProcessTasksRaceConditionHandler(BusinessObjectFactory factory, bool tryHandleConflictsAutomatically, bool tryHandleConflictsWhenServiceTasksOnlySelected)
		{
			Factory = factory;
			TryHandleConflictsAutomatically = tryHandleConflictsAutomatically;
			TryHandleConflictsWhenServiceTasksOnlySelected = tryHandleConflictsWhenServiceTasksOnlySelected;
		}

		BusinessObjectFactory Factory { get; }
		bool TryHandleConflictsAutomatically { get; }
		bool TryHandleConflictsWhenServiceTasksOnlySelected { get; }
		HashSet<ZGuid> suspectedDuplicates;

		static ZGuid TaskTemplate(ProcessTask task) => task.P9_ParentTemplateID;
		static ZGuid TaskParent(ProcessTask task) => task.P9_ParentID;
		static ZGuid LogParent(StmALog log) => log.SL_Parent;

		public void Process(IEnumerable<BusinessObject> businesObjects)
		{
			suspectedDuplicates = null;
			if (ShouldProcess)
			{
				OnBeforeProcess?.Invoke(this, default);
				var conflictCandidates = GetCandidateConflictingRows(businesObjects);
				if (conflictCandidates.tasks.Count > 0 || conflictCandidates.logs.Count > 0)
				{
					var parentIdBatches = conflictCandidates.tasks.Select(TaskParent)
						.Concat(conflictCandidates.logs.Select(LogParent))
						.Distinct()
						.Batch(TemplateApplicationRaceConditionHandlerBase<ProcessTask>.ProcessBatchSize).ToList();

					var taskTemplateLookup = MergeInMemoryDuplicates(conflictCandidates.tasks);

					var logLookup = conflictCandidates.logs.ToLookup(LogParent);

					foreach (var parentIds in parentIdBatches)
					{
						Check(parentIds, taskTemplateLookup, logLookup);
					}
				}

				foreach (var task in conflictCandidates.tasks.Where(t => !t.IsDeleted))
				{
					var job = task.GetJob();
					job.GetLogs().GetAllLogs().Reload(false);
					var (_, actualEvent) = WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(task, job, false, false);
					if (actualEvent != null)
					{
						CreateMilestoneExceptionForDateInTheFuture(task, actualEvent, actualEvent.SL_EventTimeOffset);
					}
				}
			}
		}

		void Check(IEnumerable<ZGuid> parentIds, ILookup<ZGuid, ProcessTask> taskTemplateLookup, ILookup<ZGuid, StmALog> logLookup)
		{
			if (TryLock(parentIds))
			{
				var dbQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				dbQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, parentIds);
				dbQuery.IgnoreDbQueryCache = true;
				var tasksInDb = Factory.Load<ProcessTask>(dbQuery);

				foreach (var taskInDb in tasksInDb)
				{
					if (taskInDb.IsDeleted)
					{
						continue;
					}
					var duplicates = taskTemplateLookup[TaskTemplate(taskInDb)]
						.Where(t => !t.IsDeleted && IsDuplicate(taskInDb, t))
						.ToList();

					if (duplicates.Count > 0)
					{
						foreach (var similarTaskBeingSaved in duplicates)
						{
							if (TryHandleConflictsAutomatically || (TryHandleConflictsWhenServiceTasksOnlySelected && similarTaskBeingSaved.IsCreatedFromTemplateDuringSaving && !similarTaskBeingSaved.IsInDatabase))
							{
								Merge(taskInDb, similarTaskBeingSaved);
							}
							else
							{
								suspectedDuplicates = suspectedDuplicates ?? new HashSet<ZGuid>();
								suspectedDuplicates.Add(similarTaskBeingSaved.PK);
								similarTaskBeingSaved.Validation.ValidateP9_ParentTemplateID();
							}
						}
					}
					else if (taskInDb.IsMilestoneOrWorkflowTrigger)
					{
						if (WorkflowDefaultDateProvider.IsTriggerableValidForDateDefaulting(taskInDb))
						{
							var filter = WorkflowDefaultDateProvider.GetQueryForDefaults(taskInDb, false);
							foreach (var log in logLookup[TaskParent(taskInDb)])
							{
								if (!log.IsDeleted && log.SL_SE_NKEvent.EqualsIgnoringCase(taskInDb.TriggerConditions.TriggerEventCode) && filter(log))
								{
									MaybeSetDefaultDateFrom(taskInDb, log);
								}
							}
						}
					}
				}
			}
		}

		ILookup<ZGuid, ProcessTask> MergeInMemoryDuplicates(ICollection<ProcessTask> taskCollection)
		{
			//This appears to be affecting a client 0.0002% of the time, no idea how this can happen, possibly applying to the same row as different types but this seems to be handled correctly
			var taskTemplateLookup = taskCollection.ToLookup(TaskTemplate);
			var hasDuplicates = false;
			foreach (var tasks in taskTemplateLookup)
			{
				var inMemoryTasks = tasks.Where(t => !t.IsInDatabase).ToArray();
				if (inMemoryTasks.Length > 1)
				{
					var firstTask = inMemoryTasks[0];
					for (var i = 1; i < inMemoryTasks.Length; i++)
					{
						var suspectedDuplicate = inMemoryTasks[i];
						if (IsDuplicate(firstTask, suspectedDuplicate))
						{
							hasDuplicates = true;
							Merge(firstTask, suspectedDuplicate);
						}
					}
				}
			}

			return hasDuplicates ? taskCollection.Where(t => !t.IsDeleted).ToLookup(TaskTemplate) : taskTemplateLookup;
		}

		static void MaybeSetDefaultDateFrom(ProcessTask taskInDb, StmALog log)
		{
			if (WorkflowDefaultDateProvider.ShouldUpdateActualDate(taskInDb, log, false))
			{
				var eventTime = log.SL_EventTimeOffset;
				CreateMilestoneExceptionForDateInTheFuture(taskInDb, log, eventTime);
				taskInDb.TrySetActualDateForEvent(log, (BusinessObject)log.Master, eventTime);
			}
		}

		static void CreateMilestoneExceptionForDateInTheFuture(ProcessTask taskInDb, StmALog log, ZDateTimeOffset eventTime)
		{
			if (taskInDb.IsMilestone && WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.Value && eventTime.IsInTheFuture())
			{
				taskInDb.CreateMilestoneException(ProcessWorkflowExceptionType.ExceptionFutureEvent);
			}
		}

		bool TryLock(IEnumerable<ZGuid> parentIDs)
		{
			if (ShouldLock)
			{
				OnBeforeLock?.Invoke(this, default);
				var result = TemplateApplicationTableLocker.Lock(((IDbConnected)Factory).Connection, ProcessTasksSchema.Instance, ProcessTasksSchema.P9_ParentID, parentIDs);
				OnAfterLock?.Invoke(this, default);
				return result;
			}
			else
			{
				return true;
			}
		}

		public bool IsSuspectedDuplicate(BusinessObject businesObject)
		{
			return suspectedDuplicates != null && suspectedDuplicates.Contains(businesObject.PK);
		}

		public event EventHandler OnAfterLock;
		public event EventHandler OnBeforeLock;
		public event EventHandler OnBeforeProcess;

		static bool CanTaskBeCreatedConcurrently(ProcessTask task)
		{
			return !task.IsTemplate &&
					task.P9_ParentID.IsValid &&
					task.P9_ParentTemplateID.IsValid;
		}

		static bool IsDuplicate(ProcessTask original, ProcessTask suspected)
		{
			var hasMatchingCompany = (original.P9_GC == suspected.P9_GC) || (suspected.IsTask && (suspected.P9_ShareTasksForAllCompanies || !suspected.WorkflowDescriptor.AreTasksCompanySpecific));
			return original.PK != suspected.PK && hasMatchingCompany && original.P9_ParentID == suspected.P9_ParentID && original.IsDuplicateItemTemplateApplication(suspected);
		}

		(ICollection<ProcessTask> tasks, ICollection<StmALog> logs) GetCandidateConflictingRows(IEnumerable<BusinessObject> businesObjects)
		{
			var logs = new List<StmALog>();
			var tasks = new List<ProcessTask>();
			foreach (var bizo in businesObjects.Where(b => !b.IsDeleted && !b.IsInDatabase))
			{
				if (bizo is ProcessTask task)
				{
					if (CanTaskBeCreatedConcurrently(task) && !task.CreatedByUniversalCopy)
					{
						var parent = task.ParentBusinessObject;
						if (parent != null && !parent.IsDeleted && parent.IsInDatabase)
						{
							tasks.Add(task);
						}
					}
				}
				else if (bizo is StmALog log)
				{
					var master = log.Master;
					if (master is IWorkflowProvider && !master.IsDeleted && master.IsInDatabase)
					{
						logs.Add(log);
					}
				}
			}
			return (tasks, logs);
		}

		public bool ShouldProcess => WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.Value != TemplateApplicationRaceConditionHandlerOptions.Codes.Off;
		bool ShouldLock => WorkflowDataRegistry.Instance.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock.Value;

		static bool Merge(ProcessTask original, ProcessTask duplicate)
		{
			if (duplicate.IsException)
			{
				throw new InvalidOperationException("No support for merging exceptions. How did this happen?");
			}

			if (duplicate.IsMilestoneOrWorkflowTrigger)
			{
				if (duplicate.P9_ActualDate != original.P9_ActualDate || duplicate.P9_ScheduledDate != original.P9_ScheduledDate)
				{
					var resolutionResult = new ProcessTaskDeleteAndUpdateResolver(
						delegate(ProcessTask task, ProcessTask processTask)
						{
							WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(task, processTask.GetJob(),
								false, false);
						}).DeleteDuplicateAndUpdateOriginalWhenRequired(original, duplicate);
					return resolutionResult.Resolved;
				}

				duplicate.Delete();
				return true;
			}

			if (!duplicate.IsTask)
			{
				return false;
			}

			duplicate.Delete();

			return true;
		}
	}
}
