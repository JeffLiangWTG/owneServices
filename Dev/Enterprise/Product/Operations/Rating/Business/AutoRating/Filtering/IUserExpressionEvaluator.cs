using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	interface IUserExpressionEvaluator
	{
		bool? IsUserDefinedConditionMet(ZString conditionalExpression);
	}
}
