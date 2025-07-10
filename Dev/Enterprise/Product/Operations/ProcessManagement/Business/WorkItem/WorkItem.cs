using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Shared;
using CargoWise.Types;
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
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	[CodeProperty(WorkItemSchema.Constants.WKI_WorkItemNumber), DescriptionProperty(WorkItemSchema.Constants.WKI_Summary)]
	[UniversalDataContext(DataContextType.WorkItem)]
	[UniversalCopyWithExtendedEntities]
	[UserDefinedValues]
	[System.Diagnostics.DebuggerDisplay("PK = {PK}, Number = {WKI_WorkItemNumber}")]
	public class WorkItem : WorkItemCommon,
		IWorkflowProvider,
		IProcessHandlingInfoProvider,
		IWorkflowTriggerEventSource,
		IEDocsProvider,
		ICustomFieldProvider,
		IRelatableActivity,
		IWorkTaskRelatedItem,
		IWorkTaskRelatedItemSource,
		IAuditParent,
		IUniversalXMLNoteParent,
		IConversationProvider,
		IWorkQueueMembersProvider
	{
		public WorkItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WorkItemFetchStrategy(this);
		}

		#region Name

		public static ResourceString SingularName
		{
			get { return ResString.GetMultilingualString("F02E0262-90C3-486E-A7A6-87CC8F0CE000", "Work Item"); }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = "";
				if (!IsDeleted && !WKI_WorkItemNumber.IsEmpty)
				{
					result += WKI_WorkItemNumber;
					if (!WKI_Summary.IsEmpty)
					{
						result += " - " + WKI_Summary;
					}
				}
				else
				{
					result += SingularName;
				}
				return result;
			}
		}

		#endregion

		#region Work Item Type (Product)

		[List("Lookups.ActiveTypes")]
		public override ZString WKI_WorkItemType
		{
			get { return base.WKI_WorkItemType; }
			set
			{
				if (base.WKI_WorkItemType != value)
				{
					base.WKI_WorkItemType = value;
					UpdateOtherCategories(CategoryDepth.WorkItemType);
				}
			}
		}

		enum CategoryDepth
		{
			WorkItemType = 0,
			WorkItemArea = 1,
			ActivityType = 2,
			ActivitySubtype = 3,
			Priority = 4
		}

		void UpdateOtherCategories(CategoryDepth depth)
		{
			if (inUpdateOtherCategories)
			{
				return;
			}

			inUpdateOtherCategories = true;

			try
			{
				using (GetValidationSuspender())
				{
					if (depth < CategoryDepth.WorkItemArea &&
					!WKI_WorkItemArea.IsEmpty &&
					!Lookups.ActiveAreas.ContainsCode(WKI_WorkItemArea))
					{
						WKI_WorkItemArea = "";
					}

					if (depth < CategoryDepth.ActivityType &&
						!WKI_ActivityType.IsEmpty &&
						!Lookups.ActiveActivityTypes.ContainsCode(WKI_ActivityType))
					{
						WKI_ActivityType = "";
					}

					if (depth < CategoryDepth.ActivitySubtype &&
						!WKI_ActivitySubtype.IsEmpty &&
						!Lookups.ActiveActivitySubtypes.ContainsCode(WKI_ActivitySubtype))
					{
						WKI_ActivitySubtype = "";
					}

					if (depth < CategoryDepth.Priority &&
						!WKI_Priority.IsEmpty &&
						!Lookups.ActivePriorities.ContainsCode(WKI_Priority))
					{
						WKI_Priority = "";
					}
				}
			}
			finally
			{
				inUpdateOtherCategories = false;
			}
		}

		bool inUpdateOtherCategories;

		public ZString WorkItemTypeDescription
		{
			get { return Lookups.AllTypes.GetDescriptionFromCode(WKI_WorkItemType); }
		}

		public ZPropertyInfo WorkItemTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.WorkItemTypeDescription); }
		}

		#endregion

		#region Work Item Area (Module)

		[List("Lookups.ActiveAreas")]
		public override ZString WKI_WorkItemArea
		{
			get { return base.WKI_WorkItemArea; }
			set
			{
				if (base.WKI_WorkItemArea != value)
				{
					base.WKI_WorkItemArea = value;
					UpdateOtherCategories(CategoryDepth.WorkItemArea);
				}
			}
		}

		public ZString AreaDescription
		{
			get { return Lookups.AllAreas.GetDescriptionFromCode(WKI_WorkItemArea); }
		}

		public ZPropertyInfo AreaDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.AreaDescription); }
		}

		#endregion

		#region Activity Type (Change Type)

		[List("Lookups.ActiveActivityTypes")]
		public override ZString WKI_ActivityType
		{
			get { return base.WKI_ActivityType; }
			set
			{
				if (base.WKI_ActivityType != value)
				{
					base.WKI_ActivityType = value;
					UpdateOtherCategories(CategoryDepth.ActivityType);
				}
			}
		}

		public ZString ActivityTypeDescription
		{
			get { return Lookups.AllActivityTypes.GetDescriptionFromCode(WKI_ActivityType); }
		}

		public ZPropertyInfo ActivityTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ActivityTypeDescription); }
		}

		#endregion

		#region Activity Subtype (Change Area)

		[List("Lookups.ActiveActivitySubtypes")]
		public override ZString WKI_ActivitySubtype
		{
			get { return base.WKI_ActivitySubtype; }
			set
			{
				if (base.WKI_ActivitySubtype != value)
				{
					base.WKI_ActivitySubtype = value;
					UpdateOtherCategories(CategoryDepth.ActivitySubtype);

					if (!WKI_ActivitySubtype.IsEmpty && !IsDefect)
					{
						WKI_P9_DefectCausedByTask = ZGuid.Empty;
						DefectCausedByWorkItemPK = ZGuid.Empty;
						WKI_P9_DefectFirstMissedInTask = ZGuid.Empty;
					}
				}
			}
		}

		public ZString ActivitySubtypeDescription
		{
			get { return Lookups.AllActivitySubtypes.GetDescriptionFromCode(WKI_ActivitySubtype); }
		}

		public ZPropertyInfo ActivitySubtypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ActivitySubtypeDescription); }
		}

		#endregion

		#region Priority (Checkin To)

		[List("Lookups.ActivePriorities")]
		public override ZString WKI_Priority
		{
			get { return base.WKI_Priority; }
			set { base.WKI_Priority = value; }
		}

		public ZString PriorityDescription
		{
			get { return Lookups.AllPriorities.GetDescriptionFromCode(WKI_Priority); }
		}

		public ZPropertyInfo PriorityDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PriorityDescription); }
		}

		#endregion

		#region Status

		[ReadOnly(true)]
		[List("Lookups.StatusList")]
		public override ZString WKI_Status
		{
			get { return base.WKI_Status; }
			set { base.WKI_Status = value; }
		}

		#region Business Logic

		void CalculateAndSetStatusWithoutSettingHasChanges()
		{
			// Only suspend setting HasChanges if neither this nor any child bizo have changes already.
			// This is important for causing this bizo to actually be saved when propagating task status updates to the WorkItem, but not causing the save button to activate on the form when opening.

			using (!HasChanges ? SuspendSettingHasChanges() : null)
			{
				CalculateAndSetStatus();
			}
		}

		void CalculateAndSetStatus()
		{
			if (WorkflowItems.Tasks.Count > 0 && WorkflowItems.AllTasksCancelled)
			{
				Cancel();
			}
			else if (WorkflowItems.Tasks.Count == 0 || WorkflowItems.AllTasksClosedOrCancelled)
			{
				Close();
			}
			else if (HasStartedTasks)
			{
				MarkAsWorking();
			}
			else if (AnyTaskIsAssignedToAnyResource)
			{
				WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			}
			else
			{
				WKI_Status = ProcessTaskStatusCodeList.Codes.Open;
			}

			NotifyRelatedWorkRequestsStatusUpdateRequired();
		}

		void NotifyRelatedWorkRequestsStatusUpdateRequired()
		{
			foreach (var workRequest in GetRelatedWorkRequests())
			{
				workRequest.NotifyStatusNeedsReCalculationOnFactorySaving();
			}
		}

		void Close()
		{
			if (WKI_Status != ProcessTaskStatusCodeList.Codes.Closed && WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
			{
				WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;

				if (WorkflowItems.Tasks.Count > 0) // Ensures we don't claim we're finished work that we didn't actually do.
				{
					foreach (IWorkItemRelatedItem item in RelatedItems)
					{
						item.OnRelatedWorkItemClosed(this);
					}
				}
			}
		}

		bool AnyTaskIsAssignedToAnyResource => WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => !t.P9_GS_NKAssignedStaffMember.IsEmpty);
		bool HasStartedTasks => WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => t.P9_Status.ToString().In(TaskStatusesIndicatingLikelyHumanActivity));

		static string[] TaskStatusesIndicatingLikelyHumanActivity => new[]
		{
			ProcessTaskStatusCodeList.Codes.Working,
			ProcessTaskStatusCodeList.Codes.Suspended,
			ProcessTaskStatusCodeList.Codes.Closed,
			ProcessTaskStatusCodeList.Codes.Cancelled,
		};

		void MarkAsWorking()
		{
			if (IsClosedOrCancelled)
			{
				foreach (IWorkItemRelatedItem item in RelatedItems)
				{
					item.OnRelatedWorkItemReOpened(this);
				}
			}

			WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
		}

		#endregion

		#region Synchronisation

		void HookUpTaskStatusChangeEvent(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsDeleted)
			{
				var items = WorkflowItems;
				if (!items.IsLoading)
				{
					UpdateStatusNowAndAgainAfterSave();
				}
			}
		}

		internal void OnTaskDeleted()
		{
			if (!IsDeleted)
			{
				UpdateStatusNowAndAgainAfterSave();
			}
		}

		internal void OnTaskStatusChanged()
		{
			if (!IsDeleted)
			{
				UpdateStatusNowAndAgainAfterSave();
			}
		}

		void UpdateStatusNowAndAgainAfterSave()
		{
			if (!IsAlreadySyncingStatus)
			{
				CalculateAndSetStatusWithoutSettingHasChanges();
				Factory.Saved -= UpdateStatusOnFactorySaved;
				Factory.Saved += UpdateStatusOnFactorySaved;
			}
		}

		void UpdateStatusOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= UpdateStatusOnFactorySaved;
				SyncStatusAndSave();
			}
		}

		void SyncStatusAndSave()
		{
			if (IsAlreadySyncingStatus || IsDeleted)
			{
				return;
			}

			using (settingStatusSuspender.Suspend())
			{
				WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
				Reload();
				var oldStatus = WKI_Status;

				CalculateAndSetStatus();

				if (oldStatus != WKI_Status)
				{
					try
					{
						Factory.Save();
					}
					catch (ZDataConcurrencyException)
					{
					}
				}
			}
		}

		bool IsAlreadySyncingStatus => settingStatusSuspender.IsSuspended;

		readonly ActionSuspender settingStatusSuspender = new ActionSuspender();

		#endregion

		#region For Test

		public void CalculateAndSetStatusWithoutSettingHasChanges_ForTest() => CalculateAndSetStatusWithoutSettingHasChanges();

		#endregion

		#endregion

		#region Job Workflow

		public IProcessJobHeader JobWorkflow
		{
			get
			{
				if (jobWorkflow == null)
				{
					jobWorkflow = ProcessJobHeaderProvider.GetForParent(this, Factory, addDefaultProcessHeaderIfNone: false);
				}
				return jobWorkflow;
			}
		}

		IProcessJobHeader jobWorkflow;

		#endregion

		#region Release Sequencing

		public ZString ReleaseSequenceName => JobWorkflow?.ReleaseSequenceName ?? ZString.Empty;
		public ZInt ReleaseSequencePosition => JobWorkflow?.ReleaseSequencePosition ?? ZInt.Zero;
		public ZInt ReleaseSequenceValue => JobWorkflow?.ReleaseSequenceValue ?? ZInt.Zero;

		public ZInt ReleaseSequenceInvestment => JobWorkflow?.ReleaseSequenceInvestment ?? ZInt.Zero;
		public ZDateTime ReleaseSequenceDate => HighestReleaseSequence != null ? JobWorkflow.FH_AgreedDeliveryDate : ZDateTime.Empty;

		public ZString ReleaseSequenceDateAsText => ReleaseSequenceDate.IsValid ? Env.Time.GetLocalTimeFromUtc(ReleaseSequenceDate.ToDateTime()).ToString(DateTimeFormatStrings.LongTimeFormat, CultureInfo.CurrentCulture) : string.Empty;

		public IBMReleaseSequence HighestReleaseSequence => JobWorkflow?.HighestReleaseSequence;

		public ZPropertyInfo ReleaseSequenceNameInfo => GetZPropertyInfo(Schema.ReleaseSequenceName);
		public ZPropertyInfo ReleaseSequencePositionInfo => GetZPropertyInfo(Schema.ReleaseSequencePosition);
		public ZPropertyInfo ReleaseSequenceValueInfo => GetZPropertyInfo(Schema.ReleaseSequenceValue);
		public ZPropertyInfo ReleaseSequenceInvestmentInfo => GetZPropertyInfo(Schema.ReleaseSequenceInvestment);
		public ZPropertyInfo ReleaseSequenceDateAsTextInfo => GetZPropertyInfo(Schema.ReleaseSequenceDateAsText);

		#endregion

		#region Assigned To

		[List("Lookups.Staff")]
		public ZString AssignedToCode
		{
			get
			{
				var result = CurrentTaskAssignedToCode;
				if (result.IsEmpty && OverallTaskStatusCode == ProcessTaskStatusCodeList.Codes.Closed)
				{
					var items = WorkflowItems.Tasks;
					if (items.Count > 0)
					{
						result = items.Cast<ProcessTask>().OrderByDescending(x => x.P9_Sequence).First().P9_GS_NKAssignedStaffMember;
					}
				}
				return result;
			}
		}

		public GlbStaff AssignedToStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AssignedToCode); }
		}

		#endregion

		#region Current Task

		[List("Lookups.Staff")]
		public ZString CurrentTaskAssignedToCode
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_GS_NKAssignedStaffMember : ZString.Empty;
			}
		}

		[List("Lookups.Staff")]
		public ZString CurrentOrNextTaskAssignedToCode
		{
			get
			{
				var task = CurrentOrNextTask;
				return task != null ? task.P9_GS_NKAssignedStaffMember : ZString.Empty;
			}
		}

		public ZString CurrentOrNextTaskAssignedToCodeAndName
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentOrNextTask;
				if (task != null)
				{
					var staff = task.AssignedStaffMember;
					if (staff != null)
					{
						result = staff.GS_Code + "  " + staff.GS_FullName;
					}
				}
				return result;
			}
		}

		public GlbStaff CurrentTaskAssignedTo
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.AssignedStaffMember : null;
			}
		}

		public ZGuid CurrentTaskAssignedToGroupPK
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_GG_AssignedGroup : ZGuid.Empty;
			}
		}

		public ZPropertyInfo CurrentTaskAssignedToGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTaskAssignedToGroupPK)); }
		}

		public GlbGroup CurrentTaskAssignedToGroup
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.AssignedGroup : null;
			}
		}

		[List("Lookups.StatusList")]
		public ZString CurrentTaskStatus
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_Status : ZString.Empty;
			}
		}

		[List("Lookups.StatusList")]
		public ZString CurrentOrNextTaskStatus
		{
			get
			{
				var task = CurrentOrNextTask;
				return task != null ? task.P9_Status : ZString.Empty;
			}
		}

		public ZString OverallTaskStatusCode
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentOrNextTask;
				if (task == null)
				{
					if (WorkflowItems.AllTasksCancelled)
					{
						result = ProcessTaskStatusCodeList.Codes.Cancelled;
					}
					else if (WorkflowItems.Tasks.Count > 0)
					{
						result = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}
				else
				{
					result = task.P9_Status;
				}
				return result;
			}
		}

		public ZString OverallTaskStatusDescription
		{
			get
			{
				ZString statusCode = OverallTaskStatusCode;
				return !statusCode.IsEmpty ? new ZString(Lookups.StatusList.GetDescriptionFromCode(statusCode)) : ZString.Empty;
			}
		}

		public ZString OverallTaskStatusCodeAndDescription
		{
			get
			{
				ZString statusCode = OverallTaskStatusCode;
				return !statusCode.IsEmpty ? new ZString(statusCode + "  " + Lookups.StatusList.GetDescriptionFromCode(statusCode)) : ZString.Empty;
			}
		}

		public ProcessTask CurrentOrNextTask
		{
			get
			{
				if (currentOrNextTask == null)
				{
					currentOrNextTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentOrNextStartableTask();
					}
					);
				}
				return currentOrNextTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentOrNextTask;

		public ProcessTask CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					currentTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentStartableTask();
					}
					);
				}
				return currentTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentTask;

		CurrentTaskFinder TaskFinder => taskFinder ?? (taskFinder = new CurrentTaskFinder(this));
		CurrentTaskFinder taskFinder;

		public virtual void Cancel()
		{
			var scheduleDeactivator = new ScheduleDeactivator();
			var shouldPrompt = scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(this);
			if (shouldPrompt)
			{
				var shouldCancel = WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled;

				this.CancelNonStartedTasksAndClosePartiallyCompletedTasks();

				if (shouldCancel && shouldPrompt)
				{
					WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				}
			}
		}

		#endregion

		#region Validation

		public WorkItemActualValidation ActualValidation => (WorkItemActualValidation)Validation;

		protected override WorkItemCommonValidation GetNewValidationCore()
		{
			return new WorkItemActualValidation(this);
		}

		#endregion

		#region Lookups

		public new WorkItemActualLookups Lookups => (WorkItemActualLookups)(base.Lookups);

		protected override WorkItemLookups GetNewLookupsCore()
		{
			return new WorkItemActualLookups(this);
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WKI_Status = ProcessTaskStatusCodeList.Codes.Open;
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			ExternalEntityLinkHelper.DeleteLinksForBusinessObject(this);

			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				CalculateAndSetStatus(); // Ensures that we set the status to CLS if there aren't any tasks.

				var isOpen = !IsClosedOrCancelled;
				var @event = isOpen ? AutoEvents.JobOpen : AutoEvents.JobClose;
				if (WorkflowItems.Tasks.Count > 0)
				{
					this.GetLogs().AddNew(@event);
				}
			}
			else if (WKI_StatusInfo.HasChanges)
			{
				var isOpen = !IsClosedOrCancelled;
				var wasOpen = WKI_StatusInfo.OriginalValue.ToString().In(ProcessTask.GetOpenTaskStatuses());

				if (isOpen != wasOpen && WorkflowItems.Tasks.Count > 0)
				{
					var @event = isOpen ? AutoEvents.JobOpen : AutoEvents.JobClose;
					this.GetLogs().AddNew(@event);
				}
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete => Job is null;

		public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"Work Items with Job Headers may not be deleted.";

		#endregion

		#region IDocumentSupportable

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new WorkItemDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport

		public virtual DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new WorkItemDocManagerInfo(this)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this);
			var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
			properties.Add(loader.FindMatches(this));
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IWorkflowProvider

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public WorkItemProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(CreateWorkItemProcessTaskCollection);
					workflowItems.CountChanged += HookUpTaskStatusChangeEvent;

					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		WorkItemProcessTaskCollection workflowItems;

		protected virtual WorkItemProcessTaskCollection CreateWorkItemProcessTaskCollection()
		{
			return new WorkItemProcessTaskCollection(this);
		}

		public virtual IColumnValueRanker GetTemplateSelectionCriteria()
		{
			// Must match WorkItemWorkflowDescriptor.SubTypeInformation
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, WKI_WorkItemType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, WKI_WorkItemArea, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, WKI_ActivityType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType4, WKI_ActivitySubtype, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType5, WKI_Priority, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, WKI_GE_AssignedDepartment, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, WKI_PortOrCountry, ZString.Empty);

			return result;
		}

		public ZString WorkflowType
		{
			get { return JobInvoicingConsumerTypes.WorkItem.Code; }
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.WorkItem; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { WKI_Summary, WKI_Status }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region IWorkTaskRelatedItemSource

		[ActionFieldFollow(false)]
		public WorkTaskRelatedItemCollection RelatedItems
		{
			get
			{
				if (relatedItems == null)
				{
					relatedItems = new WorkItemRelatedItemCollection(this);
					relatedItems.Load();
					relatedItems.RelatedItemAdded += RelatedItems_RelatedItemAdded;
					relatedItems.RelatedItemRemoved += RelatedItems_RelatedItemRemoved;
				}
				return relatedItems;
			}
		}

		void RelatedItems_RelatedItemAdded(object sender, RelatedItemEventArgs e)
		{
			((IWorkItemRelatedItem)e.BusinessObject).OnWorkItemAdded(this);
		}

		void RelatedItems_RelatedItemRemoved(object sender, RelatedItemEventArgs e)
		{
			((IWorkItemRelatedItem)e.BusinessObject).OnWorkItemRemoved(this);
		}

		WorkItemRelatedItemCollection relatedItems;

		public IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules => GetSupportedRelatedItemModules();

		protected virtual IEnumerable<WorkTaskRelatedItemModuleInfo> GetSupportedRelatedItemModules()
		{
			return new[] { WorkTaskRelatedItemModuleInfo.Project(Factory, false), WorkTaskRelatedItemModuleInfo.CustomerServiceTicket(Factory) };
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
		public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems
		{
			get
			{
				if (filteredRelatedItems == null)
				{
					filteredRelatedItems = new FilteredWorkTaskRelatedItemCollection(RelatedItems);
				}
				return filteredRelatedItems;
			}
		}
		FilteredWorkTaskRelatedItemCollection filteredRelatedItems;

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IWorkTaskRelatedItem Members

		public ZString ClientName => ZString.Empty;
		public ZPropertyInfo ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ZString ClientCode => ZString.Empty;
		public ZPropertyInfo ClientCodeInfo => GetZPropertyInfo(nameof(ClientCode));

		public ZString Type => WorkTaskRelatedItemTypes.WorkItem;
		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		public ZString Number => WKI_WorkItemNumber;
		public ZPropertyInfo NumberInfo => WKI_WorkItemNumberInfo;

		public ZString StatusDescription
		{
			get
			{
				var status = WKI_Status;
				if (status.IsEmpty)
				{
					status = OverallTaskStatusCode;
				}
				return Lookups.StatusList.GetDescriptionFromCode(status);
			}
		}

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(nameof(StatusDescription));

		public ZString AssignedStaffCode => AssignedToCode;
		public ZPropertyInfo AssignedStaffCodeInfo => GetZPropertyInfo(nameof(AssignedStaffCode));

		public ZString ItemDescription => WKI_Summary;
		public ZPropertyInfo ItemDescriptionInfo => WKI_SummaryInfo;

		public ZString Criticality => WKI_Priority;
		public ZPropertyInfo CriticalityInfo => WKI_PriorityInfo;

		public ControllerID ControllerID => ControllerIDs.WorkItem;

		public ZString SelectionCriterion1 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllTypes, WKI_WorkItemType);
		public ZPropertyInfo SelectionCriterion1Info => GetZPropertyInfo(nameof(SelectionCriterion1));

		public ZString SelectionCriterion2 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllAreas, WKI_WorkItemArea);
		public ZPropertyInfo SelectionCriterion2Info => GetZPropertyInfo(nameof(SelectionCriterion2));

		public ZString SelectionCriterion3 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllActivityTypes, WKI_ActivityType);
		public ZPropertyInfo SelectionCriterion3Info => GetZPropertyInfo(nameof(SelectionCriterion3));

		public ZString SelectionCriterion4 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllActivitySubtypes, WKI_ActivitySubtype);
		public ZPropertyInfo SelectionCriterion4Info => GetZPropertyInfo(nameof(SelectionCriterion4));

		public ZString SelectionCriterion5 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllPriorities, WKI_Priority);
		public ZPropertyInfo SelectionCriterion5Info => GetZPropertyInfo(nameof(SelectionCriterion5));

		public ZString Source => ZString.Empty;
		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));

		public ZBool IsClosedOrCancelled => WKI_Status == ProcessTaskStatusCodeList.Codes.Closed || WKI_Status == ProcessTaskStatusCodeList.Codes.Cancelled;

		public ZBool IsClosed => WKI_Status == ProcessTaskStatusCodeList.Codes.Closed;

		public ZBool IsCancelled => WKI_Status == ProcessTaskStatusCodeList.Codes.Cancelled;

		Type IWorkTaskRelatedItem.PivotCollectionType => typeof(GenPivotCollection);

		#endregion

		#region IHaveServices Member

		protected override JobServiceDependentCollection GetJobServicesDependentCollection(WorkItemCommon workItemCommon, BusinessObjectFactory factory)
		{
			return new JobServiceDependentCollection(this, Factory);
		}

		protected override ZGuid WorkItemServiceBranchPK => WKI_GB_AssignedBranch;

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(ProcessTasksSchema.P9_ParentID, ProcessTasksSchema.P9_TaskID);
			}
		}

		#endregion

		#region DefectCausedByWorkItem

		[ActionFieldFollow(false)]
		public WorkItem DefectCausedByWorkItem
		{
			get { return Factory.Load<WorkItem>(DefectCausedByWorkItemPK); }
		}

		[List("Lookups.OtherWorkItems")]
		public ZGuid DefectCausedByWorkItemPK
		{
			get
			{
				if (!isDefectCausedByWorkItemPKLoaded)
				{
					defectCausedByWorkItemPK = LoadDefectCausedByWorkItemPK();
					isDefectCausedByWorkItemPKLoaded = true;
				}
				return defectCausedByWorkItemPK;
			}
			set
			{
				if (DefectCausedByWorkItemPK != value)
				{
					defectCausedByWorkItemPK = value;
					WKI_P9_DefectCausedByTask = ZGuid.Empty;
					WKI_P9_DefectFirstMissedInTask = ZGuid.Empty;
					DefectCausedByWorkItemPKInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ActualValidation.ValidateDefectCausedByWorkItemPK();
				}
				ValidateTasks();
			}
		}

		protected virtual void ValidateTasks()
		{
			// implementation provided by subclass
		}

		ZGuid defectCausedByWorkItemPK;
		ZBool isDefectCausedByWorkItemPKLoaded = false;

		public ZPropertyInfo DefectCausedByWorkItemPKInfo
		{
			get { return GetZPropertyInfo(nameof(DefectCausedByWorkItemPK)); }
		}

		public bool DefectCausedByWorkItemPK_ReadOnly => !IsDefect;

		public bool IsDefect => ProcessManagementRegistry.Instance.DefectWorkItemTypes.Value.Contains((string)WKI_ActivitySubtype);

		ZGuid LoadDefectCausedByWorkItemPK()
		{
			var task = DefectCausedByTask;
			return task != null ? task.P9_ParentID : ZGuid.Empty;
		}

		#endregion

		#region DefectCausedByTask

		public bool WKI_P9_DefectCausedByTask_ReadOnly
		{
			get { return DefectCausedByWorkItemPK.IsEmpty || (!DefectCausedByWorkItemPK.IsEmpty && DefectCausedByWorkItemPKInfo.HasErrors()); }
		}

		public bool IsTargetTaskLinkedToTargetDefectCausedWI()
		{
			return DefectCausedByTask.P9_ParentID == DefectCausedByWorkItemPK;
		}

		#endregion

		#region HasIdentifyDefectCauseTaskCancelled

		public bool HasIdentifyDefectCauseTaskCancelled
		{
			get
			{
				return WorkflowItems.Tasks.Cast<ProcessTask>()
					.Any(task => task.P9_Type == ProcessManagementRegistry.Instance.IdentifyDefectCauseTaskType.Value &&
						task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled);
			}
		}

		#endregion

		#region DefectFirstMissedInTask

		public bool WKI_P9_DefectFirstMissedInTask_ReadOnly => DefectCausedByWorkItemPK.IsEmpty;

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => new WorkItemProcessHandlingInfo(this);

		#endregion

		#region IWorkflowTriggerEventSource Members

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders => GetRelatedWorkRequests();

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => Job?.Company ?? GlbCompany.GetCurrentCompany(Factory);

		protected virtual WorkRequest[] GetRelatedWorkRequests()
		{
			var pivots = Factory.Load<WorkItemRequestLink>(new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

			if (pivots.Length > 0)
			{
				return Factory.Load<WorkRequest>(new ZQuery(WorkRequestSchema.PK, pivots.Select(p => p.WKL_WKR_Request)));
			}

			return Array.Empty<WorkRequest>();
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

		protected virtual JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);

			return result;
		}

		#endregion

		#region IConversationProvider Members

		JobConversation IConversationProvider.eConversation => Conversation;
		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.WorkItem;
		ControllerID IConversationProvider.ParentController => ControllerIDs.WorkItem;
		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();
		bool IConversationProvider.SendEmailNotificationsOnSave => SendEmailNotificationsOnSaveCore;

		protected virtual bool SendEmailNotificationsOnSaveCore => true;

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
			return;
		}

		string IConversationProvider.EmailSubjectContentOverride => default;
		string IConversationProvider.FromAddressOverride => default;
		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#endregion

		#region IWorkQueueMembersProvider Members

		public ZString JobCreatedBy => WKI_SystemCreateUser;

		public ZDateTime JobCreatedDate => WKI_SystemCreateTimeUtc;

		public ZString JobCriteria1 => WorkItemTypeDescription;

		public ZString JobCriteria2 => AreaDescription;

		public ZString JobCriteria3 => ActivityTypeDescription;

		public ZString JobCriteria4 => ActivitySubtypeDescription;

		public ZString JobCriteria5 => PriorityDescription;

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WKI_WorkItemNumber = ZString.Empty;
			WKI_WorkItemType = "UDF";
			WKI_Summary = "Summary for test";
			WKI_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
		}
#endif
		#endregion
	}

	public class WorkItemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WorkItemFetchStrategy(WorkItem workitem)
			: base(workitem)
		{
			this.workitem = workitem;
		}
		readonly WorkItem workitem;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			AddRelatedItemFetchHint(GetColumn(columns));
		}

		protected void AddRelatedItemFetchHint(TableColumn column)
		{
			if (column != null)
			{
				Factory.AddFetchHint(GenPivotSchema.Instance, GenPivotCollectionRelationship.GetRelatedActivitiesQuery(workitem, Core.Constants.GenPivotTypes.ProcessManagement, includeChildren: true, includeParents: true));
				Factory.AddFetchHint(WorkItemRequestLinkSchema.Instance, WorkItemRequestLinkCollectionRelationship.GetRelatedActivitiesQuery(workitem));
			}
		}

		protected virtual TableColumn GetColumn(TableColumn[] columns)
		{
			return columns.FirstOrDefault(c => c.ColumnName.Contains(nameof(WorkItem.RelatedItems)));
		}
	}
}
