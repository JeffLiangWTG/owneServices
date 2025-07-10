using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.Integration.Workflow.Triggers;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.MasterFiles.Business.Workflow.Triggers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowTriggerEventExtensions
	{
		public static void AddWorkflowTriggerEventLog(this IWorkflowTrigger trigger, BusinessObject job, EventSource triggeringEvent, IStmALog stmALog, Action onWTELogAdded = null)
		{
			if (WorkflowTriggerEventCache.HasWTELog(trigger, triggeringEvent))
			{
				UpdateWorkflowTriggerEventLogInCache(trigger, job, triggeringEvent, stmALog, onWTELogAdded);
				return;
			}
			else if (!WorkflowChainManager.HasWTEEventFromCurrentChain(trigger))
			{
				var updatingExistingWTELog = false;
				if (trigger.IsMilestone())
				{
					updatingExistingWTELog = WorkflowTriggerEventCache.ClearExistingWTELogs(trigger) > 0;
				}

				var triggeringEventData = GetTriggeringEventData(trigger, job, triggeringEvent);

				if (trigger.DelayDurationSeconds > 0)
				{
					WorkflowDelayedTriggerProcessor.QueueScheduleActionForDelayedTrigger(trigger, triggeringEventData);
					onWTELogAdded?.Invoke();
				}
				else
				{
					var log = ((IStmALogParent)trigger).Logs.AddNew(Events.WorkflowTriggerEvent, triggeringEventData.ToReference(), triggeringEvent.EventTime);
					WorkflowTriggerEventCache.TryAddWTELog(trigger, triggeringEvent, log);
					if (!updatingExistingWTELog && trigger.TriggerFiredCountdown > short.MinValue)
					{
						trigger.TriggerFiredCountdown--;
					}
					onWTELogAdded?.Invoke();

					ApplyClientSideTriggerActions(trigger, stmALog, triggeringEventData, false);
				}

				return;
			}

			onWTELogAdded?.Invoke();
		}

		public static bool TryUpdateWorkflowTriggerEventLogInCache(this IWorkflowTrigger trigger, BusinessObject job, EventSource triggeringEvent, IStmALog stmALog, Action onWTELogAdded = null)
		{
			if (WorkflowTriggerEventCache.HasWTELog(trigger, triggeringEvent))
			{
				UpdateWorkflowTriggerEventLogInCache(trigger, job, triggeringEvent, stmALog, onWTELogAdded);
				return true;
			}
			return false;
		}

		public static void UpdateWorkflowTriggerEventLogInCache(this IWorkflowTrigger trigger, BusinessObject job, EventSource triggeringEvent, IStmALog stmALog, Action onWTELogAdded = null)
		{
			WorkflowTriggerEventCache.UpdateWTEEventTime(trigger, triggeringEvent);

			if (triggeringEvent?.Log != null && stmALog != null)
			{
				if ((triggeringEvent.Log is BusinessObject triggeringEventLog && triggeringEventLog.IsDeleted) || (stmALog is BusinessObject eventLog && eventLog.IsDeleted))
				{
					return;
				}

				var triggeringEventData = GetTriggeringEventData(trigger, job, triggeringEvent);
				onWTELogAdded?.Invoke();
				ApplyClientSideTriggerActions(trigger, stmALog, triggeringEventData, true);
			}
		}

		internal static bool IsTriggerOrMilestoneThatCanFire(this ProcessTask triggerable)
		{
			if (!triggerable.IsMilestone() || triggerable.P9_ActualDateUpdateType == ActualDateUpdateTypeCodeList.Codes.ALW)
			{
				return true;
			}
			var proxy = new WorkflowMilestoneProxy(triggerable);
			/*
			 *	The check below comes from a historical rule for milestones.
			 *	If a milestone has not fired, it should be fired by the newest event raised before it's next save.
			 *
			 *	This is so that if a user enters a date which raises an event, then corrects this event before save, the milestone will fire using the corrected value.
			 */
			if (proxy.OriginalActualDate != null && proxy.ActualDate != null)
			{
				return triggerable.P9_ActualDateForBindingInfo.HasChanges || !triggerable.P9_ActualDateForBinding.IsValid;
			}
			else
			{
				return true;
			}
		}

		internal static bool ShouldUpdateActualDateWithoutFiring(this ProcessTask triggerable)
		{
			return triggerable.IsMilestone() && (triggerable.P9_ActualDateUpdateType == ActualDateUpdateTypeCodeList.Codes.UAD || triggerable.P9_ActualDateUpdateType == ActualDateUpdateTypeCodeList.Codes.ALW);
		}

		static void ApplyClientSideTriggerActions(IWorkflowTrigger trigger, IStmALog eventSource, WorkflowTriggerEventData triggeringEventData, bool isRunningAgain)
		{
			var reasonForDoNotTriggerAction = trigger.Job is ITriggerActionProvider provider ? provider.ReasonForDoNotTriggerAction : ZString.Empty;
			if (reasonForDoNotTriggerAction.IsEmpty)
			{
				try
				{
					using (WorkflowTriggerActionTracker.TrackTriggerActions(trigger.Factory))
					{
						var notifications = new NotificationCollection();
						WorkflowDescriptor descriptor = null;
						BusinessObject parent = null;

						foreach (var action in trigger.TriggerActions)
						{
							using (TriggerActionRecursionHandler.WithActionRecursionDetection())
							{
								if (action is ProcessTaskNotification triggerAction && ((ITriggerAction)triggerAction).RunLocation == TriggerRunLocation.Client && (!isRunningAgain || ObjectFactory.Get<ICanTriggerActionRunAgain>().CanRunAgain(triggerAction)))
								{
									if (TriggerActionRecursionHandler.Instance.IsDuplicateAction(triggerAction.PK))
									{
										return;
									}
									else
									{
										descriptor = descriptor ?? trigger.GetWorkflowDescriptor();
										parent = parent ?? trigger.GetParentBusinessObject(triggeringEventData);

										using (RunningClientSideTrigger())
										using (trigger.Factory.IsInSaveTransaction ? new BusinessObjectPropertyChangeTracker(trigger, parent.Factory) : null)
										{
											IProcessor processor = descriptor.GetWorkflowTriggerAction(new WorkflowTriggerActionSource(parent, trigger, triggerAction, triggeringEventData, Lazy.Create(() => eventSource)), null);
											processor.Process(notifications, new System.Threading.CancellationToken());
										}
									}
								}
							}
						}
						WorkflowTriggerActionTracker.OnAllActionsRun(trigger.Factory);

						trigger.UpdateTriggerActionNotifications(notifications);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException()
					&& ex.Find<CannotSaveAfterCriticalErrorException>() == null)
				{
					ErrorReporter.ReportOnce(
						$"a345d72e-2cc5-426d-92ed-074d9db26dac|{ex.GetType().Name}|{ex.Message}",
						string.Format(CultureInfo.InvariantCulture, "Exception encountered applying client side actions for trigger {0}", trigger.HumanReadableName),
						ex);
				}
			}
		}

		static ZGuid GetParentPKForWorkflowTriggerEventReference(IBusiness job, IWorkflowTrigger trigger, IEventSource triggeringEvent)
		{
			if (job != null && job.Identifier != trigger.ParentID)
			{
				return job.Identifier;
			}
			else if (triggeringEvent != null)
			{
				var parentPK = triggeringEvent.ParentID;
				if (parentPK != trigger.ParentID)
				{
					return parentPK;
				}
			}

			return ZGuid.Empty;
		}

		public static (StmALog deletedLog, EventSource nextLog) DeleteWorkflowTriggerEventLog(this IWorkflowTrigger trigger, EventSource triggeringEvent)
		{
			var result = WorkflowTriggerEventCache.DeleteWTELog(trigger, triggeringEvent);
			if (result.deletedLog != null && trigger.TriggerFiredCountdown < short.MaxValue)
			{
				trigger.TriggerFiredCountdown++;
			}
			return result;
		}

		public static void DeleteWorkflowTriggerEventLogsByEventTime(this IWorkflowTrigger trigger, int numLogs)
		{
			WorkflowTriggerEventCache.ClearTopExistingWTELogsByEventTime(trigger, numLogs);
		}

		#region Unfire Trigger On Cancelled Event

		public static void UnfireTrigger(this IWorkflowTrigger trigger, IStmALog @event, IBusiness job, Action<ZDateTimeOffset> eventTimeSetter)
		{
			var (deletedLog, nextLog) = trigger.DeleteWorkflowTriggerEventLog(new EventSource(@event));
			if (deletedLog != null)
			{
				if (nextLog != null)
				{
					var newValue = (ZDateTimeOffset)nextLog.EventTime;
					trigger.AddWorkflowTriggerEventLog((BusinessObject)job, nextLog, @event, () => eventTimeSetter?.Invoke(newValue));
				}
				else
				{
					eventTimeSetter?.Invoke(ZDateTimeOffset.Empty);
				}
			}
		}

		#endregion

		public static bool IsRunningClientSideTriggerAction(this BusinessObjectFactory factory)
		{
			return isRunningClientSideTriggerAction;
		}

		static IDisposable RunningClientSideTrigger()
		{
			isRunningClientSideTriggerAction = true;
			return new DisposableAction(() => isRunningClientSideTriggerAction = false);
		}

		static WorkflowTriggerEventData GetTriggeringEventData(IWorkflowTrigger trigger, BusinessObject job, EventSource triggeringEvent)
		{
			var userContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, triggeringEvent.Log);
			return new WorkflowTriggerEventData(
				triggeringEvent,
				GetParentPKForWorkflowTriggerEventReference(job, trigger, triggeringEvent),
				GlbBranch.CurrentBranch.GB_Code,
				GlbDepartment.CurrentDepartment.GE_Code,
				userContext.StaffCode,
				userContext.BranchCode,
				userContext.DepartmentCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		static bool isRunningClientSideTriggerAction = false;
	}
}
