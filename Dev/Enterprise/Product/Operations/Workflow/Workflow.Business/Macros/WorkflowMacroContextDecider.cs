using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class WorkflowMacroContextDecider : IWorkflowMacroContextDecider
	{
		public IAntlrMacroContext GetContextForTriggerConditions(BusinessObject parent, IBaseTrigger trigger, IStmALog log)
			=> WorkflowMacroContextDeciderImpl.GetContextForTriggerConditions(parent, trigger, log);
		public IAntlrMacroContext GetContextForTriggerConditionsForUserInterface(Type parentType, BusinessObject parent, IBaseTrigger trigger, Type logParentType, BusinessObject logParent, Func<string, string> errorMessageExtender = null)
			=> WorkflowMacroContextDeciderImpl.GetContextForTriggerConditions(parentType, parent, trigger, logParentType, logParent, errorMessageExtender);

		public IAntlrMacroContext GetContextForTemplateConditions(BusinessObjectFactory factory, Type parentType, BusinessObject parent, ITemplateConditional processTask)
			=> WorkflowMacroContextDeciderImpl.GetContextForTemplateConditions(factory, parentType, parent, processTask);

		public IAntlrMacroContext GetDefaultWorkflowMacroContext(BusinessObjectFactory factory, object dataObject, Type dataObjectType = null, Dictionary<string, (object, Type)> variables = null, Func<string, string> errorMessageExtender = null)
			=> WorkflowMacroContextDeciderImpl.GetDefaultWorkflowMacroContext(factory, dataObject, dataObjectType, variables, errorMessageExtender);

		public Dictionary<string, (object Object, Type Type)> GetBaseVariables()
			=> WorkflowMacroContextDeciderImpl.GetBaseVariables();
	}

	static class WorkflowMacroContextDeciderImpl
	{
		#region Trigger Conditions

		internal static IAntlrMacroContext GetContextForTriggerConditions(Type parentType, BusinessObject parent, IBaseTrigger trigger, Type logParentType, BusinessObject logParent, Func<string, string> errorMessageExtender = null)
		{
			Argument.NotNull(parentType, nameof(parentType));
			Argument.NotNull(trigger, nameof(trigger));
			Argument.NotNull(logParentType, nameof(logParentType));
			return GetContextForTriggerConditions(parentType, parent, trigger, logParentType, logParent, typeof(IStmALog), null, errorMessageExtender);
		}

		internal static IAntlrMacroContext GetContextForTriggerConditions(BusinessObject parent, IBaseTrigger trigger, IStmALog log)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(trigger, nameof(trigger));
			return GetContextForTriggerConditions(parent.GetType(), parent, trigger, log?.Master?.GetType(), log?.Master, log?.GetType(), log);
		}

		static IAntlrMacroContext GetContextForTriggerConditions(Type parentType, BusinessObject parent, IBaseTrigger trigger, Type logParentType, BusinessObject logParent, Type logType, IStmALog log, Func<string, string> errorMessageExtender = null)
		{
			if (!WorkflowDataRegistry.Instance.McrDataFieldMapEnhancements.Value)
			{
				if (trigger is ProcessTask processTask)
				{
					var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => processTask.P9_ActualDate)), new LogEventDataModel(new ExampleLog(trigger)), processTask.ParentBusinessObject, () => (processTask.ParentBusinessObject, processTask.WorkflowDescriptor));
					return GetDefaultWorkflowMacroContext(trigger.Factory, dataModel);
				}

				return GetDefaultWorkflowMacroContext(trigger.Factory, null, parentType, WorkflowMacroVariables.GetVariablesForTriggerConditions(typeof(IStmALog), null, logParentType, null, trigger));
			}

			var variables = WorkflowMacroVariables.GetVariablesForTriggerConditions(log?.GetType() ?? typeof(IStmALog), log, logParentType, logParent, trigger);
			return GetDefaultWorkflowMacroContext(trigger.Factory, parent, parentType, variables, errorMessageExtender);
		}

		#endregion

		#region Template Conditions

		internal static IAntlrMacroContext GetContextForTemplateConditions(BusinessObjectFactory factory, Type parentType, BusinessObject parent, ITemplateConditional processTask)
		{
			return GetDefaultWorkflowMacroContext(factory, parent, parentType, WorkflowMacroVariables.GetVariablesForTemplateConditions(processTask));
		}

		#endregion

		#region Libaries and Variables

		internal static Dictionary<string, (object Object, Type Type)> GetBaseVariables() => WorkflowMacroVariables.GetBaseVariables();

		internal static IAntlrMacroContext GetDefaultWorkflowMacroContext(BusinessObjectFactory factory, object parent, Type parentType = null, Dictionary<string, (object, Type)> variables = null, Func<string, string> errorMessageExtender = null)
		{
			if (variables == null)
			{
				variables = GetBaseVariables();
			}
			return new AntlrMacroContext(parent, parentType, GetDefaultWorkflowMacroLibrary(factory), variables, errorMessageExtender);
		}

		static IMacroLibrary[] GetDefaultWorkflowMacroLibrary(BusinessObjectFactory factory)
		{
			return
				[
					ObjectFactory.Get<IMacroLibrary>(),
					new LocationsLibrary(factory),
					new ValidationToolLibrary(factory)
				];
		}

		#endregion
	}
}
