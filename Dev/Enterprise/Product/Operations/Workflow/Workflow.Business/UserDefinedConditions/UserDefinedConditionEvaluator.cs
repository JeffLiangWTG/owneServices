using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Macros;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	class UserDefinedConditionEvaluator : IUserDefinedConditionEvaluator
	{
		public void ClearCache(IBusiness workflowParent)
			=> UserDefinedConditionEvaluatorImpl.ClearCache(workflowParent);

		public bool IsTextMacroConditionMet(IMacroBooleanExpressionClause conditionValue, BusinessObject jobOrLine, BusinessObject job = null, bool useTemplateCacheForConditions = true, params BusinessObject[] dataContext)
			=> UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(conditionValue, jobOrLine, job, useTemplateCacheForConditions, dataContext);

		public bool IsTextMacroConditionMet(ZString conditionValue, BusinessObject jobOrLine, BusinessObject job = null, bool useTemplateCacheForConditions = true, params BusinessObject[] dataContext)
			=> UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(conditionValue.ToUdfEvaluatableConditionValue(), jobOrLine, job, useTemplateCacheForConditions, dataContext);
	}

	static class UserDefinedConditionEvaluatorImpl
	{
		internal static bool IsTextMacroConditionMet(IMacroBooleanExpressionClause conditionValue, BusinessObject jobOrLine, BusinessObject job = null, bool useTemplateCacheForConditions = true, params BusinessObject[] dataContext)
		{
			Func<string, Func<bool>, bool> isConditionMet;
			var prepatedDataContext = new Lazy<BusinessObject[]>(() => GetDataContextWithProcessJobHeader(jobOrLine, dataContext, job), false);

			if (useTemplateCacheForConditions)
			{
				var cache = GetCache(jobOrLine.Factory);
				var jobCache = cache.GetOrAdd(jobOrLine.PK, () => new UdfConditionAndWhetherItIsMet());

				isConditionMet = (condition, evaluator) => jobCache.GetOrAdd(condition, evaluator ?? (() => IsTextMacroConditionMet(prepatedDataContext.Value, condition)));
				return conditionValue.EvaluateBy(isConditionMet);
			}

			using (jobOrLine.Factory.ServiceContainer.GetService<ITextMacroProcessingCachingService>()?.WithNoCaching())
			{
				isConditionMet = (condition, evaluator) => evaluator == null ? IsTextMacroConditionMet(prepatedDataContext.Value, condition) : evaluator();
				return conditionValue.EvaluateBy(isConditionMet);
			}
		}

		internal static void ClearCache(IBusiness workflowParent)
		{
			var cache = GetCache(workflowParent.Factory);

			cache.Remove(workflowParent.Identifier);
		}

		#region Implementation

		static UdfConditionCacheByJobPK GetCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(UdfConditionCacheByJobPK), () => new UdfConditionCacheByJobPK(), CacheStalenessPolicy.StaleBeforeFactorySavingTransaction);
		}

		static bool IsTextMacroConditionMet(BusinessObject[] objectList, ZString conditionValue)
		{
			try
			{
				var useJs = RawDataRegistry.Instance.UseJSEngineForTriggerConditionsEvaluation.Value;
				var expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(conditionValue.Trim(), objectList, useJs: useJs);
				var res = expression.EvaluateExpression(useJs);
				return res.IsRight && res.Right;
			}
			catch (EmailHasNoRecipientsException)
			{
				// We fail silently, because this configuration is allowed.
				return false;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error in UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet", $"Failed on UdfCondition: {conditionValue}", ex);
				return false;
			}
		}

		static BusinessObject[] GetDataContextWithProcessJobHeader(BusinessObject jobOrLine, BusinessObject[] objectList, BusinessObject job = null)
		{
			var processJobHeader = ProcessJobHeaderProvider.GetForParent((IWorkflowProvider)(job ?? jobOrLine), jobOrLine.Factory, addDefaultProcessHeaderIfNone: false) as BusinessObject;
			return new BusinessObject[] { jobOrLine }.Concat(objectList).Append(processJobHeader).WhereNotNull().ToArray();
		}

		class UdfConditionCacheByJobPK : Dictionary<ZGuid, UdfConditionAndWhetherItIsMet>
		{
		}

		class UdfConditionAndWhetherItIsMet : Dictionary<ZString, bool>
		{
		}

		#endregion

#if DEBUG

		internal static bool IsCached(BusinessObject jobOrLine, ZString condition, bool result)
		{
			var cache = GetCache(jobOrLine.Factory);
			var jobOrLineCache = cache.GetOrAdd(jobOrLine.PK, () => new UdfConditionAndWhetherItIsMet());
			var isCached = jobOrLineCache.TryGetValue(condition, out bool value);

			return isCached && result == value;
		}

#endif
	}
}
