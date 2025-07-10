using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessTemplateTrigger : AutoProcessTemplateTrigger,
		IUniversalTemplateTrigger,
		IRootTypeProvider,
		ITriggerUserContextConditions,
		IAntlrMacroContextProvider
	{
		public ProcessTemplateTrigger(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public override void Delete()
		{
			if (IsInDatabase && Factory.ExistsInDatabase(ProcessJobTriggerLinkSchema.Constants.TableName, new ZQuery(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, PK)))
			{
				throw new CannotDeleteException(Res.GetString("c53868f9-9fba-497d-98a6-28e7e95e5ac1", "This trigger has previously fired and cannot be deleted. It should be marked as inactive instead."));
			}

			TriggerActions.DeleteAll();

			base.Delete();
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public CompletionTriggerActionCollection TriggerActions
		{
			get
			{
				if (triggerActions == null)
				{
					triggerActions = new CompletionTriggerActionCollection(this);
					RegisterEditableChildObject(triggerActions);
				}

				return triggerActions;
			}
		}

		CompletionTriggerActionCollection triggerActions;

		ZQuery GetJobVersionOfTriggerQuery(ZGuid jobPK, ZString parentTableCode)
		{
			var query = new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, jobPK);
			query.AddToFilter(ProcessJobTriggerLinkSchema.P9L_ParentTableCode, parentTableCode);
			query.AddToFilter(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, PK);
			return query;
		}

		ProcessJobTriggerLink GetJobVersionOfTrigger(ZGuid jobPK, ZString parentTableCode)
		{
			var query = GetJobVersionOfTriggerQuery(jobPK, parentTableCode);
			return Factory.LoadTop1<ProcessJobTriggerLink>(query);
		}

		#endregion

		#region New Properties

		public TemplateConditionsViewModel TemplateConditions
		{
			get
			{
				if (templateConditions == null)
				{
					templateConditions = new TemplateConditionsViewModel(this, Template);
					RegisterEditableChildObject(templateConditions);
				}

				return templateConditions;
			}
		}

		TemplateConditionsViewModel templateConditions;

		public TriggerConditionsViewModel TriggerConditions
		{
			get
			{
				if (triggerConditions == null)
				{
					triggerConditions = new ProcessTemplateTriggerConditionsViewModel(this);
					RegisterEditableChildObject(triggerConditions);
				}

				return triggerConditions;
			}
		}

		TriggerConditionsViewModel triggerConditions;

		[ZDateTimeDurationValueCalculatedFromSeconds]
		public ZDateTime P9T_DelayDuration
		{
			get => TimeSpan.FromSeconds(P9T_DelayDurationSeconds);
			set
			{
				P9T_DelayDurationSeconds = DurationAsDateTimeHelper.DateTimeToDuration(value);

				P9T_DelayDurationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9T_DelayDurationInfo
		{
			get { return GetZPropertyInfo(nameof(P9T_DelayDuration)); }
		}

		#endregion

		#region IWorkflowItem Members

		ZGuid IWorkflowItem.ParentID => P9T_P0_Template;

		ZString IWorkflowItem.ParentTableCode => ProcessTaskTemplateSchema.Constants.Prefix;

		ZGuid IWorkflowItem.CompanyPK => ZGuid.Empty;

		ZString IWorkflowItem.Description
		{
			get { return P9T_Description; }
			set { P9T_Description = value; }
		}

		ZInt IWorkflowItem.Sequence => P9T_Sequence;

		ZString IWorkflowItem.WorkflowItemType => Constants.Workflow.WorkflowTriggerType;

		ZString IWorkflowTypeProvider.WorkflowProcessType => Template?.P0_ProcessType ?? ZString.Empty;

		bool IWorkflowTypeProvider.IsTemplate => true;

		#endregion

		#region ITemplateConditional Members

		ZString ITemplateConditional.TemplateCondition1
		{
			get { return P9T_TemplateCondition1; }
			set { P9T_TemplateCondition1 = value; }
		}

		ZString ITemplateConditional.TemplateCondition2
		{
			get { return P9T_TemplateCondition2; }
			set { P9T_TemplateCondition2 = value; }
		}

		ZString ITemplateConditional.TemplateCondition2Value
		{
			get
			{
				if (ProcessTasksLookups.IsMacroCondition(P9T_TemplateCondition2))
				{
					return Encoding.Unicode.GetString(P9T_TemplateUserDefinedConditionValue);
				}
				else
				{
					return P9T_TemplateCondition2Value;
				}
			}
			set
			{
				if (ProcessTasksLookups.IsMacroCondition(P9T_TemplateCondition2))
				{
					P9T_TemplateUserDefinedConditionValue = Encoding.Unicode.GetBytes(value);
				}
				else
				{
					P9T_TemplateCondition2Value = value;
				}
			}
		}

		ZString ITemplateConditional.OriginCountryCode
		{
			get { return P9T_RN_NKOriginCountry; }
			set { P9T_RN_NKOriginCountry = value; }
		}

		ZString ITemplateConditional.DestinationCountryCode
		{
			get { return P9T_RN_NKDestinationCountry; }
			set { P9T_RN_NKDestinationCountry = value; }
		}

		#endregion

		#region IBaseTrigger Members

		IActiveBusinessObjectCollection IBaseTrigger.TriggerActions => TriggerActions;

		ITriggerConditions IBaseTrigger.TriggerConditions_ForBinding => TriggerConditions;

		ZDateTime IBaseTrigger.ActualDate => ZDateTime.Empty;

		ZBool IBaseTrigger.ShouldTriggerOnEstimateEvents => P9T_IsEstimate;

		ZBool IBaseTrigger.SuppressDuplicates => P9T_SuppressDuplicates;

		ZInt IBaseTrigger.DelayDurationSeconds => P9T_DelayDurationSeconds;

#if DEBUG
		void IBaseTrigger.SetShouldTriggerOnEstimateEvents_ForTests(bool value)
		{
			P9T_IsEstimate = value;
		}
#endif

		bool IBaseTrigger.AreTriggerConditionsMet(IStmALog @event, IBusiness bizo)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There's no context on a {0} to determine if trigger conditions are met since trigger conditions must be evaluated against a job, and this trigger is defined on a template.", GetType().Name));
		}

		void IBaseTrigger.SetEventTime(IStmALog @event, IBusiness job, ZDateTimeOffset eventTime)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There's no context on a {0} to fire this trigger, since this trigger is defined on a template.", GetType().Name));
		}

		void IBaseTrigger.SetEstimateTime(IStmALog @event, ZDateTimeOffset estimateTime)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There's no context on a {0} to estimate this trigger, since this trigger is defined on a template.", GetType().Name));
		}

		void IBaseTrigger.SetEventTimeWithoutFiringWorkflow(ZDateTimeOffset eventTime)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There's no context on a {0} to fire this trigger, since this trigger is defined on a template.", GetType().Name));
		}

		void IBaseTrigger.Fire(IBusiness workflowParent, IStmALog @event)
		{
			var jobTrigger = GetOrCreateJobVersionOfTrigger(workflowParent, createIfNotFound: true);
			((IBaseTrigger)jobTrigger).Fire(workflowParent, @event);
		}

		void IBaseTrigger.Withdraw(IBusiness workflowParent, IStmALog @event)
		{
			var jobTrigger = GetOrCreateJobVersionOfTrigger(workflowParent, createIfNotFound: false);

			if (jobTrigger != null)
			{
				((IBaseTrigger)jobTrigger).Withdraw(workflowParent, @event);
			}
		}

		#endregion

		#region IUniversalTemplateTrigger

		public bool AreConditionsMet(IBusiness job, IStmALog log)
		{
			var provider = (IWorkflowProvider)job;
			return provider.AreTemplateConditionsMet(this) && provider.AreTriggerConditionsMet(this, TriggerConditions.TriggerConditionValue, log);
		}

		void IUniversalTemplateTrigger.ValidateShouldTriggerOnEstimateEvents()
		{
			Validation.ValidateP9T_IsEstimate();
		}

		#endregion

		#region ITriggerConditions Members

		ZString ITriggerConditions.TriggerEventCode
		{
			get { return P9T_SE_NKTriggerEvent; }
			set { P9T_SE_NKTriggerEvent = value; }
		}

		ZString ITriggerConditions.TriggerFieldName
		{
			get { return P9T_TriggerField; }
			set { P9T_TriggerField = value; }
		}

		ZString IEventReferenceConditions.TriggerCondition
		{
			get { return P9T_TriggerCondition; }
			set { P9T_TriggerCondition = value; }
		}

		ZString IEventReferenceConditions.TriggerConditionValue
		{
			get { return Encoding.Unicode.GetString(P9T_TriggerConditionValue); }
			set { P9T_TriggerConditionValue = Encoding.Unicode.GetBytes(value); }
		}

		ZShort ITriggerConditions.TriggerFiredCountdown
		{
			get => P9T_TriggerFiredCountdown;
			set => P9T_TriggerFiredCountdown = value;
		}

		ZBool ITriggerConditions.Cascading => P9T_RespondToCascadedEvents;

		ZString ITriggerConditions.CascadingContext => P9T_CascadedEventsContext;

		IWorkflowDescriptor ITriggerConditions.Descriptor => Template.WorkflowDescriptor;

		BusinessObject ITriggerConditions.Job => null;

		#endregion

		#region ITemplateTrigger Members

		ZBool ITemplateTrigger.IsActive
		{
			get { return P9T_IsActive; }
			set { P9T_IsActive = value; }
		}

		IWorkflowTrigger ITemplateTrigger.GetOrCreateJobVersionOfTrigger(IBusiness job, bool createIfNotFound)
		{
			return GetOrCreateJobVersionOfTrigger(job, createIfNotFound);
		}

		ZQuery ITemplateTrigger.GetJobVersionOfTriggerQuery(IBusiness job)
		{
			return GetJobVersionOfTriggerQuery(job.Identifier, ((BusinessObject)job).TablePrefix);
		}

		ZGuid ITemplateTrigger.SourceTemplatePK => P9T_P0_Template;

		ITemplateTrigger ITemplateTrigger.Clone()
		{
			var result = Factory.New<ProcessTemplateTrigger>();

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.CopyPersistentValuesFrom(this);
				result.TemplateConditions.TemplateCondition2Value = this.TemplateConditions.TemplateCondition2Value;

				using (((IBusinessObjectCollection)TriggerActions).SuspendListChanged())
				{
					var universalTrigger = (IUniversalTemplateTrigger)this;
					foreach (var notification in this.TriggerActions.Cast<ProcessTaskNotification>())
					{
						var clonedNotification = (ProcessTaskNotification)notification.Clone();
						clonedNotification.PQ_P9T_Trigger = universalTrigger.Identifier;
						clonedNotification.PQ_EmailText = notification.PQ_EmailText;
						result.TriggerActions.Add(clonedNotification);
					}
				}
			}

			return result;
		}

		#endregion

		#region IRootTypeProvider Members

		Type[] IRootTypeProvider.RootTypes => this.GetRootTypes(typeof(ProcessJobTriggerLink));

		BusinessObject[] IRootTypeProvider.Roots => Array.Empty<BusinessObject>();

		#endregion

		#region Firing Workflow Implementation

		ProcessJobTriggerLink GetOrCreateJobVersionOfTrigger(IBusiness workflowParent, bool createIfNotFound = true)
		{
			var tablePrefix = ((BusinessObject)workflowParent).TablePrefix;
			var jobTrigger = GetJobVersionOfTrigger(workflowParent.Identifier, tablePrefix);

			if (jobTrigger == null && createIfNotFound)
			{
				jobTrigger = Factory.New<ProcessJobTriggerLink>();
				using (jobTrigger.GetValidationSuspender())
				{
					jobTrigger.P9L_P9T_TemplateTrigger = PK;
					jobTrigger.P9L_ParentId = workflowParent.Identifier;
					jobTrigger.P9L_ParentTableCode = tablePrefix;
					jobTrigger.P9L_TriggerFiredCountdown = P9T_TriggerFiredCountdown;
				}
			}

			return jobTrigger;
		}

		#endregion

		#region ITriggerUserContextConditions

		[BusinessObjectTestExclude]
		ZString ITriggerUserContextConditions.TriggerContextCode
		{
			get => TriggerUserContextList.Codes.Event;
			set => ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "{0} is the only user context currently supported by Process Template Triggers.", TriggerUserContextList.Codes.Event));
		}

		ZString ITriggerUserContextConditions.TriggerStaffCode
		{
			get => P9T_GS_NK_TriggerStaffCode;
			set => P9T_GS_NK_TriggerStaffCode = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerBranch
		{
			get => P9T_GB_TriggerBranch;
			set => P9T_GB_TriggerBranch = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerCompany
		{
			get => P9T_GC_TriggerCompany;
			set => P9T_GC_TriggerCompany = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerDepartment
		{
			get => P9T_GE_TriggerDepartment;
			set => P9T_GE_TriggerDepartment = value;
		}

		#endregion

		[ResourceStringData("ProcessTemplateTrigger.P9T_TriggerFiredCountdown", Caption = "Trigger Remaining Countdown", ShortCaption = "Countdown", FullDescription = "The number of times this trigger will be allowed to fire. When this value reaches zero, the trigger will no longer fire.")]
		public override ZShort P9T_TriggerFiredCountdown
		{
			get => base.P9T_TriggerFiredCountdown;
			set => base.P9T_TriggerFiredCountdown = value;
		}

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			P9T_Sequence = 1;
			P9T_SE_NKTriggerEvent = ZArchitecture.Business.AutoEvents.EditedARecordCode;
		}

#endif
		#endregion

		#region MCR

		IAntlrMacroContext IAntlrMacroContextProvider.GetSampleContext()
		{
			var parentType = Template.WorkflowDescriptor.WorkflowProviderType;
			return new WorkflowMacroContextDecider().GetContextForTriggerConditionsForUserInterface(parentType, null, this, parentType, null);
		}

		#endregion
	}
}
