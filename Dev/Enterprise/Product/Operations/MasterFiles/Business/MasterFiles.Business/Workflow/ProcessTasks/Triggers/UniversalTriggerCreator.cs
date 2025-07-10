using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class UniversalTriggerCreator
	{
		public static IEnumerable<ProcessTask> CreateAndDeleteNonPersistedUniversalTriggers(this IWorkflowProvider workflowProvider, IEnumerable<ProcessTask> currentItems, TemplateApplicationParameters parameters = null)
		{
			if (!WorkflowDataRegistry.Instance.EnableUniversalTemplates.Value)
			{
				return Enumerable.Empty<ProcessTask>();
			}

			var collection = workflowProvider.WorkflowItems;
			var parent = collection.Parent;
			var parentWorkflowProvider = parent as IWorkflowProvider;

			collection.UniversalTriggersHaveBeenLoaded = true;

			if (parent == null || parentWorkflowProvider == null || parent.IsDeleted || parent is ProcessTaskTemplate)
			{
				return Enumerable.Empty<ProcessTask>();
			}

			var universalTemplates = parameters?.SpecificTemplatesToApply
				?? new ProcessTaskTemplate.Loader(parent.Factory).FindMatches(parentWorkflowProvider, includeOnlyUniversalTemplates: true);

			if (parameters != null && parameters.JobAttributesMayHaveChangedSinceLastTemplateApplication)
			{
				ObjectFactory.Get<IUserDefinedConditionEvaluator>().ClearCache((IBusiness)workflowProvider);
			}

			var diff = GetTemplateTriggerDiff(collection, currentItems, universalTemplates);

			using (parent.SuspendSettingHasChangesIncludingChildren())
			{
				return CreateAndDelete(workflowProvider, diff);
			}
		}

		internal static bool DisallowUniversalTriggersFallbackIfEmpty(ProcessTaskTemplate template, IWorkflowItemCollection items)
		{
			throw new InvalidOperationException("It should never be possible to use EFB Trigger Fallback Method for universal templates");
		}

		static ProcessTask CreateNonPersistedRepresentationOfTemplateTrigger(ITemplateTrigger templateTrigger, IWorkflowProvider provider)
		{
			var parent = (IWorkflowProvider)provider.WorkflowItems.Parent;
			ProcessTask ghostTrigger;
			using (WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(parent.LogsFactory).SuppressWorkflowChangeLog())
			{
				 ghostTrigger = (ProcessTask)((ILegacyBusinessObjectCollectionInternals)parent.WorkflowItems.Triggers).CreateNewBusinessObject();
			}

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(ghostTrigger.Factory))
			using (ghostTrigger.SuspendSettingHasChanges())
			{
				ghostTrigger.ReadOnly = true;
				ghostTrigger.IsWorkflowTrigger = true;
				ghostTrigger.IsNonPersistedRepresentationOfTemplateTrigger = true;
				ghostTrigger.ShouldTriggerOnEstimateEvents = templateTrigger.ShouldTriggerOnEstimateEvents;
				ghostTrigger.P9_DelayDurationSeconds = templateTrigger.DelayDurationSeconds;
				ghostTrigger.P9_SuppressDuplicates = templateTrigger.SuppressDuplicates;
				ghostTrigger.P9_ParentTemplateID = templateTrigger.Identifier;
				ghostTrigger.P9_ParentID = parent.PK;
				ghostTrigger.P9_ParentTableCode = ((BusinessObject)parent).TablePrefix;

				ghostTrigger.P9_Description = templateTrigger.Description;
				ghostTrigger.P9_Sequence = templateTrigger.Sequence;

				ghostTrigger.TriggerConditions.TriggerEventCode = templateTrigger.TriggerConditions_ForBinding.TriggerEventCode;
				ghostTrigger.TriggerConditions.TriggerFieldName = templateTrigger.TriggerConditions_ForBinding.TriggerFieldName;
				ghostTrigger.TriggerConditions.TriggerCondition = templateTrigger.TriggerConditions_ForBinding.TriggerCondition;
				ghostTrigger.TriggerConditions.TriggerConditionValue = templateTrigger.TriggerConditions_ForBinding.TriggerConditionValue;
				ghostTrigger.TriggerConditions.TriggerContextCode = templateTrigger.TriggerContextCode;

				ghostTrigger.P9_RN_NKOriginCountry = templateTrigger.OriginCountryCode;
				ghostTrigger.P9_RN_NKDestinationCountry = templateTrigger.DestinationCountryCode;

				ghostTrigger.P9_RespondToCascadedEvents = templateTrigger.Cascading;
				ghostTrigger.P9_CascadedEventsContext = templateTrigger.CascadingContext;

				var jobTrigger = templateTrigger.GetOrCreateJobVersionOfTrigger((IBusiness)parent, createIfNotFound: false);

				if (jobTrigger != null)
				{
					((IProcessTaskInternals)ghostTrigger).SetActualDateWithoutFiringWorkflow(jobTrigger.LastFiredTime);
				}

				foreach (var triggerAction in templateTrigger.CompletionTriggerActionsCollection())
				{
					var cloneArgs = new BusinessObjectCloneArgs(new[] { ProcessTaskNotificationSchema.Constants.PQ_P9T_Trigger });
					var cloneTriggerAction = (ProcessTaskNotification)triggerAction.Clone(cloneArgs);
					using (cloneTriggerAction.SuspendSettingHasChanges())
					{
						cloneTriggerAction.ReadOnly = true;
						cloneTriggerAction.PQ_P9 = ghostTrigger.PK;
					}
				}

				ghostTrigger.CompletionTriggerActionsCollection().SetReadOnlyIncludingChildren(true);

				return ghostTrigger;
			}
		}

		static IEnumerable<ProcessTask> CreateAndDelete(IWorkflowProvider parent, TemplateTriggerSet set)
		{
			var trueParent = parent.WorkflowItems.Parent;
			foreach (var trigger in set.TemplateTriggersWithoutTasks)
			{
				trigger.Factory.AddFetchHint(ProcessJobTriggerLinkSchema.Instance, trigger.GetJobVersionOfTriggerQuery(trueParent));
				trigger.Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9T_Trigger, trigger.Identifier);
			}

			var tasks = new List<ProcessTask>(set.TemplateTriggersWithoutTasks.Count);
			foreach (var task in set.TasksWithoutTemplateTriggers)
			{
				task.P9_ParentTemplateID = ZGuid.Empty; // Lets us delete non-persistent triggers.
				task.Delete();
			}

			foreach (var trigger in set.TemplateTriggersWithoutTasks)
			{
				tasks.Add(CreateNonPersistedRepresentationOfTemplateTrigger(trigger, parent));
			}

			return tasks;
		}

		static TemplateTriggerSet GetTemplateTriggerDiff(ProcessTaskCollection collection, IEnumerable<ProcessTask> existingItems, IEnumerable<ProcessTaskTemplate> universalTemplates)
		{
			var triggersToAdd = new List<ITemplateTrigger>();
			var triggersToRemove = new List<ProcessTask>();

			var templatesAndTheirApplicability = new TemplateFallbackIterator(universalTemplates, TemplateEntityType.Triggers, DisallowUniversalTriggersFallbackIfEmpty).ToArray();

			if (templatesAndTheirApplicability.Length > 1)
			{
				foreach (var iteratorResult in templatesAndTheirApplicability)
				{
					iteratorResult.Template.Factory.AddFetchHint(ProcessTemplateTriggerSchema.Instance, new ZQuery(ProcessTemplateTriggerSchema.P9T_P0_Template, iteratorResult.Template.PK));
				}
			}

			var templateConditionEvaluator = ObjectFactory.New<IWorkflowTemplateConditionEvaluator>();
			foreach (var iteratorResult in templatesAndTheirApplicability)
			{
				var itemSet = iteratorResult.Template.TemplateTriggers.Cast<ITemplateTrigger>().GroupJoin(existingItems, tmp => tmp.Identifier, t => t.P9_ParentTemplateID, (tmp, tasks) => new { Template = tmp, Tasks = tasks });

				foreach (var item in itemSet)
				{
					var templateTrigger = item.Template;
					var shouldBeGhostedOnJob = iteratorResult.IsApplicableToJob && templateTrigger.IsActive && templateConditionEvaluator.AreConditionsMetForTemplateApplication((IWorkflowProvider)collection.Parent, templateTrigger);
					var exists = false;

					foreach (var existingItem in item.Tasks)
					{
						exists = true;
						if (!shouldBeGhostedOnJob)
						{
							triggersToRemove.Add(existingItem);
						}
					}

					if (!exists && shouldBeGhostedOnJob)
					{
						triggersToAdd.Add(item.Template);
					}
				}
			}

			triggersToRemove.AddRange(
				from ProcessTask t in collection.Triggers
				where t.IsNonPersistedRepresentationOfTemplateTrigger
				where !templatesAndTheirApplicability.Any(x => x.Template.PK == t.SourceTemplatePK)
				select t
				);

			return new TemplateTriggerSet(triggersToAdd, triggersToRemove);
		}

		class TemplateTriggerSet
		{
			internal TemplateTriggerSet(IList<ITemplateTrigger> triggersToAdd, IList<ProcessTask> triggersToRemove)
			{
				TemplateTriggersWithoutTasks = triggersToAdd;
				TasksWithoutTemplateTriggers = triggersToRemove;
			}

			public IList<ITemplateTrigger> TemplateTriggersWithoutTasks { get; }
			public IList<ProcessTask> TasksWithoutTemplateTriggers { get; }
		}
	}
}
