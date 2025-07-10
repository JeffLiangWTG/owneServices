using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowMacroContextDecider
	{
		IAntlrMacroContext GetContextForTriggerConditions(BusinessObject parent, IBaseTrigger trigger, IStmALog log);

		IAntlrMacroContext GetContextForTriggerConditionsForUserInterface(Type parentType, BusinessObject parent, IBaseTrigger trigger, Type logParentType, BusinessObject logParent, Func<string, string> errorMessageExtender = null);

		IAntlrMacroContext GetContextForTemplateConditions(BusinessObjectFactory factory, Type parentType, BusinessObject parent, ITemplateConditional processTask);

		IAntlrMacroContext GetDefaultWorkflowMacroContext(BusinessObjectFactory factory, object dataObject, Type dataObjectType = null, Dictionary<string, (object, Type)> variables = null, Func<string, string> errorMessageExtender = null);

		Dictionary<string, (object Object, Type Type)> GetBaseVariables();
	}
}
