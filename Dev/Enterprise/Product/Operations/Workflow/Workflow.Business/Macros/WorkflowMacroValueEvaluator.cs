using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class WorkflowMacroValueEvaluator : IWorkflowMacroValueEvaluator
	{
		public T GetMacroValue<T>(BusinessObjectFactory factory, object data, string macro)
			=> WorkflowMacroValueEvaluatorImpl.GetMacroValue<T>(factory, data, macro);

		public bool? EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, ZString macro)
			=> WorkflowMacroValueEvaluatorImpl.EvaluateBooleanExpression(factory, macroContext, macro);

		public bool EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, IMacroBooleanExpressionClause clause)
			=> WorkflowMacroValueEvaluatorImpl.EvaluateBooleanExpression(factory, macroContext, clause);
	}

	static class WorkflowMacroValueEvaluatorImpl
	{
		internal static bool EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, IMacroBooleanExpressionClause clause)
		{
			bool isConditionMet(string condition, Func<bool> evaluator) => evaluator == null ? EvalCondition(condition) : evaluator();
			return clause.EvaluateBy(isConditionMet);

			bool EvalCondition(string condition)
			{
				var result = EvaluateBooleanExpression(factory, macroContext, condition);
				return result ?? false;
			}
		}

		internal static T GetMacroValue<T>(BusinessObjectFactory factory, object data, string macro)
		{
			using (var context = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetDefaultWorkflowMacroContext(factory, data))
			{
				var result = GetMacroValueCore(factory, context, macro);
				return result is T tResult ? tResult : default(T);
			}
		}

		internal static bool? EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, string macro)
		{
			var macroValue = GetMacroValueCore(factory, macroContext, macro);

			if (macroValue == null || !ZDataType.IsConvertibleToZType(macroValue))
			{
				return null;
			}

			var result = ZDataType.ObjectToZType(macroValue);
			return (result != null && result is ZBool) ? (bool?)(ZBool)result : null;
		}

		static object GetMacroValueCore(BusinessObjectFactory factory, IAntlrMacroContext macroContext, string macro)
		{
			var expr = macro.With(macroContext.Libraries.CreateContext()).CreateExpression();
			return new MacroEvaluator().EvaluateMacroValue(macroContext.Scope, expr);
		}
	}
}
