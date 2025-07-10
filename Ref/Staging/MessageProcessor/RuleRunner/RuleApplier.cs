using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
namespace CargoWise.RefDbRepo.Staging.Rule
{
	public class RuleApplier
	{
		public RuleApplier(ISafeRepository safe)
		{
			Argument.NotNull(safe, nameof(safe));
			this.safe = safe;
		}

		readonly ISafeRepository safe;

		public async Task ApplyAsync(ITariffRuleWrapper rule, Safe.RefCusTariff tariff, Safe.RefCusTariffUOM[] uoms, Safe.RefCusTariffAttribute[] attrs, Safe.RefCusRate[] rates)
		{
			Argument.NotNull(tariff, nameof(tariff));
			Argument.NotNull(rule, nameof(rule));
			Argument.NotNull(uoms, nameof(uoms));
			Argument.NotNull(rates, nameof(rates));
			Argument.NotNull(attrs, nameof(attrs));

			await ApplyAsync(rule, tariff.ZZ1_PK, uoms);
			await ApplyAsync(rule, tariff.ZZ1_PK, attrs);
			await ApplyAsync(rule, tariff.ZZ1_PK, rates);
		}

		async Task<IRuleResult<T>[]> ApplyAsync<T>(ITariffRuleWrapper rule, Guid parentPK, T[] data) where T : class
		{
			Argument.NotNull(rule, nameof(rule));
			Argument.NotNull(data, nameof(data));

			var result = new List<IRuleResult<T>>();
			var ruleDataCollection = await rule.SynchroniseAsync(parentPK, data);
			foreach (var ruleData in ruleDataCollection)
			{
				if (ruleData.RuleType == RuleType.INSERT)
				{
					safe.Add(ruleData.Data);
					result.Add(ruleData);
				}
				if (ruleData.RuleType == RuleType.UPDATE)
				{
					safe.Update(ruleData.Data);
					result.Add(ruleData);
				}
			}
			return result.ToArray();
		}
	}
}
