using System;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	[DebuggerDisplay("ID:{P9_TaskID}, Sequence:{P9_Sequence}, Description: {P9_Description}, Resource:{P9_GS_NKAssignedStaffMember}")]
	public class ViewProcessTask : AutoViewProcessTask, IWorkflowTask, IAssignedWorkflowItem
	{
		public ViewProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		#region P9_ActualDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_ActualDuration
		{
			get => base.P9_ActualDuration;
			set => base.P9_ActualDuration = value.ConvertToDurationBasedDate(P9_ActualDurationInfo);
		}

		#endregion

		#region P9_EstDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_EstDuration
		{
			get => base.P9_EstDuration;
			set => base.P9_EstDuration = value.ConvertToDurationBasedDate(P9_EstDurationInfo);
		}

		#endregion

		#region P9_EstimatedTimeToComplete

		[ZDateTimeDurationValue]
		public override ZDateTime P9_EstimatedTimeToComplete
		{
			get => base.P9_EstimatedTimeToComplete;
			set => base.P9_EstimatedTimeToComplete = value.ConvertToDurationBasedDate(P9_EstimatedTimeToCompleteInfo);
		}

		#endregion

		#region P9_TotalSuspendedDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_TotalSuspendedDuration
		{
			get => base.P9_TotalSuspendedDuration;
			set => base.P9_TotalSuspendedDuration = value.ConvertToDurationBasedDate(P9_TotalSuspendedDurationInfo);
		}

		#endregion

		public override void OnSaving()
		{
			throw new NotSupportedException("This is a view and should never be updated. It is intended for lightweight loading only.");
		}

		public override void Delete()
		{
			throw new NotSupportedException("This is a view and should never be deleted. It is intended for lightweight loading only.");
		}

		#endregion

		#region IWorkflowItem Members

		ZGuid IWorkflowItem.ParentID => P9_ParentID;

		ZString IWorkflowItem.ParentTableCode => P9_ParentTableCode;

		ZGuid IWorkflowItem.CompanyPK => P9_GC;

		ZString IWorkflowItem.Description
		{
			get { return P9_Description; }
			set { P9_Description = value; }
		}

		ZInt IWorkflowItem.Sequence => P9_Sequence;

		ZString IWorkflowItem.WorkflowItemType => P9_Type;

		bool IWorkflowTypeProvider.IsTemplate => P9_Type.EqualsIgnoringCase(ProcessTaskTemplateSchema.Constants.Prefix);
		ZString IWorkflowTypeProvider.WorkflowProcessType
		{
			get { throw new NotSupportedException("We haven't implemented determination of workflow type in SQL for ViewProcessTask. See ViewProcessHeader."); }
		}

		#endregion

		#region IWorkflowTask Members

		public ZDecimal RelevantEstimateHours
		{
			get { return this.GetRelevantEstimateHours(); }
		}

		ZDateTimeOffset IWorkflowTask.P9_ScheduledDate
		{
			get => new ZDateTimeOffset(P9_ScheduledDate);
			set => P9_ScheduledDate = value.ToZDateTime();
		}

		ZDateTimeOffset IWorkflowTask.P9_ActualDate => new ZDateTimeOffset(P9_ActualDate);

		ZDateTimeOffset IWorkflowTask.P9_SuspendedAt
		{
			get => new ZDateTimeOffset(P9_SuspendedAt);
			set => P9_SuspendedAt = value.ToZDateTime();
		}

		ZDateTimeOffset IWorkflowTask.P9_CompletedTime
		{
			get => new ZDateTimeOffset(P9_CompletedTimeUtc, DateTimeKind.Utc);
			set => P9_CompletedTimeUtc = value.ToUtcZDateTime();
		}

		#endregion

		#region IAssignedWorkflowItem

		ZString IAssignedWorkflowItem.AssignedStaffCode => P9_GS_NKAssignedStaffMember;

		ZGuid IAssignedWorkflowItem.AssignedGroupPK => P9_GG_AssignedGroup;

		#endregion
	}
}
