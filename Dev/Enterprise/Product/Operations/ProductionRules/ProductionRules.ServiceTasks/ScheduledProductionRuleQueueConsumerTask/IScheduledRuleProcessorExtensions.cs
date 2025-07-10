using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.ProductionRules.Integration
{
	public static class IScheduledRuleProcessorExtensions
	{
		public static IDisposable GetTemporaryUserContextWithBranchOffRuleSet(
			this IScheduledRuleProcessor rulesProcessor,
			ReadOnlyBusinessObjectFactory readOnlyFactory,
			IProductionRuleSet ruleSet)
		{
			var branchPk = rulesProcessor.GetBranchToRunRulesAgainst(readOnlyFactory, ruleSet).ToGuid();
			return DisposableEnvironment.ForBranch(branchPk);
		}
	}
}
