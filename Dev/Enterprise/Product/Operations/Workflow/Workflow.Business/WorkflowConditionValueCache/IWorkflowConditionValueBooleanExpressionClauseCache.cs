using System;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	public interface IWorkflowConditionValueBooleanExpressionClauseCache
	{
		IMacroBooleanExpressionClause GetBooleanExpressionClauseCached(Guid identifier, ZString condition, Func<ZString, IMacroBooleanExpressionClause> splitFunction);
	}
}
