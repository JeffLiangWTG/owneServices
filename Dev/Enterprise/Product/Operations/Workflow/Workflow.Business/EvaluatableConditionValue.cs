using System;
using CargoWise.Application;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	abstract class EvaluatableConditionValue
	{
		protected abstract Guid cacheIdentifier { get; }
		protected abstract Func<ZString, IMacroBooleanExpressionClause> splitFunction { get; }

		public IMacroBooleanExpressionClause GetPossiblyCachedCondition2Value(ITemplateConditional templateConditional)
		{
			var booleanExpressionClauseCache = ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>();

			if (!(templateConditional is ProcessTask processTask))
			{
				return booleanExpressionClauseCache.GetBooleanExpressionClauseCached(cacheIdentifier, templateConditional.TemplateCondition2Value, splitFunction);
			}

			var parentCacheConditionValue = ObjectFactory.Get<IWorkflowConditionValueParentCache>().GetConditionValue(processTask);
			return booleanExpressionClauseCache.GetBooleanExpressionClauseCached(cacheIdentifier, parentCacheConditionValue, splitFunction);
		}

		public IMacroBooleanExpressionClause GetBooleanExpressionClause(ZString conditionValue)
		{
			return ObjectFactory.Get<IWorkflowConditionValueBooleanExpressionClauseCache>().GetBooleanExpressionClauseCached(cacheIdentifier, conditionValue, splitFunction);
		}
	}
}
