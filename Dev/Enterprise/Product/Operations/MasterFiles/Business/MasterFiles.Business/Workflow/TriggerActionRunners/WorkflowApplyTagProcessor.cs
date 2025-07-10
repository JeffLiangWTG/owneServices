using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowApplyTagProcessor : IProcessor
	{
		ProcessTaskNotification Action { get; }
		BusinessObject WorkflowProvider { get; }

		public WorkflowApplyTagProcessor(ProcessTaskNotification action, BusinessObject eventSource)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(eventSource, nameof(eventSource));

			Action = action;

			if (eventSource is ProcessTask task)
			{
				WorkflowProvider = task.ParentBusinessObject;
			}
			else
			{
				WorkflowProvider = eventSource;
			}

			WorkflowProvider.Factory.SetContext(BufferManagementBusinessContext.TAGTriggerAction);
		}

		public void Process(INotifications notifications, CancellationToken token)
		{
			var fieldNameWithoutPrefix = Action.PQ_FieldNameTrimmed;

			var tagMagnitude = Action.RelatedEntity;
			var workflows = new List<IProcessHeader>();
			if (!string.IsNullOrEmpty(fieldNameWithoutPrefix))
			{
				workflows = GetMatchingWorkflows(notifications, fieldNameWithoutPrefix).ToList();

				if (!workflows.Any())
				{
					notifications.AddWarning(Res.GetString("C0510B2D-3375-4289-A370-F295F88FE304", "No matching workflows found."));
				}
			}
			else
			{
				var jobHeader = ProcessJobHeaderProvider.GetForParent(WorkflowProvider as IWorkflowProvider, Action.Factory, false);
				if (jobHeader != null)
				{
					workflows.Add(jobHeader);
				}
			}

			AddTagIfApplicable(notifications, tagMagnitude, workflows.ToArray());
		}

		IEnumerable<IProcessHeader> GetMatchingWorkflows(INotifications notifications, ZString fieldNameWithoutPrefix)
		{
			var result = new List<IProcessHeader>();
			var types = RootTypesHelper.GetDataFieldsOnlyRootTypes(((IRootTypeProvider)Action).RootTypes);
			if (types.Length > 0)
			{
				var bizo = WorkflowProvider;
				var validation = new WorkflowMacroApplyTagValidation(Action);
				var propertyInfo = new WorkflowMacroEvaluator(notifications, validation).GetNextPropertyInfo(
					bizo.GetType(),
					fieldNameWithoutPrefix,
					out string subPath);

				if (propertyInfo != null)
				{
					var actionRoots = ((IDynamicRootProvider)Action).AugmentedRoots(bizo);
					var nextBizo = (IBusiness)WorkflowMacroEvaluator.EvaluatePropertyInfo(propertyInfo, bizo, string.Empty);
					if (nextBizo is IProcessHeaderCollection workflows)
					{
						var collectionReader = MacroHelper.GetCollectionReader(workflows);

						if (collectionReader != null)
						{
							foreach (var element in collectionReader)
							{
								var businessObjects = actionRoots != null
									? new[] { element }.Concat(actionRoots).ToArray()
									: new[] { element };

								if (TryMatchWorkflow(notifications, actionRoots, validation, subPath, businessObjects))
								{
									result.Add(element as IProcessHeader);
								}
							}
						}
					}
				}
			}

			return result;
		}

		bool TryMatchWorkflow(INotifications notifications, BusinessObject[] actionRoots, WorkflowMacroApplyTagValidation validation, string subPath, BusinessObject[] businessObjects)
		{
			_ = new MacroClauseProcessor(null, validation).ProcessPropertyAndValue(subPath, out string nextPath, out string expression);

			if ((!string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(subPath)) && MacroHelper.MatchesFilter(businessObjects, expression, notifications))
			{
				if (!string.IsNullOrEmpty(nextPath))
				{
					return TryMatchWorkflow(notifications, actionRoots, validation, nextPath, businessObjects);
				}

				return true;
			}

			return false;
		}

		bool AddTagIfApplicable(INotifications notifications, ITagMagnitude tagMagnitude, params IProcessHeader[] processHeaders)
		{
			ITagOperationResult result = null;
			foreach (var processHeader in processHeaders)
			{
				if (!processHeader.IsTagApplied(tagMagnitude))
				{
					if (!tagMagnitude.TagDefinition.TGD_IsExclusive)
					{
						result = AddTagToProcessHeader(notifications, tagMagnitude, processHeader);
					}
					else
					{
						var tagLinksWithSameTagDefinition = processHeader.TagLinks.Where(x => x.TagDefinitionPk == tagMagnitude.TagDefinition.PK).OrderBy(y => y.TagMagnitude.TGM_RuleRunSequence);
						if (tagLinksWithSameTagDefinition.Any())
						{
							var mostExclusiveExistingTagLink = tagLinksWithSameTagDefinition.First();
							var existingTagCode = mostExclusiveExistingTagLink.TagMagnitude.TGM_Code;
							var newTagCode = tagMagnitude.TGM_Code;
							foreach (var existingTagLink in tagLinksWithSameTagDefinition)
							{
								processHeader.RemoveTag(existingTagLink.TagMagnitude);
							}
							result = processHeader.AddTag(tagMagnitude);
							notifications.Add(new InfoNotification(Res.GetString("EECCC2FA-EE8D-4B19-B137-EA411FABBD3A", "Tag: '{0}' applied, Tag: '{1}' removed from Workflow: '{2}' successfully.",
								newTagCode,
								existingTagCode,
								processHeader.FH_CompletionStatement.TrimEnd('.'))));
						}
						else
						{
							result = AddTagToProcessHeader(notifications, tagMagnitude, processHeader);
						}
					}
				}
			}

			return result?.WasSuccessful ?? false;
		}

		static ITagOperationResult AddTagToProcessHeader(INotifications notifications, ITagMagnitude tagMagnitude, IProcessHeader processHeader)
		{
			var result = processHeader.AddTag(tagMagnitude);
			notifications.Add(new InfoNotification(Res.GetString("EAD39118-FF23-4ECD-9EAD-8C8F94E87ECD", "Tag: '{0}' applied to Workflow: '{1}' successfully.", tagMagnitude.TGM_Code, processHeader.FH_CompletionStatement.TrimEnd('.'))));

			return result;
		}

		internal static (PropertyInfo FinalPropertyInfo, Type ParentType, ZString fieldName) GetFinalPropertyInfoAndParentType(ProcessTaskNotification action, INotifications notifications)
		{
			return WorkflowProcessorHelper.GetFinalPropertyInfoAndParentType(action, notifications, new WorkflowMacroApplyTagValidation(action));
		}
	}
}

