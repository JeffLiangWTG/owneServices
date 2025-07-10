using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("{WKR_RequestNumber}: {WKR_Summary}")]
	[UniversalDataContext(DataContextType.CustomerServiceTicket)]
	[CodeProperty(nameof(WKR_RequestNumber)), DescriptionProperty(nameof(WKR_Summary))]
	[UserDefinedValues]
	public class WorkRequest : AutoWorkRequest,
		IDocManagerSupport,
		IWorkflowProvider,
		IWorkTaskRelatedItemSource,
		IWorkItemRelatedItem,
		IConversationProvider,
		IConversationAdditionalParticipantProvider,
		IConversationParentHyperlinkProvider,
		ICustomFieldProvider,
		IJobInvoicingPlugIn,
		INonTransportJobHeaderParent,
		IUniversalXMLNoteParent,
		IEDocsProvider
	{
		public WorkRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public static ResourceString SingularName => ResString.GetMultilingualString("cc3e08a9-8978-4c87-86cc-8f5ec4ef8d0d", "Customer Service Ticket");

		protected override ZString HumanReadableShortcutNameCore => GetHumanReadableName(WKR_RequestNumber, WKR_Summary);

		protected override ZString HumanReadableNameCore => GetHumanReadableName(SingularName, HumanReadableShortcutName);

		static string GetHumanReadableName(params string[] fields)
		{
			return string.Join(" - ", fields.Where(a => !string.IsNullOrWhiteSpace(a)));
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (shouldReCalculateStatus)
			{
				ReCalculateStatus();
			}

			if (HasChanges)
			{
				this.ApplyWorkflowTemplates();
			}
		}

		public override void OnSaving()
		{
			SetJobNumberIfRequired();
			ReCalculateStatus();

			base.OnSaving();

			RefreshBindingIncludingChildren();
		}

		public override void Delete()
		{
			base.Delete();

			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion

		#region Organisation

		[List("Lookups.Organisations")]
		[ResourceStringData("WorkRequest.OrganisationPK", Caption = "Organization")]
		public ZGuid OrganisationPK
		{
			get
			{
				if (!HasChanges && organisationPK.IsEmpty)
				{
					organisationPK = GetOrganisationPK();
				}
				return organisationPK;
			}
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOrganisationPK();
					if (!WKR_OC_Client.IsEmpty)
					{
						Validation.ValidateWKR_OC_Client();
					}
				}
				OrganisationPKInfo.RefreshBinding();
			}
		}

		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo => GetZPropertyInfo(nameof(OrganisationPK));

		public OrgHeader ClientOrganisation
		{
			get { return Factory.Load<OrgHeader>(OrganisationPK); }
		}

		#endregion

		#region Logging

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var relatedItemsToSeeEvents = new List<BusinessObject>();

				foreach (var item in RelatedItems)
				{
					if (SupportedTypesForRelatedEvents.Any(type => type.IsAssignableFrom(item.GetType())))
					{
						relatedItemsToSeeEvents.Add(item);
					}
				}

				return base.BusinessObjectsWithRelatedEventsCore.Concat(relatedItemsToSeeEvents).ToArray();
			}
		}

		readonly Type[] SupportedTypesForRelatedEvents =
		{
			typeof(WorkItem),
		};

		#endregion

		#region Related Business Objects

		public IEnumerable<WorkItem> RelatedWorkItems => RelatedItems.OfType<WorkItem>();

		#endregion

		#region Properties

		public override ZGuid WKR_OC_Client
		{
			get => base.WKR_OC_Client;
			set
			{
				base.WKR_OC_Client = value;
				var currentOrganisationPK = GetOrganisationPK();
				if (currentOrganisationPK.IsValid)
				{
					OrganisationPK = currentOrganisationPK;
				}
			}
		}

		ZGuid GetOrganisationPK()
		{
			return Client?.ParentOrg?.PK ?? ZGuid.Empty;
		}

		#region WKR_RequestNumber

		[ReadOnly(true)]
		public override ZString WKR_RequestNumber
		{
			get => base.WKR_RequestNumber;
			set => base.WKR_RequestNumber = value;
		}

		void SetJobNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(WKR_RequestNumberInfo, Env.NumberFountains.CustomerServiceTicketNumber);
			}
		}

		#endregion

		#region WKR_Status

		[ReadOnly(true)]
		[List("Lookups.StatusList")]
		public override ZString WKR_Status
		{
			get => base.WKR_Status;
			set => base.WKR_Status = value;
		}

		internal void NotifyStatusNeedsReCalculationOnFactorySaving()
		{
			shouldReCalculateStatus = true;
		}

		bool shouldReCalculateStatus;

		void ReCalculateStatus()
		{
			if (WKR_Status != TicketStatusList.Codes.Cancelled)
			{
				WKR_Status = GetNewStatus();
			}
		}

		string GetNewStatus()
		{
			var tasks = WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();
			var ticketTasksAreAllComplete = tasks.All(t => !t.IsOpen);
			var isAnyTaskShowingHumanActivity = tasks.Any(t => t.P9_Status.ToString().In(ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Working, ProcessTaskStatusCodeList.Codes.Suspended));

			if (HasIncompleteWorkItems || (isAnyTaskShowingHumanActivity && !ticketTasksAreAllComplete))
			{
				return TicketStatusList.Codes.Working;
			}
			else if (ticketTasksAreAllComplete)
			{
				return TicketStatusList.Codes.Closed;
			}
			else
			{
				return TicketStatusList.Codes.Open;
			}
		}

		#endregion

		#region eConversation

		public JobConversation Conversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}

				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;

		bool HasInitialisedConversation => conversation != null;

		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		void EnsureContactIsConversationParticipant()
		{
			if (WKR_OC_ClientInfo.HasChanges)
			{
				EnsureContactIsConversationParticipantCore(Conversation, (ZGuid)WKR_OC_ClientInfo.OriginalValue, shouldBeParticipant: false);
			}

			EnsureContactIsConversationParticipantCore(Conversation, WKR_OC_Client, shouldBeParticipant: true);
		}

		static void EnsureContactIsConversationParticipantCore(JobConversation conversation, ZGuid clientPK, bool shouldBeParticipant)
		{
			var client = conversation.Factory.Load<OrgContact>(clientPK);

			if (client != null)
			{
				var participant = conversation.RelatedParties.GetWithoutAdd(client);
				var isParticipant = participant != null;

				if (shouldBeParticipant != isParticipant)
				{
					if (shouldBeParticipant)
					{
						conversation.RelatedParties.AddNewParticipant(client);
					}
					else
					{
						participant.Delete();
					}
				}
			}
		}

		#endregion

		#region Selection Criteria

		[List("Lookups.SelectionCriterion1Values")]
		public override ZString WKR_SelectionCriteria1
		{
			get => base.WKR_SelectionCriteria1;
			set => base.WKR_SelectionCriteria1 = value;
		}

		[List("Lookups.SelectionCriterion2Values")]
		public override ZString WKR_SelectionCriteria2
		{
			get => base.WKR_SelectionCriteria2;
			set => base.WKR_SelectionCriteria2 = value;
		}

		[List("Lookups.SelectionCriterion3Values")]
		public override ZString WKR_SelectionCriteria3
		{
			get => base.WKR_SelectionCriteria3;
			set => base.WKR_SelectionCriteria3 = value;
		}

		[List("Lookups.SelectionCriterion4Values")]
		public override ZString WKR_SelectionCriteria4
		{
			get => base.WKR_SelectionCriteria4;
			set => base.WKR_SelectionCriteria4 = value;
		}

		[List("Lookups.SelectionCriterion5Values")]
		public override ZString WKR_SelectionCriteria5
		{
			get => base.WKR_SelectionCriteria5;
			set => base.WKR_SelectionCriteria5 = value;
		}

		#endregion

		#region For Workflow Macros

		public ZBool HasIncompleteWorkItems => RelatedWorkItems.Any(w => !w.IsClosedOrCancelled);

		public ZBool WereAllWorkItemsPreviouslyCompleted => this.GetLogs().MostRecentLogByEventTime(AutoEvents.JobClose, log => PropagationHandler.IsPropagatedEventLog(log)) != null;

		public ZBool HasAttachedWorkItemsAndAllAreCompleted => HasAttachedWorkItems && RelatedWorkItems.All(w => w.IsClosed);

		public ZBool HasAttachedWorkItemsAndAllAreCancelled => HasAttachedWorkItems && RelatedWorkItems.All(w => w.IsCancelled);

		public ZBool HasAttachedWorkItems => RelatedWorkItems.Any();

		#endregion

		#endregion

		#region Resource String Captions

		public static ResourceStringData SelectionCriterion1CaptionResourceString => new ResourceStringData("DEA3C63B-363C-4E7A-A25B-8F98EDE402E1",
			shortCaption: ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value,
			mediumCaption: ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value,
			caption: ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value,
			fullDescription: ProcessManagementRegistry.Instance.SelectionCriterion1Description.Value);

		public static ResourceStringData SelectionCriterion2CaptionResourceString => new ResourceStringData("220A69DD-2E15-480D-9073-0856AECF9184",
			shortCaption: ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value,
			mediumCaption: ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value,
			caption: ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value,
			fullDescription: ProcessManagementRegistry.Instance.SelectionCriterion2Description.Value);

		public static ResourceStringData SelectionCriterion3CaptionResourceString => new ResourceStringData("0F7507C5-137F-4C21-9CC9-9790B84A06A5",
			shortCaption: ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value,
			mediumCaption: ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value,
			caption: ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value,
			fullDescription: ProcessManagementRegistry.Instance.SelectionCriterion3Description.Value);

		public static ResourceStringData SelectionCriterion4CaptionResourceString => new ResourceStringData("4EB6A522-7BFC-4865-981D-1492579336A1",
			shortCaption: ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value,
			mediumCaption: ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value,
			caption: ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value,
			fullDescription: ProcessManagementRegistry.Instance.SelectionCriterion4Description.Value);

		public static ResourceStringData SelectionCriterion5CaptionResourceString => new ResourceStringData("2A66E9AD-8668-41B4-AB82-C68D8EDA7DB1",
			shortCaption: ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value,
			mediumCaption: ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value,
			caption: ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value,
			fullDescription: ProcessManagementRegistry.Instance.SelectionCriterion5Description.Value);

		public ZString SelectionCriterion1Caption => SelectionCriterion1CaptionResourceString.Caption;
		public ZString SelectionCriterion2Caption => SelectionCriterion2CaptionResourceString.Caption;
		public ZString SelectionCriterion3Caption => SelectionCriterion3CaptionResourceString.Caption;
		public ZString SelectionCriterion4Caption => SelectionCriterion4CaptionResourceString.Caption;
		public ZString SelectionCriterion5Caption => SelectionCriterion5CaptionResourceString.Caption;

		#endregion

		#region Cancel

		public void Cancel(bool shouldCancelAttachedWorkItems)
		{
			var scheduleDeactivator = new ScheduleDeactivator();

			if (scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(this))
			{
				this.CancelNonStartedTasksAndClosePartiallyCompletedTasks();

				if (shouldCancelAttachedWorkItems)
				{
					RelatedWorkItems.ForEach(w => w.Cancel());
				}

				WKR_Status = TicketStatusList.Codes.Cancelled;
			}
		}

		public void UnCancel()
		{
			WKR_Status = ZString.Empty;
			ReCalculateStatus();
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomerServiceTicket)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter => new WorkRequestDocumentSupporter(this);

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IWorkflowProvider Members

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WorkRequestProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		ZGuid IWorkflowProviderCore.PK => PK;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var ranker = new ColumnValueRanker();

			ranker.Add(ProcessTaskTemplateSchema.P0_SubType1, WKR_SelectionCriteria1, ZString.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_SubType2, WKR_SelectionCriteria2, ZString.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_SubType3, WKR_SelectionCriteria3, ZString.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_SubType4, WKR_SelectionCriteria4, ZString.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_SubType5, WKR_SelectionCriteria5, ZString.Empty);

			ranker.Add(ProcessTaskTemplateSchema.P0_GB, WKR_GB_Branch, ZGuid.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_GE, WKR_GE_Department, ZGuid.Empty);

			ranker.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, WKR_RN_NKCountry, ZString.Empty);

			return ranker;
		}

		#endregion

		#region IWorkTaskRelatedItem Members

		public ZString ClientName => Client?.ParentOrg.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ZString ClientCode => Client?.OrganisationCode ?? ZString.Empty;
		public ZPropertyInfo ClientCodeInfo => GetZPropertyInfo(nameof(ClientCode));

		public ZString Type => WorkTaskRelatedItemTypes.CustomerServiceTicket;
		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		public ZString Number => WKR_RequestNumber;
		public ZPropertyInfo NumberInfo => WKR_RequestNumberInfo;

		[ResourceStringData("WorkRequest.StatusDescription", Caption = "Status")]
		public ZString StatusDescription => Lookups.StatusList.GetDescriptionFromCode(WKR_Status);
		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(nameof(StatusDescription));

		public ZString AssignedStaffCode => string.Empty;
		public ZPropertyInfo AssignedStaffCodeInfo => GetZPropertyInfo(nameof(AssignedStaffCode));

		public ZString ItemDescription => WKR_Summary;
		public ZPropertyInfo ItemDescriptionInfo => WKR_SummaryInfo;

		public ZString Criticality => string.Empty;
		public ZPropertyInfo CriticalityInfo => GetZPropertyInfo(nameof(Criticality));

		public ZString Source => string.Empty;
		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));

		public ZBool IsClosedOrCancelled => WKR_Status.ToString().In(TicketStatusList.Codes.Closed, TicketStatusList.Codes.Cancelled);

		public ControllerID ControllerID => ControllerIDs.CustomerServiceTicket;

		public ZString SelectionCriterion1 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.SelectionCriterion1Values, WKR_SelectionCriteria1);
		public ZPropertyInfo SelectionCriterion1Info => GetWrappedZPropertyInfo(nameof(SelectionCriterion1), o => WKR_SelectionCriteria1Info);

		public ZString SelectionCriterion2 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.SelectionCriterion2Values, WKR_SelectionCriteria2);
		public ZPropertyInfo SelectionCriterion2Info => GetWrappedZPropertyInfo(nameof(SelectionCriterion2), o => WKR_SelectionCriteria2Info);

		public ZString SelectionCriterion3 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.SelectionCriterion3Values, WKR_SelectionCriteria3);
		public ZPropertyInfo SelectionCriterion3Info => GetWrappedZPropertyInfo(nameof(SelectionCriterion3), o => WKR_SelectionCriteria3Info);

		public ZString SelectionCriterion4 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.SelectionCriterion4Values, WKR_SelectionCriteria4);
		public ZPropertyInfo SelectionCriterion4Info => GetWrappedZPropertyInfo(nameof(SelectionCriterion4), o => WKR_SelectionCriteria4Info);

		public ZString SelectionCriterion5 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.SelectionCriterion5Values, WKR_SelectionCriteria5);
		public ZPropertyInfo SelectionCriterion5Info => GetWrappedZPropertyInfo(nameof(SelectionCriterion5), o => WKR_SelectionCriteria5Info);

		public Type PivotCollectionType => typeof(WorkItemRequestLinkCollection);

		#endregion

		#region IWorkTaskRelatedItemSource Members

		[ActionFieldFollow(false)]
		public WorkTaskRelatedItemCollection RelatedItems
		{
			get
			{
				if (relatedItems == null)
				{
					relatedItems = new WorkRequestRelatedItemCollection(this);
					relatedItems.Load();
					relatedItems.RelatedItemAdded += RelatedItems_RelatedItemAdded;
					relatedItems.RelatedItemRemoved += RelatedItems_RelatedItemRemoved;
				}

				return relatedItems;
			}
		}

		void RelatedItems_RelatedItemAdded(object sender, RelatedItemEventArgs e)
		{
			OnWorkItemAdded((WorkItem)e.BusinessObject);
		}

		WorkTaskRelatedItemCollection relatedItems;

		void RelatedItems_RelatedItemRemoved(object sender, RelatedItemEventArgs e)
		{
			OnWorkItemRemoved((WorkItem)e.BusinessObject);
		}

		public virtual IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules
		{
			get
			{
				yield return WorkTaskRelatedItemModuleInfo.WorkItem(Factory, allowNew: true);
			}
		}

		public void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public ZBool ShowOnlyNonClosedItems
		{
			get => !FilteredRelatedItems.IncludeAllItems;
			set => FilteredRelatedItems.IncludeAllItems = !value;
		}

		[ActionFieldFollow(false)]
		public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems => filteredRelatedItems ?? (filteredRelatedItems = new FilteredWorkTaskRelatedItemCollection(RelatedItems));
		FilteredWorkTaskRelatedItemCollection filteredRelatedItems;

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IWorkItemRelatedItem Members

		public bool OnRelatedWorkItemClosed(WorkItem workItem)
		{
			return true;
		}

		public void OnRelatedWorkItemReOpened(WorkItem workItem)
		{
		}

		public void OnWorkItemAdded(WorkItem workItem)
		{
			ReCalculateStatus();
		}

		public void OnWorkItemRemoved(WorkItem workItem)
		{
			ReCalculateStatus();
		}

		#endregion

		#region IConversationProvider Members

		JobConversation IConversationProvider.eConversation => Conversation;
		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.CustomerServiceTicket;
		ControllerID IConversationProvider.ParentController => ControllerIDs.CustomerServiceTicket;
		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();
		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
			if (HasInitialisedConversation)
			{
				EnsureContactIsConversationParticipant();
			}
		}

		string IConversationProvider.EmailSubjectContentOverride => default;
		string IConversationProvider.FromAddressOverride => default;
		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#endregion

		#region IConversationAdditionalParticipantProvider Members

		IEnumerable<IConversationParticipant> IConversationAdditionalParticipantProvider.GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			var anySubscriberAreNotClient = subscribedParticipants.Any(p => !(p is IOrgContact));
			var messagesWereSentFromStaff = sender?.Parent is IGlbStaff;

			return anySubscriberAreNotClient || messagesWereSentFromStaff
				? Enumerable.Empty<IConversationParticipant>()
				: NotificationRecipientCalculator.GetStaffForEConversationNotifications(this);
		}

		#endregion

		#region IConversationParentHyperlinkProvider Members

		bool IConversationParentHyperlinkProvider.ShouldUseThisProviderForHyperlink(IConversationParticipant participant)
		{
			return participant == null || participant is OrgContact;
		}

		string IConversationParentHyperlinkProvider.GetHyperlinkToConversationParent()
		{
			var glowUri = GlowRegistry.Instance.GlowPortalsUri.Value;

			if (!glowUri.EndsWith("/", StringComparison.InvariantCulture))
			{
				glowUri += "/";
			}

			return FormattableString.Invariant($"{glowUri}TKT/Desktop#/formFlow/c02c315d-c12a-4abb-aec7-e8d3b57a2dba/{PK}"); // This is a URL
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => WKR_RequestNumber;

		#endregion

		#region IJobInvoicingPlugin Memebers

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				return invoicingSupporter ?? (invoicingSupporter = GetInvoicingSupporterCore());
			}
		}
		IJobInvoicingSupporter invoicingSupporter;

		protected virtual IJobInvoicingSupporter GetInvoicingSupporterCore()
		{
			return new WorkRequestInvoicingSupporter(this);
		}

		public bool AllowInvoiceDeletion => false;

		public void SetJobNumberFieldOnSaving()
		{
			return;
		}

		public void OnJobCreating(JobHeader job)
		{
			return;
		}

		public void OnJobCreated(JobHeader job)
		{
			return;
		}

		public void OnJobDeleting(JobHeader job)
		{
			return;
		}

		public void OnJobDeleted(JobHeader job)
		{
			return;
		}

		#endregion

		#region For Test
#if DEBUG

		public override string ToString()
		{
			return FormattableString.Invariant($"{WKR_RequestNumber}: {WKR_Summary}");
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			WKR_Summary = "PAVE Team 100 years!";
			WKR_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
		}

#endif
		#endregion
	}
}
