using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[TestExcludeWorkflowProviderHasTestCase]
	[ActionFieldFollow(true)]
	[ModuleID(ModuleId.ProcessTasks)]
	public class ProcessTaskCollection : BusinessObjectCollection<ProcessTask>,
		IWorkflowProviderCollection,
		IProcessTaskCollection,
		IRefreshProcessTaskCollection
	{
		public ProcessTaskCollection(BusinessObject parent)
			: this(parent, new ZQuery())
		{
		}

		public ProcessTaskCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			IsManagedForDataRefresh = true;
			factory.Saved += Factory_Saved;
		}

		public ProcessTaskCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
			IsManagedForDataRefresh = true;
			factory.Saved += Factory_Saved;
		}

		public ProcessTaskCollection(BusinessObject parent, ZQuery additionalFilter)
			: this(parent.Factory, additionalFilter)
		{
			Parent = parent;
			TrackCreation();
		}

		public readonly BusinessObject Parent;

		#region Track collection creation

		void TrackCreation()
		{
#if DEBUG
			if (canCreateCounter <= 0)
			{
				ErrorReporter.ReportOnce("ProcesssTaskCollection was created but there is no proof you thought to use the ProcessTaskCollectionFactory to create it.");
			}
#endif
			GetIsParentLoadedCache(Factory).Add(Parent.PK);
		}

		public static IDisposable CanCreateTaskCollection()
		{
#if DEBUG
			canCreateCounter++;
			return new DisposableAction(() => canCreateCounter--);
#else
			return null;
#endif
		}

#if DEBUG
		[ThreadStatic]
		static int canCreateCounter;
#endif

		public static bool IsParentLoaded(BusinessObjectFactory factory, ZGuid pk) => GetIsParentLoadedCache(factory).Contains(pk);

		static HashSet<ZGuid> GetIsParentLoadedCache(BusinessObjectFactory factory) => factory.GetCachedValue("ProcessTaskCollection_Creation", () => new HashSet<ZGuid>());

		#endregion

		#region CreateNewCollection

		public virtual ProcessTaskCollection CreateNewCollection()
		{
			return (ProcessTaskCollection)Activator.CreateInstance(GetType(), Parent);
		}

		#endregion

		#region Compare

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == "EffectiveTaskNudge")
			{
				return new EffectiveTaskNudgeComparer(property, direction);
			}
			else if (property.Name == "WorkflowSequence")
			{
				return new TaskWorkflowSequenceComparer(property, direction);
			}
			return base.GetComparerForSort(property, direction);
		}

		class EffectiveTaskNudgeComparer : PropertyComparer
		{
			internal EffectiveTaskNudgeComparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				ProcessTask task1 = (ProcessTask)x;
				ProcessTask task2 = (ProcessTask)y;

				decimal nudge1, nudge2;
				var nudge1Parseable = decimal.TryParse(task1.EffectiveTaskNudge, out nudge1);
				var nudge2Parseable = decimal.TryParse(task2.EffectiveTaskNudge, out nudge2);

				int result = 0;

				if (!(nudge1Parseable && nudge2Parseable))
				{
					if (!nudge1Parseable && !nudge2Parseable)
					{
						result = 0;
					}

					if (!nudge1Parseable && nudge2Parseable)
					{
						result = -1;
					}

					if (nudge1Parseable && !nudge2Parseable)
					{
						result = 1;
					}
				}
				else
				{
					result = nudge1.CompareTo(nudge2);
				}

				if (Direction == ListSortDirection.Ascending)
				{
					return result;
				}
				else
				{
					return result * -1;
				}
			}
		}

		class TaskWorkflowSequenceComparer : PropertyComparer
		{
			internal TaskWorkflowSequenceComparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			#region Comparison Constant Values

			// for comparison methods:
			// if returned < 0, the first value is smaller than the second
			// if the return == 0, the two values are the same
			// if returned > 0, the first value is greater than the second
			const int XGreaterThanY = 1;
			const int YGreaterThanX = -1;
			const int XEqualsY = 0;

			#endregion

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				if (x == y)
				{
					return XEqualsY;
				}

				if (x is ProcessTask processTaskX && y is ProcessTask processTaskY)
				{
					var sequenceX = GetSequenceSegments(processTaskX.WorkflowSequence);
					var sequenceY = GetSequenceSegments(processTaskY.WorkflowSequence);
					return Direction == ListSortDirection.Ascending
						? CompareWorkflowSequence(sequenceX, sequenceY)
						: CompareWorkflowSequence(sequenceY, sequenceX);
				}

				return base.Compare(x, y);
			}

			static int[] GetSequenceSegments(string sequence)
			{
				return sequence.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
			}

			static int CompareWorkflowSequence(int[] sequenceX, int[] sequenceY)
			{
				var length = Math.Max(sequenceX.Length, sequenceY.Length);

				for (int i = 0; i < length; i++)
				{
					if (i >= sequenceX.Length)
					{
						return YGreaterThanX;
					}
					else if (i >= sequenceY.Length)
					{
						return XGreaterThanY;
					}
					else
					{
						var value1 = sequenceX[i];
						var value2 = sequenceY[i];

						if (value1 == value2)
						{
							continue;
						}
						else
						{
							return value1.CompareTo(value2);
						}
					}
				}

				return XEqualsY;
			}
		}

		#endregion

		#region Milestones / Tasks / Exceptions / Triggers Views

		bool wasInTasks;

		[ActionFieldFollow(true)]
		public virtual ProcessTaskCollectionView Tasks
		{
			get
			{
				if (tasks == null)
				{
					bool previouslyWasInTasks = wasInTasks;
					wasInTasks = true;
					tasks = new ProcessTaskCollectionView(this);
					if (!previouslyWasInTasks)
					{
						Parent.RegisterEditableChildObject(tasks);
					}
				}
				return tasks;
			}
		}
		ProcessTaskCollectionView tasks;

		bool wasInMilestones;

		[ActionFieldFollow(true)]
		public MilestoneCollectionView Milestones
		{
			get
			{
				if (milestones == null)
				{
					bool previouslyWasInMilestones = wasInMilestones;
					wasInMilestones = true;
					milestones = new MilestoneCollectionView(this);
					if (!previouslyWasInMilestones)
					{
						Parent.RegisterEditableChildObject(milestones);
					}
				}
				return milestones;
			}
		}
		MilestoneCollectionView milestones;

		public CodeDescriptionPairList CompletionMilestoneCodeDescriptionPairList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var milestones = Milestones?.Cast<ProcessTask>();

				if (milestones == null)
				{
					return result;
				}

				foreach (var milestone in milestones)
				{
					result.AddPair(milestone.PK, milestone.P9_MilestoneCompletionPivotKey, milestone.P9_Description);
				}

				if (result.Count > 0)
				{
					result.AddPair(Guid.Empty, string.Empty, string.Empty);
				}

				return result;
			}
		}

		bool wasInMilestonesIncludingRelated;

		public MilestoneCollectionIncludingRelatedView MilestonesIncludingRelated
		{
			get
			{
				if (milestonesIncludingRelated == null)
				{
					bool previouslyWasInMilestonesIncludingRelated = wasInMilestonesIncludingRelated;
					wasInMilestonesIncludingRelated = true;
					milestonesIncludingRelated = new MilestoneCollectionIncludingRelatedView(this);
					if (!previouslyWasInMilestonesIncludingRelated)
					{
						Parent.RegisterEditableChildObject(milestonesIncludingRelated);
					}
				}
				return milestonesIncludingRelated;
			}
		}
		MilestoneCollectionIncludingRelatedView milestonesIncludingRelated;

		bool wasInMilestonesIncludingRelatedSortable;

		public MilestoneCollectionIncludingRelatedSortableView MilestonesIncludingRelatedSortable
		{
			get
			{
				if (milestonesIncludingRelatedSortable == null)
				{
					bool previouslyWasInMilestonesIncludingRelatedSortable = wasInMilestonesIncludingRelatedSortable;
					wasInMilestonesIncludingRelatedSortable = true;
					milestonesIncludingRelatedSortable = new MilestoneCollectionIncludingRelatedSortableView(this);
					if (!previouslyWasInMilestonesIncludingRelatedSortable)
					{
						Parent.RegisterEditableChildObject(milestonesIncludingRelatedSortable);
					}
				}
				return milestonesIncludingRelatedSortable;
			}
		}
		MilestoneCollectionIncludingRelatedSortableView milestonesIncludingRelatedSortable;

		bool wasInExceptions;

		public ExceptionCollectionView Exceptions
		{
			get
			{
				if (exceptions == null)
				{
					bool previouslyWasInExceptions = wasInExceptions;
					wasInExceptions = true;
					exceptions = new ExceptionCollectionView(this);
					if (!previouslyWasInExceptions)
					{
						Parent.RegisterEditableChildObject(exceptions);
					}
				}
				return exceptions;
			}
		}
		ExceptionCollectionView exceptions;

		bool wasInExceptionsIncludingRelated;

		public ExceptionCollectionIncludingRelatedView ExceptionsIncludingRelated
		{
			get
			{
				if (exceptionsIncludingRelated == null)
				{
					bool previouslyWasInExceptionsIncludingRelated = wasInExceptionsIncludingRelated;
					wasInExceptionsIncludingRelated = true;
					exceptionsIncludingRelated = new ExceptionCollectionIncludingRelatedView(this);
					if (!previouslyWasInExceptionsIncludingRelated)
					{
						Parent.RegisterEditableChildObject(exceptionsIncludingRelated);
					}
				}
				return exceptionsIncludingRelated;
			}
		}
		ExceptionCollectionIncludingRelatedView exceptionsIncludingRelated;

		bool wasInTriggers;

		[ActionFieldFollow(true)]
		public WorkflowTriggerCollectionView Triggers
		{
			get
			{
				if (triggers == null)
				{
					bool previouslyWasInTriggers = wasInTriggers;
					wasInTriggers = true;
					triggers = new WorkflowTriggerCollectionView(this);

					if (!previouslyWasInTriggers)
					{
						Parent.RegisterEditableChildObject(triggers);
					}
				}
				return triggers;
			}
		}
		WorkflowTriggerCollectionView triggers;

		public WorkflowTriggerCollectionIncludingRelatedView TriggersIncludingRelated
		{
			get
			{
				if (triggersIncludingRelated == null)
				{
					triggersIncludingRelated = new WorkflowTriggerCollectionIncludingRelatedView(this);

					Parent.RegisterEditableChildObject(triggersIncludingRelated);
				}

				return triggersIncludingRelated;
			}
		}
		WorkflowTriggerCollectionIncludingRelatedView triggersIncludingRelated;

		internal bool UniversalTriggersHaveBeenLoaded { get; set; }

		#endregion

		#region HasTasks / HasMilestones / HasWorkflowTriggers

		internal bool HasTasks()
		{
			foreach (ProcessTask item in this)
			{
				if (item.IsTask)
				{
					return true;
				}
			}
			return false;
		}

		internal bool HasMilestones()
		{
			foreach (ProcessTask item in this)
			{
				if (item.IsMilestone)
				{
					return true;
				}
			}
			return false;
		}

		internal bool HasWorkflowTriggers()
		{
			foreach (ProcessTask item in this)
			{
				if (item.IsWorkflowTrigger)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Default Values

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Messing with this method won't make it easier to understand")]
		internal static void SetCollectionDefaultsForNewChild(IWorkflowProviderCollection collection, ProcessTask task, bool defaultAssignedStaff)
		{
			ProcessTask previousTask = null;
			if (task.IsException)
			{
				previousTask = collection.OfType<ProcessTask>().FirstOrDefault(x => x.IsMilestone
					&& x.P9_SE_NKMilestoneEvent == task.P9_SE_NKMilestoneEvent
					&& x.P9_Description == task.P9_Description);
			}
			else if (task.IsTask)
			{
				previousTask = collection.OfType<ProcessTask>().Where(x => x.IsTask).MaxBySafe(x => x.P9_Sequence);
			}
			else if (task.IsMilestone)
			{
				previousTask = collection.OfType<ProcessTask>().Where(x => x.IsMilestone).MaxBySafe(x => x.P9_Sequence);
			}
			else if (task.P9_Type == Core.Constants.Workflow.WorkflowTriggerType)
			{
				previousTask = collection.OfType<ProcessTask>().Where(x => x.P9_Type == Core.Constants.Workflow.WorkflowTriggerType).MaxBySafe(x => x.P9_Sequence);
			}

			if (previousTask != null)
			{
				if (task.P9_Sequence.IsEmpty)
				{
					task.P9_Sequence = previousTask.P9_Sequence + 1;
				}

				if (defaultAssignedStaff)
				{
					if (task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						task.P9_GS_NKAssignedStaffMember = GetActiveAssignedStaffMemberForTask(previousTask);
					}

					if (task.P9_GG_AssignedGroup.IsEmpty)
					{
						task.P9_GG_AssignedGroup = GetActiveAssignedGroupForTask(previousTask);
					}
				}
			}
			else if (task.P9_Sequence.IsEmpty)
			{
				if (task.IsException)
				{
					var previousException = collection.OfType<ProcessTask>().Where(x => x.IsException).MaxBySafe(x => x.P9_Sequence);
					task.P9_Sequence = previousException != null ? previousException.P9_Sequence + 1 : 1;
				}
				else
				{
					task.P9_Sequence = 1;
				}
			}
		}

		void IProcessTaskInitialiser.SetDefaultParents(ProcessTask task)
		{
			SetParents(task);
			SetDefaultProcessHeader(task);
		}

		public void SetParents(ProcessTask task)
		{
			if (task.P9_ParentTableCode.IsEmpty && Parent != null)
			{
				task.P9_ParentTableCode = ParentTablePrefix;
			}
			if (task.P9_ParentID.IsEmpty && Parent != null)
			{
				task.P9_ParentID = Parent.PK;
			}
		}

		public void SetDefaultProcessHeader(ProcessTask task)
		{
			if (task.Parent != null && !(task is TemplateProcessTask) && !(task.Parent is ProcessTask) && task.IsTask)
			{
				var jobHeader = ProcessJobHeaderProvider.GetForParent(task.Parent, task.Factory);
				if (jobHeader != null && jobHeader.ProcessHeaders.Count == 1)
				{
					// We need to set an intermediate value since the getter finds the default ProcessHeader, but we need to ensure the FK is set on the row.
					task.P9_FH_ProcessHeader = ZGuid.Invalid;
					task.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;
				}
			}
		}

		static ZString GetActiveAssignedStaffMemberForTask(ProcessTask task)
		{
			GlbStaff staff = task.AssignedStaffMember;
			return staff != null && staff.GS_IsActive ? task.P9_GS_NKAssignedStaffMember : ZString.Empty;
		}

		static ZGuid GetActiveAssignedGroupForTask(ProcessTask task)
		{
			GlbGroup group = task.AssignedGroup;
			return group != null && group.GG_IsActive ? task.P9_GG_AssignedGroup : ZGuid.Empty;
		}

		protected override sealed BusinessObject AddNewCore()
		{
			var child = (ProcessTask)base.AddNewCore();
			this.SetDefaultsForNewTask(child, true);
			return child;
		}

		void IProcessTaskInitialiser.SetDefaultsForNewTaskCore(ProcessTask processTask, bool defaultAssignedStaff)
		{
			SetDefaultsForNewChildCore(this, processTask, defaultAssignedStaff);
		}

		protected internal virtual void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask processTask, bool defaultAssignedStaff)
		{
			SetCollectionDefaultsForNewChild(collection, processTask, defaultAssignedStaff);
			WorkflowInfo?.SetDefaultTriggerConditions(processTask, Parent);
		}

		string ParentTablePrefix
		{
			get { return Parent.TablePrefix; }
		}

		#endregion

		#region Contact / Address Support / Country Specific Support

		public virtual bool SupportsContactAndAddress
		{
			get { return false; }
		}

		public virtual ZString OriginCountry
		{
			get { return ZString.Empty; }
		}

		public virtual ZString DestinationCountry
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region RequiresReferenceCode / ReferenceCodeCaption

		public virtual bool RequiresReferenceCode
		{
			get { return false; }
		}

		public virtual string ReferenceCodeCaption
		{
			get { return Res.GetString("MasterFiles|ProcessTaskCollection|ReferenceCodeCaption", "Reference Code"); }
		}

		#endregion

		#region ConditionsMet

		public virtual bool IsCondition1Met(ZString conditionCode)
		{
			return false;
		}

		public bool IsCondition2MetCoreExposed(ZString conditionCode, ZString value)
		{
			return IsCondition2MetCore(conditionCode, value);
		}
		protected virtual bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return false;
		}

		public virtual bool HasNewConditionBeenMetSinceLastSave()
		{
			return false;
		}

		#endregion

		#region Relationship

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (Parent != null)
			{
				result.AddToFilter(ProcessTasksSchema.P9_ParentID, Parent.PK);
			}
			return result;
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			bool result = base.ElementCanBeAdded(bizO);
			if (result && IsLoading && AreTasksCompanySpecific && !(bizO is TemplateProcessTask))
			{
				ProcessTask task = (ProcessTask)bizO;
				result = !task.IsTask || task.P9_GC.IsEmpty || task.P9_GC == GlbCompany.CurrentCompany.PK || task.P9_ShareTasksForAllCompanies;
			}
			return result;
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return Parent != null && !Parent.IsInDatabase; }
		}

		public bool AreTasksCompanySpecific
		{
			get { return WorkflowInfo != null && WorkflowInfo.AreTasksCompanySpecific; }
		}

		IWorkflowProvider WorkflowProvider => (IWorkflowProvider)Parent;

		WorkflowDescriptor WorkflowInfo
		{
			get
			{
				var workflowProvider = WorkflowProvider;
				return workflowProvider != null ? WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) : null;
			}
		}

		#endregion

		#region Properties

		public bool AllTasksCancelled
		{
			get { return AllTasksWithStatus(ProcessTaskStatusCodeList.Codes.Cancelled); }
		}

		public bool AllTasksClosedOrCancelled
		{
			get { return AllTasksWithStatus(ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled); }
		}

		public bool AnyTaskIsAssigned
		{
			get
			{
				bool result = false;
				foreach (ProcessTask task in Tasks)
				{
					if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		bool AllTasksWithStatus(params string[] taskStatus)
		{
			bool result = Tasks.Count > 0;
			foreach (ProcessTask task in Tasks)
			{
				if (!Array.Exists(taskStatus, s => s == task.P9_Status))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		#endregion

		#region IWorkflowProvider Members

		Logs IStmALogProvider.Logs => ((IWorkflowProvider)Parent)?.Logs;
		BusinessObjectFactory IStmALogProvider.LogsFactory => ((IWorkflowProvider)Parent)?.LogsFactory;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForProcessTaskCollection(this, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return this; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowType; }
		}

		public ZString WorkflowType
		{
			get { return (Parent as IWorkflowProvider)?.WorkflowType ?? ZString.Empty; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return ((IWorkflowProviderCore)Parent).GetTemplateSelectionCriteria();
		}

		public ZGuid PK
		{
			get { return ZGuid.Invalid; }
		}

		#endregion

		#region IWorkflowProviderCollection

		IEnumerable<ProcessTask> IWorkflowProviderCollection.Items => this.Cast<ProcessTask>();

		IComparer IWorkflowProviderCollection.GetDefaultOrderComparer() => new ProcessTaskDefaultOrderComparer();

		public event EventHandler OnRebuild
		{
			add { }
			remove { }
		}

		#endregion

		#region CurrentTask

		public ProcessTask GetCurrentTask()
		{
			return Tasks.Cast<ProcessTask>()
				.Where(t => t.IsCurrent)
				.OrderBy(t => t.P9_Sequence)
				.FirstOrDefault();
		}

		public ProcessTask GetCurrentTask(IProcessHeader workflow)
		{
			return GetCurrentTasks(workflow).FirstOrDefault();
		}

		public IEnumerable<ProcessTask> GetCurrentTasks(IProcessHeader workflow)
		{
			return GetCurrentTasks(workflow, Tasks.Cast<ProcessTask>());
		}

		public static IEnumerable<ProcessTask> GetCurrentTasks(IProcessHeader workflow, IEnumerable<ProcessTask> tasks)
		{
			var groups = tasks
				.Where(t => t.P9_FH_ProcessHeader == workflow.PK)
				.GroupBy(t => t.P9_Sequence)
				.OrderBy(g => g.Key)
				.ToList();
			var group = groups.FirstOrDefault(g => g.Any(t => t.IsCurrent));

			return group == null || groups.Any(g => g.Key < group.Key && g.Any(t => !t.IsClosed))
				? Enumerable.Empty<ProcessTask>()
				: group.Where(t => t.IsCurrent);
		}

		#endregion

		#region Validation

		public BusinessObjectCollectionValidationCache<ProcessTask> ValidationCache => validationCache ?? (validationCache = ProcessTaskCollectionValidationCaching.GetValidationCache(this));

		BusinessObjectCollectionValidationCache<ProcessTask> validationCache;

		protected override bool RunPreSaveValidationCore()
		{
			using (ValidationCache.SetCacheForValidateAll())
			{
				return base.RunPreSaveValidationCore();
			}
		}

		#endregion

		#region Delete

		public override void RemoveAndDeleteAll()
		{
			base.RemoveAndDeleteAll();
			foreach (var link in Factory.Load<IProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, Parent.PK)))
			{
				link.Delete();
			}

			foreach (var header in Factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, Parent.PK)))
			{
				header.Delete();
			}
		}

		#endregion

		#region Collection Susbscriptions

		readonly List<WorkflowItemCollectionView> views = new List<WorkflowItemCollectionView>();

		internal void AddView(WorkflowItemCollectionView view)
		{
			views.Add(view);
		}

		#endregion

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				return;
			}

			((IRefreshProcessTaskCollection)this).RefreshProcessTaskCollection(true);
		}

		void IRefreshProcessTaskCollection.RefreshProcessTaskCollection(bool reLoadExistingRows)
		{
			var parent = Parent;
			if (parent != null && !parent.IsDeleted)
			{
				if (views.Any())
				{
					using (new DisposableList(views.Select(v => v.SuppressRebuild())))
					{
						using (parent.SuspendSettingHasChanges())
						{
							using (SuspendListChanged())
							{
								Reload(reLoadExistingRows: reLoadExistingRows, assumeRowsMissingFromQueryResultsAreDeleted: true);
								foreach (var collection in views.ToList())
								{
									collection.ForceRebuild();
								}
							}
						}
					}
				}

				RefreshBinding();
			}
		}

		#region Fetch Hints

		internal void AddFetchHintsForTemplateApplication()
		{
			if (!AreFetchHintsAdded)
			{
				foreach (ProcessTask item in this)
				{
					item.AddCompletionTriggerActionsFetchHint();
				}
				AreFetchHintsAdded = true;
			}
		}
		bool AreFetchHintsAdded;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class DummyProcessTaskCollection : ProcessTaskCollection
	{
		public DummyProcessTaskCollection(BusinessObject master)
			: base(master)
		{
		}

		public override bool SupportsContactAndAddress
		{
			get { return SupportsContactAndAddress_Override; }
		}

		public bool SupportsContactAndAddress_Override { get; set; }

		public void SetRequiresReferenceCode(bool value)
		{
			requiresReferenceCode = value;
		}

		public override bool RequiresReferenceCode
		{
			get { return requiresReferenceCode; }
		}
		bool requiresReferenceCode;

		public override string ReferenceCodeCaption
		{
			get { return "Dummy Reference Code Caption"; }
		}

		public override bool IsCondition1Met(ZString conditionCode)
		{
			return conditionCode == Parent.Z0_Code;
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return conditionCode == Parent.Z0_Description;
		}

		public override bool HasNewConditionBeenMetSinceLastSave()
		{
			return Parent.IsInDatabase && Parent.Z0_CodeInfo.HasChanges;
		}

		new DummyBusinessObject Parent
		{
			get { return (DummyBusinessObject)base.Parent; }
		}

		public new DummyProcessTask this[int index]
		{
			get { return (DummyProcessTask)Elements[index]; }
		}

		public new DummyProcessTask AddNew()
		{
			return (DummyProcessTask)base.AddNew();
		}

		public new void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
		}
	}
}

#endif
#endregion
