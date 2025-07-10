using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using User = Enterprise.ZArchitecture.Environment.User;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowUserContextDecider
	{
		public static WorkflowUserContext GetTemporaryUserContext(
			IBaseTrigger trigger,
			BusinessObject job,
			IEventUserContextSource userContextSource,
			INotifications notifications = null
			)
		{
			Argument.NotNull(trigger, nameof(trigger));
			Argument.NotNull(userContextSource, nameof(userContextSource));

			WorkflowUserContext userContext;
			switch (GetTriggerContext(trigger, userContextSource).TriggerUserContext)
			{
				case TriggerUserContextList.Codes.Specified:
					userContext = GetSpecifiedUserContext(trigger, job, userContextSource);
					break;
				case TriggerUserContextList.Codes.Default:
					userContext = GetDefaultUserContext(trigger, userContextSource);
					break;
				case TriggerUserContextList.Codes.Event:
					userContext = GetTriggeringEventUserContext(trigger, job, userContextSource);
					break;
				case null:
					userContext = WorkflowUserContext.NullContext;
					break;
				default:
					userContext = GetDefaultUserContext(trigger, userContextSource);
					break;
			}

			return CheckInactiveUserContext(trigger, userContext, notifications);
		}

		static WorkflowUserContext CheckInactiveUserContext(
			IBaseTrigger trigger,
			WorkflowUserContext userContext,
			INotifications notifications = null
			)
		{
			if (notifications != null && userContext != null && !userContext.IsNull() && !userContext.Staff.GS_IsActive)
			{
				BusinessObjectFactory factory = trigger.Factory;
				notifications.AddWarning(FormattableString.Invariant($"User {userContext.Staff.GS_Code} is inactive. Default context will be used."));
				GlbStaff glbStaff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
				return new WorkflowUserContext(factory, glbStaff, userContext.Branch, userContext.Department);
			}
			return userContext;
		}

		public static WorkflowTriggerUserContext GetTriggerContext(
			IBaseTrigger trigger,
			IEventUserContextSource userContextSource,
			INotifications notifications = null)
		{
			Argument.NotNull(trigger, nameof(trigger));
			Argument.NotNull(userContextSource, nameof(userContextSource));

			if (!WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.Value)
			{
				if (!trigger.TriggerContextCode.Equals(TriggerUserContextList.Codes.Default))
				{
					var message = string.Format(CultureInfo.InvariantCulture,
						(NoResString)"Ignoring Trigger Context configuration because {0} is disabled and parent has a non-default Trigger Context",
						WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.HumanReadableRegistryPath());
					return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Default, message);
				}
			}

			switch (trigger.TriggerContextCode)
			{
				case TriggerUserContextList.Codes.Specified:
					return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Specified);

				case TriggerUserContextList.Codes.Default:
					return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Default);

				case TriggerUserContextList.Codes.Event:
					if (userContextSource is IQueuedLog || userContextSource is IStmALog)
					{
						return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Event);
					}
					else if (userContextSource is IStmChangeLog)
					{
						return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Default,
							(NoResString)"Field Change Trigger invalid context specifier. Using default.");
					}
					return WorkflowTriggerUserContext.NullContext;

				default:
					notifications?.AddWarning(FormattableString.Invariant($"Unknown context type: {trigger.TriggerContextCode}. Default context will be used."));
					var message = FormattableString.Invariant($"Ignoring Trigger Context configuration because {trigger.TriggerContextCode} is an unknown context type.");
					return new WorkflowTriggerUserContext(TriggerUserContextList.Codes.Default, message);
			}
		}

		#region GetContext Methods

		static WorkflowUserContext GetSpecifiedUserContext(IBaseTrigger trigger, BusinessObject job, IEventUserContextSource userContextSource)
		{
			var factory = trigger.Factory;
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, trigger.TriggerStaffCode) ?? GetDefaultStaff(factory, userContextSource);
			var branch = factory.Load<GlbBranch>(trigger.TriggerBranch) ?? GetDefaultBranch(trigger, job, staff);
			var department = factory.Load<GlbDepartment>(trigger.TriggerDepartment) ?? GetDefaultDepartment(trigger, job);

			if (staff == null || branch == null || department == null)
			{
				ErrorReporter.ReportOnce("edfec0da-b0d5-417b-80f0-b92a9cc9a8f5", FormattableString.Invariant($"Null found: Staff {trigger.TriggerStaffCode} {staff}, Branch: {trigger.TriggerBranch} {branch}, Department: {trigger.TriggerDepartment} {department}"));
			}

			return new WorkflowUserContext(factory, staff, branch, department);
		}

		static WorkflowUserContext GetTriggeringEventUserContext(IBaseTrigger trigger, BusinessObject job, IEventUserContextSource userContextSource)
		{
			if (userContextSource is IQueuedLog log)
			{
				return GetUserContextFromCodes(trigger, job, userContextSource, log.SJ_GS_NKUser, log.SJ_GB_NKBranch, log.SJ_GE_NKDepartment);
			}
			else if (userContextSource is IStmALog triggeringEvent)
			{
				return GetUserContextFromCodes(trigger, job, userContextSource, triggeringEvent.SL_GS_NKUser, triggeringEvent.SL_GB_NKBranch, triggeringEvent.SL_GE_NKDepartment);
			}
			ErrorReporter.ReportOnce("097bfda2-ee98-4de1-b420-072c9507cf94", FormattableString.Invariant($"Event Context can only be IQueuedLog or IStmALog [{userContextSource.GetType().Name}]"));
			return WorkflowUserContext.NullContext;
		}

		static WorkflowUserContext GetUserContextFromCodes(IBaseTrigger trigger, BusinessObject job, IEventUserContextSource userContextSource, ZString staffCode, ZString branchCode, ZString departmentCode)
		{
			var factory = trigger.Factory;
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode) ?? GetDefaultStaff(factory, userContextSource);
			var branch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode) ?? GetDefaultBranch(trigger, job, staff);
			var department = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode) ?? GetDefaultDepartment(trigger, job);

			return new WorkflowUserContext(factory, staff, branch, department);
		}

		static WorkflowUserContext GetDefaultUserContext(IBaseTrigger trigger, IEventUserContextSource contextSource)
		{
			BusinessObjectFactory factory = trigger.Factory;
			BusinessObject job = trigger.GetJob();
			GlbStaff glbStaff = GetDefaultStaff(factory, contextSource);
			GlbBranch defaultBranch = GetDefaultBranch(trigger, job, glbStaff);
			GlbDepartment defaultDepartment = GetDefaultDepartment(trigger, job);

			return new WorkflowUserContext(factory, glbStaff, defaultBranch, defaultDepartment);
		}

		static GlbStaff GetDefaultStaff(BusinessObjectFactory factory, IEventUserContextSource contextSource) => factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, contextSource.StaffCode) ?? GlbStaff.CurrentUser;

		static GlbBranch GetDefaultBranch(IBaseTrigger trigger, BusinessObject job, GlbStaff staff)
		{
			var userLogin = Lazy.Create(() => staff, isThreadSafe: true);
			return TriggerProvider.GetBranchForTemporaryUserContext(trigger, job, userLogin) ?? GlbBranch.CurrentBranch;
		}

		static GlbDepartment GetDefaultDepartment(IBaseTrigger trigger, BusinessObject job)
		{
			var jobHeader = job != null ? GetJobHeader(trigger, job) : null;
			return jobHeader?.Department ?? GlbDepartment.CurrentDepartment;
		}

		static JobHeader GetJobHeader(IBaseTrigger trigger, BusinessObject job)
		{
			if (job is IJobHeaderParent jobHeaderParent)
			{
				return new JobHeader.Loader(jobHeaderParent).Load();
			}

			var query = new ZQuery(JobHeaderSchema.JH_ParentID, job.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, trigger.CompanyPK);
			return trigger.Factory.LoadTop1<JobHeader>(query);
		}

		#endregion
	}

	#region WorkflowTriggerUserContext

	public class WorkflowTriggerUserContext
	{
		public static WorkflowTriggerUserContext NullContext => new WorkflowTriggerUserContext(null, "");

		public WorkflowTriggerUserContext(string triggerUserContext, string message = null)
		{
			TriggerUserContext = triggerUserContext;
			Message = message ?? GetLog(triggerUserContext);
		}

		public string TriggerUserContext { get; }

		public string Message { get; }

		string GetLog(string type)
		{
			return FormattableString.Invariant($"Switched to user context of type [{type}] for batch.");
		}
	}

	#endregion
}
