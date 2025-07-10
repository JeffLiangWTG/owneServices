using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public class TariffRuleHeaderWrapper : TariffRuleWrapper, ITariffRuleHeaderWrapper
	{
		public TariffRuleHeaderWrapper(RefCusTariffRule rule, ISafeRepository safe) : base(rule.ZZ1_PK, safe)
		{
			Argument.NotNull(rule, nameof(rule));
			Argument.NotNull(safe, nameof(safe));

			this.rule = rule;
		}

		readonly RefCusTariffRule rule;

		public async Task<RefCusTariff[]> GetMatchingTariffsWithRelatedObjects()
		{
			var results = new List<RefCusTariff>();
			foreach (var tariff in await Safe.Get<RefCusTariff>().Expand(x => x.RefCusTariffType).Expand(x => x.RefCusTariffAttributes)
				.Expand(x => x.RefCusTariffRelationships).Expand(x => x.RefCusRates)
				.Expand(x => x.RefCusTariffUOMs)
				.Where(x => x.ZZ1_TariffCode.StartsWith(rule.ZZ1_TariffCode)).ExecuteAsync())
			{
				var schedule = tariff.RefCusTariffType?.ZZI_TariffType ?? string.Empty;
				var checkdigit = tariff.RefCusTariffAttributes.FirstOrDefault(x => x.ZZ3_Name.Equals("CheckDigit", StringComparison.OrdinalIgnoreCase))?.ZZ3_Value ?? string.Empty;
				var relationship = tariff.RefCusTariffRelationships?.FirstOrDefault()?.ZZH_TariffCode ?? string.Empty;
				if (IsMatch(tariff.ZZ1_TariffCode, schedule, checkdigit, relationship, tariff.ZZ1_ZZZ_NKDataGrouping))
				{
					results.Add(tariff);
				}
			}
			return results.ToArray();
		}

		public bool IsMatch(string tariffCode, string schedule, string chkDigit, string relationshipCode, string countryCode)
		{
			Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));
			Argument.NotNullOrEmpty(schedule, nameof(schedule));
			Argument.NotNullOrEmpty(countryCode, nameof(countryCode));

			var result = countryCode.Equals(rule.ZZ1_ZZZ_NKDataGrouping, StringComparison.Ordinal) && tariffCode.StartsWith(rule.ZZ1_TariffCode, StringComparison.OrdinalIgnoreCase);
			if (result)
			{
				result = rule.RefCusTariffType == null || schedule.Equals(rule.RefCusTariffType.ZZI_TariffType, StringComparison.OrdinalIgnoreCase);
			}
			if (result)
			{
				var ruleDigit = rule.RefCusTariffAttributeRules?.FirstOrDefault(x => x.ZZ3_ZZ1_Tariff == rule.ZZ1_PK && x.ZZ3_Name.Equals("CheckDigit", StringComparison.OrdinalIgnoreCase));
				result = ruleDigit?.ZZ3_Value == null || ruleDigit.ZZ3_Value.Equals(chkDigit, StringComparison.OrdinalIgnoreCase);
			}
			if (result)
			{
				var relationship = rule.RefCusTariffRelationshipRules?.FirstOrDefault();
				result = relationship?.ZZH_TariffCode == null || relationship.ZZH_TariffCode.Equals(relationshipCode, StringComparison.OrdinalIgnoreCase);
			}
			return result;
		}

		public void MarkAsApplied()
		{
			rule.ZZ1_Applied = true;
			Safe.Update(rule);
		}
	}
}
