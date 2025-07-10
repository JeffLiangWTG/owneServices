//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTaskNotificationLookups
//
//    This class should be used for overriding collections in AutoProcessTaskNotificationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskNotificationLookups : AutoProcessTaskNotificationLookups
	{
		public ProcessTaskNotificationLookups(AutoProcessTaskNotification parent)
			: base(parent)
		{
		}

		protected new ProcessTaskNotification Parent
		{
			get { return (ProcessTaskNotification)base.Parent; }
		}

		IWorkflowProvider WorkflowProvider => Trigger?.GetJob() as IWorkflowProvider;

		IBaseTrigger Trigger => Parent.Parent;

		WorkflowDescriptor WorkflowDescriptor => Trigger?.GetWorkflowDescriptor();

		public ICodeDescriptionPairList WorkflowTriggerActionTypes
		{
			get
			{
				if (Parent == null || Parent.IsDeleted || Trigger == null)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					var workflowItem = Trigger;
					ICodeDescriptionPairList result = null;

					if (Parent.PQ_TriggerType != "STA")
					{
						result = WorkflowDescriptor?.GetWorkflowTriggerActionTypes(workflowItem, workflowItem.GetJob());
					}

					if (result == null)
					{
						result = new CodeDescriptionPairList();
					}

					return result;
				}
			}
		}

		public override ProcessTaskCollection ProcessTasks => new ProcessTaskCollection(Factory) { IsManagedForDataRefresh = false };

		public StmMenuItemCollection DocumentToSendList
		{
			get
			{
				StmMenuItemCollection result = null;

				if (Parent == null || Parent.PQ_TriggerType == "STA")
				{
					result = new DocAutoDeliverStmMenuItemCollection(new BusinessObjectFactory());
				}
				else
				{
					result = DocumentsList;
				}

				return result;
			}
		}

		StmMenuItemCollection DocumentsList
		{
			get
			{
				var documentBusinessContext = DocumentBusinessContext
					.Select(dbc => (ZString)dbc.ToString())
					.ToArray();

				var query = documentBusinessContext.Length > 0
					? new ZQuery(StmMenuItemSchema.SU_BusinessContext, documentBusinessContext)
					: new ZQuery();

				StmMenuItemCollection result = (Parent.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Print || Parent.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs)
					? new DocAutoDeliverStmMenuItemCollection(Factory, query, false)
					: new DocAutoDeliverStmMenuItemCollection(Factory, query);

				if (documentBusinessContext.Length > 0)
				{
					var contexts = ZString.Join(";", documentBusinessContext);

					result.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("767b9823-4e98-44fc-84a8-abce30e94489", "This document cannot be selected. Only '{0}' documents can be selected.", contexts));
					var filterDefault = new FilterBusinessObjectDefault("Business Context", "Property", contexts);
					result.FilterBusinessObjectDefaults.Add(filterDefault);
				}

				return result;
			}
		}

		BusinessContext[] DocumentBusinessContext
		{
			get
			{
				BusinessContext[] result = null;

				var workflowDescriptor = WorkflowDescriptor;
				var provider = workflowDescriptor as ISpecificDocumentBusinessContextProvider;

				if (provider != null)
				{
					result = provider.GetDocumentBusinessContext(WorkflowProvider);
				}
				else if (workflowDescriptor != null)
				{
					result = workflowDescriptor.DocumentBusinessContext;
				}

				return result ?? Array.Empty<BusinessContext>();
			}
		}

		/// <summary>
		/// Link To View -> Trigger-> Completion Trigger Actions -> Recipient
		/// </summary>
		public CodeDescriptionPairList MessagingTriggerPartiesList
		{
			get
			{
				var descriptor = WorkflowDescriptor;

				if (Parent == null || descriptor == null || Parent.PQ_TriggerType == "STA")
				{
					return new CodeDescriptionPairList();
				}

				var key = FormattableString.Invariant($"MessagingTriggerPartiesList{WorkflowProvider?.GetType().FullName}.{Parent.PQ_TriggerType}.{descriptor.GetType().Name}");

				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					var parentList = descriptor.GetMessagingTriggerPartiesList(Parent.PQ_TriggerType, Trigger, Trigger.GetJob());
					var isEmailSendAction = descriptor.IsEmailSendNotificationTriggerAction(Parent.PQ_TriggerType);

					foreach (PartyTypeDescriptionPair parentPair in parentList)
					{
						var isNotEmail = parentPair.Code != MessageRecipientPartyTypeList.Codes.Email;

						if (isEmailSendAction || isNotEmail)
						{
							list.AddPair(parentPair.Code, parentPair.Description);
						}
					}

					AddOtherPartyIfApplicable(list);

					return list;
				});
			}
		}

		void AddOtherPartyIfApplicable(CodeDescriptionPairList list)
		{
			var triggerType = Parent.PQ_TriggerType;
			if (WorkflowTriggerActionTypeConstants.IsXmlUniversalShipment(triggerType) ||
				WorkflowTriggerActionTypeConstants.IsXmlUniversalEvent(triggerType) ||
				WorkflowTriggerActionTypeConstants.IsXmlUniversalEventWithEdoc(triggerType) ||
				WorkflowTriggerActionTypeConstants.IsXmlUniversalEventCollection(triggerType))
			{
				list.AddPair(MessageRecipientPartyTypeList.SpecialCodes.Other, ResString.GetMultilingualString("046FF83C-7037-4D5A-B757-9ADA05A21A28", "Other"));
			}
		}

		public CodeDescriptionPairList AlternateMessagingTriggerPartiesList
		{
			get { return MessageRecipientPartyTypeList.GetAlternateMessageRecipientPartyTypeList(Factory); }
		}

		public CodeDescriptionPairList ProcessTaskTriggerPurposeList
		{
			get { return Factory.GetCachedValue("ProcessTaskTriggerPurposeList", GetNewProcessTaskTriggerPurposeList); }
		}

		internal static CodeDescriptionPairList GetNewProcessTaskTriggerPurposeList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(ZString.Empty, ZString.Empty);
			var purposes = new ReadOnlyBusinessObjectFactory()
					.Load<IEDIMessagePurpose>(new ZQuery())
					.OrderBy(e => e.EMP_Code)
					.Select(purpose => new CodeDescriptionPair((string)purpose.EMP_Code, purpose.EMP_DescriptionMultilingual))
					.ToList();
			list.AddRange(purposes);
			return list;
		}
		public CodeDescriptionPairList MacroTypeCodeList
		{
			get { return Factory.GetCachedValue("MacroTypeCodeList", GetNewMacroTypeCodeList); }
		}

		CodeDescriptionPairList GetNewMacroTypeCodeList()
		{
			var list = new CodeDescriptionPairList();

			list.AddPair(EventReferenceConditionList.Codes.ConditionWithMacros, EventReferenceConditionList.Descriptions.ConditionWithMacros);
			list.AddPair(EventReferenceConditionList.Codes.UserDefined, EventReferenceConditionList.Descriptions.UserDefined);

			return list;
		}

		#region EDIMessageDeliveryContextSelectors

		public IEDIMessageDeliveryContextSelectorCollection EDIMessageDeliveryContextSelectors
		{
			get
			{
				TryGetWorkflowType(out ZString workflowType);
				var key = FormattableString.Invariant($"IEDIMessageDeliveryContextSelectorCollection{workflowType}");
				return Factory.GetCachedValue(key, () => ObjectFactory.Get<IEDIMessageDeliveryContextSelectorCollection>("IEDIMessageDeliveryContextSelectorCollection", Factory, workflowType));
			}
		}

		#endregion

		#region TriggerPartyServices

		public CodeDescriptionPairList TriggerPartyServices
		{
			get { return WorkflowDescriptor != null && Parent.IsTriggerPartyServiceAvailable ? WorkflowListHelper.GetCachedTriggerPartyServiceList(Factory, WorkflowDescriptor, Parent.PQ_TriggerParty) : new CodeDescriptionPairList(); }
		}

		#endregion

		#region Printers

		public BusinessObjectCollection PrintQueues
		{
			get
			{
				if (printQueues == null)
				{
					ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
					printQueues = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory, filter });
					printQueues.Sort(new SortInfo(StmPrintQueueSchema.SQ_DisplayName.Name, ListSortDirection.Ascending));
				}

				return printQueues;
			}
		}
		BusinessObjectCollection printQueues;

		#endregion

		public ActiveProcessTaskTemplateCollection WorkflowTemplateList
		{
			get
			{
				var query = new ZQuery();
				var filterDefault = new FilterBusinessObjectDefault();

				if (TryGetWorkflowType(out ZString workflowType))
				{
					query.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, workflowType);
					filterDefault = new FilterBusinessObjectDefault("Workflow Type", "Property", workflowType, isRemovable: false);
				}

				var collection = new ActiveProcessTaskTemplateCollection(Factory, query);
				collection.FilterBusinessObjectDefaults.Add(filterDefault);
				return collection;
			}
		}

		public ITagMagnitudeCollection RelatedEntities
		{
			get
			{
				if (relatedEntities == null)
				{
					var tagsQuery = new ZDBOnlyQuery(typeof(ITagMagnitude));
					tagsQuery.AddToFilter(TagMagnitudeSchema.TGM_IsActive, true);

					var tagDefinitionQuery = new ZDBOnlySubQuery(typeof(ITagDefinition), TagMagnitudeSchema.TGM_TGD_Tag);
					tagDefinitionQuery.AddToFilter(TagDefinitionSchema.TGD_IsActive, true);
					tagDefinitionQuery.AddToFilter(TagDefinitionSchema.TGD_UsageScope, SQLComparisonOperator.Equal, new string[] { "ALL", "RUL" });
					tagDefinitionQuery.AddToFilter(TagDefinitionSchema.TGD_Scope, SQLComparisonOperator.Equal, new string[] { "ALL", "WFL" });

					tagsQuery.AddSubQuery(tagDefinitionQuery, JoinCondition.And);

					relatedEntities = ObjectFactory.Get<ITagMagnitudeCollection>(nameof(ITagMagnitudeCollection), Factory, tagsQuery);
				}
				return relatedEntities;
			}
		}
		ITagMagnitudeCollection relatedEntities;

		public ActiveBusinessObjectCollection<ProcessTaskTemplate> TemplateSources
		{
			get { return Factory.GetCachedValue(notificationsKey, () => new ActiveBusinessObjectCollection<ProcessTaskTemplate>(Factory)); }
		}

		const string notificationsKey = nameof(ProcessTaskNotificationLookups) + "+TemplateSources";

		public GlbStaffCollection StaffList
		{
			get { return Factory.GetCachedValue("ProcessTaskNotificationLookups.StaffList", () => new GlbStaffCollection(Factory)); }
		}

		bool TryGetWorkflowType(out ZString workflowType)
		{
			var workflowProvider = WorkflowProvider;
			if (workflowProvider is ProcessTaskTemplate template)
			{
				workflowType = template.P0_ProcessType;
				return true;
			}
			else if (workflowProvider != null)
			{
				workflowType = workflowProvider.WorkflowType;
				return true;
			}

			workflowType = ZString.Empty;
			return false;
		}
	}
}
