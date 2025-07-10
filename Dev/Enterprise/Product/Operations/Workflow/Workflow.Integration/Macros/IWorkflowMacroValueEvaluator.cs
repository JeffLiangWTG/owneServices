using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowMacroValueEvaluator
	{
		public T GetMacroValue<T>(BusinessObjectFactory factory, object data, string macro);
		public bool? EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, ZString macro);
		public bool EvaluateBooleanExpression(BusinessObjectFactory factory, IAntlrMacroContext macroContext, IMacroBooleanExpressionClause clause);
	}
}
