//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTaskNotificationValidation
//
//    This class should be used for overriding validation in AutoProcessTaskNotificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskNotificationValidation : AutoProcessTaskNotificationValidation
	{
		public ProcessTaskNotificationValidation(AutoProcessTaskNotification parent)
			: base(parent)
		{
		}

		ProcessTaskNotification processTaskNotification
		{
			get { return (ProcessTaskNotification)Parent; }
		}

		#region PQ_Calc_TriggerParty

		public void ValidatePQ_Calc_TriggerParty()
		{
			((IValidationInternals)this).Validate(processTaskNotification.PQ_Calc_TriggerPartyInfo, GetPQ_Calc_TriggerPartyValidationInvoker());
		}

		RunValidationInvoker GetPQ_Calc_TriggerPartyValidationInvoker()
		{
			return delegate
			{
				CheckPQ_Calc_TriggerParty();
			};
		}

		void CheckPQ_Calc_TriggerParty()
		{
			ListValidation.ErrorIfInvalidCode(processTaskNotification.PQ_Calc_TriggerPartyInfo);

			if (processTaskNotification.TriggerPartyIsRequired)
			{
				MandatoryValidation.CheckEntered(processTaskNotification.PQ_Calc_TriggerPartyInfo);
			}

			var trigger = processTaskNotification.Parent;
			processTaskNotification.WorkflowDescriptor?.CheckWorkflowTriggerRecipientParty(trigger, trigger.GetJob(), processTaskNotification.PQ_Calc_TriggerPartyInfo);
			CheckJobLevelWorkflowGroup(processTaskNotification.PQ_Calc_TriggerPartyInfo, trigger);

			ValidatePQ_TriggerParty();
			ValidatePQ_OH_Recipient();
			ValidatePQ_EmailAddr();
		}

		void CheckJobLevelWorkflowGroup(ZPropertyInfo info, IBaseTrigger trigger)
		{
			if (processTaskNotification.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup)
			{
				var job = (IWorkflowProvider)trigger?.GetJob();

				if (job != null)
				{
					var template = job as ProcessTaskTemplate;
					var workflowType = template == null ? job.WorkflowType : template.P0_ProcessType;

					if (!ProcessJobHeaderProvider.SupportsPAVE(workflowType, ((IBusiness)job).Factory))
					{
						info.AddWarning(Res.GetString("5780253d-8829-4af3-9aae-40f5225ba271", "Buffer Management is not enabled for this job type. The Job-level Workflow Group option is only valid for jobs associated with a Buffer Management System. Fallback logic will be used to determine an alternative recipient."));
					}
				}
			}
		}

		#endregion

		#region PQ_MessagePurpose

		protected override void CheckPQ_MessagePurpose()
		{
			base.CheckPQ_MessagePurpose();
			ListValidation.ErrorIfInvalidCode(Parent.PQ_MessagePurposeInfo);
		}

		#endregion

		#region PQ_MacroTypeCode

		public void ValidatePQ_MacroTypeCode()
		{
			ValidateCalculatedProperty(processTaskNotification.PQ_MacroTypeCodeInfo);
		}

		protected virtual void CheckPQ_MacroTypeCode()
		{
			if (!processTaskNotification.PQ_MacroTypeCode_ReadOnly)
			{
				base.CheckPQ_Code1();
				ListValidation.ErrorIfInvalidCode(processTaskNotification.PQ_MacroTypeCodeInfo);
			}
		}

		#endregion

		#region PQ_SU_Document

		protected override void CheckPQ_SU_Document()
		{
			base.CheckPQ_SU_Document();
			ListValidation.ErrorIfInvalidPK(Parent.PQ_SU_DocumentInfo);
			if (!Parent.PQ_SU_DocumentInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.PQ_SU_DocumentInfo);
				CheckDocumentDataState();
				CheckDocumentGroup();
				CheckDocumentIsApplicable();
			}
		}

		protected void CheckDocumentDataState()
		{
			IDocumentSupportable documentSupportable = processTaskNotification.Parent.GetJob() as IDocumentSupportable;

			if (documentSupportable != null && Parent.Document != null)
			{
				DocumentSupporterDataState documentDataState = documentSupportable.DocumentSupporter.GetDataStateBeforeRun(Parent.Document);

				if (documentDataState != null && !documentDataState.IsValid)
				{
					Parent.PQ_SU_DocumentInfo.AddWarning(documentDataState.ErrorMessage);
				}
			}
		}

		void CheckDocumentGroup()
		{
			if (Parent.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDocument && processTaskNotification.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery)
			{
				var documentSupportable = processTaskNotification.Parent.GetJob() as IDocumentSupportable;

				if (documentSupportable != null && Parent.Document != null)
				{
					var orgHeaderContact = documentSupportable.DocumentSupporter.GetContactOrganisation(Parent.Document.SU_MenuName, ContactType.Find(Parent.Document.SU_ContactType), Parent.Document.GetDocumentDirection());

					if (orgHeaderContact == null || (orgHeaderContact.OrgHeader == null && orgHeaderContact.RelatedOrgHeader == null && orgHeaderContact.OrgContact == null))
					{
						Parent.PQ_SU_DocumentInfo.AddWarning(Res.GetString("d4cf5c92-e471-4023-a6e1-d6609be3c2fd", "Can't get Document Delivery Contact for Document Group \"{0}\", auto delivery will not run for this document.", Parent.Document.SU_ContactType));
					}
				}
			}
		}

		void CheckDocumentIsApplicable()
		{
			if (Parent.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDocument)
			{
				var trigger = processTaskNotification.Parent;
				var job = trigger?.GetJob();

				if (job == null)
				{
					return;
				}

				if (WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.Value && TriggerUserContextList.Codes.Specified == trigger.TriggerContextCode)
				{
					var workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, new ExampleLog(trigger));
					using (Env.Instance.SetTemporaryUserContext(workflowUserContext.Staff.PK.ToGuid(), workflowUserContext.Branch.PK.ToGuid(), workflowUserContext.Department.PK.ToGuid()))
					{
						CheckDocumentCommandIsApplicable(job);
					}
				}
				else
				{
					CheckDocumentCommandIsApplicable(job);
				}
			}
		}

		void CheckDocumentCommandIsApplicable(BusinessObject job)
		{
			var menuItem = Parent.Document;
			if (menuItem != null && !menuItem.IsApplicable(job))
			{
				Parent.PQ_SU_DocumentInfo.AddWarning(Res.GetString("defddab3-d2b9-436f-a7ed-46774a89a45d", "The document may not be applicable, auto delivery may not run for this document. Filter: {0}", menuItem?.SU_FilterList ?? ZString.Empty));
			}
		}

		#endregion

		#region PQ_SQ

		protected override void CheckPQ_SQ()
		{
			base.CheckPQ_SQ();
			ListValidation.ErrorIfInvalidPK(Parent.PQ_SQInfo);
			if (Parent.PQ_SQ.IsEmpty || !Parent.PQ_SQ.IsValid)
			{
				if (Parent.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Print)
				{
					Parent.PQ_SQInfo.AddError(InvalidPrinter);
				}
			}
		}
		public static string InvalidPrinter
		{
			get { return Res.GetString("dfa30ac0-e700-4129-8461-88340c434f15", "A valid printer must be selected when you are delivering the document to a Printer."); }
		}

		#endregion

		#region PQ_TriggerParty

		protected override void CheckPQ_TriggerParty()
		{
			base.CheckPQ_TriggerParty();
			if (processTaskNotification.IsOtherTriggerParty)
			{
				if (!processTaskNotification.Parent.IsTemplate && MessageRecipientPartyTypeList.SpecialPartyTypes.ContainsCode(Parent.PQ_TriggerParty))
				{
					Parent.PQ_TriggerPartyInfo.AddWarning(Res.GetString("acac62b8-54de-4a5f-9019-3988087fe67a", "{0} is not a valid Recipient.", Parent.PQ_TriggerParty));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.PQ_TriggerPartyInfo);
				}
				MandatoryValidation.CheckEntered(Parent.PQ_TriggerPartyInfo);
			}
		}

		#endregion

		#region PQ_TriggerPartyService

		protected override void CheckPQ_TriggerPartyService()
		{
			base.CheckPQ_TriggerPartyService();

			if (processTaskNotification.IsTriggerPartyServiceAvailable)
			{
				ListValidation.ErrorIfInvalidCode(processTaskNotification.PQ_TriggerPartyServiceInfo);
				if (IsTriggerPartyServiceMandatory)
				{
					MandatoryValidation.CheckEntered(processTaskNotification.PQ_TriggerPartyServiceInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(processTaskNotification.PQ_TriggerPartyServiceInfo);
			}
		}

		public bool IsTriggerPartyServiceMandatory
		{
			get { return processTaskNotification.WorkflowDescriptor?.IsRecipientServiceMandatory(processTaskNotification.PQ_TriggerType, processTaskNotification.PQ_TriggerParty) ?? false; }
		}

		#endregion

		#region PQ_OH_Recipient

		protected override void CheckPQ_OH_Recipient()
		{
			base.CheckPQ_OH_Recipient();
			if (processTaskNotification.IsOtherTriggerParty)
			{
				MandatoryValidation.CheckEntered(Parent.PQ_OH_RecipientInfo);
			}
		}

		#endregion

		#region PQ_ECS_MessageDeliveryContextSelector

		protected override void CheckPQ_ECS_MessageDeliveryContextSelector()
		{
			base.CheckPQ_ECS_MessageDeliveryContextSelector();
			ListValidation.ErrorIfInvalidPK(Parent.PQ_ECS_MessageDeliveryContextSelectorInfo);
		}

		#endregion

		#region PQ_TriggerType

		protected override void CheckPQ_TriggerType()
		{
			base.CheckPQ_TriggerType();

			var triggerType = Parent.PQ_TriggerType;

			var shouldSkipValidateInvalidCode = (!Parent.PQ_SourceTemplateNotification.IsEmpty
																					&& (triggerType == WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage
																							|| triggerType == WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage))
																					|| (Parent.IsInDatabase
																							&& !Parent.PQ_TriggerPartyInfo.HasChanges
																							&& (triggerType == WorkflowTriggerActionTypeConstants.Codes.SendExportDemandDeTracing
																									|| triggerType == WorkflowTriggerActionTypeConstants.Codes.SendImportDemandDeTracing
																									|| triggerType == WorkflowTriggerActionTypeConstants.Codes.CINExportNotification));

			if (!shouldSkipValidateInvalidCode)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PQ_TriggerTypeInfo);
			}
			MandatoryValidation.CheckEntered(Parent.PQ_TriggerTypeInfo);

			var trigger = processTaskNotification.Parent;
			if (triggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)
			{
				if (trigger == null || trigger.TriggerEventCode == ZString.Empty)
				{
					Parent.PQ_TriggerTypeInfo.AddError(Res.GetString("b889292f-8be1-4028-bdec-0f5b9e86106f", "Cannot have a Universal Event trigger type where there is no Event Code for the Milestone."));
				}
			}
			else if (triggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML)
			{
				if (trigger == null || trigger.TriggerEventCode == ZString.Empty)
				{
					Parent.PQ_TriggerTypeInfo.AddError(Res.GetString("ed80d2dd-6efd-4acb-a181-004ca1463dc7", "Cannot have a Universal Event Collection trigger type where there is no Event Code for the Milestone."));
				}
			}
			else if (triggerType == WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs)
			{
				if (trigger == null || trigger.TriggerEventCode == Events.DocumentAllocatedCode)
				{
					Parent.PQ_TriggerTypeInfo.AddWarning(Res.GetString("0A9CFE17-56AF-4603-BDB4-51EF27699A73", "Pairing document-allocated(DDA) trigger-type with add-document-to-eDoc(EDC) completion-trigger-action may cause unnecessary documents to be added."));
				}
			}
			else if (triggerType == WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent)
			{
				if (!(trigger is IUniversalTemplateTrigger || trigger is ProcessTask task && task.IsNonPersistedRepresentationOfTemplateTrigger))
				{
					Parent.PQ_TriggerTypeInfo.AddError(Res.GetString("160969D2-A979-49CD-B04A-D52EABF1B92A", "DLY completion trigger actions are only allowed for universal triggers."));
				}
			}

			if (trigger != null && !shouldSkipValidateInvalidCode)
			{
				trigger.GetWorkflowDescriptor()?.CheckWorkflowTriggerActionType(trigger, trigger.GetJob(), Parent.PQ_TriggerTypeInfo);
			}

			if ((!Parent.IsInDatabase || Parent.PQ_TriggerTypeInfo.HasChanges) && !(trigger is TemplateProcessTask))
			{
				var workflowDescriptor = trigger?.GetWorkflowDescriptor();
				if (workflowDescriptor != null && workflowDescriptor.WorkflowTriggerActionTypeSecurityCheckPoints.TryGetValue(triggerType, out var securityCheckpoint))
				{
					if (!securityCheckpoint.IsAllowed)
					{
						Parent.PQ_TriggerTypeInfo.AddError(Res.GetString("5FE36334-60E9-4CC8-9918-5294C329E9E6", "You do not have the appropriate security rights to add this Trigger Action.\r\nIf you require access to this Trigger Action, ask your system administrator to change either your Staff or Group Security Rights to allow access to: {0}.", securityCheckpoint.DisplayTextPathToSecurityRight));
					}
				}
			}

			if (trigger != null && trigger.DelayDurationSeconds > 0 && ((ITriggerAction)Parent).RunLocation == TriggerRunLocation.Client)
			{
				Parent.PQ_TriggerTypeInfo.AddError(Res.GetString("44ECABAA-ACCA-41DC-81DB-203A482DBE9E", "Cannot have immediate trigger action on a trigger with a delay duration."));
			}
		}

		#endregion

		#region PQ_EmailAddr

		protected override void CheckPQ_EmailAddr()
		{
			base.CheckPQ_EmailAddr();
			if (!processTaskNotification.PQ_EmailAddr_ReadOnly)
			{
				if (Parent.PQ_EmailAddr.IsEmpty)
				{
					var emailEmptyMessage = Res.GetString("3a119ccf-b660-4995-ad62-5b3fb65e2ac7", "Please enter an Email Address; an Email Address is required when the recipient is '{0}'.", Parent.PQ_TriggerParty);

					if (processTaskNotification.IsPersonRelatedEmailTriggerParty)
					{
						Parent.PQ_EmailAddrInfo.AddWarning(emailEmptyMessage);
					}
					else
					{
						Parent.PQ_EmailAddrInfo.AddError(emailEmptyMessage);
					}
				}
				else
				{
					var emailAddress = Parent.PQ_EmailAddr;
					var macroEvaluation = false;
					if (WorkflowTriggerNotification.IsUserEmailMacro(Parent.PQ_EmailAddr))
					{
						var job = processTaskNotification?.Parent?.GetJob();
						if (job != null && typeof(ProcessTaskTemplate).IsAssignableFrom(job.GetType()))
						{
							return;
						}

						emailAddress = TriggerActionCommunicationModeSubstitutor.Substitute(processTaskNotification, job, null, MessageDelivery.CommunicationModeSubstitutorProperty.EmailAddress, emailAddress);
						macroEvaluation = true;
					}

					foreach (var email in emailAddress.Split(','))
					{
						if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(email.Trim()))
						{
							if (macroEvaluation)
							{
								Parent.PQ_EmailAddrInfo.AddWarning(EmailAddressIsInvalidAfterMacroEvaluation);
							}
							else
							{
								Parent.PQ_EmailAddrInfo.AddError(EmailAddressIsInvalid);
							}

							return;
						}
					}
				}
			}
		}

		internal static string EmailAddressIsInvalid
		{
			get { return Res.GetString("3ef284fe-d487-4d4c-8e71-889737c48120", "Please enter a valid Email Address."); }
		}

		internal static string EmailAddressIsInvalidAfterMacroEvaluation
		{
			get { return Res.GetString("65542ccb-8f69-43da-8a1a-63320fa09a3c", "Email address was invalid after evaluating macro."); }
		}

		#endregion

		#region PQ_FieldName

		public void ValidatePQ_FieldName()
		{
			ValidateCalculatedProperty(processTaskNotification.PQ_FieldNameInfo);
		}

		protected virtual void CheckPQ_FieldName()
		{
			if (processTaskNotification.IsSetFieldOrApplyTag)
			{
				var notifications = new ValidationNotifications();

				if (processTaskNotification.IsSetField)
				{
					MandatoryValidation.CheckEntered(processTaskNotification.PQ_FieldNameInfo);

					if (!string.IsNullOrWhiteSpace(processTaskNotification.PQ_FieldName))
					{
						WorkflowProcessorHelper.GetFinalPropertyInfoAndParentType(processTaskNotification, notifications, new WorkflowMacroSetFieldValidation(processTaskNotification), null);
					}
				}
				else if (processTaskNotification.IsApplyTag)
				{
					if (!string.IsNullOrWhiteSpace(processTaskNotification.PQ_FieldName))
					{
						WorkflowApplyTagProcessor.GetFinalPropertyInfoAndParentType(processTaskNotification, notifications);
					}
				}

				if (notifications.HasErrors())
				{
					var notificationsText = notifications.GetErrors().ToMessageListString();
					processTaskNotification.PQ_FieldNameInfo.AddError(notificationsText);
				}
				else if (notifications.HasWarnings())
				{
					var notificationsText = notifications.GetWarnings().ToMessageListString();
					processTaskNotification.PQ_FieldNameInfo.AddWarning(notificationsText);
				}
			}
		}

		[Serializable]
		public class ValidationNotifications : NotificationCollection
		{
			public override string ToString()
			{
				return this.ToMessageListString();
			}
		}

		#endregion

		#region PQ_FieldValue

		public void ValidatePQ_FieldValue()
		{
			ValidateCalculatedProperty(processTaskNotification.PQ_FieldValueInfo);
		}

		protected virtual void CheckPQ_FieldValue()
		{
			if (processTaskNotification.IsSetField && !string.IsNullOrEmpty(processTaskNotification.PQ_FieldValue.Trim()))
			{
				var trigger = processTaskNotification.Parent;
				var workflowProviderType = trigger?.GetCountrySpecificTypeIfApplicable();

				if (workflowProviderType != null)
				{
					var rootBizo = trigger.GetJob();

					if (rootBizo != null && workflowProviderType.IsAssignableFrom(rootBizo.GetType()))
					{
						var validation = new WorkflowMacroSetFieldValidation(processTaskNotification);
						var notifications = new ValidationNotifications();
						var (propertyInfo, parentType, _) = WorkflowProcessorHelper.GetFinalPropertyInfoAndParentType(processTaskNotification, notifications, validation);

						if (propertyInfo != null && !notifications.HasNotifications())
						{
							var isValueClause = new MacroClauseProcessor(notifications, validation).IsValueClause(processTaskNotification.PQ_FieldValue);

							if (!isValueClause)
							{
								var (result, _) = new WorkflowMacroEvaluator(notifications, validation)
									.EvaluateMacros(
										businessObjects: ((IRootTypeProvider)processTaskNotification).Roots,
									macrosValuePath: processTaskNotification.PQ_FieldValue);

								if (result != null)
								{
									MacroHelper.ConvertValue(
										parentType,
										propertyInfo,
										result,
										notifications,
										validation.DetailWarningMessage);
								}

								if (notifications.HasNotifications())
								{
									processTaskNotification.PQ_FieldValueInfo.AddWarning(notifications.ToString().Trim());
								}
							}
						}
						else if (notifications.HasNotifications())
						{
							processTaskNotification.PQ_FieldValueInfo.AddWarning(Res.GetString("72dafaaf-33e9-47eb-b224-56ce30a05286", "Field Name has errors."));
						}
					}
				}
			}
		}

		#endregion

		#region PQ_ActionReference

		public void ValidatePQ_ActionReference()
		{
			ValidateCalculatedProperty(processTaskNotification.PQ_ActionReferenceInfo);
		}

		protected virtual void CheckPQ_ActionReference()
		{
			if (processTaskNotification.PQ_ActionReference.Contains('|'))
			{
				processTaskNotification.PQ_ActionReferenceInfo.AddError(Res.GetString("B04ABFCA-80A5-46EB-B511-985904CDFBAD", "Action reference should not contain pipe (\"|\") symbols."));
			}
		}

		#endregion

		#region PQ_P0_WorkflowTemplate

		protected override void CheckPQ_P0_WorkflowTemplate()
		{
			base.CheckPQ_P0_WorkflowTemplate();

			var isApplyWorkflowTemplateAlwaysTrigger = processTaskNotification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			if (processTaskNotification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce || isApplyWorkflowTemplateAlwaysTrigger)
			{
				MandatoryValidation.CheckEntered(Parent.PQ_P0_WorkflowTemplateInfo);
			}

			ListValidation.ErrorIfInvalidPK(Parent.PQ_P0_WorkflowTemplateInfo);

			var trigger = processTaskNotification.Parent;

			if (trigger != null && trigger.ParentID != ZGuid.Empty && trigger.ParentID == Parent.PQ_P0_WorkflowTemplate)
			{
				Parent.PQ_P0_WorkflowTemplateInfo.AddError(Res.GetString("25018b2e-e4fe-4913-bd41-7387d47ca7ab", "Please select a workflow template that is not the current workflow template."));
			}

			if (isApplyWorkflowTemplateAlwaysTrigger
				&& (Parent.ProcessTask != null && Parent.ProcessTask.ProcessTaskNotifications.Any(x => x.PK != Parent.PK && x.PQ_P0_WorkflowTemplate == Parent.PQ_P0_WorkflowTemplate)
				|| trigger != null && trigger.TriggerActions.Cast<ProcessTaskNotification>().Any(x => x.PK != Parent.PK && x.PQ_P0_WorkflowTemplate == Parent.PQ_P0_WorkflowTemplate)))
			{
				Parent.PQ_P0_WorkflowTemplateInfo.AddError(Res.GetString("e0a0b35e-bf64-4542-876c-7e8e5763687c", "Please select a workflow template that is not already applied to this trigger."));
			}

			var template = Parent.WorkflowTemplate;
			if (template != null)
			{
				if (!template.P0_EffectiveStartDateUtc.IsEmpty && ZDateTime.UtcNow < template.P0_EffectiveStartDateUtc)
				{
					Parent.PQ_P0_WorkflowTemplateInfo.AddWarning(Res.GetString("c2e87a7f-0c01-4f67-912f-1a3ec2a71236", "The effective start date of the selected template has not yet been reached."));
				}
				if (!template.P0_EffectiveEndDateUtc.IsEmpty && ZDateTime.UtcNow > template.P0_EffectiveEndDateUtc)
				{
					Parent.PQ_P0_WorkflowTemplateInfo.AddWarning(Res.GetString("c00f9cba-39f5-465e-b6a9-de9348c2961b", "The effective end date of the selected template has been exceeded."));
				}
			}
		}

		#endregion

		#region PQ_RelatedEntityId

		protected override void CheckPQ_RelatedEntityId()
		{
			base.CheckPQ_RelatedEntityId();

			if (processTaskNotification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyTag)
			{
				MandatoryValidation.CheckEntered(Parent.PQ_RelatedEntityIdInfo);

				ListValidation.ErrorIfInvalidPK(Parent.PQ_RelatedEntityIdInfo);

				if (!Parent.PQ_RelatedEntityIdInfo.HasErrors())
				{
					var errorMessage = Res.GetString("076c94f1-ebbd-4906-a416-add5a50c6ae0", "Another TAG completion trigger action is already configured to apply a different tag from the same exclusive group. Please update or remove the existing TAG completion trigger action to apply this tag.");

					var triggerActionsLinkedToSameTrigger = (processTaskNotification.Parent?.GetJob() as IWorkflowProvider)?.WorkflowItems.OfType<ProcessTask>().SelectMany(t => t.CompletionTriggerActionsCollection().Where(p => p.PK != Parent.PK && p.PQ_P9 == Parent.PQ_P9));
					if (triggerActionsLinkedToSameTrigger != null && triggerActionsLinkedToSameTrigger.Any())
					{
						if (triggerActionsLinkedToSameTrigger.Any(x => x.RelatedEntity != null && x.RelatedEntity.TagDefinition.TGD_IsExclusive && x.RelatedEntity.TGM_TGD_Tag == processTaskNotification.RelatedEntity.TGM_TGD_Tag))
						{
							Parent.PQ_RelatedEntityIdInfo.AddError(errorMessage);
						}
					}
				}
			}
		}

		#endregion

		#region PQ_EmailTextFallbackToTemplate

		public void ValidatePQ_EmailTextFallbackToTemplate()
		{
			ValidateCalculatedProperty(processTaskNotification.PQ_EmailTextFallbackToTemplateInfo);
		}

		protected virtual void CheckPQ_EmailTextFallbackToTemplate()
		{
			if (AnalyzeTableHeaders(processTaskNotification.PQ_EmailTextFallbackToTemplate))
			{
				processTaskNotification.PQ_EmailTextFallbackToTemplateInfo.AddError(EmailWithIncorrectTableHeaders);
			}
		}

		static string EmailWithIncorrectTableHeaders => Res.GetString("762FFD85-7151-42C0-AC90-D5B54635BC3C", "Wrong table header tag found in email. You should use <th> tags inside row with \"{0}\" class.", "tableheadings");

		static bool AnalyzeTableHeaders(string htmlBody)
		{
			var exit = false;
			var wrongTagFound = false;
			var lastIndex = -1;

			while (!exit && lastIndex + 1 < htmlBody.Length)
			{
				var index = htmlBody.Replace('"', '\'').IndexOf("'tableheadings'", lastIndex + 1, StringComparison.Ordinal);
				var closingTagIndex = htmlBody.IndexOf("</tr>", index + 1, StringComparison.Ordinal);

				if (closingTagIndex == -1)
				{
					closingTagIndex = htmlBody.Length - 1;
				}

				var wrongTagIndex = index >= 0 && closingTagIndex > index ? htmlBody.IndexOf("<td", index, closingTagIndex - index, StringComparison.Ordinal) : -1;
				if (wrongTagIndex >= 0)
				{
					wrongTagFound = true;
				}

				if (index < 0 || wrongTagFound)
				{
					exit = true;
				}

				lastIndex = index;
			}

			return wrongTagFound;
		}

		#endregion

		#region StaffCode

		public void ValidateStaffCode()
		{
			ValidateCalculatedProperty(processTaskNotification.StaffCodeInfo);
		}

		protected virtual void CheckStaffCode()
		{
			ListValidation.ErrorIfInvalidCode(processTaskNotification.StaffCodeInfo);
		}

		#endregion

		#region PQ_Offset
		protected override void CheckPQ_Offset()
		{
			base.CheckPQ_Offset();

			if (processTaskNotification.IsScheduleEvent)
			{
				MandatoryValidation.CheckEntered(processTaskNotification.PQ_OffsetInfo);

				if (!processTaskNotification.PQ_Offset.IsEmpty)
				{
					CheckPQ_OffsetIsNotNegative();
				}
			}
		}

		void CheckPQ_OffsetIsNotNegative()
		{
			var trigger = processTaskNotification.Parent;

			if (trigger != null && !trigger.ShouldTriggerOnEstimateEvents && processTaskNotification.PQ_Offset.IsValid)
			{
				var timeSpan = processTaskNotification.PQ_Offset.TimeSpan6MonthsFromStartOfYear;

				if (timeSpan < TimeSpan.Zero)
				{
					Parent.PQ_OffsetInfo.AddError(Res.GetString("B35D7B70-7CF7-45FD-B0CF-C21BC1D24E60", "Cannot define a negative offset for a trigger that responds to actual events. Negative offsets are allowed for triggers responding to estimate events only."));
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			if (Parent.HasChanges || !Parent.IsInDatabase)
			{
				ValidatePQ_FieldName();
				ValidatePQ_FieldValue();
				ValidatePQ_EmailTextFallbackToTemplate();
				ValidatePQ_MacroTypeCode();
			}
			ValidatePQ_Calc_TriggerParty();
			ValidateStaffCode();
		}
	}
}
