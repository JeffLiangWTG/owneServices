using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionsViewModel : NonPersistentBusinessObject<TriggerConditionsViewModelValidation>, ITriggerConditions
	{
		public TriggerConditionsViewModel(IBaseTrigger trigger)
			: base(trigger.Factory)
		{
			Trigger = trigger;
		}

		internal IBaseTrigger Trigger { get; }

		#region Properties

		#region TriggerEventCode

		[MaxLength(3)]
		[List("Lookups.MilestoneEventTypes")]
		[RelatedBusinessObject("TriggerEvent")]
		[ResourceStringData("TriggerConditionsViewModel.TriggerEventCode", Caption = "Event Code", FullDescription = "The event associated with the workflow item. If you specify a value for both \"Event\" and \"Trigger Field\", either one of these occurring will cause the trigger actions to run.")]
		public ZString TriggerEventCode
		{
			get
			{
				return Trigger.TriggerEventCode;
			}
			set
			{
				Trigger.TriggerEventCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerEventCode();
				}

				TriggerEventCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerEventCodeInfo => GetZPropertyInfo(nameof(TriggerEventCode));

		protected bool TriggerEventCode_ReadOnly => Trigger.IsTask() || Trigger.ShouldFieldBeReadonly(TriggerEventCodeInfo);

		public StmEvent TriggerEvent
		{
			get { return Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, TriggerEventCode); }
		}

		#endregion

		#region TriggerEventDescription

		[ResourceStringData("TriggerConditionsViewModel.TriggerEventDescription", Caption = "Event Description", ShortCaption = "Event Desc.")]
		public ZString TriggerEventDescription
		{
			get { return Events.GetMultilingualDescription(TriggerEventCode); }
		}

		public ZPropertyInfo TriggerEventDescriptionInfo => GetZPropertyInfo(nameof(TriggerEventDescription));

		#endregion

		public ZString TriggerEventCodeDescription => TriggerEventCode != ZString.Empty ? new ZString($"{TriggerEventCode}-{TriggerEventDescription}") : ZString.Empty;

		#region TriggerFieldName

		[List("Lookups.WorkflowTriggerFieldNames")]
		[MaxLength("TriggerFieldNameMaxLength")]
		[ResourceStringData("TriggerConditionsViewModel.TriggerFieldName", Caption = "Trigger Field", FullDescription = "Specifies that the trigger actions will be run when the value of this field changes. If you specify a value for both \"Event\" and \"Trigger Field\", either one of these occurring will cause the trigger action to run.")]
		public ZString TriggerFieldName
		{
			get
			{
				return Trigger.TriggerFieldName;
			}
			set
			{
				CheckMaximumLength(TriggerFieldNameInfo, value);
				Trigger.TriggerFieldName = value;
				PropertyChangeSubscriptionList.GetInstance(Factory).NotifyWorkflowTriggerFieldChanged();

				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerFieldName();
				}

				TriggerFieldNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerFieldNameInfo => GetZPropertyInfo(nameof(TriggerFieldName));

		protected int TriggerFieldNameMaxLength
		{
			get
			{
				if (triggerFieldMaxLength == null || lastWorkflowTypeForTriggerFieldMaxLength != Trigger.WorkflowProcessType)
				{
					var newMaxLength = -1;

					foreach (ICodeDescription item in Lookups.WorkflowTriggerFieldNames)
					{
						newMaxLength = Math.Max(newMaxLength, item.Code.Length);
					}

					if (Trigger.ParentID.IsEmpty)
					{
						triggerFieldMaxLength = ProcessTasksSchema.P9_TriggerField.MaxLength;
					}
					else
					{
						triggerFieldMaxLength = newMaxLength == -1 ? 20 : newMaxLength;
					}

					lastWorkflowTypeForTriggerFieldMaxLength = Trigger.WorkflowProcessType;
				}

				return triggerFieldMaxLength.Value;
			}
		}

		protected bool TriggerFieldName_ReadOnly => Trigger.ShouldFieldBeReadonly(TriggerFieldNameInfo);

		int? triggerFieldMaxLength;
		string lastWorkflowTypeForTriggerFieldMaxLength;

		#endregion

		#region TriggerCondition

		[MaxLength(3)]
		[List("Lookups.TriggerConditionList")]
		[ResourceStringData("TriggerConditionsViewModel.TriggerCondition", Caption = "Trigger Condition", ShortCaption = "Trg. Cond.", FullDescription = "The condition used to restrict whether the trigger fires when the relevant event is raised or field value changes.")]
		public ZString TriggerCondition
		{
			get
			{
				return Trigger.TriggerCondition;
			}
			set
			{
				Trigger.TriggerCondition = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerCondition();
				}

				TriggerConditionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerConditionInfo => GetZPropertyInfo(nameof(TriggerCondition));

		protected bool TriggerCondition_ReadOnly => Trigger.ShouldFieldBeReadonly(TriggerConditionInfo);

		#endregion

		#region TriggerConditionValue

		[MaxLength(nameof(TriggerConditionValueMaxLength))]
		[RootTypeProvider(nameof(GetTriggerConditionValueRootTypes), nameof(GetTriggerConditionValueRoots))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerConditionValue", Caption = "Trigger Condition Value", ShortCaption = "Trg. Cond. Value", FullDescription = "The value used in conjunction with the Trigger Condition to restrict whether the trigger fires when the relevant event is raised or field value changes.")]
		public ZString TriggerConditionValue
		{
			get
			{
				return Trigger.TriggerConditionValue;
			}
			set
			{
				CheckMaximumLength(TriggerConditionValueInfo, value);
				SetTriggerConditionValue(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerConditionValue();
				}

				TriggerConditionValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerConditionValueInfo => GetZPropertyInfo(nameof(TriggerConditionValue));

		public int TriggerConditionValueMaxLength => TriggerCondition == EventReferenceConditionList.Codes.ConditionWithMacros || TriggerCondition == EventReferenceConditionList.Codes.UserDefined
			? ProcessTask.MacroTriggerConditionValueMaxLength
			: ProcessTask.TriggerConditionValueMaxLength;

		protected bool TriggerConditionValue_ReadOnly => CanEnterTriggerConditionValue || Trigger.ShouldFieldBeReadonly(TriggerConditionValueInfo);

		internal bool CanEnterTriggerConditionValue => !HasEventReferenceTriggerCondition && !HasExceptionReferenceTriggerCondition;

		protected virtual void SetTriggerConditionValue(ZString newValue)
		{
			Trigger.TriggerConditionValue = newValue;
		}

		public bool HasEventReferenceTriggerCondition => IsEventReferenceTriggerCondition(TriggerCondition);

		public bool HasExceptionReferenceTriggerCondition => IsExceptionReferenceTriggerCondition(TriggerCondition);

		public static bool IsEventReferenceTriggerCondition(ZString triggerCondition)
		{
			switch (triggerCondition)
			{
				case EventReferenceConditionList.Codes.EventReference:
				case EventReferenceConditionList.Codes.EventReferenceWithWildcards:
				case EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions:
				case EventReferenceConditionList.Codes.EventReferenceParameters:
				case EventReferenceConditionList.Codes.ConditionWithMacros:
				case EventReferenceConditionList.Codes.UserDefined:
					return true;

				default:
					return false;
			}
		}

		public static bool IsExceptionReferenceTriggerCondition(ZString triggerCondition)
		{
			switch (triggerCondition)
			{
				case ExceptionActionConditionList.Codes.ExceptionType:
				case ExceptionActionConditionList.Codes.EventType:
					return true;

				default:
					return false;
			}
		}

		public Type[] GetTriggerConditionValueRootTypes()
		{
			if (this.IsTemplate)
			{
				var descriptor = Trigger.Descriptor;
				if (descriptor == null)
				{
					return Array.Empty<Type>();
				}
				else
				{
					return new[]
					{
						descriptor.WorkflowProviderType,
						typeof(ProcessTask),
						typeof(StmALog)
					};
				}
			}
			else
			{
				return GetTriggerConditionValueRoots().Select(t => t.GetType())
					.Append(typeof(StmALog))
					.ToArray();
			}
		}

		public BusinessObject[] GetTriggerConditionValueRoots()
		{
			if (IsTemplate)
			{
				return Array.Empty<BusinessObject>();
			}
			else
			{
				var descriptor = Trigger.Descriptor;
				return new[] { Trigger.Job }
					.Concat(descriptor.GetUDFMacroDataContext(Trigger, Trigger.Job))
					.WhereNotNull()
					.ToArray();
			}
		}

		#endregion

		#region TriggerConditionValueFieldType

		public ZString TriggerConditionValueFieldType
		{
			get
			{
				switch (TriggerCondition)
				{
					case EventReferenceConditionList.Codes.EventReferenceParameters:
						return nameof(FieldType.TextCodeFindBox);

					case EventReferenceConditionList.Codes.UserDefined:
						return nameof(FieldType.TextMacro);

					case EventReferenceConditionList.Codes.ConditionWithMacros:
						return nameof(FieldType.AntlrMacro);

					default:
						return nameof(FieldType.Text);
				}
			}
		}

		public ZPropertyInfo TriggerConditionValueFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TriggerConditionValueFieldType)); }
		}

		#endregion

		#region TriggerCountDown

		[ReadOnlyMember(nameof(TriggerFiredCountdown_ReadOnly))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerFiredCountdown", Caption = "Trigger Remaining Countdown", ShortCaption = "Countdown", FullDescription = "The number of times this trigger will be allowed to fire. When this value reaches zero, the trigger will no longer fire.")]
		public ZShort TriggerFiredCountdown
		{
			get => Trigger.TriggerFiredCountdown;
			set
			{
				Trigger.TriggerFiredCountdown = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerFiredCountdown();
				}

				TriggerFiredCountdownInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerFiredCountdownInfo => GetZPropertyInfo(nameof(TriggerFiredCountdown));

		public bool TriggerFiredCountdown_ReadOnly => !(Trigger is ProcessTask) || Trigger.ShouldFieldBeReadonly(TriggerFiredCountdownInfo);

		#endregion

		#region TriggerContext

		[MaxLength(3)]
		[List("Lookups.TriggerUserContexts")]
		[ReadOnlyMember(nameof(TriggerContextCode_ReadOnly))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerContextCode", Caption = "Action User Context", ShortCaption = "User Context", FullDescription = "This controls how the User Context is determined when processing Completion Trigger Actions.")]
		public virtual ZString TriggerContextCode
		{
			get => Trigger.TriggerContextCode;
			set
			{
				if (Trigger.TriggerContextCode != value)
				{
					Trigger.TriggerContextCode = value;
					ClearTriggerContext();
					if (!IsValidationSuspended)
					{
						Validation.ValidateTriggerContextCode();
					}
					TriggerContextCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TriggerContextCodeInfo => GetZPropertyInfo(nameof(TriggerContextCode));

		public bool TriggerContextCode_ReadOnly
		{
			get
			{
				var task = Trigger as ProcessTask;
				if (task != null && task.HasProcessJobTriggerLink && task.IsNonPersistedRepresentationOfTemplateTrigger)
				{
					return true;
				}

				return !WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.Value;
			}
		}

		#endregion

		#region TriggerBranch

		[List("Lookups.TriggerBranches")]
		[RelatedBusinessObject("TriggerBranchBizo")]
		[ReadOnlyMember(nameof(TriggerBranch_ReadOnly))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerBranch", Caption = "Branch Context", ShortCaption = "Branch", FullDescription = "This determines the Branch to be used when processing Completion Trigger Actions.")]
		public ZGuid TriggerBranch
		{
			get => Trigger.TriggerBranch;
			set
			{
				Trigger.TriggerBranch = value;
				Trigger.TriggerCompany = Factory.Load<GlbBranch>(value)?.GB_GC ?? ZGuid.Empty;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerBranch();
				}
				TriggerCompanyInfo.RefreshBinding();
				TriggerBranchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerBranchInfo => GetZPropertyInfo(nameof(TriggerBranch));

		public bool TriggerBranch_ReadOnly => TriggerContextCode_ReadOnly || !TriggerUserContextList.IsTypeNeedingUserContext(TriggerContextCode);

		public GlbBranch TriggerBranchBizo => Factory.Load<GlbBranch>(TriggerBranch);

		#endregion

		#region TriggerCompany

		[List("Lookups.TriggerCompanies")]
		[RelatedBusinessObject("TriggerCompanyBizo")]
		[ReadOnlyMember(nameof(TriggerCompany_ReadOnly))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerCompany", Caption = "Company Context", ShortCaption = "Company", FullDescription = "This determines the Company to be used when processing Completion Trigger Actions.")]
		public ZGuid TriggerCompany
		{
			get => Trigger.TriggerCompany;
			set
			{
				if (value != Trigger.TriggerCompany)
				{
					Trigger.TriggerBranch = ZGuid.Empty;
				}
				Trigger.TriggerCompany = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerCompany();
				}
				TriggerCompanyInfo.RefreshBinding();
				TriggerBranchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerCompanyInfo => GetZPropertyInfo(nameof(TriggerCompany));

		public bool TriggerCompany_ReadOnly => TriggerContextCode_ReadOnly || !TriggerUserContextList.IsTypeNeedingUserContext(TriggerContextCode);

		public GlbCompany TriggerCompanyBizo => Factory.Load<GlbCompany>(TriggerCompany);

		#endregion

		#region TriggerDepartment

		[List("Lookups.TriggerDepartments")]
		[RelatedBusinessObject("DepartmentBizo")]
		[ReadOnlyMember(nameof(TriggerDepartment_ReadOnly))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerDepartment", Caption = "Department Context", ShortCaption = "Department", FullDescription = "This determines the Department to be used when processing Completion Trigger Actions.")]
		public ZGuid TriggerDepartment
		{
			get => Trigger.TriggerDepartment;
			set
			{
				Trigger.TriggerDepartment = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerDepartment();
				}
				TriggerDepartmentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerDepartmentInfo => GetZPropertyInfo(nameof(TriggerDepartment));

		public bool TriggerDepartment_ReadOnly => TriggerContextCode_ReadOnly || !TriggerUserContextList.IsTypeNeedingUserContext(TriggerContextCode);

		public GlbDepartment DepartmentBizo => Factory.Load<GlbDepartment>(TriggerDepartment);

		#endregion

		#region TriggerStaffCode

		[MaxLength(3)]
		[List("Lookups.TriggerStaff")]
		[RelatedBusinessObject("TriggerStaff")]
		[ReadOnlyMember(nameof(TriggerStaffCode_ReadOnly))]
		[ListColumnCaption(typeof(TriggerConditionsViewModel), nameof(TriggerStaffCode_ListCaption))]
		[ResourceStringData("TriggerConditionsViewModel.TriggerStaffCode", Caption = "Staff Code", ShortCaption = "Staff", FullDescription = "The assigned or responsible staff member.")]
		public ZString TriggerStaffCode
		{
			get => Trigger.TriggerStaffCode;
			set
			{
				Trigger.TriggerStaffCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTriggerStaffCode();
				}
				TriggerStaffCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TriggerStaffCodeInfo => GetZPropertyInfo(nameof(TriggerStaffCode));

		public bool TriggerStaffCode_ReadOnly
		{
			get
			{
				if (Trigger is ProcessTask task && task.IsMilestone())
				{
					return false;
				}
				else
				{
					return TriggerContextCode_ReadOnly || !TriggerUserContextList.IsTypeNeedingUserContext(TriggerContextCode);
				}
			}
		}

		public static string TriggerStaffCode_ListCaption() => Res.GetString("94b640c0-cc92-437c-9285-eec02a3cadd5", "Staff (Full Name)");

		public GlbStaff TriggerStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, TriggerStaffCode);

		#endregion

		#region ITriggerConditions

		public IWorkflowDescriptor Descriptor => Trigger.Descriptor;

		public BusinessObject Job => Trigger.Job;

		public ZBool Cascading => Trigger.Cascading;

		public ZString CascadingContext => Trigger.CascadingContext;

		#endregion

		#endregion

		#region Lookups

		public TriggerConditionsViewModelLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = CreateLookups();
				}

				return lookups;
			}
		}

		TriggerConditionsViewModelLookups lookups;

		protected virtual TriggerConditionsViewModelLookups CreateLookups()
		{
			return new TriggerConditionsViewModelLookups(this);
		}

		#endregion

		#region ClearTriggerContext

		void ClearTriggerContext()
		{
			if (TriggerDepartment_ReadOnly)
			{
				TriggerDepartment = ZGuid.Empty;
			}

			if (TriggerBranch_ReadOnly)
			{
				TriggerBranch = ZGuid.Empty;
			}

			if (TriggerCompany_ReadOnly)
			{
				if (IsTemplate || TriggerContextCode.Equals(TriggerUserContextList.Codes.Event))
				{
					TriggerCompany = ZGuid.Empty;
				}
				else
				{
					TriggerCompany = GlbCompany.CurrentCompany.PK;
				}
			}

			if (TriggerStaffCode_ReadOnly)
			{
				TriggerStaffCode = ZString.Empty;
			}
		}

		public bool IsTemplate => Trigger.ParentTableCode.Equals(ProcessTaskTemplateSchema.Constants.Prefix);

		#endregion

		#region Validation

		public override TriggerConditionsViewModelValidation GetNewValidation()
		{
			return new TriggerConditionsViewModelValidation(this);
		}

		#endregion

		#region Implementation

		public Regex TemplateRegex
		{
			get
			{
				if (templateRegex == null)
				{
					if (TriggerCondition == EventReferenceConditionList.Codes.EventReferenceWithWildcards)
					{
						templateRegex = TriggerConditionRegexProvider.GetEventReferenceWithWildcardsRegex(Trigger);
					}
					else
					{
						templateRegex = new Regex(string.Concat('^', TriggerConditionValue, '$'), RegexOptions.IgnoreCase);
					}
				}
				return templateRegex;
			}
		}

		Regex templateRegex;

		internal void ResetTemplateRegex()
		{
			templateRegex = null;
		}

		#endregion
	}
}
