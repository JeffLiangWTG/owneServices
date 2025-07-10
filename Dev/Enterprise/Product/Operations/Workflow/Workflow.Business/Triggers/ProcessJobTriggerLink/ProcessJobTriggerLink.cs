using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public partial class ProcessJobTriggerLink : AutoProcessJobTriggerLink,
		IWorkflowTrigger,
		IProcessJobTriggerLink,
		IDynamicRootProvider,
		IUniqueIndexFailureHandler
	{
		public ProcessJobTriggerLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9L_TriggerFiredCountdown), ConcurrencyPolicy.Ignore);
		}

		#region BusinessObject Overrides

		public override void Delete()
		{
			base.Delete();
			OnDeleted?.Invoke(this, EventArgs.Empty);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P9L_GC_Company = Env.CurrentCompanyPK;
		}

		#endregion

		#region Properties

		public event EventHandler OnDeleted;

		[RelatedBusinessObject("TemplateTrigger")]
		public override ZGuid P9L_P9T_TemplateTrigger
		{
			get { return base.P9L_P9T_TemplateTrigger; }
			set { base.P9L_P9T_TemplateTrigger = value; }
		}

		#endregion

		#region Related Business Objects

		public ProcessTemplateTrigger TemplateTrigger => Factory.Load<ProcessTemplateTrigger>(P9L_P9T_TemplateTrigger);

		ITemplateTrigger ITemplateTrigger => TemplateTrigger;

		BusinessObject ParentJob => this.GetJob();

		#endregion

		#region IWorkflowItem Members

		ZGuid IWorkflowItem.CompanyPK
		{
			get
			{
				var templateTrigger = TemplateTrigger;
				if (templateTrigger.TriggerConditions.TriggerContextCode == TriggerUserContextList.Codes.Specified && templateTrigger.TriggerConditions.TriggerCompany.IsValid)
				{
					return templateTrigger.TriggerConditions.TriggerCompany;
				}
				else
				{
					return P9L_GC_Company;
				}
			}
		}

		ZString IWorkflowItem.Description
		{
			get { return TemplateTrigger.P9T_Description; }
			set { throw NotSupportedOnJobTrigger(); }
		}

		ZGuid IWorkflowItem.ParentID => P9L_ParentId;

		ZString IWorkflowItem.ParentTableCode => P9L_ParentTableCode;

		ZInt IWorkflowItem.Sequence => TemplateTrigger.P9T_Sequence;

		ZString IWorkflowItem.WorkflowItemType => Core.Constants.Workflow.WorkflowTriggerType;

		ZString IWorkflowTypeProvider.WorkflowProcessType => ITemplateTrigger.WorkflowProcessType;

		bool IWorkflowTypeProvider.IsTemplate => false;

		#endregion

		#region IBaseTrigger Members

		ZBool IBaseTrigger.ShouldTriggerOnEstimateEvents => TemplateTrigger.P9T_IsEstimate;

		ZBool IBaseTrigger.SuppressDuplicates => TemplateTrigger.P9T_SuppressDuplicates;

		ZInt IBaseTrigger.DelayDurationSeconds => TemplateTrigger.P9T_DelayDurationSeconds;

#if DEBUG
		void IBaseTrigger.SetShouldTriggerOnEstimateEvents_ForTests(bool value)
		{
			// do nothing
		}
#endif

		ZDateTime IBaseTrigger.ActualDate => ((IWorkflowTrigger)this).LastFiredTime.ToZDateTime();

		IActiveBusinessObjectCollection IBaseTrigger.TriggerActions
		{
			get
			{
				if (triggerActions == null)
				{
					triggerActions = new JobCompletionTriggerActionCollection(this);
				}

				return triggerActions;
			}
		}

		JobCompletionTriggerActionCollection triggerActions;

		ZString IEventReferenceConditions.TriggerCondition
		{
			get { return ITemplateTrigger.TriggerCondition; }
			set { throw NotSupportedOnJobTrigger(); }
		}

		ITriggerConditions IBaseTrigger.TriggerConditions_ForBinding => ITemplateTrigger.TriggerConditions_ForBinding;

		ZString IEventReferenceConditions.TriggerConditionValue
		{
			get { return ITemplateTrigger.TriggerConditionValue; }
			set { throw NotSupportedOnJobTrigger(); }
		}

		ZString ITriggerConditions.TriggerEventCode
		{
			get { return ITemplateTrigger.TriggerEventCode; }
			set { throw NotSupportedOnJobTrigger(); }
		}

		ZString ITriggerConditions.TriggerFieldName
		{
			get { return ITemplateTrigger.TriggerFieldName; }
			set { throw NotSupportedOnJobTrigger(); }
		}

		ZShort ITriggerConditions.TriggerFiredCountdown
		{
			get
			{
				return P9L_TriggerFiredCountdown;
			}
			set
			{
				P9L_TriggerFiredCountdown = value;
			}
		}

		ZBool ITriggerConditions.Cascading => ITemplateTrigger.Cascading;

		ZString ITriggerConditions.CascadingContext => ITemplateTrigger.CascadingContext;

		IWorkflowDescriptor ITriggerConditions.Descriptor => this.GetWorkflowDescriptor();

		BusinessObject ITriggerConditions.Job => ParentJob;

		bool IBaseTrigger.AreTriggerConditionsMet(IStmALog @event, IBusiness job)
		{
			return TriggerConditionEvaluator.AreTriggerConditionsMet(this, @event, (BusinessObject)job);
		}

		void IBaseTrigger.SetEventTime(IStmALog @event, IBusiness job, ZDateTimeOffset eventTime)
		{
			if (@event.SL_IsCancelled)
			{
				this.UnfireTrigger(@event, job, ((IBaseTrigger)this).SetEventTimeWithoutFiringWorkflow);
			}
			else if (eventTime.IsValid && P9L_TriggerFiredCountdown > 0)
			{
				((IBaseTrigger)this).SetEventTimeWithoutFiringWorkflow(eventTime);
				this.AddWorkflowTriggerEventLog((BusinessObject)job, new EventSource(@event), @event);
			}
			else
			{
				this.DeleteWorkflowTriggerEventLog(new EventSource(@event));
			}
		}

		void IBaseTrigger.SetEstimateTime(IStmALog @event, ZDateTimeOffset eventTime)
		{
			// Do nothing. UniversalTrigger doesn't support estimates.
		}

		void IBaseTrigger.SetEventTimeWithoutFiringWorkflow(ZDateTimeOffset eventTime)
		{
			var ghostTrigger = Factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentTemplateID, P9L_P9T_TemplateTrigger) { FetchOnlyFromLocalCache = true });
			if (ghostTrigger != null)
			{
				((IProcessTaskInternals)ghostTrigger).SetActualDateWithoutFiringWorkflow(new ZDateTimeOffset(eventTime, DateTimeKind.Local));
			}
		}

		void IBaseTrigger.Fire(IBusiness workflowParent, IStmALog @event)
		{
			if (P9L_TriggerFiredCountdown > 0)
			{
				this.AddWorkflowTriggerEventLog((BusinessObject)workflowParent, new EventSource(@event), @event);
			}
		}

		void IBaseTrigger.Withdraw(IBusiness workflowParent, IStmALog @event)
		{
			this.DeleteWorkflowTriggerEventLog(new EventSource(@event));
		}

		Exception NotSupportedOnJobTrigger([CallerMemberName] string callerName = "")
		{
			return new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Changing {0} is not supported on job trigger links. Job triggers are references to template triggers only - they use the config defined on templates directly.", callerName));
		}

		#endregion

		#region IWorkflowTrigger Members

		StmALog FindLastLog()
		{
			var lastWteLogQuery = new ZQuery(StmALogSchema.SL_Parent, PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)
				.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			lastWteLogQuery.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			return LogsFactory.LoadTop1<StmALog>(lastWteLogQuery);
		}

		ZDateTimeOffset IWorkflowTrigger.LastFiredTime => FindLastLog()?.SL_EventTimeOffset ?? ZDateTimeOffset.Empty;

		ZGuid IWorkflowTrigger.ParentTemplateID => P9L_P9T_TemplateTrigger;

		bool IWorkflowTrigger.TrySetActualDateForEvent(IWorkflowTriggerSource source, BusinessObject bizo, ZDateTimeOffset time) => throw new NotImplementedException("Universal Triggers do not currently support trigger fields.");

		void IWorkflowTrigger.UpdateTriggerActionNotifications(NotificationCollection notifications)
		{
			// Nothing to do here
		}

		#endregion

		#region IUniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return this; }
		}

		void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			HookNotifyForUnitTests();

			if (IsDeleted)
			{
				return;
			}

			var caption = Res.GetString("ProcessJobTriggerLink.UnqueIndexFail.Caption", "Conflict During Save");
			if (!IsInDatabase)
			{
				notifier.ReportInformation(
					Res.GetString("ProcessJobTriggerLink.UnqueIndexFail.Message", "A conflict occurred during save. The Universal Workflow Trigger [{0}] was already found in the database. Press OK to automatically fix this, then press save again.", TemplateTrigger?.P9T_Description),
					caption);

				var query = new ZDBOnlyQuery(typeof(ProcessJobTriggerLink))
								.AddToFilter(ProcessJobTriggerLinkSchema.P9L_ParentId, P9L_ParentId)
								.AddToFilter(ProcessJobTriggerLinkSchema.P9L_ParentTableCode, P9L_ParentTableCode)
								.AddToFilter(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, P9L_P9T_TemplateTrigger);

				var duplicate = Factory.Load<ProcessJobTriggerLink>(query).FirstOrDefault(l => l.PK != PK);
				if (duplicate != null)
				{
					foreach (var log in Logs.GetAllLogs().Cast<BusinessObject>().ToList())
					{
						var clonedLog = Factory.New<StmALog>();

						using (((IUpdateFieldsLock)clonedLog).LockForUpdatingKeyFields())
						{
							clonedLog.CopyPersistentValuesFrom(log);
							clonedLog.SL_Parent = duplicate.PK;
						}
						log.Delete();
					}
				}
				Delete();
			}
			else
			{
				ErrorReporter.ReportOnce("Somehow we have modified a saved ProcessJobTriggerLink to that it is having a uniqueIndex error. This should never happen.");
			}
		}

		IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames => new[] { ProcessJobTriggerLinkSchema.Constants.Indexes.NR_UC__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger };

		#endregion

		#region IRootTypeProvider Members

		Type[] IRootTypeProvider.RootTypes => this.GetRootTypes();

		BusinessObject[] IRootTypeProvider.Roots => ((IDynamicRootProvider)this).AugmentedRoots(this.GetJob());

		BusinessObject[] IDynamicRootProvider.AugmentedRoots(BusinessObject job) => this.GetRoots(job);

		#endregion

		#region IMilestoneDateDefaultable

		bool IMilestoneDateDefaultable.IsLineTrigger => false;
		bool IMilestoneDateDefaultable.IsWorkflowTrigger => true;
		bool IMilestoneDateDefaultable.IsEstimateTrigger => TemplateTrigger.P9T_IsEstimate;
		bool IMilestoneDateDefaultable.IsInDatabase => IsInDatabase;
		bool IMilestoneDateDefaultable.HasTemplate => true;
		BusinessObject IMilestoneDateDefaultable.Parent => ParentJob;
		ZDateTime IMilestoneDateDefaultable.TemplateCreateTimeUtc => TemplateTrigger.Template.P0_SystemCreateTimeUtc;
		bool IMilestoneDateDefaultable.TrySetActualDateForEvent(IStmALog actualLog, BusinessObject b, ZDateTimeOffset time) => false;
		public bool GetLogIsValidForDateDefaulting(IStmALog log) => this.GetWorkflowDescriptor().GetLogIsValidForDateDefaulting(log);

		ZDateTimeOffset IMilestoneDateDefaultable.ScheduledDate
		{
			get => ZDateTimeOffset.Empty;
			set { }
		}

		ZDateTimeOffset IMilestoneDateDefaultable.ActualDate
		{
			get => ZDateTimeOffset.Empty;
			set { }
		}

		ZString IMilestoneDateDefaultable.ActualDateUpdateType
		{
			get => ZString.Empty;
			set { }
		}

		ZString IMilestoneDateDefaultable.TemplateCondition1 => ZString.Empty;

		#endregion

		#region ITriggerUserContextConditions

		ITriggerUserContextConditions TemplateConditions => TemplateTrigger;

		ZString ITriggerUserContextConditions.TriggerContextCode
		{
			get => TemplateConditions.TriggerContextCode;
			set => throw new InvalidOperationException("Updating this field is not possible.");
		}

		ZString ITriggerUserContextConditions.TriggerStaffCode
		{
			get => TemplateConditions.TriggerStaffCode;
			set => throw new InvalidOperationException("Updating this field is not possible.");
		}

		ZGuid ITriggerUserContextConditions.TriggerBranch
		{
			get => TemplateConditions.TriggerBranch;
			set => throw new InvalidOperationException("Updating this field is not possible.");
		}

		ZGuid ITriggerUserContextConditions.TriggerCompany
		{
			get => TemplateConditions.TriggerCompany;
			set => throw new InvalidOperationException("Updating this field is not possible.");
		}

		ZGuid ITriggerUserContextConditions.TriggerDepartment
		{
			get => TemplateConditions.TriggerDepartment;
			set => throw new InvalidOperationException("Updating this field is not possible.");
		}

		#endregion

		#region For Test Purposes
		partial void HookNotifyForUnitTests();
		#endregion
	}

	#region Test
#if DEBUG
	public partial class ProcessJobTriggerLink : AutoProcessJobTriggerLink,
			IWorkflowTrigger,
			IProcessJobTriggerLink,
			IDynamicRootProvider,
			IUniqueIndexFailureHandler
	{
		Action<ProcessJobTriggerLink> OnTriggerLinkSave;
		public void SetOnTriggerLinkSaveHookForTest(Action<ProcessJobTriggerLink> onSaveHook)
		{
			OnTriggerLinkSave = onSaveHook;
		}
		partial void HookNotifyForUnitTests()
		{
			OnTriggerLinkSave?.Invoke(this);
		}
	}
#endif
	#endregion
}
