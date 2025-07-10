using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Business.Workflow.TriggerActionRunners;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;
using Forwarding = Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business
{
	public abstract partial class WorkflowDescriptor : NonPersistentBusinessObject, IWorkflowDescriptor, IObsoleteValidation
	{
		public abstract string Code { get; }
		public abstract IMultilingualString Description { get; }
		public abstract ControllerID ControllerID { get; }
		public abstract Type WorkflowProviderType { get; }

		public Type WorkflowProviderTypeForNonPersistentBusinessObjects => WorkflowProviderTypeForNonPersistentBusinessObjectsCore;
		protected virtual Type WorkflowProviderTypeForNonPersistentBusinessObjectsCore => WorkflowProviderType;

		public virtual ProcessTemplateSubType[] SubTypeInformation => Array.Empty<ProcessTemplateSubType>();
		public virtual bool RequiresPort1 => false;
		public virtual ZString Port1Name => Res.GetString("{0377A4EF-BD56-4fbf-8F95-9BB8549FF048}", "Load Port");
		public virtual bool RequiresPort2 => false;
		public virtual ZString Port2Name => Res.GetString("{1DF5E2D9-6A46-4bd8-984E-1170D7144AC5}", "Discharge Port");
		public virtual bool RequiresClient => true;
		public virtual ZString ClientName => Res.GetString("{4887902B-71F8-42D0-95EC-9E9D6E9023FF}", "Client");
		public virtual ZString CreditorName => Res.GetString("{77F20208-DF2C-44FB-BC55-C5356E2C5EEA}", "Creditor");
		public virtual ZString WarehouseName => Res.GetString("{FF0878D0-87B9-4E6F-B9AD-9725CB5E1330}", "Warehouse");

		[BusinessObjectTestExclude]
		public virtual Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProvider => null;

		public virtual WarehouseCollectionType WarehouseType => WarehouseCollectionType.ProductWarehouse;

		public virtual bool RequiresBranch => false;
		public virtual bool RequiresDepartment => false;
		public virtual bool SupportsEventTracking => false;
		public virtual bool OnlySupportEventTrackingForUniversalTemplates => false;
		public virtual bool SupportsTasks => true;

		/// <summary>
		/// This property controls whether tasks can be seen and created on jobs. See <see cref="SupportsTasks"/> for controlling whether tasks can be created on workflow templates.
		/// </summary>
		public virtual bool ShouldHideTasksTabOnJobs => false;

		public virtual bool SupportsScreenLayout => true;
		public virtual bool SupportsCustomFields => true;
		public virtual bool AreTasksCompanySpecific => true;
		public virtual bool RequiresWarehouse => false;
		public virtual bool SupportsBufferManagement => true;
		public virtual bool SupportsUniversalTemplates => SupportsEventTracking;
		public virtual bool SupportsWorkflowTemplates => true;

		public bool SupportsTaskLineTriggers => SupportsEventTracking && SupportsTasks && SupportsTaskLineTriggersCore;
		protected virtual bool SupportsTaskLineTriggersCore => true;

		public bool SupportsExceptionLineTriggers => SupportsEventTracking && SupportsExceptionLineTriggersCore;

		protected virtual bool SupportsExceptionLineTriggersCore => true;

		public bool SupportsReleaseGroupRules => SupportsTasks && SupportsWorkflowTemplates && SupportsBufferManagement && SupportsReleaseGroupRulesCore;
		protected virtual bool SupportsReleaseGroupRulesCore => true;

		public bool SupportsWorkflowTypeFilters => SupportsWorkflowTypeFiltersCore;
		protected internal virtual bool SupportsWorkflowTypeFiltersCore => true;

		public bool SupportsReapplyTemplatesMenuItem => SupportsReapplyTemplatesMenuItemCore;

		protected virtual bool SupportsReapplyTemplatesMenuItemCore => false;

		WeakReference lastProcessTaskTemplate;
		public ProcessTaskTemplate LastProcessTaskTemplate
		{
			get => lastProcessTaskTemplate != null ? lastProcessTaskTemplate.Target as ProcessTaskTemplate : null;
			set => lastProcessTaskTemplate = new WeakReference(value);
		}

		public virtual CodeDescriptionPairList EstimateDefaultedFromList => estimateDefaultedFromList ?? (estimateDefaultedFromList = new CodeDescriptionPairList());
		CodeDescriptionPairList estimateDefaultedFromList;

		public virtual ZString MilestoneTemplateHintCaption => "";
		public ZPropertyInfo MilestoneTemplateHintCaptionInfo => GetZPropertyInfo(nameof(MilestoneTemplateHintCaption));

		public virtual Type GetAdditionalRootType(TemplateConditionsViewModel templateConditions, TriggerConditionsViewModel triggerConditions)
		{
			return null;
		}

		#region Validation Rules

		public ValidationToolSettings ValidationToolSettings => validationToolSettings ??= GetValidationToolSettings();
		ValidationToolSettings validationToolSettings;

		protected virtual ValidationToolSettings GetValidationToolSettings() => new ValidationToolSettings(this);

		#endregion

		#region Conditions

		public virtual CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem) => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem) => new CodeDescriptionPairList();

		public virtual IWorkflowTemplateApplicationExtender GetTemplateApplicationExtender() => null;

		#endregion

		#region ConditionValues

		public virtual CodeDescriptionPairList GetConditionValueList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new CodeDescriptionPairList();
		}

		public virtual TemplateConditionValueStyle GetCondition2ValueStyle(ITemplateConditionalWorkflowItem workflowItem)
		{
			return TemplateConditionValueStyle.Unused;
		}

		#endregion

		#region Workflow Triggers

		public virtual BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.INVALID };

		public virtual SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null) => Array.Empty<SchemaColumn>();

		public virtual string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			string columnDescription = DataBoundResourceStrings.GetColumnDescriptiveName(fieldColumn.TableName, fieldColumn.Name);
			return columnDescription == fieldColumn.TableName + "|" + fieldColumn.Name ? ""
				: DataBoundResourceStrings.GetTableDescriptiveName(fieldColumn.TableName) + " - " + columnDescription;
		}

		#region Workflow Trigger Action Types(For View)

		public ICodeDescriptionPairList GetWorkflowTriggerActionTypes() => GetWorkflowTriggerActionTypes(null, null);

		public virtual bool ParentSupportsWorkflowTriggerActionUniversalShipmentXML(IBusiness parent) => false;

		/// <summary>
		/// Provides trigger action types available for the ProcessTask if specified; otherwise action types available for any ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to find available action types</param>
		/// <returns>The list of available action types</returns>
		public ICodeDescriptionPairList GetWorkflowTriggerActionTypes(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();

			if (SupportedMessageRecipientParties(trigger, parent) != MessageRecipientPartyType.None)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationEmail);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationBodyEmail);

				if (DocumentBusinessContext[0] != BusinessContext.INVALID)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendDocument, WorkflowTriggerActionTypeConstants.Descriptions.SendDocument);
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs, WorkflowTriggerActionTypeConstants.Descriptions.AddDocumentToEDocs);
				}
			}
			else
			{
				if (SupportedMessageRecipientPartiesForSpecificAction(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail) != MessageRecipientPartyType.None)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationEmail);
				}
				if (SupportedMessageRecipientPartiesForSpecificAction(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail) != MessageRecipientPartyType.None)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationBodyEmail);
				}
			}
			if (SupportsAssignStaffAndEmail(trigger, parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail, WorkflowTriggerActionTypeConstants.Descriptions.AssignStaffandEmail);
			}

			if (SupportsWorkflowTriggerActionXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXML, WorkflowTriggerActionTypeConstants.Descriptions.SendXML);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLSimplified);

				if (IncludeWorkflowTriggerActionXMLDebtorBalance)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLDebtorBalance);
				}
			}

			if (SupportsWorkflowTriggerActionNativeXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, WorkflowTriggerActionTypeConstants.Descriptions.SendNativeXML);
			}

			if (SupportsWorkflowTriggerActionUniversalShipmentXML || ParentSupportsWorkflowTriggerActionUniversalShipmentXML(parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentXML);
			}

			if (SupportsWorkflowTriggerActionUniversalEventXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventXML);
				if (trigger != null)
				{
					var milestoneEvent = trigger.TriggerEventCode;
					if (milestoneEvent == Events.DocumentImportedCode || milestoneEvent == Events.DocumentAllocatedCode)
					{
						result.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventXMLWithEDoc));
						result.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml, WorkflowTriggerActionTypeConstants.Descriptions.SendEDocXml));
					}
				}
			}

			if (SupportsWorkflowTriggerActionUniversalEventCollectionXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventCollectionXML);
			}

			if (SupportsWorkflowTriggerActionUniversalTransactionXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalTransactionXML);
			}

			if (SupportsWorkflowTriggerActionUniversalScheduleXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalScheduleXML);
			}

			if (SupportsWorkflowTriggerActionUniversalTransactionBatchXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalTransactionBatchXML);
			}

			if (SupportsWorkflowTriggerActionUniversalActivityXML)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalActivityXML);
			}

			if (SupportsWorkflowTriggerActionXMLWithJobFallback)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithJobFallback);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallbackSimplified, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithJobFallbackSimplified);
			}

			if (SupportsWorkflowTriggerActionXMLWithAWB)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithAWB);
			}

			if (SupportsOtherCompanyAPInvoiceImport)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies, WorkflowTriggerActionTypeConstants.Descriptions.ImportAPInvoicesFromOtherCompanies);
			}

			if (SupportsRecognizeRevenue)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue, WorkflowTriggerActionTypeConstants.Descriptions.RecognizeRevenue);
			}

			if (SupportsAutoRateCostsAndRevenue)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateCostsAndRevenue);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateCosts);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateRevenue);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateNonConsolLevelCosts);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateNonConsolLevelCostsAndRevenue);
			}

			if (SupportsAutoPack)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoPack, WorkflowTriggerActionTypeConstants.Descriptions.AutoPack);
			}

			if (SupportsCreateTransportBooking)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportBooking);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportBookingContainer);
			}

			if (SupportsCreateTransportJob)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportJob);
			}

			if (SupportsPostConsolCostOnly)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly, WorkflowTriggerActionTypeConstants.Descriptions.PostConsolCostOnly);
			}

			if (SupportsPostAllRevenue)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue, WorkflowTriggerActionTypeConstants.Descriptions.PostAllRevenue);
			}

			if (SupportsCreateJobInvoiceHeader)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader, WorkflowTriggerActionTypeConstants.Descriptions.CreateJobInvoiceHeader);
			}

			if (SupportsCreateProfitShareCharges)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges, WorkflowTriggerActionTypeConstants.Descriptions.CreateProfitShareCharges);
			}

			if (SupportsSetFieldTriggerAction(trigger, parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange, WorkflowTriggerActionTypeConstants.Descriptions.ImmediateFieldChange);
			}

			foreach (var code in GetSupportsValidateForCustomsMessagingTriggerActions(trigger, parent))
			{
				result.AddPair(code, new WorkflowTriggerActionTypeConstants().GetDescriptionFromCode(code));
			}

			foreach (var code in GetScheduleDeferredMessageSendTriggerActions(trigger, parent))
			{
				result.AddPair(code, new WorkflowTriggerActionTypeConstants().GetDescriptionFromCode(code));
			}

			if (SupportsPostOverseasAgentCharges)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges, WorkflowTriggerActionTypeConstants.Descriptions.PostOverseasAgentCharges);
			}

			if (SupportsPostAllSisterCompanyCharges)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges, WorkflowTriggerActionTypeConstants.Descriptions.PostAllSisterCompanyCharges);
			}

			if (SupportsPostLocalSisterCompanyChargesOnly)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly, WorkflowTriggerActionTypeConstants.Descriptions.PostLocalSisterCompanyChargesOnly);
			}

			if (SupportsPostAllCosts)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PostAllCosts, WorkflowTriggerActionTypeConstants.Descriptions.PostAllCosts);
			}

			if (SupportsTransactionAllocationAndPost)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost, WorkflowTriggerActionTypeConstants.Descriptions.TransactionAllocationAndPost);
			}

			if (SupportsIncludeChargeInProfitShare)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare, WorkflowTriggerActionTypeConstants.Descriptions.IncludeChargeInProfitShare);
			}

			if (SupportsApplyWorkflowTemplate)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce, WorkflowTriggerActionTypeConstants.Descriptions.ApplyWorkflowTemplateOnce);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways, WorkflowTriggerActionTypeConstants.Descriptions.ApplyWorkflowTemplateAlways);
			}

			if (SupportsWorkflowTriggerActionApplyTag(parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ApplyTag, WorkflowTriggerActionTypeConstants.Descriptions.ApplyTag);
			}

			if (SupportsAddEConversationMessages)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, WorkflowTriggerActionTypeConstants.Descriptions.AddEConversationMessage);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, WorkflowTriggerActionTypeConstants.Descriptions.AddInternalEConversationMessage);
			}

			if (SupportsConvertToShipment)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ConvertToShipment, WorkflowTriggerActionTypeConstants.Descriptions.ConvertToShipment);
			}

			if (SupportsHVLVPreScreening)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening, WorkflowTriggerActionTypeConstants.Descriptions.RunHVLVPreScreening);
			}

			if (SupportsDelayedEvents)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, WorkflowTriggerActionTypeConstants.Descriptions.ScheduleDelayedEvent);
			}

			if (SupportsAutomatedExitReportTransferMessageSending(parent, trigger))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendExitReportTransferMessage);
			}

			if (SupportsSendCalculateCO2EmissionRequest)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest, WorkflowTriggerActionTypeConstants.Descriptions.SendCalculateCO2EmissionRequest);
			}

			if (SupportEmmaMessageGeneration)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator, WorkflowTriggerActionTypeConstants.Descriptions.CusNOEmmaMessageGenerator);
			}

			var additionalItems = GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			if (additionalItems != null)
			{
				result.AddRange(additionalItems);
			}

			return result;
		}

		/// <summary>
		/// When overridden in a derived class, provides additional action types available for the ProcessTask if specified;
		/// otherwise action types available for any ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to find available action types</param>
		/// <returns>The list of available action types</returns>
		protected virtual ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			return new CodeDescriptionPairList();
		}

		/// <summary>
		/// Performs validation of the action type for the specified ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to validate action type</param>
		/// <param name="actionTypeInfo">The info of the property containing action type</param>
		public void CheckWorkflowTriggerActionType(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo actionTypeInfo)
		{
			CheckWorkflowTriggerActionTypeCore(trigger, parent, actionTypeInfo);
		}

		/// <summary>
		/// When overridden in a derived class, performs validation of the action type for the specified ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to validate action type</param>
		/// <param name="actionTypeInfo">The info of the property containing action type</param>
		protected virtual void CheckWorkflowTriggerActionTypeCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo actionTypeInfo)
		{
			var actionTypeStringValue = actionTypeInfo.Value.ToString();
			if (actionTypeStringValue == WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange &&
				!string.IsNullOrEmpty(trigger.TriggerFieldName))
			{
				actionTypeInfo.AddError(Res.GetString("7944fb8d-2cd8-4e3f-af2e-2bc729f193ed",
					"Immediate Field Change cannot be used on Completion Trigger Actions for Milestones and Triggers that can trigger via the Trigger Field."));
			}

			if (actionTypeStringValue == WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage
				&& trigger is ProcessTask task && task.P9_LineTriggerType != TriggerLineTypes.Codes.CusExitReport)
			{
				actionTypeInfo.AddError(Res.GetString("970bf46a-b27d-4355-8b8c-d930e98b5972", "Trigger action Type ‘TRA’ requires Line Trigger Type ‘CXR’ in its Trigger"));
			}
		}

		/// <summary>
		/// Performs validation of the recipient party for the specified ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to validate recipient party</param>
		/// <param name="recipientPartyInfo">The info of the property containing recipient party</param>
		public void CheckWorkflowTriggerRecipientParty(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);
		}

		/// <summary>
		/// When overridden in a derived class, performs validation of the recipient party for the specified ProcessTask
		/// </summary>
		/// <param name="trigger">The ProcessTask to validate recipient party</param>
		/// <param name="recipientPartyInfo">The info of the property containing recipient party</param>
		protected virtual void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
		}

		public Dictionary<ZString, SecurityCheckpoint> WorkflowTriggerActionTypeSecurityCheckPoints => workflowTriggerActionTypeSecurityCheckPoints ?? (workflowTriggerActionTypeSecurityCheckPoints = GetWorkflowTriggerActionTypeSecurityCheckPoints());
		Dictionary<ZString, SecurityCheckpoint> workflowTriggerActionTypeSecurityCheckPoints;

		protected virtual Dictionary<ZString, SecurityCheckpoint> GetWorkflowTriggerActionTypeSecurityCheckPoints()
		{
			return new Dictionary<ZString, SecurityCheckpoint>();
		}

		public virtual bool SupportsWorkflowTriggerActionXML => false;
		public virtual bool IncludeWorkflowTriggerActionXMLDebtorBalance => true;
		public virtual bool SupportsWorkflowTriggerActionXMLWithJobFallback => false;

		public virtual bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			{ return true; }
		}

		#region ValidateForCustomsMessagingSupporter

		public IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActions(IBaseTrigger trigger, IBusiness parent)
		{
			var validateCustomsMessagingSupported = parent is ProcessTaskTemplate;
			if (!validateCustomsMessagingSupported && parent is IValidateForCustomsMessagingSupporter customsMessagingSupporter)
			{
				validateCustomsMessagingSupported = customsMessagingSupporter.SupportValidateCustomsMessaging;
			}

			return validateCustomsMessagingSupported ? GetSupportsValidateForCustomsMessagingTriggerActionsCore(trigger, parent) : Enumerable.Empty<string>();
		}

		protected virtual IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			return Enumerable.Empty<string>();
		}

		#endregion

		#region ScheduleDeferredMessageSend

		public IEnumerable<string> GetScheduleDeferredMessageSendTriggerActions(IBaseTrigger trigger, IBusiness parent)
		{
			return parent != null && (parent is ITriggerActionMessagingSupporterProvider || parent is ProcessTaskTemplate) ? GetScheduleDeferredMessageSendTriggerActionsCore(trigger, parent) : Enumerable.Empty<string>();
		}

		protected virtual IEnumerable<string> GetScheduleDeferredMessageSendTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			return Enumerable.Empty<string>();
		}

		#endregion

		public virtual MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.None;
		}

		public virtual MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			return MessageRecipientPartyType.None;
		}

		#region SupportedTriggerPartyServices

		public ZString[] SupportedTriggerPartyServices(ZString recipient)
		{
			return SupportedTriggerPartyServicesCore(recipient);
		}

		protected virtual ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			return Array.Empty<ZString>();
		}

		#endregion

		public virtual MessageRecipientPartyType SupportedManifestMessageRecipientParties(IBaseTrigger trigger)
		{
			return MessageRecipientPartyType.None;
		}

		public virtual bool SupportsAssignStaffAndEmail(IBaseTrigger trigger, IBusiness parent) => false;

		public virtual MessageRecipientPartyType SupportedSendDocumentMessageRecipientParties(ZString triggerAction)
		{
			var messageRecipientPartyType = MessageRecipientPartyType.Print | MessageRecipientPartyType.Email;

			if (!IsPrintOnlyTriggerAction(triggerAction))
			{
				messageRecipientPartyType |= MessageRecipientPartyType.AutoDocumentDelivery;
			}

			return messageRecipientPartyType;
		}

		public bool SupportsWorkflowTriggerActionNativeXML
		{
			get
			{
				if (!supportsWorkflowTriggerActionNativeXML.HasValue)
				{
					supportsWorkflowTriggerActionNativeXML = ExportValidator.CanBeExported(WorkflowProviderType);
				}

				return supportsWorkflowTriggerActionNativeXML.Value;
			}
		}

		bool? supportsWorkflowTriggerActionNativeXML;

		IExportValidator ExportValidator
		{
			get { return exportValidator ?? (exportValidator = ObjectFactory.Get<IExportValidator>("NativeXmlExportValidator")); }
		}

		IExportValidator exportValidator;

		public virtual bool SupportsWorkflowTriggerActionXMLWithAWB => false;

		public virtual bool SupportsOtherCompanyAPInvoiceImport
		{
			get
			{
				return DocumentBusinessContext.Length > 0 &&
					(DocumentBusinessContext[0] == BusinessContext.Shipment || DocumentBusinessContext[0] == BusinessContext.Consol);
			}
		}

		public virtual bool SupportsPostOverseasAgentCharges
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null && WorkflowProviderType.GetInterface("ISupportsPostingOverseasAgentCharge") != null;
			}
		}

		public virtual bool SupportsRecognizeRevenue
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null || WorkflowProviderType.GetInterface("IJobCostingPlugIn") != null;
			}
		}

		public virtual bool SupportsAutoRateCostsAndRevenue
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IRatingSupporter)) != null &&
					(WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null || WorkflowProviderType.GetInterface("IJobCostingPlugIn") != null);
			}
		}

		public virtual bool SupportsAutoPack
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IWhsOrder)) != null;
			}
		}

		public virtual bool SupportsCreateTransportBooking
		{
			get { return false; }
		}

		public virtual bool SupportsCreateTransportJob
		{
			get { return false; }
		}

		public virtual bool SupportsPostConsolCostOnly
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null &&
					WorkflowProviderType.GetInterface(nameof(Forwarding.IForwardingConsol)) != null;
			}
		}

		public virtual bool SupportsTransactionAllocationAndPost
		{
			get
			{
				return false;
			}
		}

		public virtual bool SupportsPostAllRevenue
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null &&
					WorkflowProviderType.GetInterface(nameof(Forwarding.IForwardingConsol)) == null;
			}
		}

		public virtual bool SupportsCreateJobInvoiceHeader
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null &&
					WorkflowProviderType.GetInterface(nameof(Forwarding.IForwardingConsol)) == null;
			}
		}

		public virtual bool SupportsCreateProfitShareCharges
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null || WorkflowProviderType.GetInterface("IJobCostingPlugIn") != null;
			}
		}

		public virtual bool SupportsPostAllSisterCompanyCharges
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null &&
					WorkflowProviderType.GetInterface(nameof(Forwarding.IForwardingConsol)) == null;
			}
		}

		public virtual bool SupportsAutomatedExitReportTransferMessageSending(IBusiness businessObject, IBaseTrigger trigger)
		{
			if (IsTrigger() && RelatedCountrySupportsAutoSendExitReportTransferMessage())
			{
				if (businessObject is ProcessTaskTemplate template)
				{
					var processType = (string)template?.P0_ProcessType;
					return IsSupportedProcessType(processType);
				}

				return true;
			}

			return false;

			bool IsTrigger() => trigger != null && trigger.IsTrigger();

			bool RelatedCountrySupportsAutoSendExitReportTransferMessage()
			{
				var companyCountryCode = GetCompanyCountryCodeFromWorkflowItemOrTemplate(trigger, (BusinessObject)businessObject);
				var exitReportTypeDecider = ObjectFactory.Get<CountrySpecificTypeDecider>("EUExitControl.CusExitReportTypeDecider");
				var exitReportTypeForCertainCountry = exitReportTypeDecider.GetTypeForCountryCode(companyCountryCode);
				var defaultTypeForUnsupportedCountry = ObjectFactory.GetType<ICusExitReport>();
				return exitReportTypeForCertainCountry != defaultTypeForUnsupportedCountry && typeof(ISupportAutoSendExitReportTransferMessage).IsAssignableFrom(exitReportTypeForCertainCountry);
			}

			bool IsSupportedProcessType(string processType)
			{
				var supportedProcessTypes = new[]
				{
					WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode,
					WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode,
					WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode
				};

				return supportedProcessTypes.Contains(processType);
			}
		}

		public virtual bool SupportsPostLocalSisterCompanyChargesOnly
		{
			get
			{
				return WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null &&
					WorkflowProviderType.GetInterface(nameof(Forwarding.IForwardingConsol)) == null;
			}
		}

		public virtual bool SupportsPostAllCosts
		{
			get
			{
				return WorkflowProviderType.GetInterface("IJobCostingPlugIn") != null || WorkflowProviderType.GetInterface(nameof(IJobInvoicingPlugIn)) != null;
			}
		}

		public virtual bool SupportsIncludeChargeInProfitShare
		{
			get
			{
				return DocumentBusinessContext.Length > 0 && DocumentBusinessContext[0] == BusinessContext.Shipment;
			}
		}

		public virtual bool SupportsAutoFinalisation => IsWhsPick;
		public virtual bool SupportsPrintAllPackageLabels => IsWhsPick;

		bool IsWhsPick
		{
			get { return WorkflowProviderType.GetInterface(nameof(IWhsPick)) != null; }
		}

		public virtual bool SupportsApplyWorkflowTemplate => SupportsWorkflowTemplates;

		public bool SupportsWorkflowTriggerActionApplyTag(IBusiness businessObject)
		{
			if (businessObject is IWorkflowProvider workflowProvider)
			{
				var workflowType = workflowProvider.WorkflowType;
				if (workflowProvider is ProcessTaskTemplate template)
				{
					workflowType = template.P0_ProcessType;
				}
				return ProcessJobHeaderProvider.SupportsPAVE(workflowType, businessObject.Factory);
			}

			return false;
		}

		public virtual bool SupportsAddEConversationMessages => false;

		public bool SupportsWtaEnrolmentTriggerAction => IsClient(Clients.EDI) && SupportsWtaEnrolmentTriggerActionCore;
		protected virtual bool SupportsWtaEnrolmentTriggerActionCore => false;

		protected internal virtual GlbStaff GetStaffForWtaEnrolment(BusinessObject parent) => null;

		internal static bool IsClient(Clients client) => ClientHookLoader.Instance?.Client == client;

		public virtual bool SupportsHVLVPreScreening => false;

		public virtual bool SupportsConvertToShipment => false;

		public virtual bool SupportsDelayedEvents => true;

		public bool SupportsSendCalculateCO2EmissionRequest => WorkflowProviderType.GetInterface(nameof(ICO2eCalculationSupporter)) != null && SupportsSendCalculateCO2EmissionRequestCore;
		protected virtual bool SupportsSendCalculateCO2EmissionRequestCore => ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;

		protected internal virtual bool SupportEmmaMessageGeneration => false;

		#region MessagingTriggerParties

		public MessageRecipientPartyTypeList GetMessagingTriggerPartiesList(ZString triggerAction, IBaseTrigger trigger, IBusiness business)
		{
			return GetMessagingTriggerPartiesListCore(triggerAction, trigger, business);
		}

		protected virtual MessageRecipientPartyTypeList GetMessagingTriggerPartiesListCore(ZString triggerAction, IBaseTrigger trigger, IBusiness business)
		{
			var messageRecipientPartyType = MessageRecipientPartyType.None;

			if (WorkflowTriggerActionTypeConstants.ValidateForCustomsMessagingCodes.Contains(triggerAction.ToString()))
			{
				messageRecipientPartyType = MessageRecipientPartyType.Email;
			}
			else if (IsMessagingOrEmailNotificationTriggerAction(triggerAction))
			{
				messageRecipientPartyType = SupportedMessageRecipientPartiesForSpecificAction(triggerAction);

				if (messageRecipientPartyType == MessageRecipientPartyType.None)
				{
					messageRecipientPartyType = SupportedMessageRecipientParties(trigger, business);
				}
			}
			else if (IsManifestMessagingTriggerAction(triggerAction))
			{
				messageRecipientPartyType = SupportedManifestMessageRecipientParties(trigger);
			}
			else if (IsPrintingTriggerAction(triggerAction))
			{
				messageRecipientPartyType = SupportedSendDocumentMessageRecipientParties(triggerAction);
			}

			return new MessageRecipientPartyTypeList(messageRecipientPartyType);
		}

		#endregion

		#endregion

		#region Get Workflow Trigger Action

		public class LogAction : IProcessor
		{
			public LogAction(string log)
			{
				this.log = log;
			}
			readonly string log;

			public void Process(INotifications notifications, CancellationToken token = default) => notifications.Add(CargoWise.ComponentModel.NotificationType.Information, log);
		}

#if DEBUG
		public IProcessor GetWorkflowTriggerAction(ProcessTaskNotification notification, IQueuedLog log)
		{
			return GetWorkflowTriggerAction(new WorkflowTriggerActionSource(notification?.Parent?.GetJob(), notification?.Parent, notification, new WorkflowTriggerEventData(new ExampleLog(notification)), null), log);
		}
#endif

		public IProcessor GetWorkflowTriggerAction(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			return GetWorkflowTriggerActionCore(source, queuedLog);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "I didn't want to refactor this method.")]
		protected virtual IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var trigger = source.Trigger;
			var action = source.Action;
			var parent = source.Job;
			var workflowProvider = source.Job as IWorkflowProvider;
			var processor = GetProcessorForTriggerActionType();
			HookGetProcessorForTriggerActionTypeForUnitTests(ref processor, source, action.PQ_TriggerType);
			return processor;

			ActionWrapper GetActionWrapper() => new ActionWrapper(action, parent, source.EventProvider, queuedLog);

			IProcessor GetProcessorForTriggerActionType()
			{
				switch (action.PQ_TriggerType)
				{
					case WorkflowTriggerActionTypeConstants.Codes.SendDocument:
						return ObjectFactory.New<ISendDocumentsTriggerActionRunnerFactory>().GetNewRunner(action, parent, Lazy.Create(() => source.Event));
					case WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs:
						return ObjectFactory.New<IAddDocumentToEDocsTriggerActionRunnerFactory>().GetNewRunner(action, parent);
					case WorkflowTriggerActionTypeConstants.Codes.NotificationEmail:
						return GetWorkflowTriggerForNotificationEmail(action, parent, source);
					case WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail:
						return GetWorkflowTriggerForNotificationBodyEmail(action, parent, source);
					case WorkflowTriggerActionTypeConstants.Codes.SendEDocXml:
						return GetWorkflowTriggerForEDocXml(action, new EventInfoProvider(queuedLog, trigger, parent), parent, source.TriggerEvent);
					case WorkflowTriggerActionTypeConstants.Codes.SendNativeXML:
						return ObjectFactory.New<INativeXmlWorkflowProcessor>(GetActionWrapper(), Lazy.Create(() => GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML)), new EventInfoProvider(queuedLog, trigger, parent));
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML:
						if (SupportsWorkflowTriggerActionUniversalShipmentXML)
						{
							return new UniversalShipmentTriggerActionBuilder(this, GetActionWrapper(), new EventInfoProvider(queuedLog, trigger, parent)).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Shipment XML is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc:
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML:
						if (SupportsWorkflowTriggerActionUniversalEventXML)
						{
							var eventInfo = new EventInfoProvider(queuedLog, trigger, parent) { DoNotProvideEventReference = true };
							return new UniversalEventTriggerActionBuilder(this, GetActionWrapper(), eventInfo).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Event is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML:
						if (SupportsWorkflowTriggerActionUniversalEventCollectionXML)
						{
							var eventInfo = new EventInfoProvider(queuedLog, trigger, parent) { DoNotProvideEventReference = true };
							return new UniversalEventCollectionTriggerActionBuilder(this, GetActionWrapper(), eventInfo).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Event is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML:
						if (SupportsWorkflowTriggerActionUniversalActivityXML)
						{
							return new UniversalActivityTriggerActionBuilder(this, GetActionWrapper(), new EventInfoProvider(queuedLog, trigger, parent)).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Activity is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML:
						if (SupportsWorkflowTriggerActionUniversalTransactionXML)
						{
							return new UniversalTransactionTriggerActionBuilder(this, GetActionWrapper(), new EventInfoProvider(queuedLog, trigger, parent)).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Transaction is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML:
						if (SupportsWorkflowTriggerActionUniversalScheduleXML)
						{
							return new UniversalScheduleTriggerActionBuilder(this, GetActionWrapper(), new EventInfoProvider(queuedLog, trigger, parent)).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Schedule is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML:
						if (SupportsWorkflowTriggerActionUniversalTransactionBatchXML)
						{
							return new UniversalTransactionBatchTriggerActionBuilder(this, GetActionWrapper(), new EventInfoProvider(queuedLog, trigger, parent)).GetUniversalWorkflowProcessor();
						}
						return new LogAction((NoResString)"Universal Transaction Batch is not supported by this job type.");
					case WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue:
						return GetWorkflowTriggerForAutoRateCostsAndRevenue(workflowProvider, autoRateRevenue: true, autoRateCosts: true, excludeConsolLevelCharges: false);
					case WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts:
						return GetWorkflowTriggerForAutoRateCostsAndRevenue(workflowProvider, autoRateRevenue: false, autoRateCosts: true, excludeConsolLevelCharges: false);
					case WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue:
						return GetWorkflowTriggerForAutoRateCostsAndRevenue(workflowProvider, autoRateRevenue: true, autoRateCosts: false, excludeConsolLevelCharges: false);
					case WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts:
						return GetWorkflowTriggerForAutoRateCostsAndRevenue(workflowProvider, autoRateRevenue: false, autoRateCosts: true, excludeConsolLevelCharges: true);
					case WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue:
						return GetWorkflowTriggerForAutoRateCostsAndRevenue(workflowProvider, autoRateRevenue: true, autoRateCosts: true, excludeConsolLevelCharges: true);
					case WorkflowTriggerActionTypeConstants.Codes.AutoPack:
						return GetWorkflowTriggerForAutoPack(workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking:
					case WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer:
						return GetWorkflowTriggerForCreateTransportBooking(action, workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob:
						return GetWorkflowTriggerForCreateTransportJob(action, workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.ConvertToShipment:
						return GetWorkflowTriggerForConvertToShipment(workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader:
					case WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges:
					case WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges:
					case WorkflowTriggerActionTypeConstants.Codes.PostAllCosts:
					case WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue:
					case WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly:
					case WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly:
					case WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue:
					case WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges:
					case WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare:
					case WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies:
						return GetAccountingTriggerActionWithOptionalFactorySaveProcessor(action, parent, queuedLog);
					case WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost:
						return GetWorkflowTriggerForTransactionAllocationAndPost(action, parent, queuedLog);
					case WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange:
					case WorkflowTriggerActionTypeConstants.Codes.SetField:
						return ObjectFactory.Get<IWorkflowTriggerActionProcessorCreator>().GetSetFieldProcessor(source);
					case WorkflowTriggerActionTypeConstants.Codes.SendARInvoice:
						return GetWorkflowTriggerForSendARInvoice(workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs:
						return GetWorkflowTriggerForGenerateARInvoiceToEdocs(workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML:
						return GetWorkflowTriggerForSendUniversalShipmentManifestXML(action, parent);
					case WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways:
					case WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce:
						return new WorkflowApplyTemplateProcessor(action, workflowProvider);
					case WorkflowTriggerActionTypeConstants.Codes.ApplyTag:
						return new WorkflowApplyTagProcessor(action, source.Job);
					case WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage:
						return new AddEConversationTriggerActionRunner(action, parent, source.EventProvider, isMessageInternalOnly: false);
					case WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage:
						return new AddEConversationTriggerActionRunner(action, parent, source.EventProvider, isMessageInternalOnly: true);
					case WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent:
						return new WorkflowDelayedEventScheduler(action, parent, queuedLog);
					case WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail:
						return new AssignStaffAndEmail(source, GetWorkflowTriggerForNotificationEmail);
					case WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML:
						return new LogAction($"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML} is no real use.");
					case WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse:
						return new EnrolInWiseTechAcademyCourseTriggerActionRunner(action, parent);
					case WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest:
						return ObjectFactory.New<ICO2eCalculationRequestProcessor>(parent,
							new ActionWrapper(action, parent, source.EventProvider, queuedLog),
							new EventInfoProvider(queuedLog, source.Trigger, parent),
							default, default, false, default);
					default:
						var triggerType = action.PQ_TriggerType.ToString();
						if (GetSupportsValidateForCustomsMessagingTriggerActions(trigger, parent).Contains(triggerType))
						{
							return new CustomsMessageValidationProcessor(action, parent);
						}
						else if (GetScheduleDeferredMessageSendTriggerActions(trigger, parent).Contains(triggerType))
						{
							return ObjectFactory.New<Enterprise.Integration.Customs.Shared.IDeferredMessageSchedulingProcessor>(parent, action, queuedLog.SJ_GS_NKUser) as IProcessor;
						}
						return GetWorkflowTriggerActionCore(source);
				}
			}
		}

		public (IEDICommunicationsMode[] communicationModes, MultilingualString failureReason) GetCommunicationModesForRecipient(OrgHeader recipient, EDICommunicationModeQuery modeQuery)
		{
			if (recipient == null)
			{
				throw new ArgumentNullException(nameof(recipient), "GetCommunicationModesForRecipient should not be called with null recipient");
			}

			var purpose = string.IsNullOrEmpty(modeQuery.Purpose) ? null : recipient.Factory.LoadFromNaturalKey<IEDIMessagePurpose>(EDIMessagePurposeSchema.EMP_Code, modeQuery.Purpose);
			var disableOrgProxyRecipientOverride = purpose?.EMP_DisableOrgProxyRecipientOverride == null ? new ZBool(false) : purpose.EMP_DisableOrgProxyRecipientOverride;

			if (!disableOrgProxyRecipientOverride && modeQuery.RecipientRole != MessageRecipientPartyTypeList.Codes.OrgProxy && recipient.IsProxyOrgOfAnyCompany() && MessageCanBeSentInternally(modeQuery.FileFormat))
			{
				// Internal Communication Mode
				return (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null);
			}
			else
			{
				var result = recipient.EDICommunicationsModes.FindModes(modeQuery);
				if (result.Length == 0)
				{
					return (result, ResString.GetMultilingualString("2086bacc-c6b4-46b7-a553-7b1369e15ec9", "Organization [{0}] for Company [{1}] has no matching Communication Modes.", recipient.OH_Code, GlbCompany.CurrentCompany.GC_Code));
				}
				else
				{
					return (result, null);
				}
			}
		}

		public ZString GetFallbackForEmptyCommunicationModeProperty(ProcessTaskNotification action, CommunicationModeSubstitutorProperty property)
		{
			switch (property)
			{
				case CommunicationModeSubstitutorProperty.EmailSubject:
					{
						return GetSubjectForEmailParty(action);
					}
				case CommunicationModeSubstitutorProperty.FileName:
					{
						return GetFileNameForEmailParty(action);
					}
			}

			return ZString.Empty;
		}

		bool MessageCanBeSentInternally(string fileFormat)
		{
			return fileFormat != EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		}

		protected bool TryGetTriggeringLogFromQueuedLog(IQueuedLog queuedLog, IBaseTrigger trigger, BusinessObject parent, out StmALog result)
		{
			return TriggeringLogFinder.TryFindLog(new WorkflowTriggerEventData(queuedLog), trigger, parent, out result, wteFallbackLog: queuedLog);
		}

		#region Universal Data Buss Hooks

		#region UniversalShipment

		internal protected virtual bool SupportsWorkflowTriggerActionUniversalShipmentXML
		{
			get { return UniversalDataContextManager.ManagesShipments(); }
		}

		public ITopLevelDataObjectWriter GetUniversalShipmentDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var shipmentManager = UniversalDataContextManager as IShipmentDataContextManager;
			return shipmentManager != null ? shipmentManager.GetShipmentDataObjectWriter(outboundSessionTracker) : null;
		}

		#endregion

		#region UniversalEvent

		internal protected virtual bool SupportsWorkflowTriggerActionUniversalEventXML
		{
			get { return UniversalDataContextManager.ManagesEvents(); }
		}

		public ITopLevelDataObjectWriter GetUniversalEventDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var eventManager = UniversalDataContextManager as IEventDataContextManager;
			return eventManager != null ? eventManager.GetEventDataObjectWriter(outboundSessionTracker) : null;
		}

		#endregion

		#region UniversalEventCollection

		public virtual bool SupportsWorkflowTriggerActionUniversalEventCollectionXML
		{
			get { return UniversalDataContextManager is IParentEventDataContextManager; }
		}

		internal ITopLevelDataObjectWriter GetUniversalEventCollectionDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var eventManager = UniversalDataContextManager as IParentEventDataContextManager;
			if (eventManager == null)
			{
				return null;
			}

			var result = eventManager.GetEventDataObjectWriter(outboundSessionTracker);
			if (result != null)
			{
				result.PopulateAdditionalContexts = true;
			}
			return result;
		}

		#endregion

		#region UniversalTransactionBatch

		public virtual bool SupportsWorkflowTriggerActionUniversalTransactionBatchXML
		{
			get { return UniversalDataContextManager.ManagesTransactionBatches(); }
		}

		public ITopLevelDataObjectWriter GetUniversalTransactionBatchDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var batchManager = UniversalDataContextManager as ITransactionBatchDataContextManager;
			if (batchManager != null)
			{
				return batchManager.GetTransactionBatchDataObjectWriter(outboundSessionTracker);
			}

			return null;
		}

		#endregion

		#region UniversalTransaction

		public virtual bool SupportsWorkflowTriggerActionUniversalTransactionXML
		{
			get { return UniversalDataContextManager.ManagesTransactions(); }
		}

		public ITopLevelDataObjectWriter GetUniversalTransactionDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var transactionManager = UniversalDataContextManager as ITransactionDataContextManager;
			return transactionManager == null ? null : transactionManager.GetTransactionDataObjectWriter(outboundSessionTracker);
		}

		#endregion

		#region UniversalSchedule

		public virtual bool SupportsWorkflowTriggerActionUniversalScheduleXML
		{
			get { return UniversalDataContextManager.ManagesSchedules(); }
		}

		public ITopLevelDataObjectWriter GetUniversalScheduleDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var scheduleManager = UniversalDataContextManager as IScheduleDataContextManager;
			return scheduleManager == null ? null : scheduleManager.GetScheduleDataObjectWriter(outboundSessionTracker);
		}

		#endregion

		#region UniversalActivity

		public bool SupportsWorkflowTriggerActionUniversalActivityXML => UniversalDataContextManager.ManagesActivities();

		public ITopLevelDataObjectWriter GetUniversalActivityDataObjectWriter(IDataWritingManager outboundSessionTracker)
		{
			var activityManager = UniversalDataContextManager as IActivityDataContextManager;
			return activityManager?.GetActivityDataObjectWriter(outboundSessionTracker, shouldIncludeRelatedItems: true);
		}

		#endregion

		#region UniversalDataContextManager

		IDataContextManager universalDataContextManager;
		protected internal IDataContextManager UniversalDataContextManager
		{
			get { return universalDataContextManager ?? (universalDataContextManager = GetUniversalDataContextManager()); }
		}

		IDataContextManager GetUniversalDataContextManager()
		{
			return WorkflowProviderType.GetUniversalDataContextManager() ?? new NullDataContextManager();
		}

		class NullDataContextManager : IDataContextManager
		{
			public Type TopLevelBusinessObjectType => null;

			void IDataContextManager.Init(BusinessObject parent)
			{
				throw new InvalidOperationException("Should never call Init() on a NullDataContextManager.");
			}

			DataContextType IEntityID.DataContextType
			{
				get { throw new InvalidOperationException("Should never call DataContextType getter on a NullDataContextManager."); }
			}

			string IEntityID.DataContextKey
			{
				get { throw new InvalidOperationException("Should never call JobNumber getter on a NullDataContextManager."); }
			}

			string IDataContextManager.DefaultOutputDirectory
			{
				get { return null; }
			}

			public bool ModuleHasReferenceAndPartyIDMatchingEnabled
			{
				get { return false; }
			}

			IUniversalXmlSchema IDataContextManager.SchemaOverride
			{
				get { return null; }
			}

			public BusinessObject LoadBusinessObjectFromDataSource(ITopLevelDataObject topLevelDataObject, IDataSourceDataObject dataSource, BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				throw new InvalidOperationException("Should never call LoadBusinessObjectFromDataSource on a NullDataContextManager.");
			}

			public BusinessObject LoadBusinessObjectFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				throw new InvalidOperationException("Should never call LoadBusinessObjectFromDataTarget on a NullDataContextManager.");
			}

			public BusinessObject[] LoadBusinessObjectsFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				throw new InvalidOperationException("Should never call LoadBusinessObjectsFromDataTarget on a NullDataContextManager.");
			}
		}

		#endregion

		#region GetTestFileWriter

		public IUniversalXmlTestFileWriter GetTestFileWriter(ProcessTaskNotification action, BusinessObject parent)
		{
			IUniversalXmlTestFileWriter testFileWriter = null;
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null;

			switch (action.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML:
					if (SupportsWorkflowTriggerActionUniversalShipmentXML)
					{
						var actionInfo = new ActionWrapper(action, parent, null);
						dataWriterGetter = GetUniversalShipmentDataObjectWriter;
						testFileWriter = ObjectFactory.New<IUniversalXmlTestFileWriter>(actionInfo, dataWriterGetter, parent);
					}
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc:
				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML:
					if (SupportsWorkflowTriggerActionUniversalEventXML)
					{
						var actionInfo = new ActionWrapper(action, parent, null);
						dataWriterGetter = GetUniversalEventDataObjectWriter;
						testFileWriter = ObjectFactory.New<IUniversalXmlTestFileWriter>(actionInfo, dataWriterGetter, GetDummyLogFor(action, parent));
					}
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML:
					if (SupportsWorkflowTriggerActionUniversalEventCollectionXML)
					{
						var actionInfo = new ActionWrapper(action, parent, null);
						dataWriterGetter = GetUniversalEventCollectionDataObjectWriter;
						testFileWriter = ObjectFactory.New<IUniversalXmlTestFileWriter>(actionInfo, dataWriterGetter, GetDummyLogFor(action, parent));
					}
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML:
					if (SupportsWorkflowTriggerActionUniversalTransactionXML)
					{
						var actionInfo = new ActionWrapper(action, parent, null);
						dataWriterGetter = GetUniversalTransactionDataObjectWriter;
						testFileWriter = ObjectFactory.New<IUniversalXmlTestFileWriter>(actionInfo, dataWriterGetter, parent);
					}
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML:
					if (SupportsWorkflowTriggerActionUniversalTransactionBatchXML)
					{
						var actionInfo = new ActionWrapper(action, parent, null);
						dataWriterGetter = (outboundSessionTracker) => GetUniversalTransactionBatchDataObjectWriter(outboundSessionTracker);
						testFileWriter = ObjectFactory.New<IUniversalXmlTestFileWriter>(actionInfo
							, dataWriterGetter
							, parent);
					}
					break;
			}

			return testFileWriter;
		}

		BaseStmALog GetDummyLogFor(ProcessTaskNotification action, IBusiness parent)
		{
			var result = new BusinessObjectFactory().New<BaseStmALog>();
			result.SL_Table = parent.TableName;
			result.SL_Parent = parent.Identifier;
			result.SL_EventTime = ZDateTime.Now;
			result.SL_Reference = action.Parent.TriggerConditionValue;
			result.SL_SE_NKEvent = action.Parent.TriggerEventCode;
			return result;
		}

		#endregion

		#endregion

		#region WorkflowTriggers

		IMessageProcessor GetWorkflowTriggerForNotificationEmail(ProcessTaskNotification action, BusinessObject parent, WorkflowTriggerActionSource source)
		{
			var modes = Lazy.Create(() => GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, source.TriggerEvent, source.EventProvider), EDICommunicationsModeFileFormatList.Codes.NotificationEmail));
			return GetWorkflowTriggerForNotificationEmail(modes, action, parent, source.EventProvider);
		}

		IMessageProcessor GetWorkflowTriggerForNotificationBodyEmail(ProcessTaskNotification action, BusinessObject parent, WorkflowTriggerActionSource source)
		{
			var modes = Lazy.Create(() => GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, source.TriggerEvent, source.EventProvider), EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail));
			return GetWorkflowTriggerForNotificationEmail(modes, action, parent, source.EventProvider);
		}

		protected virtual WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			return new WorkflowTriggerNotification(modes, action, parent, logProvider);
		}

		IMessageProcessor GetWorkflowTriggerForEDocXml(ProcessTaskNotification action, EventInfoProvider eventInfoProvider, BusinessObject parent, WorkflowTriggerEventData @event)
		{
			if (parent as IDocManagerSupport == null)
			{
				return null;
			}

			var xmlModes = GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, @event, null), EDICommunicationsModeFileFormatList.Codes.EXL);
			var exlMessageDeliveryCreator = ObjectFactory.Get<IExlMessageDeliveryCreator>();
			return exlMessageDeliveryCreator.CreateExlMessageDelivery(xmlModes, parent, action, eventInfoProvider);
		}

		IProcessor GetWorkflowTriggerForAutoRateCostsAndRevenue(IWorkflowProvider parent, bool autoRateRevenue, bool autoRateCosts, bool excludeConsolLevelCharges)
		{
			var autoRaterCreator = ObjectFactory.Get<IWorkflowAutoRaterCreator>();
			return autoRaterCreator.CreateWorkflowAutoRater(parent, autoRateRevenue, autoRateCosts, excludeConsolLevelCharges);
		}

		IProcessor GetWorkflowTriggerForAutoPack(IWorkflowProvider parent)
		{
			var autoPackCreator = ObjectFactory.Get<IWhsOrderAutoPackProcessorCreator>();
			return autoPackCreator.CreateOrderAutoPackProcessor(parent);
		}

		IProcessor GetWorkflowTriggerForCreateTransportBooking(ProcessTaskNotification action, IWorkflowProvider parent)
		{
			var transportBookingCreator = ObjectFactory.Get<IDtbBookingProcessorCreator>();
			return transportBookingCreator.CreateDtbBookingProcessor(parent, action);
		}

		IProcessor GetWorkflowTriggerForCreateTransportJob(ProcessTaskNotification action, IWorkflowProvider parent)
		{
			var transportJobCreator = ObjectFactory.Get<IBookingToTransportJobProcessorCreator>();
			return transportJobCreator.CreateBookingToTransportJobProcessor(parent, action);
		}

		IProcessor GetWorkflowTriggerForTransactionAllocationAndPost(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog)
		{
			return ObjectFactory.Get<ITransactionAllocationAndPostProcessorCreator>().Create(triggerAction, parent, queuedLog);
		}

		IProcessor GetWorkflowTriggerForConvertToShipment(IWorkflowProvider parent)
		{
			return ObjectFactory.Get<IConvertToShipmentCreator>().CreateConvertToShipmentProcessor(parent);
		}

		IProcessor GetWorkflowTriggerForSendARInvoice(IWorkflowProvider parent)
		{
			return ObjectFactory.Get<ISendARInvoiceProcessorCreator>().CreateSendARInvoiceProcessor(parent);
		}

		IProcessor GetWorkflowTriggerForGenerateARInvoiceToEdocs(IWorkflowProvider parent)
		{
			return ObjectFactory.Get<IGenerateARInvoiceToEdocsProcessorCreator>().GenerateARInvoiceToEdocsProcessor(parent);
		}

		IProcessor GetWorkflowTriggerForSendUniversalShipmentManifestXML(ProcessTaskNotification action, BusinessObject parent)
		{
			return ObjectFactory.New<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestUniversalMessagingProcessor>(parent, action.PQ_Calc_TriggerParty);
		}

		IProcessor GetAccountingTriggerActionWithOptionalFactorySaveProcessor(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog)
		{
			return ObjectFactory.Get<IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator>().Create(triggerAction, parent, queuedLog);
		}

		/// <summary>
		/// Default Action: Return Null
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		// Should be GetWorkflowTriggerActionDefault
		protected virtual IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource sourcen)
		{
			return null;
		}

		#endregion

		#endregion

		#endregion

		#region RecipientService

		public bool IsRecipientServiceAvailable(ZString action)
		{
			return WorkflowTriggerActionTypeConstants.IsXmlUniversalShipment(action);
		}

		public bool IsRecipientServiceMandatory(ZString action, ZString recipient)
		{
			var result = false;

			if (IsRecipientServiceAvailable(action) && !recipient.IsEmpty)
			{
				var supportedRecipientServices = SupportedTriggerPartyServices(recipient);
				if (supportedRecipientServices != null)
				{
					var hasAtLeastOneValidRecipientService = supportedRecipientServices.Any(s => !s.IsEmpty);
					var supportsBlankRecipientService = supportedRecipientServices.Any(s => s.IsEmpty);
					result = hasAtLeastOneValidRecipientService && !supportsBlankRecipientService;
				}
			}

			return result;
		}

		#endregion

		#region GetMessageTriggerEdiCommunicationsModes / GetMessageTriggerParties / MessageTriggerPartiesVisibilityControl

		protected MessageProcessorCommunicationModesResult GetMessageRecipientEdiCommunicationsModes(WorkflowTriggerActionSource source, ZString fileFormat)
		{
			var triggerParty = source.Action.PQ_Calc_TriggerParty;

			if (IsDirectEmailRecipient(triggerParty))
			{
				return GetMessageRecipientEdiCommunicationsModesForEmailParty(source);
			}
			else
			{
				if (triggerParty == MessageRecipientPartyTypeList.SpecialCodes.Other)
				{
					var messageRecipientParty = new MessageRecipientPartyCollection();
					messageRecipientParty.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(source.Action.Recipient, ZString.Empty));
					return GetMessageRecipientEdiCommunicationsModes(source, fileFormat, messageRecipientParty);
				}
				else
				{
					var (recipient, failureReason) = GetMessageRecipientPartyWithFailureReason(source.Job, source.Action.PQ_Calc_TriggerParty);

					if (recipient == null || !recipient.Any())
					{
						if (failureReason != null && failureReason.IsValid)
						{
							return new MessageProcessorCommunicationModesResult(Array.Empty<IEDICommunicationsMode>(), failureReason);
						}
						else
						{
							// If you are here, make sure that GetMessageRecipientPartyWithFailureReason is guaranteed to provide either a failure reason or a recipient party
							return new MessageProcessorCommunicationModesResult(Array.Empty<IEDICommunicationsMode>(), ResString.GetMultilingualString("25d39a20-eb46-40aa-96ab-ed6ee10c3da1", "No possible recipients with this configuration."));
						}
					}
					else
					{
						return GetMessageRecipientEdiCommunicationsModes(source, fileFormat, recipient);
					}
				}
			}
		}

		protected MessageProcessorCommunicationModesResult GetMessageRecipientEdiCommunicationsModes(WorkflowTriggerActionSource source, ZString fileFormat, MessageRecipientPartyCollection messageRecipientParty)
		{
			var action = source.Action;
			var communicationModes = new List<IEDICommunicationsMode>();
			var messages = new List<MultilingualString>();
			var modeQuery = EDICommunicationModeQuery.FromAction(source.Job, source.Action, source.Event, this, fileFormat);
			foreach (var recipient in messageRecipientParty)
			{
				var ediModes = recipient.Party.EDICommunicationsModes.FindModes(modeQuery);

				if (fileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationEmail ||
					fileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail)
				{
					ediModes = ediModes.Where(mode => mode.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText).ToArray();
				}

				if ((ediModes == null || ediModes.Length == 0))
				{
					if (WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.Value)
					{
						var fallbackEmail = recipient.FallbackEmail;
						if (fallbackEmail.IsEmpty && recipient.Party.MainAddress != null)
						{
							fallbackEmail = recipient.Party.MainAddress.OA_Email;
						}

						if (!fallbackEmail.IsEmpty)
						{
							var fallbackMode = GetMessageRecipientEdiCommunicationsModesForEmail(source,
								fallbackEmail, WorkflowTriggerActionTypeConstants.IsNotificationEmail(action.PQ_TriggerType)
									? EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText
									: EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment,
								GetSubjectForEmailParty(action),
								GetFileNameForEmailParty(action),
								fileFormat);

							communicationModes.Add(fallbackMode);
							messages.Add(ResString.GetMultilingualString("50b0c412-75c0-471a-9029-a3f4638d58f2", "Organization [{0}] for Company [{1}] falls back to Organization email.", recipient.Party.OH_Code, Env.CurrentCompany.Code));
							continue; /// <--- Look. This is just to skip adding the log.
						}
					}
					messages.Add(ResString.GetMultilingualString("6a540575-53ac-4152-a442-21382a9e93b6", "Organization [{0}] for Company [{1}] has no matching Communication Modes.", recipient.Party.OH_Code, Env.CurrentCompany.Code));
				}
				else
				{
					messages.Add(ResString.GetMultilingualString("7e8247dc-6e0d-4298-b3e8-efc3670c9f69", "Organization [{0}] for Company [{1}] has matching Communication Modes.", recipient.Party.OH_Code, Env.CurrentCompany.Code));
					communicationModes.AddRange(ediModes);
				}
			}

			return new MessageProcessorCommunicationModesResult(communicationModes, ResourceString.Join(System.Environment.NewLine, messages.ToArray()));
		}

		protected internal virtual bool IsDirectEmailRecipient(string triggerParty) => triggerParty == MessageRecipientPartyTypeList.Codes.Email;
		protected virtual string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent) => new[] { action.PQ_EmailAddr.ToString() };

#if DEBUG
		internal
#endif
		protected virtual MessageProcessorCommunicationModesResult GetMessageRecipientEdiCommunicationsModesForEmailParty(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			var parent = source.Job;
			var communicationMode = (WorkflowTriggerActionTypeConstants.IsNotificationEmail(action.PQ_TriggerType) ||
									action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail)
				? EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText
				: EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			var subject = GetSubjectForEmailParty(action);
			var fileName = GetFileNameForEmailParty(action);

			var modes = GetNotificationEmailAddresses(action, parent)
				.Select(email => GetMessageRecipientEdiCommunicationsModesForEmail(source, email, communicationMode, subject, fileName,
						action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail ?
						EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail :
						EDICommunicationsModeFileFormatList.Codes.NotificationEmail))
				.ToArray();

			if (modes.Length == 0)
			{
				return new MessageProcessorCommunicationModesResult(modes, ResString.GetMultilingualString("c49188fc-b75a-4772-a1bc-df603a60d2ad", "No address found for Trigger Party [{0}]", action.PQ_TriggerParty));
			}
			else
			{
				return new MessageProcessorCommunicationModesResult(modes, ResString.GetMultilingualString("e854876a-ffcc-4552-b150-e3b17cfa5c4c", "Address taken from Trigger Party [{0}]", action.PQ_TriggerParty));
			}
		}

		IEDICommunicationsMode GetMessageRecipientEdiCommunicationsModesForEmail(WorkflowTriggerActionSource source, ZString destination, ZString communicationMode, ZString subject, ZString fileName, ZString fileFormat)
		{
			destination = TriggerActionCommunicationModeSubstitutor.Substitute(source.Action, source.Job, source.Event, CommunicationModeSubstitutorProperty.EmailAddress, destination);
			return new NonPersistentEDICommunicationMode()
			{
				EK_Destination = destination,
				EK_CommunicationsTransport = communicationMode,
				EK_FileFormat = fileFormat,
				EK_ServerAddressSubject = subject,
				EK_Filename = fileName,
			};
		}

		protected virtual string GetSubjectForEmailParty(ProcessTaskNotification action)
		{
			return Description + " - " + action.Parent.GetDescriptionWithReference();
		}

		protected string GetFileNameForEmailParty(ProcessTaskNotification action)
		{
			var type = action.PQ_TriggerType;
			if (WorkflowTriggerActionTypeConstants.IsStandardXml(type)
				|| WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(type)
				|| WorkflowTriggerActionTypeConstants.IsSendEDocXml(type))
			{
				return $"{EDIMessageDelivery.ReplacementConstants.JobNumber}.xml";
			}

			return string.Empty;
		}

		public MessageRecipientPartyCollection GetMessageRecipientParty(BusinessObject bizobj, ZString partyType)
		{
			return GetMessageRecipientPartyWithFailureReason(bizobj, partyType).recipients;
		}

		internal (MessageRecipientPartyCollection recipients, MultilingualString failureReason) GetMessageRecipientPartyWithFailureReason(BusinessObject bizobj, ZString partyType)
		{
			var messageRecipientParties = new MessageRecipientPartyCollection();
			var currentCompany = GlbCompany.CurrentCompany;
			if (partyType == MessageRecipientPartyTypeList.Codes.OrgProxy)
			{
				var orgProxy = currentCompany.OrgProxy;
				if (orgProxy != null)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(orgProxy, ZString.Empty));
					return (messageRecipientParties, null);
				}
				else
				{
					return (messageRecipientParties, ResString.GetMultilingualString("192a407f-2350-45e7-93cb-59c9646360f4", "Company [{0}] has no Organization Proxy", currentCompany.GC_Code));
				}
			}
			else if (!AddLinkedJobHeaderPartiesToMessageRecipientPartyList(messageRecipientParties, bizobj, partyType))  // TODO: Check if fallback is really needed here.
			{
				AddToMessageRecipientPartyList(messageRecipientParties, bizobj, partyType);
			}

			return (messageRecipientParties, null);
		}

		protected internal virtual void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
		}

		protected bool AddLinkedJobHeaderPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			if (partyType != MessageRecipientPartyTypeList.Codes.BillToParty)
			{
				return false;
			}

			var jobHeaderParent = bizObj as IJobHeaderParent;
			if (jobHeaderParent == null)
			{
				return false;
			}

			var job = new JobHeader.Loader(jobHeaderParent).Load();
			if (job == null)
			{
				return false;
			}

			messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(job.LocalCharges, ZString.Empty));
			return true;
		}

		public virtual bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return
				WorkflowTriggerActionTypeConstants.IsStandardXml(triggerAction) ||
				WorkflowTriggerActionTypeConstants.IsDebtorBalanceXml(triggerAction) ||
				WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(triggerAction) ||
				WorkflowTriggerActionTypeConstants.ValidateForCustomsMessagingCodes.Contains(triggerAction.ToString()) ||
				WorkflowTriggerActionTypeConstants.IsNotificationEmail(triggerAction) ||
				triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml ||
				WorkflowTriggerActionTypeConstants.IsCreateTransportBooking(triggerAction) ||
				WorkflowTriggerActionTypeConstants.IsCreateTransportBookingContainer(triggerAction) ||
				WorkflowTriggerActionTypeConstants.IsCreateTransportJob(triggerAction) ||
				triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendEDocXml ||
				triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML;
		}

		public virtual bool IsEmailSendNotificationTriggerAction(ZString triggerAction)
		{
			return WorkflowTriggerActionTypeConstants.IsEmailSendAction(triggerAction);
		}

		public virtual bool IsDocumentTriggerAction(ZString triggerAction)
		{
			return triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument
				|| triggerAction == WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
		}

		public virtual bool IsManifestMessagingTriggerAction(ZString triggerAction)
		{
			return triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML;
		}

		public virtual bool IsPrintingTriggerAction(ZString triggerAction)
		{
			return WorkflowTriggerActionTypeConstants.IsSendDocument(triggerAction);
		}

#if DEBUG
		internal
#endif
		protected virtual bool IsPrintOnlyTriggerAction(ZString triggerAction)
		{
			return false;
		}

		#endregion

		#region Form Customisation

		public FormCustomisationSettingsProvider FormCustomisationSettings
		{
			get { return fFormCustomisationSettings ?? (fFormCustomisationSettings = GetFormCustomisationSettingsProvider()); }
		}
		FormCustomisationSettingsProvider fFormCustomisationSettings;

		protected virtual FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider() { return null; }

		string[] IWorkflowDescriptor.GetPropertiesThatAffectWorkflow() => FormCustomisationSettings != null ? FormCustomisationSettings.PropertiesThatAffectWorkflow : Array.Empty<string>();

		#endregion

		#region GenCustomColumnDefinition Additional Validation

		protected internal virtual AutoGenCustomColumnDefinitionValidation GetAdditionalCustomColumnDefinitionValidation(GenCustomColumnDefinition customColumnDefinition)
		{
			return null;
		}

		#endregion

		#region Events Context

		public BusinessObjectEventDataModel GetEventDataModel(BusinessObject businessObject)
		{
			return GetEventDataModelCore(businessObject);
		}

		protected virtual BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new BusinessObjectEventDataModel(businessObject);
		}

		public bool IsRelatedEntityInContext(BusinessObject entity, BusinessObject relatedEntity, ZString contextString)
		{
			try
			{
				return IsRelatedEntityInContextCore(entity, relatedEntity, GetContextPathFromString(contextString));
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

		protected virtual bool IsRelatedEntityInContextCore(BusinessObject entity, BusinessObject relatedEntity, IEnumerable<WorkflowEventContextPair> contextPath)
		{
			return false;
		}

		public ZString GetContextStringFromPath(IEnumerable<WorkflowEventContextPair> contextPath)
		{
			StringBuilder sb = new StringBuilder();
			foreach (WorkflowEventContextPair t in contextPath)
			{
				if (sb.Length > 0)
				{
					sb.Append(ContextStepsSeparator);
				}
				if (!string.IsNullOrEmpty(t.MasterClassifier.Code))
				{
					sb.Append(t.MasterClassifier.Code);
					sb.Append(ContextPartsSeparator);
				}
				sb.Append(t.MasterType.Code);
			}
			return sb.ToString();
		}

		public IEnumerable<WorkflowEventContextPair> GetContextPathFromString(string contextString, bool exceptionOnWrongStep = true)
		{
			var contextPath = new List<WorkflowEventContextPair>();

			if (!string.IsNullOrEmpty(contextString))
			{
				foreach (var stepString in contextString.Trim(ContextStepsSeparator).Split(ContextStepsSeparator))
				{
					string[] stepParts = stepString.Trim(ContextPartsSeparator).Split(ContextPartsSeparator);
					string masterClassifier;
					string masterType;

					if (stepParts.Length == 2)
					{
						masterClassifier = stepParts[0];
						masterType = stepParts[1];
					}
					else if (stepParts.Length == 1)
					{
						masterClassifier = string.Empty;
						masterType = stepParts[0];
					}
					else
					{
						throw new InvalidOperationException("Wrong number of parts in '" + stepString + "' in context path '" + contextString + "'.");
					}

					var availableSteps = GetFollowingContextSteps(contextPath);
					var nextStep = availableSteps != null ? availableSteps.FirstOrDefault(c => c.MasterClassifier.Code == masterClassifier && c.MasterType.Code == masterType) : null;
					if (nextStep == null)
					{
						if (exceptionOnWrongStep)
						{
							throw new InvalidOperationException("Incorrect step '" + stepString + "' in context path '" + contextString + "'.");
						}

						nextStep = new WorkflowEventContextPair(new CodeDescriptionPair(masterClassifier, null), new CodeDescriptionPair(masterType, null));
					}

					contextPath.Add(nextStep);
				}
			}

			return contextPath;
		}

		public const char ContextStepsSeparator = ',';
		public const char ContextPartsSeparator = ' ';

		public IEnumerable<WorkflowEventContextPair> GetFollowingContextSteps(IEnumerable<WorkflowEventContextPair> currentContextPath)
		{
			return GetFollowingContextStepsCore(currentContextPath);
		}

		protected virtual IEnumerable<WorkflowEventContextPair> GetFollowingContextStepsCore(IEnumerable<WorkflowEventContextPair> currentContextPath)
		{
			return Enumerable.Empty<WorkflowEventContextPair>();
		}

		#endregion

		#region PropertiesThatAffectWorkflowProvider

		public class PropertiesThatAffectWorkflowProvider : IPropertiesThatAffectWorkflowProvider
		{
			#region IPropertiesThatAffectWorkflowProvider Members

			public string[] GetPropertiesThatAffectWorkflow(string code)
			{
				var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
				return descriptor != null && descriptor.FormCustomisationSettings != null ? descriptor.FormCustomisationSettings.PropertiesThatAffectWorkflow : Array.Empty<string>();
			}

			#endregion
		}

		#endregion

		#region WorfklowDescriptor Loader

		public new class Loader : IWorkflowDescriptorLoader
		{
			IWorkflowDescriptor IWorkflowDescriptorLoader.GetWorkflowDescriptor(ZString workflowType)
			{
				return GetWorkflowDescriptor(workflowType);
			}

			public WorkflowDescriptor GetWorkflowDescriptor(ZString workflowType)
			{
				return WorkflowDescriptors.Instance.TryGetValueSafe(workflowType);
			}
		}

		#endregion

		#region Default Date

		public bool GetLogIsValidForDateDefaulting(IStmALog log) => GetLogIsValidForDateDefaultingCore(log);

		protected virtual bool GetLogIsValidForDateDefaultingCore(IStmALog log) => true;

		public ZDateTimeOffset GetDateTimeFromParent(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType, ZDateTime dateTimeSourceOffsetToAdd)
		{
			var dateTimeOffset = GetScheduleDateTimeAndLocationForTimezoneFromParent(defaultedFromDateProvider, dateTimeSourceType);
			return AddTimespanToDateTime(dateTimeOffset, dateTimeSourceOffsetToAdd);
		}

		protected internal ZDateTimeOffset GetScheduleDateTimeAndLocationForTimezoneFromParent(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType, Lazy<RefUNLOCO> defaultLocation = null)
		{
			var dateFromPredecessor = defaultedFromDateProvider.PredecessorDefaultDate;

			if (!dateFromPredecessor.IsEmpty)
			{
				return dateFromPredecessor;
			}
			else
			{
				var (dateTime, location) = GetScheduleDateTimeAndLocationForTimezone(defaultedFromDateProvider, dateTimeSourceType);

				if (location == null && !dateTime.IsValid)
				{
					return ZDateTimeOffset.Empty;
				}

				if (location == null && dateTime.IsValid && defaultLocation != null)
				{
					location = defaultLocation.Value;
				}

				return dateTime.ToDateTimeOffset(location);
			}
		}

		protected internal virtual (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			return (ZDateTime.Empty, null);
		}

		internal static ZDateTimeOffset AddTimespanToDateTime(ZDateTimeOffset dateTime, ZDateTime timespanAsDateTime)
		{
			if (dateTime.IsValid && dateTime.IsValidSmallDateTime)
			{
				return dateTime + ConvertDateTimeToTimespan(timespanAsDateTime);
			}

			return dateTime;
		}

		internal static TimeSpan ConvertDateTimeToTimespan(ZDateTime timespanAsDateTime) => timespanAsDateTime.IsValid ? timespanAsDateTime.TimeSpan6MonthsFromStartOfYear : TimeSpan.Zero;

		#endregion

		#region SetDefaultTriggerConditions

		public void SetDefaultTriggerConditions(IMilestoneDateDefaultable defaultable, BusinessObject parent) => SetDefaultTriggerConditionsCore(defaultable, parent);

		protected virtual void SetDefaultTriggerConditionsCore(IMilestoneDateDefaultable defaultable, BusinessObject parent)
		{
			// Do nothing
		}

		#endregion

		#region IRootTypeProvider

		public IEnumerable<Type> MacroTypes(ProcessTaskNotification action) => MacroTypesCore(action);

		protected virtual IEnumerable<Type> MacroTypesCore(ProcessTaskNotification action)
		{
			yield return action.GetType();
		}

		public IEnumerable<Type> MacroTypes(ProcessTask processTask) => MacroTypesCore(processTask);

		protected virtual IEnumerable<Type> MacroTypesCore(ProcessTask processTask)
		{
			yield return processTask.GetType();
		}

		public IEnumerable<BusinessObject> MacroRoots(ProcessTaskNotification action) => MacroRootsCore(action);

		protected virtual IEnumerable<BusinessObject> MacroRootsCore(ProcessTaskNotification action)
		{
			yield return action;
		}

		public BusinessObject[] GetUDFMacroDataContext(ITriggerConditions workflowItem, BusinessObject parent) => GetUDFMacroDataContextCore(workflowItem, parent);

		protected virtual BusinessObject[] GetUDFMacroDataContextCore(ITriggerConditions workflowItem, BusinessObject parent)
		{
			return new[] { workflowItem as BusinessObject };
		}

		#endregion

		#region For Test Purposes

		partial void HookGetProcessorForTriggerActionTypeForUnitTests(ref IProcessor processor, WorkflowTriggerActionSource source, string triggerActionType);

		#endregion

		protected bool IsGlobalTemplate(IBusiness parent)
		{
			bool result = false;

			if (parent is ProcessTaskTemplate template)
			{
				result = template.GlobalTemplate;
			}

			return result;
		}

		public static ZString GetCompanyCountryCodeFromWorkflowItemOrTemplate(IWorkflowItem workflowItem, BusinessObject parent)
		{
			var result = ZString.Empty;
			if (workflowItem != null)
			{
				var company = workflowItem.GetCompany();
				if (company == null && parent is ProcessTaskTemplate template)
				{
					company = template.Branch?.Company ?? template.Company;
				}
				result = company?.GC_RN_NKCountryCode ?? ZString.Empty;
			}
			return result;
		}

#if DEBUG

		public virtual BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(WorkflowProviderType);
		}

		public void ClearFormCustomisationSettingsCacheForTest()
		{
			fFormCustomisationSettings = null;
		}

#endif
	}

	#region BOExtensions

	static class BOExtensions
	{
		internal static StmALog GetFirstMatchingLog(this BusinessObject workflowParent, ZQuery query)
		{
			var logs = workflowParent.GetLogs().Find(query);
			if (logs != null && logs.Length > 0)
			{
				return logs[0];
			}

			return null;
		}
	}

	#endregion

	public static class ConsolAirCargoWorkflowDescriptorExtensions
	{
		/// <summary>
		/// This method checks for either WorkflowItem Country, Template Branch, Template Country, or Template DischargePort.
		/// A typlical use case is for Consol plugged-in functionality, where the DischargePort enables certain customs functionality regardless of logged in company.
		/// </summary>
		/// <param name="workflowItem"></param>
		/// <param name="countryCode"></param>
		/// <returns></returns>
		public static bool IsForCountryOrTemplateDischargePortCountry(this IWorkflowItem workflowItem, BusinessObject parent, ZString countryCode)
		{
			bool result = false;

			if (workflowItem != null)
			{
				result = workflowItem.IsForCountry(parent, countryCode);

				if (!result && parent is ProcessTaskTemplate template)
				{
					result = template.P0_DischargePortCountry.StartsWith(countryCode, StringComparison.Ordinal);
				}
			}

			return result;
		}

		/// <summary>
		/// This method checks for either WorkflowItem Country, Template Branch, or Template Country.
		/// (We should never rely on logged-in company...)
		/// </summary>
		/// <param name="workflowItem"></param>
		/// <param name="countryCode"></param>
		/// <returns></returns>
		public static bool IsForCountry(this IWorkflowItem workflowItem, BusinessObject parent, ZString countryCode)
		{
			var result = false;
			if (workflowItem != null)
			{
				var companyCountryCode = WorkflowDescriptor.GetCompanyCountryCodeFromWorkflowItemOrTemplate(workflowItem, parent);
				result = companyCountryCode == countryCode;
			}
			return result;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	public delegate void GetProcessorForTriggerActionTypesOverride(ref IProcessor processor, WorkflowTriggerActionSource source, string triggerActionType);

	public abstract partial class WorkflowDescriptor
	{
		static readonly Overridable<GetProcessorForTriggerActionTypesOverride> onGetProcessorForTriggerActionTypesOverride = new Overridable<GetProcessorForTriggerActionTypesOverride>(null);

		public static void SetGetProcessorForTriggerActionTypeHookForTest(GetProcessorForTriggerActionTypesOverride onGetProcessorForTriggerActionTypes)
		{
			onGetProcessorForTriggerActionTypesOverride.Value = onGetProcessorForTriggerActionTypes;
		}

		partial void HookGetProcessorForTriggerActionTypeForUnitTests(ref IProcessor processor, WorkflowTriggerActionSource source, string triggerActionType)
		{
			onGetProcessorForTriggerActionTypesOverride.Value?.Invoke(ref processor, source, triggerActionType);
		}
	}
}
#endif
#endregion
