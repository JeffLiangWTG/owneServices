using System;
using System.Collections.Generic;
using CargoWise.Macros;

namespace Enterprise.Workflow.Business
{
	class EmptyConditionClause : IMacroBooleanExpressionClause
	{
		public bool IsConjunction => true;

		public bool IsNotOperator => false;

		public IEnumerable<IMacroBooleanExpressionClause> Children => Array.Empty<IMacroBooleanExpressionClause>();

		public string AsString()
		{
			return string.Empty;
		}

		public bool EvaluateBy(Func<string, Func<bool>, bool> evaluator)
		{
			return false;
		}
	}
}
