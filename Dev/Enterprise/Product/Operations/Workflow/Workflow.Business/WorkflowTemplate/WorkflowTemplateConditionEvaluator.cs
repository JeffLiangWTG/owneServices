using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowTemplateConditionEvaluator : IWorkflowTemplateConditionEvaluator
	{
		public bool AreConditionsMetForTemplateApplication(IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem workflowItem)
		{
			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);
			var applicationExtender = workflowDescriptor?.GetTemplateApplicationExtender();

			var originCountry = ZString.Empty;
			var destinationCountry = ZString.Empty;
			if (!workflowItem.OriginCountryCode.IsEmpty || !workflowItem.DestinationCountryCode.IsEmpty)
			{
				if (applicationExtender is IPortCountryCode portCountryCode)
				{
					originCountry = portCountryCode.OriginCountry(workflowProvider);
					destinationCountry = portCountryCode.DestinationCountry(workflowProvider);
				}
				else
				{
					originCountry = workflowProvider.WorkflowItems.OriginCountry;
					destinationCountry = workflowProvider.WorkflowItems.DestinationCountry;
				}
			}

			return
				(workflowItem.OriginCountryCode.IsEmpty || workflowItem.OriginCountryCode == originCountry)
				&& (workflowItem.DestinationCountryCode.IsEmpty || workflowItem.DestinationCountryCode == destinationCountry)
				&& IsCondition1Met(workflowProvider, workflowItem)
				&& IsCondition2Met(workflowProvider, workflowItem);
		}

		public bool HasNewConditionBeenMetSinceLastSave(IWorkflowProvider workflowProvider)
		{
			var processTaskCollection = workflowProvider.WorkflowItems;
			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);
			var applicationExtender = workflowDescriptor?.GetTemplateApplicationExtender();

			if (applicationExtender != null)
			{
				return applicationExtender.HasNewConditionBeenMetSinceLastSave(workflowProvider);
			}

			return processTaskCollection.HasNewConditionBeenMetSinceLastSave();
		}

		bool IsCondition1Met(IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem workflowItem)
		{
			if (workflowItem.TemplateCondition1.IsEmpty)
			{
				return true;
			}

			var processTaskCollection = workflowProvider.WorkflowItems;
			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);
			var applicationExtender = workflowDescriptor?.GetTemplateApplicationExtender();

			if (applicationExtender != null)
			{
				return applicationExtender.IsCondition1Met(workflowProvider, workflowItem.TemplateCondition1);
			}

			return processTaskCollection.IsCondition1Met(workflowItem.TemplateCondition1);
		}

		public bool IsCondition2Met(IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem workflowItem)
		{
			var conditionCode = workflowItem.TemplateCondition2;
			var parent = (BusinessObject)workflowProvider;
			if (conditionCode.IsEmpty)
			{
				return true;
			}
			else if (conditionCode.EqualsIgnoringCase(ProcessTasksLookups.UserDefinedCondition))
			{
				var dataContext = workflowItem.GetWorkflowDescriptor().GetUDFMacroDataContext(workflowItem, parent);
				return ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(new UserDefinedEvaluatableConditionValue().GetPossiblyCachedCondition2Value(workflowItem), parent, dataContext: dataContext);
			}
			else if (conditionCode.EqualsIgnoringCase(ProcessTasksLookups.MacroCondition))
			{
				using (var macroContext = new WorkflowMacroContextDecider().GetContextForTemplateConditions(parent.Factory, parent.GetType(), parent, workflowItem))
				{
					return ObjectFactory.Get<IWorkflowMacroValueEvaluator>().EvaluateBooleanExpression(parent.Factory, macroContext, new MacroEvaluatableConditionValue().GetPossiblyCachedCondition2Value(workflowItem));
				}
			}
			else
			{
				var applicationExtender = workflowItem.GetWorkflowDescriptor()?.GetTemplateApplicationExtender();
				if (applicationExtender != null)
				{
					return applicationExtender.IsCondition2Met(workflowProvider, conditionCode, workflowItem.TemplateCondition2Value);
				}
				else
				{
					return workflowProvider.WorkflowItems.IsCondition2MetCoreExposed(conditionCode, workflowItem.TemplateCondition2Value);
				}
			}
		}
	}
}
