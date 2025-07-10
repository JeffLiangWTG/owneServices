using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	class TemplateApplicationDefaultableDateChecker : IMilestoneDateDefaultable
	{
		public TemplateApplicationDefaultableDateChecker(bool taskAlreadyCreated, ProcessTask templateTask, BusinessObject parent, WorkflowDescriptor descriptor)
		{
			Parent = parent ?? throw new ArgumentNullException(nameof(parent), "There is no reason that a trigger or milestone can exist without a parent.");
			Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor), "There is no reason that a trigger or milestone can exist without a workflow descriptor.");
			this.templateTask = templateTask ?? throw new ArgumentNullException(nameof(templateTask));
			TriggerCondition = templateTask.TriggerConditions.TriggerCondition;
			TriggerConditionValue = templateTask.TriggerConditions.TriggerConditionValue;
			TriggerFiredCountdown = templateTask.P9_TriggerFiredCountdown;
			if (!taskAlreadyCreated)
			{
				descriptor.SetDefaultTriggerConditions(this, parent);
			}
		}

		readonly ProcessTask templateTask;

		public BusinessObjectFactory Factory => Parent.Factory;

		public BusinessObject Parent { get; }

		public IWorkflowDescriptor Descriptor { get; }

		public bool HasTemplate => true;
		public bool IsLineTrigger => templateTask.IsLineTrigger;
		public bool IsWorkflowTrigger => templateTask.IsWorkflowTrigger;
		public bool IsEstimateTrigger => templateTask.ShouldTriggerOnEstimateEvents;
		public bool IsInDatabase => false;

		public ZDateTimeOffset ScheduledDate
		{
			get => scheduledDate;
			set => scheduledDate = value;
		}

		ZDateTimeOffset scheduledDate;

		public ZDateTimeOffset ActualDate
		{
			get => actualDate;
			set => actualDate = value;
		}

		ZDateTimeOffset actualDate;

		public ZDateTime TemplateCreateTimeUtc => templateTask.CreatedTimeUtc;

		public ZString TriggerCondition { get; set; }

		public ZString TriggerConditionValue { get; set; }

		public ZBool Cascading => templateTask.P9_RespondToCascadedEvents;

		public ZString CascadingContext => templateTask.P9_CascadedEventsContext;

		public Regex TemplateRegex => templateTask.TriggerConditions.TemplateRegex;

		public bool TrySetActualDateForEvent(IStmALog log, BusinessObject bizo, ZDateTimeOffset date)
		{
			ActualDate = date;
			return true;
		}

		public bool GetLogIsValidForDateDefaulting(IStmALog log) => Descriptor.GetLogIsValidForDateDefaulting(log);

		public ZString TemplateCondition1 => templateTask.P9_Condition1;

		public ZShort TriggerFiredCountdown { get; set; }

		public ZString TriggerEventCode
		{
			get => templateTask.P9_SE_NKMilestoneEvent;
			set => throw new InvalidOperationException("You may not override a template item through this wrapper class");
		}

		public ZString TriggerFieldName
		{
			get => templateTask.P9_TriggerField;
			set => throw new InvalidOperationException("You may not override a template item through this wrapper class");
		}

		public ZString ActualDateUpdateType
		{
			get => templateTask.P9_ActualDateUpdateType;
			set => throw new InvalidOperationException("You may not override a template item through this wrapper class");
		}

		BusinessObject ITriggerConditions.Job => Parent;
	}
}
