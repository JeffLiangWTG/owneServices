using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public class TariffRuleWrapper : ITariffRuleWrapper
	{
		public TariffRuleWrapper(Guid ruleId, ISafeRepository safe)
		{
			Argument.NotNull(safe, nameof(safe));
			this.ruleId = ruleId;
			this.safe = safe;
		}

		readonly Guid ruleId;
		readonly ISafeRepository safe;

		protected ISafeRepository Safe
		{
			get { return safe; }
		}

		public async Task<IEnumerable<IRuleResult<T>>> SynchroniseAsync<T>(Guid parentPK, T[] data)
		{
			Argument.NotNull(data, nameof(data));
			if (typeof(T) == typeof(Safe.RefCusTariffUOM))
			{
				return (await GetUOMsAsync(parentPK, data as Safe.RefCusTariffUOM[])).Select(x => (IRuleResult<T>)x);
			}
			else if (typeof(T) == typeof(Safe.RefCusTariffAttribute))
			{
				return (await GetAttributesAsync(parentPK, data as Safe.RefCusTariffAttribute[])).Select(x => (IRuleResult<T>)x);
			}
			else if (typeof(T) == typeof(Safe.RefCusRate))
			{
				return (await GetRatesAsync(parentPK, data as Safe.RefCusRate[])).Select(x => (IRuleResult<T>)x);
			}
			else if (typeof(T) == typeof(Stage.RefCusTariffUOM))
			{
				return (await GetUOMsAsync(parentPK, data as Stage.RefCusTariffUOM[])).Select(x => (IRuleResult<T>)x);
			}
			else if (typeof(T) == typeof(Stage.RefCusTariffAttribute))
			{
				return (await GetAttributesAsync(parentPK, data as Stage.RefCusTariffAttribute[])).Select(x => (IRuleResult<T>)x);
			}
			else if (typeof(T) == typeof(Stage.RefCusRate))
			{
				return (await GetRatesAsync(parentPK, data as Stage.RefCusRate[])).Select(x => (IRuleResult<T>)x);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		async Task<IEnumerable<IRuleResult<Safe.RefCusTariffUOM>>> GetUOMsAsync(Guid parentPK, Safe.RefCusTariffUOM[] uoms)
		{
			var result = new List<IRuleResult<Safe.RefCusTariffUOM>>();
			foreach (var uomRule in await safe.Get<Safe.RefCusTariffUOMRule>().Where(x => x.ZZ8_ZZ1_Tariff == ruleId).ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var uom = uoms.FirstOrDefault(x => x.ZZ8_Type.Equals(uomRule.ZZ8_Type, StringComparison.Ordinal));
				if (uom == null)
				{
					ruleType = RuleType.INSERT;
					uom = new Safe.RefCusTariffUOM
					{
						ZZ8_PK = Guid.NewGuid(),
						ZZ8_Type = uomRule.ZZ8_Type,
						ZZ8_ZZ1_Tariff = parentPK,
						ZZ8_ZZZ_NKDataGrouping = uomRule.ZZ8_ZZZ_NKDataGrouping
					};
				}
				uom.ZZ8_UOM = uomRule.ZZ8_UOM;
				result.Add(new RuleResult<Safe.RefCusTariffUOM> { Data = uom, Rule = new TariffRuleWrapper(uomRule.ZZ8_PK, safe), RuleType = ruleType });
			}
			return result;
		}

		async Task<IEnumerable<IRuleResult<Safe.RefCusTariffAttribute>>> GetAttributesAsync(Guid parentPK, Safe.RefCusTariffAttribute[] attrs)
		{
			var result = new List<IRuleResult<Safe.RefCusTariffAttribute>>();
			foreach (var attrRule in await safe.Get<Safe.RefCusTariffAttributeRule>().Where(x => x.ZZ3_ZZ1_Tariff == ruleId && x.ZZ3_Name != "CheckDigit").ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var attr = attrs.FirstOrDefault(x => x.ZZ3_Name.Equals(attrRule.ZZ3_Name, StringComparison.Ordinal));
				if (attr == null)
				{
					ruleType = RuleType.INSERT;
					attr = new Safe.RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_Name = attrRule.ZZ3_Name, ZZ3_ZZ1_Tariff = parentPK };
				}
				attr.ZZ3_Value = attrRule.ZZ3_Value;
				result.Add(new RuleResult<Safe.RefCusTariffAttribute> { Data = attr, Rule = new TariffRuleWrapper(attrRule.ZZ3_PK, safe), RuleType = ruleType });
			}
			return result;
		}

		async Task<IEnumerable<IRuleResult<Safe.RefCusRate>>> GetRatesAsync(Guid parentPK, Safe.RefCusRate[] rates)
		{
			var result = new List<IRuleResult<Safe.RefCusRate>>();
			foreach (var rateRule in await safe.Get<Safe.RefCusRateRule>().Where(x => x.ZZ2_ZZ1_Tariff == ruleId).ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var rate = rates.FirstOrDefault(x => string.IsNullOrEmpty(x.ZZ2_SelectorFormula) || x.ZZ2_SelectorFormula.Equals(rateRule.ZZ2_SelectorFormula, StringComparison.Ordinal));
				if (rate == null)
				{
					ruleType = RuleType.INSERT;
					rate = new Safe.RefCusRate
					{
						ZZ2_PK = Guid.NewGuid(),
						ZZ2_SelectorFormula = rateRule.ZZ2_SelectorFormula ?? string.Empty,
						ZZ2_ZZ1_Tariff = parentPK,
						ZZ2_StartDate = new DateTime(1900, 01, 01, 0, 0, 0).ToUTCDateTimeOffset(),
						ZZ2_EndDate = new DateTime(2079, 06, 06, 23, 59, 0).ToUTCDateTimeOffset()
					};
				}
				rate.ZZ2_RateFormula = rateRule.ZZ2_RateFormula;
				result.Add(new RuleResult<Safe.RefCusRate> { Data = rate, Rule = new TariffRuleWrapper(rateRule.ZZ2_PK, safe), RuleType = ruleType });
			}
			return result;
		}

		async Task<IEnumerable<IRuleResult<Stage.RefCusTariffUOM>>> GetUOMsAsync(Guid parentPK, Stage.RefCusTariffUOM[] uoms)
		{
			var result = new List<IRuleResult<Stage.RefCusTariffUOM>>();
			foreach (var uomRule in await safe.Get<Safe.RefCusTariffUOMRule>().Where(x => x.ZZ8_ZZ1_Tariff == ruleId).ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var uom = uoms.FirstOrDefault(x => x != null && x.ZZ8_Type.Equals(uomRule.ZZ8_Type, StringComparison.Ordinal));
				if (uom == null)
				{
					ruleType = RuleType.INSERT;
					uom = new Stage.RefCusTariffUOM { ZZ8_PK = Guid.NewGuid(), ZZ8_Type = uomRule.ZZ8_Type, ZZ8_ZZ1_Tariff = parentPK, ZZ8_ZZZ_NKDataGrouping = uomRule.ZZ8_ZZZ_NKDataGrouping };
				}
				uom.ZZ8_UOM = uomRule.ZZ8_UOM;
				result.Add(new RuleResult<Stage.RefCusTariffUOM> { Data = uom, Rule = new TariffRuleWrapper(uomRule.ZZ8_PK, safe), RuleType = ruleType });
			}
			return result;
		}

		async Task<IEnumerable<IRuleResult<Stage.RefCusTariffAttribute>>> GetAttributesAsync(Guid parentPK, Stage.RefCusTariffAttribute[] attrs)
		{
			var result = new List<IRuleResult<Stage.RefCusTariffAttribute>>();
			foreach (var attrRule in await safe.Get<Safe.RefCusTariffAttributeRule>().Where(x => x.ZZ3_ZZ1_Tariff == ruleId && x.ZZ3_Name != "CheckDigit").ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var attr = attrs.FirstOrDefault(x => x != null && x.ZZ3_Name.Equals(attrRule.ZZ3_Name, StringComparison.Ordinal));
				if (attr == null)
				{
					ruleType = RuleType.INSERT;
					attr = new Stage.RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_Name = attrRule.ZZ3_Name, ZZ3_ZZ1_Tariff = parentPK };
				}
				attr.ZZ3_Value = attrRule.ZZ3_Value;
				result.Add(new RuleResult<Stage.RefCusTariffAttribute> { Data = attr, Rule = new TariffRuleWrapper(attrRule.ZZ3_PK, safe), RuleType = ruleType });
			}
			return result;
		}

		async Task<IEnumerable<IRuleResult<Stage.RefCusRate>>> GetRatesAsync(Guid parentPK, Stage.RefCusRate[] rates)
		{
			var result = new List<IRuleResult<Stage.RefCusRate>>();
			foreach (var rateRule in await safe.Get<Safe.RefCusRateRule>().Where(x => x.ZZ2_ZZ1_Tariff == ruleId).ExecuteAsync())
			{
				var ruleType = RuleType.UPDATE;
				var rate = rates.FirstOrDefault(x => x != null && (string.IsNullOrEmpty(x.ZZ2_SelectorFormula) || x.ZZ2_SelectorFormula.Equals(rateRule.ZZ2_SelectorFormula, StringComparison.Ordinal)));
				if (rate == null)
				{
					ruleType = RuleType.INSERT;
					rate = new Stage.RefCusRate
					{
						ZZ2_PK = Guid.NewGuid(),
						ZZ2_SelectorFormula = rateRule.ZZ2_SelectorFormula ?? string.Empty,
						ZZ2_ZZ1_Tariff = parentPK,
						ZZ2_StartDate = new DateTime(1900, 01, 01, 0, 0, 0),
						ZZ2_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
						ZZ2_RateFormulaDerivedFrom = rateRule.ZZ2_RateFormulaDerivedFrom,
						ZZ2_ZZZ_NKDataGrouping = rateRule.ZZ2_ZZZ_NKDataGrouping,
						ZZ2_RX_NKCurrencyOverride = ""
					};
				}
				rate.ZZ2_RateFormula = rateRule.ZZ2_RateFormula;
				result.Add(new RuleResult<Stage.RefCusRate> { Data = rate, Rule = new TariffRuleWrapper(rateRule.ZZ2_PK, safe), RuleType = ruleType });
			}
			return result;
		}
	}
	class RuleResult<T> : IRuleResult<T>
	{
		public T Data { get; set; }
		public ITariffRuleWrapper Rule { get; set; }
		public RuleType RuleType { get; set; }
	}
}
