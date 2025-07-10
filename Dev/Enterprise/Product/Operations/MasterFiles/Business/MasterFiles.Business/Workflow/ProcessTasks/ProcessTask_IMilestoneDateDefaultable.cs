using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public partial class ProcessTask : IMilestoneDateDefaultable
	{
		IWorkflowDescriptor ITriggerConditions.Descriptor => this.GetWorkflowDescriptor();

		BusinessObject ITriggerConditions.Job => this.GetJob();

		BusinessObject IMilestoneDateDefaultable.Parent => !IsLineTrigger ? this.GetJob() : throw new InvalidOperationException();
		bool IMilestoneDateDefaultable.IsLineTrigger => IsLineTrigger;
		internal bool IsLineTrigger => !P9_LineTriggerType.IsEmpty;
		bool IMilestoneDateDefaultable.IsWorkflowTrigger => IsWorkflowTrigger;
		bool IMilestoneDateDefaultable.IsEstimateTrigger => ShouldTriggerOnEstimateEvents;
		bool IMilestoneDateDefaultable.IsInDatabase => IsInDatabase;

		ZDateTimeOffset IMilestoneDateDefaultable.ScheduledDate
		{
			get => P9_ScheduledDateOffset;
			set => P9_ScheduledDateOffset = value;
		}

		ZDateTimeOffset IMilestoneDateDefaultable.ActualDate
		{
			get => P9_ActualDateForBinding;
			set => P9_ActualDateForBinding = value;
		}

		bool IMilestoneDateDefaultable.HasTemplate => TemplateProcessTask != null;

		ZDateTime IMilestoneDateDefaultable.TemplateCreateTimeUtc => TemplateProcessTask?.CreatedTimeUtc ?? ZDateTime.Invalid;

		bool IMilestoneDateDefaultable.TrySetActualDateForEvent(IStmALog actualLog, BusinessObject job, ZDateTimeOffset time)
		{
			return TrySetActualDateForEvent(actualLog, job, time);
		}

		bool IMilestoneDateDefaultable.GetLogIsValidForDateDefaulting(IStmALog log) => WorkflowDescriptorCore?.GetLogIsValidForDateDefaulting(log) ?? true;

		ZString IMilestoneDateDefaultable.TemplateCondition1 => P9_Condition1;

		ZString IMilestoneDateDefaultable.ActualDateUpdateType
		{
			get => P9_ActualDateUpdateType;
			set => P9_ActualDateUpdateType = value;
		}
	}
}
