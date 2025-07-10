using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public sealed class ContainmentBarrierViewModel : NonPersistentBusinessObject, IContainmentBarrierViewModel, IRequiresWindowsMessagePumpToCollect
	{
		public ContainmentBarrierViewModel(ProcessTask task, string newStatus, ZString? resourceUnderReviewNk = null, bool deselectCancelledTasksFromIteration = false)
			: base(Argument.NotNull(task, nameof(task)).Factory)
		{
			Argument.NotNullOrEmpty(newStatus, nameof(newStatus));

			task.RequireContainmentBarrierTask();

			this.ContainmentBarrierTask = task;
			this.newStatus = newStatus;
			SetResourceUnderReviewNK(resourceUnderReviewNk);
			SetShouldCreateWorkflowForIteration();

			DisposableLeakListener.Instance.RegisterDisposable(this);
			this.deselectCancelledTasksFromIteration = deselectCancelledTasksFromIteration;
		}

		public ContainmentBarrierViewModel(IProcessTask task, string newStatus, ZString? resourceUnderReviewNk = null, bool deselectCancelledTasksFromIteration = false)
			: this(task as ProcessTask, newStatus, resourceUnderReviewNk, deselectCancelledTasksFromIteration)
		{
		}

		readonly bool deselectCancelledTasksFromIteration;

		void SetResourceUnderReviewNK(ZString? explicitValue)
		{
			if (string.IsNullOrEmpty(explicitValue))
			{
				if (WorkflowDataRegistry.Instance.PrefillResourceUnderReview.Value || !Globals.IsUserInteractive)
				{
					var bestResourceNK = FindBestResourceUnderReview();

					if (!bestResourceNK.IsEmpty)
					{
						ResourceUnderReviewNK = bestResourceNK;
					}
				}
				else if (Lookups.ResourceUnderReviewList.Count == 1)
				{
					ResourceUnderReviewNK = Lookups.ResourceUnderReviewList[0].GS_Code;
				}
			}
			else
			{
				ResourceUnderReviewNK = explicitValue.Value;
			}
		}

		void SetShouldCreateWorkflowForIteration()
		{
			if (ContainmentBarrierTask != null)
			{
				var taskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskType(ContainmentBarrierTask.WorkflowType, ContainmentBarrierTask.P9_Type);
				var containmentBarrierType = taskType?.ContainmentBarrierIterationType ?? ZString.Empty;

				if (containmentBarrierType == ContainmentBarrierIterationTypeList.Codes.NWF)
				{
					ShouldCreateWorkflowForIteration = true;
				}
				else if (containmentBarrierType == ContainmentBarrierIterationTypeList.Codes.CWF)
				{
					ShouldCreateWorkflowForIteration = false;
				}

				switch (ContainmentBarrierTask.TaskPatchType)
				{
					case TaskPatchType.Auto:
						ShouldCreateWorkflowForIteration = true;
						break;
					case TaskPatchType.Manual:
						ShouldCreateWorkflowForIteration = false;
						break;
				}
			}
		}

		readonly string newStatus;

		public ProcessTask ContainmentBarrierTask { get; }

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ShouldCreateWorkflowForIteration = WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.Value;
		}

		#endregion

		#region Properties

		public ZString TaskDetails
		{
			get { return ContainmentBarrierTask.P9_Description; }
		}

		bool AreIterateFieldsReadOnly()
		{
			return Response == null || Response.Value != ContainmentBarrierResponses.IterationRequired;
		}

		[List("Lookups.ProcessTaskList")]
		[RelatedBusinessObject("IterateFromTask")]
		[ReadOnlyMember(nameof(IterateFromTaskPK_ReadOnly))]
		[ResourceStringData("ContainmentBarrierViewModel.IterateFromTaskPK", Caption = "Task", FullDescription = "Specifies the task which we want to use as the starting point for the Quality Iteration.")]
		public ZGuid IterateFromTaskPK
		{
			get { return iterateFromTaskPK; }
			set
			{
				SetNonPersistentPropertyValue(IterateFromTaskPKInfo, ref iterateFromTaskPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIterateFromTaskPK();
				}

				UpdatePreviewTasks();
			}
		}

		ZGuid iterateFromTaskPK;

		public ZPropertyInfo IterateFromTaskPKInfo
		{
			get { return GetZPropertyInfo(nameof(IterateFromTaskPK)); }
		}

		bool IterateFromTaskPK_ReadOnly
		{
			get { return AreIterateFieldsReadOnly(); }
		}

		[List("Lookups.JobWorkflows")]
		[RelatedBusinessObject("IterateFromWorkflow")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IterateFromWorkflowPK_ReadOnly")]
		[ReadOnlyMember("IterateFromWorkflowPK_ReadOnly")]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject IterateFromWorkflow (IProcessHeader) is an interface")]
		[ResourceStringData("ContainmentBarrierViewModel.IterateFromWorkflowPK", Caption = "Workflow", FullDescription = "Specifies the workflow which we want to use as the starting point for the Quality Iteration.")]
		public ZGuid IterateFromWorkflowPK
		{
			get { return iterateFromWorkflowPK; }
			set
			{
				SetNonPersistentPropertyValue(IterateFromWorkflowPKInfo, ref iterateFromWorkflowPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIterateFromWorkflowPK();
				}

				UpdateTasksList();
				UpdatePreviewTasks();
				RefreshBinding();
			}
		}

		ZGuid iterateFromWorkflowPK;

		public ZPropertyInfo IterateFromWorkflowPKInfo
		{
			get { return GetZPropertyInfo(nameof(IterateFromWorkflowPK)); }
		}

		[List("Lookups.IterationReasonsRegistryLists")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IterateReasonPK_ReadOnly")]
		[ReadOnlyMember("IterateReasonPK_ReadOnly")]
		[ResourceStringData("ContainmentBarrierViewModel.IterateReasonPKInfo", Caption = "Reason", FullDescription = "Specifies the reason why the iteration is needed.")]
		public ZGuid IterateReasonPK
		{
			get { return iterateReasonPK; }
			set
			{
				SetNonPersistentPropertyValue(IterateReasonPKInfo, ref iterateReasonPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIterateReasonPK();
				}
			}
		}
		ZGuid iterateReasonPK;

		public ZPropertyInfo IterateReasonPKInfo
		{
			get { return GetZPropertyInfo(nameof(IterateReasonPK)); }
		}

		[MaxLength(AutoGlbStaff.Schema.GS_CodeMaxLength)]
		[List("Lookups.ResourceUnderReviewList")]
		[ResourceStringData("ContainmentBarrierViewModel.ResourceUnderReviewPKInfo", Caption = "User whose work is under review", FullDescription = "The user whose work is under review.")]
		public ZString ResourceUnderReviewNK
		{
			get { return resourceUnderReviewNK; }
			set
			{
				SetNonPersistentPropertyValue(ResourceUnderReviewNKInfo, ref resourceUnderReviewNK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateResourceUnderReviewNK();
				}
			}
		}

		ZString resourceUnderReviewNK;

		public ZPropertyInfo ResourceUnderReviewNKInfo
		{
			get { return GetZPropertyInfo(nameof(ResourceUnderReviewNK)); }
		}

		public IEnumerable<string> TaskTypesToNotRepeat
		{
			get { return taskTypesToNotRepeat ?? (taskTypesToNotRepeat = Array.Empty<string>()); }
			set { taskTypesToNotRepeat = value; }
		}
		IEnumerable<string> taskTypesToNotRepeat;

		public IEnumerable<string> TaskTypesToSuspendOnRepeatIfQcbTask
		{
			get { return taskTypesToSuspendOnRepeatIfQcbTask ?? (taskTypesToSuspendOnRepeatIfQcbTask = Array.Empty<string>()); }
			set { taskTypesToSuspendOnRepeatIfQcbTask = value; }
		}
		IEnumerable<string> taskTypesToSuspendOnRepeatIfQcbTask;

		internal string IterationType { get; set; }

		[ResourceStringData("8bca2dda-350d-46b8-8470-9e9ac9631d17", Caption = "Create iteration in a new workflow",
			FullDescription = "When enabled, the quality iteration will be created in a new workflow which is a child of the workflow for the containment barrier task. Otherwise the quality iteration tasks will be added to the existing workflow.")]
		public ZBool ShouldCreateWorkflowForIteration
		{
			get => shouldCreateWorkflowForIteration;
			set => SetNonPersistentPropertyValue(ShouldCreateWorkflowForIterationInfo, ref shouldCreateWorkflowForIteration, value);
		}

		ZBool shouldCreateWorkflowForIteration;

		public ZPropertyInfo ShouldCreateWorkflowForIterationInfo => GetZPropertyInfo(nameof(ShouldCreateWorkflowForIteration));

		public bool ShouldCreateWorkflowForIteration_ReadOnly => !WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.Value;

		public ZString HintLabelText => GetHintLabelText();

		public ZPropertyInfo HintLabelTextInfo => GetZPropertyInfo(nameof(HintLabelText));

		ZString GetHintLabelText()
		{
			return ShouldCreateWorkflowForIteration
				? Res.GetString("2f25a9f8-b538-46e6-a39d-cd5d97ebee17", "The following tasks will be duplicated and added to a Quality Iteration workflow. Un-tick those you do not wish to include.")
				: Res.GetString("a71c1420-4d48-424f-9379-1d702310cf59", "The following tasks will be duplicated and added to the workflow for the containment barrier task. Un-tick those you do not wish to include.");
		}

		#endregion

		#region Related Business Objects

		public IProcessHeader IterateFromWorkflow
		{
			get { return Factory.Load<IProcessHeader>(IterateFromWorkflowPK); }
		}

		public ProcessTask IterateFromTask
		{
			get { return Factory.Load<ProcessTask>(iterateFromTaskPK); }
		}

		public ZString IterationReasonValidation
		{
			get
			{
				if (iterationReasonValidation.IsEmpty)
				{
					iterationReasonValidation = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonValidationFromWorkflowCode(ContainmentBarrierTask.Parent.WorkflowType);
				}
				return iterationReasonValidation;
			}
		}
		ZString iterationReasonValidation;

		public ProcessTaskCollectionView JobTasksWithWorkflowFiltering
		{
			get { return jobTasksWithWorkflowFiltering ?? (jobTasksWithWorkflowFiltering = new IterateFromProcessTaskCollectionView(ContainmentBarrierTask)); }
		}

		ProcessTaskCollectionView jobTasksWithWorkflowFiltering;

		ProcessTaskCollectionView AllJobTasks
		{
			get { return allJobTasks ?? (allJobTasks = new IterateFromProcessTaskCollectionView(ContainmentBarrierTask)); }
		}

		ProcessTaskCollectionView allJobTasks;

		[ChildEditable]
		public TaskPreviewCollection IterationTaskPreviews
		{
			get
			{
				if (iterationTaskPreviews == null)
				{
					iterationTaskPreviews = new TaskPreviewCollection();
					RegisterEditableChildObject(iterationTaskPreviews);
				}

				return iterationTaskPreviews;
			}
		}

		TaskPreviewCollection iterationTaskPreviews;

		[ChildEditable]
		public ProcessTaskIterationLinkCollection IterationLinks
		{
			get
			{
				if (iterationLinks == null)
				{
					iterationLinks = new ProcessTaskIterationLinkCollection(ContainmentBarrierTask);
					RegisterEditableChildObject(iterationLinks);
				}

				return iterationLinks;
			}
		}

		ProcessTaskIterationLinkCollection iterationLinks;

		public string QcbCreatingUserLoginName { get; set; }

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ContainmentBarrierViewModelValidation Validation
		{
			get { return GetNewValidation(); }
		}

		public ContainmentBarrierViewModelValidation GetNewValidation()
		{
			return new ContainmentBarrierViewModelValidation(this);
		}

		#endregion

		#region Containment Barrier Response

		public ContainmentBarrierResponses ValidResponses
		{
			get
			{
				switch (newStatus)
				{
					case ProcessTaskStatusCodeList.Codes.Closed:
						var result = ContainmentBarrierResponses.Passed | ContainmentBarrierResponses.IterationRequired | ContainmentBarrierResponses.Canceled;

						var duplicateQCBTasks = GetDuplicateQCBTasks().ToArray();
						if (duplicateQCBTasks.Length > 0)
						{
							if (duplicateQCBTasks.Any(t => t.IsClosed && t.GetContainmentBarrierIterationLinks().Any(l => l.P9I_Outcome == IterationLinkOutcomeList.Codes.IterationRequired)))
							{
								result |= ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource;
							}
							else if (duplicateQCBTasks.Where(t => t.IsOpen).Any())
							{
								result |= ContainmentBarrierResponses.DeferredToAnotherResource;
							}
						}

						return result;

					case ProcessTaskStatusCodeList.Codes.Cancelled:
						return ContainmentBarrierResponses.Canceled;

					default:
						return ContainmentBarrierResponses.None;
				}
			}
		}

		void SetDefaultResponseValuesForIterationRequired()
		{
			if (response == null || response.Value != ContainmentBarrierResponses.IterationRequired)
			{
				IterateFromTaskPK = ZGuid.Empty;
				IterateReasonPK = ZGuid.Empty;
			}
			else
			{
				IterateFromTaskPK = FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeSameResourceAsQcbTask);
				IterateReasonPK = Lookups.IterationReasonsRegistryLists.Count == 1 ? Lookups.IterationReasonsRegistryLists[0].PK : ZGuid.Empty;
			}
		}

		[BusinessObjectTestExclude] // We want to return a null response when none has been selected (ie the form was closed without selecting a choice)
		public ContainmentBarrierResponses? Response
		{
			get { return response; }
			set
			{
				if (!ValidResponses.HasFlag(value))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Response [{0}] is not valid at this time", value));
				}

				response = value;
				SetDefaultResponseValuesForIterationRequired();
			}
		}

		ContainmentBarrierResponses? response;

		#endregion

		#region Commit Response

		public void CommitResponse(Action<IProcessHeader> adjustTasksAfterCopyTasks = null, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks = null)
		{
			var isUserSpecified = string.IsNullOrEmpty(QcbCreatingUserLoginName);
			var userContext = isUserSpecified ? EnvProxy.Instance.CurrentUserContext : EnvProxy.Instance.NewUserContext(QcbCreatingUserLoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			using (EnvProxy.Instance.SuppressSwitchContextCheck())
			using (EnvProxy.Instance.SetTemporaryUserContext(userContext))
			{
				if (isUserSpecified && EnvProxy.Instance.CurrentUser == null)
				{
					throw new InvalidOperationException("Invalid QcbCreatingUserLoginName specified: " + QcbCreatingUserLoginName);
				}

				if (Response != null)
				{
					switch (Response.Value)
					{
						case ContainmentBarrierResponses.IterationRequired:
							CommitQualityIteration(adjustTasksAfterCopyTasks, customQualityIterationTasks);
							break;
						case ContainmentBarrierResponses.DeferredToAnotherResource:
							CreateLinkToNonIteration(ContainmentBarrierResponses.DeferredToAnotherResource);
							break;
						case ContainmentBarrierResponses.Passed:
							CreateLinkToNonIteration(ContainmentBarrierResponses.Passed);
							break;
					}
				}

				UpdateQCBTaskNotes();
			}
		}

		void UpdateQCBTaskNotes()
		{
			var taskNotes = GetResponseTaskNotes(Response ?? ContainmentBarrierResponses.None);
			if (!string.IsNullOrEmpty(taskNotes))
			{
				ContainmentBarrierTask.AppendNote(taskNotes);
			}
		}

		void CommitQualityIteration(Action<IProcessHeader> adjustTasksAfterCopyTasks, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks)
		{
			var iterationTaskPreviewsIncluded = IterationTaskPreviews.Cast<TaskPreviewCopy>().Where(t => t.IncludeInIteration).ToArray();
			var iterationTaskPreviewsIncludedCount = iterationTaskPreviewsIncluded.Length;
			var workflow = GetWorkflowForIteration();
			var passedContainmentBarriers = new LinkedList<IProcessTask>();
			var tasksFollowingContainmentBarrierTask = ContainmentBarrierTask.Parent.WorkflowItems.Tasks
				.Cast<ProcessTask>()
				.Where(t => t.IsOpen && t.P9_Sequence > ContainmentBarrierTask.P9_Sequence)
				.Where(t => t.P9_FH_ProcessHeader == ContainmentBarrierTask.P9_FH_ProcessHeader || IsTaskWithinQcbTaskWorkflowAncestry(ContainmentBarrierTask, t))
				.ToList();

			var newIterationTasks = new List<ProcessTask>(iterationTaskPreviewsIncludedCount);

			int j = 1;

			foreach (var taskDescriptor in customQualityIterationTasks ?? Array.Empty<QualityIterationTaskDescriptor>())
			{
				var task = ContainmentBarrierTask.Parent.WorkflowItems.Tasks.AddNew();
				using (task.SuspendUpdatingIterationPivots())
				{
					newIterationTasks.Add(task);

					task.P9_GS_NKAssignedStaffMember = taskDescriptor.AssignedStaffCode;
					task.P9_Type = taskDescriptor.Type;
					task.P9_Description = taskDescriptor.Description;
					task.P9_EstDuration = taskDescriptor.EstDuration;

					task.P9_Sequence = ContainmentBarrierTask.P9_Sequence + j;

					if (workflow != null)
					{
						task.P9_FH_ProcessHeader = workflow.PK;
					}

					j++;

					ContainmentBarrierTask.Parent.WorkflowItems.Tasks.SetDefaultsForNewTask(task, false);
				}
			}

			for (int i = 0; i < iterationTaskPreviewsIncludedCount; i++)
			{
				var taskPreview = iterationTaskPreviewsIncluded[i];

				using (taskPreview.Task.SuspendUpdatingIterationPivots())
				{
					var taskCopy = (ProcessTask)taskPreview.Task.Clone();

					using (taskCopy.SuspendUpdatingIterationPivots())
					{
						newIterationTasks.Add(taskCopy);
						taskCopy.P9_CardNote = ZString.Empty;
						taskCopy.P9_NotesAsString = ZString.Empty;

						if (i != 0 && taskPreview.Sequence > iterationTaskPreviewsIncluded[i - 1].Sequence)
						{
							j++;
						}

						taskCopy.P9_Sequence = ContainmentBarrierTask.P9_Sequence + j;

						if (taskPreview.Task.PK == ContainmentBarrierTask.PK && TaskTypesToSuspendOnRepeatIfQcbTask.Contains(taskCopy.P9_Type.ToString()))
						{
							taskCopy.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						}

						if (workflow != null)
						{
							taskCopy.P9_FH_ProcessHeader = workflow.PK;
						}

						if (taskPreview.Task != ContainmentBarrierTask && taskPreview.Task.IsQualityContainmentBarrierTask() && taskPreview.Task.GetContainmentBarrierIterationLinks().Count == 0)
						{
							passedContainmentBarriers.AddLast(taskPreview.Task);
						}

						ContainmentBarrierTask.Parent.WorkflowItems.SetDefaultsForNewTask(taskCopy, false);
						ContainmentBarrierTask.Parent.WorkflowItems.Tasks.Add(taskCopy);
					}
				}
			}

			tasksFollowingContainmentBarrierTask.ForEach(t => t.P9_Sequence += newIterationTasks.Count);

			var bindingListView = ContainmentBarrierTask.ProcessHeader?.TaskCollectionIncludingChildWorkflowTasksBindingListView;
			if (bindingListView != null)
			{
				ListSortDescriptionCollection sorts;
				if (bindingListView.SortDescriptions.Count == 0)
				{
					var properties = TypeDescriptor.GetProperties(typeof(IProcessTask));
					sorts = new ListSortDescriptionCollection(new ListSortDescription[]
					{
						new ListSortDescription(properties[ProcessTasksSchema.P9_Sequence.Name], ListSortDirection.Ascending)
					});
				}
				else
				{
					sorts = bindingListView.SortDescriptions;
				}
				bindingListView.ApplySort(sorts);
			}

			CreatePivotLinks(workflow, passedContainmentBarriers, newIterationTasks);

			adjustTasksAfterCopyTasks?.Invoke(workflow);
		}

		static bool IsTaskWithinQcbTaskWorkflowAncestry(ProcessTask qcbTask, ProcessTask otherTask)
		{
			return otherTask.P9_FH_ProcessHeader.IsValid
				&& qcbTask.P9_FH_ProcessHeader.IsValid
				&& qcbTask.ProcessHeader.GetAncestors().Any(w => w.PK == otherTask.P9_FH_ProcessHeader);
		}

		IProcessHeader GetWorkflowForIteration()
		{
			return ShouldCreateWorkflowForIteration ? CreateQualityIterationWorkflow() : ContainmentBarrierTask.ProcessHeader;
		}

		IProcessHeader CreateQualityIterationWorkflow()
		{
			var workflow = ContainmentBarrierTask.ProcessHeader;
			var jobHeader = workflow?.JobHeader;

			return jobHeader?.CreateQualityIterationWorkflow(workflow, IterationType);
		}

		void CreatePivotLinks(IProcessHeader workflow, LinkedList<IProcessTask> passedContainmentBarriers, List<ProcessTask> newIterationTasks)
		{
			var iterateFromTask = IterateFromTask;

			var iterateReasonCode = ZString.Empty;
			if (IterateReasonPK != ZGuid.Empty)
			{
				iterateReasonCode = ((WorkflowIterationReason)Lookups.IterationReasonsRegistryLists.FindByPK(IterateReasonPK))?.Code ?? ZString.Empty;
			}

			if (iterateFromTask != null)
			{
				var iterationLink = CreateLinkToIterateFromTask(workflow, iterateReasonCode);

				foreach (var task in newIterationTasks)
				{
					iterationLink.TaskPivots.AddNewForTask(task);
				}
			}

			foreach (var passedQCB in passedContainmentBarriers)
			{
				CreateLinkToPassedContainmentBarrier(passedQCB, workflow, iterateReasonCode);
			}
		}

		#endregion

		#region Iteration Link Creation

		ProcessTaskIterationLink CreateLinkToIterateFromTask(IProcessHeader qualityIterationWorkflow, ZString iterationReason)
		{
			return CreateLink(IterationLinkTypeList.Codes.QualityIterationTask, IterateFromTask, qualityIterationWorkflow, iterationReason, ContainmentBarrierResponses.IterationRequired);
		}

		ProcessTaskIterationLink CreateLinkToPassedContainmentBarrier(IProcessTask passedContainmentBarrierTask, IProcessHeader qualityIterationWorkflow, ZString iterationReason)
		{
			return CreateLink(IterationLinkTypeList.Codes.PassedContainmentBarrier, passedContainmentBarrierTask, qualityIterationWorkflow, iterationReason, ContainmentBarrierResponses.IterationRequired);
		}

		ProcessTaskIterationLink CreateLinkToNonIteration(ContainmentBarrierResponses containmentBarrierResponses)
		{
			return CreateLink(linkType: IterationLinkTypeList.Codes.QualityIterationTask, relevantTask: null, qualityIterationWorkflow: null, iterationReason: ZString.Empty, containmentBarrierResponses: containmentBarrierResponses);
		}

		ProcessTaskIterationLink CreateLink(string linkType, IProcessTask relevantTask, IProcessHeader qualityIterationWorkflow, ZString iterationReason, ContainmentBarrierResponses containmentBarrierResponses)
		{
			var link = IterationLinks.AddNew();

			if (relevantTask != null)
			{
				link.P9I_P9_IterationTask = relevantTask.PK;
			}

			if (qualityIterationWorkflow != null)
			{
				link.P9I_FH_IterationWorkflow = qualityIterationWorkflow.PK;
			}

			link.P9I_LinkType = linkType;
			link.P9I_IterationReason = iterationReason;

			if (!ResourceUnderReviewNK.IsEmpty)
			{
				link.P9I_GS_NKResourceUnderReview = ResourceUnderReviewNK;
			}

			SetSequence(link, qualityIterationWorkflow, containmentBarrierResponses);
			SetParentIteration(link);
			SetOutcome(link, containmentBarrierResponses);

			return link;
		}

		public static void SetOutcome(IProcessTaskIterationLink link, ContainmentBarrierResponses containmentBarrierResponses)
		{
			switch (containmentBarrierResponses)
			{
				case ContainmentBarrierResponses.IterationRequired:
					link.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
					break;
				case ContainmentBarrierResponses.DeferredToAnotherResource:
					link.P9I_Outcome = IterationLinkOutcomeList.Codes.Deferred;
					break;
				case ContainmentBarrierResponses.Passed:
					link.P9I_Outcome = IterationLinkOutcomeList.Codes.Passed;
					break;
			}
		}

		void SetSequence(ProcessTaskIterationLink link, IProcessHeader qualityIterationWorkflow, ContainmentBarrierResponses containmentBarrierResponses)
		{
			if (ShouldSetSequenceForNewLink(link, qualityIterationWorkflow, containmentBarrierResponses))
			{
				var query = new ZQuery(ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow, qualityIterationWorkflow.PK);
				query.AddToFilter(ProcessTaskIterationLinkSchema.PK, SQLComparisonOperator.NotEqual, link.PK);
				query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);
				query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_Outcome, IterationLinkOutcomeList.Codes.IterationRequired);
				var existingLinksForThisWorkflow = link.Factory.Load<ProcessTaskIterationLink>(query);

				var maxExistingSequence = existingLinksForThisWorkflow.Any() ? existingLinksForThisWorkflow.Max(x => x.P9I_Sequence) : (ZByte)0;

				if (maxExistingSequence == byte.MaxValue)
				{
					link.P9I_Sequence = byte.MaxValue;
				}
				else
				{
					link.P9I_Sequence = ++maxExistingSequence;
				}
			}
		}

		bool ShouldSetSequenceForNewLink(ProcessTaskIterationLink link, IProcessHeader qualityIterationWorkflow, ContainmentBarrierResponses containmentBarrierResponses)
		{
			return containmentBarrierResponses == ContainmentBarrierResponses.IterationRequired &&
				   link.P9I_LinkType == IterationLinkTypeList.Codes.QualityIterationTask &&
				   !ShouldCreateWorkflowForIteration &&
				   qualityIterationWorkflow != null;
		}

		void SetParentIteration(ProcessTaskIterationLink link)
		{
			var parentIterationLink = ContainmentBarrierTask.IterationPivot?.Iteration;

			if (parentIterationLink != null && parentIterationLink.P9I_Outcome == IterationLinkOutcomeList.Codes.IterationRequired)
			{
				link.P9I_P9I_ParentIteration = parentIterationLink.PK;
			}
		}

		#endregion

		#region Lookups

		public ContainmentBarrierViewModelLookups Lookups => lookups ?? (lookups = new ContainmentBarrierViewModelLookups(this));
		ContainmentBarrierViewModelLookups lookups;

		#endregion

		#region Implementation

		public ZString FindBestResourceUnderReview()
		{
			return BestResourceUnderReviewProvider.FindDefaultResourceUnderReview(ContainmentBarrierTask);
		}

		public ZGuid FindBestIterateFromTask(ContainmentBarrierIterateFromTaskSelectionMode mode, IEnumerable<string> eligibleTaskTypes = null)
		{
			var iterateFromTask = (from ProcessTask t in JobTasksWithWorkflowFiltering
								   where (!mode.HasFlag(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeSameResourceAsQcbTask) || t.P9_GS_NKAssignedStaffMember != ContainmentBarrierTask.P9_GS_NKAssignedStaffMember)
								   && (!mode.HasFlag(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeDifferentResourceAsQcbTask) || t.P9_GS_NKAssignedStaffMember == ContainmentBarrierTask.P9_GS_NKAssignedStaffMember)
								   && (!mode.HasFlag(ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks) || !t.IsQualityContainmentBarrierTask())
								   && t.IsClosed
								   && (t.P9_Sequence < ContainmentBarrierTask.P9_Sequence || t.P9_FH_ProcessHeader != ContainmentBarrierTask.P9_FH_ProcessHeader)
								   && (eligibleTaskTypes == null || eligibleTaskTypes.Contains(t.P9_Type.ToString()))
								   orderby t.P9_FH_ProcessHeader == ContainmentBarrierTask.P9_FH_ProcessHeader descending,
								   t.P9_Sequence descending
								   select t).FirstOrDefault();

			return iterateFromTask != null ? iterateFromTask.PK : ZGuid.Empty;
		}

		void UpdatePreviewTasks()
		{
			using (IterationTaskPreviews.SuspendListChanged())
			{
				var iterateFromTask = IterateFromTask;
				var tasks = iterateFromTask != null
					? AllJobTasks.Cast<ProcessTask>().Where(t => t.PK == iterateFromTask.PK || t.PK == ContainmentBarrierTask.PK
						|| (ContainmentBarrierTaskHelper.GetContainmentBarriersPrerequisiteTasks(t, checkWithinJobOnly: true).Any(t2 => (ProcessTask)t2 == iterateFromTask) && !IsCurrentTaskWorkflowParentOfIterateFromTaskWorkflow(t) && !TaskTypesToNotRepeat.Contains(t.P9_Type.ToString()) && t.P9_Sequence > iterateFromTask.P9_Sequence))
						.Append(ContainmentBarrierTask)
					: Enumerable.Empty<ProcessTask>();

				IterationTaskPreviews.RemoveAndDeleteAll();

				var lastTask = tasks.LastOrDefault();
				foreach (var task in tasks)
				{
					task.Factory.AddFetchHint(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, task.PK);
					var newPreview = new TaskPreviewCopy(task, this);

					if (deselectCancelledTasksFromIteration && task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled && task.PK != lastTask.PK)
					{
						newPreview.IncludeInIteration = false;
					}
					IterationTaskPreviews.Add(newPreview);
				}
			}
		}

		bool IsCurrentTaskWorkflowParentOfIterateFromTaskWorkflow(ProcessTask task)
		{
			var iterateFromTask = IterateFromTask;
			var taskProcessHeader = task.ProcessHeader;
			var iterateFromTaskProcessHeader = iterateFromTask.ProcessHeader;

			if (taskProcessHeader != null && iterateFromTaskProcessHeader != null)
			{
				return taskProcessHeader.IsParentOf(iterateFromTaskProcessHeader);
			}

			return false;
		}

		public void UpdateTasksList()
		{
			((IterateFromProcessTaskCollectionView)JobTasksWithWorkflowFiltering).SelectedWorkflow = IterateFromWorkflow;
			((IterateFromProcessTaskCollectionView)JobTasksWithWorkflowFiltering).Rebuild();
			Lookups.ProcessTaskList = new ProcessTaskFriendlyViewCollectionView(JobTasksWithWorkflowFiltering);
			iterateFromTaskPK = ZGuid.Empty;
		}

		string GetResponseTaskNotes(ContainmentBarrierResponses barrierResponse)
		{
			switch (barrierResponse)
			{
				case ContainmentBarrierResponses.Passed:
					return Res.GetString("2d2ea446-7f18-4a2b-b726-117f8b943892", "Containment Barrier Passed");

				case ContainmentBarrierResponses.IterationRequired:
					{
						var iterateReason = ((WorkflowIterationReason)Lookups.IterationReasonsRegistryLists.FindByPK(IterateReasonPK));

						var taskNotes = Res.GetString("ef048548-b4e5-47b7-a1c0-cdbcb539bef2", "Containment Barrier triggered a Quality Iteration");
						if (iterateReason != null)
						{
							taskNotes += " - ";
							taskNotes += Res.GetString("1271196f-167e-4a9d-a6f9-d03b82d2ce53", "Quality Iteration Reason: {0} - {1}", iterateReason.Code, iterateReason.Description);
						}
						return taskNotes;
					}

				case ContainmentBarrierResponses.DeferredToAnotherResource:
					return Res.GetString("5299b4ee-0b3e-4532-96bc-b5addecd91bc", "Containment Barrier verification was deferred to {0}", GetDuplicateQCBResourceNames(findOpenQCBs: true));

				case ContainmentBarrierResponses.AcceptIterationCreatedByOtherResource:
					return Res.GetString("e6b4ca5f-218d-4816-9549-64b4af41cfaa", "Accepted Quality Iteration created by {0}", GetDuplicateQCBResourceNames(findOpenQCBs: false));

				case ContainmentBarrierResponses.Canceled:
					return Res.GetString("b4c02ca3-d8e9-405d-a66d-735a97afb0e5", "Containment Barrier verification canceled");

				default:
					return string.Empty;
			}
		}

		string GetDuplicateQCBResourceNames(bool findOpenQCBs)
		{
			return string.Join(", ", GetDuplicateQCBTasks().Where(t => t.IsOpen == findOpenQCBs).Select(t => t.StaffName));
		}

		IEnumerable<ProcessTask> GetDuplicateQCBTasks()
		{
			return from ProcessTask t in ContainmentBarrierTask.Parent.WorkflowItems.Tasks
				   where t.P9_Sequence == ContainmentBarrierTask.P9_Sequence
				   where t.P9_Type == ContainmentBarrierTask.P9_Type
				   where t.PK != ContainmentBarrierTask.PK
				   where t.P9_FH_ProcessHeader == ContainmentBarrierTask.P9_FH_ProcessHeader
				   where !t.P9_GS_NKAssignedStaffMember.IsEmpty
				   select t;
		}

		#endregion

		#region IDisposable Support

		public void Dispose()
		{
			jobTasksWithWorkflowFiltering?.UnhookCollection();
			allJobTasks?.UnhookCollection();
			iterationLinks?.Deactivate();
			lookups?.ResourceUnderReviewList.Deactivate();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion
	}
}
