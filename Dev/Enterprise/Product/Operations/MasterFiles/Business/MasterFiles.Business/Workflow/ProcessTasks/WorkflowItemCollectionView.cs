using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[TestExcludeWorkflowProviderHasTestCase]
	[ZArchitecture.ComponentModel.ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.ProcessTasks)]
	public abstract class WorkflowItemCollectionView : BusinessObjectCollectionView<ProcessTask>, IWorkflowItemCollection, IBindingTrackingBusinessObjectCollection
	{
		public WorkflowItemCollectionView(ProcessTaskCollection collection)
			: base(collection)
		{
			SetReadOnlyIncludingChildren(collection.ReadOnly);
		}

		protected abstract bool IsTypeMatch(ProcessTask task);

		protected abstract TemplateEntityType TemplateEntityType { get; }

		TemplateEntityType IWorkflowItemCollection.TemplateEntityType => TemplateEntityType;

		IEnumerable<ProcessTask> IWorkflowProviderCollection.Items => this.Cast<ProcessTask>();

		public event EventHandler OnRebuild;

		#region Delay Rebuild

		sealed class RebuildDelayer : Rebuilder
		{
			internal RebuildDelayer(WorkflowItemCollectionView view)
				: base(view)
			{
				this.view = view;
			}

			readonly WorkflowItemCollectionView view;

			bool shouldRebuild;
			int delayDepth;

			protected override void RebuildCore()
			{
				shouldRebuild = true;
			}

			public IDisposable Increment()
			{
				return new DisposableAction(Decrement);
			}

			public void Decrement()
			{
				delayDepth--;
				if (delayDepth == 0)
				{
					view.rebuildDelayer = null;
					if (shouldRebuild)
					{
						view.Rebuild();
					}
				}
			}
		}

		RebuildDelayer rebuildDelayer;

		public IDisposable DelayRebuild()
		{
			if (rebuildDelayer == null)
			{
				rebuildDelayer = new RebuildDelayer(this);
			}
			return rebuildDelayer.Increment();
		}

		#endregion

		#region Sorting

		protected override Rebuilder GetRebuilder() => (Rebuilder)rebuildDelayer ?? new WorkflowItemCollectionViewRebuilder(this);

		protected class WorkflowItemCollectionViewRebuilder : Rebuilder
		{
			public WorkflowItemCollectionViewRebuilder(WorkflowItemCollectionView view)
				: base(view)
			{
				this.view = view;
			}
			readonly WorkflowItemCollectionView view;

			protected override void RebuildCore()
			{
				if (view.isBinding > 0)
				{
					ErrorReporter.ReportOnce("WorkflowItemCollectionViewRebuilder Binding Error", "Don't rebuild the view in the middle of the binding process, because the index can be wrong.");
				}
				base.RebuildCore();
				view.OnRebuild?.Invoke(view, EventArgs.Empty);
			}
		}

		protected virtual IComparer OverrideBaseComparer(IComparer baseComparer) => baseComparer;

		public IComparer GetDefaultOrderComparer() => OverrideBaseComparer(GetDefaultOrderComparerCore());

		protected virtual IComparer GetDefaultOrderComparerCore() => new ProcessTaskDefaultOrderComparer();

		protected override bool AllowSort
		{
			get { return false; }
		}

		#endregion

		#region Default Values

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			// Override is this, unlike ProcessTaskCollection which overrides AddNewCore, because overriding AddNewCore breaks.
			// It  means some properties get set in the wrong order, and then many unit tests fail.
			var child = base.CreateBusinessObjectFromRow(row);
			using (child.SuspendSettingHasChanges())
			{
				AddNewTaskCore((ProcessTask)child);
			}
			return child;
		}

		protected virtual void AddNewTaskCore(ProcessTask child)
		{
			this.SetDefaultsForNewTask(child, WorkflowDataRegistry.Instance.TaskAssignmentAutoAssignStaff.Value);
			WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(Factory).ProcessTaskManualChangeLog(child, WorkflowItemsManualChangeLogService.WorkflowItemActionType.Added);
		}

		#endregion

		#region CreateItemsFromTemplate

		IEnumerable<CreateItemsFromTemplateResult> IWorkflowTemplateApplicator.ApplyTemplates(IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters)
		{
			return CreateItemsFromAllTemplates(templates, parameters);
		}

		public IEnumerable<CreateItemsFromTemplateResult> CreateItemsFromAllTemplates(IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters = null)
		{
			return CreateItemsFromAllTemplates((IWorkflowProvider)WorkflowItems.Parent, templates, parameters);
		}

		public ApplyWorkflowTemplateResult CreateItemsFromTemplate(IEnumerable<ProcessTaskTemplate> templates = null)
		{
			return new ApplyWorkflowTemplateResult.Builder(this).Add(CreateItemsFromAllTemplates(templates)).Build();
		}

		IEnumerable<CreateItemsFromTemplateResult> CreateItemsFromAllTemplates(IWorkflowProvider workflowProvider, IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters = null)
		{
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, workflowProvider.PK);
			var templatesToUse = templates ?? new ProcessTaskTemplate.Loader(Factory).FindMatches(workflowProvider);

			foreach (var template in templatesToUse)
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, template.PK);
			}

			var shouldPreventFallback = false;
			var resultSet = new List<CreateItemsFromTemplateResult>();
			var isFirstTemplateApplication = (parameters != null && parameters.AlwaysApplyTemplatesEvenWhenExistingItemsPresent)
											|| !HasExistingItemsBlockingTemplateApplication
											|| workflowProvider.WorkflowItems.HasNewConditionBeenMetSinceLastSave();

			foreach (var template in new TemplateFallbackIterator(templatesToUse, TemplateEntityType, (t, c) => shouldPreventFallback))
			{
				if (template.IsApplicableToJob)
				{
					var result = CreateItemsFromTemplate(template.Template, parameters, isFirstTemplateApplication);
					shouldPreventFallback = result.ShouldPreventFallback;
					resultSet.Add(result);
				}
				else
				{
					break;
				}
			}

			if (isFirstTemplateApplication)
			{
				SetDefaultsIfApplicable();
			}

			return resultSet;
		}

		protected virtual void SetDefaultsIfApplicable()
		{
		}

		public ICollection<ProcessTask> GetItemsToCreateFromTemplate()
		{
			foreach (var template in new ProcessTaskTemplate.Loader(Factory).FindMatches((IWorkflowProvider)WorkflowItems.Parent))
			{
				var items = GetItemsToCreateFromTemplate(template);
				if (items.Count > 0)
				{
					return items;
				}
			}

			return Array.Empty<ProcessTask>();
		}

		internal CreateItemsFromTemplateResult CreateItemsFromTemplate(ProcessTaskTemplate template, TemplateApplicationParameters parameters, bool isFirstTemplateApplication)
		{
			return CreateItemsFromTemplateCore(template, parameters, isFirstTemplateApplication);
		}

#if DEBUG

		public void CreateItemsFromTemplate_ForTest(ProcessTaskTemplate template)
		{
			foreach (var v in CreateItemsFromTemplateCore(template, null, !HasExistingItemsBlockingTemplateApplication).CreatedItems)
			{
				v.Dispose();
			}
		}

#endif

		protected virtual CreateItemsFromTemplateResult CreateItemsFromTemplateCore(ProcessTaskTemplate template, TemplateApplicationParameters parameters, bool isFirstTemplateApplication)
		{
			var templateTasks = template.WorkflowItems.Cast<ProcessTask>()
							.Where(IsTypeMatch)
							.ToArray();

			var templateApplications = FindTemplateApplications(template, templateTasks, parameters);
			var toDoList = GetItemsToCreateFromTemplate(templateApplications, t => t.ShouldCloneTask ? t.TemplateTask : null);
			var hasTasksFromTemplate = toDoList.Any();

			if (toDoList.Any(c => c.ShouldCloneTask) && ShouldReloadToReduceRaceConditionWindow(hasTasksFromTemplate))
			{
				ReloadCollection();
				templateApplications = FindTemplateApplications(template, templateTasks, parameters);
				toDoList = GetItemsToCreateFromTemplate(templateApplications, t => t.ShouldCloneTask ? t.TemplateTask : null);
				hasTasksFromTemplate = toDoList.Any();
			}

			if (hasTasksFromTemplate)
			{
				var createdList = CreateItemsFromTemplate(toDoList, isFirstTemplateApplication, parameters);
				if (createdList.Any())
				{
					return new CreateItemsFromTemplateResult(createdList);
				}
				else
				{
					return new CreateItemsFromTemplateResult(shouldPreventFallback: true);
				}
			}
			else
			{
				return new CreateItemsFromTemplateResult(shouldPreventFallback: false);
			}
		}

		protected virtual bool HasExistingItemsBlockingTemplateApplication => Count > 0;

		bool ShouldReloadToReduceRaceConditionWindow(bool hasTemplatesToAdd)
		{
			if (!hasTemplatesToAdd || WorkflowItems.HasReloadedFromDb)
			{
				return false;
			}

			var raceConditionHandlerIsEnabled = WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.Value != TemplateApplicationRaceConditionHandlerOptions.Codes.Off;
			if (!raceConditionHandlerIsEnabled)
			{
				return false;
			}

			var config = TemplateApplicationRaceHandlingConfig.GetConfig();
			return !(config.TryHandleProcessTaskConflictsAutomatically || (config.TryHandleProcessTaskConflictsWhenServiceTasksOnlySelected && BusinessObjectFactory.IsSavingTogether));
		}

		/// <summary>
		/// It is possible that multiple BusinessObjects will wrap around the same row (Even within the same factory). 
		/// If both of these implement workflow then it possible for tasks and miletones to be duplicated without this reload.
		/// </summary>
		void ReloadCollection()
		{
			if (WorkflowItems.Parent.IsInDatabase)
			{
				WorkflowItems.Reload(reLoadExistingRows: false);
			}
			else
			{
				WorkflowItems.ReloadFromLocalCache();
				WorkflowItems.HasReloadedFromDb = true;
			}
		}

		protected abstract IEqualityComparer<ProcessTask> GetTemplateApplicationComparer();

		IEnumerable<TemplateItemApplication> FindTemplateApplications(ProcessTaskTemplate template, IEnumerable<ProcessTask> templateTasks, TemplateApplicationParameters parameters)
		{
			var results = new List<TemplateItemApplication>();
			var itemsInView = this.Cast<ProcessTask>().ToArray();

			ErrorReportForDeletedItems(itemsInView, GetType().Name);

			var groups = new DefaultTaskToTemplateComparer()
						.GroupMerge(templateTasks, itemsInView)
						.GroupOrphans(GetTemplateApplicationComparer());

			var factory = template.Factory;
			groups.SelectMany(g => g.Enumerate()).Where(t => t.IsMilestoneOrWorkflowTrigger).ForEach(t => factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, t.PK));

			foreach (var group in groups)
			{
				var templateItems = group[0];
				var createdItems = group[1];

				if (createdItems.Any() && templateItems.Any())
				{
					if (parameters?.TriggerApplicationType == TriggerApplicationType.AlwaysApply)
					{
						CheckForDeletedItem(templateItems);

						// We will create tasks even if they already exist.
						// RepeatApplicationParameters will be used to bump sequence numbers on the tasks and control deduplication/merge logic
						var maxExistingSequenceNumber = itemsInView.Where(x => !x.IsCompletionStatement).Max(x => x.P9_Sequence);
						results.Add(new TemplateItemApplication(template, templateItems, repeatApplicationParams: new TemplateItemApplication.RepeatApplicationParameters(maxExistingSequenceNumber)));
					}
					else
					{
						foreach (var itemThatAlreadyExists in createdItems)
						{
							WorkflowItems.AddFetchHintsForTemplateApplication();
							var created = Lazy.Create(() => itemThatAlreadyExists.ProcessTaskNotificationsWithoutMultipleIndexes);
							foreach (var templateItem in templateItems)
							{
								if (itemThatAlreadyExists.IsDeleted)
								{
									// The fact that this is already deleted after being looked at by the comparer seems insane to me.
									ErrorReporter.ReportOnce("ItemOnJobDeleted", FormattableString.Invariant($"How did this happen? Here is the template item PK: {templateItem.PK}"));
								}

								var notificationsToCreate = Lazy.Create(() => templateItem.ProcessTaskNotificationsWithoutMultipleIndexes.Except(created.Value, new TemplateNotificationComparer()));
								// The template has already applied, but we might need to make Completion Trigger Actions
								// All the templates that may have applied need to yield in order to block template application fallback
								results.Add(new TemplateItemApplication(template, itemThatAlreadyExists, () => notificationsToCreate.Value, shouldCloneTask: false));
							}
						}
					}
				}
				else if (templateItems.Any())
				{
					CheckForDeletedItem(templateItems);
					// This is the first time the template is creating this task.
					results.Add(new TemplateItemApplication(template, templateItems));
				}
				else
				{
					// This group is a task that doesn't match this template.
				}
			}

			void CheckForDeletedItem(IReadOnlyList<ProcessTask> templateItems)
			{
				if (templateItems.First().IsDeleted)
				{
					// I don't think this will ever happen, but I'm adding this check to be thorough.
					ErrorReporter.ReportOnce("TemplateItemToBeCreatedWasDeleted", FormattableString.Invariant($"How did this happen? Here is the template PK: {template.PK}"));
				}
			}

			return results;
		}

		static void ErrorReportForDeletedItems(ProcessTask[] itemsInView, string viewName)
		{
			foreach (var item in itemsInView)
			{
				if (item.IsDeleted)
				{
					ErrorReporter.ReportOnce("ViewContainsDeletedItems", FormattableString.Invariant($"The view {viewName} contains deleted items. That shouldn't be possible due to data refresh."));
				}
			}
		}

		public ICollection<ProcessTask> GetItemsToCreateFromTemplate(ProcessTaskTemplate template)
		{
			return GetItemsToCreateFromTemplate(template.WorkflowItems.Cast<ProcessTask>().Where(IsTypeMatch), t => t);
		}

		ICollection<T> GetItemsToCreateFromTemplate<T>(IEnumerable<T> collection, Func<T, ProcessTask> selector)
		{
			var templateConditionEvaluator = ObjectFactory.New<IWorkflowTemplateConditionEvaluator>();

			List<T> result = new List<T>();
			foreach (var item in collection)
			{
				var task = selector(item);

				if (task == null || templateConditionEvaluator.AreConditionsMetForTemplateApplication((IWorkflowProvider)WorkflowItems.Parent, task))
				{
					result.Add(item);
				}
			}
			return result;
		}

		public ICollection<TemplateItemApplication> CreateItemsFromTemplate(ICollection<TemplateItemApplication> processTasksFromTemplate, bool isFirstTemplateApplication, TemplateApplicationParameters parameters)
		{
			var result = new List<TemplateItemApplication>(processTasksFromTemplate.Count);

			var size = WorkflowDataRegistry.Instance.MaximumNumberOfWorkflowItemsInTemplateApplication.Value;
			if (processTasksFromTemplate.Count > size)
			{
				ErrorReporter.ReportOnce("c40a34e7-79df-4c3c-9d02-d9fcaec5f968", FormattableString.Invariant($@"Something terrible has happened and we are now creating {processTasksFromTemplate.Count} items when {size} is the max.
				Type: {GetType().FullName}
				Tempate names: {string.Join(" ", processTasksFromTemplate.Select(s => s.Template.P0_Description).Distinct())}
				SampleUDFCondition: {processTasksFromTemplate.FirstOrDefault(f => f.TemplateTask.TemplateConditions.TemplateCondition2 == "UDF")?.TemplateTask.TemplateConditions.TemplateCondition2Value}
				Number of UDF conditions: {processTasksFromTemplate.Count(f => f.TemplateTask.TemplateConditions.TemplateCondition2 == "UDF")}"));
			}

			foreach (var task in processTasksFromTemplate.OrderBy(t => t.TemplateTask?.P9_Sequence ?? int.MaxValue))
			{
				if (isFirstTemplateApplication || ProcessTasksLookups.IsMacroCondition(task.TemplateTask.P9_Condition2))
				{
					if (TryCreateItemFromTemplate(task, parameters))
					{
						task.TrackDelete();
#if DEBUG
						OnItemCreated.Value?.Invoke(task);
#endif
						result.Add(task);
					}
				}
			}

			return result;
		}

		bool TryCreateItemFromTemplate(TemplateItemApplication itemTemplate, TemplateApplicationParameters parameters)
		{
			var parent = WorkflowItems.Parent;
			var templateTask = itemTemplate.TemplateTask;
			var descriptor = templateTask.WorkflowDescriptorCore; // It's important to use the template descriptor rather than the line items descriptor here because there is logic on this descriptor that applies to line items.

			if (descriptor == null)
			{
				ErrorReporter.ReportOnce("WorkflowItemCollectionView_WorkflowDescriptorNull", $"Workflow Descriptor cannot be null. [WorkflowDescriptorCore: {templateTask.WorkflowDescriptorCore}, WorkflowType: {templateTask.WorkflowType}, TemplateName: {itemTemplate.Template.P0_Name}, TemplateType: {itemTemplate.Template.P0_ProcessType}]");
				return false;
			}

			var defaultable = new TemplateApplicationDefaultableDateChecker(!itemTemplate.ShouldCloneTask, templateTask, parent, descriptor);

			if (templateTask.IsMilestoneOrWorkflowTrigger)
			{
				if (itemTemplate.RepeatApplicationParams != null)
				{
					var comparer = new TriggerToTemplateComparer();
					var matchesExistingMilestoneOrTrigger = this.Cast<ProcessTask>().Where(x => x.IsMilestoneOrWorkflowTrigger).ToArray().Any((x) => comparer.Equals(templateTask, x));

					if (matchesExistingMilestoneOrTrigger)
					{
						return false;
					}
				}

				var item = CreateItemFromTemplateCore(itemTemplate, parameters);
				if (!string.Equals(item.P9_TriggerConditionValue, defaultable.TriggerConditionValue, StringComparison.OrdinalIgnoreCase))
				{
					ErrorReporter.ReportOnce("OverridingTheTemplateConditionValue", $@"The template condition value on the simulated item is different to the one on the created item (And shouldn't be)
Simulated trigger condition value: {defaultable.TriggerConditionValue}
Actual trigger condition value: {item.P9_TriggerConditionValue}

Simulated: {defaultable.TriggerEventCode}|{defaultable.TriggerFieldName}|{defaultable.TriggerCondition}|{defaultable.TriggerConditionValue}
Created P9: {item.P9_SE_NKMilestoneEvent}|{item.P9_TriggerField}|{item.P9_Description}|{item.P9_TriggerCondition}|{item.P9_TriggerConditionValue}
Created: {item.TriggerConditions.TriggerEventCode}|{item.TriggerConditions.TriggerFieldName}|{item.TriggerConditions.TriggerEventDescription}|{item.TriggerConditions.TriggerCondition}|{item.TriggerConditions.TriggerConditionValue}");
				}
			}
			else
			{
				CreateItemFromTemplateCore(itemTemplate, parameters);
			}

			return true;
		}

		protected virtual ProcessTask CreateItemFromTemplateCore(TemplateItemApplication itemTemplate, TemplateApplicationParameters parameters)
		{
			var item = itemTemplate.ShouldCloneTask ? TemplateProcessTaskCopier.Clone(itemTemplate.TemplateTask, TypeOfElements) : itemTemplate.TemplateTask;

			itemTemplate.CreatedItem = item;

			if (itemTemplate.RepeatApplicationParams != null)
			{
				//During repeat applications, we use IsTemplateReapplication to trigger a different concurrency merge check
				//Sequence number must be incremented for updated concurrency check to be effective
				item.IsTemplateReapplication = true;
				item.P9_Sequence = itemTemplate.TemplateTask.P9_Sequence + 100 + itemTemplate.RepeatApplicationParams.HighestExistingSequenceNumber;
			}

			using (item.GetValidationSuspender())
			{
				if (itemTemplate.ShouldCloneTask)
				{
					using (item.SuspendUpdatingIterationPivots())
					{
						item.P9_ParentTemplateID = itemTemplate.TemplateTask.PK;
						WorkflowItems.SetDefaultsForNewTask(item, false);
						Add(item);

						if (item.IsTask)
						{
							ProcessTaskToProcessHeaderLinker.SetBestMatchingProcessHeaderOnProcessTaskForTemplateApplication(Factory, (IWorkflowProvider)WorkflowItems.Parent, itemTemplate.Template, itemTemplate.TemplateTask, item, parameters);
						}

						item.RefreshWorkflowType();
					}
				}

				foreach (ProcessTaskNotification itemNotificationTemplate in itemTemplate.NotificationsToCreate)
				{
					var notification = (ProcessTaskNotification)itemNotificationTemplate.Clone();

					using (notification.SuspendSettingHasChanges())
					{
						if (!WorkflowTriggerActionTypeConstants.IsSetField(notification.PQ_TriggerType))
						{
							notification.PQ_EmailText = ZString.Empty;
						}
						notification.PQ_P9 = item.PK;
						notification.PQ_SourceTemplateNotification = itemNotificationTemplate.PK;
						notification.RefreshReadOnlyForAllProperties();
					}
				}
				if (BusinessObjectFactory.IsSavingTogether)
				{
					item.IsCreatedFromTemplateDuringSaving = true;
				}
				return item;
			}
		}

		#endregion

		#region IWorkflowProvider

		Logs IStmALogProvider.Logs => ((IStmALogProvider)WorkflowItems).Logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory => ((IStmALogProvider)WorkflowItems).LogsFactory;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		protected virtual ProcessTaskCollection WorkflowItems
		{
			get { return (ProcessTaskCollection)CollectionToFilter; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null && WorkflowItems?.Parent is IWorkflowProviderCore parent)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(parent, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowItems.WorkflowType; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return ((IWorkflowProvider)WorkflowItems.Parent).GetTemplateSelectionCriteria();
		}

		public ZGuid PK
		{
			get { return ZGuid.Invalid; }
		}

		#endregion

		public IDisposable Binding()
		{
			isBinding++;
			return new DisposableAction(() => isBinding--);
		}
		int isBinding;

		// Only need to override AllowNewCore, and not AllowRemoveCore.
		// The reason is that ProcessTasks have their own checks for whether they are deletable, so we don't need to check it at this level
		protected override bool AllowNewCore => base.AllowNewCore && ProcessTaskSecurityMan.IsAllowed(WorkflowItems, AddSecurityCode);

		protected abstract string AddSecurityCode { get; }

		#region PAVE

		void IProcessTaskInitialiser.SetDefaultsForNewTaskCore(ProcessTask task, bool defaultAssignedStaff)
		{
			WorkflowItems.SetDefaultsForNewChildCore(this, task, defaultAssignedStaff);
		}

		void IProcessTaskInitialiser.SetDefaultParents(ProcessTask task)
		{
			WorkflowItems.SetParents(task);
			SetDefaultProcessHeader(task);
		}

		protected virtual void SetDefaultProcessHeader(ProcessTask task)
		{
			WorkflowItems.SetDefaultProcessHeader(task);
		}

		#endregion

		#region Implementation

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			ProcessTask task = (ProcessTask)element;
			return IsTypeMatch(task);
		}

		#endregion

#if DEBUG
		public readonly static Overridable<Action<TemplateItemApplication>> OnItemCreated = new Overridable<Action<TemplateItemApplication>>();

		public void CreateItemsFromTemplate_ForTest(IList<TemplateItemApplication> processTasksFromTemplate)
		{
			bool isFirstApplication = !HasExistingItemsBlockingTemplateApplication;
			foreach (var r in CreateItemsFromTemplate(processTasksFromTemplate, isFirstApplication, TemplateApplicationParameters.Default))
			{
				r.Dispose();
			}
		}
#endif
	}
}
