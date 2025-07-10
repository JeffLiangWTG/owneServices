using System;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	class MacroEvaluatableConditionValue : EvaluatableConditionValue
	{
		protected override Guid cacheIdentifier => new Guid("4A329CED-9698-42FC-B3A0-ECC22CCAA29F");

		protected override Func<ZString, IMacroBooleanExpressionClause> splitFunction => (condition) =>
		{
			var tokenizer = new MacroBooleanExpressionTokenizer();
			return MacroBooleanExpressionClauseEvaluator.CreateClause(tokenizer.Tokenize(condition).GetEnumerator(), condition);
		};
	}
}
