using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IUserDefinedConditionEvaluator
	{
		public bool IsTextMacroConditionMet(IMacroBooleanExpressionClause conditionValue, BusinessObject jobOrLine, BusinessObject job = null, bool useTemplateCacheForConditions = true, params BusinessObject[] dataContext);
		public bool IsTextMacroConditionMet(ZString conditionValue, BusinessObject jobOrLine, BusinessObject job = null, bool useTemplateCacheForConditions = true, params BusinessObject[] dataContext);
		public void ClearCache(IBusiness workflowParent);
	}
}
