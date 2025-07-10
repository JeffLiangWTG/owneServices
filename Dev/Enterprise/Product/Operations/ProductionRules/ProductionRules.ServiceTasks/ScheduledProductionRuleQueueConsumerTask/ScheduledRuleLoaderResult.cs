using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledRuleLoaderResult : IDisposable
	{
		public ScheduledRuleLoaderResult(
			AppLockedItem<ProductionRuleSet> ruleSetWithLock,
			IEnumerable<ProductionRule> rules,
			IEnumerable<ProductionRuleScheduleQueue> queueEntries,
			IScheduledRuleProcessor ruleProcessor)
		{
			RuleSetWithLock = Argument.NotNull(ruleSetWithLock, nameof(ruleSetWithLock));
			QueueEntries = Argument.NotNull(queueEntries, nameof(queueEntries));
			RuleProcessor = Argument.NotNull(ruleProcessor, nameof(ruleProcessor));

			Argument.NotNull(rules, nameof(rules));
			Rules = rules.Select(rule => rule.GetProductionRuleWrapper()).ToArray();
		}

		public AppLockedItem<ProductionRuleSet> RuleSetWithLock { get; }
		public IEnumerable<ProductionRuleScheduleQueue> QueueEntries { get; }
		public IEnumerable<IProductionRule> Rules { get; }
		public IScheduledRuleProcessor RuleProcessor { get; }

		void IDisposable.Dispose() => RuleSetWithLock.Lock?.Dispose();
	}
}
