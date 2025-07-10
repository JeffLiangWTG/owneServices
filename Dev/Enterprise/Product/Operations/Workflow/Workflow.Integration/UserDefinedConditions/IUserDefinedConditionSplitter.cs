using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IUserDefinedConditionSplitter
	{
		public IMacroBooleanExpressionClause Split(ZString condition);
	}
}
