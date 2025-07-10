using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty("Description")]
	public class ProcessTaskNotification : AutoProcessTaskNotification, IDynamicRootProvider, IProcessTaskNotification
	{
		public ProcessTaskNotification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoProcessTaskNotification.Schema
		{
			public const string OverrideEmail = "OverrideEmail";
			public const string PQ_Calc_TriggerParty = "PQ_Calc_TriggerParty";
			public const string PQ_EmailTextFallbackToTemplate = "PQ_EmailTextFallbackToTemplate";
		}

		#region Related Business Objects

		[RelatedBusinessObject("ParentProcessTask")]
		public override ZGuid PQ_P9
		{
			get { return base.PQ_P9; }
			set
			{
				var oldValue = PQ_P9;
				base.PQ_P9 = value;
				parent_DoNotTouch = null;
				if (value != oldValue && !value.IsEmpty && !IsCopying)
				{
					var parent = Parent as BusinessObject;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid PQ_P9T_Trigger
		{
			get { return base.PQ_P9T_Trigger; }
			set
			{
				base.PQ_P9T_Trigger = value;
				parent_DoNotTouch = null;
			}
		}

		public IBaseTrigger Parent => parent_DoNotTouch != null && !parent_DoNotTouch.IsDeleted ? parent_DoNotTouch : (parent_DoNotTouch = GetParent());
		internal IBaseTrigger parent_DoNotTouch;

		protected virtual IBaseTrigger GetParent()
		{
			return (IBaseTrigger)Factory.Load<ProcessTask>(PQ_P9) ?? Factory.Load<IUniversalTemplateTrigger>(PQ_P9T_Trigger);
		}

		/// <summary>
		/// The ProcessTask this completion trigger action is for. NOTE that this will be null for triggers that exist on a workflow template that are referenced by the current job (Universal Triggers).
		/// </summary>
		[Obsolete("This property is here just for ensuring related business object of PQ_P9 property. The Parent property should be used instead as the parent of a completion trigger action could be either a ProcessTasks or ProcessTemplateTrigger record.")]
		public ProcessTask ParentProcessTask => Factory.Load<ProcessTask>(PQ_P9);

		[DocumentFieldExcludeFromMap]
		public sealed override ProcessTask ProcessTask
		{
			get { return base.ProcessTask; }
		}

		#endregion

		#region Property Overrides

		#region PQ_MessagePurpose

		[List("Lookups.ProcessTaskTriggerPurposeList")]
		public override ZString PQ_MessagePurpose
		{
			get { return base.PQ_MessagePurpose; }
			set { base.PQ_MessagePurpose = value; }
		}

		protected bool PQ_MessagePurpose_ReadOnly
		{
			get { return triggersWithoutPurpose.Contains(PQ_TriggerType.ToString()) || PQ_Calc_TriggerParty_ReadOnly; } // purpose only makes sense to use if you have a recipient
		}

		static readonly string[] triggersWithoutPurpose = new[]
		{
			WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs,
			WorkflowTriggerActionTypeConstants.Codes.SendDocument,
			WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue,
			WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts,
			WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue,
			WorkflowTriggerActionTypeConstants.Codes.SendARInvoice,
			WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs,
			WorkflowTriggerActionTypeConstants.Codes.AutoPack,
			WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking,
			WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer,
			WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels,
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML,
			WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels,
			WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage,
			WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message,
		};

		#endregion

		#region PQ_EmailTextFallbackToTemplate

		[MaxLength(ProcessTaskNotification.Schema.PQ_EmailTextMaxLength)]
		public ZString PQ_EmailTextFallbackToTemplate
		{
			get
			{
				ZString result = PQ_EmailText;

				if (result.IsEmpty)
				{
					var source = SourceNotification;
					if (source != null)
					{
						return source.PQ_EmailText;
					}

					// Eventually (Maybe in two years?) we can delete this. For now there are many notifications that do not have PQ_SourceTemplateNotification set.
					var jobTrigger = Parent as IWorkflowTrigger;
					if (jobTrigger != null)
					{
						var itemTemplate = Factory.Load<ProcessTask>(jobTrigger.ParentTemplateID);
						if (itemTemplate != null)
						{
							foreach (var notificationTemplate in itemTemplate.ProcessTaskNotifications)
							{
								if (PQ_TriggerType == notificationTemplate.PQ_TriggerType &&
									PQ_Calc_TriggerParty == notificationTemplate.PQ_Calc_TriggerParty &&
									PQ_TriggerParty == notificationTemplate.PQ_TriggerParty &&
									PQ_TriggerPartyService == notificationTemplate.PQ_TriggerPartyService &&
									PQ_OH_Recipient == notificationTemplate.PQ_OH_Recipient &&
									PQ_SU_Document == notificationTemplate.PQ_SU_Document &&
									PQ_MessagePurpose == notificationTemplate.PQ_MessagePurpose &&
									PQ_EmailAddr == notificationTemplate.PQ_EmailAddr)
								{
									return notificationTemplate.PQ_EmailText;
								}
							}
						}
					}
				}

				return result;
			}
			set
			{
				if (value != PQ_EmailText)
				{
					PQ_EmailText = value;
				}
				PQ_EmailTextFallbackToTemplateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PQ_EmailTextFallbackToTemplateInfo
		{
			get { return GetZPropertyInfo(Schema.PQ_EmailTextFallbackToTemplate); }
		}

		protected bool PQ_EmailTextFallbackToTemplate_ReadOnly
		{
			get { return !OverrideEmail && !IsEConversationTriggerAction; }
		}

		#endregion

		#region PQ_EmailText

		public override ZString PQ_EmailText
		{
			get { return !IsSetFieldOrApplyTag && !IsScheduleEvent ? base.PQ_EmailText : ZString.Empty; }
			set { base.PQ_EmailText = value; }
		}

		protected bool PQ_EmailText_ReadOnly
		{
			get { return !OverrideEmail; }
		}

		#endregion

		#region PQ_SQ

		[ReadOnlyMember(nameof(PQ_SQ_ReadOnly))]
		[List("Lookups.PrintQueues")]
		public override ZGuid PQ_SQ
		{
			get { return base.PQ_SQ; }
			set { base.PQ_SQ = value; }
		}

		public bool PQ_SQ_ReadOnly
		{
			get { return PQ_Calc_TriggerParty != MessageRecipientPartyTypeList.Codes.Print; }
		}

		#endregion

		#region PQ_TriggerType

		[List("Lookups.WorkflowTriggerActionTypes")]
		public override ZString PQ_TriggerType
		{
			get { return base.PQ_TriggerType; }
			set
			{
				var oldValue = PQ_TriggerType;
				base.PQ_TriggerType = value;
				if (!IsCopying && value != oldValue)
				{
					ClearPQ_SU_DocumentIfNeeded();
					ClearPQ_SQIfNeeded();
					ClearPQ_Calc_TriggerPartyIfNeeded();
					ClearPQ_TriggerPartyServiceIfNeeded();
					ClearPQ_MessagePurposeIfNeeded();
					ClearPQ_MacroTypeCodeIfNeeded();
					ClearPQ_P0_WorkflowTemplateIfNeeded();
					ClearPQ_RelatedEntityIfNeeded();
					ClearPQ_OffsetIfNeeded();
					var parent = Parent as BusinessObject;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		bool IsEConversationTriggerAction => PQ_TriggerType.ToString().In(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage);

		public bool IsTriggerTypeXUE_or_XUS => PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML || PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

		#endregion

		#region PQ_SU_Document

		[List("Lookups.DocumentToSendList")]
		public override ZGuid PQ_SU_Document
		{
			get { return base.PQ_SU_Document; }
			set { base.PQ_SU_Document = value; }
		}

		protected bool PQ_SU_Document_ReadOnly
		{
			get
			{
				var workflowDescriptor = WorkflowDescriptor;
				if (workflowDescriptor != null)
				{
					return !workflowDescriptor.IsDocumentTriggerAction(PQ_TriggerType);
				}
				else
				{
					return PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.SendDocument && PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
				}
			}
		}

		#endregion

		#region PQ_OH_Recipient

		[ReadOnlyMember(nameof(PQ_OH_Recipient_ReadOnly))]
		public override ZGuid PQ_OH_Recipient
		{
			get { return base.PQ_OH_Recipient; }
			set { base.PQ_OH_Recipient = value; }
		}

		public bool PQ_OH_Recipient_ReadOnly
		{
			get { return !IsOtherTriggerParty; }
		}

		#endregion

		#region PQ_Calc_TriggerParty

		[List("Lookups.MessagingTriggerPartiesList")]
		[ReadOnlyMember(nameof(PQ_Calc_TriggerParty_ReadOnly))]
		[MaxLength(Schema.PQ_TriggerPartyMaxLength)]
		[ResourceStringData("Enterprise.MasterFiles.Business.ProcessTaskNotification|PQ_Calc_TriggerParty", Caption = "Recipient")]
		public ZString PQ_Calc_TriggerParty
		{
			get
			{
				return IsOtherTriggerParty ? (ZString)MessageRecipientPartyTypeList.SpecialCodes.Other : PQ_TriggerParty;
			}
			set
			{
				ZString oldValue = PQ_Calc_TriggerParty;
				value = value.TrimEndSpaceTab();
				value = value.ConvertToWesternEuropeanCharacters();
				CheckMaximumLength(PQ_Calc_TriggerPartyInfo, value);
				isOtherTriggerParty = value == MessageRecipientPartyTypeList.SpecialCodes.Other;
				if (!IsOtherTriggerParty)
				{
					PQ_TriggerParty = value;
					ClearRecipientOrganizationIfNeeded();
				}

				PQ_Calc_TriggerPartyInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePQ_Calc_TriggerParty();
					Validation.ValidatePQ_TriggerPartyService();
				}

				if (!IsCopying && oldValue != PQ_Calc_TriggerParty)
				{
					ClearEmailAddressIfNeeded();
					ClearPQ_SQIfNeeded();
					SetEmailAddressIfNeeded(Parent?.GetJob());
				}
			}
		}

		public bool PQ_Calc_TriggerParty_ReadOnly
		{
			get { return !TriggerPartyIsAValidOption; }
		}

		public ZPropertyInfo PQ_Calc_TriggerPartyInfo
		{
			get { return GetZPropertyInfo(Schema.PQ_Calc_TriggerParty); }
		}

		public bool IsOtherTriggerParty
		{
			get
			{
				if (!isOtherTriggerParty.HasValue)
				{
					isOtherTriggerParty = !PQ_OH_Recipient.IsEmpty;
				}
				return isOtherTriggerParty.Value;
			}
		}
		bool? isOtherTriggerParty;

		#endregion

		#region PQ_TriggerParty

		[List("Lookups.AlternateMessagingTriggerPartiesList")]
		[ReadOnlyMember(nameof(PQ_TriggerParty_ReadOnly))]
		public override ZString PQ_TriggerParty
		{
			get { return base.PQ_TriggerParty.ToUpperInvariant(); }
			set { base.PQ_TriggerParty = value.ToUpperInvariant(); }
		}

		public bool PQ_TriggerParty_ReadOnly
		{
			get { return !IsOtherTriggerParty; }
		}

		bool TriggerPartyIsAValidOption
		{
			get
			{
				var result = TriggerPartyIsRequired;

				if (!result)
				{
					var workflowDescriptor = WorkflowDescriptor;
					result = workflowDescriptor != null && workflowDescriptor.IsPrintingTriggerAction(PQ_TriggerType);
				}

				return result;
			}
		}

		public bool TriggerPartyIsRequired
		{
			get
			{
				var workflowDescriptor = WorkflowDescriptor;
				return workflowDescriptor != null &&
				(
					workflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(PQ_TriggerType) ||
					workflowDescriptor.IsManifestMessagingTriggerAction(PQ_TriggerType) ||
					workflowDescriptor.IsPrintingTriggerAction(PQ_TriggerType)
				);
			}
		}

		internal WorkflowDescriptor WorkflowDescriptor => Parent?.GetWorkflowDescriptor();

		#endregion

		#region PQ_TriggerPartyService

		[List("Lookups.TriggerPartyServices")]
		[ReadOnlyMember(nameof(PQ_TriggerPartyService_ReadOnly))]
		public override ZString PQ_TriggerPartyService
		{
			get { return base.PQ_TriggerPartyService; }
			set { base.PQ_TriggerPartyService = value; }
		}

		public bool PQ_TriggerPartyService_ReadOnly
		{
			get { return !IsTriggerPartyServiceAvailable; }
		}

		public bool IsTriggerPartyServiceAvailable
		{
			get { return WorkflowDescriptor?.IsRecipientServiceAvailable(PQ_TriggerType) ?? false; }
		}

		#endregion

		#region PQ_EmailAddr

		[EmailAddress]
		public override ZString PQ_EmailAddr
		{
			get { return (!IsSetFieldOrApplyTag && !ShouldActionReferenceRetrieveEmailAddressValue) || IsEmailAddressGetterRedirectionSuppressed ? base.PQ_EmailAddr : ZString.Empty; }
			set { base.PQ_EmailAddr = value; }
		}

		protected internal bool PQ_EmailAddr_ReadOnly
		{
			get
			{
				var workflowDescriptor = WorkflowDescriptor;
				return PQ_Calc_TriggerParty != MessageRecipientPartyTypeList.Codes.Email && !IsEmailAddressRetrievable(Parent?.GetJob()) || (workflowDescriptor != null && !workflowDescriptor.IsEmailSendNotificationTriggerAction(PQ_TriggerType));
			}
		}

		public bool IsEmailRelatedTriggerParty()
		{
			switch (PQ_TriggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.Email:
				case MessageRecipientPartyTypeList.Codes.PersonalEmail:
				case MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail:
				case MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail:
				case MessageRecipientPartyTypeList.Codes.CurrentUser:
				case MessageRecipientPartyTypeList.Codes.GroupOwners:
					return true;

				default:
					return false;
			}
		}

		public bool IsEmailAddressRetrievable(BusinessObject job) => job is IEmailAddressGetterForTrigger && IsPersonRelatedEmailTriggerParty;

		internal bool IsPersonRelatedEmailTriggerParty => PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.PersonalEmail ||
																											PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail ||
																											PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
		void SetEmailAddressIfNeeded(BusinessObject job)
		{
			if (job != null && IsEmailAddressRetrievable(job) && PQ_EmailAddr.IsEmpty)
			{
				PQ_EmailAddr = GetEmailAddressesFromTriggerParty(job).FirstOrDefault();
			}
		}

		public IEnumerable<string> GetSubstitutedEmailAddressesWithFallback(BusinessObject job, IStmALog @event)
		{
			var emails = !PQ_EmailAddr.IsEmpty ? new[] { PQ_EmailAddr.ToString() } : GetEmailAddressesFromTriggerParty(job);

			foreach (var email in emails)
			{
				yield return TriggerActionCommunicationModeSubstitutor.Substitute(this, job, @event, MessageDelivery.CommunicationModeSubstitutorProperty.EmailAddress, email);
			}
		}

		public string EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(BusinessObject job) => Res.GetString("14B24C54-2B94-4823-94EF-9DDD0AA79433", "Cannot send document [{0}] by EML as the evaluated Email Address of {1} in {2} is empty or invalid.",
			Document?.DocumentId, Description, job.HumanReadableName);

		internal IEnumerable<string> GetEmailAddressesFromTriggerParty(BusinessObject job)
		{
			if (job is IEmailAddressGetterForTrigger triggerEmailAddress)
			{
				return triggerEmailAddress.GetEmailAddressesFromTriggerParty(PQ_TriggerParty);
			}

			return Array.Empty<string>();
		}

		#endregion

		#region PQ_P0_WorkflowTemplate

		[ReadOnlyMember(nameof(PQ_P0_WorkflowTemplate_ReadOnly))]
		[List("Lookups.WorkflowTemplateList")]
		public override ZGuid PQ_P0_WorkflowTemplate
		{
			get { return base.PQ_P0_WorkflowTemplate; }
			set { base.PQ_P0_WorkflowTemplate = value; }
		}

		public bool PQ_P0_WorkflowTemplate_ReadOnly =>
			PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce
			&& PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;

		public override bool ReadOnly
		{
			get
			{
				var readOnly = false;
				var task = (BusinessObject)Parent;
				if (task != null)
				{
					readOnly = task.ReadOnly;
				}

				return readOnly || base.ReadOnly;
			}

			set => base.ReadOnly = value;
		}

		#endregion

		#region PQ_RelatedEntityID

		[ReadOnlyMember(nameof(PQ_RelatedEntityId_ReadOnly))]
		[List("Lookups.RelatedEntities")]
		public override ZGuid PQ_RelatedEntityId
		{
			get { return base.PQ_RelatedEntityId; }
			set
			{
				base.PQ_RelatedEntityId = value;
				PQ_RelatedEntityTableCode = TagMagnitudeSchema.Constants.Prefix;
			}
		}

		public bool PQ_RelatedEntityId_ReadOnly => PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyTag;

		public ITagMagnitude RelatedEntity => Factory.Load<ITagMagnitude>(PQ_RelatedEntityId);

		#endregion

		#endregion

		#region New Properties

		#region PQ_MacroTypeCode

		/// <summary>
		/// PQ_MacroTypeCode should be used for all macro related decisions
		/// </summary>
		[BusinessObjectTestExclude]
		[List("Lookups.MacroTypeCodeList")]
		[ResourceStringData("ProcessTaskNotification|PQ_MacroTypeCode", ShortCaption = "M. Type", Caption = "Macro Type", FullDescription = "The macro type that should be used by the process task notification")]
		public ZString PQ_MacroTypeCode
		{
			get
			{
				return IsSetField
					? base.PQ_Code1.IsEmpty ? EventReferenceConditionList.Codes.UserDefined : base.PQ_Code1
					: ZString.Empty;
			}
			set
			{
				base.PQ_Code1 = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePQ_MacroTypeCode();
				}

				PQ_MacroTypeCodeInfo.RefreshBinding();
			}
		}

		internal bool PQ_MacroTypeCode_ReadOnly
		{
			get { return !(WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.Value && WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType)); }
		}

		public ZPropertyInfo PQ_MacroTypeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PQ_MacroTypeCode)); }
		}

		public ZString MacroCodeFieldType
		{
			get
			{
				switch (PQ_MacroTypeCode)
				{
					case EventReferenceConditionList.Codes.ConditionWithMacros:
						return nameof(FieldType.AntlrMacro);

					default:
						return nameof(FieldType.TextMacro);
				}
			}
		}

		#endregion

		#region OverrideEmail

		public ZBool OverrideEmail
		{
			get
			{
				return overrideEmail
					|| (Parent != null && Parent.ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix &&
					(IsNotificationEmail || IsAssignStaffAndEmail));
			}
			set
			{
				if (!overrideEmail && value && PQ_EmailText == ZString.Empty)
				{
					PQ_EmailText = Res.GetString("91ca1be2-68b5-42b5-8387-24d7396559cd", "Enter email notification text");
				}

				if (overrideEmail && !value)
				{
					PQ_EmailText = ZString.Empty;
				}

				SetNonPersistentPropertyValue(OverrideEmailInfo, ref overrideEmail, value);
			}
		}
		protected ZBool overrideEmail;

		public ZPropertyInfo OverrideEmailInfo
		{
			get { return GetZPropertyInfo(Schema.OverrideEmail); }
		}

		protected bool OverrideEmail_ReadOnly
		{
			get { return !IsNotificationEmail; }
		}

		#endregion

		#region PQ_SourceTemplateNotification

		[ReadOnly(true)]
		[List("Lookups.TemplateSources")]
		[ResourceStringData("Enterprise.MasterFiles.Business.ProcessTaskNotification|TemplateSourcePK", Caption = "Source Template", ShortCaption = "Source", FullDescription = "The workflow template that this notification was created from.")]
		public ZGuid TemplateSourcePK
		{
			get { return SourceNotification?.Parent?.ParentID ?? ZGuid.Empty; }
		}

		public ZPropertyInfo TemplateSourcePKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TemplateSourcePK), _ => PQ_SourceTemplateNotificationInfo); }
		}

		public ProcessTaskNotification SourceNotification => Factory.Load<ProcessTaskNotification>(PQ_SourceTemplateNotification);

		public ProcessTaskTemplate TemplateSource => Factory.Load<ProcessTaskTemplate>(TemplateSourcePK);

		#endregion

		#region PQ_ECS_MessageDeliveryContextSelector

		[List("Lookups.EDIMessageDeliveryContextSelectors")]
		[ReadOnlyMember(nameof(PQ_ECS_MessageDeliveryContextSelector_ReadOnly))]
		public override ZGuid PQ_ECS_MessageDeliveryContextSelector
		{
			get => base.PQ_ECS_MessageDeliveryContextSelector;
			set => base.PQ_ECS_MessageDeliveryContextSelector = value;
		}

		protected bool PQ_ECS_MessageDeliveryContextSelector_ReadOnly => !WorkflowTriggerActionTypeConstants.IsUniversalXmlWithDeliveryContextSelector(PQ_TriggerType);

		#endregion

		#region IsSetField

		internal bool IsSetField => isSetFieldSuppressed == 0 && WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType);
		internal bool IsApplyTag => PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
		internal bool IsSetFieldOrApplyTag => IsSetField || IsApplyTag;

		internal bool IsNotificationEmail => WorkflowTriggerActionTypeConstants.IsNotificationEmail(PQ_TriggerType);

		internal bool IsAssignStaffAndEmail => PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail;

		int isSetFieldSuppressed;

		internal IDisposable SuppressIsSetFieldForSetters()
		{
			isSetFieldSuppressed++;
			return new DisposableAction(() => { if (isSetFieldSuppressed > 0) { isSetFieldSuppressed--; } });
		}

		#endregion

		#region PQ_FieldName

		[BusinessObjectTestExclude]
		[ResourceStringData("ProcessTaskNotification|PQ_FieldName", ShortCaption = "Field", Caption = "Field Name", FullDescription = "Field name for Set Field trigger action.")]
		[MaxLength(Schema.PQ_EmailAddrMaxLength)]
		public ZString PQ_FieldName
		{
			get { return IsSetFieldOrApplyTag ? base.PQ_EmailAddr : ZString.Empty; }
			set
			{
				if (PQ_FieldName != value)
				{
					using (SuppressIsSetFieldForSetters())
					{
						base.PQ_EmailAddr = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidatePQ_FieldName();
						Validation.ValidatePQ_FieldValue();
					}
					PQ_FieldNameInfo.RefreshBinding();
					PQ_FieldValueInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PQ_FieldNameInfo => GetZPropertyInfo(nameof(PQ_FieldName));

		protected bool PQ_FieldName_ReadOnly => !(WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType) || PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyTag);

		public ZString PQ_FieldNameTrimmed => Utilities.TrimExpression(PQ_FieldName);

		#endregion

		#region PQ_FieldValue

		[BusinessObjectTestExclude]
		[ResourceStringData("ProcessTaskNotification|PQ_FieldValue", ShortCaption = "Value", Caption = "Field Value", FullDescription = "Field value for Set Field trigger action.")]
		[MaxLength(Schema.PQ_EmailTextMaxLength)]
		public ZString PQ_FieldValue
		{
			get
			{
				if (IsAssignStaffAndEmail)
				{
					return base.PQ_EmailAddr;
				}
				else
				{
					return IsSetField ? base.PQ_EmailText : ZString.Empty;
				}
			}
			set
			{
				if (PQ_FieldValue != value)
				{
					using (SuppressIsSetFieldForSetters())
					{
						if (IsAssignStaffAndEmail)
						{
							base.PQ_EmailAddr = value;
						}
						else
						{
							base.PQ_EmailText = value;
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidatePQ_FieldValue();
					}
					PQ_FieldValueInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PQ_FieldValueInfo => GetZPropertyInfo(nameof(PQ_FieldValue));

		protected bool PQ_FieldValue_ReadOnly => !WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType) && !IsAssignStaffAndEmail;

		public ZString PQ_FieldValueTrimmed => Utilities.TrimExpression(PQ_FieldValue);

		public bool IsFieldValueValidClause => new MacroClauseProcessor(null, null).IsValueClause(PQ_FieldValueTrimmed);

		#endregion

		#region StaffCode

		[List("Lookups.StaffList")]
		[BusinessObjectTestExclude]
		[ResourceStringData("ProcessTaskNotification|StaffCode", ShortCaption = "Staff", Caption = "Staff", FullDescription = "Staff for setting email address.")]
		[MaxLength(AutoGlbStaff.Schema.GS_CodeMaxLength)]
		public ZString StaffCode
		{
			get { return staffCode; }
			set
			{
				if (staffCode != value)
				{
					SetNonPersistentPropertyValue(StaffCodeInfo, ref staffCode, value);
					Validation.ValidateStaffCode();

					if (!StaffCodeInfo.HasErrors() && !staffCode.IsEmpty)
					{
						var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, value);
						PQ_EmailAddr = staff?.GS_EmailAddress ?? ZString.Empty;
					}
				}
			}
		}
		ZString staffCode;

		protected internal bool StaffCode_ReadOnly => PQ_Calc_TriggerParty != MessageRecipientPartyTypeList.Codes.Email;

		public ZPropertyInfo StaffCodeInfo => GetZPropertyInfo(nameof(StaffCode));

		#endregion

		#region IsScheduleEvent

		internal bool IsScheduleEvent
		{
			get { return isScheduleEventSuppressed == 0 && WorkflowTriggerActionTypeConstants.IsScheduleEvent(PQ_TriggerType); }
		}

		int isScheduleEventSuppressed;

		internal IDisposable SuppressIsScheduleEventForSetters()
		{
			isScheduleEventSuppressed++;
			return new DisposableAction(() => { if (isScheduleEventSuppressed > 0) { isScheduleEventSuppressed--; } });
		}

		#endregion

		#region PQ_ActionReference

		[BusinessObjectTestExclude]
		[ResourceStringData("ProcessTaskNotification|PQ_ActionReference", ShortCaption = "Reference", Caption = "Action Reference", FullDescription = "Free text action reference to distinguish between DLY completion trigger actions.")]
		[MaxLength(Schema.PQ_EmailTextMaxLength)]
		public ZString PQ_ActionReference
		{
			get { return ShouldActionReferenceRetrieveEmailAddressValue ? base.PQ_EmailAddr : ZString.Empty; }
			set
			{
				if (PQ_ActionReference != value)
				{
					using (SuppressIsScheduleEventForSetters())
					using (SuppressEmailAddressGetterRedirection())
					{
						base.PQ_EmailAddr = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidatePQ_ActionReference();
					}
					PQ_ActionReferenceInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PQ_ActionReferenceInfo => GetZPropertyInfo(nameof(PQ_ActionReference));

		protected bool PQ_ActionReference_ReadOnly => !WorkflowTriggerActionTypeConstants.IsScheduleEvent(PQ_TriggerType) && PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse;

		bool ShouldActionReferenceRetrieveEmailAddressValue => IsScheduleEvent || PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse;

		bool IsEmailAddressGetterRedirectionSuppressed => suppressionCountForEmailAddressValueGetter > 0;

		IDisposable SuppressEmailAddressGetterRedirection()
		{
			++suppressionCountForEmailAddressValueGetter;
			return new DisposableAction(() =>
			{
				if (suppressionCountForEmailAddressValueGetter > 0)
				{
					--suppressionCountForEmailAddressValueGetter;
				}
			});
		}

		int suppressionCountForEmailAddressValueGetter;

		#endregion

		#region PQ_Offset

		[ZDateTimeDurationValue]
		public override ZDateTime PQ_Offset
		{
			get => base.PQ_Offset;
			set
			{
				var durationValue = value.ConvertToDurationBasedDate(PQ_OffsetInfo);
				if (PQ_Offset != durationValue)
				{
					base.PQ_Offset = durationValue;
				}
				if (!IsValidationSuspended)
				{
					if (Parent is IUniversalTemplateTrigger universalTrigger)
					{
						universalTrigger.ValidateShouldTriggerOnEstimateEvents();
					}
				}
			}
		}

		protected bool PQ_Offset_ReadOnly => !WorkflowTriggerActionTypeConstants.IsScheduleEvent(PQ_TriggerType);

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ClearPQ_SU_DocumentIfNeeded();
			SetOverrideEmailLocalVariable();
		}

		#endregion

		#region Clone, CopyPersistentValuesFrom and CopyValuesFrom

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			List<string> excludedProperties = new List<string>();
			if (!WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType) // Do not use IsSetField as it can be suppressed
				&& !WorkflowTriggerActionTypeConstants.IsScheduleEvent(PQ_TriggerType))
			{
				excludedProperties.Add(ProcessTaskNotificationSchema.Constants.PQ_EmailText);
			}
			excludedProperties.Add(ProcessTaskNotificationSchema.Constants.PQ_P9);
			args.AddExcludedColumns(excludedProperties);

			using (SuppressIsSetFieldForSetters())
			using (SuppressIsScheduleEventForSetters())
			{
				var clonedObj = base.CloneInternal(args) as ProcessTaskNotification;

				if (IsApplyTag)
				{
					using (clonedObj.GetValidationSuspender())
					using (clonedObj.SuspendSettingHasChanges())
					{
						clonedObj.PQ_FieldName = PQ_FieldName;
					}
				}

				if (ShouldActionReferenceRetrieveEmailAddressValue)
				{
					using (clonedObj.SuspendSettingHasChanges())
					{
						clonedObj.PQ_EmailAddr = PQ_ActionReference.Substring(0, Schema.PQ_EmailAddrMaxLength);
					}
				}

				return clonedObj;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();

			RefreshReadOnlyForAllProperties();
			var job = Parent?.GetJob();
			if (job != null)
			{
				SetEmailAddressIfNeeded(job);
			}
			HasChanges = false;
		}

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsNonPersistedRepresentationOfTemplateTriggerAction); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (PQ_EmailText == Res.GetString("91ca1be2-68b5-42b5-8387-24d7396559cd", "Enter email notification text"))
			{
				PQ_EmailText = ZString.Empty;
			}
		}

		#endregion

		#region Milestones and Exceptions

		public bool IsMatchForItemTemplateMerge(ProcessTaskNotification itemTemplate) => new TemplateNotificationComparer().Compare(this, itemTemplate) == 0;

		#endregion

		#region RefreshReadOnlyForAllProperties

		public void RefreshReadOnlyForAllProperties()
		{
			ClearPQ_SU_DocumentIfNeeded();
			ClearPQ_Calc_TriggerPartyIfNeeded();
			ClearPQ_TriggerPartyServiceIfNeeded();
			SetOverrideEmailLocalVariable();
			ClearPQ_MessagePurposeIfNeeded();
			ClearPQ_MacroTypeCodeIfNeeded();
			ClearPQ_OffsetIfNeeded();
		}

		#endregion

		#region Implementation

		bool IsNonPersistedRepresentationOfTemplateTriggerAction
		{
			get
			{
				if (PQ_P9.IsEmpty)
				{
					return false;
				}

				ZBool? isNonPersisted = ((ProcessTask)Parent)?.IsNonPersistedRepresentationOfTemplateTrigger;
				return isNonPersisted.HasValue && isNonPersisted.Value;
			}
		}

		void ClearRecipientOrganizationIfNeeded()
		{
			if (PQ_OH_Recipient.IsValid)
			{
				PQ_OH_Recipient = ZGuid.Empty;
			}
		}

		void ClearEmailAddressIfNeeded()
		{
			if (!PQ_EmailAddr.IsEmpty && PQ_EmailAddr_ReadOnly)
			{
				PQ_EmailAddr = ZString.Empty;
			}
		}

		void ClearPQ_SU_DocumentIfNeeded()
		{
			if (PQ_SU_Document.IsValid && PQ_SU_Document_ReadOnly)
			{
				PQ_SU_Document = ZGuid.Empty;
			}
		}

		void ClearPQ_SQIfNeeded()
		{
			if (PQ_SQ.IsValid && (PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.SendDocument || PQ_Calc_TriggerParty != MessageRecipientPartyTypeList.Codes.Print))
			{
				PQ_SQ = ZGuid.Empty;
			}
		}

		void ClearPQ_Calc_TriggerPartyIfNeeded()
		{
			if (!PQ_Calc_TriggerParty.IsEmpty && !TriggerPartyIsAValidOption)
			{
				PQ_Calc_TriggerParty = ZString.Empty;
			}
		}

		void ClearPQ_TriggerPartyServiceIfNeeded()
		{
			if (!PQ_TriggerPartyService.IsEmpty && !IsTriggerPartyServiceAvailable)
			{
				PQ_TriggerPartyService = ZString.Empty;
			}
		}

		void ClearPQ_MessagePurposeIfNeeded()
		{
			if (!PQ_MessagePurpose.IsEmpty && PQ_MessagePurpose_ReadOnly)
			{
				PQ_MessagePurpose = ZString.Empty;
			}
		}

		void ClearPQ_MacroTypeCodeIfNeeded()
		{
			if (!PQ_MacroTypeCode.IsEmpty && PQ_MacroTypeCode_ReadOnly)
			{
				PQ_MacroTypeCode = ZString.Empty;
			}
		}

		void ClearPQ_OffsetIfNeeded()
		{
			if (!PQ_Offset.IsEmpty && PQ_Offset_ReadOnly)
			{
				PQ_Offset = ZDateTime.Empty;
			}
		}

		void ClearPQ_P0_WorkflowTemplateIfNeeded()
		{
			if (PQ_P0_WorkflowTemplate.IsValid
				&& PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce
				&& PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways)
			{
				PQ_P0_WorkflowTemplate = ZGuid.Empty;
			}
		}

		void ClearPQ_RelatedEntityIfNeeded()
		{
			if (PQ_RelatedEntityId.IsValid && PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.ApplyTag)
			{
				PQ_RelatedEntityId = ZGuid.Empty;
				PQ_RelatedEntityTableCode = ZString.Empty;
			}
		}

		protected virtual void SetOverrideEmailLocalVariable()
		{
			overrideEmail = (PQ_EmailText != (ZString)null && PQ_EmailText != ZString.Empty);
		}

		#endregion

		#region IRootTypeProvider Members

		Type[] IRootTypeProvider.RootTypes
		{
			// Roots and RootTypes is a leaky abstraction given that during writing execution we have different data available. (We don't have the StmALog instance until the notification is executing, nor do we have the job on the ProcessTaskTemplate)
			// TODO: Fix this abstraction.
			get
			{
				if (Parent is IRootTypeProvider parent)
				{
					var roots = parent.RootTypes;
					if (roots.Any())
					{
						return roots.Concat(GetMacroTypes(parent))
									.Append(typeof(StmALog))
									.ToArray();
					}
				}
				return Array.Empty<Type>();
			}
		}

		IEnumerable<Type> GetMacroTypes(IRootTypeProvider parent)
		{
			if (parent is ProcessTask)
			{
				return new[] { GetType() };
			}
			else
			{
				var descriptor = GetDescriptorForLine(parent);
				return descriptor?.MacroTypes(this) ?? new[] { GetType() };
			}
		}

		BusinessObject[] IRootTypeProvider.Roots => ((IDynamicRootProvider)this).AugmentedRoots(Parent?.GetJob());

		BusinessObject[] IDynamicRootProvider.AugmentedRoots(BusinessObject job)
		{
			if (Parent is IDynamicRootProvider parent && job != null)
			{
				var roots = parent.AugmentedRoots(job);
				if (roots.Any())
				{
					return roots.Concat(GetMacroRoots(parent)).ToArray();
				}
			}
			return Array.Empty<BusinessObject>();
		}

		IEnumerable<BusinessObject> GetMacroRoots(IRootTypeProvider parent)
		{
			return GetDescriptorForLine(parent)?.MacroRoots(this) ?? new[] { this };
		}

		WorkflowDescriptor GetDescriptorForLine(IRootTypeProvider parent)
		{
			if (parent is ILineTriggerSupport line && WorkflowDescriptors.Instance.TryGetValue(line.LineTriggerType, out var descriptor))
			{
				return descriptor;
			}
			return WorkflowDescriptor;
		}

		#endregion

		#region ITriggerAction Members

		ZString ITriggerAction.ActionType
		{
			get { return PQ_TriggerType; }
			set { PQ_TriggerType = value; }
		}

		TriggerRunLocation ITriggerAction.RunLocation => ((ITriggerAction)this).ActionType == WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange
															? TriggerRunLocation.Client
															: TriggerRunLocation.Server;

		#endregion

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (PQ_P9.IsEmpty)
			{
				PQ_P9 = Factory.NewWithValidTestData<ProcessTask>().PK; // Satisfies db constraint where one of PQ_P9 OR PQ_P9T_Trigger must be not null, even though both columns are nullable.
			}
		}

#endif
		#endregion

		#region Log

		public string GetDiagnosticLogInfo()
		{
			var result = new StringBuilder();
			var parentLogInfo = Parent?.GetDiagnosticLogInfo();

			if (!string.IsNullOrEmpty(parentLogInfo))
			{
				result.AppendLine(parentLogInfo);
			}

			result.AppendFormat(CultureInfo.InvariantCulture, "{0} '{1}' (PK: {2})", HumanReadableName, PQ_TriggerType, PK);
			if (WorkflowTriggerActionTypeConstants.IsSetField(PQ_TriggerType))
			{
				result.AppendFormat(CultureInfo.InvariantCulture, (NoResString)", Field Name: ");
				result.Append(PQ_FieldName);
				result.AppendFormat(CultureInfo.InvariantCulture, (NoResString)", Field Value: ");
				result.Append(PQ_FieldValue);
			}

			return result.ToString();
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var parent = Parent;
				if (parent == null || parent.IsDeleted)
				{
					return base.HumanReadableNameCore;
				}
				else if (parent.IsTrigger())
				{
					return Res.GetString("b78933e7-36f2-4104-b9e3-d9f28a3c9a30", "Trigger Action - {0}", parent.Description);
				}
				else if (parent.IsMilestone())
				{
					return Res.GetString("fb92a419-ab2a-4037-b85c-53ea3d8f58de", "Milestone Action - {0}", parent.Description);
				}
				else
				{
					return Res.GetString("d659ccf8-2430-4e8b-948e-d9dbc3e27881", "Action - {0}", parent.Description);
				}
			}
		}

		public ZString Description => IsDeleted ? ZString.Empty : HumanReadableNameCore;

		public ZString Code => IsDeleted ? ZString.Empty : HumanReadableNameCore;

		#endregion
	}
}
