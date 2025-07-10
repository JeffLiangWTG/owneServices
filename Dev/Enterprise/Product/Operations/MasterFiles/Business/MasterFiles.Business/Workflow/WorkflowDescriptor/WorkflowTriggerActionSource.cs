using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowTriggerActionSource : IWorkflowTriggerActionSource
	{
		public WorkflowTriggerActionSource(BusinessObject job, IBaseTrigger trigger, ProcessTaskNotification action, StmALog wteLog, IStmALog triggeringLog)
			: this(job, trigger, action, new WorkflowTriggerEventData(wteLog), Lazy.Create(() => triggeringLog))
		{
		}
		public WorkflowTriggerActionSource(BusinessObject job, IBaseTrigger trigger, ProcessTaskNotification action, StmALog wteLog, Lazy<IStmALog> triggeringEventProvider)
			: this(job, trigger, action, new WorkflowTriggerEventData(wteLog), triggeringEventProvider)
		{
		}

		public WorkflowTriggerActionSource(BusinessObject job, IBaseTrigger trigger, ProcessTaskNotification action, IQueuedLog wteLog, Lazy<IStmALog> triggeringEventProvider)
			: this(job, trigger, action, new WorkflowTriggerEventData(wteLog), triggeringEventProvider)
		{
		}

		public WorkflowTriggerActionSource(BusinessObject job, IBaseTrigger trigger, ProcessTaskNotification action, WorkflowTriggerEventData triggerEvent, Lazy<IStmALog> triggeringEventProvider)
		{
			Job = job;
			Trigger = trigger;
			TriggerEvent = triggerEvent;
			Action = action;
			EventProvider = triggeringEventProvider;
		}

		public BusinessObject Job { get; }
		public IBaseTrigger Trigger { get; }
		public ProcessTaskNotification Action { get; }
		IProcessTaskNotification IWorkflowTriggerActionSource.Action => Action;
		public WorkflowTriggerEventData TriggerEvent { get; }
		public Lazy<IStmALog> EventProvider { get; }
		public IStmALog Event => EventProvider?.Value;
	}
}
