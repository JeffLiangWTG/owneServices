using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class WTELogViewModel : NonPersistentBusinessObject
	{
		public WTELogViewModel(IBaseTrigger trigger, StmALog wteLog)
			: base(trigger.Factory)
		{
			Trigger = trigger;
			WTELog = wteLog;
			if (wteLog != null)
			{
				Job = trigger.GetParentBusinessObject(wteLog);
			}
			else
			{
				Job = trigger.GetJob();
			}
		}

		public StmALog WTELog { get; }
		IBaseTrigger Trigger { get; }
		BusinessObject Job { get; }

		#region Fields

		[ResourceStringData("WTELogViewModel.SL_PK", Caption = "Primary Key", FullDescription = "Database primary key for this event.")]
		public ZString SL_PK => WTELog.PK.ToString();
		public ZPropertyInfo SL_PKInfo => GetZPropertyInfo(nameof(SL_PK));

		[ResourceStringData("WTELogViewModel.SL_Table", Caption = "Table")]
		public ZString SL_Table => WTELog.SL_Table;
		public ZPropertyInfo SL_TableInfo => GetZPropertyInfo(nameof(SL_Table));

		[ResourceStringData("WTELogViewModel.SL_Parent", Caption = "Parent")]
		public ZString SL_Parent => WTELog.SL_Parent.ToString();
		public ZPropertyInfo SL_ParentInfo => GetZPropertyInfo(nameof(SL_Parent));

		[ResourceStringData("WTELogViewModel.SL_IsEstimate", Caption = "Estimate", FullDescription = "Specifies whether this event is an estimate, rather than an actual")]
		public ZBool SL_IsEstimate => WTELog.SL_IsEstimate;
		public ZPropertyInfo SL_IsEstimateInfo => GetZPropertyInfo(nameof(SL_IsEstimate));

		[ResourceStringData("WTELogViewModel.SL_IsCancelled", Caption = "Canceled")]
		public ZBool SL_IsCancelled => WTELog.SL_IsCancelled;
		public ZPropertyInfo SL_IsCancelledInfo => GetZPropertyInfo(nameof(SL_IsCancelled));

		[ResourceStringData("WTELogViewModel.SL_PostedTimeUtc", Caption = "Posted Time (UTC)", FullDescription = "Displays the date and time that the event was created in the system. For system generated events this will be equal to the event time. For user generated events this may be different as the user may nominate a date and time that is not equal to the system date and time when adding an event.")]
		public ZDateTime SL_PostedTimeUtc => WTELog.SL_PostedTimeUtc;
		public ZPropertyInfo SLPostedTimeUtcInfo => GetZPropertyInfo(nameof(SL_PostedTimeUtc));

		[ResourceStringData("WTELogViewModel.SL_EventTime", Caption = "Event Time", FullDescription = "Displays the Date and Time of the event.")]
		public ZDateTime SL_EventTime => WTELog.SL_EventTime;
		public ZPropertyInfo SL_EventtimeInfo => GetZPropertyInfo(nameof(SL_EventTime));

		[ResourceStringData("WTELogViewModel.SL_EventTimeUtc", Caption = "Event Time (UTC)", FullDescription = "Displays the UTC Date and Time of the event.")]
		public ZDateTime SL_EventTimeUtc => WTELog.SL_EventTimeUtc;
		public ZPropertyInfo SL_EventtimeUtcInfo => GetZPropertyInfo(nameof(SL_EventTimeUtc));

		[ResourceStringData("WTELogViewModel.EventLocalBranchTime", Caption = "Event Time (Local)", FullDescription = "Displays the local Date and Time of the event.")]
		public ZDateTime EventLocalBranchTime => WTELog.EventLocalBranchTime;
		public ZPropertyInfo EventLocalBranchTimeInfo => GetZPropertyInfo(nameof(EventLocalBranchTime));

		[ResourceStringData("WTELogViewModel.SL_GS_NKUser", Caption = "Staff Code", FullDescription = "The code of the staff member who caused this log to be created.")]
		public ZString SL_GS_NKUser => WTELog.SL_GS_NKUser;
		public ZPropertyInfo SL_GS_NKUserInfo => GetZPropertyInfo(nameof(SL_GS_NKUser));

		[ResourceStringData("WTELogViewModel.SL_GB_NKBranch", Caption = "Branch", FullDescription = "The branch stored against the log when created.")]
		public ZString SL_GB_NKBranch => WTELog.SL_GB_NKBranch;
		public ZPropertyInfo SL_GL_NKBranchInfo => GetZPropertyInfo(nameof(SL_GB_NKBranch));

		[ResourceStringData("WTELogViewModel.CompanyCode", Caption = "Company", FullDescription = "The company stored against the log when created.")]
		public ZString CompanyCode => WTELog.CompanyCode;
		public ZPropertyInfo CompanyInfo => GetZPropertyInfo(nameof(CompanyCode));

		[ResourceStringData("WTELogViewModel.SL_GE_NKDepartment", Caption = "Department")]
		public ZString SL_GE_NKDepartment => WTELog.SL_GE_NKDepartment;
		public ZPropertyInfo SL_GE_NKDepartmentInfo => GetZPropertyInfo(nameof(SL_GE_NKDepartment));

		[ResourceStringData("WTELogViewModel.SL_FireWorkflow", Caption = "Fire Workflow", FullDescription = "When this flag is set, workflow templates are applied before this event is processed.")]
		public ZBool SL_FireWorkflow => WTELog.SL_FireWorkflow;
		public ZPropertyInfo SL_FireWorkflowInfo => GetZPropertyInfo(nameof(SL_FireWorkflow));

		[ResourceStringData("WTELogViewModel.SL_TriggeredBranch", Caption = "Triggered Branch", FullDescription = "This is the Branch used when implementing the actions on the trigger.")]
		public ZString SL_TriggeredBranch => ObjectFactory.Get<IWorkflowTriggerUserContextProvider>().GetBranch(Trigger, Job, WTELog);
		public ZPropertyInfo SL_TriggeredBranchInfo => GetZPropertyInfo(nameof(SL_TriggeredBranch));

		#endregion

		#region Related Business Objects

		#region Source Logs

		public StmALogCollection SourceLogs
		{
			get
			{
				if (sourceLogs == null)
				{
					sourceLogs = new StmALogCollection(Factory);

					if (WTELog != null)
					{
						var wteData = new WorkflowTriggerEventData(WTELog);
						if (TriggeringLogFinder.TryFindLog(wteData, Trigger, Job, out StmALog result, findCancelled: true)
							&& CanShowLogToCurrentUser(result))
						{
							sourceLogs.Add(result);
						}
					}
				}

				return sourceLogs;
			}
		}

		StmALogCollection sourceLogs;

		public ZString HiddenSourceLogDisplayText { get; private set; }

		public ZPropertyInfo HiddenSourceLogDisplayTextInfo => GetZPropertyInfo(nameof(HiddenSourceLogDisplayText));

		void SetHiddenSourceLogDisplayText(ZString company, ZString branch, ZString department)
		{
			HiddenSourceLogDisplayText = Res.GetString("8BBB6FAA-F8F1-48F8-B5BE-ED9FAE6001D7", "This source log is available when logged into company {0}, branch {1} and department {2}", company, branch, department);
			HiddenSourceLogDisplayTextInfo.RefreshBinding();
		}

		bool CanShowLogToCurrentUser(StmALog log)
		{
			if (TriggeringLogFinder.IsLogVisibleToCurrentUser((IStmALogParent)Job, log.SL_Parent))
			{
				return true;
			}
			else
			{
				var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SL_GB_NKBranch);
				var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, log.SL_GE_NKDepartment);

				if (branch != null && department != null)
				{
					var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());

					if (security.Login.IsAllowed)
					{
						return true;
					}
					else
					{
						SetHiddenSourceLogDisplayText(log.CompanyCode, branch.GB_Code, department.GE_Code);
						return false;
					}
				}
			}

			return false;
		}

		#endregion

		#region Job Queues

		public virtual StmJobQueueCollection JobQueues
		{
			get
			{
				if (jobQueues == null)
				{
					jobQueues = new StmJobQueueCollection(WTELog);
					jobQueues.Load();
				}

				return jobQueues;
			}
		}

		StmJobQueueCollection jobQueues;

		#endregion

		#endregion
	}
}
