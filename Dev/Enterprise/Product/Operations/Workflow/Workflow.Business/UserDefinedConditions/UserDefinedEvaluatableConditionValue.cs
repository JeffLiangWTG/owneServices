using System;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	class UserDefinedEvaluatableConditionValue : EvaluatableConditionValue
	{
		protected override Guid cacheIdentifier => new Guid("26CFF74D-1156-4FFC-A638-408F638E42A2");

		protected override Func<ZString, IMacroBooleanExpressionClause> splitFunction => new UserDefinedConditionSplitter().Split;
	}

	public static class UserDefinedEvaluatableConditionValueExtension
	{
		public static IMacroBooleanExpressionClause ToUdfEvaluatableConditionValue(this ZString value)
		{
			return new UserDefinedEvaluatableConditionValue().GetBooleanExpressionClause(value);
		}

		public static IMacroBooleanExpressionClause ToUdfEvaluatableConditionValue(this string value)
		{
			return new UserDefinedEvaluatableConditionValue().GetBooleanExpressionClause(value);
		}
	}
}
