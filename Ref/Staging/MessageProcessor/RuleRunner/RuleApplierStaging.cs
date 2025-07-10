using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using stagingDb = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public class RuleApplierStaging
	{
		public RuleApplierStaging(ISafeRepository safe, stagingDb.IStagingRepository staging)
		{
			Argument.NotNull(staging, nameof(staging));
			this.staging = staging;
		}

		readonly stagingDb.IStagingRepository staging;

		public async Task ApplyAsync(ITariffRuleWrapper rule, stagingDb.RefCusTariff tariff, stagingDb.RefCusTariffUOM[] uoms, stagingDb.RefCusTariffAttribute[] attrs, stagingDb.RefCusRate[] rates)
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
					staging.Add(ruleData.Data);
					result.Add(ruleData);
				}
				if (ruleData.RuleType == RuleType.UPDATE)
				{
					result.Add(ruleData);
				}
			}
			return result.ToArray();
		}
	}
}
