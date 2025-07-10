using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExampleLog : IQueuedLog
	{
		public ExampleLog(IBaseTrigger trigger)
		{
			this.trigger = trigger;
		}

		public ExampleLog(ProcessTaskNotification action)
			: this(action?.Parent)
		{
		}

		readonly IBaseTrigger trigger;
		IBaseTrigger Parent => trigger;

		BusinessObjectFactory IQueuedLog.Factory => trigger.Factory;
		ZGuid IQueuedLog.PK { get; } = ZGuid.NewZGuid();
		public ZGuid SJ_ParentID => trigger?.Identifier ?? ZGuid.Empty;
		ZGuid IQueuedLog.SJ_ALogReference => ZGuid.Empty;
		ZGuid IQueuedLog.SJ_TargetID => Parent?.ParentID ?? ZGuid.Empty;
		ZString IQueuedLog.SJ_GS_NKUser => StaffCode;
		public ZString SJ_Reference => Parent?.TriggerConditionValue ?? ZString.Empty;
		ZString IQueuedLog.SJ_SE_NKEvent => Events.WorkflowTriggerEventCode;
		ZString IQueuedLog.SJ_GB_NKBranch => Env.CurrentBranch.Code;
		ZString IQueuedLog.SJ_GE_NKDepartment => Env.CurrentDepartment.Code;
		public ZDateTime SJ_EventTime => ZDateTime.Now;
		public ZDateTime SJ_EventTimeUtc => ZDateTime.UtcNow;
		ZString IQueuedLog.SJ_ParentTableCode => ProcessTasksSchema.Constants.Prefix;
		public ZBool SJ_IsEstimate => ZBool.False;
		bool IQueuedLog.IsRetry => false;
		ZBool IQueuedLog.SJ_IsDelayFired => false;
		public ZDateTime SJ_PostedTimeUtc => ZDateTime.UtcNow;
		IEnumerable<IStmChangeLog> IQueuedLog.ChangeLogs => Enumerable.Empty<IStmChangeLog>();
		public ZByte SJ_RetryCount { get; set; }
		public ZString SJ_Status { get; set; }
		ZString IEventUserContextSource.StaffCode => StaffCode;
		ZString StaffCode => GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
		ZString IWorkflowTriggerSource.DepartmentCode => ((IQueuedLog)this).SJ_GE_NKDepartment;
		ZString IWorkflowTriggerSource.BranchCode => ((IQueuedLog)this).SJ_GB_NKBranch;
		ZString IWorkflowTriggerSource.CompanyCode => Env.CurrentCompany.Code;
		ZGuid IIdentified.Identifier => ZGuid.Empty;
		ZGuid IWorkflowTriggerSource.ParentID => SJ_ParentID;
		ZDateTime IWorkflowTriggerSource.EventTime => SJ_EventTime;
		ZString IWorkflowTriggerSource.SourceType => ZString.Empty;
		ZString IWorkflowTriggerSource.Reference => SJ_Reference;
		ZBool IWorkflowTriggerSource.IsEstimate => SJ_IsEstimate;
		ZDateTime IWorkflowTriggerSource.PostedTimeUtc => SJ_PostedTimeUtc;
		ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => ZDateTimeOffset.Now;
		ZString IWorkflowTriggerSource.FriendlyTableName => ZString.Empty;
		IPropagationSettings IWorkflowTriggerSource.PropagationSettings => new DefaultPropagationSettings();
		ZDateTime IWorkflowTriggerSource.EventTimeUtc => SJ_EventTimeUtc;
		ZString IWorkflowTriggerSource.Source => ZString.Empty;
		ZBool IWorkflowTriggerSource.IsCancelled => false;
		ZString IEventUserContextSource.UserCode => StaffCode;
	}
}
