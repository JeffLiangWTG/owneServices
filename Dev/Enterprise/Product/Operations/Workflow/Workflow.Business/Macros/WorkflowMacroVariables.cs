using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Workflow.Business
{
	public static class WorkflowMacroVariables
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is macro code constant.")]
		public const string EnvironmentVariableName = "env";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is macro code constant.")]
		public const string EventVariableName = "Event";
		public const string EventSourceVariableName = "EventSource";
		public const string WorkflowItemVariableName = "WorkflowItem";

		internal static Dictionary<string, (object Object, Type Type)> GetBaseVariables()
		{
			return new Dictionary<string, (object, Type)>
			{
				{ EnvironmentVariableName, (new MasterFiles.Business.Macros.Environment(), typeof(MasterFiles.Business.Macros.Environment)) }
			};
		}

		public static Dictionary<string, (object, Type)> GetVariablesForTemplateConditions(ITemplateConditional templateConditions)
		{
			var variables = GetBaseVariables();
			variables[WorkflowItemVariableName] = (templateConditions, templateConditions.GetType());
			return variables;
		}

		public static Dictionary<string, (object, Type)> GetVariablesForTriggerConditions(Type logType, IStmALog log, Type logParentType, BusinessObject logParent, ITriggerConditions triggerConditions)
		{
			var variables = GetBaseVariables();
			variables[EventVariableName] = (log, logType);
			variables[EventSourceVariableName] = (logParent, logParentType);
			variables[WorkflowItemVariableName] = (triggerConditions, triggerConditions.GetType());
			return variables;
		}
	}
}
